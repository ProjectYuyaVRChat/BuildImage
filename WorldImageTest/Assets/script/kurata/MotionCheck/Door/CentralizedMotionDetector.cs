using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class CentralizedMotionDetector : UdonSharpBehaviour
{
    [Header("管理するドアシステム")]
    [SerializeField] private DoorGimmickSystemNew[] doorSystems;
    [SerializeField] private int maxDoorSystems = 10;
    
    [Header("範囲検知システム")]
    [SerializeField] private DoorAreaTrigger[] areaTriggers;
    [SerializeField] private int maxAreaTriggers = 5;
    
    [Header("デバッグ")]
    [SerializeField] private bool showDebugInfo = true;
    
    // モーション状態管理
    private bool[] motionStates = new bool[19]; // MotionTypeの数
    
    // 各ドアシステムのアクティブ状態
    private bool[] doorSystemActiveStates;
    
    void Start()
    {
        InitializeSystem();
    }
    
    void Update()
    {
        UpdateMotionDetection();
    }
    
    private void InitializeSystem()
    {
        // ドアシステムのアクティブ状態配列を初期化
        doorSystemActiveStates = new bool[maxDoorSystems];
        
        if (showDebugInfo)
        {
            Debug.Log("[CentralizedMotionDetector] システムを初期化しました");
            Debug.Log($"  管理するドアシステム数: {(doorSystems != null ? doorSystems.Length : 0)}");
            Debug.Log($"  管理するエリアトリガー数: {(areaTriggers != null ? areaTriggers.Length : 0)}");
        }
    }
    
    private void UpdateMotionDetection()
    {
        // 各ドアシステムのアクティブ状態を更新
        UpdateDoorSystemStates();
        
        // アクティブなドアシステムにのみモーション状態を送信
        for (int i = 0; i < doorSystems.Length && i < maxDoorSystems; i++)
        {
            if (doorSystems[i] != null && doorSystemActiveStates[i])
            {
                SendMotionStatesToDoor(doorSystems[i]);
            }
        }
    }
    
    private void UpdateDoorSystemStates()
    {
        for (int i = 0; i < doorSystems.Length && i < maxDoorSystems; i++)
        {
            if (doorSystems[i] != null)
            {
                bool shouldBeActive = ShouldDoorSystemBeActive(i);
                bool wasActive = doorSystemActiveStates[i];
                doorSystemActiveStates[i] = shouldBeActive;
                
                // 状態が変化した時にデバッグ情報を表示
                if (showDebugInfo && wasActive != shouldBeActive)
                {
                    Debug.Log($"[CentralizedMotionDetector] ドア{i + 1} ({doorSystems[i].name}) の状態が変化: {(shouldBeActive ? "アクティブ" : "非アクティブ")}");
                }
            }
        }
    }
    
    private bool ShouldDoorSystemBeActive(int doorIndex)
    {
        if (doorSystems[doorIndex] == null) return false;
        
        // 範囲検知システムを使用しない場合は常にアクティブ
        if (!doorSystems[doorIndex].useAreaTrigger)
        {
            if (showDebugInfo)
            {
                Debug.Log($"[CentralizedMotionDetector] ドア{doorIndex + 1} ({doorSystems[doorIndex].name}): 範囲検知システムが無効のため常にアクティブ");
            }
            return true;
        }
        
        // 対応するエリアトリガーをチェック
        DoorAreaTrigger targetAreaTrigger = doorSystems[doorIndex].areaTrigger;
        if (targetAreaTrigger != null)
        {
            bool isActive = targetAreaTrigger.IsAreaActive;
            if (showDebugInfo)
            {
                Debug.Log($"[CentralizedMotionDetector] ドア{doorIndex + 1} ({doorSystems[doorIndex].name}): エリアトリガー {targetAreaTrigger.AreaName} の状態 = {(isActive ? "アクティブ" : "非アクティブ")}");
            }
            return isActive;
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"[CentralizedMotionDetector] ドア{doorIndex + 1} ({doorSystems[doorIndex].name}): エリアトリガーが設定されていないため非アクティブ");
        }
        return false;
    }
    
    private void SendMotionStatesToDoor(DoorGimmickSystemNew doorSystem)
    {
        // 各モーション状態をドアシステムに送信
        bool hasActiveMotion = false;
        for (int i = 0; i < motionStates.Length; i++)
        {
            if (motionStates[i])
            {
                doorSystem.SetMotionState((MotionType)i, true);
                hasActiveMotion = true;
            }
        }
        
        if (showDebugInfo && hasActiveMotion)
        {
            Debug.Log($"[CentralizedMotionDetector] ドアシステムにモーション状態を送信しました: {doorSystem.name}");
        }
        else if (showDebugInfo && Time.frameCount % 300 == 0) // 5秒ごと
        {
            Debug.Log($"[CentralizedMotionDetector] アクティブなドアシステム: {doorSystem.name} (アクティブなモーション: {hasActiveMotion})");
        }
    }
    
    // 外部からモーション状態を設定するメソッド
    public void SetMotionState(MotionType motionType, bool state)
    {
        motionStates[(int)motionType] = state;
        
        if (showDebugInfo && state)
        {
            Debug.Log($"[CentralizedMotionDetector] {GetMotionName(motionType)}: {state}");
        }
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
    
    // デバッグ用：現在の状態を表示
    public void DebugCurrentState()
    {
        if (!showDebugInfo) return;
        
        Debug.Log("[CentralizedMotionDetector] 現在の状態:");
        for (int i = 0; i < doorSystems.Length && i < maxDoorSystems; i++)
        {
            if (doorSystems[i] != null)
            {
                Debug.Log($"  ドア{i + 1}: {(doorSystemActiveStates[i] ? "アクティブ" : "非アクティブ")}");
            }
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

    // 全MotionDetectorBaseにキャリブレーションを実行
    public void CalibrateAllDetectors()
    {
        int calibratedCount = 0;
        
        // 管理している全てのMotionDetectorBaseにキャリブレーションを実行
        foreach (var doorSystem in doorSystems)
        {
            if (doorSystem != null)
            {
                var detectors = doorSystem.GetComponentsInChildren<MotionDetectorBase>();
                foreach (var detector in detectors)
                {
                    if (detector != null)
                    {
                        detector.Calibrate();
                        calibratedCount++;
                    }
                }
            }
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"[CentralizedMotionDetector] {calibratedCount}個のMotionDetectorBaseにキャリブレーションを実行しました");
        }
    }
} 