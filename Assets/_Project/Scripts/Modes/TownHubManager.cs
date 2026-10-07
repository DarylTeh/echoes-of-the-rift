using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(250)]
public sealed partial class TownHubManager : MonoBehaviour
{
    public ItemTierUpManager Forge;
    public CoopSession Session;
    public event Action ExpeditionRequested;
    public bool IsOpen=>root!=null;
    private RectTransform root,dialog,mapMarker;
    private GameObject scenery;
    private TMP_Text hint;
    private TMP_Text contextActionLabel;
    private UnityEngine.UI.Button contextAction;
    private int nearest=-1;
    private readonly TMP_Text[] merchantLabels=new TMP_Text[5];
    private static readonly string[] MerchantIcons={"hammer","bow","staff","amulet","book"};
    public static readonly string[] Names={"Hephaestus","Artemis","Helios","Asclepius","Athena","North / Raids","South / Campaign","East / DPS trial","West / World boss"};
    // Specialists sit beside the visible left/right counters in the new guild hall;
    // the campaign, raid, trial and world-boss exits frame the room edges.
    public static readonly Vector2[] Positions={new Vector2(-10,0),new Vector2(-10,2.5f),new Vector2(10,0),new Vector2(10,2.5f),new Vector2(0,5.4f),new Vector2(0,7.5f),new Vector2(0,-7.5f),new Vector2(14,0),new Vector2(-14,0)};
    private readonly System.Collections.Generic.List<RectTransform> signs=new System.Collections.Generic.List<RectTransform>();
    public void Open()
    {
        if(root!=null)return;
        scenery=TownScenery.Build(Session.Game.CharacterPrefab,Positions);
        root=GameUI.Canvas("TownHub");
        // The hall's banner is the visual title; keep instructions out of its
        // central focal area and clear of the campaign gate at the bottom.
        hint=GameUI.Label(root,"WASD / left stick: move",new Vector2(-438,-324),new Vector2(276,24),15);hint.alignment=TextAlignmentOptions.Center;
        for(int i=0;i<Names.Length;i++)
        {
            int index=i;var button=GameUI.Button(root,i<5?"":Names[i],Vector2.zero,i<5?new Vector2(38,38):new Vector2(174,32),()=>Interact(index));
            button.name="TownService"+i;
            button.GetComponentInChildren<TMP_Text>().fontSize=16;
            button.GetComponent<UnityEngine.UI.Image>().color=new Color(1,1,1,.9f);
            if(i<5){
                GameUI.Icon(button.transform,PixelArt.Icon(MerchantIcons[i]),Vector2.zero,new Vector2(30,30));
                merchantLabels[i]=GameUI.Label(button.transform,Names[i],new Vector2(0,31),new Vector2(134,24),16);merchantLabels[i].alignment=TextAlignmentOptions.Center;merchantLabels[i].gameObject.SetActive(false);
            }
            signs.Add(button.GetComponent<RectTransform>());
        }
        var interact=GameUI.Button(root,"",new Vector2(482,-286),new Vector2(236,54),()=>Interact(nearest));interact.name="TownContextAction";GameUI.Pin((RectTransform)interact.transform,new Vector2(1,0),new Vector2(-146,96));
        contextAction=interact.GetComponent<UnityEngine.UI.Button>();contextActionLabel=interact.GetComponentInChildren<TMP_Text>();interact.gameObject.SetActive(false);
        var settings=GameUI.Button(root,"",new Vector2(592,321),new Vector2(52,52),OpenSettings);settings.name="SettingsButton";GameUI.Pin((RectTransform)settings.transform,Vector2.one,new Vector2(-48,-39));
        GameUI.Icon(settings.transform,PixelArt.Icon("settings"),Vector2.zero,new Vector2(36,36));
        var rail=GameUI.Panel(root,"TownActivityRail",Vector2.zero,new Vector2(84,292));GameUI.Pin(rail,new Vector2(0,.5f),new Vector2(56,0));
        rail.GetComponent<UnityEngine.UI.Image>().color=new Color32(35,32,49,205);rail.GetComponent<UnityEngine.UI.Image>().raycastTarget=false;
        var railHeading=GameUI.Label(rail,"MENU",new Vector2(0,120),new Vector2(76,18),11);railHeading.alignment=TextAlignmentOptions.Center;railHeading.color=GameUI.Gold;
        var campaign=AddRailAction(rail,"CampaignButton","Campaign","sword",new Vector2(0,68),()=>{if(!Session.UsesDedicated||Session.Authenticated)ExpeditionRequested?.Invoke();});
        campaign.GetComponent<UnityEngine.UI.Image>().color=new Color32(112,169,201,255);
        AddRailAction(rail,"EventsButton","Events","quest",new Vector2(0,0),()=>GetComponent<EventDrawerUI>()?.Open());
        AddRailAction(rail,"InboxButton","Inbox","book",new Vector2(0,-68),()=>GetComponent<InboxDrawerUI>()?.Open());
        var map=GameUI.Panel(root,"HavenMinimap",new Vector2(570,143),new Vector2(84,84));GameUI.Pin(map,Vector2.one,new Vector2(-62,-225));
        foreach(var point in new[]{new Vector2(0,26),new Vector2(0,-26),new Vector2(26,0),new Vector2(-26,0)})GameUI.Icon(map,CombatVisual.Square,point,new Vector2(4,4)).color=new Color32(194,158,232,255);
        mapMarker=GameUI.Icon(map,CombatVisual.Square,Vector2.zero,new Vector2(4,4)).rectTransform;mapMarker.GetComponent<UnityEngine.UI.Image>().color=new Color32(122,239,172,255);
        Refresh();
    }
    private void LateUpdate()
    {
        if(root==null||!root.gameObject.activeInHierarchy||Session.Game.Player==null)return;
        mapMarker.anchoredPosition=new Vector2(Session.Game.Player.transform.position.x*1.65f,Session.Game.Player.transform.position.y*3.2f);
        nearest=-1;float distance=1.65f;
        for(int i=0;i<Positions.Length;i++)
        {
            float d=Vector2.Distance(Session.Game.Player.transform.position,Positions[i]);if(i<5)merchantLabels[i].gameObject.SetActive(d<2);if(d<distance){nearest=i;distance=d;}
            Vector3 screen=Camera.main.WorldToScreenPoint((Vector3)Positions[i]+Vector3.up*(i<5?1.3f:i==5?0:.65f));
            Rect safe=UISafeArea.TestArea??Screen.safeArea;
            bool visible=screen.x>safe.xMin&&screen.x<safe.xMax&&screen.y>safe.yMin&&screen.y<safe.yMax;signs[i].gameObject.SetActive(visible);if(!visible)continue;
            Vector2 half=signs[i].sizeDelta*root.GetComponent<Canvas>().scaleFactor*.5f+Vector2.one*8;
            screen.x=Mathf.Clamp(screen.x,safe.xMin+half.x,safe.xMax-half.x);
            screen.y=Mathf.Clamp(screen.y,safe.yMin+half.y,safe.yMax-half.y);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(root,screen,null,out var local);
            local.y=Mathf.Min(local.y,i<5?300:242);
            // Keep gate labels clear of the fixed left activity rail and right bag/minimap.
            if(i>=5)local.x=Mathf.Clamp(local.x,-405,405);
            signs[i].anchoredPosition=local;
        }
        if(!HasDialog){
            bool canInteract=nearest>=0;
            if(contextAction.gameObject.activeSelf!=canInteract)contextAction.gameObject.SetActive(canInteract);
            if(canInteract){contextActionLabel.text=nearest==6?"Enter Campaign":"Talk to "+Names[nearest];}
            hint.text=canInteract?"F / Tap to interact":"WASD / left stick: move";
            if(Keyboard.current?.fKey.wasPressedThisFrame==true)Interact(nearest);
        }

    }
    private static UnityEngine.UI.Button AddRailAction(Transform rail,string name,string label,string icon,Vector2 position,Action action)
    {
        var button=GameUI.Button(rail,"",position,new Vector2(70,64),action);button.name=name;
        GameUI.Icon(button.transform,PixelArt.Icon(icon),new Vector2(0,10),new Vector2(26,26));
        var text=GameUI.Label(button.transform,label,new Vector2(0,-20),new Vector2(66,22),11);text.alignment=TextAlignmentOptions.Center;
        return button;
    }
    public void Interact(int index)
    {
        if(root==null||!root.gameObject.activeInHierarchy||HasDialog||index<0||index>=Positions.Length)return;
        if(Vector2.Distance(Session.Game.Player.transform.position,Positions[index])>1.65f){hint.text="Walk closer to "+Names[index]+".";return;}
        if(Session.UsesDedicated&&!Session.Authenticated){hint.text="Waiting for server connection.";return;}
        if(index<5){GetComponent<InventoryModal>().OpenMerchant(Names[index]);return;}
        if(index==6){ExpeditionRequested?.Invoke();return;}
        dialog=GameUI.Panel(root,"GateDialog",Vector2.zero,new Vector2(620,index==5?330:260));
        string message=index==5?"4-player and 8-player raids are in development.\nThe current 2-player co-op test is available below.":index==7?"Boss DPS trial is in development.":"Global world boss is in development.";
        GameUI.Label(dialog,Names[index],new Vector2(0,index==5?124:90),new Vector2(550,40),28).color=GameUI.Gold;
        GameUI.Label(dialog,message,new Vector2(0,index==5?66:24),new Vector2(550,70),index==5?18:22);
        if(index==5)
        {
            GameUI.Button(dialog,"Rift Defense",new Vector2(0,-14),new Vector2(280,46),()=>{CloseDialog();GetComponent<RiftDefenseMode>()?.Open();}).name="RiftDefenseLaunch";
            if(Session.UsesDedicated)GameUI.Button(dialog,"2-player co-op test",new Vector2(0,-74),new Vector2(340,42),()=>{CloseDialog();Session.World.CommandServerRpc("coop");}).name="CoopTestLaunch";
            GameUI.Button(dialog,"Close",new Vector2(0,-132),new Vector2(180,38),CloseDialog);
        }
        else GameUI.Button(dialog,"Close",new Vector2(170,-82),new Vector2(180,42),CloseDialog);
        Session.Game.Player.GetComponent<PlayerController>().ControlsEnabled=false;
    }
    public bool HasDialog=>dialog!=null||settingsRoot!=null||GetComponent<EventDrawerUI>()?.IsOpen==true||GetComponent<InboxDrawerUI>()?.IsOpen==true;
    public void CloseDialog(){if(settingsRoot!=null){if(confirmingSignOut)ShowSettingsTab(true);else CloseSettings();return;}var events=GetComponent<EventDrawerUI>();if(events!=null&&events.IsOpen){events.Close();return;}var inbox=GetComponent<InboxDrawerUI>();if(inbox!=null&&inbox.IsOpen){inbox.Close();return;}if(dialog!=null)Destroy(dialog.gameObject);dialog=null;if(Session.Game.Player!=null)Session.Game.Player.GetComponent<PlayerController>().ControlsEnabled=!Session.UsesDedicated||Session.Authenticated;}
    public static bool Sells(string merchant,ItemData item)
    {
        switch(merchant)
        {
            case "Athena":return item.Kind==ItemKind.SkillBook;
            case "Hephaestus":return item.Kind==ItemKind.Gear&&Array.IndexOf(new[]{"spear","sword","dagger","greatsword","hammer","shield","helmet","chestpiece","pauldrons"},item.Family)>=0;
            case "Artemis":return item.Kind==ItemKind.Gear&&Array.IndexOf(new[]{"pistol","boomerang","bow","crossbow","boots","leggings"},item.Family)>=0;
            case "Helios":return item.Kind==ItemKind.Gear&&(item.Family=="staff"||item.Family=="wand");
            case "Asclepius":return item.Kind==ItemKind.Gear&&(item.Family=="amulet"||item.Family=="ring");
            default:return true;
        }
    }
    public void Refresh(){GetComponent<SkillWheelHUD>()?.RefreshWallet();}
    public void Close(){CloseSettings();GetComponent<EventDrawerUI>()?.Close();GetComponent<InboxDrawerUI>()?.Close();if(root!=null){root.gameObject.SetActive(false);Destroy(root.gameObject);}root=null;dialog=null;contextAction=null;contextActionLabel=null;signs.Clear();if(scenery!=null){scenery.SetActive(false);Destroy(scenery);}scenery=null;}
    private void OnDestroy()=>Close();
}
