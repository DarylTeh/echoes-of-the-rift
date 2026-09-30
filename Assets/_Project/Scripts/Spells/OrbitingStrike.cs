using UnityEngine;
public sealed class OrbitingStrike:MonoBehaviour
{
    public Combatant Owner;public float Power;public bool Hammers;
    private float age,next;
    private void Update()
    {
        if(Owner==null||!Owner.Alive||Owner.InSafeZone){Destroy(gameObject);return;}
        age+=Time.deltaTime;transform.position=Owner.transform.position+Vector3.up*.3f;
        if(age>=next)
        {
            next+=Hammers?.5f:.35f;
            if(Hammers){Vector2 aim=new Vector2(Mathf.Cos(age*5),Mathf.Sin(age*5));WeaponCombat.HitArc(transform.position,aim,2.4f,100,Power/6,Owner);CombatVisual.Slash(transform.position,aim,2.4f,100,GameUI.Gold);}
            else {WeaponCombat.HitArc(transform.position,Vector2.right,2,360,Power/3,Owner);CombatVisual.Slash(transform.position,Vector2.right,2,350,Color.red);}
        }
        if(age>=(Hammers?2.6f:.8f))Destroy(gameObject);
    }
}
