using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.OnScreen;

// The hit area stays fixed; only the non-raycast thumb moves. Each stick owns one pointer.
public sealed class FixedTouchStick : OnScreenControl, IPointerDownHandler, IPointerUpHandler,
    IDragHandler, IInitializePotentialDragHandler, ICancelHandler
{
    public RectTransform Thumb;
    public PlayerController Player;
    public float Radius=42;
    public Vector2 Value { get; private set; }
    private int owner=int.MinValue;
    private string path;
    protected override string controlPathInternal { get=>path; set=>path=value; }
    public static readonly bool EnabledForDevice=Application.isMobilePlatform||System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-touchControls")>=0;
    public void OnInitializePotentialDrag(PointerEventData data)=>data.useDragThreshold=false;
    public void OnPointerDown(PointerEventData data)
    {
        if(!isActiveAndEnabled||owner!=int.MinValue||data.button!=PointerEventData.InputButton.Left)return;
        owner=data.pointerId;Move(data);
    }
    public void OnDrag(PointerEventData data){if(data.pointerId==owner&&isActiveAndEnabled)Move(data);}
    public void OnPointerUp(PointerEventData data){if(data.pointerId==owner)Release();}
    public void OnCancel(BaseEventData data)=>Release();
    private void Move(PointerEventData data)
    {
        if(!RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform,data.position,data.pressEventCamera,out var offset))return;
        var value=Vector2.ClampMagnitude(offset/Mathf.Max(1,Radius),1);
        Value=value.magnitude<.12f?Vector2.zero:value;
        if(Thumb!=null)Thumb.anchoredPosition=Value*Radius;
        SendValueToControl(Value);
    }
    public void Release()
    {
        owner=int.MinValue;Value=Vector2.zero;
        if(Thumb!=null)Thumb.anchoredPosition=Vector2.zero;
        SendValueToControl(Vector2.zero);
    }
    private void Update(){if(owner!=int.MinValue&&(Time.timeScale==0||(Player!=null&&!Player.ControlsEnabled)))Release();}
    private void OnApplicationFocus(bool focused){if(!focused)Release();}
    private void OnApplicationPause(bool paused){if(paused)Release();}
    protected override void OnDisable(){Release();base.OnDisable();}
}
