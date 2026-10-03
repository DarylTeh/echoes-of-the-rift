using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class BuildPipelineAutomation
{
    public static void BuildWindows()=>Build(BuildTarget.StandaloneWindows64,"Builds/Windows/EchoesOfTheRift.exe");
    public static void BuildWebGL()=>Build(BuildTarget.WebGL,"Builds/WebGL");
    public static void BuildAndroid()=>Build(BuildTarget.Android,"Builds/Android/EchoesOfTheRift.apk");
    public static void BuildIOS()=>Build(BuildTarget.iOS,"Builds/iOS");
    private static void Build(BuildTarget target,string output)
    {
        if(!BuildPipeline.IsBuildTargetSupported(BuildPipeline.GetBuildTargetGroup(target),target))throw new InvalidOperationException("Install Unity build support for "+target+" before exporting.");
        if(Resources.Load<TMPro.TMP_Settings>("TMP Settings")==null)throw new InvalidOperationException("TMP resources missing.");
        IdentityValidation.Validate();
        ProjectValidation.EnsureRuntimeShaders();
        var scenes=Array.FindAll(EditorBuildSettings.scenes,s=>s.enabled); if(scenes.Length==0)throw new InvalidOperationException("No enabled build scenes.");
        var fullOutput=Path.GetFullPath(output);var buildRoot=Path.GetFullPath("Builds")+Path.DirectorySeparatorChar;
        if(!fullOutput.StartsWith(buildRoot,StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("Build output must stay under Builds/.");
        if(target==BuildTarget.StandaloneWindows64){var folder=Path.GetDirectoryName(fullOutput);if(Directory.Exists(folder))Directory.Delete(folder,true);}
        else if(target==BuildTarget.WebGL||target==BuildTarget.iOS){if(Directory.Exists(fullOutput))Directory.Delete(fullOutput,true);}
        else if(File.Exists(fullOutput))File.Delete(fullOutput);
        Directory.CreateDirectory(Path.GetDirectoryName(fullOutput));
        PlayerSettings.companyName="Rift Haven"; PlayerSettings.productName="Echoes of the Rift"; PlayerSettings.bundleVersion="0.2.0";
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes=Array.ConvertAll(scenes,s=>s.path),target=target,locationPathName=output,options=BuildOptions.None });
        if(report.summary.result!=BuildResult.Succeeded)throw new Exception(target+" export failed: "+report.summary.result);
        Debug.Log("BUILD_OK: "+target+" "+output);
    }
    public static void ValidateDungeonCLI()
    {
        var stages=DungeonManager.Parse(File.ReadAllText("Assets/_Project/ScriptableObjects/stages_config.json"));
        if(stages.stages.Length!=4||!stages.stages[3].boss)throw new Exception("MVP dungeon missing final boss.");
        bool rejected=false; try { DungeonManager.Parse("{\"stages\":[{\"id\":\"bad\",\"width\":1000,\"height\":9,\"enemies\":3}]}"); } catch(ArgumentException) { rejected=true; }
        if(!rejected)throw new Exception("Invalid dungeon dimensions accepted.");
        ArenaSetup.BuildCLI(); Debug.Log("STEP_10_OK: JSON validation and scene integration passed; target export entry points compiled.");
    }
}
