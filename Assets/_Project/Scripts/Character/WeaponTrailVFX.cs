using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public sealed class WeaponTrailVFX : MonoBehaviour
{
    public int Tier=1;
    public string Family="sword";
    public bool Demonstrate;
    private static Material glow;
    public static Material Glow=>glow!=null?glow:glow=new Material(Shader.Find("EchoesOfTheRift/PixelGlow"));
    private Vector3 previous;
    private float nextEmission,burstUntil;
    private SpriteRenderer weapon;
    private int renderedTier=-1;private string renderedFamily;
    private void Awake(){weapon=GetComponent<SpriteRenderer>();previous=transform.position;}
    public void Burst(){burstUntil=Time.unscaledTime+.35f;}
    private void LateUpdate()
    {
        if(renderedTier!=Tier||renderedFamily!=Family){weapon.sprite=IllustratedArt.Weapon(Family)??PixelArt.Icon(Family,0,Tier);if(IllustratedArt.Owns(weapon.sprite))weapon.sharedMaterial=IllustratedArt.World;renderedTier=Tier;renderedFamily=Family;}
        bool moving=(transform.position-previous).sqrMagnitude>.0001f;
        weapon.color=Color.white;
        float angle=Time.unscaledTime<burstUntil?Mathf.Sin(Time.unscaledTime*35)*45:0;
        transform.localRotation=Quaternion.Euler(0,0,angle);
        if(!Application.isBatchMode&&weapon.isVisible&&Tier>=3&&(moving||Demonstrate||Time.unscaledTime<burstUntil)&&Time.unscaledTime>=nextEmission)
        {
            nextEmission=Time.unscaledTime+(Tier>=5?.045f:Tier==4?.08f:.14f);
            CosmeticTrailPool.Emit(weapon.sprite,PixelArt.Rarity(Tier),transform.position,transform.rotation,transform.lossyScale,gameObject.layer,10,Glow);
            CosmeticTrailPool.Emit(CombatVisual.Square,PixelArt.Rarity(Tier),transform.position+new Vector3(Mathf.Sin(Time.unscaledTime*19)*.2f,.2f+Mathf.Cos(Time.unscaledTime*13)*.25f,0),Quaternion.identity,Vector3.one*.07f,gameObject.layer,14,Glow);
        }
        previous=transform.position;
    }
}