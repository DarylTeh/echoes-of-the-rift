import {randomBytes,createHash,scrypt,timingSafeEqual} from 'node:crypto';
import {promisify} from 'node:util';
const derive=promisify(scrypt),token=()=>randomBytes(32).toString('hex'),hash=s=>createHash('sha256').update(String(s??'')).digest('hex');
const key=async(p,s)=>derive(p,s,32,{N:32768,r:8,p:3,maxmem:64*1024*1024});
const username=s=>{if(typeof s!=='string'||!/^[a-zA-Z0-9_]{3,24}$/.test(s))throw Error('Use 3-24 letters, numbers or underscores.');return s.toLowerCase();};
const password=s=>{if(typeof s!=='string'||s.length<5||s.length>128)throw Error('Use a password of 5-128 characters.');return s;};
export function createAccounts(store,now=()=>Date.now()){
 const db=store.db;
 db.exec(`CREATE TABLE IF NOT EXISTS accounts(username TEXT PRIMARY KEY,player TEXT UNIQUE NOT NULL,salt TEXT NOT NULL,password TEXT NOT NULL,recovery TEXT NOT NULL);
 CREATE TABLE IF NOT EXISTS sessions(token TEXT PRIMARY KEY,player TEXT NOT NULL,expires INTEGER NOT NULL);
 CREATE TABLE IF NOT EXISTS tickets(token TEXT PRIMARY KEY,player TEXT NOT NULL,expires INTEGER NOT NULL);
 CREATE INDEX IF NOT EXISTS sessions_expires_idx ON sessions(expires);
 CREATE INDEX IF NOT EXISTS sessions_player_idx ON sessions(player);
 CREATE INDEX IF NOT EXISTS tickets_expires_idx ON tickets(expires);
 CREATE INDEX IF NOT EXISTS tickets_player_idx ON tickets(player);`);
 const issue=id=>{db.prepare("DELETE FROM sessions WHERE expires<=?").run(now());db.prepare("DELETE FROM tickets WHERE expires<=?").run(now());const refreshToken=token(),ticket=token();db.prepare('INSERT INTO sessions VALUES(?,?,?)').run(hash(refreshToken),id,now()+30*86400000);db.prepare('INSERT INTO tickets VALUES(?,?,?)').run(hash(ticket),id,now()+120000);return {id,refreshToken,ticket,profile:store.get(id)};};
 function atomic(fn){db.exec('BEGIN IMMEDIATE');try{const value=fn();db.exec('COMMIT');return value;}catch(e){db.exec('ROLLBACK');throw e;}}
 return {
  async register(b){const name=username(b.username),salt=token(),encoded=(await key(password(b.password),salt)).toString('hex'),recovery=token();
   return atomic(()=>{if(db.prepare('SELECT 1 FROM accounts WHERE username=?').get(name))throw Error('Username is already registered.');
    let id=randomBytes(16).toString('hex');
    if(b.id&&db.prepare('SELECT 1 FROM players WHERE id=?').get(b.id)){store.login(b.id,b.secret);id=b.id;}
    else store.login(id,token());
    if(db.prepare('SELECT 1 FROM accounts WHERE player=?').get(id))throw Error('This profile already has an account. Sign in instead.');
    db.prepare('INSERT INTO accounts VALUES(?,?,?,?,?)').run(name,id,salt,encoded,hash(recovery));return {...issue(id),created:true,recovery};});
  },
  async login(b){const name=username(b.username),p=password(b.password),row=db.prepare('SELECT * FROM accounts WHERE username=?').get(name);const result=await key(p,row?.salt??'invalid-account-salt');
   if(!row||!timingSafeEqual(result,Buffer.from(row.password,'hex')))throw Error('Username or password is incorrect.');return atomic(()=>{if(db.prepare('SELECT password FROM accounts WHERE username=?').get(name)?.password!==row.password)throw Error('Username or password is incorrect.');return issue(row.player);});},
  refresh(b){return atomic(()=>{const row=db.prepare('SELECT * FROM sessions WHERE token=?').get(hash(b.refreshToken));if(!row||row.expires<=now())throw Error('Please sign in again.');db.prepare('DELETE FROM sessions WHERE token=?').run(hash(b.refreshToken));return issue(row.player);});},
  logout(b){const row=db.prepare('SELECT player FROM sessions WHERE token=?').get(hash(b.refreshToken));if(row)atomic(()=>{db.prepare('DELETE FROM sessions WHERE player=?').run(row.player);db.prepare('DELETE FROM tickets WHERE player=?').run(row.player);});return {ok:true};},
  consume(id,ticket){return atomic(()=>{const row=db.prepare('SELECT * FROM tickets WHERE token=? AND player=?').get(hash(ticket),id);if(!row||row.expires<=now())throw Error('Join ticket expired. Sign in again.');db.prepare('DELETE FROM tickets WHERE token=?').run(hash(ticket));return store.get(id);});},
  async recover(b){const name=username(b.username),p=password(b.password),salt=token(),encoded=(await key(p,salt)).toString('hex');return atomic(()=>{const row=db.prepare('SELECT * FROM accounts WHERE username=?').get(name);if(!row||hash(b.recovery)!==row.recovery)throw Error('Recovery details are incorrect.');const recovery=token();db.prepare('UPDATE accounts SET salt=?,password=?,recovery=? WHERE username=?').run(salt,encoded,hash(recovery),name);db.prepare('DELETE FROM sessions WHERE player=?').run(row.player);db.prepare('DELETE FROM tickets WHERE player=?').run(row.player);return {...issue(row.player),recovery};});}
 };
}

// Public account routes are loopback-only until HTTPS deployment is configured.
export function accountHandler(accounts,ready=()=>false){
 const attempts=new Map();let working=0;
 return async(req,res)=>{
  const reply=(status,body)=>{res.writeHead(status,{'Content-Type':'application/json','Cache-Control':'no-store'});res.end(JSON.stringify(body));};
  if(req.method==='GET'&&req.url==='/health'){try{const available=ready();return reply(available?200:503,{ready:available});}catch{return reply(503,{ready:false});}}
  if(req.method!=='POST')return reply(405,{error:'Use POST.'});
  const route=req.url.slice(1);if(!['register','login','refresh','recover','logout'].includes(route))return reply(404,{error:'Unknown account action.'});
  const address=req.socket.remoteAddress,stamp=Date.now();let limit=attempts.get(address);
  if(!limit||stamp-limit.start>=60000){limit={start:stamp,count:0};attempts.set(address,limit);}
  if(++limit.count>30||working>=4)return reply(429,{error:'Too many attempts. Wait a minute and retry.'});
  working++;
  try{let body='';for await(const chunk of req){body+=chunk;if(Buffer.byteLength(body)>8192)throw Error('Request too large.');}const value=JSON.parse(body);reply(200,await accounts[route](value));}
  catch(e){reply(400,{error:e.message});}finally{working--;}
 };
}
