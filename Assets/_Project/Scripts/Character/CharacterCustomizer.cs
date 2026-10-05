using UnityEngine;
using System;

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
    private SpriteRenderer heroSprite;
    private Vector2 facing=Vector2.right;
    private float weaponAngle;

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
        foreach(var old in new[]{Body,Hair,Clothes})if(old!=null)old.enabled=false;
        foreach(string part in parts)
        {
            var child=transform.Find("Pixel-"+part);
            var renderer=child!=null?child.GetComponent<SpriteRenderer>():new GameObject("Pixel-"+part).AddComponent<SpriteRenderer>();
            renderer.transform.SetParent(transform,false);renderer.transform.localPosition=new Vector3(0,PresentationScale,0);renderer.transform.localScale=Vector3.one*PresentationScale;renderer.gameObject.layer=gameObject.layer;
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
        // Keep the visible weapon beside the tiny hero sprite; centring the full icon
        // on the face made the new compact race sprites unreadable.
        weapon.gameObject.layer=gameObject.layer;weapon.transform.localScale=Vector3.one*(PresentationScale*.86f);
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
    private void ApplyFacing()
    {
        if(heroSprite!=null)heroSprite.flipX=facing.x<-.1f;
        if(weapon==null)return;
        weapon.transform.localPosition=new Vector3(facing.x*.92f,.25f+facing.y*.72f,0)*PresentationScale;
        weaponAngle=Mathf.Atan2(facing.y,facing.x)*Mathf.Rad2Deg-45;weapon.transform.localRotation=Quaternion.Euler(0,0,weaponAngle);
        weapon.sortingOrder=facing.y>.35f?2:12;
    }
    public void SetWeapon(string family,int tier){if(weapon==null)return;weapon.sprite=IllustratedArt.Weapon(family)??PixelArt.Icon(string.IsNullOrEmpty(family)?"sword":family,0,tier);
        if(IllustratedArt.Owns(weapon.sprite))weapon.sharedMaterial=IllustratedArt.World;trail.Tier=tier;trail.Family=family;}
    private void LateUpdate()
    {
        Vector3 delta=transform.position-previousPosition;bool walking=delta.sqrMagnitude>.00001f;previousPosition=transform.position;if(walking)SetFacing(delta);
        float bob=Mathf.Floor(Mathf.Sin(Time.unscaledTime*(walking?14:3))*(walking?2:1))/16;
        foreach(Transform child in transform)if(child.name.StartsWith("Pixel-"))child.localPosition=new Vector3(0,1+bob,0)*PresentationScale;
        if(heroSprite!=null)heroSprite.transform.localRotation=Quaternion.Euler(0,0,walking?Mathf.Sin(Time.unscaledTime*14)*3:0);
        if(weapon!=null)weapon.transform.localRotation=Quaternion.Euler(0,0,weaponAngle+(walking?Mathf.Sin(Time.unscaledTime*14)*6:0));
    }

    public void SetEquipmentTier(int tier)
    {
        EquipmentTier = Mathf.Clamp(tier, 1, 5);
        if(trail!=null)trail.Tier=EquipmentTier;
        SetAura(TierFourAura, EquipmentTier == 4);
        SetAura(TierFiveAura, EquipmentTier == 5);
    }

    private static void SetAura(ParticleSystem aura, bool active)
    {
        if (aura == null) return;
        aura.GetComponent<ParticleSystemRenderer>().sharedMaterial=WeaponTrailVFX.Glow;
        if (active) { aura.gameObject.SetActive(true); if (!aura.isPlaying) aura.Play(); }
        else { aura.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); aura.gameObject.SetActive(false); }
    }
}
