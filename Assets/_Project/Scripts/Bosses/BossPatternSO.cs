using System;
using UnityEngine;
[Serializable]
public struct BossAttack
{
    public string Name;
    [Min(0.2f)] public float TelegraphSeconds;
    [Min(0)] public float Damage;
    [Min(0.1f)] public float Radius;
    [Min(0.1f)] public float RecoverySeconds;
}
[CreateAssetMenu(menuName="EchoesOfTheRift/Boss Pattern")]
public sealed class BossPatternSO : ScriptableObject
{
    public string Id;
    [HideInInspector] public int VisualIndex=-1;
    public BossAttack[] Attacks;
}
