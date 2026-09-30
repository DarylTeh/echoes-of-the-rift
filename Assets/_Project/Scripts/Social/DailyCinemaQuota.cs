using System;

[Serializable]
public sealed class DailyCinemaQuota
{
    public string Day;
    public int Completed;
    private bool pending;
    public bool TryBegin(bool optedIn,DateTime utc)
    {
        if(pending)return false;
        string day=utc.ToUniversalTime().ToString("yyyy-MM-dd"); if(day!=Day) { Day=day; Completed=0; }
        if(!optedIn||Completed>=5)return false; pending=true; return true;
    }
    public bool Finish(bool completed)
    {
        if(!pending)return false; pending=false; if(!completed)return false; Completed=Math.Min(5,Completed+1); return true;
    }
}
