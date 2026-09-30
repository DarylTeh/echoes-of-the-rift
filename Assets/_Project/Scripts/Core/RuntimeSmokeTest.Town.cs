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
            game.Player.transform.position=TownHubManager.Positions[4];game.Hub.Interact(4);yield return null;
            var inventory=game.GetComponent<InventoryModal>();safe&=inventory.IsOpen&&inventory.VisibleCount==System.Array.FindAll(game.Items,x=>x.Kind==ItemKind.SkillBook).Length&&inventory.VisibleCategoryMatches(4);inventory.Close();
            game.Player.transform.position=TownHubManager.Positions[7];game.Hub.Interact(7);safe&=game.Hub.HasDialog;game.Hub.CloseDialog();
        }
        var eventsButton=GameObject.Find("EventsButton")?.GetComponent<UnityEngine.UI.Button>();
        eventsButton?.onClick.Invoke();yield return null;
        bool eventDrawer=GameObject.Find("EventDrawer")!=null&&GameObject.Find("CloseEventsButton")!=null;
        if(!eventDrawer)failed=true;game.Hub.CloseDialog();
        var inboxButton=GameObject.Find("InboxButton")?.GetComponent<UnityEngine.UI.Button>();
        inboxButton?.onClick.Invoke();yield return null;
        bool inboxDrawer=GameObject.Find("InboxDrawer")!=null&&GameObject.Find("CloseInboxButton")!=null;
        if(!inboxDrawer)failed=true;game.Hub.CloseDialog();
        yield return new WaitForEndOfFrame();Capture("rift-haven.png");
        if(!moved||!safe)failed=true;
        Debug.Log($"TOWN_CHECK movement={moved} safeZone={safe} eventDrawer={eventDrawer} inboxDrawer={inboxDrawer} server={game.Session.UsesDedicated}");
    }
}
