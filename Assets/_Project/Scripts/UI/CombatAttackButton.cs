using UnityEngine;
using UnityEngine.EventSystems;

public sealed class CombatAttackButton : MonoBehaviour,IPointerDownHandler,IPointerUpHandler,IPointerExitHandler
{
    public PlayerController Player;
    public void OnPointerDown(PointerEventData data){if(data.button==PointerEventData.InputButton.Left)Player?.SetUIAttack(true);}
    public void OnPointerUp(PointerEventData data)=>Player?.SetUIAttack(false);
    public void OnPointerExit(PointerEventData data)=>Player?.SetUIAttack(false);
    private void OnDisable()=>Player?.SetUIAttack(false);
    private void OnApplicationFocus(bool focused){if(!focused)Player?.SetUIAttack(false);}
}
