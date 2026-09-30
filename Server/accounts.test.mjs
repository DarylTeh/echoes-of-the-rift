import assert from 'node:assert/strict';
import {createStore} from './persistence.mjs';
import {createAccounts,accountHandler} from './accounts.mjs';
import http from 'node:http';
const store=createStore(':memory:',[]);let now=1000000;const accounts=createAccounts(store,()=>now);
const password='a long private passphrase';
const original=await accounts.register({username:'Wayfarer',password});
assert.equal(original.profile.Coins,30);assert.equal(accounts.consume(original.id,original.ticket).Coins,30);
assert.throws(()=>accounts.consume(original.id,original.ticket));
await assert.rejects(accounts.register({username:'WAYFARER',password}));
await assert.rejects(accounts.login({username:'wayfarer',password:'an incorrect password'}));
const signed=await accounts.login({username:'wayfarer',password});assert.equal(signed.id,original.id);
const renewed=accounts.refresh({refreshToken:signed.refreshToken});assert.throws(()=>accounts.refresh({refreshToken:signed.refreshToken}));
now+=120001;assert.throws(()=>accounts.consume(renewed.id,renewed.ticket));
const recovered=await accounts.recover({username:'wayfarer',password:'another long private passphrase',recovery:original.recovery});
assert.throws(()=>accounts.refresh({refreshToken:renewed.refreshToken}));
await assert.rejects(accounts.recover({username:'wayfarer',password,recovery:original.recovery}));
await assert.rejects(accounts.login({username:'wayfarer',password}));
accounts.logout({refreshToken:recovered.refreshToken});assert.throws(()=>accounts.consume(recovered.id,recovered.ticket));
const id='a'.repeat(32),secret='b'.repeat(64);store.login(id,secret);store.transaction(id,'reward','',0,'abcdef12-12345678',0);
const linked=await accounts.register({username:'legacy',password,id,secret});assert.equal(linked.id,id);assert.equal(linked.profile.Coins,45);
await assert.rejects(accounts.register({username:'stolen',password,id,secret:'c'.repeat(64)}));
assert.equal(store.db.prepare('SELECT password FROM accounts WHERE username=?').get('legacy').password.includes(password),false);
now+=31*86400000;assert.throws(()=>accounts.refresh({refreshToken:linked.refreshToken}));
const racing=await Promise.allSettled([accounts.register({username:'race_user',password}),accounts.register({username:'race_user',password})]);assert.equal(racing.filter(x=>x.status==='fulfilled').length,1);
let available=false;const server=http.createServer(accountHandler(accounts,()=>available));await new Promise(resolve=>server.listen(0,'127.0.0.1',resolve));
try{const url='http://127.0.0.1:'+server.address().port+'/health';assert.equal((await fetch(url)).status,503);available=true;assert.equal((await fetch(url)).status,200);for(let i=0;i<31;i++){const response=await fetch('http://127.0.0.1:'+server.address().port+'/logout',{method:'POST',body:JSON.stringify({refreshToken:'invalid'})});assert.equal(response.status,i<30?200:429);}}finally{await new Promise(resolve=>server.close(resolve));}
console.log('PASS accounts: registration, case-insensitive uniqueness, password rejection, one-use/expired tickets, refresh rotation/expiry, recovery/revocation, legacy ownership, saved loot, concurrent registration and HTTP rate limit.');store.db.close();

