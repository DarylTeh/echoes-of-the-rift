using System;
using UnityEngine;

public static class RaidValidation
{
    public static void RunCLI()
    {
        var raid=new RaidState(); raid.Reset();
        if(raid.Defeated) throw new Exception("Empty lobby defeated.");
        raid.Register(1); raid.Register(2); raid.SetAlive(2,false);
        if(!raid.StartRevive(1,2)) throw new Exception("Valid revive rejected.");
        int epoch=raid.Epoch; raid.SetAlive(1,false);
        if(!raid.Defeated||raid.ActiveRevives!=0||raid.CompleteRevive(2,epoch)||raid.AlivePlayerCount!=0) throw new Exception("Late revive resurrected a wiped party.");
        raid.Reset(); raid.Register(1); raid.Register(2); raid.SetAlive(2,false); raid.StartRevive(1,2);
        if(!raid.CompleteRevive(2,raid.Epoch)||raid.AlivePlayerCount!=2) throw new Exception("Live-party revive failed.");
        Debug.Log("STEP_08_OK: Fish-Net RPC code compiled; wipe/late-revive and empty-lobby rules passed. Transport integration test remains required.");
    }
}
