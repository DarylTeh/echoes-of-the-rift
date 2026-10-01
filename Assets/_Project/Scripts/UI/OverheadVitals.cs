using UnityEngine;

public sealed class OverheadVitals:MonoBehaviour
{
    private Transform frame,fill;
    private Combatant actor;
    private SpriteRenderer[] sprites;private SpriteRenderer bar;
    private float nextBoundsRefresh,nextValueRefresh;
    private float topOffset=.92f,lastRatio=-1;
    private Vector3 lastScale;
    private Color lastBarColor;
    private void Start()
    {
        actor=GetComponent<Combatant>();
        sprites=GetComponentsInChildren<SpriteRenderer>(true);
        frame=new GameObject("OverheadHP").transform;frame.SetParent(transform,false);
        var border=frame.gameObject.AddComponent<SpriteRenderer>();border.sprite=CombatVisual.Square;border.color=new Color32(17,17,35,255);border.sortingOrder=80;
        fill=new GameObject("HealthFill").transform;fill.SetParent(frame,false);
        bar=fill.gameObject.AddComponent<SpriteRenderer>();bar.sprite=CombatVisual.Square;bar.sortingOrder=81;
    }
    private void LateUpdate()
    {
        if(frame==null)return;
        bool visible=actor==null||actor.Alive;
        if(frame.gameObject.activeSelf!=visible)frame.gameObject.SetActive(visible);
        if(!visible)return;
        if(Time.unscaledTime>=nextBoundsRefresh)
        {
            nextBoundsRefresh=Time.unscaledTime+1f/15;
            float top=transform.position.y+.7f;
            foreach(var sprite in sprites)if(sprite!=null&&sprite.enabled&&sprite.gameObject.activeInHierarchy)top=Mathf.Max(top,sprite.bounds.max.y);
            topOffset=top-transform.position.y+.12f;
        }
        frame.position=new Vector3(transform.position.x,transform.position.y+topOffset,0);
        Vector3 scale=transform.lossyScale;
        if(scale!=lastScale){lastScale=scale;frame.localScale=new Vector3(.65f/Mathf.Max(.001f,scale.x),.09f/Mathf.Max(.001f,scale.y),1);}
        if(Time.unscaledTime<nextValueRefresh)return;
        nextValueRefresh=Time.unscaledTime+1f/20;
        float ratio=actor==null?1:Mathf.Clamp01(actor.Health/Mathf.Max(1,actor.MaximumHealth));
        if(Mathf.Abs(ratio-lastRatio)>.001f){lastRatio=ratio;fill.localScale=new Vector3(ratio*.92f,.52f,1);fill.localPosition=new Vector3((ratio-1)*.46f,0,0);}
        Color color=actor==null||actor.IsPlayer?new Color32(72,244,147,255):new Color32(255,85,124,255);
        if(color!=lastBarColor){lastBarColor=color;bar.color=color;}
    }
    private void OnDestroy(){if(frame!=null)Destroy(frame.gameObject);}
}
