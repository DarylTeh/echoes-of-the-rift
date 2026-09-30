using System;
using UnityEngine;

public sealed class SkillStanceSwapper : MonoBehaviour
{
    public SpellData[] Primary=new SpellData[3];
    public SpellData[] Secondary=new SpellData[3];
    public float Mana { get; private set; }=100;
    private Combatant actor;
    private void Awake(){actor=GetComponent<Combatant>();}
    private void Update(){if(actor==null||actor.SimulationEnabled)Mana=Mathf.Min(100,Mana+Time.deltaTime*8);}
    public bool SecondaryActive { get; private set; }
    public event Action Changed;
    public event Action<SpellData> Cast;
    private readonly double[] readyAt=new double[6];
    public void Swap() { SecondaryActive=!SecondaryActive; Changed?.Invoke(); }
    public SpellData GetSpell(int index) => index<0||index>=6 ? null : index<3 ? Primary[index] : Secondary[index-3];
    public float Remaining(int index) => index<0||index>=6 ? 0 : Mathf.Max(0,(float)(readyAt[index]-Time.timeAsDouble));
    public bool TryCastActive(int index,CharacterStats stats) => TryCast(index+(SecondaryActive?3:0),stats);
    public bool TryCast(int index,CharacterStats stats)
    {
        var spell=GetSpell(index);
        var health=GetComponent<Combatant>();
        var controller=GetComponent<PlayerController>(); if(controller!=null&&!controller.ControlsEnabled)return false;
        if(Mana<10||spell==null||stats==null||Time.timeScale==0||Remaining(index)>0||(health!=null&&!health.Alive)) return false;
        Mana-=10;
        readyAt[index]=Time.timeAsDouble+stats.CooldownDuration(spell.Cooldown);
        Cast?.Invoke(spell); Changed?.Invoke(); return true;
    }
    public void ResetCooldowns() { Mana=100; Array.Clear(readyAt,0,readyAt.Length); Changed?.Invoke(); }
    public void ApplySnapshot(bool secondary,float[] cooldowns,float mana=100)
    {
        SecondaryActive=secondary; Mana=mana;
        for(int i=0;i<6;i++)readyAt[i]=Time.timeAsDouble+Mathf.Max(0,cooldowns[i]);
        Changed?.Invoke();
    }
}
