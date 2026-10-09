using System;
using TMPro;
using UnityEngine;
public sealed class ArenaGame : MonoBehaviour
{
    public GameObject CharacterPrefab;
    public ClassData Class;
    public Sprite Slime,Skeleton;
    public BossPatternSO Boss;
    public ItemData[] Items;
    public TextAsset Stages;
    public Combatant Player { get; private set; }
    public ItemTierUpManager Forge { get; private set; }
    public DungeonManager Dungeon { get; private set; }
    public TownHubManager Hub { get; private set; }
    public RiftDefenseMode Defense { get; private set; }
    public CoopSession Session;
    public bool Cooperative { get; private set; }
    private CharacterCreatorUI creator;
    private CharacterCustomizer appearance;
    private PlayerController controller;
    private TMP_Text status;
    private RectTransform result;
    private RectTransform reviveOffer;
    private bool begun,finished;
    private int rewardedStage=-1;
    private InventoryState profile;
    private string savePath;
    private void Start()
    {
        PixelPresentation.Configure(Camera.main);
        if(Camera.main.GetComponent<WorldFollowCamera>()==null)Camera.main.gameObject.AddComponent<WorldFollowCamera>();
        Application.runInBackground=true;
        Application.targetFrameRate=60;
        var canvas=GameUI.Canvas("ArenaStatus"); status=GameUI.Label(canvas,"",new Vector2(80,249),new Vector2(430,48),20);
        bool test=Array.IndexOf(Environment.GetCommandLineArgs(),"-cookieSmoke")>=0;
        if(test) savePath=System.IO.Path.Combine(Application.temporaryCachePath,"smoke-"+Guid.NewGuid().ToString("N")+".json");
        try { profile=Session.UsesDedicated?new InventoryState():ProfileStore.Load(savePath); }
        catch(Exception error) { status.text=EnglishScreens.SaveRecoveryNeeded+error.Message; return; }
        var go=Instantiate(CharacterPrefab); appearance=go.GetComponent<CharacterCustomizer>(); go.transform.position=new Vector3(-4.5f,-2.4f,0); go.transform.localScale=Vector3.one*1.5f;
        appearance.PresentationScale=1;appearance.Apply(profile.Appearance);
        creator=gameObject.AddComponent<CharacterCreatorUI>(); creator.Preview=appearance; creator.InitialAppearance=profile.Appearance; creator.Confirmed+=Begin;
        if(AccountClient.Enabled){var splash=gameObject.AddComponent<SplashScreenUI>();splash.Game=this;}
    }
    public void ApplyAccount(InventoryState state)
    {
        profile=state;appearance.Apply(state.Appearance);
        if(state.Appearance.CustomColors)Begin(state.Appearance);
        else creator.Root.gameObject.SetActive(true);
    }
    private void Update()
    {
        bool show=Cooperative&&Session!=null&&Session.Active&&Session.World.ClientReady&&Player!=null&&!Player.Alive&&!Session.World.Raid.Defeated&&!Dungeon.IsCleared;
        if(show&&reviveOffer==null)
        {
            reviveOffer=GameUI.Canvas("CoopReviveOffer");
            GameUI.Label(reviveOffer,EnglishScreens.YouAreDownWaitForAFriend,new Vector2(0,160),new Vector2(900,60),24);
            GameUI.Button(reviveOffer,EnglishScreens.Revive,new Vector2(0,90),new Vector2(250,50),ReviveWithAdPlaceholder);
        }
        else if(!show&&reviveOffer!=null) { Destroy(reviveOffer.gameObject); reviveOffer=null; }
    }
    public void ReviveWithAdPlaceholder()
    {
        if(!Cooperative||Session==null||!Session.Active||!Session.World.ClientReady||Player.Alive||Session.World.Raid.Defeated)return;
        ShowReviveAdPlaceholder();
        Session.World.Raid.RequestPlaceholderReviveServerRpc();
    }
    // Intentionally blank: no ad provider, delay, charge or external navigation.
    private void ShowReviveAdPlaceholder() { }
    public void Begin(CharacterAppearanceData selection)
    {
        if(begun)return;
        try
        {
            profile.Appearance=selection;
            if(profile.Items.Count==0) { profile.Coins=30; foreach(int index in new[] {0,2,4}) { profile.Add(Items[index].Id,1); profile.EquippedIds[index/2]=Items[index].Id; } }
            if(!Session.UsesDedicated)ProfileStore.Save(profile,savePath);
        }
        catch(Exception error) { status.text=EnglishScreens.CouldNotSaveCharacter+error.Message; return; }
        begun=true; creator.Root.gameObject.SetActive(false); appearance.transform.position=Vector3.zero; appearance.transform.localScale=Vector3.one;appearance.PresentationScale=.375f;appearance.Apply(selection);
        Items=MergeCatalogue(Items);
        var stats=appearance.gameObject.AddComponent<CharacterStats>(); stats.Health.Base=Class.BaseHealth+Class.Passive.FlatHealth; stats.Damage.Base=Class.BaseDamage; stats.Damage.Percent=Class.Passive.DamagePercent;
        Player=appearance.gameObject.AddComponent<Combatant>(); Player.IsPlayer=true; Player.Died+=Defeat;
        var collider=appearance.gameObject.AddComponent<CircleCollider2D>(); collider.radius=0.28f; collider.offset=new Vector2(0,0.3f);
        var skills=appearance.gameObject.AddComponent<SkillStanceSwapper>(); for(int i=0;i<3;i++) { skills.Primary[i]=Class.StartingSpells[i]; skills.Secondary[i]=Class.StartingSpells[i+3]; }
        controller=appearance.gameObject.AddComponent<PlayerController>(); controller.Skills=skills;
        Forge=gameObject.AddComponent<ItemTierUpManager>(); Forge.Definitions=Items; Forge.Customizer=appearance; Forge.Stats=stats; Forge.SavePath=savePath; Forge.Configure(profile);
        Player.ResetHealth(stats.MaxHealth);
        var inventory=gameObject.AddComponent<InventoryModal>();inventory.Game=this;
        var hud=gameObject.AddComponent<SkillWheelHUD>(); hud.Player=controller; hud.Health=Player; hud.Skills=skills;
        Dungeon=gameObject.AddComponent<DungeonManager>(); Dungeon.Configuration=Stages; Dungeon.Slime=Slime; Dungeon.Skeleton=Skeleton; Dungeon.Boss=Boss; Dungeon.Player=Player; Dungeon.RoomCleared+=ClearRoom;
        Hub=gameObject.AddComponent<TownHubManager>(); Hub.Forge=Forge; Hub.ExpeditionRequested+=StartExpedition;
        Hub.Session=Session;
        Defense=gameObject.AddComponent<RiftDefenseMode>();Defense.Configure(this);
        var events=gameObject.AddComponent<EventDrawerUI>();events.Session=Session;
        var inbox=gameObject.AddComponent<InboxDrawerUI>();inbox.Session=Session;
        var pause=gameObject.AddComponent<PauseManager>(); pause.PauseChanged+=paused=>{controller.ControlsEnabled=!paused&&(Hub.IsOpen||!finished);if(Session.UsesDedicated&&!Session.DedicatedServer&&!Cooperative&&Session.World.ClientReady)Session.World.CommandServerRpc(paused?"pause":"resume");};
        ReturnToTown();
        if(Session.UsesDedicated&&!Session.DedicatedServer){Forge.ServerTransaction=(action,id,tier)=>Session.World.TransactionServerRpc(action,id,tier);Session.ConnectCentral();}
    }
    private ItemData[] MergeCatalogue(ItemData[] starter)
    {
        var all=new System.Collections.Generic.List<ItemData>(starter);all.AddRange(Resources.LoadAll<ItemData>("Catalog"));return all.ToArray();
    }
    public void StartExpedition()
    {
        Defense?.Close();
        if(Session.UsesDedicated&&!Session.DedicatedServer){Session.World.CommandServerRpc("campaign");return;}
        rewardedStage=-1;
        Hub.Close(); Player.InSafeZone=false; ClearResult(); finished=false; controller.ControlsEnabled=true; Player.ResetHealth(Forge.Stats.MaxHealth); controller.Skills.ResetCooldowns(); Dungeon.StartRun(); RefreshStage();
    }
    // Stage progress is already pinned in the objective panel. Keep the combat
    // lane free of a large transient title over enemies and skill effects.
    private void RefreshStage()=>status.text="";
    private void ClearRoom(bool final)
    {
        if(Session.DedicatedServer){Session.World.PersistStage(final);return;}
        finished=true; controller.ControlsEnabled=false;
        ItemData reward=Items[Dungeon.CurrentStage.rewardItem];
        try
        {
            if(rewardedStage!=Dungeon.StageIndex)
            {
                Forge.Grant(reward.Id,Dungeon.CurrentStage.rewardCoins,Cooperative?0:Dungeon.StageIndex+1);
                rewardedStage=Dungeon.StageIndex;
                if(Cooperative&&Session.IsHost)Session.World.RewardRemote(reward.Id,Dungeon.CurrentStage.rewardCoins);
            }
            status.text="";
        }
        catch(Exception error)
        {
            Debug.LogError("Campaign reward save failed: "+error);
            var panel=CreateOutcomePanel("RewardRecovery",EnglishScreens.RewardSaveNotConfirmed,EnglishScreens.RewardSaveRecoveryBody);
            var body=panel.Find("OutcomeSubtitle");if(body!=null)body.name="RewardSaveRecoveryBody";
            GameUI.Label(panel,"NO REWARD WAS CONFIRMED",new Vector2(0,5),new Vector2(520,36),20).alignment=TMPro.TextAlignmentOptions.Center;
            var retry=GameUI.Button(panel,EnglishScreens.RetryRewardSave,new Vector2(0,-142),new Vector2(310,58),()=>ClearRoom(final));
            retry.gameObject.name=EnglishScreens.RetryRewardSave;
            return;
        }
        var rewardPanel=CreateOutcomePanel("RoomResult",final?EnglishScreens.CampaignComplete:EnglishScreens.StageCleared,$"STAGE {Dungeon.StageIndex+1} / {Dungeon.StageCount}");
        AddRewardDetails(rewardPanel,reward,Dungeon.CurrentStage.rewardCoins,EnglishScreens.RewardSaved);
        string action=final?EnglishScreens.ReturnToTown:"Next room";
        GameUI.Button(rewardPanel,action,new Vector2(0,-164),new Vector2(320,60),()=>
        {
            ClearResult(); if(final) { if(Cooperative)Session.Leave(); else ReturnToTown(); } else { finished=false; controller.ControlsEnabled=true; Dungeon.NextRoom(); RefreshStage(); }
        });
    }
    private void Defeat(Combatant player)
    {
        if(Cooperative||finished)return;
        finished=true; controller.ControlsEnabled=false; status.text="";
        var panel=CreateOutcomePanel("Defeat",EnglishScreens.RunEnded,$"DEFEATED AT STAGE {Dungeon.StageIndex+1} / {Dungeon.StageCount}");
        var safe=GameUI.Label(panel,EnglishScreens.SavedGearSafe,new Vector2(0,12),new Vector2(520,52),21);safe.alignment=TMPro.TextAlignmentOptions.Center;safe.gameObject.name="SavedGearSafe";
        GameUI.Button(panel,EnglishScreens.ReturnToTown,new Vector2(0,-142),new Vector2(320,60),ReturnToTown);
    }

    private RectTransform CreateOutcomePanel(string canvasName,string title,string subtitle)
    {
        ClearResult(); result=GameUI.Canvas(canvasName);
        var blocker=result.gameObject.AddComponent<UnityEngine.UI.Image>();blocker.color=new Color32(8,7,17,218);blocker.raycastTarget=true;
        var panel=GameUI.Panel(result,"OutcomePanel",Vector2.zero,new Vector2(720,460));panel.GetComponent<UnityEngine.UI.Image>().raycastTarget=false;
        var heading=GameUI.Label(panel,title,new Vector2(0,174),new Vector2(620,46),32);heading.alignment=TMPro.TextAlignmentOptions.Center;heading.color=GameUI.Gold;heading.gameObject.name="OutcomeTitle";
        var sub=GameUI.Label(panel,subtitle,new Vector2(0,132),new Vector2(620,34),20);sub.alignment=TMPro.TextAlignmentOptions.Center;sub.gameObject.name="OutcomeSubtitle";
        return panel;
    }

    private static void AddRewardDetails(RectTransform panel,ItemData item,int coins,string statusLabel)
    {
        var itemFrame=GameUI.Panel(panel,"RewardItemFrame",new Vector2(-194,10),new Vector2(150,150));
        Color rarityColor=PixelArt.Rarity((int)item.rarity+1);
        itemFrame.GetComponent<UnityEngine.UI.Image>().color=Color.Lerp(Color.white,rarityColor,.22f);
        var icon=GameUI.Icon(itemFrame,item.iconSprite,Vector2.zero,new Vector2(112,112));icon.gameObject.name="RewardItemIcon";
        var border=itemFrame.gameObject.AddComponent<ItemBorderVFX>();border.Tier=(int)item.rarity+1;border.EnhancementLevel=0;
        var rarity=GameUI.Label(panel,item.rarity.ToString().ToUpperInvariant()+" REWARD",new Vector2(92,94),new Vector2(340,30),18);rarity.alignment=TMPro.TextAlignmentOptions.Center;rarity.color=rarityColor;
        var name=GameUI.Label(panel,item.DisplayName,new Vector2(92,50),new Vector2(350,48),26);name.alignment=TMPro.TextAlignmentOptions.Center;name.textWrappingMode=TMPro.TextWrappingModes.Normal;name.overflowMode=TMPro.TextOverflowModes.Ellipsis;name.gameObject.name="RewardItemName";
        var count=GameUI.Label(panel,"× 1",new Vector2(92,13),new Vector2(180,30),20);count.alignment=TMPro.TextAlignmentOptions.Center;
        GameUI.Panel(panel,"RewardDivider",new Vector2(92,-20),new Vector2(350,3)).GetComponent<UnityEngine.UI.Image>().color=new Color32(114,101,145,210);
        GameUI.Icon(panel,PixelArt.Icon("gold"),new Vector2(-31,-63),new Vector2(34,34)).gameObject.name="RewardGoldIcon";
        var gold=GameUI.Label(panel,$"+{coins:N0} GOLD",new Vector2(136,-63),new Vector2(230,38),25);gold.alignment=TMPro.TextAlignmentOptions.Center;gold.color=GameUI.Gold;gold.gameObject.name="RewardCoinsValue";
        var saved=GameUI.Label(panel,statusLabel,new Vector2(0,-111),new Vector2(520,28),17);saved.alignment=TMPro.TextAlignmentOptions.Center;saved.color=new Color32(132,235,154,255);saved.gameObject.name="RewardSavedStatus";
    }
    public void ReturnToTown()
    {
        GetComponent<InventoryModal>()?.Close(); ClearResult(); Dungeon.Clear(); finished=false; Player.InSafeZone=true; controller.ControlsEnabled=!Session.UsesDedicated||Session.DedicatedServer||Session.Authenticated; Player.transform.position=new Vector3(0,-1.5f,0); Player.ResetHealth(Forge.Stats.MaxHealth);
        status.text=""; Hub.Open();
    }
    private void ClearResult() { if(result!=null){result.gameObject.SetActive(false);Destroy(result.gameObject);} result=null; }
    public void SetStatus(string message){if(status!=null)status.text=message;}
    public void PrepareCoop(bool client)
    {
        Cooperative=true; Player.InSafeZone=false; Hub.Close(); ClearResult(); finished=false;
        GetComponent<PauseManager>().NetworkSession=true;
        controller.ControlsEnabled=client; controller.ForwardInputs=client; Player.SimulationEnabled=!client;
        controller.InputSubmitted-=ForwardInput;
        if(client)controller.InputSubmitted+=ForwardInput;
        status.text=EnglishScreens.WaitingForTheParty;
    }
    private void ForwardInput(PlayerInputFrame input) { if(Session!=null&&Session.Active&&Session.World!=null&&Session.World.ClientReady&&Session.World.ReceivedSnapshots>0)Session.World.InputServerRpc(input); }
    public void EndCoop()
    {
        Cooperative=false; controller.InputSubmitted-=ForwardInput; controller.ForwardInputs=false; Player.SimulationEnabled=true;
        GetComponent<PauseManager>().NetworkSession=false; ReturnToTown();
    }
    public void BindDedicatedPlayer(Combatant actor)
    {
        Player=actor;controller=actor.GetComponent<PlayerController>();appearance=actor.GetComponent<CharacterCustomizer>();Dungeon.Player=actor;
    }
    public void NetworkStageEntered(){GetComponent<InventoryModal>()?.Close();Hub.Close();Player.InSafeZone=false;ClearResult();finished=false;}
    public void SetNetworkMode(bool cooperative){Cooperative=cooperative;Player.InSafeZone=false;GetComponent<InventoryModal>()?.Close();Hub.Close();ClearResult();finished=false;GetComponent<PauseManager>().NetworkSession=cooperative;}
    public void ReceiveProfile(InventoryState state)
    {
        Forge.Configure(state);if(Hub.IsOpen)Hub.Refresh();GetComponent<InventoryModal>()?.Refresh();
    }
    public void ShowNetworkRewardRetry()
    {
        status.text="";var panel=CreateOutcomePanel("NetworkRewardRetry",EnglishScreens.RewardSaveNotConfirmed,"The server could not confirm every reward yet.");
        GameUI.Label(panel,"Retry uses the same run receipt, so saved rewards are not granted twice.",new Vector2(0,17),new Vector2(560,50),18).alignment=TMPro.TextAlignmentOptions.Center;
        GameUI.Button(panel,EnglishScreens.RetrySave,new Vector2(0,-142),new Vector2(320,60),()=>Session.World.CommandServerRpc("retry"));
    }
    public void ShowNetworkReward(bool final,string message)
    {
        finished=true;controller.ControlsEnabled=false;status.text="";
        var reward=Items[Dungeon.CurrentStage.rewardItem];var panel=CreateOutcomePanel("NetworkReward",final?EnglishScreens.CampaignComplete:EnglishScreens.StageCleared,$"STAGE {Dungeon.StageIndex+1} / {Dungeon.StageCount}");
        AddRewardDetails(panel,reward,Dungeon.CurrentStage.rewardCoins,string.IsNullOrWhiteSpace(message)?EnglishScreens.RewardSaved:message);
        string action=final?EnglishScreens.ReturnToTown:"Next room";
        GameUI.Button(panel,action,new Vector2(0,-164),new Vector2(320,60),()=>Session.World.CommandServerRpc(final?"town":"next"));
    }
    public Combatant CreateRemotePlayer()
    {
        var go=Instantiate(CharacterPrefab); go.name="RemoteWayfarer"; go.transform.localScale=Vector3.one;
        go.GetComponent<CharacterCustomizer>().Apply(new CharacterAppearanceData { SkinIndex=2,HairColor=1 });
        var stats=go.AddComponent<CharacterStats>(); stats.Health.Base=Class.BaseHealth+Class.Passive.FlatHealth; stats.Damage.Base=Class.BaseDamage;
        var actor=go.AddComponent<Combatant>(); actor.IsPlayer=true; actor.ResetHealth(stats.MaxHealth);
        var collider=go.AddComponent<CircleCollider2D>(); collider.radius=0.28f; collider.offset=new Vector2(0,0.3f);
        var skills=go.AddComponent<SkillStanceSwapper>(); for(int i=0;i<3;i++){skills.Primary[i]=Class.StartingSpells[i];skills.Secondary[i]=Class.StartingSpells[i+3];}
        var remote=go.AddComponent<PlayerController>(); remote.Skills=skills; remote.RemoteControlled=true;
        return actor;
    }
    public void ShowPartyDefeat()
    {
        finished=true; controller.ControlsEnabled=false; status.text="";
        var panel=CreateOutcomePanel("PartyDefeat",EnglishScreens.PartyRunEnded,$"PARTY DEFEATED AT STAGE {Dungeon.StageIndex+1} / {Dungeon.StageCount}");
        var outcome=GameUI.Label(panel,"All revives are cancelled. Rewards from cleared stages are saved.",new Vector2(0,12),new Vector2(560,56),20);outcome.alignment=TMPro.TextAlignmentOptions.Center;outcome.gameObject.name="PartyDefeatDetails";
        GameUI.Button(panel,EnglishScreens.LeaveRaid,new Vector2(0,-142),new Vector2(320,60),()=>{if(Session.UsesDedicated)Session.World.CommandServerRpc("town");else Session.Leave();});
    }
    public void SetReplicaState(bool cleared,bool defeated)
    {
        if(defeated){controller.ControlsEnabled=false;return;}
        bool paused=GetComponent<PauseManager>().IsPaused;
        controller.ControlsEnabled=!cleared&&!paused&&!(GetComponent<InventoryModal>()?.IsOpen??false);
        status.text=cleared?"Room cleared · waiting for host to continue":$"Co-op · {Dungeon.StageName} · F near a fallen friend to revive";
    }
}
