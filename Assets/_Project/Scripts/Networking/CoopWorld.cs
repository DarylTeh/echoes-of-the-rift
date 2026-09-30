using System;
using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Object;
using UnityEngine;
using UnityEngine.InputSystem;

[Serializable] public struct RaidActorSnapshot
{
    public int Id;
    public Vector2 Position;
    public float Health,Maximum,Mana;
    public bool IsPlayer,IsBoss,Secondary;
    public int Sprite;
    public float[] Cooldowns;
    public CharacterAppearanceData Appearance;
    public int WeaponTier;
    public string WeaponFamily;
}

[Serializable] public struct RaidBoltSnapshot
{
    public string Family;
    public int Id;
    public Vector2 Position;
    public Color Color;
}

public sealed partial class CoopWorld : NetworkBehaviour
{
    public ArenaGame Game;
    public NetworkRaidManager Raid;
    public int MemberCount=>members.Count;
    public int ReceivedSnapshots { get; private set; }
    public bool ClientReady { get; private set; }
    public int ReceivedEffects { get; private set; }
    public int ReceivedBolts { get; private set; }
    public bool ServerSawRemoteInput { get; private set; }
    public Combatant ReplicaBoss { get; private set; }
    public void CopyHudAllies(List<Combatant> destination)
    {
        destination.Clear();if(!ClientReady)return;
        if(IsServerInitialized){foreach(var actor in members.Values)if(actor!=null&&actor!=Game.Player)destination.Add(actor);}
        else foreach(var view in replicas.Values){if(view==null)continue;var actor=view.GetComponent<Combatant>();if(actor!=null&&actor.IsPlayer)destination.Add(actor);}
    }
    private readonly Dictionary<int,Combatant> members=new Dictionary<int,Combatant>();
    private readonly Dictionary<int,GameObject> replicas=new Dictionary<int,GameObject>();
    private readonly Dictionary<int,GameObject> bolts=new Dictionary<int,GameObject>();
    private readonly Dictionary<int,float[]> cooldownBuffers=new Dictionary<int,float[]>();
    private RaidActorSnapshot[] actorSnapshotBuffer=Array.Empty<RaidActorSnapshot>();
    private RaidBoltSnapshot[] boltSnapshotBuffer=Array.Empty<RaidBoltSnapshot>();
    private readonly HashSet<int> presentReplicaIds=new HashSet<int>();
    private readonly List<int> removedReplicaIds=new List<int>();
    private readonly HashSet<int> presentBoltIds=new HashSet<int>();
    private readonly List<int> removedBoltIds=new List<int>();
    private double nextSnapshot;
    private readonly Dictionary<EnemyBrain,int> enemyIds=new Dictionary<EnemyBrain,int>();
    private int nextEnemyId=1000,enemyIdStage=-99;
    private int replicaStage=-1;
    private bool running;
    public override void OnStartServer() { members.Clear(); running=false; Raid.DefeatReceived+=OnDefeat; CombatVisual.Created+=BroadcastEffect;CombatVisual.Swung+=BroadcastSwing; }
    public override void OnStartClient() { ClientReady=true; ReceivedSnapshots=0; if(!IsServerInitialized)Raid.DefeatReceived+=OnDefeat; if(Game.Session.UsesDedicated)StartCoroutine(LoginWhenReady());else ReadyServerRpc(); }
    public override void OnStopClient() { ClientReady=false; ReceivedSnapshots=0; }
    private void OnDefeat()=>Game.ShowPartyDefeat();
    public override void OnStopNetwork()
    {
        Raid.DefeatReceived-=OnDefeat;
        CombatVisual.Created-=BroadcastEffect;CombatVisual.Swung-=BroadcastSwing;
        foreach(var actor in members.Values)if(actor!=null&&(Game.Session.DedicatedServer||actor!=Game.Player))Destroy(actor.gameObject);
        members.Clear(); ClearReplicas(); running=false;
    }
    [ServerRpc(RequireOwnership=false)]
    private void ReadyServerRpc(NetworkConnection sender=null)
    {
        if(Game.Session.DedicatedServer)return;
        if(sender==null||members.ContainsKey(sender.ClientId)||members.Count>=2||running)return;
        bool host=sender.ClientId==LocalConnection.ClientId;
        Combatant actor=host?Game.Player:Game.CreateRemotePlayer();
        if(actor==null)return;
        members.Add(sender.ClientId,actor); Raid.RegisterServerPlayer(sender.ClientId,actor);
        if(members.Count==2)
        {
            running=true; Game.StartExpedition(); int index=0;
            foreach(var member in members.Values) { member.transform.position=new Vector3(index++*1.5f,-2,0); member.GetComponent<PlayerController>().ControlsEnabled=true; }
        }
    }
    public void RemoveMember(int id)
    {
        if(!IsServerInitialized||!members.TryGetValue(id,out var actor))return;
        accounts.Remove(id); members.Remove(id); cooldownBuffers.Remove(id); Raid.RemoveServerPlayer(id); if(actor!=null&&(Game.Session.DedicatedServer||actor!=Game.Player))Destroy(actor.gameObject);
        if(Game.Session.DedicatedServer&&members.Count==0){Time.timeScale=1;running=false;Game.Dungeon.Clear();Raid.ResetServerRaid();}
        if(running)Game.SetStatus("A friend disconnected. You can finish this expedition solo.");
    }
    [ServerRpc(RequireOwnership=false)]
    public void InputServerRpc(PlayerInputFrame frame,NetworkConnection sender=null)
    {
        if((running&&Raid.Defeated)||sender==null||!members.TryGetValue(sender.ClientId,out var actor)||(!Game.Session.DedicatedServer&&actor==Game.Player))return;
        actor.GetComponent<PlayerController>().SubmitRemote(frame);
        if(frame.Move.sqrMagnitude>0.1f)ServerSawRemoteInput=true;
    }
    private void Update()
    {
        if(IsClientInitialized&&Game.Cooperative&&Keyboard.current?.fKey.wasPressedThisFrame==true)
        {
            int target=-1; float distance=1.8f;
            if(IsServerInitialized) { foreach(var pair in members)if(!pair.Value.Alive&&Vector2.Distance(Game.Player.transform.position,pair.Value.transform.position)<distance)target=pair.Key; }
            else { foreach(var pair in replicas) { var actor=pair.Value.GetComponent<Combatant>(); if(actor!=null&&actor.IsPlayer&&!actor.Alive&&Vector2.Distance(Game.Player.transform.position,actor.transform.position)<distance)target=pair.Key; } }
            if(target>=0)Raid.RequestReviveServerRpc(target);
        }
        if(!IsServerInitialized||members.Count==0||(!running&&!Game.Session.DedicatedServer)||Time.realtimeSinceStartupAsDouble<nextSnapshot)return;
        nextSnapshot=Time.realtimeSinceStartupAsDouble+0.05;
        if(Game.Session.DedicatedServer&&Array.IndexOf(Environment.GetCommandLineArgs(),"-dedicatedTest")>=0)TestCountersObserversRpc(Raid.PlaceholderRevives,Raid.TeammateRevives,TestVictimId);
        var enemies=FindObjectsByType<EnemyBrain>(FindObjectsSortMode.None);
        int actorCount=members.Count+enemies.Length;
        if(actorSnapshotBuffer.Length!=actorCount)actorSnapshotBuffer=new RaidActorSnapshot[actorCount];
        int snapshotIndex=0;
        foreach(var pair in members)
        {
            var actor=pair.Value; var skills=actor.GetComponent<SkillStanceSwapper>();
            if(!cooldownBuffers.TryGetValue(pair.Key,out var cooldowns)){cooldowns=new float[6];cooldownBuffers.Add(pair.Key,cooldowns);}
            for(int i=0;i<6;i++)cooldowns[i]=skills.Remaining(i);
            actorSnapshotBuffer[snapshotIndex++]=new RaidActorSnapshot { Id=pair.Key,IsPlayer=true,Position=actor.transform.position,Health=actor.Health,Maximum=actor.MaximumHealth,Secondary=skills.SecondaryActive,Cooldowns=cooldowns,Mana=skills.Mana,Appearance=actor.GetComponent<CharacterCustomizer>().Appearance,WeaponTier=actor.GetComponent<CharacterCustomizer>().EquipmentTier,WeaponFamily=actor.GetComponentInChildren<WeaponTrailVFX>()?.Family };
        }
        if(enemyIdStage!=Game.Dungeon.StageIndex){enemyIds.Clear();nextEnemyId=1000;enemyIdStage=Game.Dungeon.StageIndex;}
        foreach(var enemy in enemies)
        {
            var actor=enemy.GetComponent<Combatant>(); var renderer=enemy.GetComponent<SpriteRenderer>();
            if(!enemyIds.TryGetValue(enemy,out int enemyId)){enemyId=nextEnemyId++;enemyIds.Add(enemy,enemyId);}
            actorSnapshotBuffer[snapshotIndex++]=new RaidActorSnapshot { Id=enemyId,Position=enemy.transform.position,Health=actor.Health,Maximum=actor.MaximumHealth,IsBoss=enemy.Pattern!=null,Sprite=renderer.sprite==Game.Slime?0:1 };
        }
        SnapshotObserversRpc(actorSnapshotBuffer,snapshotIndex,running?Game.Dungeon.StageIndex:-1,Game.Dungeon.IsCleared,Raid.Defeated);
        var projectiles=FindObjectsByType<Projectile>(FindObjectsSortMode.None);
        if(boltSnapshotBuffer.Length!=projectiles.Length)boltSnapshotBuffer=new RaidBoltSnapshot[projectiles.Length];
        for(int i=0;i<projectiles.Length;i++){var bolt=projectiles[i];boltSnapshotBuffer[i]=new RaidBoltSnapshot { Family=bolt.VisualFamily,Id=bolt.VisualId,Position=bolt.transform.position,Color=bolt.GetComponent<SpriteRenderer>().color };}
        BoltsObserversRpc(boltSnapshotBuffer,projectiles.Length);
    }
    private void BroadcastSwing(Vector3 position,Vector2 aim,float radius,float arc,Color tint){if(IsServerInitialized&&running)SwingObserversRpc(position,aim,radius,arc,tint);}
    [ObserversRpc] private void SwingObserversRpc(Vector3 position,Vector2 aim,float radius,float arc,Color tint){if(!IsServerInitialized)CombatVisual.Slash(position,aim,radius,arc,tint);}
    private void BroadcastEffect(Vector3 position,float radius,Color color,float duration)
    {
        if(IsServerInitialized&&running)EffectObserversRpc(position,radius,color,duration);
    }
    [ObserversRpc]
    private void EffectObserversRpc(Vector3 position,float radius,Color color,float duration)
    {
        if(IsServerInitialized)return;
        ReceivedEffects++; CombatVisual.Pulse(position,radius,color,duration);
    }
    [ObserversRpc]
    private void BoltsObserversRpc(RaidBoltSnapshot[] snapshots,int snapshotCount)
    {
        if(IsServerInitialized)return;
        presentBoltIds.Clear();
        for(int i=0;i<snapshotCount&&i<snapshots.Length;i++)
        {
            var snapshot=snapshots[i];
            presentBoltIds.Add(snapshot.Id);
            if(!bolts.TryGetValue(snapshot.Id,out var go))
            {
                go=new GameObject("RemoteSpellBolt",typeof(SpriteRenderer)); bolts.Add(snapshot.Id,go); ReceivedBolts++;
                go.transform.localScale=new Vector3(.55f,.35f,1);
                var renderer=go.GetComponent<SpriteRenderer>(); renderer.sprite=ArcaneBoltVFX.Sprite; renderer.sortingOrder=8;go.AddComponent<ArcaneBoltVFX>();
            }
            NetworkPresentation.Apply(go,snapshot.Position); var visual=go.GetComponent<SpriteRenderer>();visual.color=snapshot.Color;
            if(!string.IsNullOrEmpty(snapshot.Family)){visual.sprite=IllustratedArt.Weapon(snapshot.Family)??ArcaneBoltVFX.Sprite;visual.sharedMaterial=IllustratedArt.World;go.transform.localScale=Vector3.one*.55f;}
        }
        removedBoltIds.Clear();
        foreach(var pair in bolts)if(!presentBoltIds.Contains(pair.Key)){Destroy(pair.Value);removedBoltIds.Add(pair.Key);}
        foreach(int id in removedBoltIds)bolts.Remove(id);
    }
    [ObserversRpc]
    private void SnapshotObserversRpc(RaidActorSnapshot[] snapshots,int snapshotCount,int stage,bool cleared,bool defeated)
    {
        if(IsServerInitialized||Game.Player==null||(Game.Session.UsesDedicated&&!Game.Session.Authenticated))return; ReceivedSnapshots++;
        bool stageChanged=replicaStage!=stage;
        if(stage>=0&&stageChanged) { ClearReplicas(); Game.Dungeon.LoadReplicaStage(stage); Game.NetworkStageEntered(); replicaStage=stage; }
        replicaStage=stage;
        presentReplicaIds.Clear();ReplicaBoss=null;
        for(int i=0;i<snapshotCount&&i<snapshots.Length;i++)
        {
            var snapshot=snapshots[i];
            if(snapshot.IsPlayer&&snapshot.Id==LocalConnection.ClientId)
            {
                NetworkPresentation.Apply(Game.Player.gameObject,snapshot.Position,stageChanged); Game.Player.ApplySnapshot(snapshot.Health,snapshot.Maximum);
                Game.Player.GetComponent<SkillStanceSwapper>().ApplySnapshot(snapshot.Secondary,snapshot.Cooldowns,snapshot.Mana); continue;
            }
            presentReplicaIds.Add(snapshot.Id);
            if(!replicas.TryGetValue(snapshot.Id,out var go))
            {
                if(snapshot.IsPlayer) { go=Instantiate(Game.CharacterPrefab); go.GetComponent<CharacterCustomizer>().Apply(new CharacterAppearanceData { SkinIndex=1,HairColor=2 }); }
                else { go=new GameObject("RaidEnemyReplica",typeof(SpriteRenderer)); }
                go.name="Replica-"+snapshot.Id; var actor=go.AddComponent<Combatant>(); actor.IsPlayer=snapshot.IsPlayer; actor.SimulationEnabled=false; replicas[snapshot.Id]=go;
            }
            NetworkPresentation.Apply(go,snapshot.Position);
            go.GetComponent<Combatant>().ApplySnapshot(snapshot.Health,snapshot.Maximum);
            if(snapshot.IsBoss)ReplicaBoss=go.GetComponent<Combatant>();
            if(snapshot.IsPlayer){var customizer=go.GetComponent<CharacterCustomizer>();if(!customizer.Appearance.Equals(snapshot.Appearance))customizer.Apply(snapshot.Appearance);customizer.SetEquipmentTier(snapshot.WeaponTier);customizer.SetWeapon(snapshot.WeaponFamily,snapshot.WeaponTier);}
            if(!snapshot.IsPlayer) { var renderer=go.GetComponent<SpriteRenderer>(); renderer.sprite=snapshot.IsBoss?BossRoster.Sprite(stage):snapshot.Sprite==0?Game.Slime:Game.Skeleton; renderer.sortingOrder=1; go.transform.localScale=Vector3.one*(snapshot.IsBoss?2:1); }
        }
        removedReplicaIds.Clear(); foreach(var pair in replicas)if(!presentReplicaIds.Contains(pair.Key)){Destroy(pair.Value);removedReplicaIds.Add(pair.Key);} foreach(int id in removedReplicaIds)replicas.Remove(id);
        if(stage>=0){Game.Dungeon.SetReplicaCleared(cleared);Game.SetReplicaState(cleared,defeated);}
    }
    public void ClearReplicas() { ReplicaBoss=null;foreach(var go in replicas.Values)if(go!=null)Destroy(go); replicas.Clear(); foreach(var go in bolts.Values)if(go!=null)Destroy(go); bolts.Clear(); replicaStage=-1; }
    public void RewardRemote(string itemId,int coins)
    {
        if(!IsServerInitialized)return;
        foreach(var pair in members)if(pair.Value!=Game.Player&&ServerManager.Clients.TryGetValue(pair.Key,out var connection))RewardTargetRpc(connection,itemId,coins);
    }
    [TargetRpc]
    private void RewardTargetRpc(NetworkConnection target,string itemId,int coins)
    {
        try { Game.Forge.Grant(itemId,coins); } catch(Exception error) { Debug.LogError("Co-op reward save failed: "+error.Message); }
    }
    public bool TestRevivesPassed { get; private set; }
    public System.Collections.IEnumerator TestRevives()
    {
        if(!Debug.isDebugBuild||Array.IndexOf(Environment.GetCommandLineArgs(),"-cookieCoopHost")<0||!IsServerInitialized)yield break;
        Combatant remote=null; int remoteId=-1;
        foreach(var pair in members)if(pair.Value!=Game.Player){remote=pair.Value;remoteId=pair.Key;}
        if(remote==null)yield break;
        Game.Dungeon.StageBoss.Damage(10000);
        yield return new WaitForSecondsRealtime(0.5f);
        GameObject.Find("Next room").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
        foreach(var enemy in FindObjectsByType<EnemyBrain>(FindObjectsSortMode.None))enemy.GetComponent<Combatant>().SimulationEnabled=false;
        Game.Player.ResetHealth(Game.Player.MaximumHealth); remote.ResetHealth(remote.MaximumHealth);
        remote.Damage(10000);
        double deadline=Time.realtimeSinceStartupAsDouble+10;
        while(!remote.Alive&&Time.realtimeSinceStartupAsDouble<deadline)yield return null;
        bool placeholder=remote.Alive;
        remote.ResetHealth(remote.MaximumHealth); remote.Damage(10000);
        Game.Player.transform.position=remote.transform.position+Vector3.left;
        yield return new WaitForSecondsRealtime(0.2f);
        Raid.RequestReviveServerRpc(remoteId);
        yield return new WaitForSecondsRealtime(0.5f);
        Game.Player.transform.position=remote.transform.position+Vector3.left*5;
        yield return new WaitForSecondsRealtime(3.2f);
        bool cancelled=!remote.Alive&&Raid.ActiveRevives==0;
        Game.Player.transform.position=remote.transform.position+Vector3.left;
        Raid.RequestReviveServerRpc(remoteId);
        yield return new WaitForSecondsRealtime(3.5f);
        bool rescued=remote.Alive;
        remote.ResetHealth(remote.MaximumHealth); remote.Damage(10000);
        yield return new WaitForSecondsRealtime(0.2f);
        Raid.RequestReviveServerRpc(remoteId);
        yield return new WaitForSecondsRealtime(0.5f);
        Game.Player.Damage(10000);
        yield return new WaitForSecondsRealtime(3.5f);
        Raid.RequestPlaceholderReviveServerRpc();
        yield return new WaitForSecondsRealtime(0.2f);
        TestRevivesPassed=placeholder&&cancelled&&rescued&&Raid.Defeated&&!remote.Alive&&!Game.Player.Alive&&Raid.ActiveRevives==0;
        Debug.Log($"REVIVE_CHECK placeholder={placeholder} distanceCancel={cancelled} teammate={rescued} wipeCancels={Raid.Defeated&&Raid.ActiveRevives==0}");
    }
    public void TestWipe()
    {
        if(!Debug.isDebugBuild||Array.IndexOf(Environment.GetCommandLineArgs(),"-cookieCoopHost")<0||!IsServerInitialized)return;
        foreach(var actor in members.Values)actor.Damage(10000);
    }
}

