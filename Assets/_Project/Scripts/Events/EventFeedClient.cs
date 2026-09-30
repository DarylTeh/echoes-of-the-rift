using System;
using System.Collections;
using UnityEngine;

[Serializable]
public sealed class ServerEventFeedResponse
{
    public string serverTime;
    public ServerEventSnapshot[] events;
}

// Thin transport adapter for the server-authoritative event calendar. UI code
// should render the response and EventCountdown, but never grant rewards or
// decide eligibility from a device clock.
public static class EventFeedClient
{
    public static IEnumerator Request(Action<ServerEventFeedResponse,string> complete)
    {
        string body=null,error=null;
        yield return DedicatedPersistence.Raw("/events",new ServerRequest(),(response,failure)=>{body=response;error=failure;});
        ServerEventFeedResponse feed=null;
        if(error==null)
        {
            try
            {
                feed=JsonUtility.FromJson<ServerEventFeedResponse>(body);
                if(feed==null||string.IsNullOrWhiteSpace(feed.serverTime))error="Invalid event feed.";
                else if(feed.events==null)feed.events=Array.Empty<ServerEventSnapshot>();
            }
            catch(Exception){error="Invalid event feed.";}
        }
        complete(feed,error);
    }
}
