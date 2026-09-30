import {parseUtc} from './event-schedule.mjs';

export const EVENT_TYPES=Object.freeze([
  'login_calendar','token_exchange','milestone_track','daily_quests','boss_challenge',
  'raid_ladder','event_shop','collaboration_pack','double_drop','community_goal','news_inbox','tower_defense'
]);
const idPattern=/^[a-z0-9][a-z0-9_-]{2,63}$/;

export function validateEventManifest(input){
  if(!input||typeof input!=='object'||Array.isArray(input))throw new Error('Event manifest must be an object.');
  if(typeof input.id!=='string'||!idPattern.test(input.id))throw new Error('Event id must use 3-64 lowercase letters, digits, underscores or hyphens.');
  if(typeof input.version!=='number'||!Number.isSafeInteger(input.version)||input.version<1)throw new Error('Event version must be a positive integer.');
  if(!EVENT_TYPES.includes(input.type))throw new Error(`Unknown event type: ${input.type}.`);
  const start=parseUtc(input.startAt,'startAt'),end=parseUtc(input.endAt,'endAt');if(end<=start)throw new Error('endAt must be after startAt.');
  if(input.disabled!==undefined&&typeof input.disabled!=='boolean')throw new Error('disabled must be boolean.');
  if(input.config===undefined||!input.config||typeof input.config!=='object'||Array.isArray(input.config))throw new Error('config must be an object.');
  if(input.rewards!==undefined){
   if(!Array.isArray(input.rewards)||input.rewards.some(x=>!x||typeof x!=='object'||Array.isArray(x)))throw new Error('rewards must be an array of objects.');
   for(const reward of input.rewards){
    if(reward.currency!==undefined){if(!['coins','gems'].includes(reward.currency)||!Number.isSafeInteger(reward.amount)||reward.amount<1||reward.amount>2_000_000_000)throw new Error('Currency rewards must use bounded coins/gems amounts.');}
    else if(typeof reward.itemId!=='string'||!/^[a-zA-Z0-9_-]{1,128}$/.test(reward.itemId)||!Number.isSafeInteger(reward.tier)||reward.tier<1||reward.tier>5||!Number.isSafeInteger(reward.count)||reward.count<1||reward.count>1_000_000)throw new Error('Item rewards must use a bounded itemId, tier and count.');
   }
  }
  return true;
}
export function normalizeEventManifest(input){
  validateEventManifest(input);
  return {id:input.id,version:input.version,type:input.type,title:String(input.title??input.id).slice(0,120),description:String(input.description??'').slice(0,1000),startAt:new Date(parseUtc(input.startAt,'startAt')).toISOString(),endAt:new Date(parseUtc(input.endAt,'endAt')).toISOString(),disabled:input.disabled===true,config:structuredClone(input.config),rewards:structuredClone(input.rewards??[])};
}
