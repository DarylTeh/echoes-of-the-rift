using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.U2D;

public static class ArenaSetup
{
    public static void BuildCLI()
    {
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        var camera=new GameObject("Main Camera",typeof(Camera),typeof(AudioListener)).GetComponent<Camera>(); camera.tag="MainCamera";
        camera.transform.position=new Vector3(0,0,-10); camera.orthographic=true; camera.orthographicSize=5.625f; camera.backgroundColor=GameUI.Ink;
        camera.allowHDR=camera.allowMSAA=false;
        PixelPresentation.Configure(camera);
        var game=new GameObject("ArenaGame").AddComponent<ArenaGame>();
        game.CharacterPrefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Player/Character.prefab");
        game.Class=AssetDatabase.LoadAssetAtPath<ClassData>("Assets/_Project/ScriptableObjects/Classes/Wayfarer.asset");
        game.Slime=CharacterSceneSetup.Sprite("enemy_slime"); game.Skeleton=CharacterSceneSetup.Sprite("enemy_skeleton");
        game.Boss=AssetDatabase.LoadAssetAtPath<BossPatternSO>("Assets/_Project/ScriptableObjects/Bosses/MossGuardian.asset");
        game.Items=new ItemData[6]; for(int i=0;i<6;i++)game.Items[i]=AssetDatabase.LoadAssetAtPath<ItemData>($"Assets/_Project/ScriptableObjects/Items/Gear{i}.asset");
        game.Stages=AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/_Project/ScriptableObjects/stages_config.json");
        var manager=new GameObject("NetworkManager").AddComponent<FishNet.Managing.NetworkManager>();
        manager.SpawnablePrefabs=AssetDatabase.LoadAssetAtPath<FishNet.Managing.Object.DefaultPrefabObjects>("Assets/DefaultPrefabObjects.asset");
        manager.gameObject.AddComponent<FishNet.Transporting.Tugboat.Tugboat>();
        var networkRoot=new GameObject("NetworkRaid",typeof(FishNet.Object.NetworkObject));
        var raid=networkRoot.AddComponent<NetworkRaidManager>(); var world=networkRoot.AddComponent<CoopWorld>(); world.Game=game; world.Raid=raid;
        var session=game.gameObject.AddComponent<CoopSession>(); session.Manager=manager; session.World=world; session.Game=game; game.Session=session;
        const string path="Assets/_Project/Scenes/Arena.unity"; EditorSceneManager.SaveScene(scene,path);
        // Fish-Net needs the saved scene path before its editor serialization hook can assign a scene ID.
        var networkObject=networkRoot.GetComponent<FishNet.Object.NetworkObject>();
        var serialize=typeof(FishNet.Object.NetworkObject).GetMethod("ReserializeEditorSetValues",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
        if(serialize==null)throw new System.InvalidOperationException("Pinned Fish-Net editor serialization hook changed.");
        serialize.Invoke(networkObject,new object[] {true,true});
        if(!networkObject.IsSceneObject)throw new System.InvalidOperationException("Raid scene ID was not assigned.");
        EditorUtility.SetDirty(networkObject); EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene,path);
        EditorBuildSettings.scenes=new[] { new EditorBuildSettingsScene(path,true) }; AssetDatabase.SaveAssets();
        Debug.Log("STEP_06_OK: arena scene and dual-stance combat compiled and wired. Playtest required.");
    }
}
