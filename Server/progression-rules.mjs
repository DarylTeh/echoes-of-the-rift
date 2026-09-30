// Authoritative mirror of the Unity progression model.
// Catalog tier identifies the item definition; enhancement is player-owned state.
export const NEW_PLAYER_CARRY_CHAPTERS=5;
export const GEAR_DUPLICATE_BAND=100;
export const SKILL_LEVEL_BAND=10;

const integer=(value,fallback=1)=>Number.isSafeInteger(value)?value:fallback;

export function gearDuplicatesForNext(level){
  level=Math.max(1,integer(level));
  return 5*(1+Math.floor(level/GEAR_DUPLICATE_BAND));
}
export function gearGoldForNext(level,baseGold=50){
  level=Math.max(1,integer(level));
  const decade=Math.floor(level/10);
  return Math.max(0,integer(baseGold,50))+8*level+2*decade*decade;
}
export function gearPowerMultiplier(level,rarityIndex=0){
  level=Math.max(0,integer(level,0));
  const levelFactor=1.8*(1-Math.exp(-level/140));
  return (1+levelFactor)*(1+0.10*Math.max(0,Math.min(4,integer(rarityIndex,0))));
}
export function skillCopiesForNext(level){
  level=Math.max(1,integer(level));
  return 1+Math.floor((level-1)/SKILL_LEVEL_BAND);
}
export function skillGemsForNext(level){
  level=Math.max(1,integer(level));
  const band=Math.floor((level-1)/SKILL_LEVEL_BAND);
  return 5+2*band+5*Math.floor(level/25);
}
export function skillPowerMultiplier(level){
  level=Math.max(0,integer(level,0));
  return 1+1.5*(1-Math.exp(-level/40));
}
export function chapterTargetPower(chapter){
  chapter=Math.max(1,integer(chapter));
  return Math.max(1,Math.round(120*Math.pow(1.12,chapter-1)));
}
export function hasWelcomeCarry(chapter){return integer(chapter)>=1&&chapter<=NEW_PLAYER_CARRY_CHAPTERS;}

// Adds only missing fields. Tier, counts, equipped slots and currencies are preserved.
// The returned object is safe to persist without changing AdminRevision.
export function normalizeProgression(profile){
  const p=profile&&typeof profile==='object'?profile:{};
  let changed=false;
  if(!Number.isSafeInteger(p.ProgressionVersion)||p.ProgressionVersion<1){p.ProgressionVersion=1;changed=true;}
  if(!Array.isArray(p.Items)){p.Items=[];changed=true;}
  for(const item of p.Items){
    if(!item||typeof item!=='object')continue;
    const fallback=Math.max(1,integer(item.Tier));
    if(!Number.isSafeInteger(item.EnhancementLevel)||item.EnhancementLevel<1){item.EnhancementLevel=fallback;changed=true;}
  }
  if(!Array.isArray(p.SkillIds)) {p.SkillIds=Array(6).fill('');changed=true;}
  if(!Array.isArray(p.SkillLevels)||p.SkillLevels.length!==6){
    p.SkillLevels=Array.from({length:6},(_,i)=>p.SkillIds[i]?1:0);changed=true;
  }else for(let i=0;i<6;i++){
    const expected=p.SkillIds[i]?Math.max(1,integer(p.SkillLevels[i])):0;
    if(p.SkillLevels[i]!==expected){p.SkillLevels[i]=expected;changed=true;}
  }
  if(!Array.isArray(p.EquippedIds)) {p.EquippedIds=Array(3).fill('');changed=true;}
  if(!Array.isArray(p.EquippedTiers)) {p.EquippedTiers=Array(3).fill(1);changed=true;}
  if(!Array.isArray(p.EquippedEnhancementLevels)||p.EquippedEnhancementLevels.length!==3){
    p.EquippedEnhancementLevels=Array.from({length:3},(_,i)=>{
      const stack=p.Items.find(x=>x&&x.ItemId===p.EquippedIds[i]&&x.Tier===p.EquippedTiers[i]);
      return stack?.EnhancementLevel??Math.max(1,integer(p.EquippedTiers[i]));
    });changed=true;
  }
  return {profile:p,changed};
}
