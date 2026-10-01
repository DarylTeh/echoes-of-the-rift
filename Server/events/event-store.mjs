import {normalizeEventManifest} from './event-manifest.mjs';
import {activeEvents,eventState} from './event-schedule.mjs';

export function createEventStore(db,clock=()=>Date.now()){
  db.exec('CREATE TABLE IF NOT EXISTS events(id TEXT PRIMARY KEY, version INTEGER NOT NULL, manifest TEXT NOT NULL, updated_at TEXT NOT NULL); CREATE TABLE IF NOT EXISTS event_versions(id TEXT NOT NULL, version INTEGER NOT NULL, manifest TEXT NOT NULL, updated_at TEXT NOT NULL, PRIMARY KEY(id,version)); CREATE TABLE IF NOT EXISTS event_claims(event_id TEXT NOT NULL, event_version INTEGER NOT NULL, player TEXT NOT NULL, claim_key TEXT NOT NULL, reward_receipt TEXT NOT NULL, created_at TEXT NOT NULL, PRIMARY KEY(event_id,event_version,player,claim_key)); CREATE TABLE IF NOT EXISTS event_progress(event_id TEXT NOT NULL, event_version INTEGER NOT NULL, player TEXT NOT NULL, metric TEXT NOT NULL, value INTEGER NOT NULL, updated_at TEXT NOT NULL, PRIMARY KEY(event_id,event_version,player,metric));');
  db.exec('INSERT OR IGNORE INTO event_versions SELECT id,version,manifest,updated_at FROM events;');
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
  const upsert=input=>{const manifest=normalizeEventManifest(input),existing=get(manifest.id);if(existing&&manifest.version<=existing.version)throw new Error('Event version must increase; active manifests are immutable.');const now=new Date(clock()).toISOString();db.exec('BEGIN IMMEDIATE');try{db.prepare('INSERT INTO event_versions(id,version,manifest,updated_at) VALUES(?,?,?,?)').run(manifest.id,manifest.version,JSON.stringify(manifest),now);db.prepare('INSERT INTO events(id,version,manifest,updated_at) VALUES(?,?,?,?) ON CONFLICT(id) DO UPDATE SET version=excluded.version,manifest=excluded.manifest,updated_at=excluded.updated_at').run(manifest.id,manifest.version,JSON.stringify(manifest),now);db.exec('COMMIT');return manifest;}catch(error){db.exec('ROLLBACK');throw error;}};
  const active=()=>{const current=activeEvents(history(),clock()),byId=new Map();for(const event of current){const prior=byId.get(event.id);if(!prior||event.version>prior.version)byId.set(event.id,event);}return [...byId.values()].sort((a,b)=>a.endAt.localeCompare(b.endAt));};
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
  return {list,history,get,upsert,active,status,recordProgress,recordActiveProgress,claim,state:(event,now=clock())=>eventState(event,now)};
}
