using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

// Read-only presentation for the server-authoritative live-event calendar.
// This drawer never claims rewards or decides eligibility on the client.
public sealed class EventDrawerUI : MonoBehaviour
{
    public CoopSession Session;
    public bool IsOpen=>root!=null;
    private RectTransform root,panel,viewport,list,detail;
    private UnityEngine.UI.Button closeButton,detailLaunchButton;
    private TMP_Text status,detailCountdown,detailEligibility,detailProgress;
    private ServerEventSnapshot detailEvent;
    private ServerEventFeedResponse feed;
    private readonly Dictionary<string,TMP_Text> countdowns=new Dictionary<string,TMP_Text>();
    private static Dictionary<string,ItemData> eventItems;
    private DateTimeOffset serverNow;
    private double receivedAt;
    private bool loading;

    public void Open()
    {
        if(root!=null||Session==null||Session.Game==null||Session.Game.Hub==null)return;
        root=GameUI.Canvas("EventDrawer");root.GetComponent<Canvas>().sortingOrder=360;root.gameObject.AddComponent<UIMenuFocus>();
        var blocker=GameUI.Rect("EventDrawerBlocker",root,Vector2.one*.5f,Vector2.zero,Vector2.zero);blocker.anchorMin=Vector2.zero;blocker.anchorMax=Vector2.one;blocker.sizeDelta=Vector2.zero;blocker.gameObject.AddComponent<UnityEngine.UI.Image>().color=new Color(0,0,0,.76f);
        panel=GameUI.Panel(root,"EventPanel",Vector2.zero,new Vector2(760,520));
        GameUI.Icon(panel,PixelArt.Icon("quest"),new Vector2(-330,220),new Vector2(34,34));
        var title=GameUI.Label(panel,"EVENTS",new Vector2(-252,220),new Vector2(420,38),28);title.color=new Color32(147,220,239,255);
        closeButton=GameUI.Button(panel,"X",new Vector2(333,220),new Vector2(48,48),Close);closeButton.name="CloseEvents";closeButton.GetComponent<UnityEngine.UI.Image>().color=new Color32(240,106,138,255);
        status=GameUI.Label(panel,"Checking the server calendar...",new Vector2(0,177),new Vector2(620,32),18);status.alignment=TextAlignmentOptions.Center;status.color=new Color32(180,187,220,255);
        viewport=GameUI.Rect("EventListViewport",panel,new Vector2(.5f,.5f),new Vector2(0,-20),new Vector2(650,350));
        var viewportImage=viewport.gameObject.AddComponent<UnityEngine.UI.Image>();viewportImage.color=new Color(1,1,1,.001f);viewportImage.raycastTarget=true;
        viewport.gameObject.AddComponent<UnityEngine.UI.RectMask2D>();
        list=GameUI.Rect("EventListContent",viewport,new Vector2(.5f,1),Vector2.zero,new Vector2(650,350));list.pivot=new Vector2(.5f,1);
        var scroll=viewport.gameObject.AddComponent<UnityEngine.UI.ScrollRect>();scroll.viewport=viewport;scroll.content=list;scroll.horizontal=false;scroll.vertical=true;scroll.movementType=UnityEngine.UI.ScrollRect.MovementType.Clamped;scroll.scrollSensitivity=34;
        var refresh=GameUI.Button(panel,"Refresh",new Vector2(0,-220),new Vector2(190,44),Refresh);refresh.name="RefreshEvents";
        closeButton.name="CloseEventsButton";
        EventSystem.current?.SetSelectedGameObject(refresh.gameObject);
        if(Session.Game.Player!=null)Session.Game.Player.GetComponent<PlayerController>().ControlsEnabled=false;
        Refresh();
    }

    public void Refresh()
    {
        if(root==null||loading)return;loading=true;status.text="Checking the server calendar...";ClearList();StartCoroutine(Load());
    }

    private IEnumerator Load()
    {
        ServerEventFeedResponse result=null;string error=null;
        yield return EventFeedClient.Request((response,failure)=>{result=response;error=failure;});
        loading=false;
        if(root==null)yield break;
        if(error!=null||result==null){feed=null;status.text="Events are unavailable. Try again when connected.";yield break;}
        try{serverNow=EventCountdown.ParseUtc(result.serverTime);receivedAt=Time.realtimeSinceStartupAsDouble;feed=result;RenderList();}
        catch(Exception){feed=null;status.text="The event calendar could not be read.";}
    }

    private void Update()
    {
        if(root==null||feed==null)return;
        var now=serverNow+TimeSpan.FromSeconds(Math.Max(0,Time.realtimeSinceStartupAsDouble-receivedAt));
        foreach(var pair in countdowns)
        {
            var item=Array.Find(feed.events,eventData=>eventData!=null&&eventData.id==pair.Key);
            if(item!=null&&pair.Value!=null)pair.Value.text=EventCountdown.Format(EventCountdown.Remaining(item,now));
        }
        if(detail!=null&&detailEvent!=null&&detailCountdown!=null)
            detailCountdown.text="ENDS IN  "+EventCountdown.Format(EventCountdown.Remaining(detailEvent,now));
    }

    private void RenderList()
    {
        ClearList();
        var events=feed.events??Array.Empty<ServerEventSnapshot>();
        if(events.Length==0){status.text="No active events right now.";EventSystem.current?.SetSelectedGameObject(GameObject.Find("RefreshEvents"));return;}
        status.text=$"SERVER TIME  {serverNow:dd MMM HH:mm} UTC   ·   {events.Length} active";
        var visible=new List<ServerEventSnapshot>(events.Length);
        foreach(var item in events)if(item!=null)visible.Add(item);
        if(visible.Count==0){status.text="No active events right now.";EventSystem.current?.SetSelectedGameObject(GameObject.Find("RefreshEvents"));return;}
        float y=12;int shown=0;
        foreach(var item in visible)
        {
            bool featured=shown==0;
            float height=featured?112:76;
            var row=GameUI.Panel(list,"EventRow-"+item.id,Vector2.zero,new Vector2(640,height));
            row.anchorMin=row.anchorMax=new Vector2(.5f,1);row.pivot=new Vector2(.5f,1);row.anchoredPosition=new Vector2(0,-y);
            row.GetComponent<UnityEngine.UI.Image>().raycastTarget=false;
            var hit=GameUI.Button(row,"",Vector2.zero,new Vector2(640,height),()=>OpenDetails(item));
            hit.name="EventRowButton-"+item.id;
            hit.GetComponent<UnityEngine.UI.Image>().color=new Color(1,1,1,.001f);
            string badge=TypeLabel(item.type);
            var accent=GameUI.Label(row,badge,new Vector2(-260,featured?34:15),new Vector2(170,22),featured?15:13);accent.alignment=TextAlignmentOptions.MidlineLeft;accent.color=new Color32(147,220,239,255);
            var name=GameUI.Label(row,ShortTitle(item,featured?38:32),new Vector2(-74,featured?34:15),new Vector2(330,26),featured?22:19);name.alignment=TextAlignmentOptions.MidlineLeft;
            var countdown=GameUI.Label(row,EventCountdown.Format(EventCountdown.Remaining(item,serverNow)),new Vector2(235,featured?34:15),new Vector2(130,24),featured?18:16);countdown.name="Countdown";countdown.alignment=TextAlignmentOptions.Center;countdown.color=new Color32(255,210,127,255);countdowns[item.id]=countdown;
            var note=GameUI.Label(row,featured?ShortDescription(item.description,92):ShortDescription(item.description,64),new Vector2(-54,featured?-5:-12),new Vector2(390,featured?34:23),featured?15:13);note.alignment=TextAlignmentOptions.MidlineLeft;note.textWrappingMode=TextWrappingModes.Normal;note.color=new Color32(208,199,220,255);
            var action=GameUI.Label(row,featured?"VIEW EVENT  ›":"DETAILS  ›",new Vector2(240,featured?-34:-12),new Vector2(130,22),featured?15:13);action.alignment=TextAlignmentOptions.Center;action.color=new Color32(255,210,127,255);
            y+=height+10;shown++;
        }
        list.sizeDelta=new Vector2(650,Mathf.Max(350,y+12));
        var listScroll=viewport.GetComponent<UnityEngine.UI.ScrollRect>();if(listScroll!=null)listScroll.verticalNormalizedPosition=1;
        var firstEvent=GameObject.Find("EventRowButton-"+visible[0].id);
        EventSystem.current?.SetSelectedGameObject(firstEvent!=null?firstEvent:GameObject.Find("RefreshEvents"));
    }

    private void OpenDetails(ServerEventSnapshot item)
    {
        if(root==null||item==null)return;
        if(detail!=null)Destroy(detail.gameObject);
        detailLaunchButton=null;
        detailEvent=item;
        detail=GameUI.Panel(root,"EventDetails",Vector2.zero,new Vector2(700,520));
        closeButton?.gameObject.SetActive(false);
        GameUI.Icon(detail,PixelArt.Icon("quest"),new Vector2(-300,220),new Vector2(34,34));
        var title=GameUI.Label(detail,ShortTitle(item,42),new Vector2(-220,220),new Vector2(430,34),25);title.color=new Color32(147,220,239,255);
        var back=GameUI.Button(detail,"Back",new Vector2(280,220),new Vector2(120,42),CloseDetails);back.name="BackToEventList";
        var type=GameUI.Label(detail,TypeLabel(item.type)+"  ·  SERVER-TIMED",new Vector2(0,171),new Vector2(600,26),16);type.alignment=TextAlignmentOptions.Center;type.color=new Color32(255,210,127,255);
        var description=ShortDescription(item.description);
        var body=GameUI.Label(detail,description,new Vector2(0,107),new Vector2(600,86),19);body.alignment=TextAlignmentOptions.Center;body.textWrappingMode=TextWrappingModes.Normal;
        detailCountdown=GameUI.Label(detail,"ENDS IN  "+EventCountdown.Format(EventCountdown.Remaining(item,serverNow)),new Vector2(0,43),new Vector2(600,30),22);detailCountdown.alignment=TextAlignmentOptions.Center;detailCountdown.color=new Color32(255,210,127,255);
        var eligibility=GameUI.Label(detail,"Eligibility: checking with server...",new Vector2(0,-17),new Vector2(600,32),16);eligibility.alignment=TextAlignmentOptions.Center;eligibility.color=new Color32(208,199,235,255);
        detailEligibility=eligibility;
        var progress=GameUI.Label(detail,"Checking server progress...",new Vector2(0,-83),new Vector2(600,68),15);progress.alignment=TextAlignmentOptions.Center;progress.textWrappingMode=TextWrappingModes.Normal;progress.color=new Color32(208,199,235,255);detailProgress=progress;
        RenderRewardPreview(item);
        if(item.config!=null&&item.config.destination=="campaign")
        {
            detailLaunchButton=GameUI.Button(detail,"PLAY CAMPAIGN",new Vector2(0,-234),new Vector2(240,42),LaunchCampaign);
            detailLaunchButton.name="LaunchEventDestination";
        }
        StartCoroutine(LoadStatus(item));
        EventSystem.current?.SetSelectedGameObject(detailLaunchButton!=null?detailLaunchButton.gameObject:GameObject.Find("BackToEventList"));
    }

    private IEnumerator LoadStatus(ServerEventSnapshot item)
    {
        ServerEventStatus result=null;string error=null;
        yield return EventStatusClient.Request(Session,item.id,item.version,(response,failure)=>{result=response;error=failure;});
        if(root==null||detailEvent!=item||detailEligibility==null||detailProgress==null)yield break;
        if(error!=null||result==null)
        {
            detailEligibility.text="Eligibility: unavailable until the server responds.";
            detailProgress.text=ProgressText(item.type);
            yield break;
        }
        detailEligibility.text=result.eligible?"Eligibility: AVAILABLE · server verified":"Eligibility: NOT AVAILABLE · "+result.eligibilityReason;
        detailEligibility.color=result.eligible?new Color32(180,230,180,255):new Color32(240,160,160,255);
        detailProgress.text=$"Server claims recorded: {result.claimsCompleted}\n{FormatProgress(result.progress)}\n{ProgressText(item.type)}";
    }

    private void CloseDetails()
    {
        if(detail==null)return;Destroy(detail.gameObject);detail=null;detailEvent=null;detailCountdown=null;detailEligibility=null;detailProgress=null;detailLaunchButton=null;closeButton?.gameObject.SetActive(true);
        EventSystem.current?.SetSelectedGameObject(GameObject.Find("CloseEventsButton"));
    }

    private void RenderRewardPreview(ServerEventSnapshot item)
    {
        var rewards=item?.rewards??Array.Empty<ServerEventReward>();
        var heading=GameUI.Label(detail,rewards.Length>3?$"REWARDS  ·  +{rewards.Length-3} MORE":"REWARDS",new Vector2(0,-132),new Vector2(300,22),14);heading.alignment=TextAlignmentOptions.Center;heading.color=new Color32(255,210,127,255);
        var preview=GameUI.Rect("EventRewardPreview",detail,new Vector2(.5f,.5f),new Vector2(0,-171),new Vector2(620,44));
        if(rewards.Length==0)
        {
            var none=GameUI.Label(preview,"No rewards listed",Vector2.zero,new Vector2(590,32),15);none.alignment=TextAlignmentOptions.Center;none.color=new Color32(180,187,220,255);return;
        }

        int shown=Mathf.Min(3,rewards.Length);
        for(int i=0;i<shown;i++)
        {
            var reward=rewards[i];if(reward==null)continue;
            float x=(i-(shown-1)*.5f)*198;
            var chip=GameUI.Panel(preview,"EventReward-"+i,new Vector2(x,0),new Vector2(shown==1?250:188,44));
            chip.GetComponent<UnityEngine.UI.Image>().raycastTarget=false;
            Sprite icon=null;string label;
            if(reward.currency=="coins"||reward.currency=="gems")
            {
                icon=PixelArt.Icon(reward.currency=="coins"?"gold":"gem");
                label=$"{Mathf.Max(0,reward.amount):N0} {(reward.currency=="coins"?"Gold":"Gems")}";
            }
            else
            {
                var definition=FindEventItem(reward.itemId);
                icon=definition!=null&&definition.iconSprite!=null?definition.iconSprite:PixelArt.Icon("quest");
                string name=definition!=null&&!string.IsNullOrWhiteSpace(definition.DisplayName)?definition.DisplayName:HumanizeId(reward.itemId);
                label=$"{name}  T{Mathf.Clamp(reward.tier,1,5)} ×{Mathf.Max(1,reward.count)}";
            }
            if(icon!=null)GameUI.Icon(chip,icon,new Vector2(-chip.sizeDelta.x*.5f+27,0),new Vector2(30,30));
            var text=GameUI.Label(chip,label,new Vector2(12,0),new Vector2(chip.sizeDelta.x-46,34),14);text.alignment=TextAlignmentOptions.MidlineLeft;text.overflowMode=TextOverflowModes.Ellipsis;text.textWrappingMode=TextWrappingModes.NoWrap;text.color=new Color32(239,234,255,255);
        }
    }

    private static ItemData FindEventItem(string id)
    {
        if(string.IsNullOrWhiteSpace(id))return null;
        if(eventItems==null)
        {
            eventItems=new Dictionary<string,ItemData>(StringComparer.Ordinal);
            foreach(var item in Resources.LoadAll<ItemData>("Catalog"))if(item!=null&&!string.IsNullOrWhiteSpace(item.Id))eventItems[item.Id]=item;
        }
        return eventItems.TryGetValue(id,out var result)?result:null;
    }

    private static string HumanizeId(string id)
    {
        if(string.IsNullOrWhiteSpace(id))return "Item";
        var words=id.Replace('_',' ').Replace('-',' ').Trim();
        return words.Length>22?words.Substring(0,19)+"...":words;
    }

    private void LaunchCampaign()
    {
        if(detailEvent?.config==null||detailEvent.config.destination!="campaign"||Session?.Game==null)return;
        var game=Session.Game;Close();game.StartExpedition();
    }

    private static string ProgressText(string type)
    {
        switch(type)
        {
            case "login_calendar":return "Progress: one server-validated claim per UTC day.";
            case "token_exchange":
            case "event_shop":return "Progress: earn event currency in eligible modes, then spend it in the event shop.";
            case "milestone_track":
            case "community_goal":return "Progress: verified clears advance the configured milestone track.";
            case "daily_quests":return "Progress: complete the rotating server-issued goals for today.";
            case "boss_challenge":return "Progress: your best verified boss score is recorded for the event.";
            case "raid_ladder":return "Progress: verified co-op damage contributes to personal, guild and ranked rewards.";
            case "tower_defense":return "Progress: waves, score and attempts are validated by the event module.";
            case "double_drop":return "Progress: selected modes receive the server-defined reward modifier.";
            case "collaboration_pack":return "Progress: story, missions, login rewards and the exchange shop unlock together.";
            default:return "Progress: tracked by the server event module.";
        }
    }

    private static string FormatProgress(ServerEventProgress progress)
    {
        if(progress==null||string.IsNullOrWhiteSpace(progress.metric))return "Progress: awaiting this event module.";
        var cap=progress.cap>0?$" / {progress.cap}":"";
        var next=progress.nextThreshold>0?$" · next {progress.nextThreshold}":"";
        return $"Progress: {progress.metric} {progress.value}{cap}{next}";
    }

    private static string ShortTitle(ServerEventSnapshot item,int max=30)
    {
        var value=string.IsNullOrWhiteSpace(item?.title)?item?.id??"EVENT":item.title.Trim();
        return value.Length<=max?value:value.Substring(0,max-3)+"...";
    }

    private static string ShortDescription(string value,int max=220)
    {
        if(string.IsNullOrWhiteSpace(value))return "Open event details to see its server-tracked goal and progress.";
        value=value.Trim();return value.Length<=max?value:value.Substring(0,max-3)+"...";
    }

    private static string TypeLabel(string type)
    {
        switch(type)
        {
            case "tower_defense":return "RIFT DEFENSE";
            case "boss_challenge":return "BOSS CHALLENGE";
            case "raid_ladder":return "RAID LADDER";
            case "token_exchange":return "EVENT BAZAAR";
            case "double_drop":return "DROP BOOST";
            case "news_inbox":return "RIFT NEWS";
            default:return (type??"EVENT").Replace('_',' ').ToUpperInvariant();
        }
    }

    private void ClearList(){countdowns.Clear();if(list==null)return;for(int i=list.childCount-1;i>=0;i--)Destroy(list.GetChild(i).gameObject);}

    public void Close()
    {
        if(root==null)return;Destroy(root.gameObject);root=null;panel=null;viewport=null;list=null;detail=null;detailEvent=null;detailCountdown=null;detailEligibility=null;detailProgress=null;closeButton=null;detailLaunchButton=null;status=null;feed=null;loading=false;countdowns.Clear();
        if(Session?.Game?.Player!=null)Session.Game.Player.GetComponent<PlayerController>().ControlsEnabled=!Session.UsesDedicated||Session.Authenticated;
    }
}
