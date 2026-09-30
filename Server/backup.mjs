import {DatabaseSync} from 'node:sqlite';
import {mkdirSync,existsSync} from 'node:fs';
import {fileURLToPath} from 'node:url';
import {resolve,join} from 'node:path';
import {randomBytes} from 'node:crypto';
const source=resolve(process.env.COOKIE_DB_PATH||fileURLToPath(new URL('./progress.sqlite',import.meta.url)));
if(!existsSync(source))throw Error('Database does not exist; nothing to back up.');
const directory=fileURLToPath(new URL('../Backups/',import.meta.url));mkdirSync(directory,{recursive:true});
const destination=join(directory,'progress-'+new Date().toISOString().replace(/[:.]/g,'-')+'-'+randomBytes(3).toString('hex')+'.sqlite');
const db=new DatabaseSync(source);try{db.exec('PRAGMA busy_timeout=5000');db.prepare('VACUUM INTO ?').run(destination);console.log('Consistent SQLite backup: '+destination);}finally{db.close();}
