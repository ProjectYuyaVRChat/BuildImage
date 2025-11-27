using UdonSharp;
using UnityEngine;

public class SequentialMotionHandler : UdonSharpBehaviour
{
    [Header("順次モード設定")]
    [SerializeField] private float stepTimeout = 10f;
    [SerializeField] private float stepCooldown = 1f;
    [Tooltip("モーションの安定化時間（秒）。短くすると瞬間的なモーション（Jump等）も検出しやすくなります")]
    [SerializeField] private float motionStabilizeTime = 0.1f;
    [SerializeField] private float wrongMotionResetTime = 2f;
    [SerializeField] private bool resetOnWrongMotion = true;
    [SerializeField] private bool showDebugInfo = true;

    [Header("イベント通知先(UdonSharpBehaviour)")]
    [SerializeField] private UdonSharpBehaviour eventReceiver;
    [SerializeField] private string onAllStepsCompletedEvent = "OnAllStepsCompleted";
    [SerializeField] private string onStepCompletedEvent = "OnStepCompleted";
    [SerializeField] private string onResetEvent = "OnReset";
    
    // 内部状態
    private int currentStep = 0;
    private bool[] stepCompleted = new bool[3];
    private bool[] motionAchieved = new bool[3];
    private float stepTimer = 0f;
    private float cooldownTimer = 0f;
    private bool isInCooldown = false;
    private float[] motionStabilizeTimers = new float[3];
    private float wrongMotionTimer = 0f;
    private bool hasWrongMotion = false;
    
    // 外部データ
    private bool[] motionStates = new bool[19];
    private MotionType[] requiredMotions = new MotionType[3];
    private bool[] useMotions = new bool[3];
    
    public void Initialize(bool[] states, MotionType[] motions, bool[] uses)
    {
        motionStates = states;
        requiredMotions = motions;
        useMotions = uses;
        ResetSequentialMode();
    }
    
    public bool CheckRequirements()
    {
        // すべてのステップが完了している場合は、間違った動作のチェックを停止
        if (stepCompleted[0] && stepCompleted[1] && stepCompleted[2])
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
                    Debug.Log("[SequentialMotion] クールダウン終了");
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
                Debug.Log("[SequentialMotion] ステップタイムアウト - リセット");
            }
            return false;
        }
        
        // 間違った動作のチェック
        if (resetOnWrongMotion)
        {
            CheckWrongMotions();
        }
        
        // 現在のステップのモーションをチェック
        bool currentMotionActive = false;
        MotionType currentRequiredMotion = MotionType.Jump;
        
        switch (currentStep)
        {
            case 0:
                if (useMotions[0])
                {
                    currentRequiredMotion = requiredMotions[0];
                    currentMotionActive = motionStates[(int)requiredMotions[0]];
                }
                break;
                
            case 1:
                if (useMotions[1])
                {
                    currentRequiredMotion = requiredMotions[1];
                    currentMotionActive = motionStates[(int)requiredMotions[1]];
                }
                break;
                
            case 2:
                if (useMotions[2])
                {
                    currentRequiredMotion = requiredMotions[2];
                    currentMotionActive = motionStates[(int)requiredMotions[2]];
                }
                break;
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
        
        // すべてのステップが完了しているかチェック
        return stepCompleted[0] && stepCompleted[1] && stepCompleted[2];
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
        if (currentStep != 0 && useMotions[0] && motionStates[(int)requiredMotions[0]])
        {
            wrongMotionDetected = true;
            wrongMotionType = requiredMotions[0];
        }
        // ステップ1以外でMotion2が使用されていて、Motion2がアクティブな場合
        if (currentStep != 1 && useMotions[1] && motionStates[(int)requiredMotions[1]])
        {
            wrongMotionDetected = true;
            wrongMotionType = requiredMotions[1];
        }
        // ステップ2以外でMotion3が使用されていて、Motion3がアクティブな場合
        if (currentStep != 2 && useMotions[2] && motionStates[(int)requiredMotions[2]])
        {
            wrongMotionDetected = true;
            wrongMotionType = requiredMotions[2];
        }
        
        if (wrongMotionDetected)
        {
            if (!hasWrongMotion)
            {
                hasWrongMotion = true;
                wrongMotionTimer = 0f;
                if (showDebugInfo)
                {
                    Debug.Log($"[SequentialMotion] 間違った動作を検出: {wrongMotionType} (現在のステップ: {currentStep + 1})");
                }
            }
            
            wrongMotionTimer += Time.deltaTime;
            
            if (wrongMotionTimer >= wrongMotionResetTime)
            {
                ResetSequentialMode();
                if (showDebugInfo)
                {
                    Debug.Log($"[SequentialMotion] 間違った動作でリセット: {wrongMotionType}");
                }
            }
        }
        else
        {
            if (hasWrongMotion)
            {
                hasWrongMotion = false;
                wrongMotionTimer = 0f;
            }
        }
    }
    
    private void CompleteCurrentStep(MotionType completedMotion)
    {
        stepCompleted[currentStep] = true;
        motionAchieved[currentStep] = true;
        
        if (showDebugInfo)
        {
            Debug.Log($"[SequentialMotion] ステップ{currentStep + 1}完了: {completedMotion}");
        }
        
        // イベント通知
        if (eventReceiver != null && !string.IsNullOrEmpty(onStepCompletedEvent))
        {
            eventReceiver.SendCustomEvent(onStepCompletedEvent);
        }
        
        // 次のステップに進む
        currentStep++;
        
        // すべてのステップが完了した場合
        if (currentStep >= 3)
        {
            if (showDebugInfo)
            {
                Debug.Log("[SequentialMotion] すべてのステップが完了しました！");
            }
            // イベント通知
            if (eventReceiver != null && !string.IsNullOrEmpty(onAllStepsCompletedEvent))
            {
                eventReceiver.SendCustomEvent(onAllStepsCompletedEvent);
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
                Debug.Log($"[SequentialMotion] ステップ{currentStep + 1}に進みます（クールダウン開始）");
            }
        }
    }
    
    public void ResetSequentialMode()
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
            Debug.Log("[SequentialMotion] 順次モードをリセットしました");
        }
        // イベント通知
        if (eventReceiver != null && !string.IsNullOrEmpty(onResetEvent))
        {
            eventReceiver.SendCustomEvent(onResetEvent);
        }
    }
    
    // 外部からアクセス可能なプロパティ
    public int CurrentStep => currentStep;
    public bool[] StepCompleted => stepCompleted;
    public bool IsInCooldown => isInCooldown;
    public float CooldownTimer => cooldownTimer;
    public bool HasWrongMotion => hasWrongMotion;
    public float WrongMotionTimer => wrongMotionTimer;
    public bool[] MotionAchieved => motionAchieved;
} 