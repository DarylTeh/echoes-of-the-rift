using UnityEngine;
[CreateAssetMenu(menuName="EchoesOfTheRift/Passive")]
public sealed class PassiveSkillSO : ScriptableObject
{
    public string Id;
    public string DisplayName;
    [TextArea] public string Description;
    public float FlatHealth;
    public float DamagePercent;
}
