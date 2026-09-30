using System.Collections;
using UnityEngine;
public sealed partial class RuntimeSmokeTest
{
    private Combatant CombatDummy(Vector2 position,bool friendly=false)
    {
        var go=new GameObject("CombatTestDummy");go.transform.position=position;go.AddComponent<CircleCollider2D>().radius=.2f;
        var actor=go.AddComponent<Combatant>();actor.IsPlayer=friendly;actor.ResetHealth(100);return actor;
    }
    private IEnumerator TestCombatExpansion(ArenaGame game)
    {
        var presentationObject=new GameObject("PresentationTest");var presentation=presentationObject.AddComponent<NetworkPresentation>();
        presentation.Receive(Vector3.zero);presentation.Receive(Vector3.right);
        bool noJump=presentationObject.transform.position==Vector3.zero;
        yield return new WaitForSecondsRealtime(.12f);yield return new WaitForEndOfFrame();
        bool settled=Vector3.Distance(presentationObject.transform.position,Vector3.right)<.001f;
        presentation.Receive(Vector3.right*20);bool teleported=presentationObject.transform.position==Vector3.right*20;
        presentation.Receive(Vector3.right*19,true);bool roomSnap=presentationObject.transform.position==Vector3.right*19;
        bool presentationPass=noJump&&settled&&teleported&&roomSnap;
        Debug.Log($"PRESENTATION_CHECK pass={presentationPass} noJump={noJump} settled={settled} teleport={teleported} roomSnap={roomSnap}");
        if(!presentationPass)failed=true;Destroy(presentationObject);
        Vector2 origin=new Vector2(200,200);
        var owner=CombatDummy(origin,true);var front=CombatDummy(origin+Vector2.right*1.2f);var rear=CombatDummy(origin-Vector2.right*1.2f);var far=CombatDummy(origin+Vector2.right*2.4f);
        Physics2D.SyncTransforms();WeaponCombat.Fire("sword",origin,Vector2.right,10,owner);
        bool directional=front.Health==90&&rear.Health==100&&far.Health==100;
        WeaponCombat.Fire("spear",origin,Vector2.right,10,owner);bool reach=far.Health==90;
        var wall=new GameObject("TestWall");wall.transform.position=origin+Vector2.right*.6f;wall.AddComponent<BoxCollider2D>().size=new Vector2(.1f,3);Physics2D.SyncTransforms();
        front.ResetHealth(100);WeaponCombat.Fire("sword",origin,Vector2.right,10,owner);bool blocked=front.Health==100;Destroy(wall);yield return null;
        Physics2D.SyncTransforms();front.ResetHealth(100);far.ResetHealth(100);
        Projectile.Spawn(origin,Vector2.right,10,5,owner,Color.cyan,4);yield return new WaitForSeconds(.6f);
        bool piercing=front.Health==90&&far.Health==90;
        front.ResetHealth(100);far.ResetHealth(100);
        // Returning weapon targets the owner's hand height; use the same height for test colliders.
        front.transform.position=origin+new Vector2(1.2f,.45f);far.transform.position=origin+new Vector2(2.4f,.45f);Physics2D.SyncTransforms();
        WeaponCombat.Fire("boomerang",origin+Vector2.up*.45f,Vector2.right,10,owner);yield return new WaitForSeconds(1.2f);
        bool returning=front.Health==84&&far.Health==84;
        front.ResetHealth(100);owner.InSafeZone=true;WeaponCombat.Fire("sword",origin,Vector2.right,10,owner);bool safe=front.Health==100;owner.InSafeZone=false;
        var controller=owner.gameObject.AddComponent<PlayerController>();controller.ControlsEnabled=false;
        var volley=System.Array.Find(game.Items,x=>x.Spell!=null&&x.Spell.Effect==SpellEffect.FanShot)?.Spell;
        var cyclone=System.Array.Find(game.Items,x=>x.Spell!=null&&x.Spell.Effect==SpellEffect.Whirlwind)?.Spell;
        var hammers=System.Array.Find(game.Items,x=>x.Spell!=null&&x.Spell.Effect==SpellEffect.OrbitHammers)?.Spell;
        bool skills=volley!=null&&cyclone!=null&&hammers!=null;
        if(skills)
        {
            int before=FindObjectsByType<Projectile>(FindObjectsSortMode.None).Length;
            controller.SendMessage("Cast",volley);skills&=FindObjectsByType<Projectile>(FindObjectsSortMode.None).Length==before+5;
            yield return new WaitForSeconds(1.1f);front.ResetHealth(100);
            controller.SendMessage("Cast",cyclone);yield return new WaitForSeconds(1);skills&=Mathf.Abs(front.Health-28)<.01f;
            front.ResetHealth(100);controller.SendMessage("Cast",hammers);yield return new WaitForSeconds(3);skills&=front.Health<100&&FindObjectsByType<OrbitingStrike>(FindObjectsSortMode.None).Length==0;
        }
        foreach(var actor in new[]{owner,front,rear,far})Destroy(actor.gameObject);
        bool passed=directional&&reach&&blocked&&piercing&&returning&&safe&&skills;
        Debug.Log($"COMBAT_EXPANSION_CHECK pass={passed} directional={directional} reach={reach} walls={blocked} piercing={piercing} returning={returning} safe={safe} skills={skills}");
        if(!passed)failed=true;
    }
}
