using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

public static class ProjectValidation
{
    public static string DetectPipeline()
    {
        var pipeline = GraphicsSettings.currentRenderPipeline;
        return pipeline == null ? "BuiltIn" : pipeline.GetType().FullName;
    }

    public static void CompileCLI()
    {
        Debug.Log("COMPILE_OK: Pipeline=" + DetectPipeline());
    }

    public static void ExportSmokeCLI()
    {
        if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64))
            throw new InvalidOperationException("Windows build support is unavailable or incomplete. Repair the Unity editor installation in Unity Hub.");
        Directory.CreateDirectory("Assets/_Project/Scenes/Diagnostics");
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var camera = new GameObject("Smoke Camera").AddComponent<Camera>();
        camera.orthographic = true;
        camera.backgroundColor = new Color(0.09f, 0.08f, 0.13f);
        const string path = "Assets/_Project/Scenes/Diagnostics/ExportSmoke.unity";
        EditorSceneManager.SaveScene(scene, path);
        Directory.CreateDirectory("Builds/Smoke");
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = new[] { path }, locationPathName = "Builds/Smoke/EchoesOfTheRift-Smoke.exe",
            target = BuildTarget.StandaloneWindows64, options = BuildOptions.Development
        });
        if (report.summary.result != BuildResult.Succeeded)
            throw new InvalidOperationException("Smoke export failed: " + report.summary.result);
        Debug.Log("EXPORT_SMOKE_OK");
    }
    public static void ExportCurrentCLI()
    {
        if(Resources.Load<TMPro.TMP_Settings>("TMP Settings")==null) throw new Exception("Import TMP essentials with TextResourcesSetup.ImportCLI (without -quit) before exporting.");
        EnsureRuntimeShaders();
        Directory.CreateDirectory("Builds/Prototype");
        var scenes=Array.FindAll(EditorBuildSettings.scenes,s=>s.enabled);
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes=Array.ConvertAll(scenes,s=>s.path),locationPathName="Builds/Prototype/EchoesOfTheRift.exe",
            target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development
        });
        if(report.summary.result!=BuildResult.Succeeded) throw new Exception("Prototype export failed: "+report.summary.result);
        Debug.Log("PROTOTYPE_EXPORT_OK");
    }
    public static void EnsureRuntimeShaders()
    {
        var graphics=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset")[0]);
        var shaders=graphics.FindProperty("m_AlwaysIncludedShaders");
        foreach(string name in new[] { "UI/Default","Sprites/Default","EchoesOfTheRift/CharacterPalette","EchoesOfTheRift/PixelGlow","EchoesOfTheRift/IllustratedHero","EchoesOfTheRift/IllustratedUI" })
        {
            var shader=Shader.Find(name); if(shader==null) throw new Exception("Required shader missing: "+name);
            bool found=false; for(int i=0;i<shaders.arraySize;i++) if(shaders.GetArrayElementAtIndex(i).objectReferenceValue==shader) found=true;
            if(!found) { int index=shaders.arraySize; shaders.InsertArrayElementAtIndex(index); shaders.GetArrayElementAtIndex(index).objectReferenceValue=shader; }
        }
        graphics.ApplyModifiedPropertiesWithoutUndo(); AssetDatabase.SaveAssets();
    }
}
