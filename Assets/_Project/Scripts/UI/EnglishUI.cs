using System;

// Initial English presentation catalogue. Keep stable item IDs out of player-facing copy.
public static partial class EnglishUI
{
    public const string InventoryTitle="Inventory & equipment", Close="Close", ShopBag="Shop / Bag",
        Equip="Equip", EquipBook="Equip to Q", Buy="Buy", Fuse="Upgrade", Previous="Previous", Next="Next",
        EquippedHero="Equipped hero", SelectedItem="Selected item", EmptyCategory="No items in this category.",
        SelectItem="Select an item to compare it.", Show="Show: ", Sort="Sort: ", Lore="Item story", Stats="Compare stats";
    public static readonly string[] Categories={"All", "Weapons", "Armour", "Charms", "Skills"};
    public static readonly string[] Sorting={"Name", "Enhancement", "Quantity"};
    public static string Totals(int gold,int items,int stacks,int page,int pages)=>$"{gold:N0} gold  /  {items:N0} items  /  {stacks} stacks  /  {page}/{pages}";
    public static string Signed(float value)=>value.ToString("+0.##;-0.##;0",System.Globalization.CultureInfo.InvariantCulture);
    public static string Comparison(ItemData item,int tier,ItemData equipped,int equippedTier)
    {
        if(item.Kind==ItemKind.SkillBook)return "Skill book / Primary Q\n"+item.description;
        float attack=item.FlatDamage*tier,health=item.FlatHealth*tier;
        return $"{item.Slot} / Gear bonus\nAttack {attack:0.##}  ({Signed(attack-(equipped!=null?equipped.FlatDamage*equippedTier:0))})\nHealth {health:0.##}  ({Signed(health-(equipped!=null?equipped.FlatHealth*equippedTier:0))})\n"+(item.Slot==EquipmentSlot.Weapon?WeaponCombat.Description(item.Family):"Change versus equipped item");
    }
    public static string Upgrade(ItemData item,int tier,int owned)
    {
        if(tier>=5)return "Next: higher enhancement / server migration required";
        return $"Next: +{tier+1} / {Math.Min(owned,2)}/2 copies / {10*tier} gold";
    }
    public static string VisualMilestone(int tier)=>tier<3?"+3: animated weapon":tier<4?"+4: weapon trail":tier<5?"+5: mythic trail":"Mythic trail unlocked";
}
