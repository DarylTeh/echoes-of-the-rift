using System;
using System.Collections;
using System.Net.Http;
using System.Text;
using UnityEngine;

[Serializable] public sealed class ServerRequest
{
    public string id,secret,action,itemId,receipt,inboxId,eventId,claimKey;
    public int tier,stage,eventVersion;
    public CharacterAppearanceData appearance;
}
public static class DedicatedPersistence
{
    private static readonly HttpClient client=new HttpClient { Timeout=TimeSpan.FromSeconds(5) };
    public static IEnumerator Request(string route,ServerRequest data,Action<InventoryState,string> complete)
    {
        string json=null,error=null;yield return Raw(route,data,(body,e)=>{json=body;error=e;});
        InventoryState state=null;
        if(error==null)try{state=JsonUtility.FromJson<InventoryState>(json);if(state==null||!state.IsValid())error="Invalid server profile.";}catch(Exception){error="Invalid server response.";}
        complete(state,error);
    }
    public static IEnumerator Raw(string route,ServerRequest data,Action<string,string> complete)
    {
        string key=Environment.GetEnvironmentVariable("COOKIE_SERVER_KEY");
        if(string.IsNullOrWhiteSpace(key)){complete(null,"Dedicated server persistence is not configured.");yield break;}
        string url=Environment.GetEnvironmentVariable("COOKIE_DB_URL")??"http://127.0.0.1:8081";
        var message=new HttpRequestMessage(HttpMethod.Post,url+route);message.Headers.TryAddWithoutValidation("Authorization","Bearer "+key);message.Content=new StringContent(JsonUtility.ToJson(data),Encoding.UTF8,"application/json");
        var task=client.SendAsync(message);while(!task.IsCompleted)yield return null;
        if(task.IsFaulted||task.IsCanceled){message.Dispose();complete(null,"Persistence service unavailable. Please retry.");yield break;}
        var response=task.Result;var text=response.Content.ReadAsStringAsync();while(!text.IsCompleted)yield return null;
        string body=text.IsCompletedSuccessfully?text.Result:"";
        string error=response.IsSuccessStatusCode?null:"Transaction rejected: "+body;
        response.Dispose();message.Dispose();complete(body,error);
    }
}
