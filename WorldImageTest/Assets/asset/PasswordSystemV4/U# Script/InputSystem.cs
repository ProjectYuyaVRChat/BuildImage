
using UdonSharp;
using UnityEditor;
using UnityEngine;
using System;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;
using TMPro;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class InputSystem : UdonSharpBehaviour
{

    [Header("内部設定　※基本的に弄る必要はありません")]
    [SerializeField] private Text TextPool;
    [SerializeField] private Text InputText;
    [SerializeField] private Text PlaceHolder;
    [SerializeField] private AudioSource SE;
    [SerializeField] private Toggle[] Language;
    [SerializeField] private GameObject BackGround;
    [SerializeField] private GameObject[] DummyButton;
    [SerializeField] private Toggle[] Caps;
    [SerializeField] private Toggle[] Shift;
    [System.NonSerialized] public string StrPool = "";
    [UdonSynced, FieldChangeCallback(nameof(SyncPass))]
    [System.NonSerialized] public string _SyncPass;
    [UdonSynced, FieldChangeCallback(nameof(SyncAns))]
    [System.NonSerialized] public string _SyncAns;
    private string[] HangleBefore = {
        "ᄀ", "ᄁ", "ᄀᄉ", "ᄂ", "ᄂᄌ", "ᄂᄒ", "ᄃ", "ᄅ", "ᄅᄀ", "ᄅᄆ", "ᄅᄇ", "ᄅᄉ", "ᄅᄐ", "ᄅᄑ", "ᄅᄒ",
        "ᄆ", "ᄇ", "ᄇᄉ", "ᄉ", "ᄊ", "ᄋ", "ᄌ", "ᄎ", "ᄏ", "ᄐ", "ᄑ", "ᄒ" };
    private string[] HangleAfter = {
        "ᆨ", "ᆩ", "ᆪ", "ᆫ", "ᆬ", "ᆭ", "ᆮ", "ᆯ", "ᆰ", "ᆱ", "ᆲ", "ᆳ", "ᆴ", "ᆵ", "ᆶ",
        "ᆷ", "ᆸ", "ᆹ", "ᆺ", "ᆻ", "ᆼ", "ᆽ", "ᆾ", "ᆿ", "ᇀ", "ᇁ", "ᇂ" };
    private string[] HangleReturn = {
        "ᄀ", "ᄁ", "ᆨᄉ", "ᄂ", "ᆫᄌ", "ᆫᄒ", "ᄃ", "ᄅ", "ᆯᄀ", "ᆯᄆ", "ᆯᄇ", "ᆯᄉ", "ᆯᄐ", "ᆯᄑ", "ᆯᄒ",
        "ᄆ", "ᄇ", "ᆸᄉ", "ᄉ", "ᄊ", "ᄋ", "ᄌ", "ᄎ", "ᄏ", "ᄐ", "ᄑ", "ᄒ" };
    private string[] Medial = { "ᅡ", "ᅢ", "ᅣ", "ᅤ", "ᅥ", "ᅦ", "ᅧ", "ᅨ", "ᅩ", "ᅭ", "ᅮ", "ᅲ", "ᅳ", "ᅵ" };
    private string[] MedialDBefore = { "ᅩᅡ", "ᅩᅢ", "ᅩᅵ", "ᅮᅥ", "ᅮᅦ", "ᅮᅵ", "ᅳᅵ" };
    private string[] MedialDAfter = { "ᅪ", "ᅫ", "ᅬ", "ᅯ", "ᅰ", "ᅱ", "ᅴ" };

    [Space(10f)]
    [Header("【Join時の初期表示言語】　0:かな / 1:カナ / 2:Aa / 3:한글")]
    [SerializeField,Range(0,3)] private int _Language;

    [Space(10f)]
    [Header("◆◆◆以下、変数にカーソルを当てると説明を表示します◆◆◆")]
    [Header("【基本設定】")]
    [SerializeField, Tooltip("UIに背景を表示するか")] private bool _BackGround;
    [SerializeField, Tooltip("Aa、한글にダミーボタンを表示するか\nオンにした場合、キー配列がV3までの長方形型になります")] private bool _DummyButton;
    [SerializeField, Tooltip("文字数制限\n値をマイナスにすると上限無しになります")] private int _MaxLength = -1;

    [Space(10f)]
    [Header("【パスワード設定】")]
    [SerializeField, Tooltip("答えとなるパスワード")] private string _Password;
    [SerializeField, Tooltip("複数入力可能なパスワード\nSizeが1以上だとPasswordを無視してこちらを参照、\nSizeを0にすると参照せず、Passwordを確認します")] private string[] _MultiPassword;
    [Tooltip("問題番号（MultiPassword有効時のみ）\nこのintと一致するMultiPasswordの答えを確認します\n※値は他のUdonBehaviourから変更可能\n値をマイナスにすると「MultiPasswordのどれかが一致していれば正解」のモードになります")] public int QuestionNum = -1;
    [SerializeField, Tooltip("正解時イベント先（シングルパスワード時）\nQuestionNumがマイナスの時はこちらのイベントが発火します\nQuestionNumがマイナスかつココに何も入れなければMultiEventを参照します")] private UdonBehaviour _SingleEvent;
    [SerializeField, Tooltip("正解時イベント先（マルチパスワード時）\nMultiPasswordのリストと対応したイベントが発火します\nSizeはMultiPasswordと一致させてください")] private GameObject[] _MultiEvent;

    [Space(10f)]
    [Header("【効果音設定】　※設定しない場合、音は鳴りません")]
    [SerializeField, Tooltip("入力可能な文字を押したときの効果音")] private AudioClip _ButtonSE;
    [SerializeField, Tooltip("正解音")] private AudioClip _CorrectSE;
    [SerializeField, Tooltip("誤答音")] private AudioClip _WrongSE;
    [SerializeField, Tooltip("BackSpace時の効果音")] private AudioClip _BackSpaceSE;
    [SerializeField, Tooltip("Clear時の効果音")] private AudioClip _ClearSE;

    [Space(10f)]
    [Header("【詳細設定】")]
    [SerializeField, Tooltip("同期設定\nオンにすると入力文字が同期します")] private bool _isSynced;
    [SerializeField, Tooltip("SE同期設定\nオンにすると各効果音が同期して鳴ります")] private bool _isSyncedSE;
    [SerializeField, Tooltip("SendCustomNetworkEvent設定\nオンにすると正解時イベントをSendCustomNetworkEventで飛ばします\nTargetは↓で設定してください")] private bool _SCNE;
    [SerializeField, Tooltip("SCNE有効時のTargetです")] public VRC.Udon.Common.Interfaces.NetworkEventTarget _Target;
    [SerializeField, Tooltip("再入力可否\nオフにすると正解時のイベントを何度でも発火します")] private bool _isOneShot = true;
    [SerializeField, Tooltip("正解時イベント名")] private string _EventName = "Event";

    void Start()
    {
        BackGround.SetActive(_BackGround);
        Language[_Language].isOn = true;
        foreach(GameObject DB in DummyButton)
        {
            if (DB == null) continue;
            DB.SetActive(_DummyButton);
        }
        if (_Password != null) _Password = _Password.Normalize(System.Text.NormalizationForm.FormC);
        if (_MultiPassword.Length >= 1)
        {
            for (int i = 0; i < _MultiPassword.Length; i++)
            {
                if (_MultiPassword[i] != null) _MultiPassword[i] = _MultiPassword[i].Normalize(System.Text.NormalizationForm.FormC);
            }
        }
        if (_isSynced) RequestSerialization();
    }
    
    public void OwnerCng()
    {
        if(_isSynced) Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
    }

    public void Set()
    {
        if (StrPool.Length <= _MaxLength || _MaxLength < 0)
        {
            StrPool += TextPool.text;
            if (StrPool.Length >= 2)
            {
                if (Array.IndexOf(MedialDBefore, StrPool.Substring(StrPool.Length - 2)) != -1)
                {
                    StrPool = StrPool.Substring(0, StrPool.Length - 2) + MedialDAfter[Array.IndexOf(MedialDBefore, StrPool.Substring(StrPool.Length - 2))];
                }
                else if (Array.IndexOf(Medial, StrPool.Substring(StrPool.Length - 1)) != -1 && Array.IndexOf(HangleAfter, StrPool.Substring(StrPool.Length - 2, 1)) != -1)
                {
                    StrPool = StrPool.Substring(0, StrPool.Length - 2) + HangleReturn[Array.IndexOf(HangleAfter, StrPool.Substring(StrPool.Length - 2, 1))] + TextPool.text;
                }
                else if (Array.IndexOf(HangleBefore, StrPool.Substring(StrPool.Length - 2)) != -1)
                {
                    StrPool = StrPool.Substring(0, StrPool.Length - 2) + HangleAfter[Array.IndexOf(HangleBefore, StrPool.Substring(StrPool.Length - 2))];
                }
                else if (Array.IndexOf(HangleBefore, StrPool.Substring(StrPool.Length - 1)) != -1)
                {
                    StrPool = StrPool.Substring(0, StrPool.Length - 1) + HangleAfter[Array.IndexOf(HangleBefore, StrPool.Substring(StrPool.Length - 1))];
                }
            }
            if (_isSynced)
            {
                if(!Networking.LocalPlayer.IsOwner(this.gameObject)) Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
                _SyncPass = StrPool;
                RequestSerialization();
            }
            if(_isSyncedSE)
            {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "SetSE");
            }
            else
            {
                SetSE();
            }
            SetPass();
        }
        for (int i = 0;i < Caps.Length;i++)
        {
            if (!Caps[i].isOn) Shift[i].isOn = false;
        }
    }

    public void Dakuten()
    {
        if (!String.IsNullOrEmpty(StrPool))
        {
            int c = char.ConvertToUtf32(StrPool, StrPool.Length - 1);
            string d = StrPool + Convert.ToChar(Convert.ToInt32("3099", 16)).ToString();
            if (c == 12441)
            {
                StrPool = StrPool.Substring(0, StrPool.Length - 1);
                if (_isSynced)
                {
                    if (!Networking.LocalPlayer.IsOwner(this.gameObject)) Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
                    _SyncPass = StrPool;
                    RequestSerialization();
                }
                if (_isSyncedSE)
                {
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "SetSE");
                }
                else
                {
                    SetSE();
                }
                SetPass();
            }
            else if (c == 12442)
            {
                StrPool = StrPool.Substring(0, StrPool.Length - 1) + Convert.ToChar(Convert.ToInt32("3099", 16)).ToString();
                if (_isSynced)
                {
                    if (!Networking.LocalPlayer.IsOwner(this.gameObject)) Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
                    _SyncPass = StrPool;
                    RequestSerialization();
                }
                if (_isSyncedSE)
                {
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "SetSE");
                }
                else
                {
                    SetSE();
                }
                SetPass();
            }
            else if (StrPool.Normalize(System.Text.NormalizationForm.FormC).Length == d.Normalize(System.Text.NormalizationForm.FormC).Length)
            {
                StrPool = d;
                if (_isSynced)
                {
                    if (!Networking.LocalPlayer.IsOwner(this.gameObject)) Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
                    _SyncPass = StrPool;
                    RequestSerialization();
                }
                if (_isSyncedSE)
                {
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "SetSE");
                }
                else
                {
                    SetSE();
                }
                SetPass();
            }
        }
    }

    public void Handakuten()
    {
        if (!String.IsNullOrEmpty(StrPool))
        {
            int c = char.ConvertToUtf32(StrPool, StrPool.Length - 1);
            string d = StrPool + Convert.ToChar(Convert.ToInt32("309A", 16)).ToString();
            string h = StrPool.Substring(0, StrPool.Length - 1) + Convert.ToChar(Convert.ToInt32("309A", 16)).ToString();
            if (c == 12441 && StrPool.Normalize(System.Text.NormalizationForm.FormC).Length == h.Normalize(System.Text.NormalizationForm.FormC).Length)
            {
                StrPool = h;
                if (_isSynced)
                {
                    if (!Networking.LocalPlayer.IsOwner(this.gameObject)) Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
                    _SyncPass = StrPool;
                    RequestSerialization();
                }
                if (_isSyncedSE)
                {
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "SetSE");
                }
                else
                {
                    SetSE();
                }
                SetPass();
            }
            else if (c == 12441 || c == 12442)
            {
                StrPool = StrPool.Substring(0, StrPool.Length - 1);
                if (_isSynced)
                {
                    if (!Networking.LocalPlayer.IsOwner(this.gameObject)) Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
                    _SyncPass = StrPool;
                    RequestSerialization();
                }
                if (_isSyncedSE)
                {
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "SetSE");
                }
                else
                {
                    SetSE();
                }
                SetPass();
            }
            else if (StrPool.Normalize(System.Text.NormalizationForm.FormC).Length == d.Normalize(System.Text.NormalizationForm.FormC).Length)
            {
                StrPool = d;
                if (_isSynced)
                {
                    if (!Networking.LocalPlayer.IsOwner(this.gameObject)) Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
                    _SyncPass = StrPool;
                    RequestSerialization();
                }
                if (_isSyncedSE)
                {
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "SetSE");
                }
                else
                {
                    SetSE();
                }
                SetPass();
            }
        }
    }

    public void Clear()
    {
        StrPool = string.Empty;
        if (_isSynced)
        {
            if (!Networking.LocalPlayer.IsOwner(this.gameObject)) Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
            _SyncPass = StrPool;
            RequestSerialization();
        }
        if (_isSyncedSE)
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "ClearSE");
        }
        else
        {
            ClearSE();
        }
        SetPass();
    }

    public void BS()
    {
        if (!string.IsNullOrEmpty(StrPool))
        {
            StrPool = InputText.text.Substring(0, InputText.text.Length - 1);
            if (_isSynced)
            {
                if (!Networking.LocalPlayer.IsOwner(this.gameObject)) Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
                _SyncPass = StrPool;
                RequestSerialization();
            }
            if (_isSyncedSE)
            {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "BSSE");
            }
            else
            {
                BSSE();
            }
            SetPass();
        }
    }

    public void CapsLock()
    {
        for(int i = 0;i < Caps.Length;i++)
        {
            Shift[i].interactable = !Caps[i].isOn;
            if (Caps[i].isOn == true) Shift[i].isOn = true;
        }
    }

    public string SyncPass
    {
        get => _SyncPass;
        set
        {
            _SyncPass = value;
            if (_isSynced)
            {
                StrPool = _SyncPass;
                SetPass();
            }
        }
    }

    public void SetPass()
    {
        
            InputText.text = StrPool.Normalize(System.Text.NormalizationForm.FormC);
            PlaceHolder.enabled = string.IsNullOrEmpty(StrPool);
        
    }

    public void Enter()
    {
        if(_MultiPassword.Length == 0)
        {
            if(InputText.text == _Password && _SCNE)
            {
                _SingleEvent.SendCustomNetworkEvent(_Target, _EventName);
                if (_isOneShot) _Password = "済";
                if (_isSyncedSE)
                {
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "CorrectSE");
                }
                else
                {
                    CorrectSE();
                }
            }
            else if (InputText.text == _Password && !_SCNE)
            {
                _SingleEvent.SendCustomEvent(_EventName);
                if (_isOneShot) _Password = "済";
                if (_isSyncedSE)
                {
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "CorrectSE");
                }
                else
                {
                    CorrectSE();
                }
            }
        }
        else
        {
            if(QuestionNum < 0)
            {
                int _QNum = Array.IndexOf(_MultiPassword, InputText.text);
                if (_QNum >= 0)
                {
                    if (!_SingleEvent == null)
                    {
                        if (_SCNE) _SingleEvent.SendCustomNetworkEvent(_Target, _EventName);
                        else _SingleEvent.SendCustomEvent(_EventName);
                        if (_isOneShot)
                        {
                            for (int i = 0; i < _MultiPassword.Length; i++)
                            {
                                _MultiPassword[i] = "済";
                            }
                        }
                    }
                    else
                    {
                        UdonBehaviour multiBehaviour = (UdonBehaviour)_MultiEvent[_QNum].GetComponent(typeof(UdonBehaviour));
                        if (_SCNE)
                        {
                            multiBehaviour.SendCustomNetworkEvent(_Target, _EventName);
                        }
                        else
                        {
                            multiBehaviour.SendCustomEvent(_EventName);
                        }
                        if (_isOneShot)
                        {
                            for (int i = 0; i < _MultiPassword.Length; i++)
                            {
                                if (_MultiPassword[i] == InputText.text) _MultiPassword[i] = "済";
                            }
                        }
                    }
                    if (_isSyncedSE)
                    {
                        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "CorrectSE");
                    }
                    else
                    {
                        CorrectSE();
                    }
                }
            }
            else if (InputText.text == _MultiPassword[QuestionNum] && _SCNE)
            {
                UdonBehaviour behaviour = (UdonBehaviour)_MultiEvent[QuestionNum].GetComponent(typeof(UdonBehaviour));
                behaviour.SendCustomNetworkEvent(_Target, _EventName);
                if (_isOneShot) _MultiPassword[QuestionNum] = "済";
                if (_isSyncedSE)
                {
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "CorrectSE");
                }
                else
                {
                    CorrectSE();
                }
            }
            else if (InputText.text == _MultiPassword[QuestionNum])
            {
                UdonBehaviour behaviour = (UdonBehaviour)_MultiEvent[QuestionNum].GetComponent(typeof(UdonBehaviour));
                behaviour.SendCustomEvent(_EventName);
                if (_isOneShot) _MultiPassword[QuestionNum] = "済";
                if (_isSyncedSE)
                {
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "CorrectSE");
                }
                else
                {
                    CorrectSE();
                }
            }
            else if (_isSyncedSE)
            {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "WrongSE");
            }
            else
            {
                WrongSE();
            }
        }
        if(_isSynced && _MultiPassword.Length == 0)
        {
            _SyncAns = _Password;
        }
        else if(_isSynced && _MultiPassword.Length != 0)
        {
            _SyncAns = string.Join("結", _MultiPassword);
        }
        RequestSerialization();
    }

    public string SyncAns
    {
        get => _SyncAns;
        set
        {
            _SyncAns = value;
            SetAns();
        }
    }

    public void SetAns()
    {
        if (_isSynced && _MultiPassword.Length == 0)
        {
            _Password = _SyncAns;
        }
        else if (_isSynced && _MultiPassword.Length != 0)
        {
            _MultiPassword = _SyncAns.Split('結');
        }
    }

    public void SetSE()
    {
        if (_ButtonSE != null) SE.PlayOneShot(_ButtonSE, SE.volume);
    }

    public void ClearSE()
    {
        if (_ClearSE != null) SE.PlayOneShot(_ClearSE, SE.volume);
    }

    public void BSSE()
    {
        if (_BackSpaceSE != null) SE.PlayOneShot(_BackSpaceSE, SE.volume);
    }

    public void CorrectSE()
    {
        if (_CorrectSE != null) SE.PlayOneShot(_CorrectSE, SE.volume);
    }

    public void WrongSE()
    {
        if (_WrongSE != null) SE.PlayOneShot(_WrongSE, SE.volume);
    }
}
