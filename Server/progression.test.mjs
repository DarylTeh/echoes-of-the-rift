import assert from 'node:assert/strict';
import {chapterTargetPower,gearDuplicatesForNext,gearGoldForNext,gearPowerMultiplier,hasWelcomeCarry,normalizeProgression,skillCopiesForNext,skillGemsForNext,skillPowerMultiplier} from './progression-rules.mjs';

assert.equal(gearDuplicatesForNext(1),5);assert.equal(gearDuplicatesForNext(99),5);assert.equal(gearDuplicatesForNext(100),10);assert.equal(gearDuplicatesForNext(200),15);
assert.equal(gearGoldForNext(1),58);assert.equal(gearGoldForNext(100),1050);assert.equal(gearGoldForNext(200),2450);
assert.ok(gearPowerMultiplier(200,4)>gearPowerMultiplier(100,4));assert.ok(gearPowerMultiplier(1000,4)<gearPowerMultiplier(200,4)*1.3);
assert.equal(skillCopiesForNext(1),1);assert.equal(skillCopiesForNext(11),2);assert.equal(skillGemsForNext(1),5);assert.equal(skillGemsForNext(25),14);
assert.ok(skillPowerMultiplier(100)>skillPowerMultiplier(10));assert.equal(chapterTargetPower(1),120);assert.equal(chapterTargetPower(5),189);assert.equal(chapterTargetPower(20),1034);
assert.equal(hasWelcomeCarry(5),true);assert.equal(hasWelcomeCarry(6),false);
const legacy={Version:1,Coins:30,Items:[{ItemId:'gear-0',Tier:3,Count:7}],SkillIds:['starter','','','','',''],EquippedIds:['gear-0','',''],EquippedTiers:[3,1,1]};
const migrated=normalizeProgression(legacy);assert.equal(migrated.changed,true);assert.equal(migrated.profile.Items[0].Tier,3);assert.equal(migrated.profile.Items[0].EnhancementLevel,3);assert.deepEqual(migrated.profile.SkillLevels,[1,0,0,0,0,0]);assert.deepEqual(migrated.profile.EquippedEnhancementLevels,[3,1,1]);
assert.equal(normalizeProgression(migrated.profile).changed,false);
console.log('PASS progression: server mirror costs, soft caps, campaign pressure, carry window and legacy profile migration.');
