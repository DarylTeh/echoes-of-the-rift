using UnityEngine;
using UnityEngine.UI;

// Fixed-cost ornament: only the perimeter moves, never the item or its text.
public sealed class ItemBorderVFX : MonoBehaviour
{
    public int Tier=1;
    private RectTransform rect;
    private Image edge;
    private float nextUpdate;
    private readonly Image[] halos=new Image[8];
    private readonly Image[] sparks=new Image[8];
    private void Start()
    {
        rect=(RectTransform)transform;
        edge=GameUI.Icon(transform,PixelArt.Border(),Vector2.zero,rect.sizeDelta);
        edge.name="RarityEdge";edge.type=Image.Type.Sliced;edge.preserveAspect=false;edge.pixelsPerUnitMultiplier=3.125f;
        for(int i=0;i<sparks.Length;i++)
        {
            halos[i]=GameUI.Icon(transform,CombatVisual.Square,Vector2.zero,Vector2.one*7);
            halos[i].name="BorderHalo";
            sparks[i]=GameUI.Icon(transform,CombatVisual.Square,Vector2.zero,Vector2.one*(i%3==0?3:2));
            sparks[i].name="BorderSpark";
        }
    }
    private void Update()
    {
        if(edge==null||Time.unscaledTime<nextUpdate)return;
        nextUpdate=Time.unscaledTime+1f/30;
        edge.rectTransform.sizeDelta=rect.sizeDelta;
        Color tint=PixelArt.Rarity(Tier);edge.color=tint;
        int count=Tier>=5?8:Tier==4?5:Tier==3?2:0;
        for(int i=0;i<sparks.Length;i++)
        {
            sparks[i].enabled=i<count;halos[i].enabled=sparks[i].enabled;
            if(!sparks[i].enabled)continue;
            sparks[i].rectTransform.anchoredPosition=Perimeter(Time.unscaledTime*(Tier>=5?42:28)+i*37);
            halos[i].rectTransform.anchoredPosition=sparks[i].rectTransform.anchoredPosition;
            halos[i].color=new Color(tint.r,tint.g,tint.b,.18f);
            sparks[i].color=Color.Lerp(tint,Color.white,i%3==0?.8f:.25f);
        }
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
