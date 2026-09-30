using System.Collections.Generic;
using UnityEngine;

// Keep the 1280x720 design inside the device's usable rectangle.
// Scaling is fitted, not advertised as integer-perfect at every resolution.
[DefaultExecutionOrder(200)]
public sealed class UISafeArea : MonoBehaviour
{
    private readonly Dictionary<RectTransform,Vector2> applied=new Dictionary<RectTransform,Vector2>();
    private UnityEngine.UI.CanvasScaler scaler;
    private Canvas canvas;
    private Rect lastArea;
    private int lastWidth=-1,lastHeight=-1;
    public static Rect? TestArea;
    private void Awake(){scaler=GetComponent<UnityEngine.UI.CanvasScaler>();canvas=GetComponent<Canvas>();}
    private void LateUpdate()
    {
        if(Screen.width<=0||Screen.height<=0)return;
        Rect area=Debug.isDebugBuild&&TestArea.HasValue?TestArea.Value:Screen.safeArea;
        if(area.width<=0||area.height<=0)return;
        if(lastWidth==Screen.width&&lastHeight==Screen.height&&lastArea==area)return;
        lastWidth=Screen.width;lastHeight=Screen.height;lastArea=area;
        scaler.referenceResolution=new Vector2(1280*Screen.width/area.width,720*Screen.height/area.height);
        Vector2 offset=(area.center-new Vector2(Screen.width,Screen.height)*.5f)/Mathf.Max(.001f,canvas.scaleFactor);
        foreach(Transform child in transform)
        {
            var rect=child as RectTransform;if(rect==null)continue;
            applied.TryGetValue(rect,out var previous);
            rect.anchoredPosition+=offset-previous;applied[rect]=offset;
        }
    }
}
