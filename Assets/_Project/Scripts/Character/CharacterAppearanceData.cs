using System;

[Serializable]
public struct CharacterAppearanceData
{
    public int Race;
    public bool CustomColors;
    public UnityEngine.Color SkinRGB,HairRGB,EyeRGB;
    public int SkinIndex;
    public int HairStyle;
    public int HairColor;
    public string ClassId;
    public string PassiveSkillId;
    public bool Equals(CharacterAppearanceData other)=>Race==other.Race&&CustomColors==other.CustomColors&&SkinRGB==other.SkinRGB&&HairRGB==other.HairRGB&&EyeRGB==other.EyeRGB&&SkinIndex==other.SkinIndex&&HairStyle==other.HairStyle&&HairColor==other.HairColor&&ClassId==other.ClassId&&PassiveSkillId==other.PassiveSkillId;
}
