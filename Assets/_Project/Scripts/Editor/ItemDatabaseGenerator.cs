using System;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

public static class ItemDatabaseGenerator
{
    public static void BuildCLI()
    {
        if(ProjectValidation.DetectPipeline()!="BuiltIn")throw new Exception("Pixel pipeline requires the current Built-in renderer.");
        AssetDatabase.Refresh();
        const string fontPath="Assets/_Project/Resources/Pixel/PixelFont.asset";
        var font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontPath);
        if(font==null)
        {
            font=TMP_FontAsset.CreateFontAsset(Resources.Load<Font>("Pixel/Tiny5"),16,1,GlyphRenderMode.RASTER,512,512,AtlasPopulationMode.Dynamic);
            string chars="";for(int i=32;i<127;i++)chars+=(char)i;chars+="·…–—+";
            font.TryAddCharacters(chars);font.atlasPopulationMode=AtlasPopulationMode.Static;
            AssetDatabase.CreateAsset(font,fontPath);
            foreach(var atlas in font.atlasTextures){atlas.filterMode=FilterMode.Point;AssetDatabase.AddObjectToAsset(atlas,font);}
            AssetDatabase.AddObjectToAsset(font.material,font);
        }
        string[] families={"sword","dagger","greatsword","staff","wand","bow","crossbow","scythe","hammer","shield","helmet","chestpiece","pauldrons","leggings","boots","ring","amulet"};
        string[] elements={"Ashen","Verdant","Stormglass","Eclipse","Dawnstar"};
        int count=0;
        for(int family=0;family<families.Length;family++)for(int tier=1;tier<=5;tier++)
        {
            string id=$"{families[family]}-{tier}";
            var item=ContentSetup.Asset<ItemData>($"Assets/_Project/Resources/Catalog/{id}.asset");
            item.Id=id;item.DisplayName=$"{elements[tier-1]} {families[family]}";item.Family=families[family];item.rarity=(ItemRarity)(tier-1);item.itemTier=tier;
            item.Slot=family<10?EquipmentSlot.Weapon:family<15?EquipmentSlot.Armour:EquipmentSlot.Charm;
            item.FlatDamage=family<10?3+family%4:0;item.FlatHealth=family>=10?8+family%5:0;
            item.description=$"A {elements[tier-1].ToLowerInvariant()} {families[family]} granting {(family<10?item.FlatDamage+" attack":item.FlatHealth+" health")} per fusion tier.";
            item.villageQuote=$"\"I tempered this {families[family]} beneath the {elements[tier-1].ToLowerInvariant()} sky; may it bring you home.\" - Huldra, village smith";
            item.iconSprite=SaveIcon(id,PixelArt.Icon(item.Family,family,tier));EditorUtility.SetDirty(item);count++;
        }
        string[] archetypes={"Fighter","Mage","Healer","Bard","Assassin"};
        string[] verbs={"Spark","Tempest","Sanctuary","Comet","Echo","Oath"};
        for(int a=0;a<5;a++)for(int b=0;b<6;b++)
        {
            string id=$"{archetypes[a].ToLowerInvariant()}-{verbs[b].ToLowerInvariant()}";
            var skill=ContentSetup.Asset<SpellData>($"Assets/_Project/Resources/Skills/{id}.asset");skill.Id=id;skill.DisplayName=archetypes[a]+" "+verbs[b];skill.Archetype=archetypes[a];skill.Effect=new[]{SpellEffect.Burst,SpellEffect.DashStrike,SpellEffect.Mend,SpellEffect.SummonRune,SpellEffect.StatBuff,SpellEffect.StanceModifier}[b];skill.Cooldown=3+b;skill.Power=18+a*3+b;skill.Range=b%3==1?2.2f:6;
            skill.Color=Color.HSVToRGB(a*.18f,.6f,.95f);skill.skillIcon=SaveIcon(id,PixelArt.Icon("skill",a*6+b,1+b%5));
            skill.mechanicalDescription=b==3?$"Summon a rune that pulses {skill.Power/3:0} damage three times.":b==4?"Gain 25% basic attack damage for 6 seconds.":b==5?"Switch to the other spell stance.":b==1?$"Dodge and strike nearby foes for {skill.Power} damage.":skill.Effect==SpellEffect.Mend?$"Restore {skill.Power} health.":skill.Effect==SpellEffect.Burst?$"Strike nearby enemies for {skill.Power} damage.":$"Fire a bolt for {skill.Power} damage.";
            skill.villageQuote=$"\"The {verbs[b].ToLowerInvariant()} answers a {archetypes[a].ToLowerInvariant()} who listens.\" - Vane, lantern keeper";EditorUtility.SetDirty(skill);
            var book=ContentSetup.Asset<ItemData>($"Assets/_Project/Resources/Catalog/book-{id}.asset");book.Id="book-"+id;book.DisplayName=skill.DisplayName+" tome";book.Kind=ItemKind.SkillBook;book.Family="book";book.Spell=skill;book.description=skill.mechanicalDescription;book.villageQuote=skill.villageQuote;book.iconSprite=SaveIcon(book.Id,PixelArt.Icon("book",a*6+b,1+b%5));book.itemTier=1+b%5;book.rarity=(ItemRarity)(book.itemTier-1);EditorUtility.SetDirty(book);count++;
        }
        for(int i=0;i<6;i++)
        {
            var item=AssetDatabase.LoadAssetAtPath<ItemData>($"Assets/_Project/ScriptableObjects/Items/Gear{i}.asset");item.Family=new[]{"sword","wand","chestpiece","pauldrons","amulet","ring"}[i];item.iconSprite=SaveIcon(item.Id,PixelArt.Icon(item.Family,i));item.description=i<2?$"Adds {item.FlatDamage} attack per fusion tier.":$"Adds {item.FlatHealth} health per fusion tier.";item.villageQuote="\"Even the longest road begins with honest steel.\" - Huldra";EditorUtility.SetDirty(item);
        }
        foreach(var guid in AssetDatabase.FindAssets("t:SpellData",new[]{"Assets/_Project/ScriptableObjects/Spells"}))
        {
            var skill=AssetDatabase.LoadAssetAtPath<SpellData>(AssetDatabase.GUIDToAssetPath(guid));int variant=Array.IndexOf(new[]{"ember","bramble","mend","frost","lantern","resolve"},skill.Id);skill.skillIcon=SaveIcon(skill.Id,PixelArt.Icon("skill",Mathf.Max(0,variant)));skill.mechanicalDescription=$"{skill.Effect}: {skill.Power} power, {skill.Cooldown}s cooldown.";skill.villageQuote="\"A lantern is only as bright as the hand that lifts it.\" - Vane";EditorUtility.SetDirty(skill);
        }
        ProceduralPixelSpriteGenerator.GenerateRaceSheetsCLI();ExportServerCatalogueCLI();
        AssetDatabase.SaveAssets();Debug.Log($"OVERHAUL_ASSETS_OK: {count} catalogue entries, 30 skill assets, pixel font and icons.");
    }
    [Serializable] private sealed class CatalogueRecord { public string id,name,skill;public int kind,slot,tier; }
    [Serializable] private sealed class CatalogueFile { public CatalogueRecord[] items; }
    public static void ExportServerCatalogueCLI()
    {
        var records=new System.Collections.Generic.List<CatalogueRecord>();
        foreach(var guid in AssetDatabase.FindAssets("t:ItemData",new[]{"Assets/_Project"}))
        {
            var item=AssetDatabase.LoadAssetAtPath<ItemData>(AssetDatabase.GUIDToAssetPath(guid));records.Add(new CatalogueRecord{id=item.Id,name=item.DisplayName,kind=(int)item.Kind,slot=(int)item.Slot,tier=Mathf.Max(1,item.itemTier),skill=item.Spell!=null?item.Spell.Id:""});
        }
        Directory.CreateDirectory("Server");File.WriteAllText("Server/catalog.json",JsonUtility.ToJson(new CatalogueFile{items=records.ToArray()},true));
        Debug.Log("SERVER_CATALOGUE_OK: "+records.Count+" entries.");
    }
    private static Sprite SaveIcon(string id,Sprite sprite)
    {
        string folder="Assets/_Project/Art/Icons";Directory.CreateDirectory(folder);string path=folder+"/"+id+".png";File.WriteAllBytes(path,sprite.texture.EncodeToPNG());AssetDatabase.ImportAsset(path);
        var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.spritePixelsPerUnit=32;importer.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
}
