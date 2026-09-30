using System;
using System.Collections;
using UnityEngine;

[Serializable] public sealed class InboxReward
{
    public string currency,itemId;
    public int amount,tier,count;
}
[Serializable] public sealed class InboxEntry
{
    public string id,eventId,claimKey,status,createdAt,claimedAt;
    public int eventVersion;
    public InboxReward reward;
}
[Serializable] public sealed class InboxResponse
{
    public InboxEntry[] entries;
}
[Serializable] public sealed class InboxClaimResponse
{
    public bool claimed;
    public InboxEntry entry;
    public InventoryState profile;
}

public static class InboxClient
{
    public static IEnumerator Request(CoopSession session,Action<InboxResponse,string> complete)
    {
        if(session==null||string.IsNullOrWhiteSpace(session.PlayerId)){complete(null,"Inbox is available after connecting to the server.");yield break;}
        string body=null,error=null;yield return DedicatedPersistence.Raw("/inbox",new ServerRequest{id=session.PlayerId},(response,failure)=>{body=response;error=failure;});
        InboxResponse result=null;
        if(error==null)try{result=JsonUtility.FromJson<InboxResponse>(body);if(result==null)error="Invalid inbox response.";else if(result.entries==null)result.entries=Array.Empty<InboxEntry>();}catch(Exception){error="Invalid inbox response.";}
        complete(result,error);
    }

    public static IEnumerator Claim(CoopSession session,string inboxId,Action<InboxClaimResponse,string> complete)
    {
        if(session==null||string.IsNullOrWhiteSpace(session.PlayerId)||string.IsNullOrWhiteSpace(inboxId)){complete(null,"Inbox entry is unavailable.");yield break;}
        string body=null,error=null;yield return DedicatedPersistence.Raw("/inbox-claim",new ServerRequest{id=session.PlayerId,inboxId=inboxId},(response,failure)=>{body=response;error=failure;});
        InboxClaimResponse result=null;
        if(error==null)try{result=JsonUtility.FromJson<InboxClaimResponse>(body);if(result==null)error="Invalid inbox claim response.";}catch(Exception){error="Invalid inbox claim response.";}
        complete(result,error);
    }
}
