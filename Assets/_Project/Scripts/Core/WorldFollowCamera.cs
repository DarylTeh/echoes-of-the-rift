using UnityEngine;

[DefaultExecutionOrder(150)]
public sealed class WorldFollowCamera:MonoBehaviour
{
    private ArenaGame game;
    private Camera view;
    private Vector3 tracking;
    private bool initialized,lastTown;
    private int lastStage=-1;
    private void LateUpdate()
    {
        if(game==null)game=FindFirstObjectByType<ArenaGame>();
        if(game==null||game.Player==null||game.Hub==null||game.Session.DedicatedServer)return;
        if(!game.Hub.IsOpen&&game.Dungeon.StageCount==0)return;
        Vector3 target=game.Player.transform.position;
        float halfWidth=game.Hub.IsOpen?16:game.Dungeon.CurrentStage.width*.5f;
        float halfHeight=game.Hub.IsOpen?9:game.Dungeon.CurrentStage.height*.5f;
        if(view==null)view=GetComponent<Camera>();float y=view.orthographicSize,x=y*view.aspect;
        target.x=Mathf.Clamp(target.x,-Mathf.Max(0,halfWidth-x),Mathf.Max(0,halfWidth-x));
        target.y=Mathf.Clamp(target.y,-Mathf.Max(0,halfHeight-y),Mathf.Max(0,halfHeight-y));
        bool town=game.Hub.IsOpen;int stage=game.Dungeon.StageIndex;
        target.z=-10;
        if(!initialized||town!=lastTown||stage!=lastStage||(target-tracking).sqrMagnitude>36)tracking=target;
        else tracking=Vector3.Lerp(tracking,target,1-Mathf.Exp(-24*Time.deltaTime));
        initialized=true;lastTown=town;lastStage=stage;
        // Keep subpixel tracking state so rounding never stalls a small camera movement.
        transform.position=new Vector3(Mathf.Round(tracking.x*64)/64,Mathf.Round(tracking.y*64)/64,-10);
    }
}
