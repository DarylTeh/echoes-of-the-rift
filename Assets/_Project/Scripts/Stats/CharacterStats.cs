using UnityEngine;
using StatAttribute=EchoesOfTheRift.Stats.Attribute;

public sealed class CharacterStats : MonoBehaviour
{
    public StatAttribute Health=new StatAttribute { Base=100 };
    public StatAttribute Damage=new StatAttribute { Base=15 };
    public StatAttribute MovementSpeed=new StatAttribute { Base=4 };
    public StatAttribute DodgeChance;
    public StatAttribute CooldownSpeed=new StatAttribute { Base=1 };
    public float MaxHealth=>Mathf.Max(1,Health.Evaluate());
    public float AttackDamage=>Mathf.Max(0,Damage.Evaluate());
    public float MoveSpeed=>Mathf.Clamp(MovementSpeed.Evaluate(),0,30);
    public float SafeDodgeChance=>Mathf.Clamp01(DodgeChance.Evaluate());
    public float SafeCooldownSpeed=>Mathf.Clamp(CooldownSpeed.Evaluate(),0.01f,100);
    public float CooldownDuration(float baseSeconds)
    {
        if(float.IsNaN(baseSeconds)||float.IsInfinity(baseSeconds)) return 0;
        return Mathf.Clamp(baseSeconds,0,3600)/SafeCooldownSpeed;
    }
}
