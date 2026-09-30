using UnityEngine;

// Client replicas only: gameplay and damage still run at the authoritative server position.
[DefaultExecutionOrder(100)]
public sealed class NetworkPresentation : MonoBehaviour
{
    private Vector3 from,target;
    private float receivedAt;
    private bool initialized;
    private Rigidbody2D body;
    private void Awake(){body=GetComponent<Rigidbody2D>();}
    public void Receive(Vector3 position,bool teleport=false)
    {
        if(!initialized||teleport||(position-transform.position).sqrMagnitude>9)
        {
            transform.position=position;
            if(body!=null)body.interpolation=RigidbodyInterpolation2D.None;
            initialized=true;
        }
        from=transform.position;target=position;receivedAt=Time.unscaledTime;
    }
    public static void Apply(GameObject actor,Vector3 position,bool teleport=false)
    {
        var view=actor.GetComponent<NetworkPresentation>();
        if(view==null)view=actor.AddComponent<NetworkPresentation>();
        view.Receive(position,teleport);
    }
    private void LateUpdate()
    {
        if(initialized)transform.position=Vector3.Lerp(from,target,Mathf.Clamp01((Time.unscaledTime-receivedAt)/.05f));
    }
}
