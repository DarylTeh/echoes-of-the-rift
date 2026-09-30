using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class ContentSetup
{
    public static T Asset<T>(string path) where T:ScriptableObject
    {
        var asset=AssetDatabase.LoadAssetAtPath<T>(path);
        if(asset!=null) return asset;
        Directory.CreateDirectory(Path.GetDirectoryName(path)); asset=ScriptableObject.CreateInstance<T>(); AssetDatabase.CreateAsset(asset,path); return asset;
    }
    public static void BuildCLI()
    {
        var names=new[] { "Ember", "Bramble", "Mend", "Frost", "Lantern", "Resolve" };
        var spells=new SpellData[6];
        for(int i=0;i<6;i++)
        {
            var s=Asset<SpellData>($"Assets/_Project/ScriptableObjects/Spells/{names[i]}.asset");
            s.Id=names[i].ToLowerInvariant(); s.DisplayName=names[i]; s.Effect=(SpellEffect)(i%3);
            s.Cooldown=i%3==2 ? 12 : 3+i; s.Power=i%3==2 ? 28 : 20+i*4; s.Range=i%3==1 ? 2.2f : 6;
            s.Color=i<3 ? new Color32(224,167,83,255) : new Color32(122,196,204,255);
            spells[i]=s; EditorUtility.SetDirty(s);
        }
        var passive=Asset<PassiveSkillSO>("Assets/_Project/ScriptableObjects/CharacterCreation/Steadfast.asset");
        passive.Id="steadfast"; passive.DisplayName="Steadfast"; passive.Description="Gain 20 maximum health."; passive.FlatHealth=20; EditorUtility.SetDirty(passive);
        var cls=Asset<ClassData>("Assets/_Project/ScriptableObjects/Classes/Wayfarer.asset");
        cls.Id="wayfarer"; cls.DisplayName="Wayfarer"; cls.Passive=passive; cls.StartingSpells=spells; EditorUtility.SetDirty(cls);
        var gearNames=new[] { "Worn blade","Copper wand","Trail coat","Moss vest","Amber charm","River charm" };
        for(int i=0;i<6;i++)
        {
            var gear=Asset<ItemData>($"Assets/_Project/ScriptableObjects/Items/Gear{i}.asset");
            gear.Id="gear-"+i; gear.DisplayName=gearNames[i]; gear.Slot=(EquipmentSlot)(i/2);
            gear.FlatDamage=i<2 ? 4+i*2 : 0; gear.FlatHealth=i>=2 ? 5+i*2 : 0; EditorUtility.SetDirty(gear);
        }
        var boss=Asset<BossPatternSO>("Assets/_Project/ScriptableObjects/Bosses/MossGuardian.asset");
        boss.Id="moss-guardian"; boss.Attacks=new[] { new BossAttack { Name="Root Slam",TelegraphSeconds=1.1f,Damage=25,Radius=2,RecoverySeconds=1.4f },new BossAttack { Name="Thorn Pulse",TelegraphSeconds=0.8f,Damage=18,Radius=3,RecoverySeconds=1.8f } }; EditorUtility.SetDirty(boss);
        AssetDatabase.SaveAssets(); ValidateCLI();
    }
    public static void ValidateCLI()
    {
        var post=new ForumPostData(); for(int i=0;i<5;i++) if(!post.TryAddImage("image-"+i)) throw new Exception("Attachment rejected early.");
        if(post.TryAddImage("sixth")) throw new Exception("Forum limit exceeded.");
        var imported=JsonUtility.FromJson<ForumPostData>("{\"imageAttachments\":[\"1\",\"2\",\"3\",\"4\",\"5\",\"6\"]}");
        if(imported.ImageAttachments.Count!=5) throw new Exception("Deserialized post bypassed cap.");
        Debug.Log("STEP_05_OK: MVP data assets generated; forum limit enforced on writes and deserialization.");
    }
}
