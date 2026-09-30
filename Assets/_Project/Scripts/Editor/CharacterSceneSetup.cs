using System;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.U2D;

public static class CharacterSceneSetup
{
    public const string ScenePath="Assets/_Project/Scenes/CharacterPreview.unity";
    public static void BuildCLI()
    {
        if(ProjectValidation.DetectPipeline()!="BuiltIn") throw new InvalidOperationException("This fixture requires the built-in render pipeline.");
        TMP_PackageResourceImporter.ImportResources(true,false,false);
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        var camera=new GameObject("Main Camera",typeof(Camera),typeof(AudioListener)).GetComponent<Camera>();
        camera.tag="MainCamera"; camera.transform.position=new Vector3(0,0,-10); camera.orthographic=true;
        camera.backgroundColor=GameUI.Ink; camera.clearFlags=CameraClearFlags.SolidColor;
        camera.allowHDR=camera.allowMSAA=camera.allowDynamicResolution=false;
        QualitySettings.antiAliasing=0; QualitySettings.anisotropicFiltering=AnisotropicFiltering.Disable;
        var pixels=camera.gameObject.AddComponent<PixelPerfectCamera>();
        pixels.assetsPPU=16; pixels.refResolutionX=320; pixels.refResolutionY=180;
        pixels.upscaleRT=true; pixels.cropFrameX=pixels.cropFrameY=true; pixels.stretchFill=false;
        camera.orthographicSize=5.625f;
        var character=CreateCharacter(); character.transform.position=new Vector3(-4.5f,-1.8f,0); character.transform.localScale=Vector3.one*4;
        new GameObject("CharacterCreator").AddComponent<CharacterCreatorUI>().Preview=character;
        Directory.CreateDirectory("Assets/_Project/Prefabs/Player");
        // Save the gameplay-sized prefab; the scene preview uses a deliberate integer zoom.
        character.transform.localScale=Vector3.one;
        PrefabUtility.SaveAsPrefabAsset(character.gameObject,"Assets/_Project/Prefabs/Player/Character.prefab");
        character.transform.localScale=Vector3.one*4;
        EditorSceneManager.SaveScene(scene,ScenePath);
        EditorBuildSettings.scenes=new[] { new EditorBuildSettingsScene(ScenePath,true) };
        var settings=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
        var input=settings.FindProperty("activeInputHandler"); if(input!=null) { input.intValue=1; settings.ApplyModifiedPropertiesWithoutUndo(); }
        AssetDatabase.SaveAssets();
        ValidateCLI();
    }
    public static CharacterCustomizer CreateCharacter()
    {
        var shader=Shader.Find("EchoesOfTheRift/CharacterPalette");
        if(shader==null) throw new InvalidOperationException("Palette shader missing.");
        const string materialPath="Assets/_Project/Art/Materials/CharacterPalette.mat";
        var material=AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if(material==null) { material=new Material(shader); AssetDatabase.CreateAsset(material,materialPath); }
        var character=new GameObject("Wayfarer").AddComponent<CharacterCustomizer>();
        character.Body=Layer(character.transform,"Body","body",0,material);
        character.Clothes=Layer(character.transform,"Clothes","starter_tunic",1,material);
        character.Hair=Layer(character.transform,"Hair","hair_short",2,material);
        character.HairStyles=new[] { Sprite("hair_short"),Sprite("hair_long"),Sprite("hair_spiked") };
        character.TierFourAura=Aura(character.transform,"Tier4",new Color32(226,180,72,255));
        character.TierFiveAura=Aura(character.transform,"Tier5",new Color32(134,212,189,255));
        character.Apply(new CharacterAppearanceData { ClassId="wayfarer",PassiveSkillId="steadfast" }); character.SetEquipmentTier(1);
        return character;
    }
    public static Sprite Sprite(string name) => AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/_Project/Art/Sprites/Generated/{name}_16.png");
    private static SpriteRenderer Layer(Transform parent,string name,string sprite,int order,Material material)
    {
        var renderer=new GameObject(name).AddComponent<SpriteRenderer>(); renderer.transform.SetParent(parent,false);
        renderer.sprite=Sprite(sprite); renderer.sortingOrder=order; renderer.sharedMaterial=material; return renderer;
    }
    private static ParticleSystem Aura(Transform parent,string name,Color color)
    {
        var aura=new GameObject(name).AddComponent<ParticleSystem>(); aura.transform.SetParent(parent,false);
        aura.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
        var main=aura.main; main.startColor=color; main.startSize=0.055f; main.startLifetime=0.7f; main.startSpeed=0.2f; main.maxParticles=30; main.playOnAwake=false;
        var emission=aura.emission; emission.rateOverTime=14;
        var shape=aura.shape; shape.shapeType=ParticleSystemShapeType.Circle; shape.radius=0.4f;
        aura.transform.localPosition=new Vector3(0,0.35f,0); aura.gameObject.SetActive(false); return aura;
    }
    public static void ValidateCLI()
    {
        var character=CreateCharacter();
        try
        {
            character.Apply(new CharacterAppearanceData { SkinIndex=999,HairStyle=-5,HairColor=999,ClassId="class-test",PassiveSkillId="passive-test" });
            if(character.Appearance.HairStyle!=0 || character.Appearance.ClassId!="class-test" || character.Appearance.PassiveSkillId!="passive-test") throw new Exception("Appearance validation failed.");
            character.SetEquipmentTier(4); if(!character.TierFourAura.gameObject.activeSelf || character.TierFiveAura.gameObject.activeSelf) throw new Exception("Tier 4 aura failed.");
            character.SetEquipmentTier(5); if(character.TierFourAura.gameObject.activeSelf || !character.TierFiveAura.gameObject.activeSelf) throw new Exception("Tier 5 aura failed.");
            character.SetEquipmentTier(1); if(character.TierFourAura.gameObject.activeSelf || character.TierFiveAura.gameObject.activeSelf) throw new Exception("Aura reset failed.");
        }
        finally { UnityEngine.Object.DestroyImmediate(character.gameObject); }
        Debug.Log("STEP_02_OK: appearance bounds, gameplay identity preservation and tier aura switches verified.");
    }
}
