using System;
using System.IO;
using UnityEngine;

public static class ProgressionValidation
{
    public static void RunCLI()
    {
        if(ProgressionRules.GearDuplicatesForNext(1)!=5||ProgressionRules.GearDuplicatesForNext(99)!=5||ProgressionRules.GearDuplicatesForNext(100)!=10||ProgressionRules.GearDuplicatesForNext(200)!=15) throw new Exception("Gear duplicate bands are incorrect.");
        if(ProgressionRules.SkillCopiesForNext(1)!=1||ProgressionRules.SkillCopiesForNext(11)!=2||ProgressionRules.SkillGemsForNext(1)!=5||ProgressionRules.SkillGemsForNext(25)!=14) throw new Exception("Skill progression bands are incorrect.");
        if(!(ProgressionRules.GearPowerMultiplier(200,ItemRarity.Mythic)>ProgressionRules.GearPowerMultiplier(100,ItemRarity.Mythic)&&ProgressionRules.GearPowerMultiplier(1000,ItemRarity.Mythic)<ProgressionRules.GearPowerMultiplier(200,ItemRarity.Mythic)*1.3f)) throw new Exception("Gear soft cap is incorrect.");
        var displayItem=ScriptableObject.CreateInstance<ItemData>(); displayItem.rarity=ItemRarity.Rare;
        var display=EnglishUI.Stack(displayItem,12,4);
        if(!display.Contains("*")||!display.Contains("+12")||display.Contains("T")) throw new Exception("Star-plus item display is incorrect.");
        UnityEngine.Object.DestroyImmediate(displayItem);
        Debug.Log("PROGRESSION_RULES_OK: gear bands, skill costs, star display and soft cap checks passed.");
        string path=Path.GetFullPath("Logs/fuse-test-"+Guid.NewGuid().ToString("N")+".json");
        var go=new GameObject("FuseTest"); var item=ScriptableObject.CreateInstance<ItemData>(); item.Id="test"; item.BaseFuseCost=10;
        var manager=go.AddComponent<ItemTierUpManager>(); manager.Definitions=new[] {item}; manager.SavePath=path;
        var state=new InventoryState { Coins=100 }; state.Add("test",3,2); state.EquippedIds[0]="test"; state.EquippedTiers[0]=3; state.EquippedEnhancementLevels[0]=3; manager.Configure(state);
        try
        {
            if(!manager.TryFuse("test",3,out _)||manager.State.Count("test",4)!=1||manager.State.Count("test",3)!=0||manager.State.Coins!=70||manager.State.EquippedTiers[0]!=4) throw new Exception("Fuse transaction failed.");
            if(manager.TryFuse("test",4,out _)||manager.State.Coins!=70) throw new Exception("Invalid fuse consumed currency.");
            if(ProfileStore.Load(path).Count("test",4)!=1) throw new Exception("Save roundtrip failed.");
            manager.Grant("test",5); File.WriteAllText(path,"broken save");
            if(ProfileStore.Load(path).Coins!=70) throw new Exception("Backup recovery failed.");
        }
        finally { UnityEngine.Object.DestroyImmediate(go); UnityEngine.Object.DestroyImmediate(item); }
        Debug.Log("STEP_07_OK: atomic fuse, equipped tier migration, no-cost failure, save/recovery checks passed.");
    }
}
