using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Object;
using UnityEngine;

public sealed partial class CoopWorld
{
    private readonly Dictionary<int,string> accounts=new Dictionary<int,string>();
    private readonly HashSet<string> pendingLogin=new HashSet<string>();
    private readonly HashSet<int> transactions=new HashSet<int>();
    private IEnumerator LoginWhenReady(){while(Game.Player==null||Game.Forge==null)yield return null;LoginServerRpc(Game.Session.PlayerId,Game.Session.PlayerSecret,Game.Player.GetComponent<CharacterCustomizer>().Appearance,AccountClient.Enabled);}
    private string runId;
    private bool persisting,rewardsReady;
    public bool NetworkPaused { get; private set; }
    [ObserversRpc] private void PauseObserversRpc(bool paused){NetworkPaused=paused;}
    [ServerRpc(RequireOwnership=false)]
    public void LoginServerRpc(string id,string secret,CharacterAppearanceData appearance,bool ticketAuth=false,NetworkConnection sender=null)
    {
        if(!Game.Session.DedicatedServer||sender==null||running||members.Count+pendingLogin.Count>=2||accounts.ContainsValue(id)||!pendingLogin.Add(id)){if(sender!=null)NoticeTargetRpc(sender,"Server is busy. Reconnect when an expedition slot is free.");return;}
        StartCoroutine(Login(sender,id,secret,appearance,ticketAuth));
    }
    private IEnumerator Login(NetworkConnection connection,string id,string secret,CharacterAppearanceData appearance,bool ticketAuth)
    {
        InventoryState profile=null;string error=null;
        bool fixture=!ticketAuth&&Debug.isDebugBuild&&Array.IndexOf(Environment.GetCommandLineArgs(),"-dedicatedTest")>=0;
        yield return DedicatedPersistence.Request(fixture?"/login":"/ticket",new ServerRequest{id=id,secret=secret},(p,e)=>{profile=p;error=e;});
        if(error==null&&(fixture||!profile.Appearance.CustomColors))yield return DedicatedPersistence.Request("/appearance",new ServerRequest{id=id,appearance=appearance},(p,e)=>{profile=p;error=e;});
        pendingLogin.Remove(id);
        if(!ServerManager.Clients.ContainsKey(connection.ClientId))yield break;
        if(error!=null){NoticeTargetRpc(connection,error);yield break;}
        var actor=Game.CreateRemotePlayer();actor.transform.position=new Vector3(members.Count*1.2f,-1.5f,0);actor.GetComponent<CharacterCustomizer>().Apply(profile.Appearance);
        var forge=actor.gameObject.AddComponent<ItemTierUpManager>();forge.Definitions=Game.Items;forge.Stats=actor.GetComponent<CharacterStats>();forge.Customizer=actor.GetComponent<CharacterCustomizer>();forge.Configure(profile);actor.ResetHealth(forge.Stats.MaxHealth);actor.InSafeZone=true;actor.GetComponent<PlayerController>().ControlsEnabled=true;
        members[connection.ClientId]=actor;accounts[connection.ClientId]=id;Raid.RegisterServerPlayer(connection.ClientId,actor);
        if(members.Count==1){Game.BindDedicatedPlayer(actor);Game.Forge.Stats=forge.Stats;}
        ProfileTargetRpc(connection,JsonUtility.ToJson(profile),true);
    }
    [TargetRpc]
    private void ProfileTargetRpc(NetworkConnection connection,string json,bool lobby)
    {
        Game.ReceiveProfile(JsonUtility.FromJson<InventoryState>(json));Game.Player.GetComponent<CharacterCustomizer>().Apply(Game.Forge.State.Appearance);Game.Forge.ApplyEquipment();
        if(lobby){Game.Session.Connected();Game.ReturnToTown();}
    }
    [TargetRpc]
    private void NoticeTargetRpc(NetworkConnection connection,string message){Game.SetStatus(message);if(!Game.Session.Authenticated)Game.Session.ConnectionLost(message);}
    [ServerRpc(RequireOwnership=false)]
    public void TransactionServerRpc(string action,string itemId,int tier,NetworkConnection sender=null)
    {
        if(sender==null||!accounts.TryGetValue(sender.ClientId,out var id)||running||!transactions.Add(sender.ClientId))return;
        if(action!="purchase"&&action!="equip"&&action!="fuse"&&action!="skill"){transactions.Remove(sender.ClientId);return;}
        StartCoroutine(Transaction(sender,id,action,itemId,tier));
    }
    private IEnumerator Transaction(NetworkConnection connection,string id,string action,string itemId,int tier)
    {
        yield return DedicatedPersistence.Request("/transaction",new ServerRequest{id=id,action=action,itemId=itemId,tier=tier},(profile,error)=>{
            if(!ServerManager.Clients.ContainsKey(connection.ClientId))return;
            if(error!=null)NoticeTargetRpc(connection,error);
            else{var actor=members[connection.ClientId];actor.GetComponent<ItemTierUpManager>().Configure(profile);actor.ResetHealth(actor.GetComponent<CharacterStats>().MaxHealth);ProfileTargetRpc(connection,JsonUtility.ToJson(profile),false);}
        });transactions.Remove(connection.ClientId);
    }
    [ServerRpc(RequireOwnership=false)]
    public void CommandServerRpc(string action,NetworkConnection sender=null)
    {
        if(sender==null||!members.ContainsKey(sender.ClientId))return;
        if((action=="pause"||action=="resume")&&running&&!Game.Cooperative)
        {
            bool paused=action=="pause";Time.timeScale=paused?0:1;foreach(var actor in members.Values)actor.GetComponent<PlayerController>().ControlsEnabled=!paused&&!Game.Dungeon.IsCleared&&!Raid.Defeated;PauseObserversRpc(paused);return;
        }
        if(action=="campaign"||action=="coop")
        {
            if(running||transactions.Count>0||pendingLogin.Count>0)return;
            if(action=="campaign"&&members.Count!=1||action=="coop"&&members.Count!=2){NoticeTargetRpc(sender,action=="campaign"?"Campaign requires one player.":"Waiting for a second connected player.");return;}
            running=true;runId=Guid.NewGuid().ToString("N");rewardsReady=false;
            Raid.ResetServerRaid();foreach(var pair in members)Raid.RegisterServerPlayer(pair.Key,pair.Value);
            Game.SetNetworkMode(action=="coop");Game.StartExpedition();int index=0;
            foreach(var actor in members.Values){actor.ResetHealth(actor.GetComponent<CharacterStats>().MaxHealth);actor.GetComponent<SkillStanceSwapper>().ResetCooldowns();actor.InSafeZone=false;actor.GetComponent<PlayerController>().ControlsEnabled=true;actor.transform.position=new Vector3(index++*1.5f,-2,0);}
            ModeObserversRpc(action=="coop");
        }
        else if(action=="retry"&&running&&Game.Dungeon.IsCleared)PersistStage(Game.Dungeon.StageIndex==Game.Dungeon.StageCount-1);
        else if(action=="next"&&running&&rewardsReady&&Game.Dungeon.IsCleared)
        {
            rewardsReady=false;Game.Dungeon.NextRoom();foreach(var actor in members.Values)actor.GetComponent<PlayerController>().ControlsEnabled=true;
        }
        else if(action=="town"&&(Raid.Defeated||rewardsReady))
        {
            Time.timeScale=1;running=false;Game.Dungeon.Clear();Raid.ResetServerRaid();foreach(var actor in members.Values){actor.transform.position=new Vector3(0,-1.5f,0);actor.InSafeZone=true;actor.GetComponent<PlayerController>().ControlsEnabled=true;actor.ResetHealth(actor.MaximumHealth);}TownObserversRpc();
        }
    }
    [ObserversRpc]
    private void ModeObserversRpc(bool cooperative){if(IsServerInitialized)return;Raid.ClearClientDefeat();Game.SetNetworkMode(cooperative);}
    [ObserversRpc]
    private void TownObserversRpc(){if(IsServerInitialized)return;ClearReplicas();Game.ReturnToTown();}
    public int TestSelfRevives { get; private set; }
    public int TestRescues { get; private set; }
    public int TestVictimId { get; private set; }=-1;
    [ObserversRpc]
    private void TestCountersObserversRpc(int self,int rescues,int victim){TestSelfRevives=self;TestRescues=rescues;if(victim>=0)TestVictimId=victim;}
    [ServerRpc(RequireOwnership=false)]
    public void TestDownPeerServerRpc(NetworkConnection sender=null)
    {
        if(!Debug.isDebugBuild||Array.IndexOf(Environment.GetCommandLineArgs(),"-dedicatedTest")<0||sender==null||!members.ContainsKey(sender.ClientId))return;
        foreach(var enemy in FindObjectsByType<EnemyBrain>(FindObjectsSortMode.None))enemy.GetComponent<Combatant>().SimulationEnabled=false;
        foreach(var pair in members){pair.Value.transform.position=new Vector3(pair.Key==sender.ClientId?0:1,-2,0);pair.Value.ResetHealth(pair.Value.MaximumHealth);if(pair.Key!=sender.ClientId){pair.Value.Damage(10000);TestVictimId=pair.Key;}}
        TestCountersObserversRpc(Raid.PlaceholderRevives,Raid.TeammateRevives,TestVictimId);
    }
    [ServerRpc(RequireOwnership=false)]
    public void TestBossServerRpc(NetworkConnection sender=null)
    {
        if(!Debug.isDebugBuild||Array.IndexOf(Environment.GetCommandLineArgs(),"-dedicatedTest")<0||sender==null||!members.ContainsKey(sender.ClientId)||!running)return;
        Game.Dungeon.StageBoss.SimulationEnabled=true;Game.Dungeon.StageBoss.Damage(10000);
    }
    [ServerRpc(RequireOwnership=false)]
    public void RankingsServerRpc(NetworkConnection sender=null)
    {
        if(sender==null||!accounts.ContainsKey(sender.ClientId))return;StartCoroutine(DedicatedPersistence.Raw("/leaderboard",new ServerRequest(),(json,error)=>{if(error==null&&ServerManager.Clients.ContainsKey(sender.ClientId))RankingsTargetRpc(sender,json);}));
    }
    [TargetRpc]
    private void RankingsTargetRpc(NetworkConnection connection,string json)
    {
        var modal=Game.GetComponent<LeaderboardModal>();if(modal==null)modal=Game.gameObject.AddComponent<LeaderboardModal>();modal.Show(json,Game);
    }
    public void PersistStage(bool final)
    {
        if(persisting||!IsServerInitialized)return;StartCoroutine(PersistRewards(final));
    }
    private IEnumerator PersistRewards(bool final)
    {
        persisting=true;bool saved=true;
        foreach(var pair in new Dictionary<int,string>(accounts))
        {
            string receipt=runId+"-"+Game.Dungeon.StageIndex;
            yield return DedicatedPersistence.Request("/transaction",new ServerRequest{id=pair.Value,action="reward",receipt=receipt,stage=Game.Dungeon.StageIndex},(profile,error)=>{
                if(error!=null)saved=false;
                else if(ServerManager.Clients.TryGetValue(pair.Key,out var connection))ProfileTargetRpc(connection,JsonUtility.ToJson(profile),false);
            });
        }
        persisting=false;rewardsReady=saved;
        RewardObserversRpc(final,saved);
    }
    [ObserversRpc]
    private void RewardObserversRpc(bool final,bool saved)
    {
        if(IsServerInitialized)return;
        if(saved)Game.ShowNetworkReward(final,"BOSS DEFEATED - Loot saved on server.");
        else Game.ShowNetworkRewardRetry();
    }
}
