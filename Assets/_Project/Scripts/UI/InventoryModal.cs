using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed partial class InventoryModal : MonoBehaviour
{
    public ArenaGame Game;
    public bool IsOpen=>root!=null;
    private RectTransform root,grid;
    private TMP_Text description,total,upgradeDetails,title,inventoryGold,inventoryGems;
    private UnityEngine.UI.Image selectedIcon;
    private bool showLore;
    private ItemBorderVFX selectedBorder;
    private UnityEngine.UI.Button fuseButton,previousButton,nextButton,loreButton;
    private readonly Dictionary<string,UnityEngine.UI.Outline> outlines=new Dictionary<string,UnityEngine.UI.Outline>();
    private UnityEngine.UI.Button actionButton;
    private readonly List<ItemStack> visible=new List<ItemStack>();
    private ItemStack selected;
    private int page;
    private bool shopping;
    private string merchant;
    public void OpenMerchant(string name){merchant=name;shopping=true;filter=0;page=0;selected=null;Open();Refresh();}
    private readonly List<GameObject> suspendedScreens=new List<GameObject>();
    private int filter, sorting;
    private UnityEngine.UI.Button sortButton;
    private static readonly string[] Filters=EnglishUI.Categories;
    private static readonly string[] Sorts=EnglishUI.Sorting;
    private float previousScale=1;
    private GameObject preview;
    private Camera previewCamera;
    private RenderTexture previewTexture;
    private void Update(){if(Game.Player!=null&&Keyboard.current?.iKey.wasPressedThisFrame==true){if(IsOpen)Close();else Open();}}
    public void Open()
    {
        if(IsOpen||Game.Forge==null||Game.Hub.HasDialog||Game.GetComponent<PauseManager>().IsPaused)return;
        previousScale=Time.timeScale;if(!Game.Cooperative){Time.timeScale=0;if(Game.Session.UsesDedicated&&Game.Session.World.ClientReady)Game.Session.World.CommandServerRpc("pause");}
        Game.Player.GetComponent<PlayerController>().ControlsEnabled=false;
        foreach(string name in new[]{"TownHub","SkillHUD","ArenaStatus"}){var screen=GameObject.Find(name);if(screen!=null){suspendedScreens.Add(screen);screen.SetActive(false);}}
        root=GameUI.Canvas("InventoryModal");root.GetComponent<Canvas>().sortingOrder=300;
        FullScreenShade("ModalBlocker",.8f);
        BuildCollection();Refresh();
    }
    private TMP_Text BuildCurrency(Transform parent,string family,Vector2 position)
    {
        var panel=GameUI.Panel(parent,family+"Balance",position,new Vector2(154,38));
        GameUI.Icon(panel,PixelArt.Icon(family),new Vector2(-55,0),new Vector2(30,30));
        var value=GameUI.Label(panel,"",new Vector2(15,0),new Vector2(104,28),22);value.alignment=TextAlignmentOptions.MidlineRight;return value;
    }
    private void BuildPreview(Transform panel)
    {
        preview=Instantiate(Game.CharacterPrefab);preview.transform.position=new Vector3(1000,1000,0);preview.GetComponent<CharacterCustomizer>().PresentationScale=1;preview.GetComponent<CharacterCustomizer>().Apply(Game.Forge.State.Appearance);
        foreach(Transform child in preview.GetComponentsInChildren<Transform>(true))child.gameObject.layer=30;
        var vfx=preview.GetComponentInChildren<WeaponTrailVFX>();if(vfx!=null)vfx.Demonstrate=true;
        var cameraObject=new GameObject("InspectionCamera");previewCamera=cameraObject.AddComponent<Camera>();previewCamera.orthographic=true;previewCamera.orthographicSize=1.6f;previewCamera.transform.position=new Vector3(1000,1001,-10);previewCamera.cullingMask=1<<30;previewCamera.clearFlags=CameraClearFlags.SolidColor;previewCamera.backgroundColor=new Color32(27,24,39,255);
        if(Camera.main!=null)Camera.main.cullingMask&=~(1<<30);
        previewTexture=new RenderTexture(256,256,16);previewTexture.filterMode=FilterMode.Point;previewTexture.Create();previewCamera.targetTexture=previewTexture;previewCamera.enabled=false;
        var image=GameUI.Rect("CharacterInspector",panel,Vector2.one*.5f,new Vector2(0,90),new Vector2(206,224)).gameObject.AddComponent<UnityEngine.UI.RawImage>();image.texture=previewTexture;image.raycastTarget=false;
        UpdatePreview();
    }
    private float nextPreview;
    private void LateUpdate(){if(previewCamera!=null&&!shopping&&isActiveAndEnabled&&Time.unscaledTime>=nextPreview){nextPreview=Time.unscaledTime+1f/24;previewCamera.Render();}}
    private void UpdatePreview(){if(preview==null)return;var customizer=preview.GetComponent<CharacterCustomizer>();customizer.SetEquipmentTier(Game.Player.GetComponent<CharacterCustomizer>().EquipmentTier);var weapon=Array.Find(Game.Items,x=>x.Id==Game.Forge.State.EquippedIds[0]);int enhancement=Game.Forge.State.EquippedEnhancementLevels!=null&&Game.Forge.State.EquippedEnhancementLevels.Length>0?Game.Forge.State.EquippedEnhancementLevels[0]:Game.Forge.State.EquippedTiers[0];customizer.SetWeapon(weapon!=null?weapon.Family:"sword",enhancement);}
    public void Refresh()
    {
        if(grid==null)return;title.text=shopping?(merchant??"MERCHANT"):"BACKPACK";LayoutCollection();
        inventoryGold.text=EnglishUI.Compact(Game.Forge.State.Coins);inventoryGems.text=EnglishUI.Compact(Game.Forge.State.Gems);
        foreach(Transform child in grid)Destroy(child.gameObject);
        outlines.Clear();visible.Clear();int count=0;foreach(var item in Game.Forge.State.Items)if(item.Count>0){visible.Add(item);count+=item.Count;}
        if(shopping){visible.Clear();foreach(var item in Game.Items)visible.Add(new ItemStack{ItemId=item.Id,Tier=item.itemTier,Count=Game.Forge.State.Count(item.Id,item.itemTier)});}
        visible.RemoveAll(stack=>{var item=Array.Find(Game.Items,x=>x.Id==stack.ItemId);return item==null||(shopping&&merchant!=null&&!TownHubManager.Sells(merchant,item))||(filter==4?item.Kind!=ItemKind.SkillBook:filter>0&&(item.Kind!=ItemKind.Gear||(int)item.Slot!=filter-1));});
        visible.Sort((a,b)=>{int comparison=sorting==1?b.EffectiveEnhancement.CompareTo(a.EffectiveEnhancement):sorting==2?b.Count.CompareTo(a.Count):0;if(comparison!=0)return comparison;var left=Array.Find(Game.Items,x=>x.Id==a.ItemId);var right=Array.Find(Game.Items,x=>x.Id==b.ItemId);comparison=string.CompareOrdinal(left.DisplayName,right.DisplayName);return comparison!=0?comparison:b.EffectiveEnhancement.CompareTo(a.EffectiveEnhancement);});
        sortButton.GetComponentInChildren<TMP_Text>().text=EnglishUI.Sort+Sorts[sorting]+" v";
        for(int i=0;i<categoryButtons.Length;i++)categoryButtons[i].GetComponent<UnityEngine.UI.Image>().color=i==filter?new Color32(147,138,199,255):Color.white;
        if(selected!=null&&!visible.Exists(x=>x.ItemId==selected.ItemId&&x.Tier==selected.Tier&&x.EffectiveEnhancement==selected.EffectiveEnhancement))selected=null;
        int capacity=shopping?3:20;
        page=Mathf.Clamp(page,0,Mathf.Max(0,(visible.Count-1)/capacity));
        total.text=$"{(shopping?"Offers":"Stacks")}: {visible.Count}   /   Page {page+1}/{Mathf.Max(1,(visible.Count+capacity-1)/capacity)}";
        previousButton.interactable=page>0;nextButton.interactable=(page+1)*capacity<visible.Count;
        emptyLabel.gameObject.SetActive(visible.Count==0);
        for(int n=0;n<capacity;n++)
        {
            int index=page*capacity+n;Vector2 pos=shopping?new Vector2(-330+n*330,0):new Vector2(-224+n%5*112,158-n/5*98);
            var slot=GameUI.Panel(grid,"ItemSlot",pos,shopping?new Vector2(310,366):new Vector2(104,96));
            slot.GetComponent<UnityEngine.UI.Image>().sprite=PixelArt.Frame(true);
            if(index>=visible.Count)continue;
            var stack=visible[index];var item=Array.Find(Game.Items,x=>x.Id==stack.ItemId);if(item==null)continue;
            slot.gameObject.AddComponent<ItemBorderVFX>().Tier=stack.EffectiveEnhancement;
            GameUI.Icon(slot,IllustratedArt.Item(item), new Vector2(0,shopping?24:8),shopping?new Vector2(174,174):new Vector2(72,72));
            if(shopping){var name=GameUI.Label(slot,item.DisplayName,new Vector2(0,140),new Vector2(280,56),24);name.alignment=TextAlignmentOptions.Center;name.color=PixelArt.Rarity(stack.Tier);var priceBand=GameUI.Panel(slot,"PriceBand",new Vector2(0,-146),new Vector2(290,50));GameUI.Icon(priceBand,PixelArt.Icon("gold"),new Vector2(-46,0),new Vector2(30,30));GameUI.Label(priceBand,(15*stack.Tier*stack.Tier).ToString(),new Vector2(18,0),new Vector2(76,30),26).color=GameUI.Gold;}
            var stackLabel=GameUI.Label(slot,shopping?$"{ProgressionRules.StarRating(item.rarity)}  {ProgressionRules.EnhancementBadge(stack.EffectiveEnhancement)} / Owned {EnglishUI.Compact(stack.Count)}":EnglishUI.Stack(item,stack.EffectiveEnhancement,stack.Count),new Vector2(0,shopping?-93:-33),new Vector2(shopping?266:100,24),shopping?18:14);if(shopping)stackLabel.alignment=TextAlignmentOptions.Center;
            var outline=slot.gameObject.AddComponent<UnityEngine.UI.Outline>();outline.effectColor=GameUI.Cream;outline.effectDistance=new Vector2(3,-3);outline.enabled=false;outlines[stack.ItemId+"/"+stack.Tier+"/"+stack.EffectiveEnhancement]=outline;
            var button=slot.gameObject.AddComponent<UnityEngine.UI.Button>();button.onClick.AddListener(()=>{showLore=false;Inspect(stack);});
            int equippedSlot=Array.IndexOf(Game.Forge.State.EquippedIds,stack.ItemId);
            if(!shopping&&equippedSlot>=0&&Game.Forge.State.EquippedTiers[equippedSlot]==stack.Tier&&Game.Forge.State.EquippedEnhancementLevels[equippedSlot]==stack.EffectiveEnhancement)GameUI.Label(slot,EnglishScreens.E,new Vector2(-30,31),new Vector2(24,26),18).color=GameUI.Gold;
        }
        if(IsInspecting&&selected!=null)Inspect(selected);else if(IsInspecting)CloseInspector();RefreshPaperdoll();UpdatePreview();if(previewCamera!=null)previewCamera.Render();
    }
    public void SetBrowse(int category,int order){CloseInspector();CloseSort();filter=Mathf.Clamp(category,0,4);sorting=Mathf.Clamp(order,0,2);page=0;selected=null;Refresh();}
    public int VisibleCount=>visible.Count;
    public bool VisibleCategoryMatches(int category)=>visible.TrueForAll(stack=>{var item=Array.Find(Game.Items,x=>x.Id==stack.ItemId);return category==4?item.Kind==ItemKind.SkillBook:item.Kind==ItemKind.Gear&&(int)item.Slot==category-1;});
    private void Inspect(ItemStack stack)
    {
        selected=stack;var item=Array.Find(Game.Items,x=>x.Id==stack.ItemId);if(item==null)return;
        selectedSpell=null;actionButton.gameObject.SetActive(true);fuseButton.gameObject.SetActive(true);
        OpenInspector();
        actionButton.GetComponentInChildren<TMP_Text>().text=shopping?EnglishUI.Buy:item.Kind==ItemKind.SkillBook?EnglishUI.EquipBook:EnglishUI.Equip;
        selectedIcon.enabled=true;selectedIcon.sprite=IllustratedArt.Item(item);selectedIcon.material=IllustratedArt.Owns(selectedIcon.sprite)?IllustratedArt.UI:null;selectedBorder.Tier=stack.EffectiveEnhancement;
        foreach(var pair in outlines)pair.Value.enabled=pair.Key==stack.ItemId+"/"+stack.Tier+"/"+stack.EffectiveEnhancement;
        int slot=(int)item.Slot,owned=Game.Forge.State.Count(stack.ItemId,stack.Tier,stack.EffectiveEnhancement);
        var equipped=Array.Find(Game.Items,x=>x.Id==Game.Forge.State.EquippedIds[slot]);
        itemName.text=item.DisplayName;itemName.color=PixelArt.Rarity(stack.Tier);itemRarity.text=$"{ProgressionRules.StarRating(item.rarity)}  {ProgressionRules.EnhancementBadge(stack.EffectiveEnhancement)}";
        itemSummary.text=$"{(item.Kind==ItemKind.Gear?item.Slot.ToString():"Skill book")}  /  Owned {EnglishUI.Compact(owned)}\n"+(item.Kind==ItemKind.Gear?$"{item.FlatDamage*stack.EffectiveEnhancement:0.##} ATK   {item.FlatHealth*stack.EffectiveEnhancement:0.##} HP":"Primary Q assignment");
        description.color=showLore?GameUI.Cream:new Color32(153,226,145,255);description.text=showLore?item.description+"\n\n"+item.villageQuote:EnglishUI.Comparison(item,stack.EffectiveEnhancement,equipped,Game.Forge.State.EquippedEnhancementLevels[slot]);
        upgradeDetails.text=shopping?EnglishUI.Price(15*stack.Tier*stack.Tier):EnglishUI.Upgrade(item,stack.EffectiveEnhancement,owned)+(item.Kind==ItemKind.Gear&&item.Slot==EquipmentSlot.Weapon?"\n"+EnglishUI.VisualMilestone(stack.EffectiveEnhancement):"");
        fuseButton.interactable=!shopping&&stack.Tier<5&&owned>=2&&Game.Forge.State.Coins>=10*stack.Tier;
        loreButton.GetComponentInChildren<TMP_Text>().text=showLore?EnglishUI.Stats:EnglishUI.Lore;
        actionButton.interactable=shopping?Game.Forge.State.Coins>=15*stack.Tier*stack.Tier:owned>0;
    }

    private void Equip(){if(selected==null)return;try{if(shopping){if(Game.Forge.ServerTransaction!=null)Game.Forge.ServerTransaction("purchase",selected.ItemId,selected.Tier);else{int cost=15*selected.Tier*selected.Tier;if(Game.Forge.State.Coins<cost){description.text=EnglishScreens.NotEnoughGold;return;}var next=Game.Forge.State.Copy();next.Coins-=cost;next.Add(selected.ItemId,selected.Tier);ProfileStore.Save(next,Game.Forge.SavePath);Game.Forge.Configure(next);Refresh();}return;}if(!(Array.Find(Game.Items,x=>x.Id==selected.ItemId)?.Kind==ItemKind.SkillBook?Game.Forge.EquipSkill(selected.ItemId):Game.Forge.Equip(selected.ItemId,selected.Tier)))description.text=EnglishScreens.ThisItemCannotBeEquippedInA;else Refresh();}catch(Exception e){description.text=EnglishScreens.SaveFailed+e.Message;}}
    private void Fuse(){if(selected==null)return;try{Game.Forge.TryFuse(selected.ItemId,selected.Tier,out string message);Refresh();description.text=message;}catch(Exception e){description.text=EnglishScreens.SaveFailed+e.Message;}}
    public void Close()
    {
        if(root==null)return;Destroy(root.gameObject);root=null;inspector=null;sortMenu=null;merchant=null;shopping=false;filter=0;page=0;selected=null;
        foreach(var screen in suspendedScreens)if(screen!=null)screen.SetActive(true);suspendedScreens.Clear();
        if(preview!=null)Destroy(preview);if(previewCamera!=null)Destroy(previewCamera.gameObject);if(previewTexture!=null){previewTexture.Release();Destroy(previewTexture);}
        if(!Game.Cooperative){Time.timeScale=previousScale;if(Game.Session.UsesDedicated&&Game.Session.World.ClientReady)Game.Session.World.CommandServerRpc("resume");}
        Game.Player.GetComponent<PlayerController>().ControlsEnabled=(Game.Hub.IsOpen||!Game.Dungeon.IsCleared)&&Game.Player.Alive&&(!Game.Session.UsesDedicated||Game.Session.Authenticated);
    }
    private void OnDestroy(){Close();}
}

