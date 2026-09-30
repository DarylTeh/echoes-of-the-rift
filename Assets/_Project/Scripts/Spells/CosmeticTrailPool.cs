using UnityEngine;

// Scene-owned, hard-capped cosmetic pool. Saturation drops decoration, never attacks.
public sealed class CosmeticTrailPool : MonoBehaviour
{
    private static CosmeticTrailPool instance;
    public static int Capacity=>Application.isMobilePlatform?64:128;
    public static int CreatedCount=>instance==null?0:instance.created;
    public static int ActiveCount=>instance==null?0:instance.active;
    private SpriteRenderer[] renderers;
    private float[] expires;
    private Color[] colors;
    private int created,active,cursor;
    private Material normal;
    private void Awake()
    {
        renderers=new SpriteRenderer[Capacity];expires=new float[Capacity];colors=new Color[Capacity];
        normal=new Material(Shader.Find("Sprites/Default"));
    }
    public static void Emit(Sprite sprite,Color color,Vector3 position,Quaternion rotation,Vector3 scale,int layer,int order,Material material=null)
    {
        if(Application.isBatchMode)return;
        if(instance==null)instance=new GameObject("CosmeticTrailPool").AddComponent<CosmeticTrailPool>();
        instance.Spawn(sprite,color,position,rotation,scale,layer,order,material);
    }
    private void Spawn(Sprite sprite,Color color,Vector3 position,Quaternion rotation,Vector3 scale,int layer,int order,Material material)
    {
        int index=-1;
        for(int n=0;n<renderers.Length;n++){int candidate=(cursor+n)%renderers.Length;if(expires[candidate]<=Time.unscaledTime){index=candidate;break;}}
        if(index<0)return;
        cursor=(index+1)%renderers.Length;
        var renderer=renderers[index];
        if(renderer==null){renderer=new GameObject("PooledTrail").AddComponent<SpriteRenderer>();renderer.transform.SetParent(transform,false);renderers[index]=renderer;created++;}
        renderer.gameObject.layer=layer;renderer.transform.SetPositionAndRotation(position,rotation);renderer.transform.localScale=scale;
        renderer.sprite=sprite;renderer.sharedMaterial=material!=null?material:normal;renderer.sortingOrder=order;
        color.a*=.65f;colors[index]=color;renderer.color=color;renderer.enabled=true;expires[index]=Time.unscaledTime+.45f;
    }
    private void LateUpdate()
    {
        active=0;
        for(int i=0;i<renderers.Length;i++)
        {
            var renderer=renderers[i];if(renderer==null||!renderer.enabled)continue;
            float remaining=expires[i]-Time.unscaledTime;
            if(remaining<=0){renderer.enabled=false;continue;}
            active++;Color color=colors[i];color.a*=remaining/.45f;renderer.color=color;
        }
    }
    private void OnDestroy(){if(instance==this)instance=null;if(normal!=null)Destroy(normal);}
}
