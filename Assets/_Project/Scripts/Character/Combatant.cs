using System;
using UnityEngine;

public sealed class Combatant : MonoBehaviour
{
    private void Awake(){if(GetComponent<OverheadVitals>()==null)gameObject.AddComponent<OverheadVitals>();}
    public bool IsPlayer;
    public bool InSafeZone;
    public float MaximumHealth=100;
    public float Health { get; private set; }
    public bool Alive=>Health>0;
    public double InvulnerableUntil { get; private set; }
    public event Action<Combatant> Died;
    public event Action Changed;
    public bool SimulationEnabled=true;
    public void ApplySnapshot(float health,float maximum) { MaximumHealth=Mathf.Max(1,maximum); Health=Mathf.Clamp(health,0,MaximumHealth); Changed?.Invoke(); }
    public void ResetHealth(float maximum)
    {
        MaximumHealth=Mathf.Max(1,maximum); Health=MaximumHealth; InvulnerableUntil=0; Changed?.Invoke();
    }
    public void GrantInvulnerability(float seconds) => InvulnerableUntil=Math.Max(InvulnerableUntil,Time.timeAsDouble+Mathf.Max(0,seconds));
    public void Damage(float amount)
    {
        if(InSafeZone || !SimulationEnabled || !Alive || Time.timeAsDouble<InvulnerableUntil || amount<=0 || float.IsNaN(amount)||float.IsInfinity(amount)) return;
        Health=Mathf.Max(0,Health-amount); Changed?.Invoke(); if(!Alive) Died?.Invoke(this);
    }
    public void Heal(float amount)
    {
        if(!SimulationEnabled || !Alive || amount<=0 || float.IsNaN(amount)||float.IsInfinity(amount)) return;
        Health=Mathf.Min(MaximumHealth,Health+amount); Changed?.Invoke();
    }
}
