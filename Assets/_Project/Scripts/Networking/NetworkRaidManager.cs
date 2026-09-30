using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Object;
using UnityEngine;

public sealed class NetworkRaidManager : NetworkBehaviour
{
    private readonly RaidState state=new RaidState();
    private readonly Dictionary<int,Combatant> actors=new Dictionary<int,Combatant>();
    public int PlaceholderRevives { get; private set; }
    public int TeammateRevives { get; private set; }
    public int AlivePlayerCount=>state.AlivePlayerCount;
    public bool Defeated { get; private set; }
    public int ActiveRevives=>state.ActiveRevives;
    public event Action DefeatReceived;
    public override void OnStartServer() { base.OnStartServer(); state.Reset(); Defeated=false; }
    public override void OnStartClient() { Defeated=false; }
    public override void OnStopNetwork() { StopAllCoroutines(); actors.Clear(); state.Reset(); base.OnStopNetwork(); }
    public void ClearClientDefeat(){Defeated=false;}
    public void ResetServerRaid(){if(!IsServerInitialized)return;StopAllCoroutines();actors.Clear();state.Reset();Defeated=false;}
    public bool RegisterServerPlayer(int connectionId,Combatant actor)
    {
        if(!IsServerInitialized||actor==null||!state.Register(connectionId)) return false;
        actors[connectionId]=actor; return true;
    }
    public void RemoveServerPlayer(int connectionId)
    {
        if(!IsServerInitialized)return; actors.Remove(connectionId); state.Remove(connectionId); StopAllCoroutines(); CheckDefeat();
    }
    private void Update()
    {
        if(!IsServerInitialized||Defeated)return;
        foreach(var pair in actors) state.SetAlive(pair.Key,pair.Value!=null&&pair.Value.Alive);
        CheckDefeat();
    }
    private void CheckDefeat()
    {
        if(!state.Defeated||Defeated)return;
        Defeated=true; StopAllCoroutines();
        BroadcastDefeatObserversRpc();
    }
    [ServerRpc(RequireOwnership=false)]
    public void RequestReviveServerRpc(int target,NetworkConnection sender=null)
    {
        SynchronizeActors();
        if(Defeated||sender==null||!actors.TryGetValue(sender.ClientId,out var rescuer)||!actors.TryGetValue(target,out var downed)||rescuer==null||downed==null||Vector2.Distance(rescuer.transform.position,downed.transform.position)>1.8f) return;
        if(state.StartRevive(sender.ClientId,target)) StartCoroutine(Revive(target,sender.ClientId,state.Epoch));
    }
    private IEnumerator Revive(int target,int rescuerId,int epoch)
    {
        float remaining=3;
        while(remaining>0)
        {
            if(Defeated||!actors.TryGetValue(rescuerId,out var rescuer)||rescuer==null||!rescuer.Alive||!actors.TryGetValue(target,out var downed)||downed==null||Vector2.Distance(rescuer.transform.position,downed.transform.position)>1.8f) { state.CancelRevive(target); yield break; }
            remaining-=Time.deltaTime; yield return null;
        }
        if(state.CompleteRevive(target,epoch)&&actors.TryGetValue(target,out var actor)&&actor!=null) { TeammateRevives++;actor.ResetHealth(actor.MaximumHealth); actor.GrantInvulnerability(2); }
    }
    private void SynchronizeActors()
    {
        foreach(var pair in actors)state.SetAlive(pair.Key,pair.Value!=null&&pair.Value.Alive);
        CheckDefeat();
    }
    [ServerRpc(RequireOwnership=false)]
    public void RequestPlaceholderReviveServerRpc(NetworkConnection sender=null)
    {
        SynchronizeActors();
        var world=GetComponent<CoopWorld>();
        if(Defeated||world==null||!world.Game.Cooperative||world.Game.Dungeon.IsCleared||sender==null||!actors.TryGetValue(sender.ClientId,out var actor)||actor==null||actor.Alive)return;
        state.CancelRevive(sender.ClientId);
        if(!state.SetAlive(sender.ClientId,true))return;
        PlaceholderRevives++;actor.ResetHealth(actor.MaximumHealth); actor.GrantInvulnerability(2);
    }
    [ObserversRpc(BufferLast=true)]
    public void BroadcastDefeatObserversRpc()
    {
        Defeated=true; StopAllCoroutines(); state.CancelAllRevives(); DefeatReceived?.Invoke();
    }
}
