using UnityEngine;
public sealed class ArcaneBoltVFX:MonoBehaviour
{
    private static Sprite sprite;
    public static Sprite Sprite {get {if(sprite!=null)return sprite;var a=new PixelArt.Raster();a.L(1,16,23,16,Color.white,3);a.Diamond(23,16,8,Color.white);return sprite=a.Finish();}}
    private Vector3 previous;private float next;private SpriteRenderer rendererCache;
    private void Start(){previous=transform.position;rendererCache=GetComponent<SpriteRenderer>();}
    private void LateUpdate()
    {
        Vector3 delta=transform.position-previous;if(delta.sqrMagnitude>.0001f)transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(delta.y,delta.x)*Mathf.Rad2Deg);
        if(!Application.isBatchMode&&rendererCache.isVisible&&Time.time>=next&&delta.sqrMagnitude>.0001f){next=Time.time+.08f;CosmeticTrailPool.Emit(Sprite,rendererCache.color,transform.position,transform.rotation,transform.lossyScale*.7f,gameObject.layer,6);}

        previous=transform.position;
    }
}
