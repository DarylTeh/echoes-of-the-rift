using System;

// Server-mirrorable progression math. UI may display these values, but rewards and
// spending must eventually be evaluated by the authoritative service.
public static class ProgressionRules
{
    public const int GearStarterLevel=5;
    public const int NewPlayerCarryChapters=5;
    public const int GearDuplicateBand=100;
    public const int SkillLevelBand=10;

    // The cost applies to the level being left: 1->2 through 99->100 cost five;
    // 100->101 through 199->200 cost ten, matching the requested bands.
    public static int GearDuplicatesForNext(int currentLevel)
    {
        currentLevel=Math.Max(1,currentLevel);
        return checked(5*(1+currentLevel/GearDuplicateBand));
    }

    // Gold remains common, but its quadratic decade term creates a late-game sink
    // without making the first upgrades feel punitive.
    public static int GearGoldForNext(int currentLevel,int baseGold=50)
    {
        currentLevel=Math.Max(1,currentLevel);int decade=currentLevel/10;
        return checked(Math.Max(0,baseGold)+8*currentLevel+2*decade*decade);
    }

    // Rarity changes the ceiling slightly; enhancement power has a soft cap so
    // campaign encounters remain threatening after very long investment.
    public static float GearPowerMultiplier(int level,ItemRarity rarity)
    {
        level=Math.Max(0,level);
        double levelFactor=1.8*(1.0-Math.Exp(-level/140.0));
        return (float)((1.0+levelFactor)*(1.0+0.10*(int)rarity));
    }

    public static int SkillCopiesForNext(int currentLevel)
    {
        currentLevel=Math.Max(1,currentLevel);
        return 1+((currentLevel-1)/SkillLevelBand);
    }

    public static int SkillGemsForNext(int currentLevel)
    {
        currentLevel=Math.Max(1,currentLevel);int band=(currentLevel-1)/SkillLevelBand;
        return checked(5+2*band+5*(currentLevel/25));
    }

    public static float SkillPowerMultiplier(int level)
    {
        level=Math.Max(0,level);
        return (float)(1.0+1.5*(1.0-Math.Exp(-level/40.0)));
    }

    // Campaign pressure grows faster than the post-200 personal-power curve.
    public static int ChapterTargetPower(int chapter)
    {
        chapter=Math.Max(1,chapter);
        return Math.Max(1,(int)Math.Round(120*Math.Pow(1.12,chapter-1)));
    }

    public static bool HasWelcomeCarry(int chapter)=>chapter<=NewPlayerCarryChapters;

    public static string StarRating(ItemRarity rarity)
    {
        int filled=Math.Max(1,Math.Min(5,(int)rarity+1));
        // Tiny5 is intentionally kept ASCII-safe on mobile and low-end PC builds.
        // Asterisks preserve the filled/empty star rhythm without missing-glyph boxes.
        return new string('*',filled)+new string('.',5-filled);
    }

    public static string EnhancementBadge(int level)=>"+"+Math.Max(1,level);
}
