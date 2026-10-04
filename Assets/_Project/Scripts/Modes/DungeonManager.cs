using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable] public sealed class StageDefinition { public string id; public int width=17; public int height=9; public int enemies=3; public bool boss; public int rewardItem; public int rewardCoins=15; }
[Serializable] public sealed class StageConfiguration { public StageDefinition[] stages; }

public sealed class DungeonManager : MonoBehaviour
{
    public TextAsset Configuration;
    public Sprite Slime,Skeleton;
    public BossPatternSO Boss;
    public Combatant Player;
    public event Action<bool> RoomCleared;
    public int StageIndex { get; private set; }
    public int StageCount=>config?.stages.Length??0;
    public string StageName=>config.stages[StageIndex].id;
    public bool IsCleared=>cleared;
    private StageConfiguration config;
    private readonly Queue<GameObject> enemyPool=new Queue<GameObject>();
    private readonly List<Combatant> active=new List<Combatant>();
    private readonly List<GameObject> tiles=new List<GameObject>();
    private bool cleared;
    private Combatant stageBoss;
    public Combatant StageBoss=>stageBoss;
    public StageDefinition CurrentStage=>config.stages[StageIndex];
    public static StageConfiguration Parse(string json)
    {
        var result=JsonUtility.FromJson<StageConfiguration>(json);
        if(result?.stages==null||result.stages.Length==0||result.stages.Length>20)throw new ArgumentException("A dungeon requires 1–20 stages.");
        var ids=new HashSet<string>();
        foreach(var stage in result.stages)
            if(stage==null||string.IsNullOrWhiteSpace(stage.id)||!ids.Add(stage.id)||stage.width<9||stage.width>41||stage.height<7||stage.height>31||stage.enemies<1||stage.enemies>12||!stage.boss||stage.rewardItem<0||stage.rewardItem>=6||stage.rewardCoins<0||stage.rewardCoins>1000)throw new ArgumentException("Invalid or duplicate stage definition.");
        return result;
    }
    public void SetReplicaCleared(bool value){cleared=value;}
    public void StartRun() { config=Parse(Configuration.text); StageIndex=0; LoadStage(); }
    public void LoadReplicaStage(int index) { config=Parse(Configuration.text); StageIndex=Mathf.Clamp(index,0,config.stages.Length-1); LoadStage(true); }
    public void NextRoom() { if(!cleared||StageIndex+1>=StageCount)return; StageIndex++; LoadStage(); }
    public void Clear()
    {
        foreach(var enemy in active) { enemy.Died-=OnEnemyDied; enemy.gameObject.SetActive(false); enemyPool.Enqueue(enemy.gameObject); }
        active.Clear(); foreach(var tile in tiles)tile.SetActive(false);
        foreach(var rune in FindObjectsByType<SummonedRune>())Destroy(rune.gameObject);
        foreach(var projectile in FindObjectsByType<Projectile>(FindObjectsSortMode.None))Destroy(projectile.gameObject);
        foreach(var visual in FindObjectsByType<CombatVisual>(FindObjectsSortMode.None))Destroy(visual.gameObject);
    }
    private void LoadStage(bool replica=false)
    {
        Clear(); cleared=false; stageBoss=null; Player.transform.position=new Vector3(0,-2,0);
        var stage=config.stages[StageIndex]; int index=0;
        for(int y=0;y<stage.height;y++)for(int x=0;x<stage.width;x++)
        {
            GameObject tile;
            if(index<tiles.Count)tile=tiles[index]; else { tile=new GameObject("PooledFlagstone",typeof(SpriteRenderer),typeof(BoxCollider2D)); tile.transform.SetParent(transform,false); tiles.Add(tile); }
            index++; tile.SetActive(true); tile.transform.position=new Vector3(x-(stage.width-1)*0.5f,y-(stage.height-1)*0.5f,1); tile.transform.localScale=new Vector3(0.97f,0.97f,1);
            bool wall=x==0||y==0||x==stage.width-1||y==stage.height-1;
            var render=tile.GetComponent<SpriteRenderer>(); render.sprite=IllustratedArt.FloorTile(wall,(x+y)%4)??PixelArt.Floor(wall,(x+y)%3); render.sharedMaterial=IllustratedArt.Owns(render.sprite)?IllustratedArt.World:null; render.sortingOrder=-10;
            render.color=Color.white;
            tile.GetComponent<BoxCollider2D>().enabled=wall;
        }
        if(replica)return;
        int count=stage.enemies+1;
        for(int i=0;i<count;i++)
        {
            bool isBoss=i==stage.enemies;
            GameObject go;
            if(enemyPool.Count>0)go=enemyPool.Dequeue();
            else { go=new GameObject("PooledEnemy",typeof(SpriteRenderer),typeof(CircleCollider2D),typeof(Combatant),typeof(EnemyBrain)); go.transform.SetParent(transform,false); }
            go.SetActive(true); go.transform.position=new Vector3((i%5-2)*1.5f,1.5f+(i/5)*0.5f,0); go.transform.localScale=Vector3.one*(isBoss?2:1);
            var renderer=go.GetComponent<SpriteRenderer>();
            var creature=IllustratedArt.Monster((i*3+StageIndex)%12);
            renderer.sprite=isBoss?BossRoster.Sprite(StageIndex):(creature!=null?creature:(i%2==0?Slime:Skeleton)); renderer.color=Color.white; renderer.sharedMaterial=IllustratedArt.Owns(renderer.sprite)?IllustratedArt.World:null; renderer.sortingOrder=1;
            var collider=go.GetComponent<CircleCollider2D>(); collider.enabled=true; collider.radius=0.3f; collider.offset=new Vector2(0,0.3f);
            var health=go.GetComponent<Combatant>(); health.IsPlayer=false; health.SimulationEnabled=true; health.ResetHealth(isBoss?100+StageIndex*55:35+StageIndex*10); health.Died+=OnEnemyDied;
            var brain=go.GetComponent<EnemyBrain>(); brain.Pooled=true; brain.Target=Player; brain.Pattern=isBoss?BossRoster.Pattern(StageIndex):null; brain.ResetBrain(); active.Add(health); if(isBoss)stageBoss=health;
        }
    }
    private void OnEnemyDied(Combatant enemy)
    {
        if(!active.Remove(enemy))return;
        enemy.Died-=OnEnemyDied; enemy.gameObject.SetActive(false); enemyPool.Enqueue(enemy.gameObject);
        if(enemy==stageBoss&&!cleared)
        {
            cleared=true;
            foreach(var survivor in active)survivor.SimulationEnabled=false;
            RoomCleared?.Invoke(StageIndex+1==StageCount);
        }
    }
    private void OnDestroy() { foreach(var enemy in active) if(enemy!=null)enemy.Died-=OnEnemyDied; }
}
