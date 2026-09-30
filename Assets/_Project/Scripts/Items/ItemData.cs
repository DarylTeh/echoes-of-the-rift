using UnityEngine;
public enum ItemRarity { Common,Rare,Epic,Legendary,Mythic }
public enum ItemKind { Gear, SkillBook }
public enum EquipmentSlot { Weapon, Armour, Charm }
[CreateAssetMenu(menuName="EchoesOfTheRift/Item")]
public sealed class ItemData : ScriptableObject
{
    public Sprite iconSprite;
    public string Family;
    public ItemRarity rarity;
    public int itemTier=1;
    [TextArea] public string description;
    [TextArea] public string villageQuote;
    public string Id;
    public string DisplayName;
    public ItemKind Kind;
    public EquipmentSlot Slot;
    public float FlatDamage;
    public float FlatHealth;
    public SpellData Spell;
    [Min(1)] public int DuplicatesPerFuse=2;
    [Min(0)] public int BaseFuseCost=10;
}
