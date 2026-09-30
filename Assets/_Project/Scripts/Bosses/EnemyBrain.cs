using UnityEngine;

[RequireComponent(typeof(Combatant))]
public sealed class EnemyBrain : MonoBehaviour
{
    public Combatant Target;
    public BossPatternSO Pattern;
    public bool Pooled;
    private Combatant self;
    private double actionAt;
    private Vector2 attackPosition;
    private bool winding;
    private int attackIndex;
    private BossAttack current;
    private void Awake() { self=GetComponent<Combatant>(); self.Died+=OnDeath; }
    private void OnDeath(Combatant actor) { GetComponent<Collider2D>().enabled=false; if(!Pooled) Destroy(gameObject,0.15f); }
    public void ResetBrain() { StopAllCoroutines(); winding=false; actionAt=Time.timeAsDouble+0.75; attackIndex=0; }
    private void Update()
    {
        if(self.SimulationEnabled&&(Target==null||!Target.Alive))
            foreach(var actor in FindObjectsByType<Combatant>(FindObjectsSortMode.None))if(actor.IsPlayer&&actor.Alive){Target=actor;break;}
        if(!self.SimulationEnabled||!self.Alive||Target==null||!Target.Alive||Time.timeScale==0) return;
        if(winding)
        {
            if(Time.timeAsDouble<actionAt) return;
            if(Vector2.Distance(Target.transform.position,attackPosition)<=current.Radius) Target.Damage(current.Damage);
            CombatVisual.Pulse(attackPosition,current.Radius,new Color32(233,127,98,255),0.2f);
            winding=false; actionAt=Time.timeAsDouble+current.RecoverySeconds; return;
        }
        if(Time.timeAsDouble<actionAt) return;
        float distance=Vector2.Distance(transform.position,Target.transform.position);
        if(distance>1.3f && Pattern==null) { transform.position=Vector2.MoveTowards(transform.position,Target.transform.position,Time.deltaTime*1.5f); return; }
        if(Pattern!=null&&distance>3) { transform.position=Vector2.MoveTowards(transform.position,Target.transform.position,Time.deltaTime); return; }
        current=Pattern!=null&&Pattern.Attacks.Length>0 ? Pattern.Attacks[attackIndex++%Pattern.Attacks.Length] : new BossAttack { TelegraphSeconds=0.7f,Damage=12,Radius=1.1f,RecoverySeconds=1 };
        attackPosition=Target.transform.position; winding=true; actionAt=Time.timeAsDouble+current.TelegraphSeconds;
        CombatVisual.Pulse(attackPosition,current.Radius,GameUI.Gold,current.TelegraphSeconds);
    }
}
