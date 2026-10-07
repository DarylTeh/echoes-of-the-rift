using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public sealed partial class RuntimeSmokeTest
{
    private IEnumerator TestFramePacing(ArenaGame game)
    {
        const float sampleSeconds=10f;
        Application.targetFrameRate=60;
        QualitySettings.vSyncCount=0;
        game.StartExpedition();
        var controller=game.Player.GetComponent<PlayerController>();
        controller.SetUIAttack(true);
        yield return new WaitForSecondsRealtime(1f); // let shaders, sprites and the first effects warm up

        var frameTimes=new List<float>(720);
        int missedBudget=0;
        double start=Time.realtimeSinceStartupAsDouble;
        double end=start+sampleSeconds;
        double nextSkill=start+.25;
        int skill=0;
        while(Time.realtimeSinceStartupAsDouble<end)
        {
            if(Time.realtimeSinceStartupAsDouble>=nextSkill)
            {
                controller.RequestSkill(skill++%3);
                nextSkill=Time.realtimeSinceStartupAsDouble+1.2;
            }
            if(game.Dungeon.IsCleared)
            {
                if(game.Dungeon.StageIndex+1<game.Dungeon.StageCount)game.Dungeon.NextRoom();
                else game.StartExpedition();
            }
            float frame=Time.unscaledDeltaTime;
            if(frame>0)
            {
                frameTimes.Add(frame);
                // Unity's float delta can land a few ulps above 1/60 even
                // when the frame was presented at the target cadence.
                if(frame>1f/60f+.0005f)missedBudget++;
            }
            yield return null;
        }
        controller.SetUIAttack(false);
        frameTimes.Sort();
        int count=frameTimes.Count;
        float p50=count==0?0:frameTimes[(count-1)/2];
        float p95=count==0?0:frameTimes[Mathf.Clamp(Mathf.CeilToInt(count*.95f)-1,0,count-1)];
        double elapsed=Time.realtimeSinceStartupAsDouble-start;
        double fps=elapsed>0?count/elapsed:0;
        double missPercent=count>0?missedBudget*100.0/count:100;
        bool passed=count>=300&&fps>=59.0&&missPercent<=5.0&&!failed;
        string result=$"{(passed?"PASS":"FAIL")} framePacing target=60 average={fps:0.0} p50={p50*1000:0.00}ms p95={p95*1000:0.00}ms overBudget={missPercent:0.0}% samples={count} runtimeErrors={failed}";
        File.WriteAllText(Path.Combine(output,"frame-pacing.txt"),result+System.Environment.NewLine);
        Finish(result);
    }
}
