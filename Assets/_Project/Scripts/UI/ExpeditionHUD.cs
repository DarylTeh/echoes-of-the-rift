using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Presentation reads local simulation or received server snapshots; it never grants rewards.
public sealed class ExpeditionHUD : MonoBehaviour
{
    private ArenaGame game;
    private RectTransform bossPanel,mapPanel,mapRoom,partyPanel;
    private TMP_Text bossName,bossValue,mapTitle,partyState;
    private UnityEngine.UI.Image bossFill,partyFill,partyPortrait,playerDot,bossDot,allyDot;
    private readonly List<Combatant> allies=new List<Combatant>(2);
    private float nextRefresh;
    public bool BossVisible=>bossPanel!=null&&bossPanel.gameObject.activeSelf;
    public bool PartyVisible=>partyPanel!=null&&partyPanel.gameObject.activeSelf;
    public float BossFraction=>bossFill==null?0:bossFill.fillAmount;
    public static Vector2 Project(Vector2 world,Vector2 room,Vector2 size)=>new Vector2(Mathf.Clamp(world.x/Mathf.Max(1,room.x),-.5f,.5f)*size.x,Mathf.Clamp(world.y/Mathf.Max(1,room.y),-.5f,.5f)*size.y);
    public void Initialize(ArenaGame source,RectTransform root)
    {
        game=source;
        bossPanel=GameUI.Panel(root,"BossStatus",new Vector2(35,316),new Vector2(350,48));
        bossName=GameUI.Label(bossPanel,"",new Vector2(0,10),new Vector2(322,24),18);bossName.alignment=TextAlignmentOptions.Center;
        bossFill=Fill(bossPanel,new Vector2(0,-10),new Vector2(318,12),new Color32(186,87,239,255));
        bossValue=GameUI.Label(bossPanel,"",new Vector2(0,-10),new Vector2(310,20),14);bossValue.alignment=TextAlignmentOptions.Center;
        mapPanel=GameUI.Panel(root,"DungeonMinimap",new Vector2(531,110),new Vector2(152,160));
        mapTitle=GameUI.Label(mapPanel,"",new Vector2(0,57),new Vector2(132,24),18);mapTitle.alignment=TextAlignmentOptions.Center;
        mapRoom=GameUI.Panel(mapPanel,"RoomOutline",new Vector2(0,-3),new Vector2(124,92));
        bossDot=Dot(mapRoom,new Color32(245,96,190,255),12);allyDot=Dot(mapRoom,GameUI.Gold,9);playerDot=Dot(mapRoom,Color.cyan,7);
        string[] names={"YOU","ALLY","BOSS"};Color[] colors={Color.cyan,GameUI.Gold,new Color32(245,96,190,255)};
        for(int i=0;i<3;i++){var label=GameUI.Label(mapPanel,names[i],new Vector2((i-1)*46,-62),new Vector2(44,22),12);label.alignment=TextAlignmentOptions.Center;label.color=colors[i];}
        partyPanel=GameUI.Panel(root,"PartyAlly",new Vector2(-478,34),new Vector2(264,72));
        partyPortrait=GameUI.Icon(partyPanel,null,new Vector2(-97,0),new Vector2(58,58));
        partyState=GameUI.Label(partyPanel,"",new Vector2(30,13),new Vector2(178,28),18);
        partyFill=Fill(partyPanel,new Vector2(30,-13),new Vector2(172,10),GameUI.Gold);
        Refresh();
    }
    private static UnityEngine.UI.Image Fill(Transform parent,Vector2 at,Vector2 size,Color tint)
    {
        var background=GameUI.Icon(parent,CombatVisual.Square,at,size);background.preserveAspect=false;background.color=new Color32(17,15,26,255);
        var fill=GameUI.Icon(parent,CombatVisual.Square,at,size);fill.preserveAspect=false;fill.type=UnityEngine.UI.Image.Type.Filled;fill.fillMethod=UnityEngine.UI.Image.FillMethod.Horizontal;fill.fillOrigin=0;fill.color=tint;return fill;
    }
    private static UnityEngine.UI.Image Dot(Transform parent,Color tint,float size){var dot=GameUI.Icon(parent,CombatVisual.Square,Vector2.zero,Vector2.one*size);dot.color=tint;return dot;}
    private void Update(){if(game!=null&&Time.unscaledTime>=nextRefresh){nextRefresh=Time.unscaledTime+.05f;Refresh();}}
    public void Refresh()
    {
        if(game==null||game.Player==null||game.Hub==null)return;
        bool expedition=!game.Hub.IsOpen&&game.Dungeon.StageCount>0;
        mapPanel.gameObject.SetActive(expedition);
        var world=game.Session.World;
        var boss=game.Session.Active&&!game.Session.IsHost?world.ReplicaBoss:game.Dungeon.StageBoss;
        bool liveBoss=expedition&&!game.Dungeon.IsCleared&&boss!=null&&boss.gameObject.activeInHierarchy&&boss.Alive;
        bossPanel.gameObject.SetActive(liveBoss);
        if(liveBoss)
        {
            bossName.text=BossRoster.Names[Mathf.Clamp(game.Dungeon.StageIndex,0,BossRoster.Names.Length-1)];
            bossFill.fillAmount=Mathf.Clamp01(boss.Health/Mathf.Max(1,boss.MaximumHealth));
            bossValue.text=$"{Mathf.CeilToInt(boss.Health)} / {Mathf.CeilToInt(boss.MaximumHealth)}";
        }
        allies.Clear();if(game.Session.Active&&world!=null&&world.ClientReady)world.CopyHudAllies(allies);
        Combatant ally=allies.Count>0?allies[0]:null;
        partyPanel.gameObject.SetActive(expedition&&ally!=null);
        if(ally!=null)
        {
            var appearance=ally.GetComponent<CharacterCustomizer>();partyPortrait.sprite=IllustratedArt.Hero(appearance!=null?appearance.Appearance.Race:0);partyPortrait.material=IllustratedArt.UI;
            partyState.text=ally.Alive?"ALLY":"ALLY DOWN";partyState.color=ally.Alive?GameUI.Cream:new Color32(255,130,155,255);
            partyFill.fillAmount=Mathf.Clamp01(ally.Health/Mathf.Max(1,ally.MaximumHealth));
        }
        if(!expedition)return;
        var stage=game.Dungeon.CurrentStage;Vector2 room=new Vector2(stage.width,stage.height);
        Vector2 mapSize=room*Mathf.Min(124/room.x,92/room.y);mapRoom.sizeDelta=mapSize;
        mapTitle.text=$"ROOM {game.Dungeon.StageIndex+1}/{game.Dungeon.StageCount}";
        playerDot.rectTransform.anchoredPosition=Project(game.Player.transform.position,room,mapSize);
        bossDot.gameObject.SetActive(liveBoss);if(liveBoss)bossDot.rectTransform.anchoredPosition=Project(boss.transform.position,room,mapSize);
        allyDot.gameObject.SetActive(ally!=null);if(ally!=null)allyDot.rectTransform.anchoredPosition=Project(ally.transform.position,room,mapSize);
    }
}
