using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

// Restore a visible selectable when keyboard/gamepad navigation enters a menu.
public sealed class UIMenuFocus : MonoBehaviour
{
    private void Update()
    {
        var events=EventSystem.current;if(events==null)return;
        bool navigation=Keyboard.current?.tabKey.wasPressedThisFrame==true || Keyboard.current?.downArrowKey.wasPressedThisFrame==true || Gamepad.current?.dpad.down.wasPressedThisFrame==true;
        if(!navigation)return;
        var canvas=GetComponent<Canvas>();
        foreach(var other in FindObjectsByType<UIMenuFocus>(FindObjectsSortMode.None))
            if(other.isActiveAndEnabled&&other.GetComponent<Canvas>().sortingOrder>canvas.sortingOrder)return;
        if(events.currentSelectedGameObject!=null&&events.currentSelectedGameObject.activeInHierarchy&&events.currentSelectedGameObject.transform.IsChildOf(transform))return;
        foreach(var button in GetComponentsInChildren<UnityEngine.UI.Selectable>())
            if(button.IsActive()&&button.IsInteractable()){events.SetSelectedGameObject(button.gameObject);break;}
    }
}
