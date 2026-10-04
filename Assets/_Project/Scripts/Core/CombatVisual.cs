using UnityEngine;

public sealed class CombatVisual : MonoBehaviour
{
    public static event System.Action<Vector3,float,Color,float> Created;
    public static event System.Action<Vector3,Vector2,float,float,Color> Swung;
    private static Sprite square;
    public static Sprite Square
    {
        get
        {
            if(square!=null) return square;
            var texture=new Texture2D(1,1); texture.SetPixel(0,0,Color.white); texture.Apply(); texture.filterMode=FilterMode.Point;
            square=Sprite.Create(texture,new Rect(0,0,1,1),Vector2.one*0.5f,1); return square;
        }
    }
    private float remaining,total;
    private bool burst;
    private Color tint;
    private LineRenderer cachedLine;
    private SpriteRenderer[] cachedSparks;
    private SpriteRenderer cachedFlash;
    private static Material sharedLineMaterial;
    private static readonly System.Collections.Generic.Queue<CombatVisual> idle=new System.Collections.Generic.Queue<CombatVisual>();
    private static int burstCount;
    private static int BurstBudget=>Application.isMobilePlatform?QualitySettings.GetQualityLevel()<=1?16:24:QualitySettings.GetQualityLevel()<=1?24:48;
    public static void Pulse(Vector3 position,float radius,Color color,float duration)
    {
        Created?.Invoke(position,radius,color,duration);
        Emit(position,radius,color,duration,Vector2.right,360);
    }
    public static void Slash(Vector3 position,Vector2 aim,float radius,float arc,Color color)
    {
        Swung?.Invoke(position,aim,radius,arc,color);Emit(position,radius,color,.22f,aim,arc);
    }
    private static void Emit(Vector3 position,float radius,Color color,float duration,Vector2 aim,float arc)
    {
        if(Application.isBatchMode)return;
        bool isBurst=duration<=.5f;
        CombatVisual effect=null;
        if(isBurst)
        {
            while(idle.Count>0&&effect==null)effect=idle.Dequeue();
            if(effect==null&&burstCount>=BurstBudget)return;
        }
        if(effect==null)
        {
            effect=new GameObject("Impact").AddComponent<CombatVisual>();effect.burst=isBurst;
            effect.cachedLine=effect.gameObject.AddComponent<LineRenderer>();
            var line=effect.cachedLine;line.useWorldSpace=false;line.loop=true;line.positionCount=32;line.sortingOrder=5;
            if(sharedLineMaterial==null)sharedLineMaterial=new Material(Shader.Find("Sprites/Default"));
            line.sharedMaterial=sharedLineMaterial;line.startWidth=line.endWidth=isBurst?.13f:.07f;
            if(isBurst)
            {
                var flashObject=new GameObject("IllustratedImpact");flashObject.transform.SetParent(effect.transform,false);
                effect.cachedFlash=flashObject.AddComponent<SpriteRenderer>();effect.cachedFlash.sortingOrder=4;
            }
            effect.cachedSparks=new SpriteRenderer[isBurst?12:0];
            for(int i=0;i<effect.cachedSparks.Length;i++)
            {
                var spark=new GameObject("PrismaticSpark").AddComponent<SpriteRenderer>();spark.transform.SetParent(effect.transform,false);
                spark.transform.localScale=Vector3.one*.13f;spark.transform.localRotation=Quaternion.Euler(0,0,45);
                spark.sprite=Square;spark.sortingOrder=9;effect.cachedSparks[i]=spark;
            }
            if(isBurst)burstCount++;
        }
        effect.gameObject.SetActive(true);effect.transform.position=position;effect.transform.localScale=Vector3.one;
        effect.remaining=effect.total=duration;effect.tint=color;effect.cachedLine.startColor=effect.cachedLine.endColor=color;
        effect.cachedLine.loop=arc>=359;
        if(effect.cachedFlash!=null)
        {
            var flash=effect.cachedFlash;flash.sprite=IllustratedArt.CombatEffect(color,arc<359);
            flash.sharedMaterial=IllustratedArt.Owns(flash.sprite)?IllustratedArt.World:null;flash.color=Color.white;
            flash.transform.localScale=Vector3.one*Mathf.Max(.2f,radius);
            float angle=arc<359?Mathf.Atan2(aim.y,aim.x)*Mathf.Rad2Deg:0;flash.transform.localRotation=Quaternion.Euler(0,0,angle);
        }
        for(int i=0;i<32;i++){float angle=Mathf.Atan2(aim.y,aim.x)+(i/31f-.5f)*arc*Mathf.Deg2Rad;effect.cachedLine.SetPosition(i,new Vector3(Mathf.Cos(angle)*radius,Mathf.Sin(angle)*radius,0));}
        for(int i=0;i<effect.cachedSparks.Length;i++){float angle=Mathf.Atan2(aim.y,aim.x)+(i/11f-.5f)*arc*Mathf.Deg2Rad;var spark=effect.cachedSparks[i];spark.transform.localPosition=new Vector3(Mathf.Cos(angle),Mathf.Sin(angle),0)*radius*.6f;spark.color=i%3==0?Color.white:color;}
    }
    private void Update()
    {
        remaining-=Time.deltaTime;
        if(burst)
        {
            float t=1-Mathf.Clamp01(remaining/Mathf.Max(.01f,total));transform.localScale=Vector3.one*(.6f+t*.7f);
            var color=Color.Lerp(Color.white,tint,t);color.a=1-t;cachedLine.startColor=cachedLine.endColor=color;
            if(cachedFlash!=null){var flashColor=Color.white;flashColor.a=1-t;cachedFlash.color=flashColor;}
            foreach(var sprite in cachedSparks){var c=sprite.color;c.a=1-t;sprite.color=c;}
        }
        if(remaining>0)return;
        if(burst){gameObject.SetActive(false);idle.Enqueue(this);}else Destroy(gameObject);
    }
    private void OnDestroy(){if(burst)burstCount--;}
}
