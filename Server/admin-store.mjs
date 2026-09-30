import {createHash} from 'node:crypto';
import {normalizeProgression} from './progression-rules.mjs';
export const MAX_VALUE=2_000_000_000;
export class ApiError extends Error {constructor(status,message){super(message);this.status=status;}}
const fail=(status,message)=>{throw new ApiError(status,message);};
const integer=(v,min=0,max=MAX_VALUE)=>{if(!Number.isSafeInteger(v)||v<min||v>max)fail(400,`Expected an integer from ${min} to ${max}.`);return v;};
export function createAdminStore(store,catalog){
 const db=store.db;
 db.exec(`PRAGMA busy_timeout=5000; CREATE TABLE IF NOT EXISTS admin_audit(request_id TEXT PRIMARY KEY,player TEXT NOT NULL,action TEXT NOT NULL,reason TEXT NOT NULL,created_at TEXT NOT NULL,fingerprint TEXT NOT NULL,before_json TEXT NOT NULL,after_json TEXT NOT NULL);`);
 const profile=id=>{if(!/^[a-f0-9]{32}$/.test(id))fail(400,'Invalid player ID.');const row=db.prepare('SELECT profile FROM players WHERE id=?').get(id);if(!row)fail(404,'Player not found.');return normalizeProgression({Gems:0,Level:1,AdminRevision:0,...JSON.parse(row.profile)}).profile;};
 const page=(query={})=>({limit:integer(Number(query.limit??25),1,100),offset:integer(Number(query.offset??0),0,10000000)});
 const players=query=>{const {limit,offset}=page(query);const q=String(query.q??'').slice(0,100);const rows=db.prepare(`SELECT p.id,a.username,p.profile FROM players p LEFT JOIN accounts a ON a.player=p.id WHERE instr(p.id,?)>0 OR instr(lower(COALESCE(a.username,'')),lower(?))>0 ORDER BY p.id LIMIT ? OFFSET ?`).all(q,q,limit,offset);return {limit,offset,players:rows.map(r=>{const p=JSON.parse(r.profile);return {id:r.id,username:r.username,gold:p.Coins,gems:p.Gems??0,level:p.Level??1,adminRevision:p.AdminRevision??0};})};};
 const audit=query=>{const {limit,offset}=page(query);return {limit,offset,entries:db.prepare('SELECT request_id,player,action,reason,created_at,before_json,after_json FROM admin_audit WHERE (? = ? OR player=?) ORDER BY rowid DESC LIMIT ? OFFSET ?').all(query.player??'','',query.player??'',limit,offset).map(({before_json,after_json,...r})=>({...r,before:JSON.parse(before_json),after:JSON.parse(after_json)}))};};
 function mutate(id,action,b){
  if(!b||typeof b!=='object'||Array.isArray(b))fail(400,'JSON object required.');
  if(typeof b.requestId!=='string'||! /^[a-zA-Z0-9_-]{8,80}$/.test(b.requestId))fail(400,'Supply a unique requestId (8-80 letters, digits, hyphens or underscores).');
  if(typeof b.reason!=='string'||b.reason.trim().length<3||b.reason.length>300)fail(400,'A reason of 3-300 characters is required.');
  integer(b.expectedRevision);const allowed=new Set(['requestId','reason','expectedRevision',...(['giveItem','setItem','deleteItem'].includes(action)?['itemId','tier','quantity','unequip']:['amount'])]);
  if(Object.keys(b).some(k=>!allowed.has(k)))fail(400,'Unknown request field.');
  const fingerprint=createHash('sha256').update(JSON.stringify([id,action,Object.fromEntries(Object.keys(b).sort().map(k=>[k,b[k]]))])).digest('hex');
  db.exec('BEGIN IMMEDIATE');
  try{
   const prior=db.prepare('SELECT fingerprint,after_json FROM admin_audit WHERE request_id=?').get(b.requestId);
   if(prior){if(prior.fingerprint!==fingerprint)fail(409,'requestId already used for a different operation.');db.exec('COMMIT');return {replayed:true,profile:JSON.parse(prior.after_json)};}
   const p=profile(id),before=JSON.stringify(p);if(p.AdminRevision!==b.expectedRevision)fail(409,'Profile changed. Read the player again and use the returned AdminRevision.');
   const fields={giveGold:'Coins',setGold:'Coins',giveGems:'Gems',setGems:'Gems',setLevel:'Level'};
   if(fields[action]){const field=fields[action];integer(b.amount,action==='setLevel'||action.startsWith('give')?1:0,action==='setLevel'?1000000:MAX_VALUE);p[field]=integer(action.startsWith('give')?p[field]+b.amount:b.amount,action==='setLevel'?1:0);}
   else if(['giveItem','setItem','deleteItem'].includes(action)){
    const item=catalog.find(x=>x.id===b.itemId);if(!item)fail(400,'Unknown catalogue item.');integer(b.tier,1,5);
    if(b.unequip!==undefined&&typeof b.unequip!=='boolean')fail(400,'unequip must be a boolean.');
    const stack=p.Items.find(x=>x.ItemId===b.itemId&&x.Tier===b.tier);
    if(action==='deleteItem'&&!stack)fail(404,'Item stack not found.');
    const quantity=action==='deleteItem'?0:integer(b.quantity,action==='giveItem'?1:0,1000000);
    const count=integer(action==='giveItem'?(stack?.Count??0)+quantity:quantity,0,1000000);
    if(count===0){
     const slots=(p.EquippedIds??[]).map((v,i)=>v===b.itemId&&p.EquippedTiers[i]===b.tier?i:-1).filter(i=>i>=0);
     const losesBook=item.kind===1&&!p.Items.some(x=>x.ItemId===b.itemId&&x.Tier!==b.tier&&x.Count>0)&&!p.Items.some(x=>x.ItemId!==b.itemId&&x.Count>0&&catalog.some(c=>c.id===x.ItemId&&c.skill===item.skill));
     const skills=losesBook?(p.SkillIds??[]).map((v,i)=>v===item.skill?i:-1).filter(i=>i>=0):[];
     if((slots.length||skills.length)&&!b.unequip)fail(409,'Item is equipped. Explicitly set unequip=true to remove it.');
     for(const i of slots){p.EquippedIds[i]='';p.EquippedTiers[i]=1;}for(const i of skills)p.SkillIds[i]='';
     p.Items=p.Items.filter(x=>x!==stack);
    }else if(stack)stack.Count=count;else p.Items.push({ItemId:b.itemId,Tier:b.tier,EnhancementLevel:b.tier,Count:count});
   }else fail(404,'Unknown admin operation.');
   p.AdminRevision=integer(p.AdminRevision+1);const after=JSON.stringify(p);
   db.prepare('UPDATE players SET profile=? WHERE id=?').run(after,id);
   db.prepare('INSERT INTO admin_audit VALUES(?,?,?,?,?,?,?,?)').run(b.requestId,id,action,b.reason.trim(),new Date().toISOString(),fingerprint,before,after);
   db.exec('COMMIT');return {replayed:false,profile:p};
  }catch(e){db.exec('ROLLBACK');throw e;}
 }
 const events=()=>({events:store.events.list().map(event=>({...event,state:store.events.state(event)}))});
 const publishEvent=input=>{try{return store.events.upsert(input);}catch(error){fail(400,error.message);}};
 const disableEvent=id=>{const event=store.events.get(id);if(!event)fail(404,'Event not found.');return publishEvent({...event,version:event.version+1,disabled:true});};
 return {profile,players,audit,mutate,events,publishEvent,disableEvent,accounts(query){const {limit,offset}=page(query),q=String(query.q??'').slice(0,100);return {limit,offset,accounts:db.prepare('SELECT username,player FROM accounts WHERE instr(username,lower(?))>0 OR instr(player,?)>0 ORDER BY username LIMIT ? OFFSET ?').all(q,q,limit,offset)};}};
}
