using System;
using System.Collections;
using UnityEngine;

[Serializable]
public sealed class ServerEventStatus
{
    public string eventId,eligibilityReason,state;
    public int eventVersion,claimsCompleted;
    public bool eligible;
}

// Event status is authoritative server data. The client only renders it;
// claims and module-specific progress remain server-owned operations.
public static class EventStatusClient
{
    public static IEnumerator Request(CoopSession session,string eventId,int eventVersion,Action<ServerEventStatus,string> complete)
    {
        if(session==null||string.IsNullOrWhiteSpace(session.PlayerId)||string.IsNullOrWhiteSpace(eventId)){complete(null,"Event status is available after connecting to the server.");yield break;}
        string body=null,error=null;
        yield return DedicatedPersistence.Raw("/event-status",new ServerRequest{id=session.PlayerId,eventId=eventId,eventVersion=eventVersion},(response,failure)=>{body=response;error=failure;});
        ServerEventStatus result=null;
        if(error==null)
        {
            try{result=JsonUtility.FromJson<ServerEventStatus>(body);if(result==null||string.IsNullOrWhiteSpace(result.eventId))error="Invalid event status.";}
            catch(Exception){error="Invalid event status.";}
        }
        complete(result,error);
    }
}
