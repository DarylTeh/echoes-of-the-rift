using System;
using UnityEngine;
[Serializable] public sealed class RankingEntry { public string tag,profile; }
[Serializable] public sealed class RankingList { public RankingEntry[] entries; }
public sealed class LeaderboardModal : MonoBehaviour
{
    private RectTransform root;
    public void Show(string json,ArenaGame game)
    {
        if(root!=null)Destroy(root.gameObject);root=GameUI.Canvas("Leaderboard");root.GetComponent<Canvas>().sortingOrder=350;
        var panel=GameUI.Panel(root,"RankingFrame",Vector2.zero,new Vector2(1008,608));
        GameUI.Label(panel,EnglishScreens.WayfarerRankings,new Vector2(-170,246),new Vector2(600,48),32).color=GameUI.Gold;
        GameUI.Button(panel,EnglishScreens.Close,new Vector2(377,246),new Vector2(176,44),()=>Destroy(root.gameObject));
        var previewRect=GameUI.Rect("RankedHero",panel,Vector2.one*.5f,new Vector2(292,45),new Vector2(256,256));var preview=previewRect.gameObject.AddComponent<HeroPortrait>();preview.Configure(game.CharacterPrefab,game.Forge.State,game.Items);
        var details=GameUI.Label(panel,EnglishScreens.SelectAWayfarer,new Vector2(292,-160),new Vector2(320,120),22);
        var rankings=JsonUtility.FromJson<RankingList>(json);int index=0;
        foreach(var entry in rankings.entries??Array.Empty<RankingEntry>())
        {
            var state=JsonUtility.FromJson<InventoryState>(entry.profile);if(state==null||!state.IsValid())continue;
            string tag=entry.tag;int rank=++index;
            GameUI.Button(panel,EnglishUI.RankRow(rank,tag,state.CampaignStagesCompleted),new Vector2(-180,185-(rank-1)*42),new Vector2(528,38),()=>{preview.Apply(state);details.text=EnglishUI.RankDetails(tag,state.Coins,CharacterCustomizer.Races[Mathf.Clamp(state.Appearance.Race,0,8)]);});
        }
    }
    private void OnDestroy(){if(root!=null)Destroy(root.gameObject);}
}
