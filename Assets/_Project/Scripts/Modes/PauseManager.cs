using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class PauseManager : MonoBehaviour
{
    public bool NetworkSession;
    public bool IsPaused { get; private set; }
    public event Action<int> CountdownChanged;
    public event Action<bool> PauseChanged;
    private Coroutine resume;
    private float previousScale=1;
    private RectTransform root;
    private TMPro.TMP_Text label;
    private void Update() { if(Keyboard.current?.escapeKey.wasPressedThisFrame==true) { if(IsPaused) Resume(); else Pause(); } }
    public void Pause()
    {
        var town=GetComponent<TownHubManager>();if(town!=null&&town.HasDialog){town.CloseDialog();return;}
        var inventory=GetComponent<InventoryModal>();
        if(inventory!=null&&inventory.IsOpen){inventory.Back();return;}
        if(IsPaused)return;
        IsPaused=true;
        if(!NetworkSession) { previousScale=Time.timeScale; Time.timeScale=0; }
        root=GameUI.Canvas("PauseMenu");
        label=GameUI.Label(root,NetworkSession?"Menu open · raid continues":"PAUSED",new Vector2(0,100),new Vector2(650,60),28);
        GameUI.Button(root,EnglishScreens.Resume,Vector2.zero,new Vector2(250,55),Resume);
        PauseChanged?.Invoke(true);
    }
    public void Resume() { if(!IsPaused||resume!=null)return; if(NetworkSession) FinishResume(); else resume=StartCoroutine(Countdown()); }
    private IEnumerator Countdown()
    {
        for(int i=3;i>0;i--) { if(label!=null)label.text=EnglishScreens.ResumingIn+i; CountdownChanged?.Invoke(i); yield return new WaitForSecondsRealtime(1); }
        FinishResume();
    }
    private void FinishResume()
    {
        if(!NetworkSession) Time.timeScale=previousScale;
        IsPaused=false; resume=null; if(root!=null)Destroy(root.gameObject); PauseChanged?.Invoke(false);
    }
    private void OnDisable() { if(resume!=null)StopCoroutine(resume); if(IsPaused)FinishResume(); }
}
