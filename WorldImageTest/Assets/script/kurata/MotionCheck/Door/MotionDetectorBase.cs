using UdonSharp;
using VRC.Udon;
using UnityEngine;
using VRC.SDKBase;
using TMPro;

public abstract class MotionDetectorBase : UdonSharpBehaviour
{
    protected VRCPlayerApi localPlayer;

    // トラッキングデータ (位置・回転)
    protected Vector3 headPos;
    protected Vector3 leftHandPos;
    protected Vector3 rightHandPos;
    protected Vector3 basePos;

    protected Quaternion headRot;
    protected Quaternion leftHandRot;
    protected Quaternion rightHandRot;
    protected Quaternion baseRot;

    protected Quaternion bodyRot;

    // 高さ・速度
    protected float currentHeadHeight;
    private float previousHeadHeight;
    protected float verticalHeadVelocity;

    private bool initialize = false;

    // しゃがみ・伏せ検出用の変数
    private float basePlayerHeight = 0f;
    private float lastPlayerHeight = 0f;
    private float heightChangeThreshold = 0.01f; // 微小変化を無視
    private bool heightInitialized = false;

    // キャリブレーション関連
    protected bool headHeightInitialized = false;
    private int calibrationFrameCount = 0;
    private const int calibrationFramesNeeded = 30;
    protected float baseHeadHeight = 0f;
    protected float lastHeadHeight = 0f;
    protected Vector3 baseLeftHandPos;
    protected Vector3 baseRightHandPos;
    protected bool handPosInitialized = false;


    // ロール通知
    [UdonSynced] private string NoticeText = "";
    [SerializeField] protected TextMeshProUGUI debugText;
    public PlayerRoleManagerSimple roleManager;

    protected virtual void Start()
    {
        localPlayer = Networking.LocalPlayer;
    }

    protected virtual void Update()
    {
        // 範囲検知システムが有効で、エリアが非アクティブの場合は処理をスキップ
        if (!IsMotionDetectionEnabled())
        {
            return;
        }
        
        UpdateTrackingData();
        if(!CalibrateHeadHeight()) return;
        DetectMotion();
        CheckHeightChange();
    }

    /// <summary>
    /// プレイヤーのトラッキングデータを毎フレーム更新
    /// </summary>
    protected void UpdateTrackingData()
    {

        basePos = localPlayer.GetPosition();
        baseRot = localPlayer.GetRotation();
        bodyRot = baseRot;

        var headData = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
        headPos = headData.position;
        headRot = headData.rotation;

        var leftHandData = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand);
        leftHandPos = leftHandData.position;
        leftHandRot = leftHandData.rotation;

        var rightHandData = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand);
        rightHandPos = rightHandData.position;
        rightHandRot = rightHandData.rotation;

        float currentHeadHeight = headPos.y - basePos.y;

        if (!initialize)
        {
            this.currentHeadHeight = currentHeadHeight;
            previousHeadHeight = currentHeadHeight;
            initialize = true;
            return;
        }

        verticalHeadVelocity = (currentHeadHeight - previousHeadHeight) / Time.deltaTime;
        previousHeadHeight = currentHeadHeight;;
    }

    /// <summary>
    /// 推定身長の取得：HeadとPositionのY差の2倍
    /// </summary>
    protected float EstimatePlayerHeight()
    {
        float halfHeight = headPos.y - basePos.y;
        return halfHeight * 2f;
    }

    /// <summary>
    /// Update内で呼ぶ：起動時に基準を取得し、以後変化を検出
    /// </summary>
    protected void CheckHeightChange()
    {
        float currentHeight = EstimatePlayerHeight();

        if (!heightInitialized)
        {
            basePlayerHeight = currentHeight;
            lastPlayerHeight = currentHeight;
            heightInitialized = true;
            return;
        }

        float delta = Mathf.Abs(currentHeight - lastPlayerHeight);

        if (delta > heightChangeThreshold)
        {
            lastPlayerHeight = currentHeight;
            OnPlayerHeightChanged(currentHeight, basePlayerHeight);
        }
    }

    /// <summary>
    /// 身長が変化したときに呼ばれる（派生クラスでオーバーライド）
    /// </summary>
    protected virtual void OnPlayerHeightChanged(float current, float initial)
    {
        // デフォルトは何もしない
    }

    protected bool CalibrateHeadHeight()
    {
        float currentHeadHeight = headPos.y - basePos.y;

        if (!headHeightInitialized)
        {
            calibrationFrameCount++;
            baseHeadHeight = Mathf.Lerp(baseHeadHeight, currentHeadHeight, 0.1f);
            if (calibrationFrameCount >= calibrationFramesNeeded)
            {
                headHeightInitialized = true;
                lastHeadHeight = baseHeadHeight;
            }
            return false; // まだ未完了
        }

        return true; // キャリブレーション完了済
    }

    /// <summary>
    /// キャリブレーション（基準値再設定）
    /// </summary>
    public virtual void Calibrate()
    {
        // 頭の基準値
        baseHeadHeight = headPos.y - basePos.y;
        previousHeadHeight = baseHeadHeight;
        headHeightInitialized = true;

        // 手の基準値
        baseLeftHandPos = leftHandPos;
        baseRightHandPos = rightHandPos;
        handPosInitialized = true;

        calibrationFrameCount = calibrationFramesNeeded;
    }


    /// <summary>
    /// 派生クラスで各モーションを検出するための抽象メソッド
    /// </summary>
    protected abstract void DetectMotion();
    
    /// <summary>
    /// モーション検知が有効かどうかをチェック
    /// 派生クラスでオーバーライドして範囲検知システムとの連携を実装
    /// </summary>
    protected virtual bool IsMotionDetectionEnabled()
    {
        return true; // デフォルトは常に有効
    }

    /// <summary>
    /// デバッグ用のTextMeshProを外部から設定
    /// </summary>
    public void SetDebugText(TextMeshProUGUI text)
    {
        debugText = text;
    }

    /// <summary>
    /// ロール付きの通知表示
    /// </summary>
    protected void ShowMotionMessage(string motionName)
    {
        string role = "Unknown";
        if (roleManager != null)
        {
            role = roleManager.GetPlayerRole(localPlayer);
        }

        string message = $"{role} が {motionName} しました！";
        SetGlobalNotice(message);
    }

    /// <summary>
    /// 全員に同期される通知を設定
    /// </summary>
    protected void SetGlobalNotice(string msg)
    {
        if (!Networking.IsOwner(gameObject))
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }

        NoticeText = msg;
        RequestSerialization();

        if (debugText != null)
            debugText.text = msg;
    }

    public override void OnDeserialization()
    {
        base.OnDeserialization();

        if (debugText != null)
        {
            debugText.text = NoticeText;
        }
    }
}