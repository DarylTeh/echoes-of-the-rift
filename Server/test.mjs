import {createStore} from './persistence.mjs';
import assert from 'node:assert/strict';
const catalog=[{id:'gear-0',kind:0,slot:0,tier:1}];const store=createStore(':memory:',catalog);const id='a'.repeat(32),secret='b'.repeat(64);
assert.equal(store.login(id,secret).Coins,30);assert.throws(()=>store.login(id,'c'.repeat(64)));
store.transaction(id,'reward','',0,'12345678',0);store.transaction(id,'reward','',0,'12345678',0);assert.equal(store.get(id).Coins,45);assert.equal(store.get(id).Items[0].Count,2);
store.transaction(id,'fuse','gear-0',1);assert.equal(store.get(id).EquippedTiers[0],2);assert.equal(store.get(id).Coins,35);
const before=JSON.stringify(store.get(id));assert.throws(()=>store.transaction(id,'fuse','gear-0',2));assert.equal(JSON.stringify(store.get(id)),before);
assert.throws(()=>store.transaction(id,'equip','missing',5));assert.throws(()=>store.transaction(id,'reward','',0,'bad',99));
console.log('PASS central store: credentials, atomic fusion, duplicate reward receipt, invalid transactions.');store.db.close();
