using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

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
    
    [Header("UI表示")]
    [Tooltip("動作状況を表示するTextMeshPro")]
    [SerializeField] private TMPro.TextMeshProUGUI statusText;
    [Tooltip("UI表示を有効にするかどうか")]
    [SerializeField] private bool showUIStatus = true;
    [Tooltip("UI表示の更新間隔（秒）")]
    [SerializeField] private float uiUpdateInterval = 0.1f;
    

    [Header("順次モード設定")]
    [Tooltip("各ステップの制限時間（秒）。この時間内に次のステップを完了しないとリセットされます")]
    [SerializeField] private float stepTimeout = 10f; // 各ステップの制限時間
    [Tooltip("ステップ完了後のクールダウン時間（秒）。この時間中は次のステップに進めません")]
    [SerializeField] private float stepCooldown = 1f; // ステップ完了後のクールダウン時間
    [Tooltip("モーションの安定化時間（秒）。この時間継続してからステップ完了とします")]
    [SerializeField] private float motionStabilizeTime = 0.5f; // モーションの安定化時間
    [Tooltip("間違った動作をした場合のリセット時間（秒）。この時間内に間違った動作をするとリセットされます")]
    [SerializeField] private float wrongMotionResetTime = 2f; // 間違った動作のリセット時間
    [Tooltip("間違った動作をした場合にリセットするかどうか")]
    [SerializeField] private bool resetOnWrongMotion = true; // 間違った動作でリセットするかどうか
    [Tooltip("ドアが開いた後に順次モードをリセットするかどうか")]
    [SerializeField] private bool resetSequentialAfterOpen = false; // ドアが開いた後に順次モードをリセットするかどうか
    
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
    private float stepTimer = 0f; // ステップのタイマー
    private float cooldownTimer = 0f; // クールダウンのタイマー
    private bool isInCooldown = false; // クールダウン中かどうか
    private float[] motionStabilizeTimers = new float[3]; // 各モーションの安定化タイマー
    private float wrongMotionTimer = 0f; // 間違った動作のタイマー
    private bool hasWrongMotion = false; // 間違った動作があったかどうか
    
    // UI表示用
    private float uiUpdateTimer = 0f; // UI更新タイマー
    private string currentStatusMessage = ""; // 現在のステータスメッセージ
    
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
        
        // 初期UIメッセージを設定
        UpdateStatusMessage();
    }
    
    void Update()
    {
        CheckDoorRequirements();
        UpdateDoorAnimation();
        UpdateUI();
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
        stepTimer = 0f;
        cooldownTimer = 0f;
        isInCooldown = false;
        wrongMotionTimer = 0f;
        hasWrongMotion = false;
        
        for (int i = 0; i < stepCompleted.Length; i++)
        {
            stepCompleted[i] = false;
            motionAchieved[i] = false;
            motionStabilizeTimers[i] = 0f;
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
        // 使用されているモーションのみで完了チェックを行う
        bool allUsedStepsCompleted = true;
        if (useMotion1 && !stepCompleted[0]) allUsedStepsCompleted = false;
        if (useMotion2 && !stepCompleted[1]) allUsedStepsCompleted = false;
        if (useMotion3 && !stepCompleted[2]) allUsedStepsCompleted = false;
        
        // すべての使用されているステップが完了している場合は、間違った動作のチェックを停止
        if (allUsedStepsCompleted)
        {
            return true;
        }
        
        // クールダウン中は何もしない
        if (isInCooldown)
        {
            cooldownTimer += Time.deltaTime;
            if (cooldownTimer >= stepCooldown)
            {
                isInCooldown = false;
                cooldownTimer = 0f;
                if (showDebugInfo)
                {
                    Debug.Log("[DoorGimmick] クールダウン終了");
                }
            }
            return false;
        }
        
        // ステップタイマーを更新
        stepTimer += Time.deltaTime;
        
        // タイムアウトチェック
        if (stepTimer >= stepTimeout)
        {
            ResetSequentialMode();
            if (showDebugInfo)
            {
                Debug.Log("[DoorGimmick] ステップタイムアウト - リセット");
            }
            return false;
        }
        
        // 間違った動作のチェック
        if (resetOnWrongMotion)
        {
            CheckWrongMotions();
        }
        
        // 使用されていないモーションのステップをスキップ
        while (currentStep < 3)
        {
            bool stepEnabled = false;
            switch (currentStep)
            {
                case 0: stepEnabled = useMotion1; break;
                case 1: stepEnabled = useMotion2; break;
                case 2: stepEnabled = useMotion3; break;
            }
            
            if (stepEnabled) break; // 使用されているステップが見つかった
            
            // 使用されていないステップは自動的に完了扱いにする
            stepCompleted[currentStep] = true;
            if (showDebugInfo)
            {
                Debug.Log($"[DoorGimmick] ステップ{currentStep + 1}は使用されていないためスキップします");
            }
            currentStep++;
            
            // すべてのステップをチェック済みの場合
            if (currentStep >= 3)
            {
                break;
            }
        }
        
        // すべてのステップをチェック済みの場合、完了を返す
        if (currentStep >= 3)
        {
            // 最初に定義した変数を使用（再計算）
            allUsedStepsCompleted = true;
            if (useMotion1 && !stepCompleted[0]) allUsedStepsCompleted = false;
            if (useMotion2 && !stepCompleted[1]) allUsedStepsCompleted = false;
            if (useMotion3 && !stepCompleted[2]) allUsedStepsCompleted = false;
            return allUsedStepsCompleted;
        }
        
        // 現在のステップのモーションをチェック
        bool currentMotionActive = false;
        MotionType currentRequiredMotion = MotionType.Jump;
        
        switch (currentStep)
        {
            case 0:
                if (useMotion1)
                {
                    currentRequiredMotion = requiredMotion1;
                    currentMotionActive = motionStates[(int)requiredMotion1];
                }
                break;
                
            case 1:
                if (useMotion2)
                {
                    currentRequiredMotion = requiredMotion2;
                    currentMotionActive = motionStates[(int)requiredMotion2];
                }
                break;
                
            case 2:
                if (useMotion3)
                {
                    currentRequiredMotion = requiredMotion3;
                    currentMotionActive = motionStates[(int)requiredMotion3];
                }
                break;
        }
        
        // デバッグ情報: 現在のステップと期待されるモーション
        if (showDebugInfo && currentStep < 3)
        {
            Debug.Log($"[DoorGimmick] ステップ{currentStep + 1}: 期待={currentRequiredMotion}, アクティブ={currentMotionActive}");
        }
        
        // 現在のモーションがアクティブな場合、安定化タイマーを更新
        if (currentMotionActive)
        {
            motionStabilizeTimers[currentStep] += Time.deltaTime;
            
            // 安定化時間を満たした場合、ステップ完了
            if (motionStabilizeTimers[currentStep] >= motionStabilizeTime)
            {
                CompleteCurrentStep(currentRequiredMotion);
            }
        }
        else
        {
            // モーションが非アクティブになった場合、安定化タイマーをリセット
            motionStabilizeTimers[currentStep] = 0f;
        }
        
        // 使用されているモーションのみで完了チェックを行う（最初に定義した変数を再計算）
        allUsedStepsCompleted = true;
        if (useMotion1 && !stepCompleted[0]) allUsedStepsCompleted = false;
        if (useMotion2 && !stepCompleted[1]) allUsedStepsCompleted = false;
        if (useMotion3 && !stepCompleted[2]) allUsedStepsCompleted = false;
        
        return allUsedStepsCompleted;
    }
    
    private void CheckWrongMotions()
    {
        // すべてのステップが完了している場合は、間違った動作のチェックを停止
        if (stepCompleted[0] && stepCompleted[1] && stepCompleted[2])
        {
            return;
        }
        
        // 現在のステップ以外のモーションがアクティブになった場合
        bool wrongMotionDetected = false;
        MotionType wrongMotionType = MotionType.Jump;
        
        // ステップ0以外でMotion1が使用されていて、Motion1がアクティブな場合
        if (currentStep != 0 && useMotion1 && motionStates[(int)requiredMotion1])
        {
            wrongMotionDetected = true;
            wrongMotionType = requiredMotion1;
        }
        // ステップ1以外でMotion2が使用されていて、Motion2がアクティブな場合
        if (currentStep != 1 && useMotion2 && motionStates[(int)requiredMotion2])
        {
            wrongMotionDetected = true;
            wrongMotionType = requiredMotion2;
        }
        // ステップ2以外でMotion3が使用されていて、Motion3がアクティブな場合
        if (currentStep != 2 && useMotion3 && motionStates[(int)requiredMotion3])
        {
            wrongMotionDetected = true;
            wrongMotionType = requiredMotion3;
        }
        
        if (wrongMotionDetected)
        {
            if (!hasWrongMotion)
            {
                hasWrongMotion = true;
                wrongMotionTimer = 0f;
                if (showDebugInfo)
                {
                    Debug.Log($"[DoorGimmick] 間違った動作を検出: {wrongMotionType} (現在のステップ: {currentStep + 1})");
                    Debug.Log($"[DoorGimmick] 設定: Motion1={requiredMotion1}(使用:{useMotion1}), Motion2={requiredMotion2}(使用:{useMotion2}), Motion3={requiredMotion3}(使用:{useMotion3})");
                }
                
                // UIメッセージを更新
                UpdateStatusMessage();
            }
            
            wrongMotionTimer += Time.deltaTime;
            
            if (wrongMotionTimer >= wrongMotionResetTime)
            {
                ResetSequentialMode();
                if (showDebugInfo)
                {
                    Debug.Log($"[DoorGimmick] 間違った動作でリセット: {wrongMotionType}");
                }
            }
        }
        else
        {
            if (hasWrongMotion)
            {
                hasWrongMotion = false;
                wrongMotionTimer = 0f;
                
                // UIメッセージを更新
                UpdateStatusMessage();
            }
        }
    }
    
    private void CompleteCurrentStep(MotionType completedMotion)
    {
        stepCompleted[currentStep] = true;
        motionAchieved[currentStep] = true;
        
        if (showDebugInfo)
        {
            Debug.Log($"[DoorGimmick] ステップ{currentStep + 1}完了: {completedMotion}");
        }
        
        // 次のステップに進む
        currentStep++;
        
        // 使用されていないモーションのステップをスキップ
        while (currentStep < 3)
        {
            bool stepEnabled = false;
            switch (currentStep)
            {
                case 0: stepEnabled = useMotion1; break;
                case 1: stepEnabled = useMotion2; break;
                case 2: stepEnabled = useMotion3; break;
            }
            
            if (stepEnabled) break; // 使用されているステップが見つかった
            
            // 使用されていないステップは自動的に完了扱いにする
            stepCompleted[currentStep] = true;
            if (showDebugInfo)
            {
                Debug.Log($"[DoorGimmick] ステップ{currentStep + 1}は使用されていないためスキップします");
            }
            currentStep++;
        }
        
        // すべてのステップが完了した場合
        if (currentStep >= 3)
        {
            if (showDebugInfo)
            {
                Debug.Log("[DoorGimmick] すべてのステップが完了しました！");
            }
        }
        else
        {
            // クールダウンを開始
            isInCooldown = true;
            cooldownTimer = 0f;
            
            // ステップタイマーをリセット
            stepTimer = 0f;
            
            if (showDebugInfo)
            {
                Debug.Log($"[DoorGimmick] ステップ{currentStep + 1}に進みます（クールダウン開始）");
                
                // 次のステップの設定を確認
                MotionType nextMotion = MotionType.Jump;
                bool nextMotionEnabled = false;
                
                switch (currentStep)
                {
                    case 0:
                        nextMotion = requiredMotion1;
                        nextMotionEnabled = useMotion1;
                        break;
                    case 1:
                        nextMotion = requiredMotion2;
                        nextMotionEnabled = useMotion2;
                        break;
                    case 2:
                        nextMotion = requiredMotion3;
                        nextMotionEnabled = useMotion3;
                        break;
                }
                
                Debug.Log($"[DoorGimmick] 次のステップ{currentStep + 1}: {nextMotion} (使用: {nextMotionEnabled})");
            }
        }
        
        // UIメッセージを更新
        UpdateStatusMessage();
    }
    
    private void ResetSequentialMode()
    {
        currentStep = 0;
        stepTimer = 0f;
        cooldownTimer = 0f;
        isInCooldown = false;
        wrongMotionTimer = 0f;
        hasWrongMotion = false;
        
        for (int i = 0; i < stepCompleted.Length; i++)
        {
            stepCompleted[i] = false;
            motionStabilizeTimers[i] = 0f;
        }
        
        if (showDebugInfo)
        {
            Debug.Log("[DoorGimmick] 順次モードをリセットしました");
        }
        
        // UIメッセージを更新
        UpdateStatusMessage();
    }
    
    private void UpdateUI()
    {
        if (!showUIStatus || statusText == null)
        {
            return;
        }
        
        uiUpdateTimer += Time.deltaTime;
        if (uiUpdateTimer >= uiUpdateInterval)
        {
            UpdateStatusMessage();
            uiUpdateTimer = 0f;
        }
    }
    
    private void UpdateStatusMessage()
    {
        if (!showUIStatus || statusText == null)
        {
            return;
        }
        
        string message = "";
        
        switch (motionMode)
        {
            case MOTION_MODE_SIMULTANEOUS:
                message = GetSimultaneousStatusMessage();
                break;
                
            case MOTION_MODE_SEQUENTIAL:
                message = GetSequentialStatusMessage();
                break;
                
            case MOTION_MODE_COUNTER:
                message = GetCounterStatusMessage();
                break;
        }
        
        if (message != currentStatusMessage)
        {
            statusText.text = message;
            currentStatusMessage = message;
        }
    }
    
    private string GetSimultaneousStatusMessage()
    {
        string message = "同時条件モード\n";
        
        if (useMotion1)
        {
            bool isActive = motionStates[(int)requiredMotion1];
            string status = isActive ? "✓" : "✗";
            message += $"{GetMotionName(requiredMotion1)}: {status}\n";
        }
        
        if (useMotion2)
        {
            bool isActive = motionStates[(int)requiredMotion2];
            string status = isActive ? "✓" : "✗";
            message += $"{GetMotionName(requiredMotion2)}: {status}\n";
        }
        
        if (useMotion3)
        {
            bool isActive = motionStates[(int)requiredMotion3];
            string status = isActive ? "✓" : "✗";
            message += $"{GetMotionName(requiredMotion3)}: {status}\n";
        }
        
        if (isDoorOpen)
        {
            message += "\n🎉 ドアが開きました！";
        }
        else if (isOpening)
        {
            message += "\n🚪 ドアを開いています...";
        }
        
        return message;
    }
    
    private string GetSequentialStatusMessage()
    {
        string message = "順次条件モード\n";
        
        // 各ステップの状況を表示
        for (int i = 0; i < 3; i++)
        {
            string stepStatus = "";
            MotionType stepMotion = MotionType.Jump;
            bool stepEnabled = false;
            
            switch (i)
            {
                case 0:
                    stepMotion = requiredMotion1;
                    stepEnabled = useMotion1;
                    break;
                case 1:
                    stepMotion = requiredMotion2;
                    stepEnabled = useMotion2;
                    break;
                case 2:
                    stepMotion = requiredMotion3;
                    stepEnabled = useMotion3;
                    break;
            }
            
            if (stepEnabled)
            {
                if (stepCompleted[i])
                {
                    stepStatus = "✓ 完了";
                }
                else if (i == currentStep && !isInCooldown)
                {
                    bool isActive = motionStates[(int)stepMotion];
                    stepStatus = isActive ? "🔄 実行中..." : "⏳ 待機中";
                }
                else if (i < currentStep)
                {
                    stepStatus = "✓ 完了";
                }
                else
                {
                    stepStatus = "⏸️ 待機";
                }
                
                message += $"{GetMotionName(stepMotion)}: {stepStatus}\n";
            }
        }
        
        // 現在の状況を表示
        if (isInCooldown)
        {
            message += $"\n⏰ クールダウン中... ({cooldownTimer:F1}s)";
        }
        else if (hasWrongMotion)
        {
            message += $"\n⚠️ 間違った動作！ ({wrongMotionTimer:F1}s)";
        }
        else if (stepCompleted[0] && stepCompleted[1] && stepCompleted[2])
        {
            message += "\n🎉 すべてのステップが完了しました！";
        }
        else if (currentStep < 3)
        {
            // 現在のステップの名前を表示
            MotionType currentMotion = MotionType.Jump;
            switch (currentStep)
            {
                case 0:
                    currentMotion = requiredMotion1;
                    break;
                case 1:
                    currentMotion = requiredMotion2;
                    break;
                case 2:
                    currentMotion = requiredMotion3;
                    break;
            }
            message += $"\n📋 {GetMotionName(currentMotion)} を実行中";
        }
        
        // ドアの状況
        if (isDoorOpen)
        {
            message += "\n🚪 ドアが開いています";
        }
        else if (isOpening)
        {
            message += "\n🚪 ドアを開いています...";
        }
        
        return message;
    }
    
    private string GetCounterStatusMessage()
    {
        string message = "カウンターモード\n";
        
        if (useMotion1)
        {
            bool isActive = motionStates[(int)requiredMotion1];
            bool isAchieved = motionAchievedCounter[0];
            string status = isAchieved ? "✓" : (isActive ? "🔄" : "✗");
            message += $"{GetMotionName(requiredMotion1)}: {status}\n";
        }
        
        if (useMotion2)
        {
            bool isActive = motionStates[(int)requiredMotion2];
            bool isAchieved = motionAchievedCounter[1];
            string status = isAchieved ? "✓" : (isActive ? "🔄" : "✗");
            message += $"{GetMotionName(requiredMotion2)}: {status}\n";
        }
        
        if (useMotion3)
        {
            bool isActive = motionStates[(int)requiredMotion3];
            bool isAchieved = motionAchievedCounter[2];
            string status = isAchieved ? "✓" : (isActive ? "🔄" : "✗");
            message += $"{GetMotionName(requiredMotion3)}: {status}\n";
        }
        
        message += $"\n進捗: {motionCounter}/{requiredCount}";
        
        if (isDoorOpen)
        {
            message += "\n🎉 ドアが開きました！";
        }
        else if (isOpening)
        {
            message += "\n🚪 ドアを開いています...";
        }
        
        return message;
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
            
            // 順次モードでドアが開いた場合、リセットするかどうかチェック
            if (motionMode == MOTION_MODE_SEQUENTIAL && resetSequentialAfterOpen)
            {
                ResetSequentialMode();
                if (showDebugInfo)
                {
                    Debug.Log("[DoorGimmick] ドアが開いたため順次モードをリセットしました");
                }
            }
            
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
    
    // 順次モードを手動でリセット
    public void ResetSequentialModeManual()
    {
        ResetSequentialMode();
    }
    
    // モーション要求をリセット
    public void ResetMotionRequirements()
    {
        // すべてのモーション状態をリセット
        for (int i = 0; i < motionStates.Length; i++)
        {
            motionStates[i] = false;
        }
        
        // 順次モードのリセット
        ResetSequentialMode();
        
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