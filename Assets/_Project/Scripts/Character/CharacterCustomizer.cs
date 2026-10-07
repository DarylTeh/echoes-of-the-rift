using UnityEngine;
using System;

public enum HeroActionPose { Idle, Attack, Skill, Dodge, Hit }

public sealed class CharacterCustomizer : MonoBehaviour
{
    public SpriteRenderer Body;
    public SpriteRenderer Hair;
    public SpriteRenderer Clothes;
    public Sprite[] HairStyles;
    public ParticleSystem TierFourAura;
    public ParticleSystem TierFiveAura;
    public Color[] SkinPalette = { new Color32(244,203,163,255), new Color32(202,147,103,255), new Color32(132,83,62,255), new Color32(83,53,46,255) };
    public Color[] HairPalette = { new Color32(64,42,44,255), new Color32(194,125,54,255), new Color32(218,203,159,255), new Color32(130,149,143,255) };
    public Color ClothColor = new Color32(80,135,130,255);
    public CharacterAppearanceData Appearance { get; private set; }
    public int EquipmentTier { get; private set; } = 1;
    public float PresentationScale=0.375f;
    public bool IsWalking { get; private set; }
    public int WalkFrame { get; private set; }
    public HeroActionPose CurrentAction { get; private set; }
    public bool IsActionAnimating=>CurrentAction!=HeroActionPose.Idle;
    private SpriteRenderer heroSprite;
    private Transform[] pixelParts=Array.Empty<Transform>();
    private Vector2 facing=Vector2.right;
    private Vector2 actionDirection=Vector2.right;
    private float weaponAngle;
    private float actionDuration,actionElapsed;
    private Color actionColor=Color.white;

    public void Apply(CharacterAppearanceData data)
    {
        data.SkinIndex = Mathf.Clamp(data.SkinIndex, 0, Mathf.Max(0, SkinPalette.Length - 1));
        data.HairColor = Mathf.Clamp(data.HairColor, 0, Mathf.Max(0, HairPalette.Length - 1));
        data.HairStyle = Mathf.Clamp(data.HairStyle, 0, Mathf.Max(0, HairStyles.Length - 1));
        data.Race=Mathf.Clamp(data.Race,0,8);
        Appearance = data;
        Color skin=data.CustomColors?Safe(data.SkinRGB):SkinPalette[data.SkinIndex];
        Color hair=data.CustomColors?Safe(data.HairRGB):HairPalette[data.HairColor];
        Color eyes=data.CustomColors?Safe(data.EyeRGB):new Color32(117,221,199,255);
        string[] parts={"tail","body","armour","head","ears","hair","horns"};
        pixelParts=new Transform[parts.Length];
        foreach(var old in new[]{Body,Hair,Clothes})if(old!=null)old.enabled=false;
        for(int partIndex=0;partIndex<parts.Length;partIndex++)
        {
            string part=parts[partIndex];
            var child=transform.Find("Pixel-"+part);
            var renderer=child!=null?child.GetComponent<SpriteRenderer>():new GameObject("Pixel-"+part).AddComponent<SpriteRenderer>();
            renderer.transform.SetParent(transform,false);renderer.transform.localPosition=new Vector3(0,PresentationScale,0);renderer.transform.localScale=Vector3.one*PresentationScale;renderer.gameObject.layer=gameObject.layer;
            pixelParts[partIndex]=renderer.transform;
            var illustrated=IllustratedArt.Hero(data.Race);
            renderer.enabled=illustrated==null||part=="body";
            renderer.sprite=illustrated!=null?illustrated:PixelArt.Character(part,data.Race,data.HairStyle,skin,hair,eyes);renderer.sortingOrder=Array.IndexOf(parts,part)+2;
            if(part=="body"){heroSprite=renderer;heroSprite.flipX=facing.x<0;}
            if(illustrated!=null&&part=="body")
            {
                renderer.sharedMaterial=IllustratedArt.HeroMaterial;
                if(palette==null)palette=new MaterialPropertyBlock();
                // Keep the creator's skin/hair controls working while preserving race details
                // that the shader does not classify as skin or hair.
                palette.SetFloat("_Customize",1);palette.SetFloat("_Race",data.Race);
                palette.SetColor("_Skin",skin);palette.SetColor("_Hair",hair);palette.SetColor("_Eyes",eyes);renderer.SetPropertyBlock(palette);
            }
        }
        if(weapon==null)
        {
            weapon=new GameObject("EquippedWeapon").AddComponent<SpriteRenderer>();weapon.transform.SetParent(transform,false);weapon.sortingOrder=12;
            trail=weapon.gameObject.AddComponent<WeaponTrailVFX>();
        }
        // Place the weapon at the hero's hand, relative to the body sprite pivot.
        // This keeps it visibly held in both the world view and paperdoll preview.
        weapon.gameObject.layer=gameObject.layer;weapon.transform.localScale=Vector3.one*(PresentationScale*.78f);
        SetWeapon("sword",EquipmentTier);
        previousPosition=transform.position;ApplyFacing();
    }
    private Vector3 previousPosition;
    private SpriteRenderer weapon;
    private MaterialPropertyBlock palette;
    private WeaponTrailVFX trail;
    public static readonly string[] Races={"Human","Elf","Dwarf","Orc","Goblin","Undead","Beastkin / Lizardfolk","Demon / Tiefling","Angel / Celestial"};
    private static Color Safe(Color c)=>new Color(float.IsFinite(c.r)?Mathf.Clamp01(c.r):.5f,float.IsFinite(c.g)?Mathf.Clamp01(c.g):.5f,float.IsFinite(c.b)?Mathf.Clamp01(c.b):.5f,1);
    public void SetFacing(Vector2 direction)
    {
        if(direction.sqrMagnitude<.01f)return;
        direction.Normalize();
        if(Vector2.Dot(direction,facing)>.995f)return;
        facing=direction;ApplyFacing();
    }
    public void PlayAttackPose(Vector2 direction)=>PlayAction(HeroActionPose.Attack,direction,Color.white,.24f);
    public void PlaySkillPose(Vector2 direction,Color color)=>PlayAction(HeroActionPose.Skill,direction,color,.38f);
    public void PlayDodgePose(Vector2 direction)=>PlayAction(HeroActionPose.Dodge,direction,new Color32(87,227,255,255),.2f);
    public void PlayHitPose(Vector2 direction)=>PlayAction(HeroActionPose.Hit,direction,new Color32(255,93,132,255),.2f);
    private void PlayAction(HeroActionPose pose,Vector2 direction,Color color,float duration)
    {
        if(direction.sqrMagnitude>.01f)actionDirection=direction.normalized;
        CurrentAction=pose;actionColor=color;actionDuration=duration;actionElapsed=0;
    }
    private void ApplyFacing()
    {
        if(heroSprite!=null)
        {
            int direction=Mathf.Abs(facing.x)>Mathf.Abs(facing.y)?(facing.x<0?3:1):(facing.y>0?2:0);
            heroSprite.sprite=IllustratedArt.Hero(Appearance.Race,direction,WalkFrame);
            heroSprite.flipX=direction==3;
        }
        if(weapon==null)return;
        weapon.transform.localPosition=new Vector3(facing.x*.34f,.84f+facing.y*.14f,0)*PresentationScale;
        weaponAngle=Mathf.Atan2(facing.y,facing.x)*Mathf.Rad2Deg-45;weapon.transform.localRotation=Quaternion.Euler(0,0,weaponAngle);
        weapon.sortingOrder=facing.y>.35f?2:12;
    }
    public void SetWeapon(string family,int tier){if(weapon==null)return;weapon.sprite=IllustratedArt.Weapon(family)??PixelArt.Icon(string.IsNullOrEmpty(family)?"sword":family,0,tier);
        if(IllustratedArt.Owns(weapon.sprite))weapon.sharedMaterial=IllustratedArt.World;trail.Tier=tier;trail.Family=family;}
    private void LateUpdate()
    {
        Vector3 delta=transform.position-previousPosition;bool walking=delta.sqrMagnitude>.00001f;previousPosition=transform.position;IsWalking=walking;if(walking)SetFacing(delta);
        int frame=walking?(Mathf.FloorToInt(Time.unscaledTime*10f)&1):0;
        if(WalkFrame!=frame){WalkFrame=frame;ApplyFacing();}
        if(Time.timeScale>0&&CurrentAction!=HeroActionPose.Idle)
        {
            actionElapsed+=Time.deltaTime;
            if(actionElapsed>=actionDuration)CurrentAction=HeroActionPose.Idle;
        }
        float actionProgress=CurrentAction==HeroActionPose.Idle?0:Mathf.Clamp01(actionElapsed/actionDuration);
        float action=CurrentAction==HeroActionPose.Idle?0:Mathf.Sin(actionProgress*Mathf.PI);
        float bob=walking?Mathf.Sin(Time.unscaledTime*14)*.045f:Mathf.Sin(Time.unscaledTime*3)*.025f;
        for(int i=0;i<pixelParts.Length;i++)if(pixelParts[i]!=null)pixelParts[i].localPosition=new Vector3(0,1+bob,0)*PresentationScale;
        float lean=walking?Mathf.Sin(Time.unscaledTime*14)*4:0;
        float poseLean=CurrentAction==HeroActionPose.Dodge?-actionDirection.x*18:CurrentAction==HeroActionPose.Hit?actionDirection.x*24:CurrentAction==HeroActionPose.Attack?-actionDirection.x*8:CurrentAction==HeroActionPose.Skill?actionDirection.y*5:0;
        if(heroSprite!=null)
        {
            heroSprite.transform.localRotation=Quaternion.Euler(0,0,lean+poseLean*action);
            float stretch=CurrentAction==HeroActionPose.Dodge?0.13f:CurrentAction==HeroActionPose.Skill?0.08f:CurrentAction==HeroActionPose.Hit?-.1f:0;
            heroSprite.transform.localScale=Vector3.Scale(Vector3.one*PresentationScale,new Vector3(1+stretch*action,1-stretch*.45f*action,1));
            heroSprite.color=CurrentAction==HeroActionPose.Skill?Color.Lerp(Color.white,actionColor,action*.28f):CurrentAction==HeroActionPose.Dodge?Color.Lerp(Color.white,actionColor,action*.22f):CurrentAction==HeroActionPose.Hit?Color.Lerp(Color.white,actionColor,action*.32f):Color.white;
        }
        if(weapon!=null)
        {
            float sweep=0,reach=0;
            if(CurrentAction==HeroActionPose.Attack)
            {
                float t=actionProgress;
                sweep=t<.18f?Mathf.Lerp(0,-30,Ease(t/.18f)):t<.58f?Mathf.Lerp(-30,142,Ease((t-.18f)/.4f)):Mathf.Lerp(142,0,Ease((t-.58f)/.42f));
                reach=t<.18f?-.12f*Ease(t/.18f):t<.58f?Mathf.Lerp(-.12f,.28f,Ease((t-.18f)/.4f)):Mathf.Lerp(.28f,0,Ease((t-.58f)/.42f));
            }
            else if(CurrentAction==HeroActionPose.Skill)
            {
                float t=actionProgress;sweep=t<.25f?Mathf.Lerp(0,82,Ease(t/.25f)):t<.48f?Mathf.Lerp(82,48,Ease((t-.25f)/.23f)):Mathf.Lerp(48,0,Ease((t-.48f)/.52f));
                reach=.18f*action;
            }
            else if(CurrentAction==HeroActionPose.Dodge){sweep=-actionDirection.x*28;reach=.12f*action;}
            else if(CurrentAction==HeroActionPose.Hit){sweep=actionDirection.x*18;reach=-.1f*action;}
            weapon.transform.localPosition=new Vector3(facing.x*.34f,.84f+facing.y*.14f,0)*PresentationScale+(Vector3)(actionDirection*(PresentationScale*reach));
            weapon.transform.localRotation=Quaternion.Euler(0,0,weaponAngle+sweep+(walking?Mathf.Sin(Time.unscaledTime*14)*7:0));
        }
    }
    private static float Ease(float value){value=Mathf.Clamp01(value);return value*value*(3-2*value);}

    public void SetEquipmentTier(int tier)
    {
        EquipmentTier = Mathf.Clamp(tier, 1, 5);
        if(trail!=null)trail.Tier=EquipmentTier;
        SetAura(TierFourAura, EquipmentTier == 4,28,16);
        SetAura(TierFiveAura, EquipmentTier == 5,48,28);
    }

    private static void SetAura(ParticleSystem aura, bool active,int particleBudget,float emissionRate)
    {
        if (aura == null) return;
        aura.GetComponent<ParticleSystemRenderer>().sharedMaterial=WeaponTrailVFX.Glow;
        if (active)
        {
            var main=aura.main;main.maxParticles=particleBudget;
            var emission=aura.emission;emission.rateOverTime=emissionRate;
            aura.gameObject.SetActive(true);if (!aura.isPlaying) aura.Play();
        }
        else { aura.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); aura.gameObject.SetActive(false); }
    }
}
