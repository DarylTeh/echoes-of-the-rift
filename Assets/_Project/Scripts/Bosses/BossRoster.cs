using UnityEngine;

public static class BossRoster
{
    public static readonly string[] Names={"Osiris · Thorn Warden","Ra · Sun Phoenix","Sobek · Tidal Hydra","Zeus · Storm Colossus"};
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
        int i=Mathf.Clamp(stage,0,3);if(sprites[i]!=null)return sprites[i];
        var a=new PixelArt.Raster();Color[] colors={new Color32(83,231,133,255),new Color32(255,104,38,255),new Color32(39,218,244,255),new Color32(174,100,255,255)};Color c=colors[i];
        if(i==0){a.Box(9,10,15,19,PixelArt.Dark(c));a.Box(6,5,21,15,c);a.L(9,9,3,1,PixelArt.Gold,3);a.L(24,9,30,1,PixelArt.Gold,3);a.Box(3,19,7,10,c);a.Box(24,19,6,10,c);}
        if(i==1){for(int j=0;j<5;j++){a.L(14,19,1,5+j*4,c,3);a.L(18,19,30,5+j*4,c,3);}a.Diamond(16,20,9,PixelArt.Gold);a.Box(11,5,11,13,c);a.L(14,6,18,0,PixelArt.Gold,3);a.Diamond(22,12,4,PixelArt.Gold);}
        if(i==2){a.Box(7,21,20,10,PixelArt.Dark(c));for(int j=0;j<3;j++){a.L(14+j*3,27,4+j*11,13,c,4);a.Box(1+j*11,5+j%2*4,10,12,c);a.R(3+j*11,9+j%2*4,2,2,Color.white);a.R(7+j*11,9+j%2*4,2,2,PixelArt.Gold);}}
        if(i==3){a.Box(8,8,17,22,c);a.Box(2,13,8,14,PixelArt.Gold);a.Box(23,13,8,14,PixelArt.Gold);a.Box(10,3,14,13,c);a.L(4,2,11,10,Color.cyan,2);a.L(28,2,21,10,Color.cyan,2);a.Diamond(17,22,5,Color.cyan);}
        if(i!=2){a.R(11,12,4,3,PixelArt.Ink);a.R(20,12,4,3,PixelArt.Ink);a.R(12,12,2,2,Color.white);a.R(21,12,2,2,Color.white);}a.P(16,18,Color.white);
        return sprites[i]=a.Finish();
    }
}
