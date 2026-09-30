using UnityEngine;

public sealed class OverheadVitals:MonoBehaviour
{
    private Transform frame,fill;
    private Combatant actor;
    private SpriteRenderer[] sprites;private SpriteRenderer bar;
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
        float ratio=actor==null?1:actor.Health/Mathf.Max(1,actor.MaximumHealth);
        frame.gameObject.SetActive(actor==null||actor.Alive);
        float top=transform.position.y+.7f;
        foreach(var sprite in sprites)if(sprite!=null&&sprite.enabled&&sprite.gameObject.activeInHierarchy)top=Mathf.Max(top,sprite.bounds.max.y);
        frame.position=new Vector3(transform.position.x,top+.12f,0);
        frame.localScale=new Vector3(.65f/transform.lossyScale.x,.09f/transform.lossyScale.y,1);
        fill.localScale=new Vector3(Mathf.Clamp01(ratio)*.92f,.52f,1);fill.localPosition=new Vector3((Mathf.Clamp01(ratio)-1)*.46f,0,0);
        bar.color=actor==null||actor.IsPlayer?new Color32(72,244,147,255):new Color32(255,85,124,255);
    }
    private void OnDestroy(){if(frame!=null)Destroy(frame.gameObject);}
}
