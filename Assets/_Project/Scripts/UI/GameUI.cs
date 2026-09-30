using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

public static class GameUI
{
    public static readonly Color Ink = new Color32(29,27,39,255);
    public static readonly Color Cream = new Color32(232,231,244,255);
    public static readonly Color Gold = new Color32(202,161,87,255);
    public static RectTransform Canvas(string name)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster));
        go.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        go.GetComponent<Canvas>().sortingOrder=100;
        go.GetComponent<Canvas>().pixelPerfect=true;
        var scaler = go.GetComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280,720);
        scaler.screenMatchMode=UnityEngine.UI.CanvasScaler.ScreenMatchMode.Expand;
        go.AddComponent<UISafeArea>();
        if(name=="ServerUnavailable"||name=="SplashScreen"||name=="InventoryModal"||name=="CharacterCreator"||name=="Leaderboard"||name=="TownHub")go.AddComponent<UIMenuFocus>();
        if (UnityEngine.Object.FindFirstObjectByType<EventSystem>() == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        return (RectTransform)go.transform;
    }
    public static RectTransform Rect(string name, Transform parent, Vector2 anchor, Vector2 position, Vector2 size)
    {
        var rect = (RectTransform)new GameObject(name, typeof(RectTransform)).transform;
        rect.SetParent(parent, false); rect.anchorMin = rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f,0.5f); rect.anchoredPosition=position; rect.sizeDelta=size;
        return rect;
    }
    public static void Pin(RectTransform rect,Vector2 anchor,Vector2 offset)
    {
        rect.anchorMin=rect.anchorMax=anchor;rect.pivot=Vector2.one*.5f;rect.anchoredPosition=offset;
    }
    public static TMP_Text Label(Transform parent, string text, Vector2 position, Vector2 size, float fontSize=24)
    {
        var label=Rect("Label",parent,new Vector2(0.5f,0.5f),position,size).gameObject.AddComponent<TextMeshProUGUI>();
        label.font=Resources.Load<TMP_FontAsset>("Pixel/PixelFont");
        label.richText=false; label.enableAutoSizing=false;
        label.text=text; label.fontSize=fontSize; label.color=Cream; label.raycastTarget=false;
        label.alignment=TextAlignmentOptions.MidlineLeft;
        return label;
    }
    public static RectTransform Panel(Transform parent,string name,Vector2 position,Vector2 size)
    {
        var rect=Rect(name,parent,Vector2.one*.5f,position,size); var image=rect.gameObject.AddComponent<UnityEngine.UI.Image>();
        image.sprite=PixelArt.Frame();image.type=UnityEngine.UI.Image.Type.Sliced;image.pixelsPerUnitMultiplier=3.125f;return rect;
    }
    public static UnityEngine.UI.Image Icon(Transform parent,Sprite sprite,Vector2 position,Vector2 size)
    {
        var image=Rect("Icon",parent,Vector2.one*.5f,position,size).gameObject.AddComponent<UnityEngine.UI.Image>();image.sprite=sprite;if(IllustratedArt.Owns(sprite))image.material=IllustratedArt.UI;image.preserveAspect=true;image.raycastTarget=false;return image;
    }
    public static UnityEngine.UI.Button Button(Transform parent,string text,Vector2 position,Vector2 size,Action action)
    {
        var rect=Rect(text,parent,new Vector2(0.5f,0.5f),position,size);
        var image=rect.gameObject.AddComponent<UnityEngine.UI.Image>(); image.sprite=PixelArt.Frame(); image.type=UnityEngine.UI.Image.Type.Sliced;image.pixelsPerUnitMultiplier=3.125f;
        var button=rect.gameObject.AddComponent<UnityEngine.UI.Button>();
        var colors=button.colors;colors.selectedColor=new Color32(178,177,229,255);colors.highlightedColor=new Color32(208,208,244,255);colors.fadeDuration=0;button.colors=colors;
        var label=Label(rect,text,Vector2.zero,size-Vector2.one*12,20); label.alignment=TextAlignmentOptions.Center;
        string key=text.ToLowerInvariant();string family=(key.Contains("fuse")||key.Contains("upgrade"))?"hammer":key.Contains("equip")?"sword":key.Contains("inventory")||key.Contains("bag")?"bag":key.Contains("library")||key.Contains("shop")?"book":key.Contains("quest")||key.Contains("objective")?"quest":key.Contains("reconnect")?"swap":null;
        if(family!=null&&size.x>=144){Icon(rect,PixelArt.Icon(family),new Vector2(-size.x/2+26,0),new Vector2(32,32));label.rectTransform.anchoredPosition=new Vector2(18,0);label.rectTransform.sizeDelta=size-new Vector2(52,12);}
        button.onClick.AddListener(()=>action()); return button;
    }
}
