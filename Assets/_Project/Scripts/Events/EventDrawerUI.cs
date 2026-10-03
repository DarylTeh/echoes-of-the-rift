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
    private RectTransform root,panel,list,detail;
    private TMP_Text status,detailCountdown,detailEligibility,detailProgress;
    private ServerEventSnapshot detailEvent;
    private ServerEventFeedResponse feed;
    private readonly Dictionary<string,TMP_Text> countdowns=new Dictionary<string,TMP_Text>();
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
        var close=GameUI.Button(panel,"X",new Vector2(333,220),new Vector2(48,48),Close);close.name="CloseEvents";close.GetComponent<UnityEngine.UI.Image>().color=new Color32(240,106,138,255);
        status=GameUI.Label(panel,"Checking the server calendar...",new Vector2(0,177),new Vector2(620,32),18);status.alignment=TextAlignmentOptions.Center;status.color=new Color32(180,187,220,255);
        list=GameUI.Rect("EventList",panel,new Vector2(.5f,.5f),new Vector2(0,-22),new Vector2(650,350));
        var refresh=GameUI.Button(panel,"Refresh",new Vector2(-118,-220),new Vector2(190,44),Refresh);refresh.name="RefreshEvents";
        var back=GameUI.Button(panel,"Close",new Vector2(118,-220),new Vector2(190,44),Close);back.name="CloseEventsButton";
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
        if(events.Length==0){status.text="No active events right now.";return;}
        status.text=$"SERVER TIME  {serverNow:dd MMM HH:mm} UTC   ·   {events.Length} active";
        float y=145;int shown=0;
        foreach(var item in events)
        {
            if(item==null||shown>=4)continue;
            var row=GameUI.Panel(list,"EventRow-"+item.id,new Vector2(0,y),new Vector2(640,72));
            row.GetComponent<UnityEngine.UI.Image>().raycastTarget=false;
            var hit=GameUI.Button(row,"",Vector2.zero,new Vector2(640,72),()=>OpenDetails(item));
            hit.name="EventRowButton-"+item.id;
            hit.GetComponent<UnityEngine.UI.Image>().color=new Color(1,1,1,.001f);
            var accent=GameUI.Label(row,TypeLabel(item.type),new Vector2(-272,14),new Vector2(150,22),14);accent.color=new Color32(147,220,239,255);
            var name=GameUI.Label(row,ShortTitle(item),new Vector2(-82,14),new Vector2(330,28),20);name.alignment=TextAlignmentOptions.MidlineLeft;
            var countdown=GameUI.Label(row,EventCountdown.Format(EventCountdown.Remaining(item,serverNow)),new Vector2(235,14),new Vector2(140,24),18);countdown.name="Countdown";countdown.alignment=TextAlignmentOptions.Center;countdown.color=new Color32(255,210,127,255);countdowns[item.id]=countdown;
            var version=GameUI.Label(row,$"v{item.version}",new Vector2(-272,-15),new Vector2(120,18),13);version.color=new Color32(146,139,177,255);
            var note=GameUI.Label(row,"Server-timed · rewards delivered through inbox",new Vector2(28,-15),new Vector2(380,18),13);note.color=new Color32(146,139,177,255);
            y-=82;shown++;
        }
        if(events.Length>shown){var more=GameUI.Label(list,$"+ {events.Length-shown} more active event(s)",new Vector2(0,y),new Vector2(620,28),16);more.alignment=TextAlignmentOptions.Center;more.color=new Color32(180,187,220,255);}
        EventSystem.current?.SetSelectedGameObject(GameObject.Find("CloseEventsButton"));
    }

    private void OpenDetails(ServerEventSnapshot item)
    {
        if(root==null||item==null)return;
        if(detail!=null)Destroy(detail.gameObject);
        detailEvent=item;
        detail=GameUI.Panel(root,"EventDetails",Vector2.zero,new Vector2(700,470));
        GameUI.Icon(detail,PixelArt.Icon("quest"),new Vector2(-300,195),new Vector2(34,34));
        var title=GameUI.Label(detail,ShortTitle(item,42),new Vector2(-220,195),new Vector2(430,34),25);title.color=new Color32(147,220,239,255);
        var back=GameUI.Button(detail,"Back",new Vector2(280,195),new Vector2(120,42),CloseDetails);back.name="BackToEventList";
        var type=GameUI.Label(detail,TypeLabel(item.type)+"  ·  SERVER-TIMED",new Vector2(0,146),new Vector2(600,26),16);type.alignment=TextAlignmentOptions.Center;type.color=new Color32(255,210,127,255);
        var description=ShortDescription(item.description);
        var body=GameUI.Label(detail,description,new Vector2(0,82),new Vector2(600,86),19);body.alignment=TextAlignmentOptions.Center;body.textWrappingMode=TextWrappingModes.Normal;
        detailCountdown=GameUI.Label(detail,"ENDS IN  "+EventCountdown.Format(EventCountdown.Remaining(item,serverNow)),new Vector2(0,18),new Vector2(600,30),22);detailCountdown.alignment=TextAlignmentOptions.Center;detailCountdown.color=new Color32(255,210,127,255);
        var eligibility=GameUI.Label(detail,"Eligibility: checking with server...",new Vector2(0,-42),new Vector2(600,38),16);eligibility.alignment=TextAlignmentOptions.Center;eligibility.color=new Color32(208,199,235,255);
        detailEligibility=eligibility;
        var progress=GameUI.Label(detail,"Checking server progress...",new Vector2(0,-108),new Vector2(600,72),16);progress.alignment=TextAlignmentOptions.Center;progress.textWrappingMode=TextWrappingModes.Normal;progress.color=new Color32(208,199,235,255);detailProgress=progress;
        StartCoroutine(LoadStatus(item));
        EventSystem.current?.SetSelectedGameObject(GameObject.Find("BackToEventList"));
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
        if(detail==null)return;Destroy(detail.gameObject);detail=null;detailEvent=null;detailCountdown=null;detailEligibility=null;detailProgress=null;
        EventSystem.current?.SetSelectedGameObject(GameObject.Find("CloseEventsButton"));
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

    private static string ShortDescription(string value)
    {
        if(string.IsNullOrWhiteSpace(value))return "This event is managed by the server calendar.";
        value=value.Trim();return value.Length<=220?value:value.Substring(0,217)+"...";
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
        if(root==null)return;Destroy(root.gameObject);root=null;panel=null;list=null;detail=null;detailEvent=null;detailCountdown=null;detailEligibility=null;detailProgress=null;status=null;feed=null;loading=false;
        if(Session?.Game?.Player!=null)Session.Game.Player.GetComponent<PlayerController>().ControlsEnabled=!Session.UsesDedicated||Session.Authenticated;
    }
}
