using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>Solo, deterministic first release of the skill-book Rift Defense mode.</summary>
public sealed class RiftDefenseMode : MonoBehaviour
{
    // Random Dice co-op footage repeatedly uses a compact 5-by-3 dice board.
    // Keep this solo board legible on a phone now; the second board comes with authoritative co-op.
    private const int Rows=3, Columns=5, MaxWaves=20;
    private const float SummonCost=10, UpgradeCost=14;
    private static readonly Color[] RankColors={new Color32(92,205,255,255),new Color32(154,112,255,255),new Color32(255,101,209,255),new Color32(255,188,74,255),new Color32(255,82,114,255)};
    private static readonly Color[] EnemyColors={new Color32(255,183,84,255),new Color32(134,156,181,255),new Color32(131,255,182,255),new Color32(138,205,255,255),new Color32(231,119,255,255)};
    private static readonly string[] BossNames={"Cinder Knight","Mire Matron","Clockwork Colossus","Ashen Gatekeeper"};
    private readonly Tower[] towers=new Tower[Rows*Columns];
    private readonly List<Enemy> enemies=new List<Enemy>(24);
    private readonly List<BookPick> deck=new List<BookPick>(5);
    private readonly List<UnityEngine.UI.Button> cells=new List<UnityEngine.UI.Button>(Rows*Columns);
    private RectTransform root, board;
    private TMP_Text waveText, manaText, coreText, hintText, selectedText, statusText;
    private UnityEngine.UI.Button summonButton,upgradeButton,mergeButton,castButton;
    private ArenaGame game;
    private int mana=50, core=100, wave, selected=-1, bossCount, lastBossWave;
    private float waveBreak, tickTimer, castCooldown;
    private bool running, completed, testFast, waveSpawned;
    private Coroutine smoke;
    public int WavesCleared=>wave;
    public int CoreHealth=>core;
    public int TowerCount { get { int n=0;foreach(var tower in towers)if(tower!=null)n++;return n; } }
    public int BossCount=>bossCount;
    public bool IsRunning=>running;
    public bool IsComplete=>completed;
    public int Mana=>mana;

    private sealed class BookPick { public SpellData Spell; public string ItemId; public Sprite Icon; public Color Color; }
    private sealed class Tower { public BookPick Book; public int Rank=1; public float Cooldown; }
    private sealed class Enemy { public int Lane; public float Progress,Health,MaxHealth,Speed; public bool Boss; public GameObject View,Bar,HealthFill; }

    public void Configure(ArenaGame arena) { game=arena; }

    public void Open()
    {
        if(root!=null)return;
        if(game==null||game.Player==null||game.Player.GetComponent<PlayerController>()==null)return;
        BuildDeck();
        if(deck.Count==0){game.SetStatus("No skill books are available for this run.");return;}
        game.Player.GetComponent<PlayerController>().ControlsEnabled=false;
        root=GameUI.Canvas("RiftDefense");
        var panel=GameUI.Panel(root,"RiftDefensePanel",Vector2.zero,new Vector2(1160,650));
        GameUI.Label(panel,"RIFT DEFENSE",new Vector2(-500,284),new Vector2(250,34),26);
        waveText=GameUI.Label(panel,"WAVE 0 / 20",new Vector2(-178,284),new Vector2(180,32),23);waveText.alignment=TextAlignmentOptions.Center;
        coreText=GameUI.Label(panel,"CORE 100%",new Vector2(20,284),new Vector2(180,32),23);coreText.alignment=TextAlignmentOptions.Center;coreText.color=new Color32(126,238,174,255);
        manaText=GameUI.Label(panel,"MANA 50",new Vector2(206,284),new Vector2(145,32),23);manaText.alignment=TextAlignmentOptions.Center;manaText.color=new Color32(96,213,255,255);
        var close=GameUI.Button(panel,"×",new Vector2(526,284),new Vector2(54,48),Close);close.name="CloseRiftDefense";
        board=GameUI.Panel(panel,"RiftDefenseBoard",new Vector2(-204,-18),new Vector2(700,468));
        BuildBoard();
        var side=GameUI.Panel(panel,"RiftDefenseControls",new Vector2(376,-18),new Vector2(332,468));
        GameUI.Label(side,"RIFT DECK · 5 BOOKS",new Vector2(0,196),new Vector2(280,28),19).alignment=TextAlignmentOptions.Center;
        for(int i=0;i<deck.Count;i++)
        {
            int index=i;var card=GameUI.Button(side,"",new Vector2(-104+(i%3)*104,140-(i/3)*92),new Vector2(88,76),()=>SelectDeck(index));card.name="DefenseDeck"+i;
            GameUI.Icon(card.transform,deck[i].Icon,Vector2.zero,new Vector2(48,48));
            GameUI.Label(card.transform,deck[i].Spell.DisplayName,new Vector2(0,-27),new Vector2(82,18),10).alignment=TextAlignmentOptions.Center;
        }
        selectedText=GameUI.Label(side,"Tap an empty cell · random book",new Vector2(0,25),new Vector2(290,28),15);selectedText.alignment=TextAlignmentOptions.Center;
        summonButton=GameUI.Button(side,"SUMMON · 10 MANA",new Vector2(0,-24),new Vector2(272,46),SummonSelected);summonButton.name="DefenseSummon";
        upgradeButton=GameUI.Button(side,"UPGRADE · 14 MANA",new Vector2(0,-80),new Vector2(272,42),UpgradeSelected);upgradeButton.name="DefenseUpgrade";
        mergeButton=GameUI.Button(side,"MERGE MATCHING",new Vector2(0,-130),new Vector2(272,42),MergeSelected);mergeButton.name="DefenseMerge";
        castButton=GameUI.Button(side,"HERO BURST · READY",new Vector2(0,-181),new Vector2(272,42),HeroBurst);castButton.name="DefenseHeroCast";
        hintText=GameUI.Label(side,"Summon a random deck book. Merge matching books\nof the same star rank. Elite bosses every 5 waves.",new Vector2(0,-222),new Vector2(290,46),12);hintText.alignment=TextAlignmentOptions.Center;
        statusText=GameUI.Label(panel,"20 waves · elite boss every 5 waves",new Vector2(-204,-267),new Vector2(690,26),15);statusText.alignment=TextAlignmentOptions.Center;
        mana=50;core=100;wave=0;selected=-1;waveBreak=tickTimer=castCooldown=0;bossCount=0;lastBossWave=0;completed=false;running=true;waveSpawned=false;
        Refresh();
    }

    private void BuildDeck()
    {
        deck.Clear();
        var owned=game.Forge.State.Items;
        foreach(var stack in owned)
        {
            if(stack==null||stack.Count<1)continue;
            var item=Array.Find(game.Items,x=>x!=null&&x.Id==stack.ItemId&&x.Kind==ItemKind.SkillBook&&x.Spell!=null);
            if(item==null||deck.Exists(x=>x.Spell.Id==item.Spell.Id))continue;
            deck.Add(new BookPick {Spell=item.Spell,ItemId=item.Id,Icon=IllustratedArt.Item(item),Color=item.Spell.Color});
            if(deck.Count==5)break;
        }
        // The class's six actual active skills form a starter trial deck, so new players can try the mode.
        if(deck.Count<5)
        {
            var skills=game.Player.GetComponent<SkillStanceSwapper>();
            for(int i=0;i<6&&deck.Count<5;i++)
            {
                var spell=skills.GetSpell(i);if(spell==null||deck.Exists(x=>x.Spell.Id==spell.Id))continue;
                var item=Array.Find(game.Items,x=>x!=null&&x.Spell==spell);
                deck.Add(new BookPick {Spell=spell,ItemId=item!=null?item.Id:"trial-"+spell.Id,Icon=spell.skillIcon!=null?spell.skillIcon:IllustratedArt.Book(spell),Color=spell.Color});
            }
        }
    }

    private void BuildBoard()
    {
        for(int lane=0;lane<Rows;lane++)
        {
            var label=GameUI.Label(board,"CORE",new Vector2(-270,110-lane*82),new Vector2(58,22),12);label.alignment=TextAlignmentOptions.Center;label.color=new Color32(255,116,129,255);
            for(int col=0;col<Columns;col++)
            {
                int cell=lane*Columns+col;
                var button=GameUI.Button(board,"",new Vector2(-190+col*72,110-lane*82),new Vector2(64,66),()=>TapCell(cell));button.name="RiftCell"+cell;
                cells.Add(button);
            }
        }
    }

    private void Update()
    {
        if(!running)return;
        float delta=Time.deltaTime*(testFast?18f:1f);tickTimer+=delta;waveBreak=Mathf.Max(0,waveBreak-delta);castCooldown=Mathf.Max(0,castCooldown-delta);
        if(waveBreak<=0)
        {
            if(!waveSpawned&&wave<=MaxWaves){SpawnEnemy();waveSpawned=true;}
            else if(waveSpawned&&enemies.Count==0)
            {
                if(wave>=MaxWaves){Finish(true);return;}
                wave++;waveSpawned=false;waveBreak=wave%5==0?4f:2.5f;mana=Mathf.Min(80,mana+8);Refresh();
            }
        }
        for(int i=enemies.Count-1;i>=0;i--)
        {
            var enemy=enemies[i];enemy.Progress-=enemy.Speed*delta;
            float x=-190+enemy.Progress*72,y=110-enemy.Lane*82;
            if(enemy.View!=null)enemy.View.transform.localPosition=new Vector3(x,y,-1);
            if(enemy.Bar!=null)enemy.Bar.transform.localPosition=new Vector3(x,y-23,-2);
            if(enemy.HealthFill!=null){var fill=enemy.HealthFill.GetComponent<RectTransform>();fill.sizeDelta=new Vector2(26*Mathf.Clamp01(enemy.Health/enemy.MaxHealth),4);}
            if(enemy.Progress<-.35f){core=Mathf.Max(0,core-(enemy.Boss?22:enemy.MaxHealth>20?7:3));RemoveEnemy(enemy);Refresh();}
        }
        for(int i=0;i<towers.Length;i++)
        {
            var tower=towers[i];if(tower==null)continue;tower.Cooldown-=delta;if(tower.Cooldown>0)continue;
            Enemy target=TargetFor(i,tower);if(target==null)continue;
            float damage=tower.Book.Spell.Power*Mathf.Pow(1.32f,tower.Rank-1)*.34f;
            target.Health-=damage;tower.Cooldown=Mathf.Max(.24f,tower.Book.Spell.Cooldown*.65f/Mathf.Sqrt(tower.Rank));
            if(target.Health<=0)Kill(target);
        }
        if(core<=0){Finish(false);return;}
        if(tickTimer>=.12f){tickTimer=0;Refresh();}
    }

    private void SpawnEnemy()
    {
        if(wave==0){wave=1;waveBreak=1.5f;}
        bool bossWave=wave%5==0;
        int count=2+(wave/3)+(bossWave?1:0);
        if(enemies.Count>=24)return;
        int lane=(wave*7+enemies.Count*3+enemies.Count/2)%Rows;
        for(int n=0;n<count&&enemies.Count<24;n++)
        {
            bool boss=bossWave&&n==count-1;
            int bossType=(wave/5)-1;
            int archetype=(wave+n)%5;
            float hp=boss?(wave==MaxWaves?680:220+wave*14):22+wave*8;
            float speed=boss?(bossType==0?.22f:.14f):.24f+wave*.003f;
            if(!boss&&archetype==0)speed*=1.55f; // runner
            if(!boss&&archetype==1)hp*=1.8f;     // shellback
            if(!boss&&archetype==2)hp*=.62f;     // swarmling
            var enemy=new Enemy {Lane=(lane+n)%Rows,Progress=Columns-.5f,MaxHealth=hp,Health=hp,Speed=speed,Boss=boss};
            if(boss&&lastBossWave!=wave){bossCount++;lastBossWave=wave;}
            var sprite=game.Slime!=null?game.Slime:PixelArt.Icon("book");
            var icon=GameUI.Icon(board,sprite,new Vector2(-190+enemy.Progress*72,110-enemy.Lane*82),boss?new Vector2(52,52):new Vector2(28,28));
            icon.name=boss?"RiftBoss_"+BossNames[Mathf.Clamp(bossType,0,BossNames.Length-1)].Replace(" ",""):"RiftEnemy"+archetype;
            icon.color=boss?(Color)(wave==MaxWaves?new Color32(255,64,139,255):new Color32(255,170,82,255)):EnemyColors[archetype];enemy.View=icon.gameObject;enemies.Add(enemy);
            var bar=GameUI.Rect("EnemyHealthBar",board,Vector2.one*.5f,new Vector2(-190+enemy.Progress*72,87-enemy.Lane*82),new Vector2(boss?48:30,6));bar.gameObject.AddComponent<UnityEngine.UI.Image>().color=new Color32(20,18,31,255);enemy.Bar=bar.gameObject;
            var fillRect=GameUI.Rect("EnemyHealthFill",bar,Vector2.zero,Vector2.zero,new Vector2(boss?44:26,4));fillRect.anchorMin=new Vector2(0,0.5f);fillRect.anchorMax=new Vector2(0,0.5f);fillRect.pivot=new Vector2(0,0.5f);fillRect.anchoredPosition=new Vector2(1,0);fillRect.gameObject.AddComponent<UnityEngine.UI.Image>().color=boss?new Color32(255,69,161,255):new Color32(255,93,106,255);enemy.HealthFill=fillRect.gameObject;
        }
        statusText.text=bossWave?"ELITE WAVE · "+BossNames[Mathf.Clamp((wave/5)-1,0,BossNames.Length-1)].ToUpperInvariant():"Defend all lanes · wave "+wave;
    }

    private Enemy TargetFor(int cell,Tower tower)
    {
        int lane=cell/Columns,col=cell%Columns;Enemy target=null;float best=float.MaxValue;
        foreach(var enemy in enemies)
        {
            if(Mathf.Abs(enemy.Lane-lane)>1)continue;float dist=Mathf.Abs(enemy.Progress-col)+Mathf.Abs(enemy.Lane-lane)*1.8f;
            float range=3.1f+Mathf.Min(1.5f,tower.Rank*.18f);if(dist<=range&&enemy.Progress<best){best=enemy.Progress;target=enemy;}
        }
        return target;
    }

    private void Kill(Enemy enemy)
    {
        RemoveEnemy(enemy);mana=Mathf.Min(80,mana+(enemy.Boss?24:3));
    }
    private void RemoveEnemy(Enemy enemy){if(enemy.View!=null)Destroy(enemy.View);if(enemy.Bar!=null)Destroy(enemy.Bar);enemies.Remove(enemy);}

    private void TapCell(int cell)
    {
        if(!running)return;
        if(towers[cell]==null){selected=cell;if(mana>=SummonCost)Summon(cell);else selectedText.text="Need 10 Rift Mana";}
        else if(selected>=0&&selected!=cell&&towers[selected]!=null&&towers[selected].Book.Spell.Id==towers[cell].Book.Spell.Id)Merge(cell);
        else {selected=cell;selectedText.text="Selected rank "+towers[cell].Rank+" · "+towers[cell].Book.Spell.DisplayName;}
        Refresh();
    }
    private void SelectDeck(int index){if(index<0||index>=deck.Count)return;selectedText.text=deck[index].Spell.DisplayName+" · summoned randomly from deck";}
    private void SummonSelected(){if(selected<0||selected>=towers.Length){selectedText.text="Tap an empty board cell first";return;}Summon(selected);}
    private void Summon(int cell)
    {
        if(cell<0||cell>=towers.Length||towers[cell]!=null||mana<SummonCost)return;
        mana-=(int)SummonCost;var book=deck[UnityEngine.Random.Range(0,deck.Count)];towers[cell]=new Tower{Book=book};selected=cell;
        selectedText.text=book.Spell.DisplayName+" · rank 1";
    }
    private void UpgradeSelected()
    {
        if(selected<0||towers[selected]==null){selectedText.text="Select a tower first";return;}
        var tower=towers[selected];if(tower.Rank>=5){selectedText.text="This tower is at max rank";return;}if(mana<UpgradeCost){selectedText.text="Need 14 Rift Mana";return;}
        mana-=(int)UpgradeCost;tower.Rank++;selectedText.text="Upgraded to rank "+tower.Rank;
    }
    private void MergeSelected()
    {
        if(selected<0||towers[selected]==null){selectedText.text="Select a tower to merge";return;}
        selectedText.text="Tap another matching tower to merge";
    }
    private void Merge(int other)
    {
        int rank=towers[selected].Rank;if(rank>=5){selectedText.text="Maximum merge rank reached";return;}
        if(rank!=towers[other].Rank){selectedText.text="Merge matching star ranks";return;}
        towers[other]=new Tower{Book=deck[UnityEngine.Random.Range(0,deck.Count)],Rank=rank+1};towers[selected]=null;selected=other;selectedText.text="Fusion roll · rank "+towers[other].Rank+" "+towers[other].Book.Spell.DisplayName;
    }
    private void HeroBurst()
    {
        if(castCooldown>0){selectedText.text="Hero burst recharging";return;}
        if(mana<15){selectedText.text="Need 15 Rift Mana";return;}
        mana-=15;castCooldown=12;
        for(int i=enemies.Count-1;i>=0;i--){var enemy=enemies[i];enemy.Health-=enemy.Boss?130:90;if(enemy.Health<=0)Kill(enemy);}
        selectedText.text="Rift burst!";
    }

    private void Refresh()
    {
        if(root==null)return;
        waveText.text="WAVE "+wave+" / "+MaxWaves;coreText.text="CORE "+core+"%";coreText.color=core<35?new Color32(255,100,115,255):new Color32(126,238,174,255);manaText.text="MANA "+mana;
        summonButton.interactable=mana>=SummonCost;upgradeButton.interactable=mana>=UpgradeCost&&selected>=0&&towers[selected]!=null;castButton.interactable=castCooldown<=0&&mana>=15;castButton.GetComponentInChildren<TMP_Text>().text=castCooldown<=0?"HERO BURST · READY":"HERO BURST · "+Mathf.CeilToInt(castCooldown)+"s";
        for(int i=0;i<towers.Length;i++)
        {
            var button=cells[i];var tower=towers[i];var text=button.GetComponentInChildren<TMP_Text>();var image=button.GetComponent<UnityEngine.UI.Image>();
            text.text=tower==null?"+":"R"+tower.Rank;image.color=tower==null?(Color)new Color32(39,36,56,255):Color.Lerp(new Color32(39,36,56,255),RankColors[Mathf.Clamp(tower.Rank-1,0,4)],.34f);
            if(tower!=null){var icon=button.transform.Find("Icon")?.GetComponent<UnityEngine.UI.Image>();if(icon==null)GameUI.Icon(button.transform,tower.Book.Icon,new Vector2(0,-7),new Vector2(38,38));else icon.sprite=tower.Book.Icon;}
        }
    }

    private void Finish(bool victory)
    {
        running=false;completed=victory;statusText.text=victory?"RIFT CLEARED · 20 / 20":"CORE BREACHED · BEST WAVE "+wave;
        GameUI.Button(root,victory?"Return to Haven":"Try Again",new Vector2(390,-274),new Vector2(220,44),()=>{if(victory)Close();else Restart();}).name="RiftDefenseFinish";
        if(victory)game.SetStatus("Rift Defense cleared. Rewards will unlock with server-authoritative mode support.");
    }
    private void Restart(){ClearRun();if(root!=null){Destroy(root.gameObject);root=null;}Open();}
    public void StartSmokeTest()
    {
        UnityEngine.Random.InitState(20261004);
        testFast=true;Open();if(root==null)return;mana=80;
        // Deterministic stress setup: fill and max-rank every cell to exercise
        // all 20 waves, four bosses, target selection, and victory handling.
        for(int cell=0;cell<towers.Length;cell++){mana=80;selected=cell;Summon(cell);}
        for(int cell=0;cell<towers.Length;cell++)while(towers[cell]!=null&&towers[cell].Rank<5){mana=80;selected=cell;UpgradeSelected();}
        mana=80;Refresh();
        smoke=StartCoroutine(SmokeWatch());
    }
    private IEnumerator SmokeWatch()
    {
        float deadline=Time.realtimeSinceStartup+50;while(running&&Time.realtimeSinceStartup<deadline)yield return null;
        bool passed=completed&&wave==MaxWaves&&bossCount==4&&core>0&&TowerCount>0&&mana>=0;
        System.IO.Directory.CreateDirectory(Application.persistentDataPath);
        System.IO.File.WriteAllText(System.IO.Path.Combine(Application.persistentDataPath,"rift-defense-test.txt"),(passed?"PASS":"FAIL")+" waves="+wave+" boss="+bossCount+" core="+core+" towers="+TowerCount+" mana="+mana);
        Debug.Log("RIFT_DEFENSE_CHECK "+(passed?"PASS":"FAIL")+" waves="+wave+" bosses="+bossCount+" core="+core+" towers="+TowerCount+" mana="+mana);
        testFast=false;if(root!=null)Destroy(root.gameObject);root=null;
    }
    private void ClearRun()
    {
        running=false;waveSpawned=false;for(int i=0;i<enemies.Count;i++){if(enemies[i].View!=null)Destroy(enemies[i].View);if(enemies[i].Bar!=null)Destroy(enemies[i].Bar);}enemies.Clear();Array.Clear(towers,0,towers.Length);cells.Clear();
    }
    public void Close()
    {
        if(smoke!=null){StopCoroutine(smoke);smoke=null;}ClearRun();if(root!=null)Destroy(root.gameObject);root=null;
        if(game!=null&&game.Player!=null&&game.Hub!=null&&game.Hub.IsOpen)game.Player.GetComponent<PlayerController>().ControlsEnabled=!game.Session.UsesDedicated||game.Session.DedicatedServer||game.Session.Authenticated;
    }
    private void OnDestroy()=>Close();
}
