using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public sealed partial class RuntimeSmokeTest : MonoBehaviour
{
    private string output;
    private bool failed;
    private readonly System.Collections.Generic.List<string> runtimeErrors=new System.Collections.Generic.List<string>();
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Init()
    {
        if(Array.IndexOf(Environment.GetCommandLineArgs(),"-cookieSmoke")>=0&&Array.IndexOf(Environment.GetCommandLineArgs(),"-dedicatedServer")<0) new GameObject("RuntimeSmokeTest").AddComponent<RuntimeSmokeTest>();
    }
    private void Awake()
    {
        Application.runInBackground=true;
        // Hidden test windows must retain their synthetic keyboard state when focus changes.
        InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
        var args=Environment.GetCommandLineArgs(); int i=Array.IndexOf(args,"-cookieOutput");
        output=i>=0&&i+1<args.Length ? args[i+1] : Application.persistentDataPath;
        Directory.CreateDirectory(output); Application.logMessageReceived+=OnLog;
        if(Array.IndexOf(args,"-accountFlowTest")>=0&&Array.IndexOf(args,"-accountResume")<0)AccountClient.Forget();
    }
    private void OnLog(string message,string stack,LogType type) { if(type==LogType.Exception||type==LogType.Error||type==LogType.Assert){failed=true;runtimeErrors.Add(type+": "+message+"\n"+stack);} }
    private IEnumerator Start()
    {
        yield return new WaitForSecondsRealtime(1);
        yield return new WaitForEndOfFrame(); Capture("character.png");
        var game=FindFirstObjectByType<ArenaGame>();
        if(game==null) { Finish("Missing arena"); yield break; }
        if(Array.IndexOf(Environment.GetCommandLineArgs(),"-startupUnavailableTest")>=0){yield return TestUnavailableStartup();yield break;}
        if(Array.IndexOf(Environment.GetCommandLineArgs(),"-accountFlowTest")>=0){yield return TestAccountFlow(game);yield break;}
        var creator=FindFirstObjectByType<CharacterCreatorUI>();
        if(Array.IndexOf(Environment.GetCommandLineArgs(),"-uiLayoutTest")>=0)yield return TestCreatorLayouts(creator);
        creator.Preview.Apply(new CharacterAppearanceData { SkinIndex=2,HairStyle=2,HairColor=1,ClassId="wayfarer",PassiveSkillId="steadfast" });
        game.Begin(creator.Preview.Appearance);
        var arguments=Environment.GetCommandLineArgs();
        if(Array.IndexOf(arguments,"-bossArtTest")>=0){yield return TestBossArt();yield break;}
        if(Array.IndexOf(arguments,"-showRiftDefense")>=0)
        {
            game.Defense.StartPreviewShowcase();
            yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(output,"rift-defense-preview.png"));
            yield break;
        }
        if(Array.IndexOf(arguments,"-riftDefenseTest")>=0)
        {
            string defenseResult=Path.Combine(output,"rift-defense-test.txt");if(File.Exists(defenseResult))File.Delete(defenseResult);
            game.Defense.StartSmokeTest();
            double deadline=Time.realtimeSinceStartupAsDouble+55;
            while(Time.realtimeSinceStartupAsDouble<deadline&&runtimeErrors.Count==0)
            {
                if(File.Exists(defenseResult))break;
                yield return null;
            }
            string result=File.Exists(defenseResult)?File.ReadAllText(defenseResult):"FAIL smoke test timed out";
            Finish(result+" runtimeErrors="+failed);yield break;
        }
        if(Array.IndexOf(arguments,"-uiLayoutTest")>=0){yield return TestInventoryLayouts(game);yield break;}
        if(Array.IndexOf(arguments,"-cookieCoopHost")>=0||Array.IndexOf(arguments,"-cookieCoopClient")>=0)
        {
            yield return TestCoop(game,Array.IndexOf(arguments,"-cookieCoopHost")>=0); yield break;
        }
        if(Array.IndexOf(arguments,"-connectionFailureTest")>=0)
        {
            double until=Time.realtimeSinceStartupAsDouble+15;while(GameObject.Find("ConnectionLost")==null&&Time.realtimeSinceStartupAsDouble<until)yield return null;
            var reconnect=GameObject.Find("Reconnect")?.GetComponent<UnityEngine.UI.Button>();bool shown=GameObject.Find("ConnectionLost")!=null&&reconnect!=null;
            bool unsafeSignOut=GameObject.Find("Sign in again")!=null||GameObject.Find("Sign out")!=null||GameObject.Find("AccountSignOut")!=null;
            yield return new WaitForEndOfFrame();Capture("connection-lost.png");reconnect?.onClick.Invoke();yield return new WaitForSecondsRealtime(.5f);
            Finish($"{(shown&&!unsafeSignOut&&!failed?"PASS":"FAIL")} missing server modal={shown} reconnectButton={reconnect!=null} unsafeSignOut={unsafeSignOut} runtimeErrors={failed}");yield break;
        }
        if(Array.IndexOf(arguments,"-dedicatedPair")>=0){yield return TestDedicatedPair(game,Array.IndexOf(arguments,"-pairLeader")>=0);yield break;}
        if(Array.IndexOf(arguments,"-dedicatedClientTest")>=0){yield return TestDedicated(game);yield break;}
        yield return TestCombatExpansion(game);
        yield return TestTown(game);
        game.StartExpedition();
        yield return new WaitForSecondsRealtime(0.5f);
        if(game.Player==null) { Finish("Player not created"); yield break; }
        var keyboard=InputSystem.AddDevice<Keyboard>();
        var customizer=game.Player.GetComponent<CharacterCustomizer>();
        Vector3 before=game.Player.transform.position;
        InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.D));
        double moveStart=Time.realtimeSinceStartupAsDouble, movementDeadline=moveStart+2;
        bool walkFrameObserved=false;
        while((game.Player.transform.position.x<=before.x+0.1f||Time.realtimeSinceStartupAsDouble<moveStart+.4)&&Time.realtimeSinceStartupAsDouble<movementDeadline)
        {
            InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.D));
            if(customizer.IsWalking&&customizer.WalkFrame==1)walkFrameObserved=true;
            yield return null;
            if(customizer.IsWalking&&customizer.WalkFrame==1)walkFrameObserved=true;
        }
        InputSystem.QueueStateEvent(keyboard,new KeyboardState());
        bool moved=game.Player.transform.position.x>before.x+0.1f;
        bool walkCycle=moved&&walkFrameObserved;
        Debug.Log($"MOVEMENT_CHECK before={before} after={game.Player.transform.position} key={keyboard.dKey.isPressed} controls={game.Player.GetComponent<PlayerController>().ControlsEnabled} velocity={game.Player.GetComponent<Rigidbody2D>().linearVelocity} body={game.Player.GetComponent<Rigidbody2D>().bodyType}");
        var controller=game.Player.GetComponent<PlayerController>();
        var heroRenderer=game.Player.transform.Find("Pixel-body")?.GetComponent<SpriteRenderer>();
        customizer.SetFacing(Vector2.left);bool facesLeft=heroRenderer!=null&&heroRenderer.flipX&&heroRenderer.sprite==IllustratedArt.Hero(customizer.Appearance.Race,3,customizer.WalkFrame);
        customizer.SetFacing(Vector2.up);var equippedWeapon=game.Player.transform.Find("EquippedWeapon");
        bool facesUp=heroRenderer!=null&&heroRenderer.sprite==IllustratedArt.Hero(customizer.Appearance.Race,2,customizer.WalkFrame)&&equippedWeapon!=null&&equippedWeapon.localPosition.y>0;
        customizer.SetFacing(Vector2.down);bool facesDown=heroRenderer!=null&&heroRenderer.sprite==IllustratedArt.Hero(customizer.Appearance.Race,0,customizer.WalkFrame);
        customizer.SetFacing(Vector2.right);bool facesRight=heroRenderer!=null&&!heroRenderer.flipX&&heroRenderer.sprite==IllustratedArt.Hero(customizer.Appearance.Race,1,customizer.WalkFrame);
        bool allRaceFrames=true;for(int race=0;race<9;race++)for(int direction=0;direction<4;direction++)for(int frame=0;frame<2;frame++)allRaceFrames&=IllustratedArt.Hero(race,direction,frame)!=null;
        bool directionalFacing=facesLeft&&facesUp&&facesDown&&facesRight&&allRaceFrames;
        bool dodged=controller.Dodge();bool dodgePose=customizer.CurrentAction==HeroActionPose.Dodge;float health=game.Player.Health;game.Player.Damage(10);
        bool immune=game.Player.Health==health;
        var skills=controller.Skills;bool cast=skills.TryCastActive(0,game.Player.GetComponent<CharacterStats>());bool skillPose=customizer.CurrentAction==HeroActionPose.Skill;
        bool cooldown=!skills.TryCastActive(0,game.Player.GetComponent<CharacterStats>());
        skills.Swap(); bool bank=skills.SecondaryActive;
        controller.SetUIAttack(true);yield return null;controller.SetUIAttack(false);bool attackPose=customizer.CurrentAction==HeroActionPose.Attack;
        yield return new WaitForEndOfFrame(); Capture("arena.png");
        yield return new WaitForSecondsRealtime(0.3f);
        bool actionPoseResets=customizer.CurrentAction==HeroActionPose.Idle;
        game.Player.Damage(1); bool expires=game.Player.Health<health;
        InputSystem.RemoveDevice(keyboard);
        foreach(var enemy in FindObjectsByType<EnemyBrain>(FindObjectsSortMode.None))if(enemy.Pattern==null)enemy.GetComponent<Combatant>().Damage(10000);
        bool bossRequired=!game.Dungeon.IsCleared&&game.Dungeon.StageBoss.Alive;
        game.StartExpedition();
        int initialCoins=game.Forge.State.Coins;
        bool loops=true,saveRecovery=false,bossOnly=true;var bossTypes=new System.Collections.Generic.HashSet<string>();
        for(int run=0;run<10;run++)
        {
            if(run>0)game.StartExpedition();
            for(int room=0;room<4;room++)
            {
                bossTypes.Add(game.Dungeon.StageBoss.GetComponent<EnemyBrain>().Pattern.Id);
                if(game.Dungeon.StageBoss.GetComponent<OverheadVitals>()==null)failed=true;
                string validPath=game.Forge.SavePath;
                bool injectFailure=run==0&&room==0;
                if(injectFailure)game.Forge.SavePath=Path.Combine(validPath,"unwritable.json");
                game.Dungeon.StageBoss.Damage(10000);
                bossOnly&=game.Dungeon.IsCleared&&Array.Exists(FindObjectsByType<EnemyBrain>(FindObjectsSortMode.None),enemy=>enemy.Pattern==null&&enemy.GetComponent<Combatant>().Alive);
                yield return null;
                if(injectFailure)
                {
                    var recovery=GameObject.Find("Retry reward save")?.GetComponent<UnityEngine.UI.Button>();
                    saveRecovery=recovery!=null&&game.Forge.State.Coins==initialCoins&&GameObject.Find("Next room")==null;
                    game.Forge.SavePath=validPath;
                    recovery?.onClick.Invoke();
                    // A repeated callback must not duplicate the room reward.
                    recovery?.onClick.Invoke();
                    yield return null;
                    saveRecovery&=game.Forge.State.Coins==initialCoins+15&&ProfileStore.Load(validPath).Coins==initialCoins+15;
                }
                var button=GameObject.Find(room==3?"Return to town":"Next room")?.GetComponent<UnityEngine.UI.Button>();
                if(button==null) { loops=false; break; } button.onClick.Invoke(); yield return null;
            }
            if(!loops||!game.Hub.IsOpen) { loops=false; break; }
        }
        if(bossTypes.Count!=4)failed=true;Debug.Log("BOSS_VARIETY_CHECK types="+bossTypes.Count);
        bool rewards=game.Forge.State.Coins==initialCoins+800;
        bool saved=ProfileStore.Load(game.Forge.SavePath).Coins==game.Forge.State.Coins&&ProfileStore.Load(game.Forge.SavePath).CampaignStagesCompleted==4;
        bool loot=true; for(int i=0;i<4;i++)loot&=game.Forge.State.Count(game.Items[i].Id,1)==10+((i==0||i==2)?1:0);
        yield return new WaitForEndOfFrame(); Capture("town.png");
        var inventory=game.GetComponent<InventoryModal>();inventory.Open();yield return null;
        Capture("inventory.png");
        bool inventoryUI=inventory.IsOpen&&GameObject.Find("ItemGrid")!=null&&Resources.Load<TMPro.TMP_FontAsset>("Pixel/PixelFont")!=null;
        inventory.SetBrowse(1,1);inventoryUI&=inventory.VisibleCount>0&&inventory.VisibleCategoryMatches(1);
        inventory.SetBrowse(4,2);inventoryUI&=inventory.VisibleCategoryMatches(4);
        inventory.SetBrowse(0,0);yield return null;Capture("inventory.png");inventory.Close();
        bool catalogue=game.Items.Length>=121;foreach(var item in game.Items)catalogue&=item.iconSprite!=null&&!string.IsNullOrEmpty(item.description)&&!string.IsNullOrEmpty(item.villageQuote);
        var cosmetics=game.Player.GetComponent<CharacterCustomizer>();
        var original=cosmetics.Appearance;bool races=true;
        for(int race=0;race<9;race++){var custom=original;custom.Race=race;custom.CustomColors=true;custom.SkinRGB=Color.magenta;custom.HairRGB=Color.cyan;custom.EyeRGB=Color.yellow;cosmetics.Apply(custom);races&=cosmetics.Appearance.Race==race&&cosmetics.Appearance.SkinRGB==Color.magenta;}
        cosmetics.Apply(original);
        var tierState=game.Forge.State.Copy();tierState.Coins=1000;tierState.Add(game.Items[0].Id,4,2);ProfileStore.Save(tierState,game.Forge.SavePath);game.Forge.Configure(tierState);game.Forge.Equip(game.Items[0].Id,4);
        bool tierEffects=game.Player.GetComponentInChildren<WeaponTrailVFX>().Tier==4;
        game.Forge.TryFuse(game.Items[0].Id,4,out _);tierEffects&=game.Player.GetComponentInChildren<WeaponTrailVFX>().Tier==5;
        inventory.Open();yield return new WaitForSecondsRealtime(.5f);Capture("mythic-inspector.png");inventory.Close();


        game.StartExpedition();
        var pause=game.GetComponent<PauseManager>(); pause.Pause();
        float simulation=Time.time; yield return new WaitForSecondsRealtime(0.5f);
        bool paused=Time.time==simulation; pause.Resume(); yield return new WaitForSecondsRealtime(3.2f);
        bool resumed=Time.timeScale==1;
        game.Player.Damage(10000); yield return null;
        game.ReviveWithAdPlaceholder(); bool campaignNoRevive=!game.Player.Alive&&GameObject.Find("Revive")==null;
        var retry=GameObject.Find("Return to town")?.GetComponent<UnityEngine.UI.Button>(); bool defeat=retry!=null; retry?.onClick.Invoke();
        bool passed=moved&&walkCycle&&directionalFacing&&dodgePose&&skillPose&&attackPose&&actionPoseResets&&dodged&&immune&&cast&&cooldown&&bank&&expires&&loops&&rewards&&saved&&saveRecovery&&bossOnly&&bossRequired&&inventoryUI&&catalogue&&races&&tierEffects&&loot&&campaignNoRevive&&paused&&resumed&&defeat&&!failed;
        Finish($"{(passed?"PASS":"FAIL")} movement={moved} walkCycle={walkCycle} directionalFacing={directionalFacing} attackPose={attackPose} skillPose={skillPose} dodgePose={dodgePose} actionPoseResets={actionPoseResets} 72RaceDirectionFrames={allRaceFrames} dodge={dodged} immune={immune} cooldown={cooldown} bank={bank} iframeExpires={expires} tenRuns={loops} rewards={rewards} saved={saved} saveRecovery={saveRecovery} bossOnly={bossOnly} bossRequired={bossRequired} inventoryUI={inventoryUI} catalogue={catalogue} races={races} tierEffects={tierEffects} loot={loot} campaignNoRevive={campaignNoRevive} pause={paused} resume={resumed} defeat={defeat} runtimeErrors={failed}");
    }
    private void Capture(string name)
    {
        var camera=Camera.main;
        var pixels=camera.GetComponent<UnityEngine.U2D.PixelPerfectCamera>(); bool pixelEnabled=pixels!=null&&pixels.enabled;
        if(pixels!=null) pixels.enabled=false;
        var oldRect=camera.rect; float oldSize=camera.orthographicSize; camera.rect=new Rect(0,0,1,1); camera.orthographicSize=5.625f;
        var canvases=FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach(var canvas in canvases) { canvas.renderMode=RenderMode.ScreenSpaceCamera; canvas.worldCamera=camera; canvas.planeDistance=1; }
        var target=new RenderTexture(1280,720,24); target.Create(); camera.targetTexture=target;
        Canvas.ForceUpdateCanvases(); camera.Render();
        var previous=RenderTexture.active; RenderTexture.active=target;
        var texture=new Texture2D(1280,720,TextureFormat.RGB24,false); texture.ReadPixels(new Rect(0,0,1280,720),0,0); texture.Apply();
        File.WriteAllBytes(Path.Combine(output,name),texture.EncodeToPNG());
        RenderTexture.active=previous; camera.targetTexture=null;
        camera.rect=oldRect; camera.orthographicSize=oldSize; if(pixels!=null) pixels.enabled=pixelEnabled;
        foreach(var canvas in canvases) canvas.renderMode=RenderMode.ScreenSpaceOverlay;
        target.Release(); Destroy(target); Destroy(texture);
    }
    private IEnumerator TestCoop(ArenaGame game,bool host)
    {
        if(host)game.Session.Host(); else { yield return new WaitForSecondsRealtime(2); game.Session.Join("127.0.0.1"); }
        var world=game.Session.World; double deadline=Time.realtimeSinceStartupAsDouble+25;
        while(Time.realtimeSinceStartupAsDouble<deadline&&(host?world.MemberCount<2:world.ReceivedSnapshots<3))yield return null;
        bool connected=host?world.MemberCount==2:world.ReceivedSnapshots>=3;
        if(!connected) { Finish("FAIL co-op connection timed out"); yield break; }
        bool input;
        if(host)
        {
            deadline=Time.realtimeSinceStartupAsDouble+10;
            while(!world.ServerSawRemoteInput&&Time.realtimeSinceStartupAsDouble<deadline)yield return null;
            input=world.ServerSawRemoteInput;
            CombatVisual.Pulse(Vector3.zero,2,GameUI.Gold,1);
            Projectile.Spawn(Vector2.zero,Vector2.right,1,7,game.Player);
            yield return new WaitForSecondsRealtime(2);
            yield return world.TestRevives();
            yield return new WaitForSecondsRealtime(2);
        }
        else
        {
            var keyboard=InputSystem.AddDevice<Keyboard>(); Vector3 before=game.Player.transform.position;
            InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.D)); yield return new WaitForSecondsRealtime(0.6f);
            InputSystem.QueueStateEvent(keyboard,new KeyboardState()); input=game.Player.transform.position.x>before.x+0.2f; InputSystem.RemoveDevice(keyboard);
            yield return new WaitForEndOfFrame(); Capture("coop.png");
            deadline=Time.realtimeSinceStartupAsDouble+25;
            bool clicked=false;
            while(!world.Raid.Defeated&&Time.realtimeSinceStartupAsDouble<deadline)
            {
                var revive=GameObject.Find("Revive")?.GetComponent<UnityEngine.UI.Button>();
                if(!clicked&&revive!=null){Capture("revive.png");revive.onClick.Invoke();clicked=true;}
                yield return null;
            }
            // Remain connected long enough for the host to verify late callbacks cannot undo a wipe.
            yield return new WaitForSecondsRealtime(4);
            while(!world.Raid.Defeated&&Time.realtimeSinceStartupAsDouble<deadline)yield return null;
        }
        bool wiped=world.Raid.Defeated;
        bool effects=host||world.ReceivedEffects>0&&world.ReceivedBolts>0;
        bool revives=host?world.TestRevivesPassed:!game.Player.Alive&&GameObject.Find("Revive")==null;
        bool lootSaved=game.Forge.State.Coins==45&&ProfileStore.Load(game.Forge.SavePath).Coins==45;
        Finish($"{(connected&&input&&wiped&&effects&&revives&&lootSaved&&!failed?"PASS":"FAIL")} co-op host={host} connected={connected} authoritativeInput={input} wipeBroadcast={wiped} remoteVisuals={effects} reviveRules={revives} lootSaved={lootSaved} runtimeErrors={failed}");
    }
    private IEnumerator TestDedicatedPair(ArenaGame game,bool leader)
    {
        var world=game.Session.World;double deadline=Time.realtimeSinceStartupAsDouble+15;
        while(!game.Session.Authenticated&&Time.realtimeSinceStartupAsDouble<deadline)yield return null;
        if(!game.Session.Authenticated){Finish("FAIL dedicated pair authentication");yield break;}
        yield return TestTown(game);
        world.TestTownReadyServerRpc();deadline=Time.realtimeSinceStartupAsDouble+10;
        while(world.TownTestsReady<2&&Time.realtimeSinceStartupAsDouble<deadline)yield return null;
        if(world.TownTestsReady<2){Finish("FAIL town pair readiness");yield break;}
        int coins=game.Forge.State.Coins;
        deadline=Time.realtimeSinceStartupAsDouble+12;
        while((game.Hub.IsOpen||world.ReceivedSnapshots<2)&&Time.realtimeSinceStartupAsDouble<deadline){if(leader)world.CommandServerRpc("coop");yield return new WaitForSecondsRealtime(.5f);}
        yield return new WaitForSecondsRealtime(.2f);
        var expedition=FindAnyObjectByType<ExpeditionHUD>();expedition.Refresh();
        bool bossVisible=expedition.BossVisible,partyVisible=expedition.PartyVisible;float bossFraction=expedition.BossFraction;int layoutFailuresBefore=layoutFailures.Count;
        AuditLayout(GameObject.Find("SkillHUD").transform,"network expedition HUD");
        bool hudReady=bossVisible&&partyVisible&&bossFraction>0&&layoutFailures.Count==layoutFailuresBefore;
        Debug.Log($"DEDICATED_HUD_CHECK boss={bossVisible} party={partyVisible} fraction={bossFraction:0.00} newLayoutFailures={layoutFailures.Count-layoutFailuresBefore}");
        if(layoutFailures.Count>layoutFailuresBefore)Debug.Log("DEDICATED_HUD_LAYOUT: "+string.Join(" | ",layoutFailures.GetRange(layoutFailuresBefore,layoutFailures.Count-layoutFailuresBefore)));
        yield return new WaitForEndOfFrame();Capture("party-hud.png");
        if(leader)
        {
            world.TestDownPeerServerRpc();deadline=Time.realtimeSinceStartupAsDouble+10;
            while(world.TestSelfRevives<1&&Time.realtimeSinceStartupAsDouble<deadline)yield return null;
            world.TestDownPeerServerRpc();yield return new WaitForSecondsRealtime(.5f);
            world.Raid.RequestReviveServerRpc(world.TestVictimId);deadline=Time.realtimeSinceStartupAsDouble+6;
            while(world.TestRescues<1&&Time.realtimeSinceStartupAsDouble<deadline)yield return null;
            world.TestBossServerRpc();
        }
        else
        {
            bool clicked=false;deadline=Time.realtimeSinceStartupAsDouble+22;
            while(world.TestRescues<1&&Time.realtimeSinceStartupAsDouble<deadline){var button=GameObject.Find("Revive")?.GetComponent<UnityEngine.UI.Button>();if(!clicked&&button!=null){button.onClick.Invoke();clicked=true;}yield return null;}
        }
        deadline=Time.realtimeSinceStartupAsDouble+8;while(game.Forge.State.Coins!=coins+15&&Time.realtimeSinceStartupAsDouble<deadline)yield return null;
        deadline=Time.realtimeSinceStartupAsDouble+3;
        while(expedition.BossVisible&&Time.realtimeSinceStartupAsDouble<deadline){yield return new WaitForSecondsRealtime(.1f);expedition.Refresh();}
        bool bossHidden=!expedition.BossVisible;hudReady&=bossHidden;
        Debug.Log($"DEDICATED_BOSS_HIDE_CHECK hidden={bossHidden} stageCleared={game.Dungeon.IsCleared} replicaAlive={world.ReplicaBoss!=null&&world.ReplicaBoss.Alive}");
        bool passed=hudReady&&game.Cooperative&&world.TestSelfRevives>=1&&world.TestRescues>=1&&game.Forge.State.Coins==coins+15&&!failed;
        if(leader)yield return new WaitForSecondsRealtime(1);
        Finish($"{(passed?"PASS":"FAIL")} dedicated pair leader={leader} hud={hudReady} selfRevives={world.TestSelfRevives} teammateRevives={world.TestRescues} reward={game.Forge.State.Coins==coins+15} runtimeErrors={failed}");
    }
    private IEnumerator TestDedicated(ArenaGame game)
    {
        double deadline=Time.realtimeSinceStartupAsDouble+15;
        while(!game.Session.Authenticated&&Time.realtimeSinceStartupAsDouble<deadline)yield return null;
        if(!game.Session.Authenticated){Finish("FAIL dedicated authentication");yield break;}
        yield return TestTown(game);
        int coins=game.Forge.State.Coins;bool ownedByServer=false;
        try{game.Forge.Grant(game.Items[0].Id,100000);}catch(InvalidOperationException){ownedByServer=true;}
        game.StartExpedition();
        deadline=Time.realtimeSinceStartupAsDouble+10;while((game.Hub.IsOpen||game.Session.World.ReceivedSnapshots<2)&&Time.realtimeSinceStartupAsDouble<deadline)yield return null;
        var pause=game.GetComponent<PauseManager>();pause.Pause();yield return new WaitForSecondsRealtime(.5f);bool serverPause=game.Session.World.NetworkPaused;pause.Resume();yield return new WaitForSecondsRealtime(3.5f);serverPause&=!game.Session.World.NetworkPaused;
        bool campaign=!game.Cooperative,rooms=true;
        for(int stage=0;stage<4;stage++)
        {
            game.Session.World.TestBossServerRpc();
            deadline=Time.realtimeSinceStartupAsDouble+8;
            string buttonName=stage==3?"Return to town":"Next room";
            while((GameObject.Find(buttonName)==null||game.Forge.State.Coins!=coins+(stage==3?80:(stage+1)*15))&&Time.realtimeSinceStartupAsDouble<deadline)yield return null;
            var button=GameObject.Find(buttonName)?.GetComponent<UnityEngine.UI.Button>();Debug.Log($"DEDICATED_STAGE_CHECK stage={stage} coins={game.Forge.State.Coins} expected={coins+(stage==3?80:(stage+1)*15)} button={button!=null} cleared={game.Dungeon.IsCleared} snapshots={game.Session.World.ReceivedSnapshots}");if(button==null){rooms=false;break;}button.onClick.Invoke();
            yield return new WaitForSecondsRealtime(.5f);
        }
        bool rewards=game.Forge.State.Coins==coins+80&&game.Forge.State.CampaignStagesCompleted==4;
        var inventory=game.GetComponent<InventoryModal>();inventory.Open();yield return null;Capture("dedicated-inventory.png");inventory.Close();
        game.Session.World.RankingsServerRpc();deadline=Time.realtimeSinceStartupAsDouble+5;while(GameObject.Find("Leaderboard")==null&&Time.realtimeSinceStartupAsDouble<deadline)yield return null;
        bool rankings=GameObject.Find("Leaderboard")!=null;yield return new WaitForEndOfFrame();Capture("rankings.png");
        Finish($"{(rankings&&serverPause&&ownedByServer&&campaign&&rooms&&rewards&&!failed?"PASS":"FAIL")} dedicated auth={game.Session.Authenticated} serverOnlyGrants={ownedByServer} campaign={campaign} fourStages={rooms} centralRewards={rewards} serverPause={serverPause} rankings={rankings} runtimeErrors={failed}");
    }
    private void Finish(string message)
    {
        File.WriteAllText(Path.Combine(output,"runtime-smoke.txt"),message+(runtimeErrors.Count==0?"":"\n\nRuntime errors:\n"+string.Join("\n---\n",runtimeErrors))); Debug.Log("RUNTIME_SMOKE: "+message); Application.Quit(message.StartsWith("PASS")?0:1);
    }
    private void OnDestroy() { Application.logMessageReceived-=OnLog; }
}
