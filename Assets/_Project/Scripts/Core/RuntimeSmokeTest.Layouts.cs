using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public sealed partial class RuntimeSmokeTest
{
    private readonly List<string> layoutFailures=new List<string>();
    private readonly List<string> layoutChecks=new List<string>();
    private static readonly Vector2Int[] LayoutSizes={new Vector2Int(1280,720),new Vector2Int(1024,768),new Vector2Int(1920,1080),new Vector2Int(1600,900)};
    private IEnumerator SetLayoutSize(Vector2Int size)
    {
        if(size.x<=size.y)layoutFailures.Add("Requested test resolution is not landscape: "+size);
        if(Screen.width!=size.x||Screen.height!=size.y)Screen.SetResolution(size.x,size.y,FullScreenMode.Windowed);
        yield return new WaitForSecondsRealtime(.4f);yield return new WaitForEndOfFrame();
    }
    private void AuditLayout(Transform root,string context)
    {
        Canvas.ForceUpdateCanvases();
        foreach(var label in root.GetComponentsInChildren<TMP_Text>())
        {
            if(!label.isActiveAndEnabled||string.IsNullOrWhiteSpace(label.text))continue;
            if(label.text.Contains("\u00c2")||label.text.Contains("\ufffd")||label.text.Contains("\u00e2\u20ac"))layoutFailures.Add(context+": corrupted text: "+label.text);
            label.ForceMeshUpdate();
            if(label.isTextOverflowing)layoutFailures.Add(context+": text overflow: "+label.text.Replace('\n',' '));
            var corners=new Vector3[4];label.rectTransform.GetWorldCorners(corners);
            Rect safe=UISafeArea.TestArea??Screen.safeArea;
            if(corners[0].x<safe.xMin-2||corners[0].y<safe.yMin-2||corners[2].x>safe.xMax+2||corners[2].y>safe.yMax+2)layoutFailures.Add(context+": outside safe area: "+label.text.Replace('\n',' '));
        }
        layoutChecks.Add(context+": "+Screen.width+"x"+Screen.height);
    }
    private IEnumerator NativeCapture(string name)
    {
        yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(output,name));
        yield return new WaitForSecondsRealtime(.15f);
    }
    private IEnumerator TestCreatorLayouts(CharacterCreatorUI creator)
    {
        foreach(var size in LayoutSizes)
        {
            yield return SetLayoutSize(size);AuditLayout(creator.Root,"creator "+size);
            yield return NativeCapture("creator-"+Screen.width+"x"+Screen.height+".png");
        }
        yield return SetLayoutSize(LayoutSizes[0]);
    }
    private IEnumerator TestInventoryLayouts(ArenaGame game)
    {
        yield return null;
        var visual=game.Player.transform.Find("Pixel-body").GetComponent<SpriteRenderer>();
        float pixels=visual.bounds.size.y*Screen.height/(2*Camera.main.orthographicSize);
        if(pixels<31||pixels>49)layoutFailures.Add("World character height outside 32-48px target: "+pixels);
        layoutChecks.Add("world sprite height: "+pixels+"px");
        var inventory=game.GetComponent<InventoryModal>();
        var originalProfile=game.Forge.State.Copy();var largeProfile=originalProfile.Copy();largeProfile.Coins=2000000000;largeProfile.Gems=2000000000;largeProfile.Level=1000000;largeProfile.Items[0].Count=1000000;
        game.Forge.Configure(largeProfile);game.Hub.Refresh();AuditLayout(GameObject.Find("TownHub").transform,"admin large balances");AuditLayout(GameObject.Find("SkillHUD").transform,"wallet large balances");
        inventory.Open();yield return null;AuditLayout(GameObject.Find("InventoryModal").transform,"admin large stack");inventory.Close();
        game.Forge.Configure(originalProfile);game.Hub.Refresh();inventory.Open();yield return null;
        if(GameObject.Find("TownHub")!=null||GameObject.Find("SkillHUD")!=null)layoutFailures.Add("Inventory retained a background menu");
        layoutChecks.Add("inventory owns primary interaction overlay");
        foreach(var size in LayoutSizes)
        {
            yield return SetLayoutSize(size);
            var root=GameObject.Find("InventoryModal").transform;
            AuditLayout(root,"inventory "+size);yield return NativeCapture("inventory-"+Screen.width+"x"+Screen.height+".png");
        }
        yield return SetLayoutSize(LayoutSizes[0]);
        UISafeArea.TestArea=new Rect(70,24,Screen.width-94,Screen.height-48);
        yield return new WaitForSecondsRealtime(.2f);AuditLayout(GameObject.Find("InventoryModal").transform,"simulated asymmetric notch");
        yield return NativeCapture("inventory-safe-area.png");
        UISafeArea.TestArea=null;yield return new WaitForSecondsRealtime(.2f);
        var keyboard=UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Keyboard>();
        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
        UnityEngine.InputSystem.InputSystem.QueueStateEvent(keyboard,new UnityEngine.InputSystem.LowLevel.KeyboardState(UnityEngine.InputSystem.Key.DownArrow));
        yield return null;yield return null;
        if(UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject==null)layoutFailures.Add("Keyboard navigation did not focus a control");
        UnityEngine.InputSystem.InputSystem.RemoveDevice(keyboard);
        if(inventory.IsInspecting)layoutFailures.Add("Inventory opened an unsolicited item popup");
        for(int skillIndex=0;skillIndex<6;skillIndex++)
        {
            var spell=game.Player.GetComponent<SkillStanceSwapper>().GetSpell(skillIndex);if(spell==null)continue;
            inventory.SendMessage("InspectStarter",spell);AuditLayout(GameObject.Find("InventoryModal").transform,"starter skill "+skillIndex);
            var story=GameObject.Find(EnglishUI.Lore).GetComponent<UnityEngine.UI.Button>();story.onClick.Invoke();AuditLayout(GameObject.Find("InventoryModal").transform,"starter lore "+skillIndex);story.onClick.Invoke();
        }
        inventory.CloseInspector();
        inventory.SendMessage("Inspect",new ItemStack{ItemId=game.Items[0].Id,Tier=game.Items[0].itemTier,Count=1});
        var lore=GameObject.Find(EnglishUI.Lore).GetComponent<UnityEngine.UI.Button>();
        foreach(var item in game.Items)
        {
            inventory.SendMessage("Inspect",new ItemStack{ItemId=item.Id,Tier=item.itemTier,Count=1});
            AuditLayout(GameObject.Find("InventoryModal").transform,"item "+item.Id);
            lore.onClick.Invoke();AuditLayout(GameObject.Find("InventoryModal").transform,"lore "+item.Id);lore.onClick.Invoke();
        }
        foreach(var size in LayoutSizes){yield return SetLayoutSize(size);AuditLayout(GameObject.Find("InventoryModal").transform,"inspector "+size);yield return NativeCapture("item-inspector-"+Screen.width+"x"+Screen.height+".png");}
        yield return SetLayoutSize(LayoutSizes[0]);
        int retainedPage=inventory.CurrentPage;game.GetComponent<PauseManager>().Pause();
        if(!inventory.IsOpen||inventory.IsInspecting||inventory.CurrentPage!=retainedPage||game.Player.GetComponent<PlayerController>().ControlsEnabled)layoutFailures.Add("Item Back lost browsing context or enabled movement");
        layoutChecks.Add("Item Back closes top layer, retains page and blocks world input");
        inventory.SendMessage("OpenSort");yield return null;
        AuditLayout(GameObject.Find("InventoryModal").transform,"sort choices");inventory.Back();
        if(!inventory.IsOpen||GameObject.Find("SortChoices")!=null)layoutFailures.Add("Sort Back failed");
        for(int category=1;category<=4;category++){inventory.SetBrowse(category,0);yield return null;if(!inventory.VisibleCategoryMatches(category))layoutFailures.Add("Wrong visible inventory category "+category);}
        inventory.SetBrowse(0,0);
        inventory.SendMessage("Inspect",new ItemStack{ItemId=game.Items[0].Id,Tier=5,Count=1});
        // Exercise ornament while paused: motion stays outside the icon.
        var ornament=GameObject.Find("SelectedItemFrame").GetComponent<ItemBorderVFX>();
        ornament.Tier=5;yield return null;yield return null;
        var icon=ornament.transform.Find("Icon").GetComponent<UnityEngine.UI.Image>();
        var originalSprite=icon.sprite;var originalColour=icon.color;
        var spark=ornament.transform.Find("BorderSpark").GetComponent<UnityEngine.UI.Image>();
        var before=spark.rectTransform.anchoredPosition;
        yield return new WaitForSecondsRealtime(.2f);
        if((spark.rectTransform.anchoredPosition-before).sqrMagnitude<1||icon.sprite!=originalSprite||icon.color!=originalColour)layoutFailures.Add("Border motion missing or icon core changed");
        foreach(var image in ornament.GetComponentsInChildren<UnityEngine.UI.Image>())
            if(image.name=="BorderSpark"&&(image.raycastTarget||Mathf.Max(Mathf.Abs(image.rectTransform.anchoredPosition.x),Mathf.Abs(image.rectTransform.anchoredPosition.y))<50))layoutFailures.Add("Spark intrudes into icon or blocks clicks");
        layoutChecks.Add("Tier 5 perimeter movement, static icon, click-through");
        inventory.CloseInspector();inventory.Close();yield return null;inventory.OpenMerchant(null);yield return null;
        foreach(var size in LayoutSizes){yield return SetLayoutSize(size);AuditLayout(GameObject.Find("InventoryModal").transform,"merchant cards "+size);yield return NativeCapture("merchant-"+Screen.width+"x"+Screen.height+".png");}
        yield return SetLayoutSize(LayoutSizes[0]);
        // Use only this smoke test's disposable save for actual UI transactions.
        var beforeTransactions=game.Forge.State.Copy();var transactionFixture=beforeTransactions.Copy();transactionFixture.Coins=1000;
        var tradeItem=Array.Find(game.Items,x=>x.Kind==ItemKind.Gear&&x.Slot!=EquipmentSlot.Weapon&&x.itemTier==1&&x.Id!=game.Forge.State.EquippedIds[(int)x.Slot]);
        transactionFixture.Add(tradeItem.Id,1,Mathf.Max(2,tradeItem.DuplicatesPerFuse));game.Forge.Configure(transactionFixture);inventory.Refresh();
        int tradeCount=game.Forge.State.Count(tradeItem.Id,1);
        inventory.SendMessage("Inspect",new ItemStack{ItemId=tradeItem.Id,Tier=1,Count=tradeCount});
        GameObject.Find(EnglishUI.Equip).GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
        if(game.Forge.State.Coins!=985||game.Forge.State.Count(tradeItem.Id,1)!=tradeCount+1)layoutFailures.Add("Merchant Buy did not commit expected price and item");
        inventory.Close();yield return null;inventory.Open();inventory.SendMessage("Inspect",new ItemStack{ItemId=tradeItem.Id,Tier=1,Count=tradeCount+1});
        GameObject.Find(EnglishUI.Equip).GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
        if(game.Forge.State.EquippedIds[(int)tradeItem.Slot]!=tradeItem.Id)layoutFailures.Add("Item popup Equip failed");
        if(!inventory.PaperdollShowsEquipped(tradeItem.Slot))layoutFailures.Add("Equipped gear was not shown on the hero paperdoll");
        int tierTwoBefore=game.Forge.State.Count(tradeItem.Id,2);
        GameObject.Find(EnglishUI.Fuse).GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
        if(game.Forge.State.Count(tradeItem.Id,2)!=tierTwoBefore+1||game.Forge.State.Coins!=985-Mathf.Max(0,tradeItem.BaseFuseCost))layoutFailures.Add("Item popup Upgrade failed");
        layoutChecks.Add("Actual popup Buy, Equip and Upgrade commit to disposable save");
        var weaponItem=Array.Find(game.Items,x=>x.Kind==ItemKind.Gear&&x.Slot==EquipmentSlot.Weapon&&x.itemTier==1&&x.Id!=game.Forge.State.EquippedIds[(int)EquipmentSlot.Weapon]);
        if(weaponItem!=null)
        {
            var weaponFixture=game.Forge.State.Copy();weaponFixture.Add(weaponItem.Id,1);game.Forge.Configure(weaponFixture);inventory.Refresh();
            inventory.SendMessage("Inspect",new ItemStack{ItemId=weaponItem.Id,Tier=1,Count=1});GameObject.Find(EnglishUI.Equip).GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
            var heldWeapon=game.Player.transform.Find("EquippedWeapon");var customizer=game.Player.GetComponent<CharacterCustomizer>();
            if(game.Forge.State.EquippedIds[(int)EquipmentSlot.Weapon]!=weaponItem.Id||inventory.CollectionContains(weaponItem.Id,1)||!inventory.PaperdollShowsEquipped(EquipmentSlot.Weapon))layoutFailures.Add("Equipped weapon was not moved from collection to hero paperdoll");
            if(heldWeapon==null||heldWeapon.GetComponent<SpriteRenderer>().sprite==null||heldWeapon.localPosition.y<customizer.PresentationScale*.6f)layoutFailures.Add("Equipped weapon was not visibly held at the hero's hand");
        }
        game.Forge.Configure(beforeTransactions);ProfileStore.Save(beforeTransactions,game.Forge.SavePath);inventory.Close();yield return null;inventory.Open();
        // Burst pressure must not grow the cosmetic pool after its first fill.
        for(int i=0;i<2000;i++)CosmeticTrailPool.Emit(CombatVisual.Square,Color.cyan,new Vector3(9000,9000),Quaternion.identity,Vector3.one,0,1);
        yield return null;
        int warmed=CosmeticTrailPool.CreatedCount;
        if(warmed>CosmeticTrailPool.Capacity||CosmeticTrailPool.ActiveCount>CosmeticTrailPool.Capacity)layoutFailures.Add("Trail pool exceeded budget");
        yield return new WaitForSecondsRealtime(.6f);yield return null;
        if(CosmeticTrailPool.ActiveCount!=0)layoutFailures.Add("Trail pool failed to expire");
        long allocationStart=GC.GetAllocatedBytesForCurrentThread();
        for(int i=0;i<10000;i++)CosmeticTrailPool.Emit(CombatVisual.Square,Color.cyan,new Vector3(9000,9000),Quaternion.identity,Vector3.one,0,1);
        long emittedBytes=GC.GetAllocatedBytesForCurrentThread()-allocationStart;
        if(CosmeticTrailPool.CreatedCount!=warmed||emittedBytes>1024)layoutFailures.Add("Trail pool allocated after warmup: "+emittedBytes);
        layoutChecks.Add("Trail stress 12000 emissions; objects="+warmed+" cap="+CosmeticTrailPool.Capacity+" warm allocation="+emittedBytes);
        inventory.Close();yield return null;
        if(GameObject.Find("TownHub")==null||GameObject.Find("SkillHUD")==null)layoutFailures.Add("Inventory failed to restore underlying screen");
        foreach(var size in LayoutSizes){yield return SetLayoutSize(size);AuditLayout(GameObject.Find("TownHub").transform,"town "+size);yield return NativeCapture("town-"+Screen.width+"x"+Screen.height+".png");}
        yield return SetLayoutSize(LayoutSizes[0]);
        yield return TestSettingsLayouts(game);
        var eventsButton=GameObject.Find("EventsButton")?.GetComponent<UnityEngine.UI.Button>();
        eventsButton?.onClick.Invoke();yield return null;
        if(GameObject.Find("EventDrawer")==null)layoutFailures.Add("Events drawer did not open");
        else {
            var eventHit=GameObject.Find("EventDrawer").GetComponentsInChildren<UnityEngine.UI.Button>(true);
            var rowButton=System.Array.Find(eventHit,x=>x!=null&&x.name.StartsWith("EventRowButton-"));
            if(rowButton!=null){rowButton.onClick.Invoke();yield return null;if(GameObject.Find("EventDetails")==null||GameObject.Find("BackToEventList")==null)layoutFailures.Add("Event detail view did not open");else {foreach(var size in LayoutSizes){yield return SetLayoutSize(size);AuditLayout(GameObject.Find("EventDetails").transform,"event details "+size);}yield return SetLayoutSize(LayoutSizes[0]);GameObject.Find("BackToEventList").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();yield return null;if(GameObject.Find("EventDetails")!=null)layoutFailures.Add("Event detail Back did not close");}}
            foreach(var size in LayoutSizes){yield return SetLayoutSize(size);AuditLayout(GameObject.Find("EventDrawer").transform,"events "+size);} yield return SetLayoutSize(LayoutSizes[0]);
        }
        game.Hub.CloseDialog();
        var inboxButton=GameObject.Find("InboxButton")?.GetComponent<UnityEngine.UI.Button>();
        inboxButton?.onClick.Invoke();yield return null;
        if(GameObject.Find("InboxDrawer")==null)layoutFailures.Add("Inbox drawer did not open");
        else { foreach(var size in LayoutSizes){yield return SetLayoutSize(size);AuditLayout(GameObject.Find("InboxDrawer").transform,"inbox "+size);} yield return SetLayoutSize(LayoutSizes[0]); }
        game.Hub.CloseDialog();
        AuditLayout(GameObject.Find("SkillHUD").transform,"HUD");
        game.StartExpedition();yield return new WaitForSecondsRealtime(.2f);
        var expedition=FindAnyObjectByType<ExpeditionHUD>();var boss=game.Dungeon.StageBoss;
        expedition.Refresh();bool bossInitiallyVisible=expedition.BossVisible;
        boss.Damage(25);expedition.Refresh();
        bool bossTracks=bossInitiallyVisible&&Mathf.Abs(expedition.BossFraction-boss.Health/boss.MaximumHealth)<.001f&&!expedition.PartyVisible;
        boss.ResetHealth(boss.MaximumHealth);
        Vector2 projected=ExpeditionHUD.Project(new Vector2(100,-100),new Vector2(20,10),new Vector2(120,60));
        if(!bossTracks||projected!=new Vector2(60,-30))layoutFailures.Add("Boss HUD health / minimap bounds / solo party visibility");
        layoutChecks.Add("Expedition HUD: boss damage tracking, solo party hidden, minimap bounds");
        if(FixedTouchStick.EnabledForDevice)yield return TestTouchSticks(game);
        foreach(var size in LayoutSizes){yield return SetLayoutSize(size);AuditLayout(GameObject.Find("SkillHUD").transform,"combat HUD "+size);yield return NativeCapture("combat-hud-"+Screen.width+"x"+Screen.height+".png");}
        boss.Damage(100000);expedition.Refresh();if(expedition.BossVisible)layoutFailures.Add("Boss HUD remained visible after clear");
        File.WriteAllLines(Path.Combine(output,"layout-checks.txt"),layoutChecks);
        File.WriteAllLines(Path.Combine(output,"layout-failures.txt"),layoutFailures);
        Finish($"{(layoutFailures.Count==0&&!failed?"PASS":"FAIL")} UI layout checks={layoutChecks.Count} failures={layoutFailures.Count} runtimeErrors={failed}");
    }
}
