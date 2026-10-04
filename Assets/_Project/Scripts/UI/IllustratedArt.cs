using UnityEngine;

// Shared source atlases, runtime slices and materials. Original PNGs remain untouched.
public static class IllustratedArt
{
    private static readonly Sprite[] equipment=new Sprite[16],heroes=new Sprite[9],skillCubes=new Sprite[12];
    private static Texture2D equipmentTexture,heroTexture,expansionTexture,skillCubeTexture;
    private static readonly Sprite[] expansion=new Sprite[16];
    public static Sprite Expansion(int index){if(expansionTexture==null)expansionTexture=Resources.Load<Texture2D>("Illustrated/Expansion");return expansion[index]!=null?expansion[index]:expansion[index]=Slice(expansionTexture,index,4,1);}
    private static Material uiMaterial,heroMaterial,worldMaterial;
    public static Material UI => uiMaterial!=null?uiMaterial:uiMaterial=new Material(Shader.Find("EchoesOfTheRift/IllustratedUI"));
    public static Material World => worldMaterial!=null?worldMaterial:worldMaterial=new Material(Shader.Find("EchoesOfTheRift/IllustratedHero"));
    public static Material HeroMaterial => heroMaterial!=null?heroMaterial:heroMaterial=new Material(Shader.Find("EchoesOfTheRift/IllustratedHero"));
    private static Sprite Slice(Texture2D texture,int index,int columns,int rows,float worldSize)
    {
        if(texture==null)return null;
        texture.filterMode=FilterMode.Point;texture.wrapMode=TextureWrapMode.Clamp;texture.anisoLevel=0;
        float cellX=texture.width/(float)columns,cellY=texture.height/(float)rows;
        return Sprite.Create(texture,new Rect(index%columns*cellX,texture.height-(index/columns+1)*cellY,cellX,cellY),Vector2.one*.5f,cellY/worldSize,0,SpriteMeshType.FullRect);
    }
    private static Sprite Slice(Texture2D texture,int index,int columns,float worldSize)=>Slice(texture,index,columns,columns,worldSize);
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
    private static Sprite SkillCube(int index)
    {
        index=Mathf.Clamp(index,0,11);
        if(skillCubeTexture==null)skillCubeTexture=Resources.Load<Texture2D>("Illustrated/SkillCubes");
        return skillCubes[index]!=null?skillCubes[index]:skillCubes[index]=Slice(skillCubeTexture,index,4,3,1);
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
        string id=spell.Id??string.Empty;
        if(id.Contains("flame")||id.Contains("fire")||id.Contains("ember"))return SkillCube(0);
        if(id.Contains("slash")||id.Contains("dash")||id.Contains("blade"))return SkillCube(1);
        if(id.Contains("heal")||id.Contains("mend")||id.Contains("leaf"))return SkillCube(2);
        if(id.Contains("star")||id.Contains("rune")||id.Contains("comet"))return SkillCube(3);
        if(id.Contains("frost")||id.Contains("ice"))return SkillCube(4);
        if(id.Contains("ward")||id.Contains("shield"))return SkillCube(11);
        if(id.Contains("buff")||id.Contains("sun")||id.Contains("oath"))return SkillCube(5);
        if(id.Contains("thorn")||id.Contains("root")||id.Contains("poison"))return SkillCube(6);
        if(id.Contains("arcane")||id.Contains("portal"))return SkillCube(7);
        if(id.Contains("meteor")||id.Contains("burst"))return SkillCube(8);
        if(id.Contains("lightning")||id.Contains("spark"))return SkillCube(9);
        if(id.Contains("wind")||id.Contains("whirl")||id.Contains("tempest"))return SkillCube(10);
        switch(spell.Effect)
        {
            case SpellEffect.Burst:return SkillCube(8);
            case SpellEffect.DashStrike:return SkillCube(1);
            case SpellEffect.Mend:return SkillCube(2);
            case SpellEffect.SummonRune:return SkillCube(7);
            case SpellEffect.StatBuff:case SpellEffect.StanceModifier:return SkillCube(5);
            case SpellEffect.FanShot:case SpellEffect.Whirlwind:return SkillCube(10);
            case SpellEffect.PiercingLance:return SkillCube(4);
            case SpellEffect.OrbitHammers:return SkillCube(9);
            default:return spell.Color.r>spell.Color.g?SkillCube(0):spell.Color.g>spell.Color.b?SkillCube(6):SkillCube(4);
        }
    }
    public static Sprite Item(ItemData item)=>item.Kind==ItemKind.SkillBook?Book(item.Spell):Weapon(item.Family)??item.iconSprite;
    public static Sprite Book(SpellData spell)=>Skill(spell);
    public static bool Owns(Sprite sprite)=>sprite!=null&&(sprite.texture==equipmentTexture||sprite.texture==heroTexture||sprite.texture==expansionTexture||sprite.texture==skillCubeTexture);
}
