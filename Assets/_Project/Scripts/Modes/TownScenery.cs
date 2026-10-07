using UnityEngine;
using UnityEngine.Tilemaps;

// Original runtime tilemap assembled on a fixed pixel grid; no reference-game assets.
public static class TownScenery
{
    private static Sprite grass,path,water,gate,lobbyBackdrop;
    public static GameObject Build(GameObject prefab,Vector2[] positions)
    {
        var root=new GameObject("RiftHavenWorld",typeof(Grid));
        var texture=Resources.Load<Texture2D>("Illustrated/HavenLobbyBackdrop");
        if(texture!=null)
        {
            texture.filterMode=FilterMode.Point;texture.wrapMode=TextureWrapMode.Clamp;
            if(lobbyBackdrop==null)lobbyBackdrop=Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),Vector2.one*.5f,texture.width/32f,0,SpriteMeshType.FullRect);
            SpriteObject(root,"HavenLobbyBackdrop",lobbyBackdrop,Vector2.zero,1,Color.white,-20);
        }
        else
        {
            var floor=new GameObject("HavenTilemap",typeof(Tilemap),typeof(TilemapRenderer));floor.transform.SetParent(root.transform,false);floor.transform.localPosition=new Vector3(-.5f,-.5f,0);
            var map=floor.GetComponent<Tilemap>();floor.GetComponent<TilemapRenderer>().sortingOrder=-20;
            if(grass==null)CreateTiles();
            var grassTile=ScriptableObject.CreateInstance<Tile>();grassTile.sprite=grass;
            var pathTile=ScriptableObject.CreateInstance<Tile>();pathTile.sprite=path;
            var waterTile=ScriptableObject.CreateInstance<Tile>();waterTile.sprite=water;
            var cleanup=root.AddComponent<TownTileCleanup>();cleanup.Tiles=new[]{grassTile,pathTile,waterTile};
            for(int y=-9;y<=9;y++)for(int x=-16;x<=16;x++)map.SetTile(new Vector3Int(x,y,0),Mathf.Abs(x)<=1||Mathf.Abs(y)<=1?pathTile:grassTile);
            for(int x=-1;x<=1;x++)for(int y=-1;y<=1;y++)if(x!=0||y!=0)map.SetTile(new Vector3Int(x,y,0),waterTile);
            SpriteObject(root,"RiftObelisk",PixelArt.Icon("skill",4,4),new Vector2(0,.3f),1.2f,new Color32(159,148,255,255),-1);
        }
        for(int i=0;i<5;i++)
        {
            if(texture==null)SpriteObject(root,"Stall-"+TownHubManager.Names[i],PixelArt.Frame(),positions[i]+Vector2.up*.55f,1.4f,Color.HSVToRGB(.08f+i*.15f,.35f,.75f),-5);
            var npc=Object.Instantiate(prefab,root.transform);npc.name=TownHubManager.Names[i];npc.transform.position=positions[i];npc.transform.localScale=Vector3.one;
            var appearance=new CharacterAppearanceData{Race=i,HairStyle=i%3,CustomColors=true,SkinRGB=new Color32(221,177,135,255),HairRGB=Color.HSVToRGB(.08f+i*.16f,.5f,.8f),EyeRGB=new Color32(151,230,217,255)};
            var custom=npc.GetComponent<CharacterCustomizer>();custom.PresentationScale=.375f;custom.Apply(appearance);npc.AddComponent<OverheadVitals>();custom.SetWeapon(new[]{"hammer","bow","staff","amulet","book"}[i],1);
        }
        for(int i=5;i<9;i++)SpriteObject(root,"RealmGate-"+i,gate,positions[i],1.25f,i==6?new Color32(236,198,108,255):new Color32(167,153,220,255),-3);
        return root;
    }
    private static void SpriteObject(GameObject root,string name,Sprite sprite,Vector2 position,float scale,Color tint,int order)
    {
        var go=new GameObject(name,typeof(SpriteRenderer));go.transform.SetParent(root.transform,false);go.transform.position=position;go.transform.localScale=Vector3.one*scale;
        var renderer=go.GetComponent<SpriteRenderer>();renderer.sprite=sprite;renderer.color=tint;renderer.sortingOrder=order;
    }
    private static void CreateTiles()
    {
        var a=new PixelArt.Raster();a.R(0,0,32,32,new Color32(26,53,53,255));
        for(int i=0;i<21;i++){int x=(i*13)%30,y=(i*7)%30;a.L(x,y,x+1,y-2,new Color32(43,78,66,255));}grass=a.Finish();
        a=new PixelArt.Raster();a.R(0,0,32,32,new Color32(43,43,59,255));
        for(int y=0;y<32;y+=8)for(int x=-8;x<32;x+=16){int offset=y%16==0?0:8;a.Box(x+offset,y,16,8,new Color32(76,73,88,255));}path=a.Finish();
        a=new PixelArt.Raster();a.Box(0,0,32,32,new Color32(82,84,104,255));a.R(3,3,26,26,new Color32(27,68,85,255));for(int y=6;y<28;y+=7)a.L(6,y,21,y,new Color32(43,113,128,255));water=a.Finish();
        a=new PixelArt.Raster();a.Box(2,7,7,24,new Color32(191,191,209,255));a.Box(23,7,7,24,new Color32(191,191,209,255));a.Box(4,3,24,7,new Color32(212,210,224,255));a.Diamond(16,6,4,new Color32(115,232,236,255));a.R(9,11,14,20,new Color32(38,32,65,255));a.L(11,29,21,13,new Color32(113,88,173,255));gate=a.Finish();
    }
}
public sealed class TownTileCleanup:MonoBehaviour
{
    public Tile[] Tiles;
    private void OnDestroy(){if(Tiles!=null)foreach(var tile in Tiles)Destroy(tile);}
}
