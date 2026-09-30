using UnityEngine;

// Shared source atlases, runtime slices and materials. Original PNGs remain untouched.
public static class IllustratedArt
{
    private static readonly Sprite[] equipment=new Sprite[16],heroes=new Sprite[9];
    private static Texture2D equipmentTexture,heroTexture,expansionTexture;
    private static readonly Sprite[] expansion=new Sprite[16];
    public static Sprite Expansion(int index){if(expansionTexture==null)expansionTexture=Resources.Load<Texture2D>("Illustrated/Expansion");return expansion[index]!=null?expansion[index]:expansion[index]=Slice(expansionTexture,index,4,1);}
    private static Material uiMaterial,heroMaterial,worldMaterial;
    public static Material UI => uiMaterial!=null?uiMaterial:uiMaterial=new Material(Shader.Find("EchoesOfTheRift/IllustratedUI"));
    public static Material World => worldMaterial!=null?worldMaterial:worldMaterial=new Material(Shader.Find("EchoesOfTheRift/IllustratedHero"));
    public static Material HeroMaterial => heroMaterial!=null?heroMaterial:heroMaterial=new Material(Shader.Find("EchoesOfTheRift/IllustratedHero"));
    private static Sprite Slice(Texture2D texture,int index,int columns,float worldSize)
    {
        if(texture==null)return null;
        float cellX=texture.width/(float)columns,cellY=texture.height/(float)columns;
        return Sprite.Create(texture,new Rect(index%columns*cellX,texture.height-(index/columns+1)*cellY,cellX,cellY),Vector2.one*.5f,cellY/worldSize,0,SpriteMeshType.FullRect);
    }
    public static Sprite Hero(int race)
    {
        race=Mathf.Clamp(race,0,8);if(heroTexture==null)heroTexture=Resources.Load<Texture2D>("Illustrated/Heroes");
        return heroes[race]!=null?heroes[race]:heroes[race]=Slice(heroTexture,race,3,2);
    }
    public static Sprite Equipment(int index)
    {
        index=Mathf.Clamp(index,0,15);if(equipmentTexture==null)equipmentTexture=Resources.Load<Texture2D>("Illustrated/Equipment");
        return equipment[index]!=null?equipment[index]:equipment[index]=Slice(equipmentTexture,index,4,1);
    }
    public static Sprite Weapon(string family)
    {
        switch(family)
        {
            case "sword":case "dagger":return Equipment(0);
            case "greatsword":return Equipment(1);
            case "bow":return Equipment(2);
            case "crossbow":return Expansion(3);
            case "spear":return Expansion(0);case "boomerang":return Expansion(1);case "pistol":return Expansion(2);
            case "boots":return Expansion(4);case "leggings":return Expansion(5);case "pauldrons":return Expansion(6);
            case "scythe":return Equipment(3);
            case "staff":case "wand":return Equipment(4);
            case "hammer":return Equipment(5);
            case "helmet":return Equipment(6);
            case "chestpiece":return Equipment(7);
            case "amulet":return Equipment(8);
            case "ring":return Equipment(9);
            case "shield":return Expansion(7);
            default:return null;
        }
    }
    public static Sprite Skill(SpellData spell)
    {
        if(spell==null)return null;
        if(spell.Effect>=SpellEffect.FanShot)return Expansion(12+(int)spell.Effect-(int)SpellEffect.FanShot);
        if(spell.Effect==SpellEffect.Mend)return Equipment(12);
        if(spell.Effect==SpellEffect.SummonRune)return Equipment(13);
        if(spell.Effect==SpellEffect.StatBuff||spell.Effect==SpellEffect.StanceModifier)return Equipment(15);
        if(spell.Effect==SpellEffect.DashStrike)return Equipment(14);
        return Equipment(spell.Color.b>spell.Color.r?11:10);
    }
    public static Sprite Item(ItemData item)=>item.Kind==ItemKind.SkillBook?Book(item.Spell):Weapon(item.Family)??item.iconSprite;
    public static Sprite Book(SpellData spell)=>Expansion(spell==null?8:spell.Effect==SpellEffect.Mend||spell.Effect==SpellEffect.SummonRune?11:spell.Effect==SpellEffect.Whirlwind?9:spell.Color.b>spell.Color.r?10:9);
    public static bool Owns(Sprite sprite)=>sprite!=null&&(sprite.texture==equipmentTexture||sprite.texture==heroTexture||sprite.texture==expansionTexture);
}
