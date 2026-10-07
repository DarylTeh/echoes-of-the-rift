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
    public void Burst()
    {
        burstUntil=Time.unscaledTime+.35f;
        if(Application.isBatchMode||Tier<3||weapon==null)return;
        int count=Tier>=5?14:Tier==4?8:4;
        Color rarity=PixelArt.Rarity(Tier);
        float baseAngle=weapon.transform.eulerAngles.z;
        for(int i=0;i<count;i++)
        {
            float angle=baseAngle-58+i*(116f/Mathf.Max(1,count-1));
            float radians=angle*Mathf.Deg2Rad;
            Vector3 offset=new Vector3(Mathf.Cos(radians),Mathf.Sin(radians),0)*(.14f+(i%3)*.045f);
            float size=Tier>=5?(i%3==0?.12f:.075f):Tier==4?.08f:.055f;
            Color spark=i%3==0?Color.white:rarity;
            CosmeticTrailPool.Emit(CombatVisual.Square,spark,weapon.transform.position+offset,Quaternion.Euler(0,0,angle),Vector3.one*size,gameObject.layer,15,Glow);
        }
    }
    private void LateUpdate()
    {
        if(renderedTier!=Tier||renderedFamily!=Family){weapon.sprite=IllustratedArt.Weapon(Family)??PixelArt.Icon(Family,0,Tier);if(IllustratedArt.Owns(weapon.sprite))weapon.sharedMaterial=IllustratedArt.World;renderedTier=Tier;renderedFamily=Family;}
        bool moving=(transform.position-previous).sqrMagnitude>.0001f;
        weapon.color=Color.white;
        float angle=Time.unscaledTime<burstUntil?Mathf.Sin(Time.unscaledTime*35)*45:0;
        transform.localRotation=Quaternion.Euler(0,0,angle);
        if(!Application.isBatchMode&&weapon.isVisible&&Tier>=3&&(moving||Demonstrate||Time.unscaledTime<burstUntil)&&Time.unscaledTime>=nextEmission)
        {
            nextEmission=Time.unscaledTime+(Tier>=5?.09f:Tier==4?.11f:.14f);
            CosmeticTrailPool.Emit(weapon.sprite,PixelArt.Rarity(Tier),transform.position,transform.rotation,transform.lossyScale,gameObject.layer,10,Glow);
            CosmeticTrailPool.Emit(CombatVisual.Square,Color.Lerp(PixelArt.Rarity(Tier),Color.white,Tier>=5?.85f:.55f),transform.position+new Vector3(Mathf.Sin(Time.unscaledTime*19)*.2f,.2f+Mathf.Cos(Time.unscaledTime*13)*.25f,0),Quaternion.identity,Vector3.one*(Tier>=5?.105f:Tier==4?.08f:.07f),gameObject.layer,14,Glow);
            if(Tier>=4)
            {
                float orbit=Time.unscaledTime*(Tier>=5?13:9);
                for(int i=0;i<(Tier>=5?2:1);i++)
                {
                    float orbitAngle=orbit+i*Mathf.PI;
                    Vector3 offset=new Vector3(Mathf.Cos(orbitAngle),Mathf.Sin(orbitAngle),0)*(Tier>=5?.24f:.18f);
                    CosmeticTrailPool.Emit(CombatVisual.Square,i==0?Color.white:PixelArt.Rarity(Tier),transform.position+offset,Quaternion.Euler(0,0,orbitAngle*Mathf.Rad2Deg),Vector3.one*(Tier>=5?.065f:.05f),gameObject.layer,16,Glow);
                }
            }
        }
        previous=transform.position;
    }
}
