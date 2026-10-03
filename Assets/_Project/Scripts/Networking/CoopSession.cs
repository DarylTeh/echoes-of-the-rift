using System;
using System.Collections;
using System.IO;
using FishNet.Managing;
using FishNet.Transporting;
using UnityEngine;

public sealed class CoopSession : MonoBehaviour
{
    public NetworkManager Manager;
    public CoopWorld World;
    public ArenaGame Game;
    public bool Active { get; private set; }
    public bool Authenticated { get; private set; }
    public bool IsHost=>Manager.IsServerStarted;
    public bool DedicatedServer=>Array.IndexOf(Environment.GetCommandLineArgs(),"-dedicatedServer")>=0;
    public bool UsesDedicated=>DedicatedServer||Array.IndexOf(Environment.GetCommandLineArgs(),"-cookieSmoke")<0||Array.IndexOf(Environment.GetCommandLineArgs(),"-dedicatedClientTest")>=0;
    public string PlayerId { get; private set; }
    public string PlayerSecret { get; private set; }
    public string Address="127.0.0.1";
    private RectTransform lost;
    private bool reconnect;
    [Serializable] private class Identity { public string id,secret; }
    [Serializable] private class Endpoint { public string address; }
    private void Start()
    {
        var args=Environment.GetCommandLineArgs();bool dedicatedHarness=Array.IndexOf(args,"-dedicatedTest")>=0||Array.IndexOf(args,"-dedicatedClientTest")>=0;bool developerSmoke=Debug.isDebugBuild&&Array.IndexOf(args,"-cookieSmoke")>=0;
        if((dedicatedHarness||developerSmoke)&&ushort.TryParse(Environment.GetEnvironmentVariable("RIFT_TEST_GAME_PORT"),out ushort testPort))Manager.TransportManager.Transport.SetPort(testPort);
        Manager.ClientManager.OnClientConnectionState+=ConnectionChanged;
        Manager.ServerManager.OnRemoteConnectionState+=(connection,args)=>{if(args.ConnectionState==RemoteConnectionState.Stopped)World.RemoveMember(connection.ClientId);};
        string config=Path.Combine(Application.streamingAssetsPath,"server.json");if(File.Exists(config)){var endpoint=JsonUtility.FromJson<Endpoint>(File.ReadAllText(config));if(!string.IsNullOrWhiteSpace(endpoint?.address))Address=endpoint.address;}
        if(UsesDedicated&&!DedicatedServer&&!AccountClient.Enabled)
        {
            string path=Path.Combine(Application.persistentDataPath,Array.IndexOf(Environment.GetCommandLineArgs(),"-dedicatedClientTest")>=0?(Array.IndexOf(Environment.GetCommandLineArgs(),"-pairLeader")>=0?"test-leader-identity.json":Array.IndexOf(Environment.GetCommandLineArgs(),"-dedicatedPair")>=0?"test-peer-identity.json":"test-connection-identity.json"):"connection-identity.json");Identity identity;
            if(File.Exists(path))identity=JsonUtility.FromJson<Identity>(File.ReadAllText(path));else{identity=new Identity{id=Guid.NewGuid().ToString("N"),secret=Guid.NewGuid().ToString("N")+Guid.NewGuid().ToString("N")};Directory.CreateDirectory(Application.persistentDataPath);File.WriteAllText(path,JsonUtility.ToJson(identity));}
            PlayerId=identity.id;PlayerSecret=identity.secret;reconnect=true;InvokeRepeating(nameof(Retry),5,5);Join(Address);
        }
        if(AccountClient.Enabled)InvokeRepeating(nameof(Retry),5,5);
        if(DedicatedServer)StartCoroutine(BootDedicated());
    }
    private IEnumerator BootDedicated()
    {
        yield return new WaitForSecondsRealtime(1);
        var appearance=new CharacterAppearanceData{ClassId="wayfarer",PassiveSkillId="steadfast",CustomColors=true,SkinRGB=Color.white,HairRGB=Color.black,EyeRGB=Color.cyan};
        Game.Begin(appearance);Game.PrepareCoop(false);Game.Player.gameObject.SetActive(false);Active=true;
        if(!Manager.ServerManager.StartConnection())throw new InvalidOperationException("Dedicated transport could not bind.");
        Debug.Log("DEDICATED_SERVER_STARTED: testPort="+Environment.GetEnvironmentVariable("RIFT_TEST_GAME_PORT"));
        StartCoroutine(ReportReadiness());
    }
    private IEnumerator ReportReadiness()
    {
        while(Manager!=null){if(Manager.IsServerStarted)yield return DedicatedPersistence.Raw("/heartbeat",new ServerRequest(),(body,error)=>{if(error==null)Debug.Log("DEDICATED_HEARTBEAT_OK");else Debug.LogWarning("DEDICATED_HEARTBEAT_FAILED: "+error);});yield return new WaitForSecondsRealtime(2);}
    }
    private bool refreshing;
    public void SetAccount(AccountResponse response){PlayerId=response.id;PlayerSecret=response.ticket;}
    public void ConnectCentral(){reconnect=true;if(AccountClient.Enabled){if(!refreshing&&!Active)StartCoroutine(RefreshAccount());return;}if(Game.Player!=null)Game.PrepareCoop(true);Join(Address);}
    private IEnumerator RefreshAccount()
    {
        refreshing=true;string failure=null;
        yield return AccountClient.Request(Address,"refresh",new AccountRequest{refreshToken=AccountClient.Token},(response,error)=>{failure=error;if(error==null){AccountClient.Save(response.refreshToken);SetAccount(response);}});
        refreshing=false;
        if(failure!=null){Game.SetStatus(failure);ConnectionLost(failure);yield break;}
        Join(Address);
    }
    public void SignOut(){StartCoroutine(SignOutAccount());}
    private IEnumerator SignOutAccount(){yield return AccountClient.Request(Address,"logout",new AccountRequest{refreshToken=AccountClient.Token},(response,error)=>{});AccountClient.Forget();reconnect=false;Leave();UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);}
    public void Connected(){Authenticated=true;reconnect=false;if(lost!=null)Destroy(lost.gameObject);lost=null;}
    private void Retry(){if(reconnect&&!Active){if(AccountClient.Enabled)ConnectCentral();else Join(Address);}}
    public void ConnectionLost(string message=null)
    {
        if(DedicatedServer||!UsesDedicated)return;if(Active){Active=false;Authenticated=false;Manager.ClientManager.StopConnection();if(Game.Player!=null)Game.EndCoop();}reconnect=true;
        if(lost!=null)return;lost=GameUI.Canvas("ConnectionLost");lost.GetComponent<Canvas>().sortingOrder=500;
        var blocker=GameUI.Rect("ConnectionBlocker",lost,Vector2.one*.5f,Vector2.zero,new Vector2(1280,720));blocker.gameObject.AddComponent<UnityEngine.UI.Image>().color=new Color(0,0,0,.85f);
        var panel=GameUI.Panel(lost,"ConnectionDialog",Vector2.zero,new Vector2(720,256));
        GameUI.Label(panel,EnglishScreens.ConnectionLostNretryingConnectionToServer,new Vector2(0,40),new Vector2(640,100),28);
        // A connection error must never revoke credentials. Sign-out is only
        // reachable through Settings > Accounts and its explicit confirmation.
        GameUI.Button(panel,EnglishScreens.Reconnect,new Vector2(0,-70),new Vector2(300,48),()=>{Active=false;ConnectCentral();});
    }
    public void Host()
    {
        if(UsesDedicated||Active)return;Active=true;Game.PrepareCoop(false);
        if(!Manager.ServerManager.StartConnection()||!Manager.ClientManager.StartConnection("127.0.0.1")){Leave();Game.SetStatus("Could not start test host.");}
    }
    public void Join(string address)
    {
        if(Active)return;Active=true;if(Game.Player!=null)Game.PrepareCoop(true);Game.SetStatus("Connecting to server...");
        if(!Manager.ClientManager.StartConnection(address)){Active=false;ConnectionLost();}
    }
    private void ConnectionChanged(ClientConnectionStateArgs args)
    {
        if(args.ConnectionState==LocalConnectionState.Stopped&&Active){Authenticated=false;Active=false;if(Game.Player!=null)Game.EndCoop();ConnectionLost();}
    }
    public void Leave()
    {
        reconnect=false;Active=false;Manager.ClientManager.StopConnection();if(Manager.IsServerStarted)Manager.ServerManager.StopConnection(true);World.ClearReplicas();Game.EndCoop();
    }
    private void OnDestroy(){if(Manager!=null)Manager.ClientManager.OnClientConnectionState-=ConnectionChanged;}
}
