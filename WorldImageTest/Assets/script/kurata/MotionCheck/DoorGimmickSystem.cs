using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public enum MotionType
{
    Jump,
    Crouch,
    Prone,
    HeadTiltLeft,
    HeadTiltRight,
    HeadTiltForward,
    HeadTiltBackward,
    HeadTurnLeft,
    HeadTurnRight,
    BodyLeanLeft,
    BodyLeanRight,
    BodyLeanForward,
    BodyLeanBackward,
    RightHandUp,
    RightHandSide,
    RightHandForward,
    LeftHandUp,
    LeftHandSide,
    LeftHandForward
}

public class DoorGimmickSystem : UdonSharpBehaviour
{
    // ドアモード定数
    private const int DOOR_MODE_ONE_SHOT = 0;
    private const int DOOR_MODE_AUTO_CLOSE = 1;
    
    // モーション要求モード定数
    private const int MOTION_MODE_SIMULTANEOUS = 0;  // 同時条件：3つのモーションを同時に満たす
    private const int MOTION_MODE_SEQUENTIAL = 1;    // 順次条件：3つのモーションを順番に満たす
    private const int MOTION_MODE_COUNTER = 2;       // カウンター：3つのモーションを何回でも満たせば開く
    
    [Header("ドア設定")]
    [SerializeField] private GameObject leftDoor;  // 左側のドア
    [SerializeField] private GameObject rightDoor; // 右側のドア
    [SerializeField] private float openAngle = 90f; // 開く角度
    [SerializeField] private float openSpeed = 2f;  // 開く速度
    
    [Header("動作モード")]
    [Tooltip("0: ワンショット（一度開いたら閉じない）\n1: 自動閉じる（条件が満たされなくなったら一定時間後に閉じる）")]
    [SerializeField] private int doorMode = DOOR_MODE_ONE_SHOT; // ドアの動作モード
    [SerializeField] private float autoCloseDelay = 5f; // 自動閉じるまでの時間（秒）
    
    [Header("モーション要求")]
    [Header("モード説明:")]
    [Header("0=同時条件: 3つのモーションを同時に満たす必要があります")]
    [Header("1=順次条件: 3つのモーションを順番に満たす必要があります")]
    [Header("2=カウンター: 3つのモーションを何回でも満たせば開きます")]
    [Tooltip("0: 同時条件（3つのモーションを同時に満たす）\n1: 順次条件（3つのモーションを順番に満たす）\n2: カウンター（3つのモーションを何回でも満たせば開く）")]
    [SerializeField] private int motionMode = MOTION_MODE_SIMULTANEOUS; // モーション要求モード
    [Tooltip("1番目のモーション（同時条件では必須、順次条件では1番目、カウンターではカウント対象）")]
    [SerializeField] private MotionType requiredMotion1 = MotionType.Jump;
    [Tooltip("2番目のモーション（同時条件では必須、順次条件では2番目、カウンターではカウント対象）")]
    [SerializeField] private MotionType requiredMotion2 = MotionType.RightHandUp;
    [Tooltip("3番目のモーション（同時条件では必須、順次条件では3番目、カウンターではカウント対象）")]
    [SerializeField] private MotionType requiredMotion3 = MotionType.HeadTiltLeft;
    [Tooltip("1番目のモーションを使用するかどうか")]
    [SerializeField] private bool useMotion1 = true;
    [Tooltip("2番目のモーションを使用するかどうか")]
    [SerializeField] private bool useMotion2 = false;
    [Tooltip("3番目のモーションを使用するかどうか")]
    [SerializeField] private bool useMotion3 = false;
    
    [Header("デバッグ")]
    [SerializeField] private bool showDebugInfo = true;
    
    // 内部状態
    private bool isDoorOpen = false;
    private bool isOpening = false;
    private bool isClosing = false;
    private float currentAngle = 0f;
    private float autoCloseTimer = 0f;
    private bool hasBeenOpened = false; // ワンショットモード用
    
    // 順次モード用
    private int currentStep = 0; // 現在のステップ（0, 1, 2）
    private bool[] stepCompleted = new bool[3]; // 各ステップの完了状態
    private bool[] motionAchieved = new bool[3]; // 各モーションを一度でも達成したかどうか
    
    // カウンターモード用
    private int motionCounter = 0; // 満たしたモーションの数
    private int requiredCount = 0; // 必要なモーション数
    private bool[] motionAchievedCounter = new bool[3]; // 各モーションを一度でも達成したかどうか（カウンター用）
    
    // Dictionaryの代わりに配列を使用
    private bool[] motionStates = new bool[19]; // MotionTypeの数
    
    void Start()
    {
        InitializeMotionStates();
        CloseDoorImmediate();
    }
    
    void Update()
    {
        CheckDoorRequirements();
        UpdateDoorAnimation();
    }
    
    private void InitializeMotionStates()
    {
        // すべてのモーション状態を初期化
        for (int i = 0; i < motionStates.Length; i++)
        {
            motionStates[i] = false;
        }
        
        // 順次モード用の初期化
        currentStep = 0;
        for (int i = 0; i < stepCompleted.Length; i++)
        {
            stepCompleted[i] = false;
            motionAchieved[i] = false;
        }
        
        // カウンターモード用の初期化
        motionCounter = 0;
        requiredCount = 0;
        for (int i = 0; i < motionAchievedCounter.Length; i++)
        {
            motionAchievedCounter[i] = false;
        }
        if (useMotion1) requiredCount++;
        if (useMotion2) requiredCount++;
        if (useMotion3) requiredCount++;
    }
    
    private void CheckDoorRequirements()
    {
        bool allRequirementsMet = false;
        
        switch (motionMode)
        {
            case MOTION_MODE_SIMULTANEOUS:
                // 同時条件：3つのモーションを同時に満たす
                allRequirementsMet = CheckSimultaneousRequirements();
                break;
                
            case MOTION_MODE_SEQUENTIAL:
                // 順次条件：3つのモーションを順番に満たす
                allRequirementsMet = CheckSequentialRequirements();
                break;
                
            case MOTION_MODE_COUNTER:
                // カウンター：3つのモーションを何回でも満たせば開く
                allRequirementsMet = CheckCounterRequirements();
                break;
        }
        
        // ドアの開閉状態を更新
        switch (doorMode)
        {
            case DOOR_MODE_ONE_SHOT:
                // ワンショットモード：一度開いたら条件が満たされなくなっても閉じない
                if (allRequirementsMet && !isDoorOpen && !isOpening && !hasBeenOpened)
                {
                    OpenDoor();
                    hasBeenOpened = true;
                }
                break;
                
            case DOOR_MODE_AUTO_CLOSE:
                // 自動閉じるモード：条件が満たされなくなったら一定時間後に閉じる
                if (allRequirementsMet && !isDoorOpen && !isOpening)
                {
                    OpenDoor();
                    autoCloseTimer = 0f;
                }
                else if (!allRequirementsMet && isDoorOpen && !isClosing)
                {
                    // 条件が満たされなくなったらタイマーを開始
                    autoCloseTimer += Time.deltaTime;
                    if (autoCloseTimer >= autoCloseDelay)
                    {
                        CloseDoor();
                        autoCloseTimer = 0f;
                    }
                }
                else if (allRequirementsMet && isDoorOpen)
                {
                    // 条件が再び満たされたらタイマーをリセット
                    autoCloseTimer = 0f;
                }
                break;
        }
    }
    
    private bool CheckSimultaneousRequirements()
    {
        bool allMet = true;
        
        if (useMotion1 && !motionStates[(int)requiredMotion1])
        {
            allMet = false;
        }
        
        if (useMotion2 && !motionStates[(int)requiredMotion2])
        {
            allMet = false;
        }
        
        if (useMotion3 && !motionStates[(int)requiredMotion3])
        {
            allMet = false;
        }
        
        return allMet;
    }
    
    private bool CheckSequentialRequirements()
    {
        // 現在のステップのモーションが満たされているかチェック
        bool currentStepMet = false;
        
        switch (currentStep)
        {
            case 0:
                if (useMotion1 && motionStates[(int)requiredMotion1])
                {
                    stepCompleted[0] = true;
                    motionAchieved[0] = true; // 一度達成したことを記録
                    currentStep = 1;
                    currentStepMet = true;
                    if (showDebugInfo)
                    {
                        Debug.Log("[DoorGimmick] ステップ1完了: " + requiredMotion1);
                    }
                }
                break;
                
            case 1:
                if (useMotion2 && motionStates[(int)requiredMotion2])
                {
                    stepCompleted[1] = true;
                    motionAchieved[1] = true; // 一度達成したことを記録
                    currentStep = 2;
                    currentStepMet = true;
                    if (showDebugInfo)
                    {
                        Debug.Log("[DoorGimmick] ステップ2完了: " + requiredMotion2);
                    }
                }
                break;
                
            case 2:
                if (useMotion3 && motionStates[(int)requiredMotion3])
                {
                    stepCompleted[2] = true;
                    motionAchieved[2] = true; // 一度達成したことを記録
                    currentStepMet = true;
                    if (showDebugInfo)
                    {
                        Debug.Log("[DoorGimmick] ステップ3完了: " + requiredMotion3);
                    }
                }
                break;
        }
        
        // すべてのステップが完了しているかチェック
        return stepCompleted[0] && stepCompleted[1] && stepCompleted[2];
    }
    
    private bool CheckCounterRequirements()
    {
        // 現在満たされているモーションの数をカウント
        int currentCount = 0;
        
        // 各モーションをチェックし、一度でも達成したものはカウント
        if (useMotion1 && (motionStates[(int)requiredMotion1] || motionAchievedCounter[0]))
        {
            if (motionStates[(int)requiredMotion1])
            {
                motionAchievedCounter[0] = true; // 一度達成したことを記録
            }
            currentCount++;
        }
        
        if (useMotion2 && (motionStates[(int)requiredMotion2] || motionAchievedCounter[1]))
        {
            if (motionStates[(int)requiredMotion2])
            {
                motionAchievedCounter[1] = true; // 一度達成したことを記録
            }
            currentCount++;
        }
        
        if (useMotion3 && (motionStates[(int)requiredMotion3] || motionAchievedCounter[2]))
        {
            if (motionStates[(int)requiredMotion3])
            {
                motionAchievedCounter[2] = true; // 一度達成したことを記録
            }
            currentCount++;
        }
        
        // カウンターが更新された場合
        if (currentCount > motionCounter)
        {
            motionCounter = currentCount;
            if (showDebugInfo)
            {
                Debug.Log($"[DoorGimmick] カウンター更新: {motionCounter}/{requiredCount}");
            }
        }
        
        return motionCounter >= requiredCount;
    }
    
    private void UpdateDoorAnimation()
    {
        if (isOpening)
        {
            currentAngle += openSpeed * Time.deltaTime;
            if (currentAngle >= openAngle)
            {
                currentAngle = openAngle;
                isOpening = false;
                isDoorOpen = true;
            }
            UpdateDoorRotation();
        }
        else if (isClosing)
        {
            currentAngle -= openSpeed * Time.deltaTime;
            if (currentAngle <= 0f)
            {
                currentAngle = 0f;
                isClosing = false;
                isDoorOpen = false;
            }
            UpdateDoorRotation();
        }
    }
    
    private void UpdateDoorRotation()
    {
        if (leftDoor != null)
        {
            leftDoor.transform.localRotation = Quaternion.Euler(0, -currentAngle, 0);
        }
        if (rightDoor != null)
        {
            rightDoor.transform.localRotation = Quaternion.Euler(0, currentAngle, 0);
        }
    }
    
    public void OpenDoor()
    {
        if (!isOpening && !isDoorOpen)
        {
            isOpening = true;
            if (showDebugInfo)
            {
                Debug.Log("[DoorGimmick] ドアを開いています");
            }
        }
    }
    
    public void CloseDoor()
    {
        if (!isClosing && isDoorOpen)
        {
            isClosing = true;
            if (showDebugInfo)
            {
                Debug.Log("[DoorGimmick] ドアを閉じています");
            }
        }
    }
    
    public void CloseDoorImmediate()
    {
        currentAngle = 0f;
        isOpening = false;
        isClosing = false;
        isDoorOpen = false;
        hasBeenOpened = false; // ワンショットモードをリセット
        autoCloseTimer = 0f;
        
        // モーション要求モードのリセット
        ResetMotionRequirements();
        
        UpdateDoorRotation();
    }
    
    public void OpenDoorImmediate()
    {
        currentAngle = openAngle;
        isOpening = false;
        isClosing = false;
        isDoorOpen = true;
        hasBeenOpened = true; // ワンショットモードで開いた状態にする
        autoCloseTimer = 0f;
        UpdateDoorRotation();
    }
    
    // ワンショットモードをリセット（手動でドアを閉じる場合）
    public void ResetOneShotMode()
    {
        hasBeenOpened = false;
        if (showDebugInfo)
        {
            Debug.Log("[DoorGimmick] ワンショットモードをリセットしました");
        }
    }
    
    // モーション要求をリセット
    public void ResetMotionRequirements()
    {
        // 順次モードのリセット
        currentStep = 0;
        for (int i = 0; i < stepCompleted.Length; i++)
        {
            stepCompleted[i] = false;
            motionAchieved[i] = false;
        }
        
        // カウンターモードのリセット
        motionCounter = 0;
        for (int i = 0; i < motionAchievedCounter.Length; i++)
        {
            motionAchievedCounter[i] = false;
        }
        
        if (showDebugInfo)
        {
            Debug.Log("[DoorGimmick] モーション要求をリセットしました");
        }
    }
    
    // 外部からモーション状態を設定するメソッド（各モーション検出器から呼び出される）
    public void SetMotionState(MotionType motionType, bool state)
    {
        motionStates[(int)motionType] = state;
        if (showDebugInfo)
        {
            Debug.Log($"[DoorGimmick] {motionType}: {state}");
        }
    }
    
    // 現在の要求状態を取得
    public bool AreRequirementsMet()
    {
        if (useMotion1 && !motionStates[(int)requiredMotion1])
        {
            return false;
        }
        
        if (useMotion2 && !motionStates[(int)requiredMotion2])
        {
            return false;
        }
        
        if (useMotion3 && !motionStates[(int)requiredMotion3])
        {
            return false;
        }
        
        return true;
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
}