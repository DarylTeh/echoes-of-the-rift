using UnityEngine;
public enum SpellEffect { Bolt, Burst, Mend, DashStrike, SummonRune, StatBuff, StanceModifier, FanShot, PiercingLance, Whirlwind, OrbitHammers }
[CreateAssetMenu(menuName="EchoesOfTheRift/Spell")]
public sealed class SpellData : ScriptableObject
{
    public Sprite skillIcon;
    public string Archetype;
    [TextArea] public string mechanicalDescription;
    [TextArea] public string villageQuote;
    public string Id;
    public string DisplayName;
    public SpellEffect Effect;
    [Min(0.1f)] public float Cooldown=3;
    [Min(0)] public float Power=20;
    [Min(0.1f)] public float Range=5;
    public Color Color=Color.white;
}
