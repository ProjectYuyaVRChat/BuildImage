using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

/// <summary>
/// 特定のオブジェクトに衝突したらモーション検出器のキャリブレーションを実行するコンポーネント
/// </summary>
public class MotionCalibrationTrigger : UdonSharpBehaviour
{
    [Header("キャリブレーション対象")]
    [Tooltip("全ての検出器をキャリブレーションする場合はこちらを設定")]
    [SerializeField] private CentralizedMotionDetector centralizedMotionDetector;
    
    [Tooltip("個別の検出器をキャリブレーションする場合はこちらを設定")]
    [SerializeField] private MotionDetectorBase[] motionDetectors;
    
    [Header("衝突検出設定")]
    [Tooltip("特定のオブジェクトのみに反応する場合は設定（未設定の場合は全てのオブジェクトに反応）")]
    [SerializeField] private GameObject[] targetObjects;
    
    [Tooltip("プレイヤーのみに反応するか")]
    [SerializeField] private bool onlyReactToPlayers = false;
    
    [Header("キャリブレーション設定")]
    [Tooltip("キャリブレーション実行までの遅延時間（秒）")]
    [SerializeField] private float calibrationDelay = 0.0f;
    
    [Header("デバッグ")]
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] private string triggerName = "モーションキャリブレーショントリガー";
    
    void Start()
    {
        // Colliderの設定を確認
        CheckColliderSetup();
        
        if (showDebugInfo)
        {
            Debug.Log($"[MotionCalibrationTrigger] {triggerName} を初期化しました");
            Debug.Log($"  CentralizedMotionDetector: {(centralizedMotionDetector != null ? "設定済み" : "未設定")}");
            Debug.Log($"  MotionDetectors: {(motionDetectors != null ? motionDetectors.Length + "個" : "未設定")}");
            Debug.Log($"  対象オブジェクト: {(targetObjects != null && targetObjects.Length > 0 ? targetObjects.Length + "個" : "全て")}");
            Debug.Log($"  プレイヤーのみ: {onlyReactToPlayers}");
        }
    }
    
    /// <summary>
    /// Colliderの設定を確認
    /// </summary>
    private void CheckColliderSetup()
    {
        Collider triggerCollider = GetComponent<Collider>();
        if (triggerCollider == null)
        {
            Debug.LogError($"[MotionCalibrationTrigger] {triggerName}: Colliderが見つかりません。衝突検出ができません。");
        }
        else if (!triggerCollider.isTrigger)
        {
            Debug.LogWarning($"[MotionCalibrationTrigger] {triggerName}: ColliderがTriggerではありません。IsTriggerをtrueに設定してください。");
        }
    }
    
    /// <summary>
    /// 通常のオブジェクトとの衝突検出（OnTriggerEnter）
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        // プレイヤーのみに反応する設定の場合はスキップ
        if (onlyReactToPlayers)
        {
            return;
        }
        
        // 対象オブジェクトが指定されている場合はチェック
        if (targetObjects != null && targetObjects.Length > 0)
        {
            bool isTarget = false;
            foreach (var targetObj in targetObjects)
            {
                if (targetObj != null && other.gameObject == targetObj)
                {
                    isTarget = true;
                    break;
                }
            }
            
            if (!isTarget)
            {
                return; // 対象外のオブジェクト
            }
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"[MotionCalibrationTrigger] {triggerName}: {other.gameObject.name} が衝突しました");
        }
        
        // キャリブレーションを実行
        ExecuteCalibration();
    }
    
    /// <summary>
    /// プレイヤーとの衝突検出（VRChat専用）
    /// </summary>
    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        if (player == null) return;
        
        // ローカルプレイヤーのみに反応するかチェック（必要に応じて変更可能）
        if (!player.isLocal)
        {
            return;
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"[MotionCalibrationTrigger] {triggerName}: プレイヤー {player.displayName} がエリアに入りました");
        }
        
        // キャリブレーションを実行
        ExecuteCalibration();
    }
    
    /// <summary>
    /// キャリブレーションを実行
    /// </summary>
    private void ExecuteCalibration()
    {
        if (calibrationDelay > 0.0f)
        {
            SendCustomEventDelayedSeconds(nameof(CalibrateMotionDetectors), calibrationDelay);
        }
        else
        {
            CalibrateMotionDetectors();
        }
    }
    
    /// <summary>
    /// モーション検出器をキャリブレーション
    /// </summary>
    public void CalibrateMotionDetectors()
    {
        bool calibrated = false;
        
        // CentralizedMotionDetectorが設定されている場合
        if (centralizedMotionDetector != null)
        {
            centralizedMotionDetector.CalibrateAllDetectors();
            calibrated = true;
            
            if (showDebugInfo)
            {
                Debug.Log($"[MotionCalibrationTrigger] {triggerName}: CentralizedMotionDetector のキャリブレーションを実行しました");
            }
        }
        
        // 個別のMotionDetectorBaseが設定されている場合
        if (motionDetectors != null && motionDetectors.Length > 0)
        {
            foreach (var detector in motionDetectors)
            {
                if (detector != null)
                {
                    detector.Calibrate();
                    calibrated = true;
                    
                    if (showDebugInfo)
                    {
                        Debug.Log($"[MotionCalibrationTrigger] {triggerName}: {detector.name} のキャリブレーションを実行しました");
                    }
                }
            }
        }
        
        if (!calibrated && showDebugInfo)
        {
            Debug.LogWarning($"[MotionCalibrationTrigger] {triggerName}: キャリブレーション対象が設定されていません");
        }
    }
}

