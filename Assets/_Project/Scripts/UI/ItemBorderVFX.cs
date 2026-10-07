using UnityEngine;
using UnityEngine.UI;

// Fixed-cost ornament: only the perimeter moves, never the item or its text.
public sealed class ItemBorderVFX : MonoBehaviour
{
    // Tier is the rarity band (1-5); EnhancementLevel only adds a small number
    // of extra perimeter motes so rarity stays the main color signal.
    public int Tier=1;
    public int EnhancementLevel=1;
    private RectTransform rect;
    private Image edge;
    private float nextUpdate;
    private readonly Image[] halos=new Image[24];
    private readonly Image[] sparks=new Image[24];
    private int allocatedSparks;
    private void Start()
    {
        rect=(RectTransform)transform;
        edge=GameUI.Icon(transform,PixelArt.Border(),Vector2.zero,rect.sizeDelta);
        edge.name="RarityEdge";edge.type=Image.Type.Sliced;edge.preserveAspect=false;edge.pixelsPerUnitMultiplier=3.125f;
    }
    private void Update()
    {
        if(edge==null||Time.unscaledTime<nextUpdate)return;
        nextUpdate=Time.unscaledTime+1f/30;
        edge.rectTransform.sizeDelta=rect.sizeDelta;
        Color tint=PixelArt.Rarity(Tier);
        float pulse=.5f+.5f*Mathf.Sin(Time.unscaledTime*(Tier>=5?7.5f:Tier>=4?5f:3f));
        edge.color=Color.Lerp(tint,Color.white,pulse*(Tier>=5?.64f:Tier>=4?.42f:.18f));
        int count=Tier>=5?16:Tier==4?10:Tier==3?4:0;
        count=Mathf.Min(sparks.Length,count+(EnhancementLevel>=20?4:EnhancementLevel>=10?3:EnhancementLevel>=5?1:0));
        EnsureSparks(count);
        float perimeter=PerimeterLength();
        for(int i=0;i<sparks.Length;i++)
        {
            if(sparks[i]==null)continue;
            sparks[i].enabled=i<count;halos[i].enabled=sparks[i].enabled;
            if(!sparks[i].enabled)continue;
            float speed=Tier>=5?110:Tier==4?76:42;
            sparks[i].rectTransform.anchoredPosition=Perimeter(Time.unscaledTime*speed+perimeter*i/count);
            halos[i].rectTransform.anchoredPosition=sparks[i].rectTransform.anchoredPosition;
            float sparkle=.5f+.5f*Mathf.Sin(Time.unscaledTime*(Tier>=5?11:7)+i*2.1f);
            halos[i].color=new Color(tint.r,tint.g,tint.b,(Tier>=5?.62f:Tier==4?.44f:.24f)*sparkle);
            sparks[i].color=Color.Lerp(tint,Color.white,Tier>=5?.82f+sparkle*.18f:.52f+sparkle*.38f);
            float size=Tier>=5?Mathf.Lerp(8,16,sparkle):Tier==4?Mathf.Lerp(6,11,sparkle):Mathf.Lerp(4,7,sparkle);
            sparks[i].rectTransform.sizeDelta=Vector2.one*size;
            sparks[i].rectTransform.localRotation=Quaternion.Euler(0,0,Time.unscaledTime*(Tier>=5?180:110)+i*45);
        }
    }
    private void EnsureSparks(int count)
    {
        while(allocatedSparks<count)
        {
            int i=allocatedSparks++;
            halos[i]=GameUI.Icon(transform,CombatVisual.Square,Vector2.zero,Vector2.one*24);
            halos[i].name="BorderHalo";
            sparks[i]=GameUI.Icon(transform,CombatVisual.Square,Vector2.zero,Vector2.one*8);
            sparks[i].name="BorderSpark";
        }
    }
    private float PerimeterLength()
    {
        float w=Mathf.Max(1,rect.rect.width-5),h=Mathf.Max(1,rect.rect.height-5),r=Mathf.Min(6,Mathf.Min(w,h)/2);
        return 2*(w+h-4*r)+2*Mathf.PI*r;
    }
    private Vector2 Perimeter(float travel)
    {
        float w=Mathf.Max(1,rect.rect.width-5),h=Mathf.Max(1,rect.rect.height-5),r=Mathf.Min(6,Mathf.Min(w,h)/2);
        float horizontal=w-2*r,vertical=h-2*r,arc=Mathf.PI*r/2;
        float t=Mathf.Repeat(travel,2*horizontal+2*vertical+4*arc);
        for(int side=0;side<4;side++)
        {
            float line=side%2==0?horizontal:vertical;
            Vector2 start=side==0?new Vector2(-w/2+r,h/2):side==1?new Vector2(w/2,h/2-r):side==2?new Vector2(w/2-r,-h/2):new Vector2(-w/2,-h/2+r);
            Vector2 direction=side==0?Vector2.right:side==1?Vector2.down:side==2?Vector2.left:Vector2.up;
            if(t<line)return start+direction*t;t-=line;
            Vector2 center=side==0?new Vector2(w/2-r,h/2-r):side==1?new Vector2(w/2-r,-h/2+r):side==2?new Vector2(-w/2+r,-h/2+r):new Vector2(-w/2+r,h/2-r);
            if(t<arc){float angle=(90-side*90)*Mathf.Deg2Rad-t/r;return center+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*r;}t-=arc;
        }
        return Vector2.zero;
    }
}
