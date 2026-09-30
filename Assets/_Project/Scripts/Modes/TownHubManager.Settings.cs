using TMPro;
using UnityEngine;

public sealed partial class TownHubManager
{
    private RectTransform settingsRoot,settingsPage,settingsFrame;
    private bool confirmingSignOut,accountsTab;
    public bool SettingsOpen=>settingsRoot!=null;
    public bool SignOutConfirmationOpen=>confirmingSignOut;
    public void OpenSettings()
    {
        if(!IsOpen||!root.gameObject.activeInHierarchy||HasDialog||GetComponent<InventoryModal>().IsOpen||GetComponent<PauseManager>().IsPaused)return;
        settingsRoot=GameUI.Canvas("SettingsMenu");settingsRoot.GetComponent<Canvas>().sortingOrder=350;settingsRoot.gameObject.AddComponent<UIMenuFocus>();
        var shade=GameUI.Rect("SettingsBlocker",settingsRoot,Vector2.one*.5f,Vector2.zero,Vector2.zero);
        shade.anchorMin=Vector2.zero;shade.anchorMax=Vector2.one;shade.sizeDelta=Vector2.zero;
        shade.gameObject.AddComponent<UnityEngine.UI.Image>().color=new Color(0,0,0,.72f);
        settingsFrame=GameUI.Panel(settingsRoot,"SettingsFrame",Vector2.zero,new Vector2(840,490));
        GameUI.Icon(settingsFrame,PixelArt.Icon("settings"),new Vector2(-366,203),new Vector2(34,34));
        GameUI.Label(settingsFrame,"SETTINGS",new Vector2(-205,203),new Vector2(260,40),28);
        var close=GameUI.Button(settingsFrame,"X",new Vector2(377,203),new Vector2(48,48),CloseSettings);close.name="CloseSettings";
        close.GetComponent<UnityEngine.UI.Image>().color=new Color32(240,106,138,255);
        GameUI.Button(settingsFrame,"Controls",new Vector2(-300,119),new Vector2(180,48),()=>ShowSettingsTab(false)).name="ControlsTab";
        GameUI.Button(settingsFrame,"Accounts",new Vector2(-300,57),new Vector2(180,48),()=>ShowSettingsTab(true)).name="AccountsTab";
        Session.Game.Player.GetComponent<PlayerController>().ControlsEnabled=false;
        ShowSettingsTab(false);
    }
    public void ShowSettingsTab(bool accounts)
    {
        if(settingsRoot==null)return;accountsTab=accounts;confirmingSignOut=false;
        if(settingsPage!=null){settingsPage.gameObject.SetActive(false);Destroy(settingsPage.gameObject);}
        settingsPage=GameUI.Panel(settingsFrame,"SettingsPage",new Vector2(102,-22),new Vector2(580,366));
        foreach(string tab in new[]{"ControlsTab","AccountsTab"}){
            var button=settingsFrame.Find(tab).GetComponent<UnityEngine.UI.Button>();
            button.GetComponent<UnityEngine.UI.Image>().color=(tab=="AccountsTab")==accounts?new Color32(173,183,246,255):Color.white;
        }
        GameUI.Label(settingsPage,accounts?"ACCOUNT & SESSION":"CONTROLS",new Vector2(0,135),new Vector2(510,36),24).color=new Color32(147,220,239,255);
        if(accounts){
            GameUI.Label(settingsPage,AccountClient.Enabled?"Your progress is saved to your account.\nSigning out returns you to the title screen.\nYou will need to sign in to play again.":"Local playtest session.\nNo online account is signed in.",new Vector2(0,44),new Vector2(510,110),22);
            var signout=GameUI.Button(settingsPage,"Sign out",new Vector2(136,-117),new Vector2(226,48),RequestSignOut);signout.name="AccountSignOut";signout.interactable=AccountClient.Enabled;signout.GetComponent<UnityEngine.UI.Image>().color=new Color32(126,78,108,255);
        }else{
            GameUI.Label(settingsPage,"MOVE      WASD / left stick\nATTACK    Mouse / right stick\nSKILLS    Q, E, R / skill buttons\nINTERACT  F / tap nearby character\nBAG       I / backpack\nBACK      Esc / close button",new Vector2(0,-12),new Vector2(510,230),22);
        }
        UnityEngine.EventSystems.EventSystem.current?.SetSelectedGameObject(settingsFrame.Find(accounts?"AccountsTab":"ControlsTab").gameObject);
    }
    public void RequestSignOut()
    {
        if(settingsPage==null||!accountsTab)return;
        settingsPage.gameObject.SetActive(false);Destroy(settingsPage.gameObject);
        settingsPage=GameUI.Panel(settingsFrame,"ConfirmSignOut",new Vector2(102,-22),new Vector2(580,366));confirmingSignOut=true;
        GameUI.Label(settingsPage,"SIGN OUT?",new Vector2(0,116),new Vector2(500,40),28).color=new Color32(255,164,178,255);
        GameUI.Label(settingsPage,"Your saved progress stays on your account.\nYou will return to the title screen.",new Vector2(0,30),new Vector2(500,108),22);
        var cancel=GameUI.Button(settingsPage,"Stay in game",new Vector2(-135,-113),new Vector2(226,48),()=>ShowSettingsTab(true));cancel.name="CancelSignOut";
        var confirm=GameUI.Button(settingsPage,"Confirm sign out",new Vector2(135,-113),new Vector2(226,48),()=>{if(!AccountClient.Enabled)return;CloseSettings();Session.SignOut();});confirm.name="ConfirmSignOutButton";confirm.interactable=AccountClient.Enabled;confirm.GetComponent<UnityEngine.UI.Image>().color=new Color32(157,70,100,255);
        UnityEngine.EventSystems.EventSystem.current?.SetSelectedGameObject(cancel.gameObject);
    }
    private void CloseSettings()
    {
        if(settingsRoot==null)return;settingsRoot.gameObject.SetActive(false);Destroy(settingsRoot.gameObject);settingsRoot=null;settingsPage=null;settingsFrame=null;confirmingSignOut=false;
        if(Session?.Game?.Player!=null)Session.Game.Player.GetComponent<PlayerController>().ControlsEnabled=!Session.UsesDedicated||Session.Authenticated;
    }
}
