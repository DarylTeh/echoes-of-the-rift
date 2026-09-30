using System;
using System.IO;
using UnityEngine;

public static class ProfileStore
{
    public static string DefaultPath=>Path.Combine(Application.persistentDataPath,"profile-v1.json");
    public static void Save(InventoryState state,string path=null)
    {
        if(state==null||!state.IsValid()) throw new InvalidDataException("Invalid profile.");
        path??=DefaultPath; Directory.CreateDirectory(Path.GetDirectoryName(path));
        string temporary=path+".tmp"; File.WriteAllText(temporary,JsonUtility.ToJson(state,true));
        if(File.Exists(path)) File.Replace(temporary,path,path+".bak"); else File.Move(temporary,path);
    }
    public static InventoryState Load(string path=null)
    {
        path??=DefaultPath;
        foreach(string candidate in new[] {path,path+".bak"})
        {
            if(!File.Exists(candidate)) continue;
            try { var state=JsonUtility.FromJson<InventoryState>(File.ReadAllText(candidate)); if(state!=null&&state.IsValid()) return state; }
            catch(Exception error) when(error is IOException||error is ArgumentException) { Debug.LogWarning("Could not read profile: "+error.Message); }
        }
        if(File.Exists(path)) throw new InvalidDataException("Profile and backup could not be recovered. Original files were preserved.");
        return new InventoryState();
    }
}
