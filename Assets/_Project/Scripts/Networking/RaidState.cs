using System.Collections.Generic;

/// <summary>Server-owned raid rules, independent of transport and presentation.</summary>
public sealed class RaidState
{
    private readonly Dictionary<int,bool> members=new Dictionary<int,bool>();
    private readonly Dictionary<int,int> revives=new Dictionary<int,int>();
    public bool Defeated { get; private set; }
    public int Epoch { get; private set; }
    public int ActiveRevives=>revives.Count;
    public int AlivePlayerCount { get { int count=0; foreach(bool alive in members.Values) if(alive) count++; return count; } }
    public void Reset() { members.Clear(); revives.Clear(); Defeated=false; Epoch++; }
    public bool Register(int id) { if(Defeated||members.ContainsKey(id))return false; members.Add(id,true); return true; }
    public bool SetAlive(int id,bool alive)
    {
        if(Defeated||!members.ContainsKey(id)) return false;
        members[id]=alive; CheckWipe(); return true;
    }
    public void Remove(int id) { members.Remove(id); CancelAllRevives(); if(members.Count>0) CheckWipe(); }
    public bool StartRevive(int rescuer,int target)
    {
        if(Defeated||rescuer==target||!members.TryGetValue(rescuer,out bool alive)||!alive||!members.TryGetValue(target,out bool targetAlive)||targetAlive||revives.ContainsKey(target)) return false;
        revives.Add(target,rescuer); return true;
    }
    public bool CompleteRevive(int target,int epoch)
    {
        if(Defeated||epoch!=Epoch||!revives.TryGetValue(target,out int rescuer)||!members.TryGetValue(rescuer,out bool alive)||!alive) { revives.Remove(target); return false; }
        revives.Remove(target); members[target]=true; return true;
    }
    public void CancelRevive(int target)=>revives.Remove(target);
    public void CancelAllRevives() { revives.Clear(); Epoch++; }
    private void CheckWipe()
    {
        if(members.Count==0||AlivePlayerCount>0) return;
        Defeated=true; CancelAllRevives();
    }
}
