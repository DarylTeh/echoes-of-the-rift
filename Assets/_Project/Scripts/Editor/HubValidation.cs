using System;
using UnityEngine;

public static class HubValidation
{
    public static void RunCLI()
    {
        var quota=new DailyCinemaQuota(); var day=new DateTime(2026,9,22,0,0,0,DateTimeKind.Utc);
        if(quota.TryBegin(false,day))throw new Exception("Ads require opt in.");
        for(int i=0;i<5;i++) { if(!quota.TryBegin(true,day)||quota.TryBegin(true,day)||!quota.Finish(true)||quota.Finish(true))throw new Exception("Cinema reservation or duplicate callback failed."); }
        if(quota.TryBegin(true,day)||!quota.TryBegin(true,day.AddDays(1)))throw new Exception("Daily cap/reset failed.");
        quota.Finish(false); if(quota.Completed!=0)throw new Exception("Failed ad counted.");
        if(CommunityForumManager.PlainMarkdown("# Guide\n**Dodge**")!="Guide\nDodge")throw new Exception("Wiki conversion failed.");
        Debug.Log("STEP_09_OK: hub/pause compiled; opt-in daily quota and basic wiki rendering rules verified. No live forum/ad service configured.");
    }
}
