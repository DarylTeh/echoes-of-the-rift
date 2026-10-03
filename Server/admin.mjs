import http from 'node:http';
import {readFileSync} from 'node:fs';
import {fileURLToPath} from 'node:url';
import {createRequire} from 'node:module';
import {join,resolve} from 'node:path';
import {createStore} from './persistence.mjs';
import {createAccounts} from './accounts.mjs';
import {createAdminStore,ApiError} from './admin-store.mjs';
import {specification,actions,BASE} from './admin-openapi.mjs';
import {verifyAdmin} from './admin-auth.mjs';
const require=createRequire(import.meta.url),assets=require('swagger-ui-dist').getAbsoluteFSPath();
const html='<!doctype html><html lang="en"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>Echoes of the Rift — Local Admin</title><link rel="stylesheet" href="${BASE}/swagger-ui.css"></head><body><div id="swagger-ui"></div><script src="${BASE}/swagger-ui-bundle.js"></script><script src="${BASE}/init.js"></script></body></html>'.replaceAll('${BASE}',BASE);
const init='window.onload=()=>{window.ui=SwaggerUIBundle({url:"${BASE}/openapi.json",dom_id:"#swagger-ui",deepLinking:true,validatorUrl:null,persistAuthorization:false,queryConfigEnabled:false,displayRequestDuration:true,defaultModelsExpandDepth:-1,presets:[SwaggerUIBundle.presets.apis]});};'.replaceAll('${BASE}',BASE);
export function startAdmin({port=8083,path=new URL('./progress.sqlite',import.meta.url),credentials,catalog=[]}={}){
 if(!credentials?.username||! /^[a-f0-9]{64}$/.test(credentials.salt)||! /^[a-f0-9]{64}$/.test(credentials.passwordHash))throw Error('Configure valid admin credentials first.');
 let failedLogins=0,windowStart=Date.now(),authInFlight=0;
 const store=createStore(path,catalog);createAccounts(store);const admin=createAdminStore(store,catalog),spec=specification();
 const staticFiles={'/swagger-ui.css':['text/css',readFileSync(join(assets,'swagger-ui.css'))],'/swagger-ui-bundle.js':['text/javascript',readFileSync(join(assets,'swagger-ui-bundle.js'))]};
 const server=http.createServer(async(req,res)=>{
  const reply=(status,value,type='application/json')=>{res.writeHead(status,{'Content-Type':type,'Cache-Control':'no-store','X-Content-Type-Options':'nosniff','Content-Security-Policy':"default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; connect-src 'self'; frame-ancestors 'none'; object-src 'none'",'Referrer-Policy':'no-referrer'});res.end(type==='application/json'?JSON.stringify(value):value);};
  const host=req.headers.host?.toLowerCase();const allowed=[`localhost:${server.address().port}`,`127.0.0.1:${server.address().port}`];
  if(!allowed.includes(host)||!['127.0.0.1','::ffff:127.0.0.1'].includes(req.socket.remoteAddress))return reply(403,{error:'Localhost requests only.'});
  if(req.headers.origin&&req.headers.origin!==`http://${host}`)return reply(403,{error:'Cross-origin requests are not allowed.'});
  try{
   const url=new URL(req.url,`http://${host}`);
   if(url.pathname!==BASE&&!url.pathname.startsWith(BASE+'/'))return reply(404,{error:'Not found.'});
   const route=url.pathname.slice(BASE.length)||'/';
   if(req.method==='GET'){
    if(route==='/'||route==='/docs')return reply(200,html,'text/html');
    if(route==='/init.js')return reply(200,init,'text/javascript');
    if(route==='/openapi.json')return reply(200,spec);
    if(route==='/health')return reply(200,{ok:true,service:'echoes-local-admin'});
    if(staticFiles[route])return reply(200,staticFiles[route][1],staticFiles[route][0]);
   }
   if(Date.now()-windowStart>60000){failedLogins=0;windowStart=Date.now();}
   if(failedLogins>=10||authInFlight>=2)return reply(429,{error:'Too many login attempts; try again shortly.'});
   let authenticated=false;authInFlight++;try{authenticated=await verifyAdmin(req.headers.authorization,credentials);}finally{authInFlight--;}
   if(!authenticated){failedLogins++;return reply(401,{error:'Authorize with your admin username and password.'});}
   const query=Object.fromEntries(url.searchParams);
   if(req.method==='GET'){
    if(route==='/api/players')return reply(200,admin.players(query));
    if(route==='/api/accounts')return reply(200,admin.accounts(query));
    if(route==='/api/audit')return reply(200,admin.audit(query));
   if(route==='/api/catalog'){
     const limit=Number(query.limit??25),offset=Number(query.offset??0);if(!Number.isInteger(limit)||limit<1||limit>100||!Number.isInteger(offset)||offset<0)throw new ApiError(400,'Invalid pagination.');
     const items=catalog.filter(x=>(x.id+' '+(x.name??'')).toLowerCase().includes((query.q??'').toLowerCase()));return reply(200,{total:items.length,limit,offset,items:items.slice(offset,offset+limit)});
    }
    if(route==='/api/events')return reply(200,admin.events());
    const match=route.match(/^\/api\/players\/([a-f0-9]{32})$/);if(match)return reply(200,{id:match[1],profile:admin.profile(match[1])});
   }
   if(req.method==='POST'&&(route==='/api/events'||route==='/api/events/preview'||/^\/api\/events\/[a-z0-9][a-z0-9_-]{2,63}\/disable$/.test(route))){
    if(!/^application\/json(?:;|$)/i.test(req.headers['content-type']??''))throw new ApiError(400,'Use application/json.');
    let body='';for await(const chunk of req){body+=chunk;if(Buffer.byteLength(body)>65536)throw new ApiError(400,'Request too large.');}
    let data={};try{data=body.trim()?JSON.parse(body):{};}catch{throw new ApiError(400,'Invalid JSON.');}
    if(route==='/api/events/preview'){
     if(!data||typeof data!=='object'||Array.isArray(data)||!data.manifest||Object.keys(data).some(key=>!['manifest','previewAt'].includes(key)))throw new ApiError(400,'Provide only manifest and optional previewAt.');
     return reply(200,admin.previewEvent(data.manifest,data.previewAt));
    }
    if(route==='/api/events')return reply(200,admin.publishEvent(data));
    const match=route.match(/^\/api\/events\/([a-z0-9][a-z0-9_-]{2,63})\/disable$/);return reply(200,admin.disableEvent(match[1]));
   }
   const match=route.match(/^\/api\/players\/([a-f0-9]{32})\/([a-zA-Z]+)$/);
   if(match&&actions.includes(match[2])&&req.method===(match[2]==='deleteItem'?'DELETE':'POST')){
    if(!/^application\/json(?:;|$)/i.test(req.headers['content-type']??''))throw new ApiError(400,'Use application/json.');
    let body='';for await(const chunk of req){body+=chunk;if(Buffer.byteLength(body)>16384)throw new ApiError(400,'Request too large.');}
    let data;try{data=JSON.parse(body);}catch{throw new ApiError(400,'Invalid JSON.');}
    return reply(200,admin.mutate(match[1],match[2],data));
   }
   reply(404,{error:'Unknown route or method.'});
  }catch(e){reply(e instanceof ApiError?e.status:500,{error:e instanceof ApiError?e.message:'Admin operation failed; no partial change was committed.'});}
 });
 server.requestTimeout=15000;server.headersTimeout=10000;server.listen(port,'127.0.0.1');return {server,store,admin};
}
if(process.argv[1]===fileURLToPath(import.meta.url)){
 const credentials=JSON.parse(readFileSync(new URL('./admin-auth.json',import.meta.url),'utf8')); const data=JSON.parse(readFileSync(new URL('./catalog.json',import.meta.url)));
 const path=resolve(process.env.COOKIE_DB_PATH||fileURLToPath(new URL('./progress.sqlite',import.meta.url)));
 const service=startAdmin({port:Number(process.env.RIFT_ADMIN_PORT||8083),path,credentials,catalog:data.items??data});
 service.server.on('listening',()=>console.log(`Admin Swagger: http://localhost:${service.server.address().port}${BASE}/ | Database: ${path} | Authorize with your admin username and password`));
 service.server.on('error',e=>{console.error(e.message);service.store.db.close();process.exitCode=1;});
}
