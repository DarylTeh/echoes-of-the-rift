using System;
using TMPro;
using UnityEngine;

public sealed class CharacterCreatorUI : MonoBehaviour
{
    public CharacterCustomizer Preview;
    public CharacterAppearanceData InitialAppearance;
    public event Action<CharacterAppearanceData> Confirmed;
    public RectTransform Root { get; private set; }
    private CharacterAppearanceData selection;
    private TMP_Text summary,targetLabel;
    private readonly UnityEngine.UI.Slider[] sliders=new UnityEngine.UI.Slider[3];
    private int colorTarget;
    private bool loading;
    private void Start()
    {
        selection=InitialAppearance;selection.ClassId="wayfarer";selection.PassiveSkillId="steadfast";
        if(!selection.CustomColors){selection.SkinRGB=Preview.SkinPalette[Mathf.Clamp(selection.SkinIndex,0,Preview.SkinPalette.Length-1)];selection.HairRGB=Preview.HairPalette[Mathf.Clamp(selection.HairColor,0,Preview.HairPalette.Length-1)];selection.EyeRGB=new Color32(117,221,199,255);selection.CustomColors=true;}
        Root=GameUI.Canvas("CharacterCreator");
        GameUI.Label(Root,EnglishScreens.EchoesOfTheRift,new Vector2(-360,280),new Vector2(500,64),32).color=GameUI.Gold;
        GameUI.Label(Root,EnglishScreens.ChooseYourOrigin,new Vector2(-360,225),new Vector2(500,40),24);
        var panel=GameUI.Panel(Root,"OriginPanel",new Vector2(290,0),new Vector2(600,580));
        GameUI.Label(panel,EnglishScreens.TheWayfarer,new Vector2(0,235),new Vector2(470,48),32).color=GameUI.Gold;
        GameUI.Button(panel,EnglishScreens.ChangeRace,new Vector2(-125,165),new Vector2(230,48),()=>{selection.Race=(selection.Race+1)%9;selection.SkinRGB=new[]{new Color32(244,203,163,255),new Color32(234,204,180,255),new Color32(202,147,103,255),new Color32(125,170,101,255),new Color32(109,186,143,255),new Color32(190,195,199,255),new Color32(105,173,161,255),new Color32(182,112,156,255),new Color32(237,204,123,255)}[selection.Race];SelectColor(0);Refresh();});
        GameUI.Button(panel,"Hair palette",new Vector2(125,165),new Vector2(230,48),()=>{selection.HairColor=(selection.HairColor+1)%Preview.HairPalette.Length;selection.HairRGB=Preview.HairPalette[selection.HairColor];SelectColor(1);Refresh();});
        summary=GameUI.Label(panel,"",new Vector2(0,108),new Vector2(470,48),24);
        GameUI.Button(panel,EnglishScreens.Skin,new Vector2(-165,45),new Vector2(145,44),()=>SelectColor(0));
        GameUI.Button(panel,EnglishScreens.Hair,new Vector2(0,45),new Vector2(145,44),()=>SelectColor(1));
        GameUI.Button(panel,EnglishScreens.Eyes,new Vector2(165,45),new Vector2(145,44),()=>SelectColor(2));
        targetLabel=GameUI.Label(panel,"",new Vector2(0,-5),new Vector2(470,32),20);
        for(int i=0;i<3;i++)
        {
            int channel=i;float y=-53-i*43;
            GameUI.Label(panel,new[]{"R","G","B"}[i],new Vector2(-220,y),new Vector2(36,32),24);
            var track=GameUI.Panel(panel,"RGB"+i,new Vector2(5,y),new Vector2(405,24));
            var slider=track.gameObject.AddComponent<UnityEngine.UI.Slider>();slider.minValue=0;slider.maxValue=255;slider.wholeNumbers=true;
            var handle=GameUI.Icon(track,PixelArt.Frame(true),Vector2.zero,new Vector2(24,32));handle.raycastTarget=true;
            slider.handleRect=handle.rectTransform;slider.targetGraphic=handle;sliders[i]=slider;
            slider.onValueChanged.AddListener(value=>{if(loading)return;Color c=GetColor();c[channel]=value/255;c.a=1;SetColor(c);Refresh();});
        }
        GameUI.Button(panel,EnglishScreens.ConfirmCharacter,new Vector2(0,-225),new Vector2(475,50),()=>Confirmed?.Invoke(selection));
        GameUI.Label(Root,EnglishScreens.RaceAndColoursAreCosmetic,new Vector2(-345,-290),new Vector2(560,40),20);
        SelectColor(0);Refresh();
    }
    private Color GetColor()=>colorTarget==0?selection.SkinRGB:colorTarget==1?selection.HairRGB:selection.EyeRGB;
    private void SetColor(Color color){if(colorTarget==0)selection.SkinRGB=color;else if(colorTarget==1)selection.HairRGB=color;else selection.EyeRGB=color;}
    private void SelectColor(int target){colorTarget=target;loading=true;Color color=GetColor();for(int i=0;i<3;i++)sliders[i].value=color[i]*255;loading=false;Refresh();}
    private void Refresh(){Preview.Apply(selection);summary.text=CharacterCustomizer.Races[selection.Race]+" / Wayfarer";targetLabel.text=new[]{"SKIN","HAIR","EYES"}[colorTarget]+" #"+ColorUtility.ToHtmlStringRGB(GetColor());}
    private void OnDestroy(){if(Root!=null)Destroy(Root.gameObject);}
}
