using UnityEngine;
public sealed class SummonedRune : MonoBehaviour
{
    public Combatant Owner;public float Power;public Color Tint;
    private float remaining=3,next;private int pulses=3;
    private static readonly Collider2D[] targets=new Collider2D[64];
    private void Update(){if(Owner==null||!Owner.Alive){Destroy(gameObject);return;}remaining-=Time.deltaTime;next-=Time.deltaTime;if(next<=0&&pulses>0){pulses--;next=1;CombatVisual.Pulse(transform.position,1.5f,Tint,.7f);int count=Physics2D.OverlapCircleNonAlloc(transform.position,1.5f,targets);for(int i=0;i<count;i++){var target=targets[i]?.GetComponent<Combatant>();if(target!=null&&!target.IsPlayer&&((Vector2)target.transform.position-(Vector2)transform.position).sqrMagnitude<=2.25f)target.Damage(Power/3);}}if(remaining<=0)Destroy(gameObject);}
}
