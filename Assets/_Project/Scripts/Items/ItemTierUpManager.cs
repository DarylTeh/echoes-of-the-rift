using System;
using UnityEngine;

public sealed class ItemTierUpManager : MonoBehaviour
{
    public InventoryState State { get; private set; }=new InventoryState();
    public ItemData[] Definitions;
    public CharacterCustomizer Customizer;
    public CharacterStats Stats;
    public event Action Changed;
    public string SavePath;
    public Action<string,string,int> ServerTransaction;
    public void Configure(InventoryState state) { if(!state.IsValid()) throw new ArgumentException("Invalid inventory."); State=state; ApplyEquipment(); }
    public bool TryFuse(string itemId,int tier,out string result)
    {
        if(ServerTransaction!=null){ServerTransaction("fuse",itemId,tier);result="Waiting for server...";return false;}
        var item=Array.Find(Definitions,x=>x!=null&&x.Id==itemId);
        if(item==null||tier<1||tier>=5) { result="This item cannot be fused further."; return false; }
        int needed=Mathf.Max(2,item.DuplicatesPerFuse); long cost=(long)Mathf.Max(0,item.BaseFuseCost)*tier;
        if(State.Count(itemId,tier)<needed||State.Coins<cost) { result=$"Requires {needed} matching tier {tier} items and {cost} coins."; return false; }
        var next=State.Copy(); next.Items.Find(x=>x.ItemId==itemId&&x.Tier==tier).Count-=needed; next.Coins-=(int)cost; next.Add(itemId,tier+1);
        if(item.Kind==ItemKind.Gear)
        {
            int slot=(int)item.Slot;
            if(next.EquippedIds[slot]==itemId&&next.EquippedTiers[slot]==tier) { next.EquippedTiers[slot]=tier+1; next.EquippedEnhancementLevels[slot]=tier+1; }
        }
        ProfileStore.Save(next,SavePath); State=next; ApplyEquipment(); Changed?.Invoke(); result=$"{item.DisplayName} reached tier {tier+1}."; return true;
    }
    public bool Equip(string itemId,int tier)
    {
        if(ServerTransaction!=null){ServerTransaction("equip",itemId,tier);return true;}
        var item=Array.Find(Definitions,x=>x!=null&&x.Id==itemId);
        if(item==null||item.Kind!=ItemKind.Gear||State.Count(itemId,tier)<1) return false;
        var next=State.Copy(); next.EquippedIds[(int)item.Slot]=itemId; next.EquippedTiers[(int)item.Slot]=tier;var stack=next.Items.Find(x=>x.ItemId==itemId&&x.Tier==tier);next.EquippedEnhancementLevels[(int)item.Slot]=stack?.EffectiveEnhancement??tier;
        ProfileStore.Save(next,SavePath); State=next; ApplyEquipment(); Changed?.Invoke(); return true;
    }
    public bool EquipSkill(string itemId,int slot=0)
    {
        if(ServerTransaction!=null){if(slot<0||slot>=6)return false;ServerTransaction("skill",itemId,slot+1);return true;}
        var item=Array.Find(Definitions,x=>x.Id==itemId);if(item==null||item.Spell==null||slot<0||slot>=6||!State.Items.Exists(x=>x.ItemId==itemId&&x.Count>0))return false;
        var next=State.Copy();if(next.SkillIds==null||next.SkillIds.Length!=6)next.SkillIds=new string[6];if(next.SkillLevels==null||next.SkillLevels.Length!=6)next.SkillLevels=new int[6];next.SkillIds[slot]=item.Spell.Id;next.SkillLevels[slot]=Mathf.Max(1,next.SkillLevels[slot]);ProfileStore.Save(next,SavePath);State=next;ApplyEquipment();Changed?.Invoke();return true;
    }
    public void Grant(string itemId,int coins,int campaignStage=0)
    {
        if(ServerTransaction!=null)throw new InvalidOperationException("Only the dedicated server grants rewards.");
        if(coins<0||Array.Find(Definitions,x=>x!=null&&x.Id==itemId)==null) throw new ArgumentException("Unknown reward.");
        var next=State.Copy(); next.Coins=checked(next.Coins+coins); next.Add(itemId,1); next.CampaignStagesCompleted=Mathf.Max(next.CampaignStagesCompleted,campaignStage); ProfileStore.Save(next,SavePath); State=next; Changed?.Invoke();
    }
    public void ApplyEquipment()
    {
        var skills=Customizer!=null?Customizer.GetComponent<SkillStanceSwapper>():null;
        if(skills!=null&&State.SkillIds!=null)for(int i=0;i<Mathf.Min(6,State.SkillIds.Length);i++){var book=Array.Find(Definitions,x=>x.Spell!=null&&x.Spell.Id==State.SkillIds[i]);if(book!=null){if(i<3)skills.Primary[i]=book.Spell;else skills.Secondary[i-3]=book.Spell;}}
        float damage=0,health=0; int bestTier=1;
        for(int i=0;i<3;i++)
        {
            var item=Array.Find(Definitions??Array.Empty<ItemData>(),x=>x!=null&&x.Id==State.EquippedIds[i]);
            if(item==null) continue;int enhancement=State.EquippedEnhancementLevels!=null&&i<State.EquippedEnhancementLevels.Length?State.EquippedEnhancementLevels[i]:State.EquippedTiers[i];damage+=item.FlatDamage*enhancement; health+=item.FlatHealth*enhancement; bestTier=Mathf.Max(bestTier,enhancement);
        }
        if(Stats!=null) { Stats.Damage.Flat=damage; Stats.Health.Flat=health; }
        if(Customizer!=null) { Customizer.SetEquipmentTier(bestTier);var weapon=Array.Find(Definitions,x=>x.Id==State.EquippedIds[0]);int weaponEnhancement=State.EquippedEnhancementLevels!=null&&State.EquippedEnhancementLevels.Length>0?State.EquippedEnhancementLevels[0]:State.EquippedTiers[0];Customizer.SetWeapon(weapon!=null?weapon.Family:"sword",weaponEnhancement); }
    }
}
