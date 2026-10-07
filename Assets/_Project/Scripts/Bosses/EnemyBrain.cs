using UnityEngine;

[RequireComponent(typeof(Combatant))]
public sealed class EnemyBrain : MonoBehaviour
{
    public Combatant Target;
    public BossPatternSO Pattern;
    public bool Pooled;
    private static Combatant cachedPlayerTarget;
    private static double nextPlayerSearchAt;
    private Combatant self;
    private Collider2D selfCollider;
    private double actionAt;
    private Vector2 attackPosition;
    private bool winding;
    private int attackIndex;
    private BossAttack current;
    private BossAttackVisual bossVisual;
    private void Awake() { self=GetComponent<Combatant>(); selfCollider=GetComponent<Collider2D>(); self.Died+=OnDeath; }
    private void OnDeath(Combatant actor) { if(selfCollider!=null)selfCollider.enabled=false; bossVisual?.Cancel();bossVisual=null;if(!Pooled) Destroy(gameObject,0.15f); }
    public void ResetBrain() { StopAllCoroutines();bossVisual?.Cancel();bossVisual=null;winding=false; actionAt=Time.timeAsDouble+0.75; attackIndex=0; }
    private void Update()
    {
        if(self.SimulationEnabled&&(Target==null||!Target.Alive))Target=FindPlayerTarget();
        if(!self.SimulationEnabled||!self.Alive||Target==null||!Target.Alive||Time.timeScale==0) return;
        if(winding)
        {
            if(Time.timeAsDouble<actionAt) return;
            if(Vector2.Distance(Target.transform.position,attackPosition)<=current.Radius) Target.Damage(current.Damage);
            Color impactColor=Pattern!=null?BossAttackVisual.Tint(Pattern.VisualIndex):new Color32(233,127,98,255);
            CombatVisual.Pulse(attackPosition,current.Radius,impactColor,0.2f);bossVisual?.Impact();bossVisual=null;
            winding=false; actionAt=Time.timeAsDouble+current.RecoverySeconds; return;
        }
        if(Time.timeAsDouble<actionAt) return;
        float sqrDistance=((Vector2)transform.position-(Vector2)Target.transform.position).sqrMagnitude;
        if(sqrDistance>1.69f && Pattern==null) { transform.position=Vector2.MoveTowards(transform.position,Target.transform.position,Time.deltaTime*1.5f); return; }
        if(Pattern!=null&&sqrDistance>9f) { transform.position=Vector2.MoveTowards(transform.position,Target.transform.position,Time.deltaTime); return; }
        int visualAttack=0;
        if(Pattern!=null&&Pattern.Attacks.Length>0){visualAttack=attackIndex++%Pattern.Attacks.Length;current=Pattern.Attacks[visualAttack];}
        else current=new BossAttack { TelegraphSeconds=0.7f,Damage=12,Radius=1.1f,RecoverySeconds=1 };
        attackPosition=Target.transform.position; winding=true; actionAt=Time.timeAsDouble+current.TelegraphSeconds;
        Color telegraphColor=Pattern!=null?BossAttackVisual.Tint(Pattern.VisualIndex):GameUI.Gold;
        if(Pattern!=null&&Pattern.VisualIndex>=0)bossVisual=BossAttackVisual.Begin(attackPosition,Pattern.VisualIndex,visualAttack,current.TelegraphSeconds,current.Radius);
        CombatVisual.Pulse(attackPosition,current.Radius,telegraphColor,current.TelegraphSeconds);
    }
    private static Combatant FindPlayerTarget()
    {
        if(cachedPlayerTarget!=null&&cachedPlayerTarget.Alive)return cachedPlayerTarget;
        double now=Time.timeAsDouble;
        if(now<nextPlayerSearchAt)return null;
        nextPlayerSearchAt=now+.25;
        cachedPlayerTarget=null;
        foreach(var actor in FindObjectsByType<Combatant>(FindObjectsSortMode.None))
        {
            if(!actor.IsPlayer||!actor.Alive)continue;
            cachedPlayerTarget=actor;
            break;
        }
        return cachedPlayerTarget;
    }
    private void OnDestroy(){bossVisual?.Cancel();if(self!=null)self.Died-=OnDeath;}
}
