using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

// The inbox is the recovery boundary for server-issued event rewards.
// Collection is idempotent on the server and profile data is re-synced after a claim.
public sealed class InboxDrawerUI : MonoBehaviour
{
    public CoopSession Session;
    public bool IsOpen=>root!=null;
    private RectTransform root,panel,list;
    private TMP_Text status;
    private bool loading;
    private string claiming;

    public void Open()
    {
        if(root!=null||Session==null||Session.Game==null||Session.Game.Hub==null)return;
        root=GameUI.Canvas("InboxDrawer");root.GetComponent<Canvas>().sortingOrder=365;root.gameObject.AddComponent<UIMenuFocus>();
        var blocker=GameUI.Rect("InboxDrawerBlocker",root,Vector2.one*.5f,Vector2.zero,Vector2.zero);blocker.anchorMin=Vector2.zero;blocker.anchorMax=Vector2.one;blocker.sizeDelta=Vector2.zero;blocker.gameObject.AddComponent<UnityEngine.UI.Image>().color=new Color(0,0,0,.78f);
        panel=GameUI.Panel(root,"InboxPanel",Vector2.zero,new Vector2(760,520));
        GameUI.Icon(panel,PixelArt.Icon("book"),new Vector2(-330,220),new Vector2(34,34));
        var title=GameUI.Label(panel,"REWARD INBOX",new Vector2(-205,220),new Vector2(460,38),28);title.color=new Color32(147,220,239,255);
        var close=GameUI.Button(panel,"X",new Vector2(333,220),new Vector2(48,48),Close);close.name="CloseInbox";close.GetComponent<UnityEngine.UI.Image>().color=new Color32(240,106,138,255);
        status=GameUI.Label(panel,"Checking your rewards...",new Vector2(0,177),new Vector2(620,32),18);status.alignment=TextAlignmentOptions.Center;status.color=new Color32(180,187,220,255);
        list=GameUI.Rect("InboxList",panel,new Vector2(.5f,.5f),new Vector2(0,-22),new Vector2(650,350));
        var refresh=GameUI.Button(panel,"Refresh",new Vector2(-118,-220),new Vector2(190,44),Refresh);refresh.name="RefreshInbox";
        var back=GameUI.Button(panel,"Close",new Vector2(118,-220),new Vector2(190,44),Close);back.name="CloseInboxButton";
        if(Session.Game.Player!=null)Session.Game.Player.GetComponent<PlayerController>().ControlsEnabled=false;
        Refresh();
    }

    public void Refresh()
    {
        if(root==null||loading||claiming!=null)return;loading=true;status.text="Checking your rewards...";ClearList();StartCoroutine(Load());
    }

    private IEnumerator Load()
    {
        InboxResponse result=null;string error=null;yield return InboxClient.Request(Session,(response,failure)=>{result=response;error=failure;});
        loading=false;if(root==null)yield break;
        if(error!=null||result==null){status.text="Rewards are unavailable. Try again when connected.";return;}
        RenderList(result.entries??Array.Empty<InboxEntry>());
    }

    private void RenderList(InboxEntry[] entries)
    {
        ClearList();int pending=0;float y=145;
        foreach(var entry in entries)
        {
            if(entry==null||entry.status=="claimed")continue;
            if(pending>=4)continue;
            var row=GameUI.Panel(list,"InboxRow-"+entry.id,new Vector2(0,y),new Vector2(640,72));
            var eventLabel=GameUI.Label(row,string.IsNullOrWhiteSpace(entry.eventId)?"SERVER REWARD":entry.eventId,new Vector2(-255,15),new Vector2(250,24),16);eventLabel.color=new Color32(147,220,239,255);
            var reward=GameUI.Label(row,RewardText(entry.reward),new Vector2(-255,-15),new Vector2(360,24),19);reward.color=new Color32(255,210,127,255);
            var collect=GameUI.Button(row,"Collect",new Vector2(237,0),new Vector2(150,44),()=>Claim(entry));collect.name="ClaimInbox-"+entry.id;
            y-=82;pending++;
        }
        status.text=pending==0?"No rewards are waiting.":$"{pending} reward{(pending==1?"":"s")} waiting · server delivered";
        if(entries.Length>pending&&pending>=4){var more=GameUI.Label(list,"More rewards are available; collect these first.",new Vector2(0,y),new Vector2(620,28),16);more.alignment=TextAlignmentOptions.Center;more.color=new Color32(180,187,220,255);}
        EventSystem.current?.SetSelectedGameObject(GameObject.Find("CloseInboxButton"));
    }

    private void Claim(InboxEntry entry)
    {
        if(entry==null||claiming!=null||entry.status=="claimed")return;claiming=entry.id;status.text="Delivering reward...";StartCoroutine(ClaimRoutine(entry.id));
    }

    private IEnumerator ClaimRoutine(string id)
    {
        InboxClaimResponse result=null;string error=null;yield return InboxClient.Claim(Session,id,(response,failure)=>{result=response;error=failure;});
        claiming=null;if(root==null)yield break;
        if(error!=null||result==null){status.text="Reward delivery failed. Try again.";return;}
        if(result.profile!=null)Session.Game.ReceiveProfile(result.profile);
        status.text=result.claimed?"Reward added to your account.":"Reward already collected.";Refresh();
    }

    private string RewardText(InboxReward reward)
    {
        if(reward==null)return "Reward";
        if(!string.IsNullOrEmpty(reward.currency))return $"{EnglishUI.Compact(reward.amount)} {reward.currency.ToUpperInvariant()}";
        var item=Array.Find(Session.Game.Items,x=>x!=null&&x.Id==reward.itemId);var name=item!=null?item.DisplayName:reward.itemId;
        return $"{name}  ×{reward.count}";
    }

    private void ClearList(){if(list==null)return;for(int i=list.childCount-1;i>=0;i--)Destroy(list.GetChild(i).gameObject);}

    public void Close()
    {
        if(root==null)return;Destroy(root.gameObject);root=null;panel=null;list=null;status=null;loading=false;claiming=null;
        if(Session?.Game?.Player!=null)Session.Game.Player.GetComponent<PlayerController>().ControlsEnabled=!Session.UsesDedicated||Session.Authenticated;
    }
}
