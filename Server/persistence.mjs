import http from 'node:http';
import {createAccounts,accountHandler} from './accounts.mjs';
import { DatabaseSync } from 'node:sqlite';
import { createHash, timingSafeEqual } from 'node:crypto';
import { readFileSync } from 'node:fs';
import { fileURLToPath } from 'node:url';
import { normalizeProgression } from './progression-rules.mjs';
import { createEventStore } from './events/event-store.mjs';
const hash=s=>createHash('sha256').update(s).digest('hex');
export function createStore(path,catalog) {
 const db=new DatabaseSync(path);db.exec('PRAGMA journal_mode=WAL; PRAGMA busy_timeout=5000; CREATE TABLE IF NOT EXISTS players(id TEXT PRIMARY KEY, secret TEXT NOT NULL, profile TEXT NOT NULL); CREATE TABLE IF NOT EXISTS receipts(player TEXT, receipt TEXT, PRIMARY KEY(player,receipt)); CREATE TABLE IF NOT EXISTS inbox(id TEXT PRIMARY KEY, player TEXT NOT NULL, event_id TEXT NOT NULL, event_version INTEGER NOT NULL, claim_key TEXT NOT NULL, reward_json TEXT NOT NULL, status TEXT NOT NULL, created_at TEXT NOT NULL, claimed_at TEXT); CREATE INDEX IF NOT EXISTS inbox_player_status_created_idx ON inbox(player,status,created_at DESC);');
 const events=createEventStore(db);
 const get=id=>{const row=db.prepare('SELECT profile FROM players WHERE id=?').get(id);if(!row)throw Error('Unknown player');const result=normalizeProgression({Gems:0,Level:1,AdminRevision:0,...JSON.parse(row.profile)});if(result.changed)db.prepare('UPDATE players SET profile=? WHERE id=?').run(JSON.stringify(result.profile),id);return result.profile;};
 const save=(id,p)=>{normalizeProgression(p);p.AdminRevision=(p.AdminRevision??0)+1;return db.prepare('UPDATE players SET profile=? WHERE id=?').run(JSON.stringify(p),id);};
 const add=(p,id,tier)=>{let s=p.Items.find(x=>x.ItemId===id&&x.Tier===tier&&x.EnhancementLevel===tier);if(s)s.Count++;else p.Items.push({ItemId:id,Tier:tier,EnhancementLevel:tier,Count:1});};
 const maxReward=2_000_000_000;
 const normalizeReward=raw=>{
  if(!raw||typeof raw!=='object'||Array.isArray(raw))throw Error('Invalid event reward.');
  if(raw.currency!==undefined){if(!['coins','gems'].includes(raw.currency)||!Number.isSafeInteger(raw.amount)||raw.amount<1||raw.amount>maxReward)throw Error('Invalid currency reward.');return {currency:raw.currency,amount:raw.amount};}
  if(typeof raw.itemId!=='string'||!catalog.some(x=>x.id===raw.itemId)||!Number.isSafeInteger(raw.tier)||raw.tier<1||raw.tier>5||!Number.isSafeInteger(raw.count)||raw.count<1||raw.count>1_000_000)throw Error('Invalid item reward.');
  return {itemId:raw.itemId,tier:raw.tier,count:raw.count};
 };
 const publicReward=row=>({id:row.id,eventId:row.event_id,eventVersion:row.event_version,claimKey:row.claim_key,reward:JSON.parse(row.reward_json),status:row.status,createdAt:row.created_at,claimedAt:row.claimed_at});
 const enqueueRewards=(event,player,receipt,rewards=event.rewards??[])=>{
  const rows=[];for(let i=0;i<rewards.length;i++){
   const reward=normalizeReward(rewards[i]),id=hash(`${receipt}:${i}`).slice(0,64),createdAt=new Date().toISOString();
   db.prepare('INSERT OR IGNORE INTO inbox(id,player,event_id,event_version,claim_key,reward_json,status,created_at,claimed_at) VALUES(?,?,?,?,?,?,?, ?,NULL)').run(id,player,event.id,event.version,`${event.id}:${event.version}:${i}`,JSON.stringify(reward),'pending',createdAt);const row=db.prepare('SELECT * FROM inbox WHERE id=?').get(id);if(row)rows.push(publicReward(row));
 }return rows;
 };
 const claimEvent=(eventId,player,claimKey)=>{get(player);return events.claim(eventId,player,claimKey,undefined,(event,_owner,receipt)=>enqueueRewards(event,player,receipt));};
 const purchaseEventShop=(eventId,eventVersion,player,offerId,requestId)=>{get(player);return events.purchase(eventId,eventVersion,player,offerId,requestId,(event,owner,receipt,offer)=>enqueueRewards(event,owner,receipt,[offer.reward]));};
 const eventStatus=(eventId,player,eventVersion)=>{get(player);return events.status(eventId,player,eventVersion);};
 const inbox=(player)=>{get(player);return {entries:db.prepare('SELECT * FROM inbox WHERE player=? ORDER BY CASE status WHEN \'pending\' THEN 0 ELSE 1 END,created_at DESC').all(player).map(publicReward)};};
 const claimInbox=(player,inboxId)=>{get(player);if(typeof inboxId!=='string'||inboxId.length<1||inboxId.length>128)throw Error('Invalid inbox entry.');db.exec('BEGIN IMMEDIATE');try{
  const row=db.prepare('SELECT * FROM inbox WHERE id=? AND player=?').get(inboxId,player);if(!row)throw Error('Inbox entry not found.');
  if(row.status==='claimed'){db.exec('COMMIT');return {claimed:false,entry:publicReward(row),profile:get(player)};}
  const reward=normalizeReward(JSON.parse(row.reward_json)),p=get(player);
  if(reward.currency){const field=reward.currency==='coins'?'Coins':'Gems';p[field]=Math.min(maxReward,(p[field]??0)+reward.amount);}else{for(let i=0;i<reward.count;i++)add(p,reward.itemId,reward.tier);}
  save(player,p);const claimedAt=new Date().toISOString();db.prepare('UPDATE inbox SET status=\'claimed\',claimed_at=? WHERE id=? AND player=?').run(claimedAt,inboxId,player);db.exec('COMMIT');return {claimed:true,entry:{...publicReward({...row,status:'claimed',claimed_at:claimedAt}),reward},profile:p};
 }catch(error){db.exec('ROLLBACK');throw error;}};
 return {db,get,events,claimEvent,purchaseEventShop,eventStatus,inbox,claimInbox, leaders(){return {entries:db.prepare("SELECT substr(id,1,6) AS tag,profile FROM players ORDER BY json_extract(profile,'$.CampaignStagesCompleted') DESC,json_extract(profile,'$.Coins') DESC LIMIT 10").all()};}, login(id,secret){
  if(!/^[a-f0-9]{32}$/.test(id)||!/^[a-f0-9]{64}$/.test(secret))throw Error('Invalid identity');
  const row=db.prepare('SELECT secret FROM players WHERE id=?').get(id);
  if(row&&!timingSafeEqual(Buffer.from(row.secret),Buffer.from(hash(secret))))throw Error('Invalid credentials');
  if(!row){const p=normalizeProgression({Version:1,Coins:30,Gems:0,Level:1,AdminRevision:0,CampaignStagesCompleted:0,Appearance:{},Items:[0,2,4].map(i=>({ItemId:'gear-'+i,Tier:1,EnhancementLevel:1,Count:1})),EquippedIds:['gear-0','gear-2','gear-4'],EquippedTiers:[1,1,1]}).profile;db.prepare('INSERT INTO players VALUES(?,?,?)').run(id,hash(secret),JSON.stringify(p));}
  return get(id);
 }, transaction(id,action,itemId,tier,receipt='',stage=-1){
  db.exec('BEGIN IMMEDIATE');try{
   const p=get(id),item=catalog.find(x=>x.id===itemId),stack=p.Items.find(x=>x.ItemId===itemId&&x.Tier===tier);
   if(action==='reward'){
    if(!/^[a-f0-9-]{8,80}$/.test(receipt)||!Number.isInteger(stage)||stage<0||stage>3)throw Error('Invalid room receipt');
    if(!db.prepare('SELECT 1 FROM receipts WHERE player=? AND receipt=?').get(id,receipt)){
     add(p,'gear-'+stage,1);p.Coins+=stage===3?35:15;p.CampaignStagesCompleted=Math.max(p.CampaignStagesCompleted,stage+1);db.prepare('INSERT INTO receipts VALUES(?,?)').run(id,receipt);events.recordActiveProgress(id,'campaign',{'verified-event-clears':1,'event-tokens':10});
    }
   }else{
    if(!item||!Number.isInteger(tier)||tier<1||tier>5)throw Error('Unknown item/tier');
    if(action==='purchase'){const cost=15*tier*tier;if(tier!==item.tier||p.Coins<cost)throw Error('Insufficient gold');p.Coins-=cost;add(p,itemId,tier);}
    else if(action==='skill'){if(item.kind!==1||!p.Items.some(x=>x.ItemId===itemId&&x.Count>0))throw Error('Book not owned');p.SkillIds??=Array(6).fill('');p.SkillIds[0]=item.skill;}
    else if(action==='equip'){if(item.kind!==0||!stack||stack.Count<1)throw Error('Item not owned or not gear');p.EquippedIds[item.slot]=itemId;p.EquippedTiers[item.slot]=tier;}
    else if(action==='fuse'){const cost=10*tier;if(tier>=5||!stack||stack.Count<2||p.Coins<cost)throw Error('Fusion requirements not met');stack.Count-=2;p.Coins-=cost;add(p,itemId,tier+1);const slot=p.EquippedIds.indexOf(itemId);if(slot>=0&&p.EquippedTiers[slot]===tier)p.EquippedTiers[slot]++;}
    else throw Error('Unknown transaction');
   }
   save(id,p);db.exec('COMMIT');return p;
  }catch(e){db.exec('ROLLBACK');throw e;}
 }, appearance(id,a){
  const p=get(id);if(!a||!Number.isInteger(a.Race)||a.Race<0||a.Race>8)throw Error('Invalid race');
  const color=c=>{if(!c||!['r','g','b'].every(k=>Number.isFinite(c[k])&&c[k]>=0&&c[k]<=1))throw Error('Invalid color');return {...c,a:1};};
  p.Appearance={Race:a.Race,CustomColors:!!a.CustomColors,HairStyle:Math.max(0,Math.min(2,a.HairStyle|0)),SkinIndex:Math.max(0,Math.min(3,a.SkinIndex|0)),HairColor:Math.max(0,Math.min(3,a.HairColor|0)),SkinRGB:color(a.SkinRGB),HairRGB:color(a.HairRGB),EyeRGB:color(a.EyeRGB),ClassId:'wayfarer',PassiveSkillId:'steadfast'};save(id,p);return p;
 }};
}
export function startServer({port=8081,host='127.0.0.1',key=process.env.COOKIE_SERVER_KEY,path='progress.sqlite',catalog=[]}={}){
 if(!key||key.length<32)throw Error('Set COOKIE_SERVER_KEY to a private random value of at least 32 characters.');
 const store=createStore(path,catalog);const accounts=createAccounts(store);let heartbeat=0;const ready=()=>{store.db.prepare("SELECT 1").get();return Date.now()-heartbeat<10000;};const server=http.createServer(async(req,res)=>{
  const reply=(code,body)=>{res.writeHead(code,{'Content-Type':'application/json'});res.end(JSON.stringify(body));};
  if(req.method==='GET'&&req.url==='/health')return reply(200,{ok:true});
  if(req.method!=='POST'||req.headers.authorization!==`Bearer ${key}`)return reply(403,{error:'Forbidden'});
  try{
   // The event feed is a read-only server-timer request. Handle it before
   // parsing a body so clients can send an empty POST safely.
   if(req.url==='/events')return reply(200,{serverTime:new Date().toISOString(),events:store.events.active()});
   let body='';for await(const chunk of req){body+=chunk;if(body.length>16384)throw Error('Request too large');}const b=JSON.parse(body);
   if(req.url==='/heartbeat'){heartbeat=Date.now();return reply(200,{ok:true});}
   if(req.url==='/leaderboard')return reply(200,store.leaders());
   if(req.url==='/inbox')return reply(200,store.inbox(b.id));
   if(req.url==='/inbox-claim')return reply(200,store.claimInbox(b.id,b.inboxId));
   if(req.url==='/event-status')return reply(200,store.eventStatus(b.eventId,b.id,Number.isInteger(b.eventVersion)&&b.eventVersion>0?b.eventVersion:null));
   if(req.url==='/event-claim')return reply(200,store.claimEvent(b.eventId,b.id,b.claimKey));
   if(req.url==='/event-shop-purchase')return reply(200,store.purchaseEventShop(b.eventId,b.eventVersion,b.id,b.offerId,b.requestId));
   let profile;if(req.url==='/ticket')profile=accounts.consume(b.id,b.secret);
   else if(req.url==='/login')profile=store.login(b.id,b.secret);
   else if(req.url==='/transaction')profile=store.transaction(b.id,b.action,b.itemId,b.tier,b.receipt,b.stage);
   else if(req.url==='/appearance')profile=store.appearance(b.id,b.appearance);
   else throw Error('Unknown endpoint');reply(200,profile);
  }catch(e){reply(400,{error:e.message});}
 });server.listen(port,host);return {server,store,accounts,ready};
}
if(process.argv[1]===fileURLToPath(import.meta.url)){
 const data=JSON.parse(readFileSync(new URL('./catalog.json',import.meta.url)));const catalog=data.items??data;
 const service=startServer({port:Number(process.env.COOKIE_DB_PORT||8081),path:process.env.COOKIE_DB_PATH||'progress.sqlite',catalog});http.createServer(accountHandler(service.accounts,service.ready)).listen(Number(process.env.COOKIE_ACCOUNT_PORT||8082),'127.0.0.1');console.log('Private persistence service ready.');
}
