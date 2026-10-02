import {normalizeEventManifest} from './event-manifest.mjs';
import {activeEvents,eventState} from './event-schedule.mjs';
import {createHash} from 'node:crypto';

export function createEventStore(db,clock=()=>Date.now()){
  db.exec('CREATE TABLE IF NOT EXISTS events(id TEXT PRIMARY KEY, version INTEGER NOT NULL, manifest TEXT NOT NULL, updated_at TEXT NOT NULL); CREATE TABLE IF NOT EXISTS event_versions(id TEXT NOT NULL, version INTEGER NOT NULL, manifest TEXT NOT NULL, updated_at TEXT NOT NULL, PRIMARY KEY(id,version)); CREATE TABLE IF NOT EXISTS event_claims(event_id TEXT NOT NULL, event_version INTEGER NOT NULL, player TEXT NOT NULL, claim_key TEXT NOT NULL, reward_receipt TEXT NOT NULL, created_at TEXT NOT NULL, PRIMARY KEY(event_id,event_version,player,claim_key)); CREATE TABLE IF NOT EXISTS event_progress(event_id TEXT NOT NULL, event_version INTEGER NOT NULL, player TEXT NOT NULL, metric TEXT NOT NULL, value INTEGER NOT NULL, updated_at TEXT NOT NULL, PRIMARY KEY(event_id,event_version,player,metric)); CREATE TABLE IF NOT EXISTS event_shop_stock(event_id TEXT NOT NULL,event_version INTEGER NOT NULL,offer_id TEXT NOT NULL,period TEXT NOT NULL,remaining INTEGER NOT NULL,PRIMARY KEY(event_id,event_version,offer_id,period)); CREATE TABLE IF NOT EXISTS event_shop_purchases(event_id TEXT NOT NULL,event_version INTEGER NOT NULL,player TEXT NOT NULL,offer_id TEXT NOT NULL,request_id TEXT NOT NULL,purchase_period TEXT NOT NULL,price INTEGER NOT NULL,reward_receipt TEXT NOT NULL,created_at TEXT NOT NULL,PRIMARY KEY(event_id,event_version,player,request_id)); CREATE INDEX IF NOT EXISTS event_shop_player_offer_idx ON event_shop_purchases(event_id,event_version,player,offer_id,purchase_period); CREATE INDEX IF NOT EXISTS event_claims_player_idx ON event_claims(player,event_id,event_version); CREATE INDEX IF NOT EXISTS event_progress_player_idx ON event_progress(player,event_id,event_version);');
  db.exec('INSERT OR IGNORE INTO event_versions SELECT id,version,manifest,updated_at FROM events;');
  let activeCache=null,activeCacheAt=0;
  const read=row=>row?JSON.parse(row.manifest):null;
  const list=()=>db.prepare('SELECT manifest FROM events ORDER BY json_extract(manifest,\'$.startAt\'),id').all().map(read);
  const history=()=>db.prepare('SELECT manifest FROM event_versions ORDER BY json_extract(manifest,\'$.startAt\'),id,version').all().map(read);
  const get=id=>read(db.prepare('SELECT manifest FROM events WHERE id=?').get(id));
  const definition=event=>{
    if(!event?.config||typeof event.config!=='object')return null;
    const metric=typeof event.config.progressMetric==='string'?event.config.progressMetric:typeof event.config.metric==='string'?event.config.metric:null;
    if(!metric||event.config.progressScope==='community')return null;
    const thresholds=Array.isArray(event.config.thresholds)?event.config.thresholds.filter(Number.isSafeInteger).filter(x=>x>0).sort((a,b)=>a-b):[];
    const cap=Number.isSafeInteger(event.config.progressCap)?event.config.progressCap:Number.isSafeInteger(event.config.cap)?event.config.cap:null;
    const modes=Array.isArray(event.config.eligibleModes)?event.config.eligibleModes.filter(x=>typeof x==='string'):[];
    return {metric,thresholds,cap,modes,scope:event.config.progressScope==='community'?'community':'player'};
  };
  const readProgress=(event,player)=>{
    const spec=definition(event);if(!spec)return null;
    const row=db.prepare('SELECT value,updated_at FROM event_progress WHERE event_id=? AND event_version=? AND player=? AND metric=?').get(event.id,event.version,player,spec.metric);
    const value=Number(row?.value??0),nextThreshold=spec.thresholds.find(x=>x>value)??0;
    return {metric:spec.metric,value,cap:spec.cap??0,nextThreshold,updatedAt:row?.updated_at??null};
  };
  const claimEligibility=(event,player,state)=>{
    if(state!=='active')return {eligible:false,reason:`Event is ${state}.`};
    const progress=readProgress(event,player);
    if(event.type==='milestone_track'&&progress){
      const thresholds=definition(event).thresholds;
      for(let i=0;i<thresholds.length;i++){
        const key=`milestone:${i+1}`;
        const claimed=db.prepare('SELECT 1 FROM event_claims WHERE event_id=? AND event_version=? AND player=? AND claim_key=?').get(event.id,event.version,player,key);
        if(!claimed){
          if(progress.value>=thresholds[i])return {eligible:true,reason:`Milestone ${i+1} is ready to claim.`};
          return {eligible:false,reason:`Reach ${thresholds[i]} ${progress.metric} to unlock the next milestone.`};
        }
      }
      return {eligible:false,reason:'All milestones are claimed.'};
    }
    return {eligible:true,reason:'Event is active.'};
  };
  const upsert=input=>{const manifest=normalizeEventManifest(input),existing=get(manifest.id);if(existing&&manifest.version<=existing.version)throw new Error('Event version must increase; active manifests are immutable.');const now=new Date(clock()).toISOString();db.exec('BEGIN IMMEDIATE');try{db.prepare('INSERT INTO event_versions(id,version,manifest,updated_at) VALUES(?,?,?,?)').run(manifest.id,manifest.version,JSON.stringify(manifest),now);db.prepare('INSERT INTO events(id,version,manifest,updated_at) VALUES(?,?,?,?) ON CONFLICT(id) DO UPDATE SET version=excluded.version,manifest=excluded.manifest,updated_at=excluded.updated_at').run(manifest.id,manifest.version,JSON.stringify(manifest),now);db.exec('COMMIT');activeCache=null;return manifest;}catch(error){db.exec('ROLLBACK');throw error;}};
  const active=()=>{const now=clock();if(activeCache&&Math.abs(now-activeCacheAt)<1000)return activeCache;const current=activeEvents(history(),now),byId=new Map();for(const event of current){const prior=byId.get(event.id);if(!prior||event.version>prior.version)byId.set(event.id,event);}activeCache=[...byId.values()].sort((a,b)=>a.endAt.localeCompare(b.endAt));activeCacheAt=now;return activeCache;};
  const status=(eventId,player,eventVersion=null)=>{
    if(typeof player!=='string'||player.length<1||player.length>128)throw new Error('Invalid player.');
    const event=eventVersion==null?get(eventId):history().find(candidate=>candidate.id===eventId&&candidate.version===eventVersion);if(!event)throw new Error('Unknown event.');
    const state=eventState(event,clock());
    const row=db.prepare('SELECT COUNT(*) AS count FROM event_claims WHERE event_id=? AND event_version=? AND player=?').get(event.id,event.version,player),gate=claimEligibility(event,player,state);
    return {eventId:event.id,eventVersion:event.version,state,eligible:gate.eligible,eligibilityReason:gate.reason,claimsCompleted:Number(row?.count??0),progress:readProgress(event,player)};
  };
  const recordProgress=(eventId,player,amount,metric=null,mode=null)=>{
    if(typeof player!=='string'||player.length<1||player.length>128)throw new Error('Invalid player.');
    if(!Number.isSafeInteger(amount)||amount<1||amount>100000)throw new Error('Invalid progress amount.');
    const event=get(eventId);if(!event)throw new Error('Unknown event.');if(eventState(event,clock())!=='active')throw new Error('Event is not active.');
    const spec=definition(event);if(!spec)throw new Error('Event does not track gameplay progress.');
    if(metric!==null&&metric!==spec.metric)throw new Error('Progress metric does not match the event.');
    if(spec.modes.length>0&&(typeof mode!=='string'||!spec.modes.includes(mode)))throw new Error('Mode is not eligible for this event.');
    const prior=readProgress(event,player),cap=spec.cap??Number.MAX_SAFE_INTEGER,value=Math.min(cap,(prior?.value??0)+amount),updatedAt=new Date(clock()).toISOString();
    db.prepare('INSERT INTO event_progress(event_id,event_version,player,metric,value,updated_at) VALUES(?,?,?,?,?,?) ON CONFLICT(event_id,event_version,player,metric) DO UPDATE SET value=excluded.value,updated_at=excluded.updated_at').run(event.id,event.version,player,spec.metric,value,updatedAt);
    return readProgress(event,player);
  };
  const recordActiveProgress=(player,mode,facts={})=>{
    const results=[];for(const event of active()){const spec=definition(event),amount=spec?facts[spec.metric]:null;if(Number.isSafeInteger(amount)&&amount>0){try{results.push(recordProgress(event.id,player,amount,spec.metric,mode));}catch(error){if(!/Mode is not eligible/.test(error.message))throw error;}}}return results;
  };
  const claim=(eventId,player,claimKey,rewardReceipt,onFirstClaim=null)=>{
    if(typeof player!=='string'||player.length<1||player.length>128)throw new Error('Invalid player.');
    if(typeof claimKey!=='string'||!/^[a-zA-Z0-9:_-]{1,120}$/.test(claimKey))throw new Error('Invalid claim key.');
    const event=get(eventId);if(!event)throw new Error('Unknown event.');if(eventState(event,clock())!=='active')throw new Error('Event is not active.');
    if(event.type==='milestone_track'&&/^milestone:\d+$/.test(claimKey)){const index=Number(claimKey.split(':')[1])-1,thresholds=definition(event)?.thresholds??[];if(index<0||index>=thresholds.length)throw new Error('Unknown milestone.');const progress=readProgress(event,player);if((progress?.value??0)<thresholds[index])throw new Error('Event milestone is not complete.');}
    const receipt=String(rewardReceipt??`${eventId}:${event.version}:${player}:${claimKey}`);db.exec('BEGIN IMMEDIATE');try{
      const prior=db.prepare('SELECT reward_receipt FROM event_claims WHERE event_id=? AND event_version=? AND player=? AND claim_key=?').get(event.id,event.version,player,claimKey);
      if(prior){db.exec('COMMIT');return {claimed:false,rewardReceipt:prior.reward_receipt,event};}
      const delivery=onFirstClaim?onFirstClaim(event,player,receipt):[];
      db.prepare('INSERT INTO event_claims VALUES(?,?,?,?,?,?)').run(event.id,event.version,player,claimKey,receipt,new Date(clock()).toISOString());db.exec('COMMIT');return {claimed:true,rewardReceipt:receipt,event,delivery};
    }catch(error){db.exec('ROLLBACK');throw error;}
  };
  const purchase=(eventId,eventVersion,player,offerId,requestId,onPurchase=null)=>{
    if(typeof player!=='string'||player.length<1||player.length>128)throw new Error('Invalid player.');
    if(!Number.isSafeInteger(eventVersion)||eventVersion<1)throw new Error('Invalid event version.');
    if(typeof offerId!=='string'||!/^[a-z0-9][a-z0-9_-]{0,63}$/.test(offerId))throw new Error('Invalid shop offer.');
    if(typeof requestId!=='string'||!/^[a-zA-Z0-9_-]{8,80}$/.test(requestId))throw new Error('Invalid purchase request id.');
    const current=get(eventId);if(!current)throw new Error('Unknown event.');
    const event=current.version===eventVersion?current:history().find(candidate=>candidate.id===eventId&&candidate.version===eventVersion);
    if(!event)throw new Error('Unknown event version.');
    if(!['event_shop','token_exchange','collaboration_pack'].includes(event.type))throw new Error('This event has no shop.');
    const offer=(event.config.shopOffers??[]).find(candidate=>candidate.id===offerId);if(!offer)throw new Error('Unknown shop offer.');
    const spec=definition(event);if(!spec||spec.metric!==event.config.currencyMetric)throw new Error('Shop currency progress is not configured.');
    const now=clock(),iso=new Date(now).toISOString(),day=iso.slice(0,10),restock=event.config.restock==='utc-day'?'utc-day':'event';
    const stockPeriod=restock==='utc-day'?day:'event',limitPeriod=event.config.purchaseLimitScope==='utc-day'?day:'event';
    const purchaseId=createHash('sha256').update(`${event.id}:${event.version}:${player}:${requestId}`).digest('hex');
    const rewardReceipt=`shop:${purchaseId}`;
    db.exec('BEGIN IMMEDIATE');try{
      const prior=db.prepare('SELECT * FROM event_shop_purchases WHERE event_id=? AND event_version=? AND player=? AND request_id=?').get(event.id,event.version,player,requestId);
      if(prior){db.exec('COMMIT');return {purchased:false,duplicate:true,purchaseId,rewardReceipt:prior.reward_receipt,offerId:prior.offer_id,price:prior.price};}
      if(current.version!==eventVersion)throw new Error('Shop event version is no longer current. Refresh the shop.');
      if(eventState(event,now)!=='active')throw new Error('Event shop is not active.');
      const progress=db.prepare('SELECT value FROM event_progress WHERE event_id=? AND event_version=? AND player=? AND metric=?').get(event.id,event.version,player,spec.metric),balance=Number(progress?.value??0);
      if(balance<offer.price)throw new Error('Not enough event currency.');
      const count=db.prepare('SELECT COUNT(*) AS count FROM event_shop_purchases WHERE event_id=? AND event_version=? AND player=? AND offer_id=? AND purchase_period=?').get(event.id,event.version,player,offer.id,limitPeriod);
      if(Number(count?.count??0)>=offer.playerLimit)throw new Error('Player purchase limit reached.');
      db.prepare('INSERT OR IGNORE INTO event_shop_stock(event_id,event_version,offer_id,period,remaining) VALUES(?,?,?,?,?)').run(event.id,event.version,offer.id,stockPeriod,offer.stock);
      const stock=db.prepare('SELECT remaining FROM event_shop_stock WHERE event_id=? AND event_version=? AND offer_id=? AND period=?').get(event.id,event.version,offer.id,stockPeriod);
      if(!stock||stock.remaining<1)throw new Error('This shop offer is sold out.');
      const delivery=onPurchase?onPurchase(event,player,rewardReceipt,offer):[];
      db.prepare('UPDATE event_progress SET value=value-?,updated_at=? WHERE event_id=? AND event_version=? AND player=? AND metric=?').run(offer.price,iso,event.id,event.version,player,spec.metric);
      db.prepare('UPDATE event_shop_stock SET remaining=remaining-1 WHERE event_id=? AND event_version=? AND offer_id=? AND period=? AND remaining>0').run(event.id,event.version,offer.id,stockPeriod);
      db.prepare('INSERT INTO event_shop_purchases(event_id,event_version,player,offer_id,request_id,purchase_period,price,reward_receipt,created_at) VALUES(?,?,?,?,?,?,?,?,?)').run(event.id,event.version,player,offer.id,requestId,limitPeriod,offer.price,rewardReceipt,iso);
      db.exec('COMMIT');return {purchased:true,duplicate:false,purchaseId,rewardReceipt,offerId:offer.id,price:offer.price,balance:balance-offer.price,stockRemaining:stock.remaining-1,delivery};
    }catch(error){db.exec('ROLLBACK');throw error;}
  };
  const shopStatus=(eventId,eventVersion,player)=>{
    if(typeof player!=='string'||player.length<1||player.length>128)throw new Error('Invalid player.');
    if(!Number.isSafeInteger(eventVersion)||eventVersion<1)throw new Error('Invalid event version.');
    const event=get(eventId);if(!event)throw new Error('Unknown event.');
    if(event.version!==eventVersion)throw new Error('Shop event version is no longer current. Refresh the shop.');
    if(!['event_shop','token_exchange','collaboration_pack'].includes(event.type))throw new Error('This event has no shop.');
    const spec=definition(event);if(!spec||spec.metric!==event.config.currencyMetric)throw new Error('Shop currency progress is not configured.');
    const now=clock(),iso=new Date(now).toISOString(),day=iso.slice(0,10),state=eventState(event,now);
    const stockPeriod=event.config.restock==='utc-day'?day:'event',limitPeriod=event.config.purchaseLimitScope==='utc-day'?day:'event';
    const progress=db.prepare('SELECT value FROM event_progress WHERE event_id=? AND event_version=? AND player=? AND metric=?').get(event.id,event.version,player,spec.metric),balance=Number(progress?.value??0);
    const offers=(event.config.shopOffers??[]).map(offer=>{
      const stockRow=db.prepare('SELECT remaining FROM event_shop_stock WHERE event_id=? AND event_version=? AND offer_id=? AND period=?').get(event.id,event.version,offer.id,stockPeriod);
      const stockRemaining=stockRow?Number(stockRow.remaining):offer.stock;
      const count=db.prepare('SELECT COUNT(*) AS count FROM event_shop_purchases WHERE event_id=? AND event_version=? AND player=? AND offer_id=? AND purchase_period=?').get(event.id,event.version,player,offer.id,limitPeriod);
      const purchasesRemaining=Math.max(0,offer.playerLimit-Number(count?.count??0));
      let reason='Ready to purchase.';
      if(state!=='active')reason=`Event is ${state}.`;
      else if(stockRemaining<1)reason='This offer is sold out.';
      else if(purchasesRemaining<1)reason='Purchase limit reached.';
      else if(balance<offer.price)reason='Not enough event currency.';
      return {id:offer.id,price:offer.price,stock:offer.stock,stockRemaining,playerLimit:offer.playerLimit,purchasesRemaining,reward:offer.reward,canPurchase:reason==='Ready to purchase.',reason};
    });
    return {eventId:event.id,eventVersion:event.version,state,serverTime:iso,currencyMetric:spec.metric,balance,restock:stockPeriod==='event'?'event':'utc-day',purchaseLimitScope:limitPeriod==='event'?'event':'utc-day',offers};
  };
  return {list,history,get,upsert,active,status,shopStatus,recordProgress,recordActiveProgress,claim,purchase,state:(event,now=clock())=>eventState(event,now)};
}
