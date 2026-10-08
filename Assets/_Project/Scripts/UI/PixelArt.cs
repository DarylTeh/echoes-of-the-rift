using System;
using System.Collections.Generic;
using UnityEngine;

// Original deterministic 32px art; all silhouettes use discrete pixels and a shared ink outline.
public static class PixelArt
{
    public static readonly Color Ink=new Color32(17,20,34,255), Gold=new Color32(220,169,72,255);
    private static readonly Dictionary<string,Sprite> cache=new Dictionary<string,Sprite>();
    public sealed class Raster
    {
        public readonly Texture2D Texture=new Texture2D(32,32,TextureFormat.RGBA32,false);
        public Raster(){Texture.SetPixels(new Color[1024]);Texture.filterMode=FilterMode.Point;Texture.wrapMode=TextureWrapMode.Clamp;}
        public void P(int x,int y,Color c){if(x>=0&&x<32&&y>=0&&y<32)Texture.SetPixel(x,31-y,c);}
        public void R(int x,int y,int w,int h,Color c){for(int j=y;j<y+h;j++)for(int i=x;i<x+w;i++)P(i,j,c);}
        public void Box(int x,int y,int w,int h,Color c){R(x,y,w,h,Ink);R(x+1,y+1,w-2,h-2,c);R(x+1,y+1,w-2,1,Light(c));R(x+1,y+h-2,w-2,1,Dark(c));}
        public void L(int x,int y,int x2,int y2,Color c,int thick=1){int steps=Math.Max(Math.Abs(x2-x),Math.Abs(y2-y));for(int i=0;i<=steps;i++){float t=steps==0?0:(float)i/steps;R(Mathf.RoundToInt(Mathf.Lerp(x,x2,t)),Mathf.RoundToInt(Mathf.Lerp(y,y2,t)),thick,thick,c);}}
        public void Diamond(int x,int y,int radius,Color c){for(int j=-radius;j<=radius;j++)for(int i=-radius;i<=radius;i++)if(Math.Abs(i)+Math.Abs(j)<=radius)P(x+i,y+j,c);}
        public Sprite Finish(float ppu=32,bool frame=false){Texture.Apply();return Sprite.Create(Texture,new Rect(0,0,32,32),new Vector2(.5f,.5f),ppu,0,SpriteMeshType.FullRect,frame?new Vector4(8,8,8,8):Vector4.zero);}
    }
    public static Color Light(Color c)=>Color.Lerp(c,Color.white,.42f);
    public static Color Dark(Color c)=>Color.Lerp(c,Ink,.45f);
    private static readonly Color32[] rarityColors={new Color32(164,185,186,255),new Color32(96,189,134,255),new Color32(121,137,239,255),new Color32(221,122,232,255),new Color32(255,192,73,255)};
    public static Color Rarity(int tier)=>rarityColors[Mathf.Clamp(tier-1,0,4)];
    // Rounded, nine-sliced bevels retain their corner shape at every panel size.
    public static Sprite Frame(bool slot=false) => Surface(slot ? "slot" : "frame",false,false,slot);
    public static Sprite Border() => Surface("border",true,false,false);
    public static Sprite Orb() => Surface("orb",false,true,false);
    public static Sprite RarityStars(ItemRarity rarity)
    {
        string key="rarity-stars/"+(int)rarity;
        if(cache.TryGetValue(key,out var sprite))return sprite;
        var art=new Raster();
        // Tall, chunky five-point stars stay legible when the strip is scaled
        // into a compact inventory cell.
        string[] shape={"..#..","..#..","..#..",".###.","#####","#####","#####",".###.","..#..","..#..",".#.#.",".#.#.","#...#","#...#"};
        Color filled=new Color32(255,205,75,255),empty=new Color32(76,72,91,255);
        for(int star=0;star<5;star++)
        for(int y=0;y<shape.Length;y++)
        for(int x=0;x<shape[y].Length;x++)
            if(shape[y][x]=='#')art.P(star*6+x,9+y,star<Mathf.Clamp((int)rarity+1,1,5)?filled:empty);
        return cache[key]=art.Finish(32);
    }
    private static Sprite Surface(string key,bool edge,bool circle,bool slot)
    {
        if(cache.TryGetValue(key,out var sprite))return sprite;
        var a=new Raster();
        for(int y=0;y<32;y++)for(int x=0;x<32;x++)
        {
            float distance=circle?new Vector2(x-15.5f,y-15.5f).magnitude-15.5f:
                new Vector2(Mathf.Max(Mathf.Abs(x-15.5f)-8,0),Mathf.Max(Mathf.Abs(y-15.5f)-8,0)).magnitude-7.5f;
            if(distance>0)continue;
            if(edge){if(distance>=-2)a.P(x,y,distance>-.8f?new Color(1,1,1,.35f):Color.white);continue;}
            Color c=slot?new Color32(21,20,29,255):new Color32(35,32,49,255);
            if(distance>-1)c=new Color32(12,11,19,255);
            else if(distance>-2)c=new Color32(128,119,155,255);
            else if(distance>-3)c=new Color32(66,60,86,255);
            else if(distance>-4)c=y<16?new Color32(86,78,108,255):new Color32(25,23,36,255);
            a.P(x,y,c);
        }
        return cache[key]=a.Finish(32,!circle);
    }
    public static Sprite Icon(string family,int variant=0,int tier=1)
    {
        var illustrated=IllustratedArt.UtilityIcon(family);if(illustrated!=null)return illustrated;
        string key=family+variant+"/"+tier;if(cache.TryGetValue(key,out var sprite))return sprite;
        var a=new Raster();Color metal=new Color32(177,207,218,255),leather=new Color32(123,77,58,255),glow=Color.HSVToRGB((variant*.117f+tier*.073f)%1,.65f,.95f);
        switch(family.ToLowerInvariant())
        {
            case "gold":
                for(int y=3;y<29;y++)for(int x=3;x<29;x++){
                    float d=new Vector2(x-15.5f,y-15.5f).magnitude;
                    if(d<13)a.P(x,y,d>11?Ink:d>9?new Color32(255,212,68,255):new Color32(184,100,24,255));
                    if(d<8)a.P(x,y,y<16?new Color32(255,213,60,255):new Color32(240,157,26,255));
                }
                a.R(14,9,4,14,new Color32(255,240,137,255));a.R(11,11,10,3,new Color32(255,240,137,255));a.R(11,19,10,3,new Color32(155,82,27,255));a.L(7,7,12,4,Color.white,2);break;
            case "gem":
                a.Diamond(16,16,14,Ink);a.Diamond(16,15,12,new Color32(24,117,200,255));
                for(int y=4;y<=26;y++)for(int x=4;x<=28;x++)if(Mathf.Abs(x-16)+Mathf.Abs(y-15)<=11){
                    Color c=x<16?new Color32(86,230,255,255):new Color32(31,171,239,255);
                    if(y>15)c=x<16?new Color32(36,157,224,255):new Color32(32,100,180,255);a.P(x,y,c);
                }
                a.L(7,14,25,14,new Color32(146,249,255,255));a.L(16,5,12,14,Color.white);a.L(12,14,16,25,new Color32(87,221,255,255));a.R(12,7,3,3,Color.white);break;
            case "settings":
                for(int i=0;i<8;i++){float angle=i*Mathf.PI/4;a.Box(13+Mathf.RoundToInt(Mathf.Cos(angle)*10),13+Mathf.RoundToInt(Mathf.Sin(angle)*10),6,6,new Color32(117,174,207,255));}
                for(int y=5;y<27;y++)for(int x=5;x<27;x++){float d=new Vector2(x-15.5f,y-15.5f).magnitude;if(d<11)a.P(x,y,d>9?Ink:d>5?new Color32(113,189,227,255):Ink);if(d<3)a.P(x,y,new Color32(35,53,80,255));}
                a.L(10,8,17,6,new Color32(203,243,255,255),2);break;
            case "sword":case "dagger":case "greatsword":
                a.L(10,23,23,4,Ink,5);a.L(11,23,24,4,metal,3);a.L(11,23,24,4,Color.white);a.L(7,20,15,26,Ink,3);a.L(7,20,15,26,Gold);a.L(8,25,5,29,leather,3);a.Diamond(15,20,2,glow);break;
            case "staff":case "wand":
                a.L(8,28,21,8,Ink,4);a.L(9,28,22,8,leather,2);a.Diamond(23,7,6,Ink);a.Diamond(23,7,4,glow);a.Diamond(22,6,2,Light(glow));a.L(18,12,26,10,Gold);break;
            case "bow":case "crossbow":
                a.L(9,4,20,10,Ink,3);a.L(20,10,23,18,Ink,3);a.L(23,18,9,29,Ink,3);a.L(10,5,20,11,Gold);a.L(20,11,23,19,Gold);a.L(23,19,10,29,Gold);a.L(9,5,9,29,metal);a.L(3,17,27,17,leather,2);a.Diamond(27,17,3,metal);break;
            case "scythe":
                a.L(9,29,21,5,Ink,4);a.L(10,29,22,5,leather,2);a.Box(7,3,18,5,metal);a.L(7,6,4,15,metal,2);a.Diamond(22,6,2,glow);break;
            case "hammer":
                a.L(10,28,20,10,Ink,4);a.L(11,28,21,10,leather,2);a.Box(9,5,20,10,metal);a.Box(17,6,5,8,Gold);a.P(19,9,glow);break;
            case "shield":
                a.Box(6,5,20,16,metal);a.Diamond(16,19,9,Ink);a.Diamond(16,18,7,metal);a.Box(9,7,14,11,glow);a.L(16,7,16,24,Gold,2);break;
            case "helmet":
                a.Box(6,8,20,16,metal);a.Box(9,5,14,8,metal);a.R(9,16,14,4,Ink);a.R(14,12,4,14,Gold);a.L(16,4,20,1,glow,2);break;
            case "chestpiece":case "pauldrons":
                a.Box(8,8,16,19,metal);a.Box(2,7,10,9,glow);a.Box(20,7,10,9,glow);a.Box(12,5,8,6,leather);a.L(9,15,21,24,Gold,2);a.Diamond(16,13,3,glow);break;
            case "leggings":case "boots":
                a.Box(6,7,9,21,metal);a.Box(18,7,9,21,metal);a.Box(4,24,11,6,leather);a.Box(18,24,12,6,leather);a.R(8,16,5,3,Gold);a.R(20,16,5,3,Gold);break;
            case "ring":case "amulet":
                for(int i=0;i<40;i++){float t=i*Mathf.PI/20;int x=16+Mathf.RoundToInt(Mathf.Cos(t)*8),y=18+Mathf.RoundToInt(Mathf.Sin(t)*8);a.R(x,y,3,3,Gold);}a.Diamond(17,10,6,Ink);a.Diamond(17,10,4,glow);a.P(16,8,Color.white);break;
            case "book":
                a.Box(5,5,23,24,leather);a.Box(8,3,18,24,glow);a.R(10,25,15,3,metal);a.R(7,5,3,20,Gold);a.Diamond(18,14,6,Ink);a.Diamond(18,14,4,Gold);a.R(17,9,2,10,Light(glow));break;
            case "quest": a.Box(7,3,19,26,new Color32(222,207,158,255));a.Box(4,3,25,5,leather);a.R(15,10,3,9,leather);a.R(15,21,3,3,leather);break;
            case "bag": a.Box(6,9,21,20,leather);a.Box(11,4,11,8,leather);a.Box(7,12,19,7,Gold);a.Box(13,16,6,8,metal);break;
            case "swap": a.L(5,10,25,10,glow,3);a.L(25,10,20,5,glow,3);a.L(25,21,5,21,Gold,3);a.L(5,21,10,26,Gold,3);break;
            case "dodge": a.L(5,9,20,9,metal,2);a.L(2,16,15,16,Gold,2);a.L(6,24,19,24,metal,2);a.Diamond(22,16,7,glow);break;
            default:
                for(int i=0;i<8;i++){float angle=i*Mathf.PI/4+variant*.3f; a.L(16,16,16+(int)(Mathf.Cos(angle)*12),16+(int)(Mathf.Sin(angle)*12),Dark(glow),2);}
                a.Diamond(16,16,10,Ink);a.Diamond(16,16,8,glow);a.Diamond(15,14,5,Light(glow));
                if(variant%3==2){a.R(14,8,4,17,Color.white);a.R(8,14,17,4,Color.white);}else if(variant%3==1){a.Box(10,10,13,13,Dark(glow));a.Diamond(16,16,4,Gold);}else{a.L(18,7,12,18,Color.white,3);a.L(12,18,18,18,Color.white,3);a.L(18,18,13,26,Color.white,2);}break;
        }
        if(tier>=3){a.P(3,4,glow);a.P(28,24,glow);a.L(2,4,4,4,Light(glow));a.L(3,3,3,5,Light(glow));}
        return cache[key]=a.Finish();
    }
    public static Sprite Character(string part,int race,int style,Color skin,Color hair,Color eyes)
    {
        string key=part+race+"/"+style+ColorUtility.ToHtmlStringRGB(skin)+ColorUtility.ToHtmlStringRGB(hair)+ColorUtility.ToHtmlStringRGB(eyes);if(cache.TryGetValue(key,out var s))return s;
        var a=new Raster();Color armour=new Color32(66,88,226,255), steel=new Color32(177,234,249,255);
        // Oversized square head, small tunic and boots: a readable original chibi silhouette.
        if(part=="body") {a.Box(11,22,10,7,skin);a.Box(8,23,4,5,skin);a.Box(20,23,4,5,skin);a.Box(11,28,4,4,Dark(armour));a.Box(18,28,4,4,Dark(armour));}
        if(part=="tail"&&race!=8){a.Box(9,20,14,10,new Color32(172,49,138,255));a.R(10,22,2,7,new Color32(243,92,161,255));}
        if(part=="head") {a.Box(6,5,21,18,skin);a.R(8,7,17,3,Light(skin));a.R(24,9,2,11,Dark(skin));a.R(10,13,3,6,Ink);a.R(20,13,3,6,Ink);a.R(11,14,1,3,eyes);a.R(21,14,1,3,eyes);a.P(10,13,Color.white);a.P(20,13,Color.white);a.R(15,20,3,1,Dark(skin));a.P(9,19,new Color32(247,130,153,255));a.P(23,19,new Color32(247,130,153,255));if(race==5){a.R(10,13,4,5,Ink);a.R(19,13,4,5,Ink);a.R(13,21,8,1,Ink);}if(race==3||race==4){a.R(12,20,2,3,Color.white);a.R(21,20,2,3,Color.white);}if(race==2){a.Box(9,19,15,6,hair);a.R(13,19,7,2,skin);}}
        if(part=="hair") {a.Box(5,3,23,9,hair);a.R(8,4,15,2,Light(hair));a.R(6,10,4,5,hair);a.R(24,10,3,6,Dark(hair));a.R(11,10,4,3,hair);a.R(19,9,4,3,hair);if(style==1){a.Box(4,7,4,16,hair);a.Box(25,7,4,17,hair);a.R(5,8,1,12,Light(hair));}if(style==2){a.Box(7,1,7,5,hair);a.Box(17,0,6,5,hair);a.L(24,5,28,1,hair,2);}a.R(7,5,2,4,Light(hair));a.R(12,5,2,3,Light(hair));a.R(20,6,4,2,Light(hair));}
        if(part=="ears"&&(race==1||race==3||race==4||race==6)){a.L(7,15,2,10,Ink,3);a.L(7,14,3,11,skin,2);a.L(25,15,30,10,Ink,2);a.L(25,14,29,11,skin);}
        if(part=="horns"&&(race==7||race==6)){a.L(8,7,3,1,Ink,3);a.L(8,6,4,1,Gold,2);a.L(24,6,29,1,Ink,3);a.L(24,5,28,1,Gold,2);}
        if(part=="tail"&&race==8){for(int i=0;i<4;i++){a.L(10,23,2,15+i*3,steel,2);a.L(22,23,30,15+i*3,steel,2);}}
        if(part=="armour"){a.Box(11,23,11,7,armour);a.R(12,24,2,3,Light(armour));a.R(11,28,11,2,Gold);a.Diamond(17,25,2,Color.cyan);a.P(17,24,Color.white);a.R(11,31,4,1,Gold);a.R(18,31,4,1,Gold);}
        return cache[key]=a.Finish(16);
    }
    public static Sprite Floor(bool wall,int variant)
    {
        string key="floor"+wall+variant;if(cache.TryGetValue(key,out var s))return s;var a=new Raster();Color stone=wall?new Color32(67,79,89,255):new Color32(37,53,59,255);a.R(0,0,32,32,Ink);a.Box(1,1,30,30,stone);a.L(1,15,30,15,Dark(stone));a.L(13,1,13,14,Dark(stone));a.L(22,16,22,30,Dark(stone));for(int i=0;i<12;i++){int x=(i*13+variant*7)%28+2,y=(i*7+variant*3)%28+2;a.R(x,y,2,1,i%3==0?new Color32(59,89,71,255):Light(stone));}return cache[key]=a.Finish();
    }
}
