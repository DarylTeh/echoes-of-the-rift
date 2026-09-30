using System;
using System.Text;

public static partial class EnglishUI
{
    public const string MoonStance="MOON STANCE",EmberStance="EMBER STANCE";
    public static string Health(int current,float maximum)=>$"HP {current} / {maximum:0}";
    public static string Mana(float current)=>$"MP {current:0} / 100";
    public static string Objective(bool town,int stage)=>town?"RIFT HAVEN\nPrepare your equipment\nI - Inspect inventory":$"STAGE {stage} / 4\nDefeat the stage boss\nSmall foes are optional";
    public static string Price(int gold)=>$"Price: {gold} gold";
    public static string Compact(int value){float scale=value>=1000000000?1000000000f:value>=1000000?1000000f:value>=1000?1000f:1;string suffix=scale>=1000000000?"B":scale>=1000000?"M":scale>=1000?"K":"";return scale==1?value.ToString(): (System.Math.Floor(value/scale*10)/10).ToString("0.#",System.Globalization.CultureInfo.InvariantCulture)+suffix;}

    public static string Stack(int tier,int count)=>$"+{Math.Max(1,tier)}  x{Compact(count)}";
    public static string Stack(ItemData item,int level,int count)=>$"{ProgressionRules.StarRating(item!=null?item.rarity:ItemRarity.Common)}  {ProgressionRules.EnhancementBadge(level)}  x{Compact(count)}";
    public static string ItemHeader(string name,ItemRarity rarity,int tier,int owned)=>$"{name}\n{ProgressionRules.StarRating(rarity)}  {ProgressionRules.EnhancementBadge(tier)} / Owned {owned}\n\n";
    public static string RankRow(int rank,string tag,int stages)=>$"{rank}. WAYFARER {tag} / {stages} STAGES";
    public static string RankDetails(string tag,int gold,string race)=>$"WAYFARER {tag}\n{gold} GOLD\n{race}";
    public static string TownItem(ItemData item,InventoryState inventory)
    {
        var text=new StringBuilder($"{Price(15*item.itemTier*item.itemTier)}\nGold: {inventory.Coins} / Campaign: {inventory.CampaignStagesCompleted}/4\n\n{item.DisplayName}\n");
        for(int tier=1;tier<=5;tier++)text.AppendLine($"+{tier}: {inventory.Count(item.Id,tier)}");
        return text.ToString();
    }
}
