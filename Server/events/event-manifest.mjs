import {parseUtc} from './event-schedule.mjs';

export const EVENT_TYPES=Object.freeze([
  'login_calendar','token_exchange','milestone_track','daily_quests','boss_challenge',
  'raid_ladder','event_shop','collaboration_pack','double_drop','community_goal','news_inbox','tower_defense'
]);
const idPattern=/^[a-z0-9][a-z0-9_-]{2,63}$/;
const offerIdPattern=/^[a-z0-9][a-z0-9_-]{0,63}$/;

function validateReward(reward,label='reward'){
  if(!reward||typeof reward!=='object'||Array.isArray(reward))throw new Error(`${label} must be an object.`);
  if(reward.currency!==undefined){if(!['coins','gems'].includes(reward.currency)||!Number.isSafeInteger(reward.amount)||reward.amount<1||reward.amount>2_000_000_000)throw new Error(`${label} currency must use a bounded coins/gems amount.`);return;}
  if(typeof reward.itemId!=='string'||!/^[a-zA-Z0-9_-]{1,128}$/.test(reward.itemId)||!Number.isSafeInteger(reward.tier)||reward.tier<1||reward.tier>5||!Number.isSafeInteger(reward.count)||reward.count<1||reward.count>1_000_000)throw new Error(`${label} item must use a bounded itemId, tier and count.`);
}

export function validateEventManifest(input){
  if(!input||typeof input!=='object'||Array.isArray(input))throw new Error('Event manifest must be an object.');
  if(typeof input.id!=='string'||!idPattern.test(input.id))throw new Error('Event id must use 3-64 lowercase letters, digits, underscores or hyphens.');
  if(typeof input.version!=='number'||!Number.isSafeInteger(input.version)||input.version<1)throw new Error('Event version must be a positive integer.');
  if(!EVENT_TYPES.includes(input.type))throw new Error(`Unknown event type: ${input.type}.`);
  const start=parseUtc(input.startAt,'startAt'),end=parseUtc(input.endAt,'endAt');if(end<=start)throw new Error('endAt must be after startAt.');
  if(input.disabled!==undefined&&typeof input.disabled!=='boolean')throw new Error('disabled must be boolean.');
  if(input.config===undefined||!input.config||typeof input.config!=='object'||Array.isArray(input.config))throw new Error('config must be an object.');
  if(input.config.destination!==undefined){
   if(input.config.destination!=='campaign')throw new Error('Unsupported event destination. The only enabled destination is campaign.');
   if(!Array.isArray(input.config.eligibleModes)||!input.config.eligibleModes.includes(input.config.destination))throw new Error('Event destination must also be listed in eligibleModes.');
  }
  if(input.rewards!==undefined){
   if(!Array.isArray(input.rewards)||input.rewards.some(x=>!x||typeof x!=='object'||Array.isArray(x)))throw new Error('rewards must be an array of objects.');
   for(const reward of input.rewards)validateReward(reward,'Event reward');
  }
  if(input.config.shopOffers!==undefined){
   const offers=input.config.shopOffers;
   if(!Array.isArray(offers)||offers.length<1||offers.length>100)throw new Error('shopOffers must contain 1-100 offers.');
   const ids=new Set();
   for(const offer of offers){
    if(!offer||typeof offer!=='object'||Array.isArray(offer)||typeof offer.id!=='string'||!offerIdPattern.test(offer.id)||ids.has(offer.id))throw new Error('Shop offer ids must be unique lowercase identifiers.');ids.add(offer.id);
    if(!Number.isSafeInteger(offer.price)||offer.price<1||offer.price>2_000_000_000)throw new Error('Shop offer price must be a positive bounded integer.');
    if(!Number.isSafeInteger(offer.stock)||offer.stock<1||offer.stock>1_000_000)throw new Error('Shop offer stock must be between 1 and 1000000.');
    if(!Number.isSafeInteger(offer.playerLimit)||offer.playerLimit<1||offer.playerLimit>1000)throw new Error('Shop offer playerLimit must be between 1 and 1000.');
    validateReward(offer.reward,'Shop offer reward');
   }
   if(!['event_shop','token_exchange','collaboration_pack'].includes(input.type))throw new Error('Shop offers are only valid on shop-enabled event types.');
   if(typeof input.config.currencyMetric!=='string'||!offerIdPattern.test(input.config.currencyMetric))throw new Error('Shop offers require a currencyMetric.');
   if(input.config.restock!==undefined&&!['event','utc-day'].includes(input.config.restock))throw new Error('Shop restock must be event or utc-day.');
   if(input.config.purchaseLimitScope!==undefined&&!['event','utc-day'].includes(input.config.purchaseLimitScope))throw new Error('Shop purchaseLimitScope must be event or utc-day.');
  }
  if(input.config.progressPerClear!==undefined&&(!Number.isSafeInteger(input.config.progressPerClear)||input.config.progressPerClear<1||input.config.progressPerClear>100000))throw new Error('progressPerClear must be an integer from 1 to 100000.');
  if(input.config.dailyTokenCap!==undefined&&(!Number.isSafeInteger(input.config.dailyTokenCap)||input.config.dailyTokenCap<1||input.config.dailyTokenCap>2_000_000_000))throw new Error('dailyTokenCap must be a positive bounded integer.');
  return true;
}
export function normalizeEventManifest(input){
  validateEventManifest(input);
  return {id:input.id,version:input.version,type:input.type,title:String(input.title??input.id).slice(0,120),description:String(input.description??'').slice(0,1000),startAt:new Date(parseUtc(input.startAt,'startAt')).toISOString(),endAt:new Date(parseUtc(input.endAt,'endAt')).toISOString(),disabled:input.disabled===true,config:structuredClone(input.config),rewards:structuredClone(input.rewards??[])};
}
