import {scrypt,randomBytes,timingSafeEqual} from 'node:crypto';
import {promisify} from 'node:util';
import {writeFileSync} from 'node:fs';
import {fileURLToPath} from 'node:url';
const derive=promisify(scrypt);
const hash=(password,salt)=>derive(password,salt,32,{N:32768,r:8,p:2,maxmem:64*1024*1024});
export async function createAdminCredentials(username,password){
 if(typeof username!=='string'||! /^[a-zA-Z0-9_]{3,32}$/.test(username)||typeof password!=='string'||password.length<8||password.length>128)throw Error('Use a 3-32 character username and an 8-128 character password.');
 const salt=randomBytes(32).toString('hex');return {username,salt,passwordHash:(await hash(password,salt)).toString('hex')};
}
export async function verifyAdmin(header,credentials){
 if(typeof header!=='string'||header.length>1024||!header.startsWith('Basic '))return false;
 const decoded=Buffer.from(header.slice(6),'base64').toString('utf8'),colon=decoded.indexOf(':');if(colon<0)return false;
 const username=decoded.slice(0,colon),password=decoded.slice(colon+1);if(password.length>128)return false;
 const result=await hash(password,credentials.salt);return username===credentials.username&&timingSafeEqual(result,Buffer.from(credentials.passwordHash,'hex'));
}
if(process.argv[1]===fileURLToPath(import.meta.url)){
 const result=await createAdminCredentials(process.env.RIFT_ADMIN_USERNAME,process.env.RIFT_ADMIN_PASSWORD);
 writeFileSync(new URL('./admin-auth.json',import.meta.url),JSON.stringify(result,null,2),{mode:0o600});
 console.log('Local admin credentials configured; only a salted password hash was saved.');
}
