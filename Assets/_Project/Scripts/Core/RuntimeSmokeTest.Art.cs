using System.Collections;
using System.IO;
using UnityEngine;

public sealed partial class RuntimeSmokeTest
{
    private IEnumerator TestBossArt()
    {
        bool atlas=true,phases=true;
        for(int boss=0;boss<4;boss++)
        {
            for(int phase=0;phase<4;phase++)atlas&=IllustratedArt.BossAttack(boss,phase)!=null;
            var visual=BossAttackVisual.Begin(Vector3.zero,boss,boss,2f,2.4f);
            atlas&=visual!=null&&visual.CurrentArt==IllustratedArt.BossAttack(boss,0)&&visual.IsLeased;
            yield return new WaitForSecondsRealtime(.7f);
            phases&=visual!=null&&visual.CurrentPhase==0;
            yield return new WaitForSecondsRealtime(.6f);
            phases&=visual!=null&&visual.CurrentPhase==1;
            yield return new WaitForEndOfFrame();Capture($"boss-{boss}-telegraph.png");
            visual?.Impact();
            phases&=visual!=null&&visual.CurrentPhase==2;
            yield return new WaitForEndOfFrame();Capture($"boss-{boss}-impact.png");
            yield return new WaitForSecondsRealtime(.15f);
            phases&=visual!=null&&visual.CurrentPhase==3;
            yield return new WaitForSecondsRealtime(.2f);
            phases&=visual!=null&&!visual.IsLeased;
        }
        Finish($"{(atlas&&phases&&!failed?"PASS":"FAIL")} bossAtlas={atlas} fourPhaseSequences={phases} runtimeErrors={failed}");
    }
}
