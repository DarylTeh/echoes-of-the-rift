using UnityEditor;
using UnityEngine;
public static class CombatExpansionSetup
{
    public static void BuildCLI()
    {
        IllustratedArtSetup.BuildCLI();
        string[] families={"spear","boomerang","pistol"},names={"Tidepiercer","Verdant Return","Emberlock"};
        for(int f=0;f<3;f++)for(int tier=1;tier<=5;tier++)
        {
            string id=families[f]+"-"+tier;
            var item=ContentSetup.Asset<ItemData>("Assets/_Project/Resources/Catalog/"+id+".asset");
            item.Id=id;item.DisplayName=names[f]+" "+tier;item.Family=families[f];item.Kind=ItemKind.Gear;
            item.Slot=EquipmentSlot.Weapon;item.itemTier=tier;item.rarity=(ItemRarity)(tier-1);item.FlatDamage=4;item.FlatHealth=0;
            item.description=WeaponCombat.Description(families[f])+". Adds 4 attack per fusion tier.";
            item.villageQuote="Forged for a wayfarer who fights on the move.";
            item.iconSprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Icons/bow-1.png");EditorUtility.SetDirty(item);
        }
        string[] ids={"tempest-volley","astral-lance","crimson-cyclone","solar-hammers"};
        string[] titles={"Tempest Volley","Astral Lance","Crimson Cyclone","Solar Hammers"};
        string[] descriptions={"Fire 5 arrows, 14 damage each.","Pierce up to 4 foes for 40 damage each.","Spin 3 times for 24 damage per hit.","6 rotating sweeps, 16 damage per hit."};
        SpellEffect[] effects={SpellEffect.FanShot,SpellEffect.PiercingLance,SpellEffect.Whirlwind,SpellEffect.OrbitHammers};
        float[] powers={40,40,72,96},ranges={9,10,2,2.4f},cooldowns={7,6,8,10};
        Color[] colors={Color.cyan,new Color(.7f,.25f,1),new Color(1,.2f,.35f),GameUI.Gold};
        for(int i=0;i<4;i++)
        {
            var skill=ContentSetup.Asset<SpellData>("Assets/_Project/Resources/Skills/"+ids[i]+".asset");
            skill.Id=ids[i];skill.DisplayName=titles[i];skill.Archetype="Wayfarer";skill.Effect=effects[i];skill.Power=powers[i];skill.Range=ranges[i];skill.Cooldown=cooldowns[i];skill.Color=colors[i];skill.mechanicalDescription=descriptions[i];skill.villageQuote="A technique carried between the old temples.";
            skill.skillIcon=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Icons/mage-spark.png");EditorUtility.SetDirty(skill);
            var book=ContentSetup.Asset<ItemData>("Assets/_Project/Resources/Catalog/book-"+ids[i]+".asset");
            book.Id="book-"+ids[i];book.DisplayName=titles[i]+" tome";book.Kind=ItemKind.SkillBook;book.Family="book";book.Spell=skill;book.itemTier=2;book.rarity=ItemRarity.Rare;book.iconSprite=skill.skillIcon;book.description=descriptions[i];book.villageQuote=skill.villageQuote;EditorUtility.SetDirty(book);
        }
        AssetDatabase.SaveAssets();ItemDatabaseGenerator.ExportServerCatalogueCLI();
        Debug.Log("COMBAT_EXPANSION_OK: 15 weapons, 4 books/skills, shared expansion atlas.");
    }
}
