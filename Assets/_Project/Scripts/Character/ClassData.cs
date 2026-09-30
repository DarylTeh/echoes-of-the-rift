using UnityEngine;
[CreateAssetMenu(menuName="EchoesOfTheRift/Class")]
public sealed class ClassData : ScriptableObject
{
    public string Id;
    public string DisplayName;
    public float BaseHealth=100;
    public float BaseDamage=15;
    public PassiveSkillSO Passive;
    public SpellData[] StartingSpells=new SpellData[6];
}
