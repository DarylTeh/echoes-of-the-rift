using UnityEngine;

public static class BossRoster
{
    public static readonly string[] Names={"Osiris · Thorn Warden","Ra · Sun Dragon","Sobek · Deep Leviathan","Zeus · Storm Colossus"};
    private static readonly Sprite[] sprites=new Sprite[4];
    private static readonly BossPatternSO[] patterns=new BossPatternSO[4];
    public static BossPatternSO Pattern(int stage)
    {
        int i=Mathf.Clamp(stage,0,3);if(patterns[i]!=null)return patterns[i];
        var pattern=ScriptableObject.CreateInstance<BossPatternSO>();pattern.Id=Names[i];
        float[] radius={1.6f,2.5f,1.1f,3.2f};float[] windup={1.0f,1.3f,.65f,1.7f};
        pattern.Attacks=new[]{new BossAttack{Name=Names[i]+" strike",TelegraphSeconds=windup[i],Radius=radius[i],Damage=18+i*4,RecoverySeconds=i==2?.45f:1.1f},new BossAttack{Name="Realm burst",TelegraphSeconds=windup[i]+.4f,Radius=radius[i]+.7f,Damage=24+i*5,RecoverySeconds=1.5f}};
        return patterns[i]=pattern;
    }
    public static Sprite Sprite(int stage)
    {
        int i=Mathf.Clamp(stage,0,3);
        return sprites[i]!=null?sprites[i]:sprites[i]=IllustratedArt.Monster(12+i);
    }
}
