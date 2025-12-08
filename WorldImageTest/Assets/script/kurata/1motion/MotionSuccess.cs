using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

public class MotionSuccess : UdonSharpBehaviour
{
    [UdonSynced] private bool motion1State;
    [UdonSynced] private bool motion2State;
    [UdonSynced] private bool motion3State;

    [Header("ドアギミック本体")]
    public DoorGimmickSystemNew gimmick;

    [Header("成功時に ON になるオブジェクト")]
    public GameObject motion1Object;
    public GameObject motion2Object;
    public GameObject motion3Object;

    void Start()
    {
        EnsureMasterOwnership();
        UpdateDisplay();
    }

    void Update()
    {
        if (!Networking.LocalPlayer.isMaster) return; // ホストだけが制御

        // マスターが所有権を持っていないと同期が送られない
        EnsureMasterOwnership();

        if (gimmick == null) return;

        bool new1 = gimmick.IsMotion1Success();
        bool new2 = gimmick.IsMotion2Success();
        bool new3 = gimmick.IsMotion3Success();

        // 値が変わったときだけ同期
        if (new1 != motion1State || new2 != motion2State || new3 != motion3State)
        {
            motion1State = new1;
            motion2State = new2;
            motion3State = new3;

            RequestSerialization(); // ★同期発動
        }
    }

    // プレイヤー全員の表示更新
    public override void OnDeserialization()
    {
        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        if (motion1Object) motion1Object.SetActive(motion1State);
        if (motion2Object) motion2Object.SetActive(motion2State);
        if (motion3Object) motion3Object.SetActive(motion3State);
    }

    // マスターが必ずオーナーになるようにする
    void EnsureMasterOwnership()
    {
        if (Networking.IsMaster && !Networking.IsOwner(gameObject))
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }
    }

    // マスターが入れ替わったときに再度オーナーを取り直す
    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        EnsureMasterOwnership();
    }
}
