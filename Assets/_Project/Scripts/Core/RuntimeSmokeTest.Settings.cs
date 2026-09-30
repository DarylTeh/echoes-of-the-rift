using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public sealed partial class RuntimeSmokeTest
{
    private IEnumerator TestSettingsLayouts(ArenaGame game)
    {
        var controller=game.Player.GetComponent<PlayerController>();
        if(GameObject.Find("Sign out")!=null||GameObject.Find("AccountSignOut")!=null||GameObject.Find("Sign in again")!=null)layoutFailures.Add("Account action exposed outside Settings > Accounts");
        foreach(var size in LayoutSizes)
        {
            yield return SetLayoutSize(size);game.Hub.OpenSettings();yield return null;
            AuditLayout(GameObject.Find("SettingsMenu").transform,"settings controls "+size);
            GameObject.Find("AccountsTab").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();yield return null;
            AuditLayout(GameObject.Find("SettingsMenu").transform,"settings accounts "+size);
            var inventory=game.GetComponent<InventoryModal>();inventory.Open();
            if(inventory.IsOpen||controller.ControlsEnabled)layoutFailures.Add("Settings failed to own input");
            // Open confirmation without executing sign-out or altering any account.
            game.Hub.RequestSignOut();yield return null;
            AuditLayout(GameObject.Find("SettingsMenu").transform,"sign-out confirmation "+size);
            if(EventSystem.current.currentSelectedGameObject?.name!="CancelSignOut")layoutFailures.Add("Sign-out confirmation does not default to cancel");
            yield return NativeCapture("settings-"+Screen.width+"x"+Screen.height+".png");
            game.GetComponent<PauseManager>().Pause();yield return null;
            if(!game.Hub.SettingsOpen||game.Hub.SignOutConfirmationOpen||game.GetComponent<PauseManager>().IsPaused)layoutFailures.Add("Back failed to cancel sign-out confirmation");
            game.GetComponent<PauseManager>().Pause();yield return null;
            if(game.Hub.HasDialog||!controller.ControlsEnabled)layoutFailures.Add("Settings failed to close/restore controls");
        }
        yield return SetLayoutSize(LayoutSizes[0]);
        layoutChecks.Add("Settings owns input; Accounts-only sign-out; cancel default; Back unwinds confirmation then settings");
    }
}
