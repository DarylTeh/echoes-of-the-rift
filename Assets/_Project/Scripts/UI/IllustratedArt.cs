using UnityEngine;

// Shared source atlases, runtime slices and materials. Original PNGs remain untouched.
public static class IllustratedArt
{
    private static readonly Sprite[] equipment=new Sprite[16],heroes=new Sprite[9],heroSides=new Sprite[9],heroBacks=new Sprite[9],skillCubes=new Sprite[12],monsters=new Sprite[16],dungeonTiles=new Sprite[8],utilityIcons=new Sprite[16],combatEffects=new Sprite[16],bossAttacks=new Sprite[16];
    private static Texture2D equipmentTexture,heroTexture,heroSideTexture,heroBackTexture,expansionTexture,skillCubeTexture,monsterTexture,dungeonTilesTexture,utilityIconsTexture,combatEffectsTexture,bossAttacksTexture;
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
    // Direction order: down/front, right/side, up/back, left/side (flipped by the renderer).
    public static Sprite Hero(int race,int direction)
    {
        race=Mathf.Clamp(race,0,8);direction=Mathf.Clamp(direction,0,3);
        if(direction==0)return Hero(race);
        if(direction==1||direction==3)
        {
            if(heroSideTexture==null)heroSideTexture=Resources.Load<Texture2D>("Illustrated/HeroSide");
            return heroSides[race]!=null?heroSides[race]:heroSides[race]=Slice(heroSideTexture,race,3,2);
        }
        if(heroBackTexture==null)heroBackTexture=Resources.Load<Texture2D>("Illustrated/HeroBack");
        return heroBacks[race]!=null?heroBacks[race]:heroBacks[race]=Slice(heroBackTexture,race,3,2);
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
    public static Sprite Monster(int index)
    {
        index=Mathf.Clamp(index,0,15);
        if(monsterTexture==null)monsterTexture=Resources.Load<Texture2D>("Illustrated/Monsters");
        return monsters[index]!=null?monsters[index]:monsters[index]=Slice(monsterTexture,index,4,4,1);
    }
    public static Sprite FloorTile(bool wall,int variant)
    {
        int index=(wall?4:0)+Mathf.Clamp(variant,0,3);
        if(dungeonTilesTexture==null)dungeonTilesTexture=Resources.Load<Texture2D>("Illustrated/DungeonTiles");
        return dungeonTiles[index]!=null?dungeonTiles[index]:dungeonTiles[index]=Slice(dungeonTilesTexture,index,4,2,1);
    }
    public static Sprite UtilityIcon(string family)
    {
        int index;
        switch((family??string.Empty).ToLowerInvariant())
        {
            case "gold":case "coin":return Utility(0);
            case "gem":case "gems":case "diamond":return Utility(1);
            case "settings":case "gear":return Utility(2);
            case "sword":case "weapon":return Utility(3);
            case "bag":case "backpack":case "inventory":return Utility(4);
            case "quest":case "daily":return Utility(5);
            case "swap":case "switch":return Utility(6);
            case "dodge":case "dash":return Utility(7);
            case "book":case "spellbook":case "skillbook":return Utility(8);
            case "shield":case "defense":return Utility(9);
            case "potion":case "health":return Utility(10);
            case "chest":case "loot":return Utility(11);
            case "heart":case "life":return Utility(12);
            case "map":case "compass":return Utility(13);
            case "mail":case "inbox":return Utility(14);
            case "skill":case "rune":case "magic":return Utility(15);
            default:index=-1;break;
        }
        return index<0?null:Utility(index);
    }
    private static Sprite Utility(int index)
    {
        if(utilityIconsTexture==null)utilityIconsTexture=Resources.Load<Texture2D>("Illustrated/UtilityIcons");
        return utilityIcons[index]!=null?utilityIcons[index]:utilityIcons[index]=Slice(utilityIconsTexture,index,4,4,1);
    }
    public static Sprite CombatEffect(Color color,bool slash)
    {
        if(combatEffectsTexture==null)combatEffectsTexture=Resources.Load<Texture2D>("Illustrated/CombatEffects");
        Color.RGBToHSV(color,out float hue,out float saturation,out float value);
        int index;
        if(slash)index=hue<.08f||hue>.94f?0:hue<.48f?2:hue<.68f?1:3;
        else if(hue<.06f||hue>.96f)index=4;
        else if(hue<.16f)index=8;
        else if(hue<.42f)index=7;
        else if(hue<.56f)index=5;
        else if(hue<.70f)index=6;
        else if(hue<.90f)index=11;
        else index=14;
        if(saturation<.18f)index=10;
        return combatEffects[index]!=null?combatEffects[index]:combatEffects[index]=Slice(combatEffectsTexture,index,4,4,2);
    }
    public static Sprite BossAttack(int boss,int phase)
    {
        int index=Mathf.Clamp(boss,0,3)*4+Mathf.Clamp(phase,0,3);
        if(bossAttacksTexture==null)bossAttacksTexture=Resources.Load<Texture2D>("Illustrated/BossAttacks");
        return bossAttacks[index]!=null?bossAttacks[index]:bossAttacks[index]=Slice(bossAttacksTexture,index,4,4,2);
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
    public static bool Owns(Sprite sprite)=>sprite!=null&&(sprite.texture==equipmentTexture||sprite.texture==heroTexture||sprite.texture==heroSideTexture||sprite.texture==heroBackTexture||sprite.texture==expansionTexture||sprite.texture==skillCubeTexture||sprite.texture==monsterTexture||sprite.texture==dungeonTilesTexture||sprite.texture==utilityIconsTexture||sprite.texture==combatEffectsTexture||sprite.texture==bossAttacksTexture);
}
