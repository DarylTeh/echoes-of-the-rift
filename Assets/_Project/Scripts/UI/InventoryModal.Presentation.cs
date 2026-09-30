using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public sealed partial class InventoryModal
{
    private RectTransform inspector,sortMenu,paperdollSlots,equipmentFrame,collectionFrame;
    private CanvasGroup browseGroup;
    private TMP_Text itemName,itemRarity,itemSummary,heroLevel,heroStats,emptyLabel;
    private UnityEngine.UI.Button closeInspectorButton,closeCollectionButton;
    private UnityEngine.UI.Button[] categoryButtons;
    private GameObject returnFocus;
    private SpellData selectedSpell;
    public bool IsInspecting=>inspector!=null&&inspector.gameObject.activeSelf;
    public int CurrentPage=>page;

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
        var browse=GameUI.Rect("CollectionBrowse",root,Vector2.one*.5f,Vector2.zero,new Vector2(1200,640));
        browseGroup=browse.gameObject.AddComponent<CanvasGroup>();
        title=GameUI.Label(browse,"BACKPACK",new Vector2(-378,294),new Vector2(440,36),28);
        inventoryGems=BuildCurrency(browse,"gem",new Vector2(286,294));inventoryGold=BuildCurrency(browse,"gold",new Vector2(455,294));
        closeCollectionButton=RedClose(browse,new Vector2(568,294),Close);
        var left=CollectionPanel(browse,"EquipmentFrame",new Vector2(-295,-26),new Vector2(438,552));
        var right=CollectionPanel(browse,"CollectionFrame",new Vector2(230,-26),new Vector2(596,552));
        equipmentFrame=left;collectionFrame=right;
        var bag=GameUI.Button(browse,"Bag",new Vector2(-558,215),new Vector2(76,78),()=>SwitchCollection(false,0));
        GameUI.Icon(bag.transform,PixelArt.Icon("bag"),new Vector2(0,12),new Vector2(36,36));
        bag.GetComponentInChildren<TMP_Text>().rectTransform.anchoredPosition=new Vector2(0,-22);
        var skills=GameUI.Button(browse,"Skills",new Vector2(-558,127),new Vector2(76,78),()=>SwitchCollection(false,4));
        GameUI.Icon(skills.transform,PixelArt.Icon("book"),new Vector2(0,12),new Vector2(36,36));skills.GetComponentInChildren<TMP_Text>().rectTransform.anchoredPosition=new Vector2(0,-22);
        var shop=GameUI.Button(browse,"Shop",new Vector2(-558,39),new Vector2(76,78),()=>SwitchCollection(true,0));
        GameUI.Icon(shop.transform,PixelArt.Icon("gold"),new Vector2(0,12),new Vector2(36,36));shop.GetComponentInChildren<TMP_Text>().rectTransform.anchoredPosition=new Vector2(0,-22);
        var header=GameUI.Label(left,"YOUR HERO",new Vector2(0,241),new Vector2(392,32),24);header.alignment=TextAlignmentOptions.Center;
        BuildPreview(left);
        paperdollSlots=GameUI.Rect("EquippedSlots",left,Vector2.one*.5f,Vector2.zero,new Vector2(420,500));
        heroLevel=GameUI.Label(left,"",new Vector2(0,-43),new Vector2(180,32),22);heroLevel.alignment=TextAlignmentOptions.Center;heroLevel.color=new Color32(137,218,199,255);
        var stance=GameUI.Label(left,"MOON  /  EMBER",new Vector2(0,-94),new Vector2(390,26),18);stance.alignment=TextAlignmentOptions.Center;
        var stats=CollectionPanel(left,"EquipmentTotals",new Vector2(0,-222),new Vector2(400,68));
        heroStats=GameUI.Label(stats,"",Vector2.zero,new Vector2(364,54),22);heroStats.alignment=TextAlignmentOptions.Center;
        categoryButtons=new UnityEngine.UI.Button[Filters.Length];
        for(int i=0;i<Filters.Length;i++){int category=i;categoryButtons[i]=GameUI.Button(right,Filters[i],new Vector2(-226+i*113,243),new Vector2(109,40),()=>SetBrowse(category,sorting));}
        sortButton=GameUI.Button(right,"Sort",new Vector2(176,196),new Vector2(204,38),OpenSort);
        total=GameUI.Label(right,"",new Vector2(-101,196),new Vector2(310,32),18);
        grid=GameUI.Rect("ItemGrid",right,Vector2.one*.5f,new Vector2(0,-34),new Vector2(560,430));
        emptyLabel=GameUI.Label(right,EnglishUI.EmptyCategory,new Vector2(0,0),new Vector2(420,40),22);emptyLabel.alignment=TextAlignmentOptions.Center;
        previousButton=GameUI.Button(right,"<",new Vector2(-222,-245),new Vector2(68,40),()=>{page=Mathf.Max(0,page-1);Refresh();});
        nextButton=GameUI.Button(right,">",new Vector2(222,-245),new Vector2(68,40),()=>{page++;Refresh();});
        var hint=GameUI.Label(right,"Tap an item to inspect",new Vector2(0,-245),new Vector2(324,32),18);hint.alignment=TextAlignmentOptions.Center;
        BuildInspector();
    }
    private void SwitchCollection(bool shop,int category)
    {
        shopping=shop;merchant=null;SetBrowse(category,sorting);
    }
    private void LayoutCollection()
    {
        equipmentFrame.gameObject.SetActive(!shopping);
        collectionFrame.anchoredPosition=new Vector2(shopping?40:230,-26);
        collectionFrame.sizeDelta=new Vector2(shopping?1040:596,552);
        previousButton.GetComponent<RectTransform>().anchoredPosition=new Vector2(shopping?-442:-222,-245);
        nextButton.GetComponent<RectTransform>().anchoredPosition=new Vector2(shopping?442:222,-245);
    }
    private void RefreshPaperdoll()
    {
        foreach(Transform child in paperdollSlots)Destroy(child.gameObject);
        var state=Game.Forge.State;heroLevel.text="Lv. "+EnglishUI.Compact(state.Level);
        float attack=0,health=0;
        Vector2[] positions={new Vector2(-158,127),new Vector2(158,127),new Vector2(158,8)};
        for(int i=0;i<3;i++)
        {
            var item=Array.Find(Game.Items,x=>x.Id==state.EquippedIds[i]);int tier=state.EquippedTiers[i];int enhancement=state.EquippedEnhancementLevels!=null&&i<state.EquippedEnhancementLevels.Length?state.EquippedEnhancementLevels[i]:tier;
            var slot=GameUI.Panel(paperdollSlots,"Equipped"+(EquipmentSlot)i,positions[i],new Vector2(94,94));
            slot.GetComponent<UnityEngine.UI.Image>().sprite=PixelArt.Frame(true);slot.GetComponent<UnityEngine.UI.Image>().pixelsPerUnitMultiplier=1.8f;
            var caption=GameUI.Label(paperdollSlots,((EquipmentSlot)i).ToString(),positions[i]+new Vector2(0,-60),new Vector2(110,24),18);caption.alignment=TextAlignmentOptions.Center;
            if(item==null)continue;
            attack+=item.FlatDamage*enhancement;health+=item.FlatHealth*enhancement;
            slot.gameObject.AddComponent<ItemBorderVFX>().Tier=enhancement;
            GameUI.Icon(slot,IllustratedArt.Item(item),new Vector2(0,5),new Vector2(74,74));
            GameUI.Label(slot,ProgressionRules.EnhancementBadge(enhancement),new Vector2(0,-30),new Vector2(62,24),18);
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
        heroStats.text=$"GEAR BONUSES\nATK +{attack:0.##}       HP +{health:0.##}";
    }
    private void BuildInspector()
    {
        inspector=FullScreenShade("ItemInspector",.72f);
        var panel=CollectionPanel(inspector,"ItemDetails",new Vector2(-62,0),new Vector2(732,536));
        var ribbon=CollectionPanel(panel,"RarityRibbon",new Vector2(0,263),new Vector2(274,38));ribbon.GetComponent<UnityEngine.UI.Image>().color=new Color32(154,80,160,255);
        itemRarity=GameUI.Label(ribbon,"",Vector2.zero,new Vector2(250,30),20);itemRarity.alignment=TextAlignmentOptions.Center;
        closeInspectorButton=RedClose(inspector,new Vector2(426,252),CloseInspector);
        var frame=GameUI.Panel(panel,"SelectedItemFrame",new Vector2(-277,164),new Vector2(124,124));frame.GetComponent<UnityEngine.UI.Image>().sprite=PixelArt.Frame(true);
        selectedBorder=frame.gameObject.AddComponent<ItemBorderVFX>();selectedIcon=GameUI.Icon(frame,null,Vector2.zero,new Vector2(100,100));
        itemName=GameUI.Label(panel,"",new Vector2(66,201),new Vector2(472,58),28);
        itemSummary=GameUI.Label(panel,"",new Vector2(66,135),new Vector2(472,62),24);
        description=GameUI.Label(panel,"",new Vector2(0,-25),new Vector2(672,234),22);description.richText=false;description.alignment=TextAlignmentOptions.TopLeft;
        upgradeDetails=GameUI.Label(panel,"",new Vector2(0,-203),new Vector2(672,70),20);upgradeDetails.color=new Color32(140,220,170,255);
        actionButton=GameUI.Button(inspector,EnglishUI.Equip,new Vector2(393,137),new Vector2(156,62),Equip);
        actionButton.GetComponent<UnityEngine.UI.Image>().color=new Color32(138,195,109,255);
        fuseButton=GameUI.Button(inspector,EnglishUI.Fuse,new Vector2(393,57),new Vector2(156,62),Fuse);
        loreButton=GameUI.Button(inspector,EnglishUI.Lore,new Vector2(393,-23),new Vector2(156,62),()=>{showLore=!showLore;if(selectedSpell!=null)InspectStarter(selectedSpell);else if(selected!=null)Inspect(selected);});
        inspector.gameObject.SetActive(false);
    }
    private void InspectStarter(SpellData spell)
    {
        selected=null;selectedSpell=spell;OpenInspector();
        selectedIcon.enabled=true;selectedIcon.sprite=IllustratedArt.Skill(spell);selectedIcon.material=IllustratedArt.Owns(selectedIcon.sprite)?IllustratedArt.UI:null;selectedBorder.Tier=1;
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
