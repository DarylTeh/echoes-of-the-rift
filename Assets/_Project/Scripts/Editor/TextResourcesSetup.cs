using TMPro;
using UnityEditor;
using UnityEngine;

public static class TextResourcesSetup
{
    public static void ImportCLI()
    {
        if(Resources.Load<TMP_Settings>("TMP Settings")!=null) { Debug.Log("TEXT_RESOURCES_OK"); EditorApplication.Exit(0); return; }
        AssetDatabase.importPackageCompleted+=Completed;
        AssetDatabase.importPackageFailed+=(name,error)=> { Debug.LogError(error); EditorApplication.Exit(1); };
        TMP_PackageResourceImporter.ImportResources(true,false,false);
    }
    private static void Completed(string name)
    {
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        if(Resources.Load<TMP_Settings>("TMP Settings")==null) { Debug.LogError("TMP settings missing after import"); EditorApplication.Exit(1); return; }
        Debug.Log("TEXT_RESOURCES_OK"); EditorApplication.Exit(0);
    }
}
