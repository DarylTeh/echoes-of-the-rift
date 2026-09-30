import {normalizeEventManifest} from './event-manifest.mjs';
import {activeEvents,eventState} from './event-schedule.mjs';

export function createEventStore(db,clock=()=>Date.now()){
  db.exec('CREATE TABLE IF NOT EXISTS events(id TEXT PRIMARY KEY, version INTEGER NOT NULL, manifest TEXT NOT NULL, updated_at TEXT NOT NULL); CREATE TABLE IF NOT EXISTS event_versions(id TEXT NOT NULL, version INTEGER NOT NULL, manifest TEXT NOT NULL, updated_at TEXT NOT NULL, PRIMARY KEY(id,version)); CREATE TABLE IF NOT EXISTS event_claims(event_id TEXT NOT NULL, event_version INTEGER NOT NULL, player TEXT NOT NULL, claim_key TEXT NOT NULL, reward_receipt TEXT NOT NULL, created_at TEXT NOT NULL, PRIMARY KEY(event_id,event_version,player,claim_key));');
  db.exec('INSERT OR IGNORE INTO event_versions SELECT id,version,manifest,updated_at FROM events;');
  const read=row=>row?JSON.parse(row.manifest):null;
  const list=()=>db.prepare('SELECT manifest FROM events ORDER BY json_extract(manifest,\'$.startAt\'),id').all().map(read);
  const history=()=>db.prepare('SELECT manifest FROM event_versions ORDER BY json_extract(manifest,\'$.startAt\'),id,version').all().map(read);
  const get=id=>read(db.prepare('SELECT manifest FROM events WHERE id=?').get(id));
  const upsert=input=>{const manifest=normalizeEventManifest(input),existing=get(manifest.id);if(existing&&manifest.version<=existing.version)throw new Error('Event version must increase; active manifests are immutable.');const now=new Date(clock()).toISOString();db.exec('BEGIN IMMEDIATE');try{db.prepare('INSERT INTO event_versions(id,version,manifest,updated_at) VALUES(?,?,?,?)').run(manifest.id,manifest.version,JSON.stringify(manifest),now);db.prepare('INSERT INTO events(id,version,manifest,updated_at) VALUES(?,?,?,?) ON CONFLICT(id) DO UPDATE SET version=excluded.version,manifest=excluded.manifest,updated_at=excluded.updated_at').run(manifest.id,manifest.version,JSON.stringify(manifest),now);db.exec('COMMIT');return manifest;}catch(error){db.exec('ROLLBACK');throw error;}};
  const active=()=>{const current=activeEvents(history(),clock()),byId=new Map();for(const event of current){const prior=byId.get(event.id);if(!prior||event.version>prior.version)byId.set(event.id,event);}return [...byId.values()].sort((a,b)=>a.endAt.localeCompare(b.endAt));};
  const status=(eventId,player,eventVersion=null)=>{
    if(typeof player!=='string'||player.length<1||player.length>128)throw new Error('Invalid player.');
    const event=eventVersion==null?get(eventId):history().find(candidate=>candidate.id===eventId&&candidate.version===eventVersion);if(!event)throw new Error('Unknown event.');
    const state=eventState(event,clock());
    const row=db.prepare('SELECT COUNT(*) AS count FROM event_claims WHERE event_id=? AND event_version=? AND player=?').get(event.id,event.version,player);
    return {eventId:event.id,eventVersion:event.version,state,eligible:state==='active',eligibilityReason:state==='active'?'Event is active.':`Event is ${state}.`,claimsCompleted:Number(row?.count??0)};
  };
  const claim=(eventId,player,claimKey,rewardReceipt,onFirstClaim=null)=>{
    if(typeof player!=='string'||player.length<1||player.length>128)throw new Error('Invalid player.');
    if(typeof claimKey!=='string'||!/^[a-zA-Z0-9:_-]{1,120}$/.test(claimKey))throw new Error('Invalid claim key.');
    const event=get(eventId);if(!event)throw new Error('Unknown event.');if(eventState(event,clock())!=='active')throw new Error('Event is not active.');
    const receipt=String(rewardReceipt??`${eventId}:${event.version}:${player}:${claimKey}`);db.exec('BEGIN IMMEDIATE');try{
      const prior=db.prepare('SELECT reward_receipt FROM event_claims WHERE event_id=? AND event_version=? AND player=? AND claim_key=?').get(event.id,event.version,player,claimKey);
      if(prior){db.exec('COMMIT');return {claimed:false,rewardReceipt:prior.reward_receipt,event};}
      const delivery=onFirstClaim?onFirstClaim(event,player,receipt):[];
      db.prepare('INSERT INTO event_claims VALUES(?,?,?,?,?,?)').run(event.id,event.version,player,claimKey,receipt,new Date(clock()).toISOString());db.exec('COMMIT');return {claimed:true,rewardReceipt:receipt,event,delivery};
    }catch(error){db.exec('ROLLBACK');throw error;}
  };
  return {list,history,get,upsert,active,status,claim,state:(event,now=clock())=>eventState(event,now)};
}
