using System.Collections;
using UnityEngine;

public sealed partial class RuntimeSmokeTest
{
    private IEnumerator TestUnavailableStartup()
    {
        double deadline=Time.realtimeSinceStartupAsDouble+15;
        while(GameObject.Find("ServerUnavailable")==null&&Time.realtimeSinceStartupAsDouble<deadline)yield return null;
        var dialog=GameObject.Find("ServerUnavailable");var splash=FindFirstObjectByType<SplashScreenUI>();
        bool above=dialog!=null&&splash!=null&&dialog.GetComponent<Canvas>().sortingOrder>splash.Root.GetComponent<Canvas>().sortingOrder;
        if(above){yield return new WaitForEndOfFrame();Capture("server-unavailable.png");AuditLayout(dialog.transform,"server unavailable");}
        splash?.Enter();bool blocked=FindFirstObjectByType<ArenaGame>().Player==null;
        var retry=GameObject.Find("Retry connection")?.GetComponent<UnityEngine.UI.Button>();retry?.onClick.Invoke();yield return new WaitForSecondsRealtime(6);
        bool retryVisible=GameObject.Find("ServerUnavailable")!=null;
        Finish($"{(above&&blocked&&retry!=null&&retryVisible&&layoutFailures.Count==0&&!failed?"PASS":"FAIL")} unavailable aboveSplash={above} blocksEntry={blocked} retry={retryVisible} runtimeErrors={failed}");
    }
}
