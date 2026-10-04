using UnityEngine;

public sealed class Projectile : MonoBehaviour
{
    private static int nextVisualId;
    public int VisualId { get; private set; }
    private Vector2 direction;
    private float damage,remaining;
    private Combatant owner;
    public string VisualFamily;
    private int pierce;private bool returning,returnPhase;private float life;
    private readonly System.Collections.Generic.HashSet<Combatant> struck=new System.Collections.Generic.HashSet<Combatant>();
    private readonly RaycastHit2D[] hitBuffer=new RaycastHit2D[64];
    private SpriteRenderer rendererCache;
    private ContactFilter2D contactFilter;
    private void Awake(){VisualId=++nextVisualId;contactFilter=new ContactFilter2D{useTriggers=false};}
    public static Projectile Spawn(Vector2 position,Vector2 direction,float damage,float range,Combatant owner,Color? color=null,int pierce=1,bool returning=false,string family=null)
    {
        var go=new GameObject("SpellBolt"); go.transform.position=position;
        var render=go.AddComponent<SpriteRenderer>(); render.sprite=ArcaneBoltVFX.Sprite; render.color=color??GameUI.Gold; render.sortingOrder=8; if(IllustratedArt.Owns(render.sprite))render.sharedMaterial=IllustratedArt.World; go.transform.localScale=new Vector3(.55f,.35f,1);
        if(family!=null){render.sprite=IllustratedArt.Weapon(family)??render.sprite;render.color=Color.white;render.sharedMaterial=IllustratedArt.World;go.transform.localScale=Vector3.one*.55f;}
        go.AddComponent<ArcaneBoltVFX>();
        var projectile=go.AddComponent<Projectile>(); projectile.direction=direction.normalized; projectile.damage=damage; projectile.remaining=range; projectile.owner=owner;projectile.pierce=Mathf.Max(1,pierce);projectile.returning=returning;projectile.VisualFamily=family;projectile.rendererCache=render;return projectile;
    }
    private void Update()
    {
        if(owner==null||!owner.Alive||owner.InSafeZone) { Destroy(gameObject); return; }
        life+=Time.deltaTime;if(life>6){Destroy(gameObject);return;}
        if(returnPhase){Vector2 delta=(Vector2)owner.transform.position+Vector2.up*.45f-(Vector2)transform.position;if(delta.sqrMagnitude<.16f){Destroy(gameObject);return;}direction=delta.normalized;remaining=delta.magnitude;}
        float distance=Mathf.Min(remaining,Time.deltaTime*10);
        int hitCount=Physics2D.CircleCast(transform.position,.16f,direction,contactFilter,hitBuffer,distance);
        for(int i=0;i<hitCount;i++)
        {
            var hit=hitBuffer[i];var target=hit.collider.GetComponent<Combatant>();
            if(target==owner)continue;
            if(target!=null)
            {
                if(target.IsPlayer==owner.IsPlayer||!target.Alive||!struck.Add(target))continue;
                target.Damage(damage);CombatVisual.Pulse(hit.point,.5f,rendererCache.color,.25f);
                if(--pierce>0)continue;
            }
            if(returning&&!returnPhase){BeginReturn();return;}
            Destroy(gameObject);return;
        }
        transform.position+=(Vector3)(direction*distance);remaining-=distance;
        if(remaining<=0){if(returning&&!returnPhase)BeginReturn();else Destroy(gameObject);}
    }
    private void BeginReturn(){returnPhase=true;pierce=8;struck.Clear();}
}
