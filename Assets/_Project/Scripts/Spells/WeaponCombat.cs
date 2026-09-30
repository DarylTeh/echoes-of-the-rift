using UnityEngine;

public static class WeaponCombat
{
    private static readonly System.Collections.Generic.List<Collider2D> targets=new System.Collections.Generic.List<Collider2D>(128);
    private static readonly System.Collections.Generic.HashSet<Combatant> struck=new System.Collections.Generic.HashSet<Combatant>();
    public static float Interval(string family)=>family=="dagger"?.22f:family=="pistol"?.3f:family=="hammer"?.85f:family=="greatsword"?.7f:family=="boomerang"?.9f:.45f;
    public static string Description(string family)
    {
        switch(family){case "spear":return "Long thrust / narrow reach";case "boomerang":return "Returning throw / hits each way";case "pistol":return "Rapid aimed shots";case "bow":return "Long-range arrows";case "crossbow":return "Three-bolt spread";case "staff":case "wand":return "Piercing magic bolt";case "hammer":return "Heavy forward shockwave";case "greatsword":return "Wide heavy cleave";case "scythe":return "Sweeping crescent";case "dagger":return "Fast short thrust";case "shield":return "Close shield bash";default:return "Directional sword slash";}
    }
    public static void Fire(string family,Vector2 origin,Vector2 aim,float damage,Combatant owner)
    {
        if(owner==null||!owner.Alive||owner.InSafeZone||!owner.SimulationEnabled)return;
        aim=aim.sqrMagnitude>.01f?aim.normalized:Vector2.right;
        if(family=="bow"||family=="pistol") {Projectile.Spawn(origin,aim,damage,family=="bow"?10:8,owner,new Color32(255,194,83,255));return;}
        if(family=="crossbow"){Fan(origin,aim,damage*.55f,9,owner,3,12,Color.cyan);return;}
        if(family=="staff"||family=="wand"){Projectile.Spawn(origin,aim,damage*.9f,8,owner,Color.magenta,3);return;}
        if(family=="boomerang"){Projectile.Spawn(origin,aim,damage*.8f,4.5f,owner,Color.green,8,true,"boomerang");return;}
        float range=family=="spear"?2.8f:family=="greatsword"||family=="scythe"?2.1f:family=="dagger"?1.15f:1.65f;
        float arc=family=="spear"||family=="dagger"?35:family=="scythe"?190:family=="greatsword"?140:110;
        float power=family=="hammer"?1.8f:family=="greatsword"?1.5f:family=="dagger"?.65f:1;
        HitArc(origin,aim,range,arc,damage*power,owner);
        CombatVisual.Slash(origin,aim,range,arc,family=="spear"?Color.cyan:GameUI.Gold);
    }
    public static void Fan(Vector2 origin,Vector2 aim,float damage,float range,Combatant owner,int count,float spacing,Color tint)
    {
        for(int i=0;i<count;i++){Vector2 direction=Quaternion.Euler(0,0,(i-(count-1)*.5f)*spacing)*aim;Projectile.Spawn(origin,direction,damage,range,owner,tint);}
    }
    public static int HitArc(Vector2 origin,Vector2 aim,float radius,float degrees,float damage,Combatant owner)
    {
        if(owner==null||!owner.Alive||owner.InSafeZone||!owner.SimulationEnabled)return 0;
        int count=Physics2D.OverlapCircle(origin,radius,new ContactFilter2D{useTriggers=false},targets),hits=0;struck.Clear();
        float threshold=Mathf.Cos(degrees*.5f*Mathf.Deg2Rad);
        for(int i=0;i<count;i++)
        {
            var target=targets[i].GetComponent<Combatant>();if(target==null||target==owner||target.IsPlayer==owner.IsPlayer||!target.Alive||target.InSafeZone||!struck.Add(target))continue;
            Vector2 direction=(Vector2)target.transform.position-origin;
            if(degrees<359&&direction.sqrMagnitude>.04f&&Vector2.Dot(direction.normalized,aim.normalized)<threshold)continue;
            // Walls also block melee/area hits. Ignore actor colliders along the ray.
            bool blocked=false;int obstacles=Physics2D.Raycast(origin,direction.normalized,new ContactFilter2D{useTriggers=false},wallHits,direction.magnitude);
            for(int n=0;n<obstacles;n++)if(wallHits[n].collider.GetComponent<Combatant>()==null){blocked=true;break;}
            if(blocked)continue;
            target.Damage(damage);hits++;
        }
        return hits;
    }
    private static readonly RaycastHit2D[] wallHits=new RaycastHit2D[32];
}
