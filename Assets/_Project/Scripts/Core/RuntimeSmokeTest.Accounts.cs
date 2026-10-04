using System;
using System.Collections;
using System.IO;
using UnityEngine;

public sealed partial class RuntimeSmokeTest
{
    private IEnumerator TestAccountFlow(ArenaGame game)
    {
        bool resume=Array.IndexOf(Environment.GetCommandLineArgs(),"-accountResume")>=0;
        var splash=FindFirstObjectByType<SplashScreenUI>();
        double deadline=Time.realtimeSinceStartupAsDouble+30;
        while((splash==null||splash.Username==null)&&Time.realtimeSinceStartupAsDouble<deadline){yield return null;splash=FindFirstObjectByType<SplashScreenUI>();}
        if(splash==null||splash.Username==null){Finish("FAIL account splash missing");yield break;}
        yield return new WaitForEndOfFrame();Capture("splash.png");
        if(!resume)
        {
            splash.Enter();yield return null;
            string testUsername="test_"+Guid.NewGuid().ToString("N").Substring(0,12);
            splash.Username.text=testUsername;splash.Password.text="unused wrong passphrase";
            GameObject.Find("SignInMode")?.GetComponent<UnityEngine.UI.Button>()?.onClick.Invoke();
            GameObject.Find("AccountSubmitButton")?.GetComponent<UnityEngine.UI.Button>()?.onClick.Invoke();
            deadline=Time.realtimeSinceStartupAsDouble+20;while(splash.IsBusy&&Time.realtimeSinceStartupAsDouble<deadline)yield return null;
            bool signInRejected=!splash.Ready&&!splash.IsBusy&&!string.IsNullOrEmpty(splash.LastError);
            yield return new WaitForEndOfFrame();Capture("sign-in-rejected.png");
            splash.Username.text=testUsername;splash.Password.text="new test password";splash.Recovery.text="invalid-test-code";
            GameObject.Find("RecoverAccountMode")?.GetComponent<UnityEngine.UI.Button>()?.onClick.Invoke();
            string recoveryPasswordLabel=splash.Password.transform.Find("Label").GetComponent<TMPro.TMP_Text>().text;
            GameObject.Find("AccountSubmitButton")?.GetComponent<UnityEngine.UI.Button>()?.onClick.Invoke();
            deadline=Time.realtimeSinceStartupAsDouble+20;while(splash.IsBusy&&Time.realtimeSinceStartupAsDouble<deadline)yield return null;
            bool recoveryRejected=!splash.Ready&&!splash.IsBusy&&!string.IsNullOrEmpty(splash.LastError);
            bool recoveryForm=splash.Recovery.gameObject.activeSelf&&recoveryPasswordLabel==EnglishAccount.NewPassword5Characters;
            yield return new WaitForEndOfFrame();Capture("recovery-rejected.png");
            if(!signInRejected||!recoveryRejected||!recoveryForm){Finish($"FAIL account form rejection signIn={signInRejected} recovery={recoveryRejected} recoveryForm={recoveryForm}");yield break;}
            Debug.Log("ACCOUNT_FORM_REJECTIONS_OK: invalid sign-in and recovery showed errors; recovery uses an explicit new-password label.");
            GameObject.Find("CreateAccountMode")?.GetComponent<UnityEngine.UI.Button>()?.onClick.Invoke();
            splash.Username.text=testUsername;splash.Password.text="isolated account test passphrase";
            yield return new WaitForEndOfFrame();Capture("registration.png");splash.Submit(true);
        }
        deadline=Time.realtimeSinceStartupAsDouble+30;
        while(!splash.Ready&&Time.realtimeSinceStartupAsDouble<deadline)yield return null;
        if(!splash.Ready){Finish("FAIL account authentication: "+splash.LastError);yield break;}
        AuditLayout(splash.Root,"account ready");
        bool protectedSession=false;
        try{protectedSession=File.Exists(AccountClient.CachePath)&&WindowsSessionProtection.Unprotect(File.ReadAllBytes(AccountClient.CachePath))==AccountClient.Token&&!System.Text.Encoding.UTF8.GetString(File.ReadAllBytes(AccountClient.CachePath)).Contains(AccountClient.Token);}catch(Exception error){Debug.LogWarning("Protected-session verification was unavailable in this test process: "+error.GetType().Name+".");}
        yield return new WaitForEndOfFrame();Capture(resume?"auto-login.png":"account-ready.png");
        if(layoutFailures.Count>0){File.WriteAllLines(Path.Combine(output,"account-layout.txt"),layoutFailures);Finish($"FAIL account layout layoutFailures={layoutFailures.Count}");yield break;}
        if(!protectedSession)Debug.LogWarning("ACCOUNT_SESSION_CACHE_UNVERIFIED: registration may continue for gameplay UAT, but this run does not certify saved-session resume.");
        if(!resume)
        {
            bool choicesHidden=GameObject.Find("CreateAccountMode")==null&&GameObject.Find("SignInMode")==null&&GameObject.Find("RecoverAccountMode")==null;
            bool gated=!splash.RecoveryAcknowledged&&!splash.CanEnter;
            splash.Enter();yield return null;
            var copy=GameObject.Find("CopyRecoveryCodeButton")?.GetComponent<UnityEngine.UI.Button>();copy?.onClick.Invoke();yield return null;
            bool copied=splash.RecoveryCopied&&!splash.CanEnter;
            var saved=GameObject.Find(EnglishAccount.IHaveSavedMyCode)?.GetComponent<UnityEngine.UI.Button>();saved?.onClick.Invoke();yield return null;
            bool acknowledged=splash.RecoveryAcknowledged&&splash.CanEnter;
            if(!choicesHidden||!gated||!copied||!acknowledged){Finish($"FAIL recovery handoff choicesHidden={choicesHidden} gated={gated} copied={copied} acknowledged={acknowledged}");yield break;}
            Debug.Log("ACCOUNT_RECOVERY_HANDOFF_OK: inactive modes hidden; entry required code copy and explicit save acknowledgement.");
        }
        splash.Enter();yield return null;
        if(game.Player==null){var confirm=GameObject.Find(EnglishScreens.ConfirmCharacter)?.GetComponent<UnityEngine.UI.Button>();confirm?.onClick.Invoke();}
        deadline=Time.realtimeSinceStartupAsDouble+25;
        while(!game.Session.Authenticated&&Time.realtimeSinceStartupAsDouble<deadline)yield return null;
        if(!game.Session.Authenticated){Finish("FAIL account game-ticket authentication");yield break;}
        yield return null;
        string savedToken=AccountClient.Token;
        game.Hub.OpenSettings();yield return null;
        GameObject.Find("AccountsTab").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();yield return null;
        yield return new WaitForEndOfFrame();Capture("settings-accounts.png");
        GameObject.Find("AccountSignOut").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();yield return null;
        bool protectedSignout=game.Hub.SignOutConfirmationOpen&&game.Session.Authenticated;
        GameObject.Find("CancelSignOut").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();yield return null;
        protectedSignout&=game.Hub.SettingsOpen&&!game.Hub.SignOutConfirmationOpen&&AccountClient.Token==savedToken&&game.Session.Authenticated;
        game.Hub.CloseDialog();yield return null;
        if(!protectedSignout){Finish("FAIL protected account sign-out flow");yield break;}
        Debug.Log("ACCOUNT_SETTINGS_OK: cancel retained session and credentials.");
        game.Session.Manager.ClientManager.StopConnection();yield return new WaitForSecondsRealtime(.5f);
        deadline=Time.realtimeSinceStartupAsDouble+25;
        while(!game.Session.Authenticated&&Time.realtimeSinceStartupAsDouble<deadline)yield return null;
        if(!game.Session.Authenticated){Finish("FAIL account reconnect with renewed ticket");yield break;}
        Debug.Log("ACCOUNT_RECONNECT_OK: renewed ticket and saved profile restored.");
        Debug.Log("ACCOUNT_FLOW_OK: "+(resume?"auto-login after process restart":"registered through UI")+"; protectedSession="+protectedSession+"; single-use game ticket.");
        yield return TestDedicated(game);
    }
}
