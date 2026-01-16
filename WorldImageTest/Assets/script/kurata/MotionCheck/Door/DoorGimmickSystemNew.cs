using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class DoorGimmickSystemNew : UdonSharpBehaviour
{
    // ドアモード定数
    private const int DOOR_MODE_ONE_SHOT = 0;
    private const int DOOR_MODE_AUTO_CLOSE = 1;
    
    // モーション要求モード定数
    private const int MOTION_MODE_SIMULTANEOUS = 0;
    private const int MOTION_MODE_SEQUENTIAL = 1;
    private const int MOTION_MODE_COUNTER = 2;
    
    [Header("コンポーネント")]
    [SerializeField] private DoorAnimationController doorController;
    [SerializeField] private UIStatusManager uiManager;
    [SerializeField] private SequentialMotionHandler sequentialHandler;
    [SerializeField] private SimultaneousMotionHandler simultaneousHandler;
    [SerializeField] private CounterMotionHandler counterHandler;
    
    [Header("動作モード")]
    [Tooltip("0: ワンショット（一度開いたら閉じない）\n1: 自動閉じる（条件が満たされなくなったら一定時間後に閉じる）")]
    [SerializeField] private int doorMode = DOOR_MODE_ONE_SHOT;
    [SerializeField] private float autoCloseDelay = 5f;
    
    [Header("モーション要求")]
    [Tooltip("0: 同時条件（3つのモーションを同時に満たす）\n1: 順次条件（3つのモーションを順番に満たす）\n2: カウンター（3つのモーションを何回でも満たせば開く）")]
    [SerializeField] private int motionMode = MOTION_MODE_SIMULTANEOUS;
    [SerializeField] private MotionType requiredMotion1 = MotionType.Jump;
    [SerializeField] private MotionType requiredMotion2 = MotionType.RightHandUp;
    [SerializeField] private MotionType requiredMotion3 = MotionType.HeadTiltLeft;
    [SerializeField] private bool useMotion1 = true;
    [SerializeField] private bool useMotion2 = false;
    [SerializeField] private bool useMotion3 = false;
    
    [Header("順次モード設定")]
    [SerializeField] private bool resetSequentialAfterOpen = false;
    
    [Header("デバッグ")]
    [SerializeField] public bool showDebugInfo = true;
    
    [Header("範囲検知システム")]
    [SerializeField] public bool useAreaTrigger = false;
    [SerializeField] public DoorAreaTrigger areaTrigger;
    
    // 内部状態
    private bool hasBeenOpened = false;
    private float autoCloseTimer = 0f;
    private bool isAreaActive = false;
    
    // モーション状態
    private bool[] motionStates = new bool[19];
    private MotionType[] requiredMotions = new MotionType[3];
    private bool[] useMotions = new bool[3];
    
    void Start()
    {
        InitializeSystem();
    }
    
    void Update()
    {
        // 範囲検知システムが有効で、エリアが非アクティブの場合は処理をスキップ
        if (useAreaTrigger && !isAreaActive)
        {
            return;
        }
        
        CheckDoorRequirements();
        UpdateUI();
    }
    
    private void InitializeSystem()
    {
        // モーション設定を初期化
        requiredMotions[0] = requiredMotion1;
        requiredMotions[1] = requiredMotion2;
        requiredMotions[2] = requiredMotion3;
        useMotions[0] = useMotion1;
        useMotions[1] = useMotion2;
        useMotions[2] = useMotion3;
        
        // 各ハンドラーを初期化
        if (sequentialHandler != null)
        {
            sequentialHandler.Initialize(motionStates, requiredMotions, useMotions);
        }
        
        if (simultaneousHandler != null)
        {
            simultaneousHandler.Initialize(motionStates, requiredMotions, useMotions);
        }
        
        if (counterHandler != null)
        {
            counterHandler.Initialize(motionStates, requiredMotions, useMotions);
        }
        
        // ドアを初期状態に設定
        if (doorController != null)
        {
            doorController.CloseDoorImmediate();
        }
        
        // 範囲検知システムの初期状態を設定
        if (useAreaTrigger)
        {
            isAreaActive = false;
        }
        else
        {
            isAreaActive = true; // 範囲検知システムを使用しない場合は常にアクティブ
        }
    }
    
    private void CheckDoorRequirements()
    {
        bool allRequirementsMet = false;
        
        switch (motionMode)
        {
            case MOTION_MODE_SIMULTANEOUS:
                if (simultaneousHandler != null)
                {
                    allRequirementsMet = simultaneousHandler.CheckRequirements();
                }
                break;
                
            case MOTION_MODE_SEQUENTIAL:
                if (sequentialHandler != null)
                {
                    allRequirementsMet = sequentialHandler.CheckRequirements();
                }
                break;
                
            case MOTION_MODE_COUNTER:
                if (counterHandler != null)
                {
                    allRequirementsMet = counterHandler.CheckRequirements();
                }
                break;
        }
        
        // ドアの開閉状態を更新
        switch (doorMode)
        {
            case DOOR_MODE_ONE_SHOT:
                if (allRequirementsMet && !doorController.IsDoorOpen && !doorController.IsOpening && !hasBeenOpened)
                {
                    if (showDebugInfo)
                    {
                        Debug.Log("[DoorGimmickSystem] ワンショットモード: ドアを開く条件が満たされました！");
                    }
                    OpenDoor();
                    hasBeenOpened = true;
                }
                else if (allRequirementsMet && !doorController.IsDoorOpen && !doorController.IsOpening)
                {
                    // デバッグ情報を追加（順次モードで扉が開かない原因を特定するため）
                    if (showDebugInfo)
                    {
                        Debug.LogWarning($"[DoorGimmickSystem] 条件は満たされていますが、hasBeenOpened={hasBeenOpened}のため扉が開きません。モード: {(motionMode == MOTION_MODE_SEQUENTIAL ? "順次" : "同時/カウンター")}");
                    }
                }
                else if (!allRequirementsMet && showDebugInfo)
                {
                    // 条件が満たされていない場合のデバッグ情報
                    Debug.Log($"[DoorGimmickSystem] ドアを開く条件が満たされていません。allRequirementsMet={allRequirementsMet}, IsDoorOpen={doorController.IsDoorOpen}, IsOpening={doorController.IsOpening}");
                }
                break;
                
            case DOOR_MODE_AUTO_CLOSE:
                if (allRequirementsMet && !doorController.IsDoorOpen && !doorController.IsOpening)
                {
                    if (showDebugInfo)
                    {
                        Debug.Log("[DoorGimmickSystem] 自動閉じモード: ドアを開く条件が満たされました！");
                    }
                    OpenDoor();
                    autoCloseTimer = 0f;
                }
                else if (!allRequirementsMet && doorController.IsDoorOpen && !doorController.IsClosing)
                {
                    autoCloseTimer += Time.deltaTime;
                    if (autoCloseTimer >= autoCloseDelay)
                    {
                        CloseDoor();
                        autoCloseTimer = 0f;
                    }
                }
                else if (allRequirementsMet && doorController.IsDoorOpen)
                {
                    autoCloseTimer = 0f;
                }
                break;
        }
    }
    
    private void UpdateUI()
    {
        if (uiManager != null)
        {
            // UIマネージャーにデータを送信
            uiManager.SetData(
                motionMode,
                motionStates,
                requiredMotions,
                useMotions,
                doorController.IsDoorOpen,
                doorController.IsOpening,
                sequentialHandler != null ? sequentialHandler.CurrentStep : 0,
                sequentialHandler != null ? sequentialHandler.StepCompleted : new bool[3],
                sequentialHandler != null ? sequentialHandler.IsInCooldown : false,
                sequentialHandler != null ? sequentialHandler.CooldownTimer : 0f,
                sequentialHandler != null ? sequentialHandler.HasWrongMotion : false,
                sequentialHandler != null ? sequentialHandler.WrongMotionTimer : 0f,
                counterHandler != null ? counterHandler.MotionCounter : 0,
                counterHandler != null ? counterHandler.RequiredCount : 0,
                counterHandler != null ? counterHandler.MotionAchievedCounter : new bool[3]
            );
        }
    }
    
    private void OpenDoor()
    {
        if (doorController != null)
        {
            doorController.OpenDoor();
            
            // 順次モードでドアが開いた場合、リセットするかどうかチェック
            if (motionMode == MOTION_MODE_SEQUENTIAL && resetSequentialAfterOpen && sequentialHandler != null)
            {
                sequentialHandler.ResetSequentialMode();
            }
        }
    }
    
    private void CloseDoor()
    {
        if (doorController != null)
        {
            doorController.CloseDoor();
        }
    }
    
    // 外部からモーション状態を設定するメソッド
    public void SetMotionState(MotionType motionType, bool state)
    {
        motionStates[(int)motionType] = state;
    }
    
    // 外部からモーション状態を取得するメソッド
    public bool GetMotionState(MotionType motionType)
    {
        return motionStates[(int)motionType];
    }
    
    // 各モーションの成功状態を取得するメソッド（MotionSuccessObjectActivator用）
    public bool IsMotion1Success()
    {
        if (!useMotions[0]) return false;
        return motionStates[(int)requiredMotions[0]];
    }
    
    public bool IsMotion2Success()
    {
        if (!useMotions[1]) return false;
        return motionStates[(int)requiredMotions[1]];
    }
    
    public bool IsMotion3Success()
    {
        if (!useMotions[2]) return false;
        return motionStates[(int)requiredMotions[2]];
    }
    
    // 各モーション検出器用の専用メソッド
    public void SetJumpState(bool state) => SetMotionState(MotionType.Jump, state);
    public void SetCrouchState(bool state) => SetMotionState(MotionType.Crouch, state);
    public void SetProneState(bool state) => SetMotionState(MotionType.Prone, state);
    public void SetHeadTiltLeftState(bool state) => SetMotionState(MotionType.HeadTiltLeft, state);
    public void SetHeadTiltRightState(bool state) => SetMotionState(MotionType.HeadTiltRight, state);
    public void SetHeadTiltForwardState(bool state) => SetMotionState(MotionType.HeadTiltForward, state);
    public void SetHeadTiltBackwardState(bool state) => SetMotionState(MotionType.HeadTiltBackward, state);
    public void SetHeadTurnLeftState(bool state) => SetMotionState(MotionType.HeadTurnLeft, state);
    public void SetHeadTurnRightState(bool state) => SetMotionState(MotionType.HeadTurnRight, state);
    public void SetBodyLeanLeftState(bool state) => SetMotionState(MotionType.BodyLeanLeft, state);
    public void SetBodyLeanRightState(bool state) => SetMotionState(MotionType.BodyLeanRight, state);
    public void SetBodyLeanForwardState(bool state) => SetMotionState(MotionType.BodyLeanForward, state);
    public void SetBodyLeanBackwardState(bool state) => SetMotionState(MotionType.BodyLeanBackward, state);
    public void SetRightHandUpState(bool state) => SetMotionState(MotionType.RightHandUp, state);
    public void SetRightHandSideState(bool state) => SetMotionState(MotionType.RightHandSide, state);
    public void SetRightHandForwardState(bool state) => SetMotionState(MotionType.RightHandForward, state);
    public void SetLeftHandUpState(bool state) => SetMotionState(MotionType.LeftHandUp, state);
    public void SetLeftHandSideState(bool state) => SetMotionState(MotionType.LeftHandSide, state);
    public void SetLeftHandForwardState(bool state) => SetMotionState(MotionType.LeftHandForward, state);
    
    // 範囲検知システムとの連携
    public void SetAreaActive(bool active)
    {
        isAreaActive = active;
        
        if (!active)
        {
            // エリアが非アクティブになった時の処理
            ResetMotionRequirements();
        }
    }
    
    // エリアの状態を取得
    public bool IsAreaActive => isAreaActive;
    
    // リセットメソッド
    public void ResetOneShotMode()
    {
        hasBeenOpened = false;
    }
    
    public void ResetMotionRequirements()
    {
        // すべてのモーション状態をリセット
        for (int i = 0; i < motionStates.Length; i++)
        {
            motionStates[i] = false;
        }
        
        // 各ハンドラーをリセット
        if (sequentialHandler != null)
        {
            sequentialHandler.ResetSequentialMode();
        }
        
        if (counterHandler != null)
        {
            counterHandler.ResetCounter();
        }
    }
    
    public void ResetSequentialModeManual()
    {
        if (sequentialHandler != null)
        {
            sequentialHandler.ResetSequentialMode();
        }
    }
    
    private string GetMotionName(MotionType motionType)
    {
        switch (motionType)
        {
            case MotionType.Jump:
                return "ジャンプ";
            case MotionType.Crouch:
                return "しゃがむ";
            case MotionType.Prone:
                return "伏せる";
            case MotionType.HeadTiltLeft:
                return "頭を左に傾ける";
            case MotionType.HeadTiltRight:
                return "頭を右に傾ける";
            case MotionType.HeadTiltForward:
                return "頭を前に傾ける";
            case MotionType.HeadTiltBackward:
                return "頭を後ろに傾ける";
            case MotionType.HeadTurnLeft:
                return "頭を左に回す";
            case MotionType.HeadTurnRight:
                return "頭を右に回す";
            case MotionType.BodyLeanLeft:
                return "体を左に傾ける";
            case MotionType.BodyLeanRight:
                return "体を右に傾ける";
            case MotionType.BodyLeanForward:
                return "体を前に傾ける";
            case MotionType.BodyLeanBackward:
                return "体を後ろに傾ける";
            case MotionType.RightHandUp:
                return "右手を上げる";
            case MotionType.RightHandSide:
                return "右手を横に伸ばす";
            case MotionType.RightHandForward:
                return "右手を前に伸ばす";
            case MotionType.LeftHandUp:
                return "左手を上げる";
            case MotionType.LeftHandSide:
                return "左手を横に伸ばす";
            case MotionType.LeftHandForward:
                return "左手を前に伸ばす";
            default:
                return motionType.ToString();
        }
    }
} 