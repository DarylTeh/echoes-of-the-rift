using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public sealed partial class RuntimeSmokeTest
{
    private IEnumerator TestTown(ArenaGame game)
    {
        double deadline=Time.realtimeSinceStartupAsDouble+5;
        while(game.Session.UsesDedicated&&game.Session.World.ReceivedSnapshots<2&&Time.realtimeSinceStartupAsDouble<deadline)yield return null;
        var keyboard=InputSystem.AddDevice<Keyboard>();
        Vector3 before=game.Player.transform.position;float cameraBefore=Camera.main.transform.position.x;
        deadline=Time.realtimeSinceStartupAsDouble+1;
        while(Time.realtimeSinceStartupAsDouble<deadline){InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.D));yield return null;}
        InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return new WaitForSecondsRealtime(.2f);InputSystem.RemoveDevice(keyboard);
        bool moved=game.Player.transform.position.x>before.x+.1f;bool scrolled=Camera.main.transform.position.x>cameraBefore+.1f;
        if(!scrolled)failed=true;Debug.Log("SCROLL_CHECK following="+scrolled);
        float health=game.Player.Health,mana=game.Player.GetComponent<SkillStanceSwapper>().Mana;
        game.Player.Damage(10000);game.Player.GetComponent<PlayerController>().RequestSkill(0);
        bool safe=game.Player.InSafeZone&&game.Player.Health==health&&game.Player.GetComponent<SkillStanceSwapper>().Mana==mana&&!game.Player.GetComponent<PlayerController>().Dodge();
        if(game.Session.UsesDedicated)
        {
            game.Session.World.TestTownSafetyServerRpc();deadline=Time.realtimeSinceStartupAsDouble+3;
            while(!game.Session.World.TownSafetyVerified&&Time.realtimeSinceStartupAsDouble<deadline)yield return null;
            safe&=game.Session.World.TownSafetyVerified;
        }
        else
        {
            var inventory=game.GetComponent<InventoryModal>();bool npcs=true;
            for(int merchant=0;merchant<5;merchant++)
            {
                game.Player.transform.position=TownHubManager.Positions[merchant];game.Hub.Interact(merchant);yield return null;
                int expected=System.Array.FindAll(game.Items,item=>TownHubManager.Sells(TownHubManager.Names[merchant],item)).Length;
                npcs&=inventory.IsOpen&&inventory.VisibleCount==expected;
                if(merchant==4)safe&=inventory.IsOpen&&inventory.VisibleCategoryMatches(4);
                inventory.Close();yield return null;
            }
            bool maps=true;
            foreach(int gate in new[]{5,7,8})
            {
                game.Player.transform.position=TownHubManager.Positions[gate];game.Hub.Interact(gate);yield return null;
                bool dialog=game.Hub.HasDialog&&GameObject.Find("GateDialog")!=null;
                if(gate==5)dialog&=GameObject.Find("RiftDefenseLaunch")!=null;
                maps&=dialog;game.Hub.CloseDialog();
            }
        if(!npcs||!maps)failed=true;
            Debug.Log($"TOWN_SERVICE_CHECK merchantNPCs={npcs} raidDpsWorldBossGates={maps}");
        }
        game.Player.transform.position=TownHubManager.Positions[8];yield return new WaitForSecondsRealtime(.1f);
        var contextButton=GameObject.Find("TownContextAction")?.GetComponent<UnityEngine.UI.Button>();
        var contextTarget=GameObject.Find("TownContextTarget")?.GetComponent<TMPro.TMP_Text>();
        bool compactContext=contextButton!=null&&contextButton.gameObject.activeInHierarchy&&contextTarget!=null&&contextTarget.text=="World Boss"&&contextButton.GetComponentInChildren<TMPro.TMP_Text>().text=="VIEW MODE";
        if(!compactContext)failed=true;
        else{AuditLayout(GameObject.Find("TownHub").transform,"town compact world interaction");contextButton.onClick.Invoke();yield return null;compactContext&=GameObject.Find("GateDialog")!=null;game.Hub.CloseDialog();}
        if(!compactContext)failed=true;
        Debug.Log($"TOWN_CONTEXT_CHECK compactLabelsAndTap={compactContext}");
        var eventsButton=GameObject.Find("EventsButton")?.GetComponent<UnityEngine.UI.Button>();
        bool activityRail=GameObject.Find("TownActivityRail")!=null&&GameObject.Find("CampaignButton")!=null&&GameObject.Find("TownContextAction")!=null;
        if(!activityRail)failed=true;
        eventsButton?.onClick.Invoke();yield return null;
        bool eventDrawer=GameObject.Find("EventDrawer")!=null&&GameObject.Find("CloseEventsButton")!=null;
        if(!eventDrawer)failed=true;game.Hub.CloseDialog();
        var inboxButton=GameObject.Find("InboxButton")?.GetComponent<UnityEngine.UI.Button>();
        inboxButton?.onClick.Invoke();yield return null;
        bool inboxDrawer=GameObject.Find("InboxDrawer")!=null&&GameObject.Find("CloseInboxButton")!=null;
        if(!inboxDrawer)failed=true;game.Hub.CloseDialog();
        yield return new WaitForEndOfFrame();Capture("rift-haven.png");
        bool campaignGate=true;
        if(!game.Session.UsesDedicated)
        {
            game.Player.transform.position=TownHubManager.Positions[6];game.Hub.Interact(6);
            campaignGate=GameObject.Find("TownHub")==null&&!game.Player.InSafeZone;
            if(!campaignGate)failed=true;
            Debug.Log("CAMPAIGN_GATE_CHECK landscapeEntry="+campaignGate);
        }
        if(!moved||!safe)failed=true;
        Debug.Log($"TOWN_CHECK movement={moved} safeZone={safe} localSafe={game.Player.InSafeZone} localHealth={game.Player.Health}/{health} localMana={game.Player.GetComponent<SkillStanceSwapper>().Mana}/{mana} townRpc={game.Session.World.TownSafetyVerified} campaignGate={campaignGate} activityRail={activityRail} eventDrawer={eventDrawer} inboxDrawer={inboxDrawer} server={game.Session.UsesDedicated}");
    }
}
