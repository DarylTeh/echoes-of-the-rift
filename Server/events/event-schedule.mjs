export const EVENT_STATES=Object.freeze(['draft','scheduled','active','ended','disabled']);

export function parseUtc(value,name='timestamp'){
  if(typeof value!=='string'||!/Z$/.test(value))throw new Error(`${name} must be an ISO-8601 UTC timestamp ending in Z.`);
  const time=Date.parse(value);if(!Number.isFinite(time))throw new Error(`${name} is invalid.`);return time;
}
export function eventState(event,now=Date.now()){
  if(event.disabled===true)return 'disabled';
  const start=parseUtc(event.startAt,'startAt'),end=parseUtc(event.endAt,'endAt');
  if(end<=start)throw new Error('endAt must be after startAt.');
  if(now<start)return 'scheduled';if(now>=end)return 'ended';return 'active';
}
export function activeEvents(events,now=Date.now()){
  return events.filter(event=>{try{return eventState(event,now)==='active';}catch{return false;}});
}
export function nextBoundary(event,now=Date.now()){
  const start=parseUtc(event.startAt,'startAt'),end=parseUtc(event.endAt,'endAt');
  if(now<start)return new Date(start).toISOString();if(now<end)return new Date(end).toISOString();return null;
}
