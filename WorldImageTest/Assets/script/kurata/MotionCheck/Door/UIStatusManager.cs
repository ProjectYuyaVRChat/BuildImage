using UdonSharp;
using UnityEngine;
using TMPro;

public class UIStatusManager : UdonSharpBehaviour
{
    [Header("UI設定")]
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private bool showUIStatus = true;
    [SerializeField] private float uiUpdateInterval = 0.1f;

    // 内部状態
    private float uiUpdateTimer = 0f;
    private string currentStatusMessage = "";

    // 外部から設定されるデータ
    private int motionMode = 0;
    private bool[] motionStates = new bool[19];
    private MotionType[] requiredMotions = new MotionType[3];
    private bool[] useMotions = new bool[3];
    private bool isDoorOpen = false;
    private bool isOpening = false;
    private int currentStep = 0;
    private bool[] stepCompleted = new bool[3];
    private bool isInCooldown = false;
    private float cooldownTimer = 0f;
    private bool hasWrongMotion = false;
    private float wrongMotionTimer = 0f;
    private int motionCounter = 0;
    private int requiredCount = 0;
    private bool[] motionAchievedCounter = new bool[3];

    void Update()
    {
        UpdateUI();
    }

    public void SetData(int mode, bool[] states, MotionType[] motions, bool[] uses, 
                       bool doorOpen, bool opening, int step, bool[] completed, 
                       bool cooldown, float cooldownTime, bool wrongMotion, float wrongTime,
                       int counter, int required, bool[] achieved)
    {
        motionMode = mode;
        motionStates = states;
        requiredMotions = motions;
        useMotions = uses;
        isDoorOpen = doorOpen;
        isOpening = opening;
        currentStep = step;
        stepCompleted = completed;
        isInCooldown = cooldown;
        cooldownTimer = cooldownTime;
        hasWrongMotion = wrongMotion;
        wrongMotionTimer = wrongTime;
        motionCounter = counter;
        requiredCount = required;
        motionAchievedCounter = achieved;
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
            case 0: // Simultaneous
                message = GetSimultaneousStatusMessage();
                break;
                
            case 1: // Sequential
                message = GetSequentialStatusMessage();
                break;
                
            case 2: // Counter
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
        
        if (useMotions[0])
        {
            bool isActive = motionStates[(int)requiredMotions[0]];
            string status = isActive ? "○" : "×";
            message += $"{GetMotionName(requiredMotions[0])}: {status}\n";
        }
        
        if (useMotions[1])
        {
            bool isActive = motionStates[(int)requiredMotions[1]];
            string status = isActive ? "○" : "×";
            message += $"{GetMotionName(requiredMotions[1])}: {status}\n";
        }
        
        if (useMotions[2])
        {
            bool isActive = motionStates[(int)requiredMotions[2]];
            string status = isActive ? "○" : "×";
            message += $"{GetMotionName(requiredMotions[2])}: {status}\n";
        }
        
        if (isDoorOpen)
        {
            message += "\n ドアが開きました！";
        }
        else if (isOpening)
        {
            message += "\n ドアを開いています...";
        }
        
        return message;
    }

    private string GetSequentialStatusMessage()
    {
        string message = "順次条件モード\n";
        
        // 各ステップの状況を表示
        for (int i = 0; i < 3; i++)
        {
            if (useMotions[i])
            {
                string stepStatus = "";
                
                if (stepCompleted[i])
                {
                    stepStatus = "○ 完了";
                }
                else if (i == currentStep && !isInCooldown)
                {
                    bool isActive = motionStates[(int)requiredMotions[i]];
                    stepStatus = isActive ? "… 実行中..." : " 待機中";
                }
                else if (i < currentStep)
                {
                    stepStatus = "○ 完了";
                }
                else
                {
                    stepStatus = "⏸ 待機";
                }
                
                message += $"{GetMotionName(requiredMotions[i])}: {stepStatus}\n";
            }
        }
        
        // 現在の状況を表示
        if (isInCooldown)
        {
            message += $"\n クールダウン中... ({cooldownTimer:F1}s)";
        }
        else if (hasWrongMotion)
        {
            message += $"\n 間違った動作！ ({wrongMotionTimer:F1}s)";
        }
        else if (stepCompleted[0] && stepCompleted[1] && stepCompleted[2])
        {
            message += "\n すべてのステップが完了しました！";
        }
        else if (currentStep < 3)
        {
            MotionType currentMotion = requiredMotions[currentStep];
            message += $"\n {GetMotionName(currentMotion)} を実行中";
        }
        
        // ドアの状況
        if (isDoorOpen)
        {
            message += "\n ドアが開いています";
        }
        else if (isOpening)
        {
            message += "\n ドアを開いています...";
        }
        
        return message;
    }

    private string GetCounterStatusMessage()
    {
        string message = "カウンターモード\n";
        
        if (useMotions[0])
        {
            bool isActive = motionStates[(int)requiredMotions[0]];
            bool isAchieved = motionAchievedCounter[0];
            string status = isAchieved ? "○" : (isActive ? "…" : "×");
            message += $"{GetMotionName(requiredMotions[0])}: {status}\n";
        }
        
        if (useMotions[1])
        {
            bool isActive = motionStates[(int)requiredMotions[1]];
            bool isAchieved = motionAchievedCounter[1];
            string status = isAchieved ? "○" : (isActive ? "…" : "×");
            message += $"{GetMotionName(requiredMotions[1])}: {status}\n";
        }
        
        if (useMotions[2])
        {
            bool isActive = motionStates[(int)requiredMotions[2]];
            bool isAchieved = motionAchievedCounter[2];
            string status = isAchieved ? "○" : (isActive ? "…" : "×");
            message += $"{GetMotionName(requiredMotions[2])}: {status}\n";
        }
        
        message += $"\n進捗: {motionCounter}/{requiredCount}";
        
        if (isDoorOpen)
        {
            message += "\n ドアが開きました！";
        }
        else if (isOpening)
        {
            message += "\n ドアを開いています...";
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
} 