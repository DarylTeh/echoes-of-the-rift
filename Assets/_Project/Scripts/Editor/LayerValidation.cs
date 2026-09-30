using System;
using UnityEngine;

public static class LayerValidation
{
    public static void StatsCLI()
    {
        var value=new EchoesOfTheRift.Stats.Attribute { Base=10,Flat=5,Percent=0.2f };
        if(Mathf.Abs(value.Evaluate()-18)>0.001f) throw new Exception("Stat formula failed.");
        var go=new GameObject("StatsTest"); var stats=go.AddComponent<CharacterStats>();
        try
        {
            stats.CooldownSpeed=new EchoesOfTheRift.Stats.Attribute { Base=0 };
            if(float.IsInfinity(stats.CooldownDuration(5))) throw new Exception("Zero cooldown divisor.");
            stats.DodgeChance=new EchoesOfTheRift.Stats.Attribute { Base=2 };
            if(stats.SafeDodgeChance!=1) throw new Exception("Dodge upper clamp.");
            stats.DodgeChance=new EchoesOfTheRift.Stats.Attribute { Base=-1 };
            if(stats.SafeDodgeChance!=0) throw new Exception("Dodge lower clamp.");
            var invalid=new EchoesOfTheRift.Stats.Attribute { Base=float.NaN,Flat=float.PositiveInfinity };
            if(invalid.Evaluate()!=0) throw new Exception("Non-finite stat guard.");
        }
        finally { UnityEngine.Object.DestroyImmediate(go); }
        Debug.Log("STEP_04_OK: modifier formula, dodge boundaries and finite cooldown division verified.");
    }
}
