using TMPro;
using UnityEngine;

public sealed class SkillWheelHUD : MonoBehaviour
{
    public PlayerController Player;
    public SkillStanceSwapper Skills;
    public Combatant Health;
    private RectTransform root,quest;
    private ArenaGame game;private float nextRefresh;
    private static readonly string[] Keys={"Q","E","R"};
    private bool questExpanded=true;
    private CharacterStats stats;
    private WeaponTrailVFX weapon;
    private UnityEngine.UI.Image attackIcon;
    private readonly UnityEngine.UI.Image[] cooldowns=new UnityEngine.UI.Image[3];
    private readonly System.Collections.Generic.List<GameObject> combatControls=new System.Collections.Generic.List<GameObject>();
    private TMP_Text healthText,manaText,stanceText,objective,goldValue,gemValue,levelValue;
    private int shownGold=-1,shownGems=-1,shownLevel=-1;
    private int shownHealth=-1,shownMana=-1;
    private float shownHealthRatio=-1,shownManaRatio=-1;
    private string shownWeaponFamily;
    private readonly SpellData[] shownSpells=new SpellData[3];
    private UnityEngine.UI.Image hp,mp;
    private readonly TMP_Text[] labels=new TMP_Text[3];
    private readonly UnityEngine.UI.Image[] icons=new UnityEngine.UI.Image[3];
    private readonly int[] shownCooldownTenths={-1,-1,-1};
    private bool lastTown;
    private int lastStage=-1;
    private bool lastSecondary;
    private void Start()
    {
        game=FindAnyObjectByType<ArenaGame>();
        root=GameUI.Canvas("SkillHUD");
        stats=Player.GetComponent<CharacterStats>();weapon=Player.GetComponentInChildren<WeaponTrailVFX>();
        var portrait=GameUI.Panel(root,"Portrait",new Vector2(-572,304),new Vector2(72,72));
        GameUI.Pin(portrait,new Vector2(0,1),new Vector2(68,-56));
        var appearance=Player.GetComponent<CharacterCustomizer>().Appearance;
        GameUI.Icon(portrait,IllustratedArt.Hero(appearance.Race),Vector2.zero,new Vector2(64,64));
        levelValue=GameUI.Label(root,"",new Vector2(-554,256),new Vector2(116,24),18);levelValue.alignment=TextAlignmentOptions.Center;GameUI.Pin(levelValue.rectTransform,new Vector2(0,1),new Vector2(86,-104));
        goldValue=Wallet("GoldCounter","gold",new Vector2(478,321));
        gemValue=Wallet("GemCounter","gem",new Vector2(330,321));
        RefreshWallet();
        var status=GameUI.Panel(root,"Vitals",new Vector2(-414,304),new Vector2(232,72));
        GameUI.Pin(status,new Vector2(0,1),new Vector2(226,-56));
        healthText=GameUI.Label(status,"",new Vector2(0,22),new Vector2(208,24),18);
        hp=Bar(status,new Vector2(0,8),new Color32(91,190,113,255));mp=Bar(status,new Vector2(0,-25),new Color32(84,165,212,255));
        manaText=GameUI.Label(status,"",new Vector2(0,-9),new Vector2(208,24),18);
        stanceText=GameUI.Label(root,"",new Vector2(477,-92),new Vector2(258,26),18);
        lastSecondary=!Skills.SecondaryActive;stanceText.text=Skills.SecondaryActive?EnglishUI.MoonStance:EnglishUI.EmberStance;
        // Keep touch actions in a compact two-row cluster so right-thumb travel is short.
        // Desktop retains the wider keyboard-oriented arrangement used by the HUD.
        Vector2[] positions=FixedTouchStick.EnabledForDevice
            ?new[]{new Vector2(316,-260),new Vector2(414,-260),new Vector2(414,-162)}
            :new[]{new Vector2(389,-159),new Vector2(376,-268),new Vector2(487,-157)};
        for(int i=0;i<3;i++)
        {
            int slot=i;var button=GameUI.Button(root,"",positions[i],new Vector2(88,88),()=>Player.RequestSkill(slot));
            button.GetComponent<UnityEngine.UI.Image>().sprite=PixelArt.Orb();
            button.GetComponent<UnityEngine.UI.Image>().type=UnityEngine.UI.Image.Type.Simple;
            combatControls.Add(button.gameObject);
            icons[i]=GameUI.Icon(button.transform,null,new Vector2(0,5),new Vector2(58,58));
            cooldowns[i]=GameUI.Icon(button.transform,PixelArt.Orb(),Vector2.zero,new Vector2(80,80));
            cooldowns[i].type=UnityEngine.UI.Image.Type.Filled;cooldowns[i].fillMethod=UnityEngine.UI.Image.FillMethod.Radial360;cooldowns[i].fillOrigin=2;cooldowns[i].color=new Color(0.03f,0.02f,.09f,.7f);
            labels[i]=GameUI.Label(button.transform,"",new Vector2(0,-26),new Vector2(72,24),18);labels[i].alignment=TextAlignmentOptions.Center;
        }
        var swap=GameUI.Button(root,FixedTouchStick.EnabledForDevice?"SWAP":EnglishScreens.Tab,new Vector2(316,-162),new Vector2(88,88),Player.RequestSwap);GameUI.Icon(swap.transform,PixelArt.Icon("swap"),new Vector2(0,7),new Vector2(42,42));
        var dodge=GameUI.Button(root,FixedTouchStick.EnabledForDevice?"DODGE":EnglishScreens.Space,FixedTouchStick.EnabledForDevice?new Vector2(512,-162):new Vector2(273,-282),new Vector2(88,88),()=>Player.Dodge());GameUI.Icon(dodge.transform,PixelArt.Icon("dodge"),new Vector2(0,7),new Vector2(42,42));
        foreach(var button in new[]{swap,dodge}){var caption=button.GetComponentInChildren<TMP_Text>();caption.rectTransform.anchoredPosition=new Vector2(0,-27);caption.rectTransform.sizeDelta=new Vector2(80,20);caption.fontSize=FixedTouchStick.EnabledForDevice?14:16;}
        combatControls.Add(swap.gameObject);combatControls.Add(dodge.gameObject);combatControls.Add(stanceText.gameObject);
        var bag=GameUI.Button(root,"",new Vector2(576,240),new Vector2(64,64),()=>FindAnyObjectByType<InventoryModal>().Open());
        bag.name="BackpackButton";GameUI.Pin((RectTransform)bag.transform,Vector2.one,new Vector2(-64,-120));
        GameUI.Icon(bag.transform,PixelArt.Icon("bag"),new Vector2(0,6),new Vector2(40,40));
        GameUI.Label(bag.transform,"Bag [I]",new Vector2(0,-23),new Vector2(64,20),16).alignment=TextAlignmentOptions.Center;
        var attack=GameUI.Button(root,"",FixedTouchStick.EnabledForDevice?new Vector2(512,-260):new Vector2(523,-267),new Vector2(128,128),()=>{});
        attack.name="AttackControl";attack.GetComponent<UnityEngine.UI.Image>().sprite=PixelArt.Orb();attack.GetComponent<UnityEngine.UI.Image>().type=UnityEngine.UI.Image.Type.Simple;
        attackIcon=GameUI.Icon(attack.transform,null,Vector2.zero,new Vector2(82,82));
        combatControls.Add(attack.gameObject);
        if(FixedTouchStick.EnabledForDevice)AttachStick((RectTransform)attack.transform,"<Gamepad>/rightStick",36);
        else attack.gameObject.AddComponent<CombatAttackButton>().Player=Player;
        quest=GameUI.Panel(root,"QuestDrawer",new Vector2(-478,138),new Vector2(264,112));
        GameUI.Icon(quest,PixelArt.Icon("quest"),new Vector2(-104,30),new Vector2(28,28));
        objective=GameUI.Label(quest,"",new Vector2(6,-10),new Vector2(232,76),18);
        combatControls.Add(GameUI.Button(root,EnglishScreens.Objectives,new Vector2(-478,220),new Vector2(264,36),()=>questExpanded=!questExpanded).gameObject);
        if(FixedTouchStick.EnabledForDevice)Stick("MoveStick",new Vector2(-492,-245),"<Gamepad>/leftStick");
        root.gameObject.AddComponent<ExpeditionHUD>().Initialize(game,root);
    }
    private TMP_Text Wallet(string name,string icon,Vector2 position)
    {
        var panel=GameUI.Panel(root,name,position,new Vector2(140,38));panel.GetComponent<UnityEngine.UI.Image>().raycastTarget=false;GameUI.Pin(panel,Vector2.one,position-new Vector2(640,360));
        GameUI.Icon(panel,PixelArt.Icon(icon),new Vector2(-49,0),new Vector2(32,32));
        var label=GameUI.Label(panel,"",new Vector2(17,0),new Vector2(94,28),22);label.alignment=TextAlignmentOptions.MidlineRight;return label;
    }
    public void RefreshWallet()
    {
        if(goldValue==null||game?.Forge?.State==null)return;var state=game.Forge.State;
        if(shownGold!=state.Coins){shownGold=state.Coins;goldValue.text=EnglishUI.Compact(shownGold);}
        if(shownGems!=state.Gems){shownGems=state.Gems;gemValue.text=EnglishUI.Compact(shownGems);}
        if(shownLevel!=state.Level){shownLevel=state.Level;levelValue.text="Lv. "+EnglishUI.Compact(shownLevel);}
    }
    private UnityEngine.UI.Image Bar(Transform parent,Vector2 pos,Color color)
    {
        var border=GameUI.Panel(parent,"BarFrame",pos,new Vector2(212,12));var fill=GameUI.Icon(border,CombatVisual.Square,Vector2.zero,new Vector2(200,6));fill.preserveAspect=false;fill.color=color;fill.rectTransform.pivot=new Vector2(0,.5f);fill.rectTransform.anchoredPosition=new Vector2(-100,0);return fill;
    }
    private void Stick(string name,Vector2 position,string control)
    {
        var rect=GameUI.Panel(root,name,position,new Vector2(136,136));
        var image=rect.GetComponent<UnityEngine.UI.Image>();image.sprite=PixelArt.Orb();image.type=UnityEngine.UI.Image.Type.Simple;image.color=new Color(1,1,1,.45f);
        AttachStick(rect,control,42);
    }
    private void AttachStick(RectTransform background,string control,float radius)
    {
        var thumb=GameUI.Icon(background,PixelArt.Orb(),Vector2.zero,new Vector2(44,44));thumb.name="TouchThumb";thumb.color=new Color(.5f,.86f,1,.8f);
        var stick=background.gameObject.AddComponent<FixedTouchStick>();stick.Thumb=thumb.rectTransform;stick.Radius=radius;stick.Player=Player;stick.controlPath=control;
    }
    private void Update()
    {
        if(root==null||Time.unscaledTime<nextRefresh)return;
        nextRefresh=Time.unscaledTime+.05f;
        RefreshWallet();
        int healthValue=Mathf.CeilToInt(Health.Health),manaValue=Mathf.CeilToInt(Skills.Mana);
        if(shownHealth!=healthValue){shownHealth=healthValue;healthText.text=EnglishUI.Health(healthValue,Health.MaximumHealth);}
        if(shownMana!=manaValue){shownMana=manaValue;manaText.text=EnglishUI.Mana(manaValue);}
        float healthRatio=Mathf.Clamp01(Health.Health/Mathf.Max(1,Health.MaximumHealth)),manaRatio=Mathf.Clamp01(Skills.Mana/100);
        if(Mathf.Abs(shownHealthRatio-healthRatio)>.001f){shownHealthRatio=healthRatio;hp.rectTransform.sizeDelta=new Vector2(200*healthRatio,6);}
        if(Mathf.Abs(shownManaRatio-manaRatio)>.001f){shownManaRatio=manaRatio;mp.rectTransform.sizeDelta=new Vector2(200*manaRatio,6);}
        if(weapon!=null&&shownWeaponFamily!=weapon.Family){shownWeaponFamily=weapon.Family;attackIcon.sprite=IllustratedArt.Weapon(weapon.Family);attackIcon.material=IllustratedArt.Owns(attackIcon.sprite)?IllustratedArt.UI:null;}
        if(lastSecondary!=Skills.SecondaryActive){lastSecondary=Skills.SecondaryActive;stanceText.text=Skills.SecondaryActive?EnglishUI.MoonStance:EnglishUI.EmberStance;}
        for(int i=0;i<3;i++){int index=i+(Skills.SecondaryActive?3:0);var spell=Skills.GetSpell(index);float remaining=Skills.Remaining(index);if(shownSpells[i]!=spell){shownSpells[i]=spell;icons[i].sprite=IllustratedArt.Skill(spell)??spell?.skillIcon;icons[i].material=IllustratedArt.Owns(icons[i].sprite)?IllustratedArt.UI:null;}int tenths=remaining>0?Mathf.CeilToInt(remaining*10):0;if(shownCooldownTenths[i]!=tenths){shownCooldownTenths[i]=tenths;labels[i].text=tenths>0?(tenths/10f).ToString("0.0"):FixedTouchStick.EnabledForDevice?(i+1).ToString():Keys[i];}cooldowns[i].fillAmount=spell==null?0:Mathf.Clamp01(remaining/Mathf.Max(.01f,stats.CooldownDuration(spell.Cooldown)));}
        bool town=game.Hub.IsOpen;int stage=game.Dungeon.StageIndex;foreach(var control in combatControls)control.SetActive(!town);quest.gameObject.SetActive(!town&&questExpanded);if(lastTown!=town||lastStage!=stage){lastTown=town;lastStage=stage;objective.text=EnglishUI.Objective(town,stage+1);}
    }
    private void OnDestroy(){if(root!=null)Destroy(root.gameObject);}
}

