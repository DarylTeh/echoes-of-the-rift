using System.Collections.Generic;
using UnityEngine;

/// <summary>Short pooled four-stage boss telegraph and impact presentation.</summary>
public sealed class BossAttackVisual : MonoBehaviour
{
    private static readonly Queue<BossAttackVisual> idle=new Queue<BossAttackVisual>();
    private SpriteRenderer art;
    private int boss,attack,phase=-1;
    private float remaining,total,radius;
    private bool impact,leased;
    public int CurrentPhase=>phase;
    public bool IsLeased=>leased;
    public Sprite CurrentArt=>art!=null?art.sprite:null;

    public static Color Tint(int bossIndex)
    {
        switch(Mathf.Clamp(bossIndex,0,3))
        {
            case 0:return new Color32(107,246,126,255);
            case 1:return new Color32(255,166,57,255);
            case 2:return new Color32(63,220,255,255);
            default:return new Color32(192,96,255,255);
        }
    }

    public static BossAttackVisual Begin(Vector3 position,int bossIndex,int attackIndex,float telegraph,float radius)
    {
        if(Application.isBatchMode)return null;
        BossAttackVisual visual=null;
        while(idle.Count>0&&visual==null)visual=idle.Dequeue();
        if(visual==null)
        {
            var go=new GameObject("BossAttackSequence");visual=go.AddComponent<BossAttackVisual>();
            visual.art=go.AddComponent<SpriteRenderer>();visual.art.sortingOrder=4;
        }
        visual.leased=true;visual.gameObject.SetActive(true);visual.transform.position=position;visual.transform.localScale=Vector3.one;
        visual.boss=Mathf.Clamp(bossIndex,0,3);visual.attack=attackIndex;visual.radius=Mathf.Max(.5f,radius);
        visual.impact=false;visual.total=visual.remaining=Mathf.Max(.2f,telegraph);visual.SetPhase(0);return visual;
    }

    private void Update()
    {
        if(!leased)return;
        remaining-=Time.deltaTime;
        float progress=1-Mathf.Clamp01(remaining/Mathf.Max(.01f,total));
        if(impact)
        {
            SetPhase(progress<.4f?2:3);
            var color=Color.white;color.a=1-progress;art.color=color;
            float scale=radius*(.55f+progress*.32f);transform.localScale=Vector3.one*scale;
        }
        else
        {
            SetPhase(progress<.58f?0:1);
            var color=Color.white;color.a=.38f+progress*.5f;art.color=color;
            float pulse=1+Mathf.Sin(progress*18)*.045f;transform.localScale=Vector3.one*(radius*(.36f+progress*.12f)*pulse);
        }
        if(remaining<=0)Recycle();
    }

    public void Impact()
    {
        if(!leased)return;
        impact=true;total=remaining=.28f;SetPhase(2);transform.localScale=Vector3.one*(radius*.55f);
    }

    public void Cancel(){if(leased)Recycle();}

    private void SetPhase(int value)
    {
        if(phase==value)return;phase=value;art.sprite=IllustratedArt.BossAttack(boss,phase);
        art.sharedMaterial=IllustratedArt.Owns(art.sprite)?IllustratedArt.World:null;art.flipX=phase==2&&(attack%2==1);
        art.color=Color.white;
    }

    private void Recycle()
    {
        if(!leased)return;leased=false;gameObject.SetActive(false);idle.Enqueue(this);
    }
}
