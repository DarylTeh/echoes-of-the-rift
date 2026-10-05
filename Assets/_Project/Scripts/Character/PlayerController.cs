using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D),typeof(Combatant),typeof(CharacterStats))]
public sealed class PlayerController : MonoBehaviour
{
    public SkillStanceSwapper Skills;
    public Vector2 Aim { get; private set; }=Vector2.right;
    public bool ControlsEnabled=true;
    public bool RemoteControlled;
    public bool ForwardInputs;
    public event Action<PlayerInputFrame> InputSubmitted;
    private Rigidbody2D body;
    private Combatant health;
    private CharacterCustomizer customizer;
    private WeaponTrailVFX equippedWeapon;
    private CharacterStats stats;
    private InputAction move,aimStick,pointer,attack,dodge,swap;
    private InputAction[] casts;
    private double buffUntil;
    private double dodgeReady,dodgeUntil,attackReady,lastRemote;
    private Vector2 dodgeDirection,movement;
    private PlayerInputFrame remote;
    private bool uiDodge,uiSwap,uiAttack;
    public void SetUIAttack(bool held)=>uiAttack=held;
    private int uiSkill;
    private void Awake()
    {
        body=GetComponent<Rigidbody2D>(); health=GetComponent<Combatant>(); stats=GetComponent<CharacterStats>();customizer=GetComponent<CharacterCustomizer>();
        body.gravityScale=0; body.freezeRotation=true; body.interpolation=RigidbodyInterpolation2D.Interpolate;
        move=new InputAction("Move",InputActionType.Value); move.AddCompositeBinding("2DVector").With("Up","<Keyboard>/w").With("Down","<Keyboard>/s").With("Left","<Keyboard>/a").With("Right","<Keyboard>/d"); move.AddBinding("<Gamepad>/leftStick");
        aimStick=new InputAction("AimStick",InputActionType.Value,"<Gamepad>/rightStick"); pointer=new InputAction("AimPointer",InputActionType.Value,"<Mouse>/position");
        attack=new InputAction("Attack",InputActionType.Button,"<Mouse>/leftButton"); attack.AddBinding("<Gamepad>/rightTrigger");
        dodge=new InputAction("Dodge",InputActionType.Button,"<Keyboard>/space"); dodge.AddBinding("<Gamepad>/buttonSouth");
        swap=new InputAction("Stance",InputActionType.Button,"<Keyboard>/tab"); swap.AddBinding("<Gamepad>/leftShoulder");
        casts=new[] { new InputAction("Skill1",InputActionType.Button,"<Keyboard>/q"),new InputAction("Skill2",InputActionType.Button,"<Keyboard>/e"),new InputAction("Skill3",InputActionType.Button,"<Keyboard>/r") };
        foreach(var action in new[] {move,aimStick,pointer,attack,dodge,swap})action.Enable(); foreach(var action in casts)action.Enable();
    }
    private void Start() { if(Skills!=null)Skills.Cast+=Cast; }
    public void SubmitRemote(PlayerInputFrame input)
    {
        if(!float.IsFinite(input.Move.x)||!float.IsFinite(input.Move.y)||!float.IsFinite(input.Aim.x)||!float.IsFinite(input.Aim.y))return;
        input.Move=Vector2.ClampMagnitude(input.Move,1); input.Aim=input.Aim.normalized; input.Skill=Mathf.Clamp(input.Skill,0,3);
        input.Dodge|=remote.Dodge; input.Swap|=remote.Swap; if(input.Skill==0)input.Skill=remote.Skill;
        remote=input; lastRemote=Time.realtimeSinceStartupAsDouble;
    }
    private void Update()
    {
        if(!ControlsEnabled||!health.Alive||Time.timeScale==0) { movement=Vector2.zero;if(ForwardInputs)InputSubmitted?.Invoke(default);return; }
        PlayerInputFrame frame;
        if(RemoteControlled)
        {
            frame=Time.realtimeSinceStartupAsDouble-lastRemote<0.3 ? remote : default;
            remote.Dodge=remote.Swap=false; remote.Skill=0;
        }
        else
        {
            var stick=aimStick.ReadValue<Vector2>();
            if(stick.sqrMagnitude>0.1f)Aim=stick.normalized;
            else if(Mouse.current!=null&&Camera.main!=null) { Vector2 delta=(Vector2)Camera.main.ScreenToWorldPoint(pointer.ReadValue<Vector2>())-((Vector2)transform.position+Vector2.up*0.45f); if(delta.sqrMagnitude>0.01f)Aim=delta.normalized; }
            frame=new PlayerInputFrame { Move=move.ReadValue<Vector2>(),Aim=Aim,Attack=uiAttack||(FixedTouchStick.EnabledForDevice&&stick.sqrMagnitude>.1f)||attack.IsPressed()&&!(UnityEngine.EventSystems.EventSystem.current?.IsPointerOverGameObject()??false),Dodge=dodge.WasPressedThisFrame()||uiDodge,Swap=swap.WasPressedThisFrame()||uiSwap,Skill=uiSkill };
            for(int i=0;i<casts.Length;i++)if(casts[i].WasPressedThisFrame())frame.Skill=i+1;
            uiDodge=uiSwap=false; uiSkill=0;
        }
        if(health.InSafeZone){frame.Attack=frame.Dodge=frame.Swap=false;frame.Skill=0;}
        if(ForwardInputs) { InputSubmitted?.Invoke(frame); return; }
        movement=frame.Move; if(frame.Aim.sqrMagnitude>0.01f)Aim=frame.Aim;customizer?.SetFacing(movement.sqrMagnitude>.04f?movement:Aim);
        if(frame.Dodge)Dodge(); if(frame.Swap)Skills.Swap(); if(frame.Skill>0)Skills.TryCastActive(frame.Skill-1,stats);
        if(frame.Attack&&Time.timeAsDouble>=attackReady)
        {
            if(equippedWeapon==null)equippedWeapon=GetComponentInChildren<WeaponTrailVFX>();
            string family=equippedWeapon!=null?equippedWeapon.Family:"sword";
            attackReady=Time.timeAsDouble+WeaponCombat.Interval(family);equippedWeapon?.Burst();
            customizer?.PlayAttackPose(Aim);
            WeaponCombat.Fire(family,transform.position+Vector3.up*.45f,Aim,stats.AttackDamage*(Time.timeAsDouble<buffUntil?1.25f:1),health);
        }
    }
    private void FixedUpdate()
    {
        if(ForwardInputs||!ControlsEnabled||!health.Alive) { body.linearVelocity=Vector2.zero; return; }
        if(health.InSafeZone){body.position=new Vector2(Mathf.Clamp(body.position.x,-15,15),Mathf.Clamp(body.position.y,-8,8));body.linearVelocity=Vector2.ClampMagnitude(movement,1)*stats.MoveSpeed;return;}
        body.linearVelocity=Time.timeAsDouble<dodgeUntil?dodgeDirection*11:Vector2.ClampMagnitude(movement,1)*stats.MoveSpeed;
    }
    public void RequestSkill(int slot) { if(health.InSafeZone||!ControlsEnabled)return; if(ForwardInputs)uiSkill=slot+1; else if(ControlsEnabled)Skills.TryCastActive(slot,stats); }
    public void RequestSwap() { if(health.InSafeZone||!ControlsEnabled)return; if(ForwardInputs)uiSwap=true; else Skills.Swap(); }
    public bool Dodge()
    {
        if(health.InSafeZone||!ControlsEnabled||!health.Alive||Time.timeScale==0||Time.timeAsDouble<dodgeReady)return false;
        if(ForwardInputs) { uiDodge=true; return true; }
        GetComponentInChildren<WeaponTrailVFX>()?.Burst(); dodgeReady=Time.timeAsDouble+1; dodgeUntil=Time.timeAsDouble+0.2; dodgeDirection=movement.normalized; if(dodgeDirection==Vector2.zero)dodgeDirection=Aim;customizer?.PlayDodgePose(dodgeDirection);health.GrantInvulnerability(0.2f); return true;
    }
    private void Cast(SpellData spell)
    {
        if(health.InSafeZone)return;
        customizer?.PlaySkillPose(Aim,spell.Color);
        CombatVisual.Pulse(transform.position+Vector3.up*.3f,.8f,spell.Color,.3f);
        GetComponentInChildren<WeaponTrailVFX>()?.Burst();
        if(spell.Effect==SpellEffect.FanShot){WeaponCombat.Fan(transform.position+Vector3.up*.45f,Aim,spell.Power*.35f,spell.Range,health,5,13,spell.Color);return;}
        if(spell.Effect==SpellEffect.PiercingLance){Projectile.Spawn(transform.position+Vector3.up*.45f,Aim,spell.Power,spell.Range,health,spell.Color,4);return;}
        if(spell.Effect==SpellEffect.Whirlwind||spell.Effect==SpellEffect.OrbitHammers){var effect=new GameObject("OrbitingStrike").AddComponent<OrbitingStrike>();effect.Owner=health;effect.Power=spell.Power;effect.Hammers=spell.Effect==SpellEffect.OrbitHammers;return;}
        if(spell.Effect==SpellEffect.StatBuff){buffUntil=Time.timeAsDouble+6;CombatVisual.Pulse(transform.position,1,spell.Color,.4f);return;}
        if(spell.Effect==SpellEffect.StanceModifier){Skills.Swap();CombatVisual.Pulse(transform.position,1,spell.Color,.4f);return;}
        if(spell.Effect==SpellEffect.SummonRune){var rune=new GameObject("SummonedRune").AddComponent<SummonedRune>();rune.transform.position=transform.position+(Vector3)Aim;rune.Owner=health;rune.Power=spell.Power;rune.Tint=spell.Color;return;}
        if(spell.Effect==SpellEffect.DashStrike){Dodge();foreach(var target in FindObjectsByType<Combatant>(FindObjectsSortMode.None))if(!target.IsPlayer&&Vector2.Distance(transform.position,target.transform.position)<2)target.Damage(spell.Power);return;}
        if(spell.Effect==SpellEffect.Mend)health.Heal(spell.Power);
        else if(spell.Effect==SpellEffect.Bolt)Projectile.Spawn(transform.position+Vector3.up*0.45f,Aim,spell.Power,spell.Range,health,spell.Color);
        else { foreach(var target in FindObjectsByType<Combatant>(FindObjectsSortMode.None))if(!target.IsPlayer&&Vector2.Distance(transform.position,target.transform.position)<=spell.Range)target.Damage(spell.Power); CombatVisual.Pulse(transform.position,spell.Range,spell.Color,0.25f); }
    }
    private void OnDestroy()
    {
        if(Skills!=null)Skills.Cast-=Cast;
        foreach(var action in new[] {move,aimStick,pointer,attack,dodge,swap})action?.Dispose(); if(casts!=null)foreach(var action in casts)action.Dispose();
    }
}
