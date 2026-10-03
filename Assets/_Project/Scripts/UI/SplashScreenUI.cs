using System;
using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;

public sealed class SplashScreenUI : MonoBehaviour
{
    public ArenaGame Game;
    public RectTransform Root { get; private set; }
    public TMP_InputField Username,Password,Recovery;
    public bool Ready=>account!=null;
    public bool CanEnter=>enter!=null&&enter.interactable;
    public string LastError { get; private set; }
    private RectTransform form;
    private TMP_Text message,prompt;
    private UnityEngine.UI.Button enter,submit;
    private UnityEngine.UI.Button createMode,signInMode,recoverMode,saveCode;
    private AccountResponse account;
    private bool busy,register=true,recover;
    private bool recoveryCopied,recoveryAcknowledged;
    private string recoveryCode;
    public bool RecoveryCopied=>recoveryCopied;
    public bool RecoveryAcknowledged=>recoveryAcknowledged;
    private IEnumerator Start()
    {
        while(FindFirstObjectByType<CharacterCreatorUI>()?.Root==null)yield return null;
        FindFirstObjectByType<CharacterCreatorUI>().Root.gameObject.SetActive(false);
        Root=GameUI.Canvas("SplashScreen");Root.GetComponent<Canvas>().sortingOrder=600;Root.gameObject.AddComponent<CanvasGroup>();
        var shade=GameUI.Rect("SplashBackground",Root,Vector2.one*.5f,Vector2.zero,new Vector2(3000,2000));shade.gameObject.AddComponent<UnityEngine.UI.Image>().color=new Color32(13,19,32,255);
        GameUI.Icon(Root,PixelArt.Icon("sword",0,5),new Vector2(-370,40),new Vector2(192,192));
        GameUI.Label(Root,EnglishAccount.EchoesOfTheRift,new Vector2(-330,245),new Vector2(560,80),36).color=GameUI.Gold;
        GameUI.Label(Root,EnglishAccount.LightYourPathForgeYourLegend,new Vector2(-335,167),new Vector2(545,64),24);
        enter=GameUI.Button(Root,EnglishAccount.ClickTapToEnter,new Vector2(-340,-180),new Vector2(480,64),Enter);
        prompt=enter.GetComponentInChildren<TMP_Text>();
        form=GameUI.Panel(Root,"AccountPanel",new Vector2(292,0),new Vector2(620,640));
        GameUI.Label(form,EnglishAccount.YourAdventure,new Vector2(0,270),new Vector2(520,48),32).color=GameUI.Gold;
        Username=Field(form,EnglishAccount.Username,205,false);Password=Field(form,EnglishAccount.Password5Characters,115,true);Recovery=Field(form,EnglishAccount.RecoveryCode,25,false);Recovery.gameObject.SetActive(false);
        message=GameUI.Label(form,EnglishAccount.ChooseCreateOrSignIn,new Vector2(0,-74),new Vector2(520,132),22);
        submit=GameUI.Button(form,EnglishAccount.CreateAccount,new Vector2(0,-175),new Vector2(520,48),()=>Submit());
        createMode=GameUI.Button(form,EnglishAccount.CreateAccount,new Vector2(-135,-236),new Vector2(250,44),()=>SetMode(true,false));
        signInMode=GameUI.Button(form,EnglishAccount.SignIn,new Vector2(135,-236),new Vector2(250,44),()=>SetMode(false,false));
        recoverMode=GameUI.Button(form,EnglishAccount.RecoverAccount,new Vector2(0,-287),new Vector2(520,44),()=>SetMode(false,true));
        createMode.gameObject.name="CreateAccountMode";signInMode.gameObject.name="SignInMode";recoverMode.gameObject.name="RecoverAccountMode";
        saveCode=GameUI.Button(form,EnglishAccount.IHaveSavedMyCode,new Vector2(0,-236),new Vector2(520,44),AcknowledgeRecoveryCode);saveCode.gameObject.SetActive(false);
        form.gameObject.SetActive(false);
        AccountClient.Load();yield return CheckConnection();
    }
    private RectTransform unavailable;
    public IEnumerator CheckConnection()
    {
        if(busy)yield break;busy=true;enter.interactable=false;
        prompt.text="Connecting to server...";
        bool available=false;yield return AccountClient.CheckServer(value=>available=value);
        if(!available){busy=false;ShowUnavailable();yield break;}
        if(unavailable!=null){Destroy(unavailable.gameObject);unavailable=null;}
        prompt.text=EnglishAccount.ClickTapToEnter;
        if(account==null&&!string.IsNullOrEmpty(AccountClient.Token))
        {
            prompt.text=EnglishAccount.SigningIn;
            yield return AccountClient.Request(Game.Session.Address,"refresh",new AccountRequest{refreshToken=AccountClient.Token},Accept);
            if(account==null){prompt.text=EnglishAccount.ClickTapToEnter;form.gameObject.SetActive(true);}
        }
        busy=false;enter.interactable=true;Root.GetComponent<CanvasGroup>().interactable=true;
    }
    private void ShowUnavailable()
    {
        Root.GetComponent<CanvasGroup>().interactable=false;prompt.text=EnglishAccount.ClickTapToEnter;if(unavailable!=null)return;
        unavailable=GameUI.Canvas("ServerUnavailable");unavailable.GetComponent<Canvas>().sortingOrder=1000;
        var blocker=GameUI.Rect("UnavailableShade",unavailable,Vector2.one*.5f,Vector2.zero,new Vector2(3000,2000));blocker.gameObject.AddComponent<UnityEngine.UI.Image>().color=new Color(0,0,0,.8f);
        var panel=GameUI.Panel(unavailable,"UnavailableDialog",Vector2.zero,new Vector2(736,300));
        GameUI.Label(panel,"Connection error",new Vector2(0,91),new Vector2(640,46),30).color=GameUI.Gold;
        GameUI.Label(panel,"The server is unavailable.\nPlease try again later.",new Vector2(0,15),new Vector2(640,90),26);
        GameUI.Button(panel,"Retry connection",new Vector2(-162,-94),new Vector2(288,48),()=>StartCoroutine(CheckConnection()));
        GameUI.Button(panel,"Quit",new Vector2(176,-94),new Vector2(224,48),Application.Quit);
    }

    private TMP_InputField Field(Transform parent,string title,float y,bool secret)
    {
        var rect=GameUI.Panel(parent,title+"Input",new Vector2(0,y-9),new Vector2(520,48));
        GameUI.Label(rect,title,new Vector2(0,38),new Vector2(520,28),20);
        var field=rect.gameObject.AddComponent<TMP_InputField>();
        var viewport=GameUI.Rect("Viewport",rect,Vector2.one*.5f,Vector2.zero,new Vector2(488,38));viewport.gameObject.AddComponent<UnityEngine.UI.RectMask2D>();
        var text=GameUI.Label(viewport,"",Vector2.zero,new Vector2(488,38),22);field.textViewport=viewport;field.textComponent=(TextMeshProUGUI)text;field.targetGraphic=rect.GetComponent<UnityEngine.UI.Image>();field.characterLimit=128;
        field.contentType=secret?TMP_InputField.ContentType.Password:TMP_InputField.ContentType.Standard;field.lineType=TMP_InputField.LineType.SingleLine;return field;
    }
    private void SetMode(bool create,bool reset)
    {
        if(busy||account!=null)return;
        register=create;recover=reset;Recovery.gameObject.SetActive(reset);
        string passwordLabel=reset?EnglishAccount.NewPassword5Characters:create?EnglishAccount.Password5Characters:EnglishAccount.Password5Characters.Replace(" (5+ characters)","");
        Password.transform.Find("Label").GetComponent<TMP_Text>().text=passwordLabel;
        submit.GetComponentInChildren<TMP_Text>().text=reset?"Reset password":create?EnglishAccount.CreateAccount:EnglishAccount.SignIn;
        message.text=reset?EnglishAccount.EnterYourUsernameRecoveryCodeAndA:create?EnglishAccount.CreateAccountDescription:EnglishAccount.SignInDescription;
        createMode.interactable=!create;signInMode.interactable=create;
    }
    private void AcknowledgeRecoveryCode()
    {
        if(!recoveryCopied||account==null)return;
        recoveryAcknowledged=true;saveCode.interactable=false;enter.interactable=true;
        message.text="Recovery code saved. You can enter your adventure.";
    }
    private void Update(){if(prompt!=null&&!busy)prompt.alpha=.8f+.2f*Mathf.Sin(Time.unscaledTime*2);}
    public void Enter()
    {
        if(busy||unavailable!=null)return;
        if(account==null){form.gameObject.SetActive(true);Username.Select();return;}
        if(!string.IsNullOrEmpty(recoveryCode)&&!recoveryAcknowledged)return;
        Game.ApplyAccount(account.profile);Destroy(Root.gameObject);Destroy(this);
    }
    public void Submit(bool? create=null)
    {
        if(busy||unavailable!=null)return;if(create.HasValue){register=create.Value;recover=false;}StartCoroutine(Authenticate());
    }
    private IEnumerator Authenticate()
    {
        busy=true;submit.interactable=false;message.text=EnglishAccount.ContactingTheAccountService;
        var request=new AccountRequest{username=Username.text,password=Password.text,recovery=Recovery.text};Password.text="";
        if(register&&!recover)
        {
            string old=Path.Combine(Application.persistentDataPath,"connection-identity.json");
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-accountFlowTest")<0&&File.Exists(old))try{var legacy=JsonUtility.FromJson<AccountRequest>(File.ReadAllText(old));request.id=legacy.id;request.secret=legacy.secret;}catch{}
        }
        yield return AccountClient.Request(Game.Session.Address,recover?"recover":register?"register":"login",request,Accept);
        request.password=null;busy=false;submit.interactable=true;
    }
    private void Accept(AccountResponse response,string error)
    {
        if(error==null&&(response?.profile==null||!response.profile.IsValid()))error="Account profile could not be loaded. Please retry.";
        LastError=error;
        if(error!=null){message.text=error;return;}
        account=response;bool cached=AccountClient.Save(response.refreshToken);Game.Session.SetAccount(response);
        prompt.text=EnglishAccount.EnterAdventure;
        if(!string.IsNullOrEmpty(response.recovery))
        {
            recoveryCode=response.recovery;recoveryCopied=false;recoveryAcknowledged=false;message.text=EnglishAccount.AccountReadySaveYourRecoveryCodePrivately;
            submit.interactable=true;submit.gameObject.name="CopyRecoveryCodeButton";submit.GetComponentInChildren<TMP_Text>().text=EnglishAccount.CopyRecoveryCode;submit.onClick.RemoveAllListeners();submit.onClick.AddListener(()=>
            {
                try{GUIUtility.systemCopyBuffer=recoveryCode;recoveryCopied=true;saveCode.gameObject.SetActive(true);message.text=EnglishAccount.RecoveryCodeCopiedKeepItSomewhereSafe;}
                catch(Exception){message.text="Could not copy the recovery code. Please try again.";}
            });
            createMode.gameObject.SetActive(false);signInMode.gameObject.SetActive(false);recoverMode.gameObject.SetActive(false);enter.interactable=false;
            Username.readOnly=true;Password.gameObject.SetActive(false);Recovery.gameObject.SetActive(false);
        }
        else
        {
            message.text=EnglishAccount.WelcomeBackYourProgressIsReady;
            createMode.gameObject.SetActive(false);signInMode.gameObject.SetActive(false);recoverMode.gameObject.SetActive(false);
            Username.readOnly=true;Password.gameObject.SetActive(false);Recovery.gameObject.SetActive(false);enter.interactable=true;
        }
        if(!cached)message.text+="\nThis device could not save your session; sign in next time.";
    }
    private void OnDestroy(){if(unavailable!=null)Destroy(unavailable.gameObject);if(Root!=null)Destroy(Root.gameObject);recoveryCode=null;}
}
