using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public sealed partial class RuntimeSmokeTest
{
    private IEnumerator TestTouchSticks(ArenaGame game)
    {
        var move=GameObject.Find("MoveStick").GetComponent<FixedTouchStick>();
        var aim=GameObject.Find("AttackControl").GetComponent<FixedTouchStick>();
        var moveRect=(RectTransform)move.transform;var aimRect=(RectTransform)aim.transform;
        Vector2 moveHome=moveRect.anchoredPosition,aimHome=aimRect.anchoredPosition;
        PointerEventData Pointer(FixedTouchStick stick,int id,Vector2 offset)=>new PointerEventData(EventSystem.current){pointerId=id,position=RectTransformUtility.WorldToScreenPoint(null,stick.transform.TransformPoint(offset)),button=PointerEventData.InputButton.Left};
        Vector2 Read(FixedTouchStick stick)=>((InputControl<Vector2>)stick.control).ReadValue();
        move.OnPointerDown(Pointer(move,10,Vector2.right*84));
        aim.OnPointerDown(Pointer(aim,20,Vector2.up*72));yield return null;
        bool independent=Read(move).x>.9f&&Read(aim).y>.9f;
        bool fixedBases=moveRect.anchoredPosition==moveHome&&aimRect.anchoredPosition==aimHome&&move.Thumb.anchoredPosition.x==move.Radius;
        move.OnPointerDown(Pointer(move,30,Vector2.left*84));move.OnDrag(Pointer(move,30,Vector2.left*84));move.OnPointerUp(Pointer(move,30,Vector2.zero));yield return null;
        bool ownership=Read(move).x>.9f;
        move.OnPointerUp(Pointer(move,10,Vector2.zero));yield return null;
        bool separateRelease=Read(move).sqrMagnitude<.001f&&Read(aim).y>.9f;
        aim.SendMessage("OnApplicationFocus",false);yield return null;
        bool focus=Read(aim).sqrMagnitude<.001f&&aim.Thumb.anchoredPosition==Vector2.zero;
        aim.OnPointerDown(Pointer(aim,20,Vector2.up*72));yield return null;
        var inventory=FindAnyObjectByType<InventoryModal>();inventory.Open();yield return null;inventory.Close();yield return null;
        bool menu=move.Value==Vector2.zero&&aim.Value==Vector2.zero&&Read(aim).sqrMagnitude<.001f;
        move.OnPointerDown(Pointer(move,10,Vector2.right*84));yield return null;
        move.OnCancel(new BaseEventData(EventSystem.current));yield return null;
        bool cancel=Read(move).sqrMagnitude<.001f;
        bool pass=independent&&fixedBases&&ownership&&separateRelease&&focus&&menu&&cancel;
        layoutChecks.Add($"Touch controls pass={pass} independent={independent} fixedBases={fixedBases} ownership={ownership} separateRelease={separateRelease} focus={focus} menu={menu} cancel={cancel}");
        if(!pass)layoutFailures.Add("Touch pointer ownership / cancellation regression");
    }
}
