using System.Collections.Generic;
using UnityEngine;

// Keep each screen's authored design resolution inside the device's usable rectangle.
// Scaling is fitted, not advertised as integer-perfect at every resolution.
[DefaultExecutionOrder(200)]
public sealed class UISafeArea : MonoBehaviour
{
    private readonly Dictionary<RectTransform,Vector2> applied=new Dictionary<RectTransform,Vector2>();
    private UnityEngine.UI.CanvasScaler scaler;
    private Canvas canvas;
    private Vector2 designResolution=new Vector2(1280,720);
    private Rect lastArea;
    private int lastWidth=-1,lastHeight=-1;
    public static Rect? TestArea;
    private void Awake(){scaler=GetComponent<UnityEngine.UI.CanvasScaler>();canvas=GetComponent<Canvas>();if(scaler!=null)designResolution=scaler.referenceResolution;}
    public void SetDesignResolution(Vector2 resolution)
    {
        designResolution=resolution;
        if(scaler!=null)scaler.referenceResolution=resolution;
        lastWidth=-1;lastHeight=-1;
    }
    private void LateUpdate()
    {
        if(Screen.width<=0||Screen.height<=0)return;
        bool layoutHarness=System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-uiLayoutTest")>=0;
        Rect area=layoutHarness&&TestArea.HasValue?TestArea.Value:Screen.safeArea;
        if(area.width<=0||area.height<=0)return;
        if(lastWidth==Screen.width&&lastHeight==Screen.height&&lastArea==area)return;
        lastWidth=Screen.width;lastHeight=Screen.height;lastArea=area;
        scaler.referenceResolution=new Vector2(designResolution.x*Screen.width/area.width,designResolution.y*Screen.height/area.height);
        Vector2 offset=(area.center-new Vector2(Screen.width,Screen.height)*.5f)/Mathf.Max(.001f,canvas.scaleFactor);
        foreach(Transform child in transform)
        {
            var rect=child as RectTransform;if(rect==null||rect.name=="RiftDefenseBackdrop")continue;
            applied.TryGetValue(rect,out var previous);
            rect.anchoredPosition+=offset-previous;applied[rect]=offset;
        }
    }
}
