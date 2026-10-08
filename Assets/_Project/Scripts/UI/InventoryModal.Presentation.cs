using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public sealed partial class InventoryModal
{
    private RectTransform inspector,sortMenu,paperdollSlots,equipmentFrame,collectionFrame;
    private CanvasGroup browseGroup;
    private TMP_Text itemName,itemRarity,itemSummary,heroLevel,heroStats,emptyLabel;
    private UnityEngine.UI.Image selectedStars;
    private UnityEngine.UI.Button closeInspectorButton,closeCollectionButton;
    private UnityEngine.UI.Button[] categoryButtons;
    private GameObject returnFocus;
    private SpellData selectedSpell;
    public bool IsInspecting=>inspector!=null&&inspector.gameObject.activeSelf;
    public int CurrentPage=>page;
    public bool PaperdollShowsEquipped(EquipmentSlot slot)
    {
        var icon=paperdollSlots!=null?paperdollSlots.Find("Equipped"+slot+"/Icon")?.GetComponent<UnityEngine.UI.Image>():null;
        return icon!=null&&icon.sprite!=null;
    }

    private RectTransform FullScreenShade(string name,float opacity)
    {
        var shade=GameUI.Rect(name,root,Vector2.one*.5f,Vector2.zero,Vector2.zero);
        shade.anchorMin=Vector2.zero;shade.anchorMax=Vector2.one;
        // Extend beyond the safe-area offset so wider/taller screens have no bright strips.
        shade.sizeDelta=Vector2.one*256;
        shade.gameObject.AddComponent<UnityEngine.UI.Image>().color=new Color(0,0,0,opacity);
        return shade;
    }

    // Heavier double bevels for collection surfaces, while keeping slot interiors quiet.
    private static RectTransform CollectionPanel(Transform parent,string name,Vector2 at,Vector2 size)
    {
        var panel=GameUI.Panel(parent,name,at,size);
        panel.GetComponent<UnityEngine.UI.Image>().pixelsPerUnitMultiplier=1.25f;
        return panel;
    }
    private static UnityEngine.UI.Button RedClose(Transform parent,Vector2 at,Action close)
    {
        var button=GameUI.Button(parent,"X",at,new Vector2(46,46),close);
        button.GetComponent<UnityEngine.UI.Image>().color=new Color32(224,48,78,255);
        return button;
    }
    private void BuildCollection()
    {
        var browse=GameUI.Rect("CollectionBrowse",root,Vector2.one*.5f,Vector2.zero,new Vector2(1200,680));
        browseGroup=browse.gameObject.AddComponent<CanvasGroup>();
        // Match the reference's nearly full-height, balanced paperdoll / collection split.
        var left=CollectionPanel(browse,"EquipmentFrame",new Vector2(-254,0),new Vector2(520,650));
        var right=CollectionPanel(browse,"CollectionFrame",new Vector2(274,0),new Vector2(520,650));
        equipmentFrame=left;collectionFrame=right;
        // Align the heading's left edge with the paperdoll frame, not the side rail.
        title=GameUI.Label(browse,"BACKPACK",new Vector2(-290,316),new Vector2(440,36),28);
        inventoryGems=BuildCurrency(browse,"gem",new Vector2(286,316));inventoryGold=BuildCurrency(browse,"gold",new Vector2(455,316));
        closeCollectionButton=RedClose(browse,new Vector2(568,316),Close);
        var bag=GameUI.Button(browse,"Bag",new Vector2(-558,215),new Vector2(76,78),()=>SwitchCollection(false,0));
        GameUI.Icon(bag.transform,PixelArt.Icon("bag"),new Vector2(0,12),new Vector2(36,36));
        bag.GetComponentInChildren<TMP_Text>().rectTransform.anchoredPosition=new Vector2(0,-22);
        var skills=GameUI.Button(browse,"Skills",new Vector2(-558,127),new Vector2(76,78),()=>SwitchCollection(false,4));
        GameUI.Icon(skills.transform,PixelArt.Icon("book"),new Vector2(0,12),new Vector2(36,36));skills.GetComponentInChildren<TMP_Text>().rectTransform.anchoredPosition=new Vector2(0,-22);
        var shop=GameUI.Button(browse,"Shop",new Vector2(-558,39),new Vector2(76,78),()=>SwitchCollection(true,0));
        GameUI.Icon(shop.transform,PixelArt.Icon("gold"),new Vector2(0,12),new Vector2(36,36));shop.GetComponentInChildren<TMP_Text>().rectTransform.anchoredPosition=new Vector2(0,-22);
        var header=GameUI.Label(left,"YOUR HERO",new Vector2(0,278),new Vector2(460,32),24);header.alignment=TextAlignmentOptions.Center;
        BuildPreview(left);
        paperdollSlots=GameUI.Rect("EquippedSlots",left,Vector2.one*.5f,Vector2.zero,new Vector2(420,500));
        heroLevel=GameUI.Label(left,"",new Vector2(0,-43),new Vector2(270,32),20);heroLevel.gameObject.name="HeroProfileLevel";heroLevel.alignment=TextAlignmentOptions.Center;heroLevel.color=new Color32(137,218,199,255);
        var stance=GameUI.Label(left,"MOON  /  EMBER",new Vector2(0,-94),new Vector2(390,26),18);stance.alignment=TextAlignmentOptions.Center;
        var stats=CollectionPanel(left,"EquipmentTotals",new Vector2(0,-222),new Vector2(400,68));
        heroStats=GameUI.Label(stats,"",Vector2.zero,new Vector2(364,54),18);heroStats.gameObject.name="HeroCombatSummary";heroStats.alignment=TextAlignmentOptions.Center;
        categoryButtons=new UnityEngine.UI.Button[Filters.Length];
        for(int i=0;i<Filters.Length;i++){int category=i;categoryButtons[i]=GameUI.Button(right,Filters[i],new Vector2(-196+i*98,292),new Vector2(94,40),()=>SetBrowse(category,sorting));categoryButtons[i].GetComponentInChildren<TMP_Text>().fontSize=16;}
        sortButton=GameUI.Button(right,"Sort",new Vector2(152,236),new Vector2(180,38),OpenSort);
        sortButton.GetComponentInChildren<TMP_Text>().fontSize=16;
        total=GameUI.Label(right,"",new Vector2(-100,236),new Vector2(260,30),17);
        total.textWrappingMode=TextWrappingModes.NoWrap;
        grid=GameUI.Rect("ItemGrid",right,Vector2.one*.5f,new Vector2(0,-40),new Vector2(500,470));
        emptyLabel=GameUI.Label(right,EnglishUI.EmptyCategory,new Vector2(0,-40),new Vector2(420,40),22);emptyLabel.alignment=TextAlignmentOptions.Center;
        previousButton=GameUI.Button(right,"<",new Vector2(-208,-286),new Vector2(68,40),()=>{page=Mathf.Max(0,page-1);Refresh();});
        nextButton=GameUI.Button(right,">",new Vector2(208,-286),new Vector2(68,40),()=>{page++;Refresh();});
        var hint=GameUI.Label(right,"Tap an item to inspect",new Vector2(0,-286),new Vector2(300,32),17);hint.alignment=TextAlignmentOptions.Center;
        BuildInspector();
    }
    private void SwitchCollection(bool shop,int category)
    {
        shopping=shop;merchant=null;SetBrowse(category,sorting);
    }
    private void LayoutCollection()
    {
        equipmentFrame.gameObject.SetActive(!shopping);
        collectionFrame.anchoredPosition=new Vector2(shopping?0:274,0);
        collectionFrame.sizeDelta=new Vector2(shopping?1040:520,650);
        previousButton.GetComponent<RectTransform>().anchoredPosition=new Vector2(shopping?-442:-208,-286);
        nextButton.GetComponent<RectTransform>().anchoredPosition=new Vector2(shopping?442:208,-286);
    }
    private void RefreshPaperdoll()
    {
        foreach(Transform child in paperdollSlots)Destroy(child.gameObject);
        var state=Game.Forge.State;string className=string.IsNullOrWhiteSpace(state.Appearance.ClassId)?"HERO":state.Appearance.ClassId.Replace('_',' ').ToUpperInvariant();heroLevel.text=$"{className}  /  Lv. {EnglishUI.Compact(state.Level)}";
        float attack=0,health=0;
        Vector2[] positions={new Vector2(-158,127),new Vector2(158,127),new Vector2(158,8)};
        for(int i=0;i<3;i++)
        {
            var item=Array.Find(Game.Items,x=>x.Id==state.EquippedIds[i]);int tier=state.EquippedTiers[i];int enhancement=state.EquippedEnhancementLevels!=null&&i<state.EquippedEnhancementLevels.Length?state.EquippedEnhancementLevels[i]:tier;
            var slot=GameUI.Panel(paperdollSlots,"Equipped"+(EquipmentSlot)i,positions[i],new Vector2(94,94));
            slot.GetComponent<UnityEngine.UI.Image>().sprite=PixelArt.Frame(true);slot.GetComponent<UnityEngine.UI.Image>().pixelsPerUnitMultiplier=1.8f;
            var caption=GameUI.Label(paperdollSlots,item!=null?((EquipmentSlot)i).ToString():"Empty "+(EquipmentSlot)i,positions[i]+new Vector2(0,-60),new Vector2(112,24),14);caption.alignment=TextAlignmentOptions.Center;
            if(item!=null){var equippedLabel=GameUI.Label(paperdollSlots,"EQUIPPED",positions[i]+new Vector2(0,-79),new Vector2(94,18),11);equippedLabel.alignment=TextAlignmentOptions.Center;equippedLabel.color=new Color32(147,226,155,255);}
            if(item==null)continue;
            attack+=item.FlatDamage*enhancement;health+=item.FlatHealth*enhancement;
            var rarityBorder=slot.gameObject.AddComponent<ItemBorderVFX>();rarityBorder.Tier=(int)item.rarity+1;rarityBorder.EnhancementLevel=enhancement;
            GameUI.Icon(slot,IllustratedArt.Item(item),new Vector2(0,5),new Vector2(74,74));
            var enhancementBadge=GameUI.Panel(slot,"EnhancementBadge",new Vector2(27,34),new Vector2(42,22));enhancementBadge.GetComponent<UnityEngine.UI.Image>().color=new Color32(31,24,43,245);
            var enhancementText=GameUI.Label(enhancementBadge,ProgressionRules.EnhancementBadge(enhancement),Vector2.zero,new Vector2(38,20),13);enhancementText.alignment=TextAlignmentOptions.Center;enhancementText.color=GameUI.Cream;
            var stack=new ItemStack{ItemId=item.Id,Tier=tier,EnhancementLevel=enhancement,Count=state.Count(item.Id,tier,enhancement)};
            slot.gameObject.AddComponent<UnityEngine.UI.Button>().onClick.AddListener(()=>{shopping=false;Refresh();Inspect(stack);});
        }
        var activeSkills=Game.Player.GetComponent<SkillStanceSwapper>();
        for(int i=0;i<6;i++)
        {
            string id=state.SkillIds!=null&&i<state.SkillIds.Length?state.SkillIds[i]:null;
            var activeSpell=activeSkills!=null?activeSkills.GetSpell(i):null;
            if(string.IsNullOrEmpty(id))id=activeSpell!=null?activeSpell.Id:null;
            var item=Array.Find(Game.Items,x=>x.Spell!=null&&x.Spell.Id==id);
            var slot=GameUI.Panel(paperdollSlots,"EquippedSkill"+i,new Vector2(-163+i*65,-145),new Vector2(58,62));
            slot.GetComponent<UnityEngine.UI.Image>().sprite=PixelArt.Frame(true);
            if(item==null&&activeSpell==null)continue;
            GameUI.Icon(slot,item!=null?IllustratedArt.Item(item):IllustratedArt.Skill(activeSpell),new Vector2(0,4),new Vector2(48,48));
            var hotkey=GameUI.Label(slot,new[]{"Q","E","R"}[i%3],new Vector2(0,-21),new Vector2(36,18),14);hotkey.alignment=TextAlignmentOptions.Center;
            if(item==null){slot.gameObject.AddComponent<UnityEngine.UI.Button>().onClick.AddListener(()=>{showLore=false;InspectStarter(activeSpell);});continue;}
            var owned=state.Items.Find(x=>x.ItemId==item.Id&&x.Count>0)??new ItemStack{ItemId=item.Id,Tier=item.itemTier,Count=0};
            slot.gameObject.AddComponent<UnityEngine.UI.Button>().onClick.AddListener(()=>{shopping=false;Refresh();Inspect(owned);});
        }
        var stats=Game.Player.GetComponent<CharacterStats>();
        heroStats.text=$"OFFENSE {stats.AttackDamage:0}       SURVIVAL {stats.MaxHealth:0}\nGEAR ATK +{attack:0.##}    HP +{health:0.##}";
    }
    private void BuildInspector()
    {
        inspector=FullScreenShade("ItemInspector",.72f);
        // Keep the item details and their actions together in one centered card.
        // This follows the dense, inspect-then-act pattern used by MyHeroes while
        // giving the effect text enough room to remain legible on laptop displays.
        var panel=CollectionPanel(inspector,"ItemDetails",Vector2.zero,new Vector2(900,560));
        var ribbon=CollectionPanel(panel,"RarityRibbon",new Vector2(0,258),new Vector2(274,38));ribbon.GetComponent<UnityEngine.UI.Image>().color=new Color32(154,80,160,255);
        selectedStars=GameUI.Icon(ribbon,PixelArt.RarityStars(ItemRarity.Mythic),new Vector2(-54,0),new Vector2(82,16));selectedStars.name="InspectorRarityStars";selectedStars.preserveAspect=false;
        itemRarity=GameUI.Label(ribbon,"",new Vector2(76,0),new Vector2(112,30),20);itemRarity.alignment=TextAlignmentOptions.Center;
        closeInspectorButton=RedClose(panel,new Vector2(426,256),CloseInspector);
        var frame=GameUI.Panel(panel,"SelectedItemFrame",new Vector2(-344,174),new Vector2(124,124));frame.GetComponent<UnityEngine.UI.Image>().sprite=PixelArt.Frame(true);
        selectedBorder=frame.gameObject.AddComponent<ItemBorderVFX>();selectedIcon=GameUI.Icon(frame,null,Vector2.zero,new Vector2(100,100));
        itemName=GameUI.Label(panel,"",new Vector2(-70,211),new Vector2(360,52),28);
        itemSummary=GameUI.Label(panel,"",new Vector2(-70,156),new Vector2(360,66),21);
        var effectPanel=CollectionPanel(panel,"EffectPanel",new Vector2(-43,6),new Vector2(602,230));
        var effectHeading=GameUI.Label(effectPanel,"EFFECT / COMPARISON",new Vector2(-20,94),new Vector2(550,24),16);effectHeading.color=new Color32(174,162,212,255);
        description=GameUI.Label(effectPanel,"",new Vector2(0,-8),new Vector2(550,166),20);description.richText=false;description.alignment=TextAlignmentOptions.TopLeft;
        var upgradePanel=CollectionPanel(panel,"UpgradePanel",new Vector2(-43,-174),new Vector2(602,82));
        upgradeDetails=GameUI.Label(upgradePanel,"",Vector2.zero,new Vector2(550,64),18);upgradeDetails.color=new Color32(140,220,170,255);upgradeDetails.alignment=TextAlignmentOptions.Center;
        var actionRail=CollectionPanel(panel,"ItemActionRail",new Vector2(337,-25),new Vector2(174,382));
        var actionHeading=GameUI.Label(actionRail,"ACTIONS",new Vector2(0,151),new Vector2(150,26),16);actionHeading.color=new Color32(174,162,212,255);actionHeading.alignment=TextAlignmentOptions.Center;
        actionButton=GameUI.Button(actionRail,EnglishUI.Equip,new Vector2(0,91),new Vector2(146,58),Equip);
        actionButton.GetComponent<UnityEngine.UI.Image>().color=new Color32(138,195,109,255);
        fuseButton=GameUI.Button(actionRail,EnglishUI.Fuse,new Vector2(0,17),new Vector2(146,58),Fuse);
        loreButton=GameUI.Button(actionRail,EnglishUI.Lore,new Vector2(0,-57),new Vector2(146,58),()=>{showLore=!showLore;if(selectedSpell!=null)InspectStarter(selectedSpell);else if(selected!=null)Inspect(selected);});
        var closeHint=GameUI.Label(actionRail,"ESC  /  BACK",new Vector2(0,-145),new Vector2(150,26),14);closeHint.alignment=TextAlignmentOptions.Center;closeHint.color=new Color32(159,153,179,255);
        inspector.gameObject.SetActive(false);
    }
    private void InspectStarter(SpellData spell)
    {
        selected=null;selectedSpell=spell;OpenInspector();
        selectedIcon.enabled=true;selectedIcon.sprite=IllustratedArt.Skill(spell);selectedIcon.material=IllustratedArt.Owns(selectedIcon.sprite)?IllustratedArt.UI:null;selectedBorder.Tier=1;selectedStars.enabled=false;itemRarity.rectTransform.anchoredPosition=Vector2.zero;itemRarity.rectTransform.sizeDelta=new Vector2(250,30);
        itemName.text=spell.DisplayName;itemName.color=new Color32(139,224,235,255);itemRarity.text="STARTER SKILL";
        itemSummary.text=$"Cooldown {spell.Cooldown:0.#}s / Mana 10\nPower {spell.Power:0.#} / Range {spell.Range:0.#}";
        description.color=GameUI.Cream;description.text=showLore?spell.villageQuote:spell.mechanicalDescription;
        upgradeDetails.text="Included in your class loadout";actionButton.gameObject.SetActive(false);fuseButton.gameObject.SetActive(false);
        loreButton.GetComponentInChildren<TMP_Text>().text=showLore?EnglishUI.Stats:EnglishUI.Lore;
    }
    private void OpenInspector()
    {
        if(IsInspecting)return;
        CloseSort();returnFocus=EventSystem.current?.currentSelectedGameObject;
        browseGroup.interactable=false;browseGroup.blocksRaycasts=false;
        inspector.gameObject.SetActive(true);inspector.SetAsLastSibling();closeInspectorButton.Select();
    }
    public void CloseInspector()
    {
        if(!IsInspecting)return;
        inspector.gameObject.SetActive(false);browseGroup.interactable=true;browseGroup.blocksRaycasts=true;
        if(returnFocus!=null&&returnFocus.activeInHierarchy)EventSystem.current?.SetSelectedGameObject(returnFocus);else closeCollectionButton.Select();
        returnFocus=null;
    }
    public void Back(){if(sortMenu!=null){CloseSort();return;}if(IsInspecting){CloseInspector();return;}Close();}
    private void OpenSort()
    {
        if(sortMenu!=null){CloseSort();return;}
        sortMenu=FullScreenShade("SortChoices",.5f);
        browseGroup.interactable=false;browseGroup.blocksRaycasts=false;
        var panel=CollectionPanel(sortMenu,"SortOptions",Vector2.zero,new Vector2(330,272));
        for(int i=0;i<Sorts.Length;i++){int order=i;var button=GameUI.Button(panel,Sorts[i],new Vector2(0,76-i*70),new Vector2(264,56),()=>SetBrowse(filter,order));if(i==sorting)button.Select();}
        RedClose(panel,new Vector2(155,133),CloseSort);
    }
    private void CloseSort()
    {
        if(sortMenu==null)return;sortMenu.gameObject.SetActive(false);Destroy(sortMenu.gameObject);sortMenu=null;
        browseGroup.interactable=true;browseGroup.blocksRaycasts=true;sortButton.Select();
    }
}
