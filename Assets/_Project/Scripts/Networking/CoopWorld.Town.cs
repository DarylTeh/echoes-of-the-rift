using System;
using FishNet.Connection;
using FishNet.Object;
using UnityEngine;

public sealed partial class CoopWorld
{
    public bool TownSafetyVerified { get; private set; }
    private readonly System.Collections.Generic.HashSet<int> townTestsReady=new System.Collections.Generic.HashSet<int>();
    public int TownTestsReady { get; private set; }
    [ServerRpc(RequireOwnership=false)]
    public void TestTownReadyServerRpc(NetworkConnection sender=null)
    {
        if(!Debug.isDebugBuild||Array.IndexOf(Environment.GetCommandLineArgs(),"-dedicatedTest")<0||running||sender==null||!members.ContainsKey(sender.ClientId))return;
        townTestsReady.Add(sender.ClientId);TownTestReadyObserversRpc(townTestsReady.Count);
    }
    [ObserversRpc] private void TownTestReadyObserversRpc(int count){TownTestsReady=count;}
    [ServerRpc(RequireOwnership=false)]
    public void TestTownSafetyServerRpc(NetworkConnection sender=null)
    {
        if(!Debug.isDebugBuild||Array.IndexOf(Environment.GetCommandLineArgs(),"-dedicatedTest")<0||running||sender==null||!members.TryGetValue(sender.ClientId,out var actor))return;
        float health=actor.Health,mana=actor.GetComponent<SkillStanceSwapper>().Mana;
        actor.Damage(10000);
        var controller=actor.GetComponent<PlayerController>();controller.RequestSkill(0);
        bool protectedTown=actor.InSafeZone&&actor.Health==health&&!controller.Dodge()&&actor.GetComponent<SkillStanceSwapper>().Mana==mana&&FindObjectsByType<EnemyBrain>(FindObjectsSortMode.None).Length==0;
        TownSafetyTargetRpc(sender,protectedTown);
    }
    [TargetRpc]
    private void TownSafetyTargetRpc(NetworkConnection connection,bool result){TownSafetyVerified=result;}
}
