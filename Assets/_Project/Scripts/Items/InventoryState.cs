using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class ItemStack
{
    public string ItemId;
    public int Tier=1;
    // Catalog identity stays in Tier; enhancement is player-owned progression.
    // Zero means a legacy/local object and falls back to Tier.
    public int EnhancementLevel;
    public int Count;
    public int EffectiveEnhancement=>EnhancementLevel>0?EnhancementLevel:Math.Max(1,Tier);
}
[Serializable]
public sealed class InventoryState : ISerializationCallbackReceiver
{
    public int Version=1;
    public int Coins;
    public int Gems;
    public int Level=1;
    public int AdminRevision;
    public int ProgressionVersion=1;
    public int CampaignStagesCompleted;
    public string[] SkillIds=new string[6];
    public int[] SkillLevels=new int[6];
    public CharacterAppearanceData Appearance;
    public List<ItemStack> Items=new List<ItemStack>();
    public string[] EquippedIds=new string[3];
    public int[] EquippedTiers=new[] {1,1,1};
    public int[] EquippedEnhancementLevels=new[] {1,1,1};
    public int Count(string id,int tier) { int total=0;foreach(var item in Items)if(item!=null&&item.ItemId==id&&item.Tier==tier)total=checked(total+item.Count);return total; }
    public int Count(string id,int tier,int enhancement) => Items.Find(x=>x.ItemId==id&&x.Tier==tier&&x.EffectiveEnhancement==enhancement)?.Count??0;
    public void Add(string id,int tier,int amount=1)
    {
        if(string.IsNullOrWhiteSpace(id)||tier<1||tier>5||amount<=0) throw new ArgumentException("Invalid item grant.");
        var stack=Items.Find(x=>x.ItemId==id&&x.Tier==tier&&x.EffectiveEnhancement==tier);
        if(stack==null) Items.Add(new ItemStack { ItemId=id,Tier=tier,EnhancementLevel=tier,Count=amount }); else stack.Count=checked(stack.Count+amount);
    }
    public void OnBeforeSerialize(){}
    public void OnAfterDeserialize()
    {
        if(Level==0)Level=1; // Legacy profiles predate character level.
        if(ProgressionVersion==0)ProgressionVersion=1;
        if(SkillIds==null||SkillIds.Length!=6)SkillIds=new string[6];
        if(SkillLevels==null||SkillLevels.Length!=6)SkillLevels=new int[6];
        for(int i=0;i<6;i++)SkillLevels[i]=string.IsNullOrEmpty(SkillIds[i])?0:Math.Max(1,SkillLevels[i]);
        if(EquippedIds==null||EquippedIds.Length!=3)EquippedIds=new string[3];
        if(EquippedTiers==null||EquippedTiers.Length!=3)EquippedTiers=new[]{1,1,1};
        if(EquippedEnhancementLevels==null||EquippedEnhancementLevels.Length!=3)EquippedEnhancementLevels=new int[3];
        for(int i=0;i<3;i++)EquippedEnhancementLevels[i]=Math.Max(1,EquippedEnhancementLevels[i]>0?EquippedEnhancementLevels[i]:EquippedTiers[i]);
        if(Items!=null)foreach(var item in Items)if(item!=null&&item.EnhancementLevel<1)item.EnhancementLevel=Math.Max(1,item.Tier);
    }
    public InventoryState Copy()=>JsonUtility.FromJson<InventoryState>(JsonUtility.ToJson(this));
    public bool IsValid()
    {
        if(Version!=1||ProgressionVersion<1||Coins<0||Gems<0||Level<1||AdminRevision<0||Items==null||EquippedIds==null||EquippedTiers==null||EquippedEnhancementLevels==null||EquippedIds.Length!=3||EquippedTiers.Length!=3||EquippedEnhancementLevels.Length!=3||SkillIds==null||SkillLevels==null||SkillIds.Length!=6||SkillLevels.Length!=6) return false;
        var keys=new HashSet<string>();
        foreach(var item in Items) if(item==null||string.IsNullOrWhiteSpace(item.ItemId)||item.Count<0||item.Tier<1||item.Tier>5||item.EnhancementLevel<1||!keys.Add(item.ItemId+"/"+item.Tier+"/"+item.EnhancementLevel)) return false;
        for(int i=0;i<3;i++) if(!string.IsNullOrEmpty(EquippedIds[i])&&Count(EquippedIds[i],EquippedTiers[i],EquippedEnhancementLevels[i])<1) return false;
        return true;
    }
}
