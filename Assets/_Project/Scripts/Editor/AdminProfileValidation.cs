using System;
using UnityEngine;
public static class AdminProfileValidation
{
    public static void BuildCLI()
    {
        var legacy=JsonUtility.FromJson<InventoryState>("{\"Version\":1,\"Coins\":30,\"Items\":[],\"EquippedIds\":[\"\",\"\",\"\"],\"EquippedTiers\":[1,1,1]}");
        if(legacy.Level!=1||legacy.Gems!=0||!legacy.IsValid())throw new Exception("Legacy admin profile migration failed.");
        legacy.Level=25;legacy.Gems=123;legacy.AdminRevision=7;var copy=legacy.Copy();
        if(copy.Level!=25||copy.Gems!=123||copy.AdminRevision!=7||!copy.IsValid())throw new Exception("Admin fields lost in profile roundtrip.");
        copy.Gems=-1;if(copy.IsValid())throw new Exception("Negative gems accepted.");
        Debug.Log("ADMIN_PROFILE_CHECK PASS legacy defaults, gems/level/revision roundtrip, invalid balance.");
        ItemDatabaseGenerator.ExportServerCatalogueCLI();BuildPipelineAutomation.BuildWindows();
    }
}
