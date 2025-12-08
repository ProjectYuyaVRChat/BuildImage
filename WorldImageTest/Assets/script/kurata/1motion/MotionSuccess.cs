using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
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
        // 初期表示を全員で揃える
        UpdateDisplay();
    }

    void Update()
    {
        if (gimmick == null) return;

        bool new1 = gimmick.IsMotion1Success();
        bool new2 = gimmick.IsMotion2Success();
        bool new3 = gimmick.IsMotion3Success();

        // 値が変わったときだけ同期
        if (new1 != motion1State || new2 != motion2State || new3 != motion3State)
        {
            // ローカルでモーションを成功させた人だけが同期を送る
            // → 他の人が受信後に「自分は成功していない」判定で書き戻すのを防ぐ
            if (!Networking.IsOwner(gameObject) && !(new1 || new2 || new3))
            {
                // オーナーでなく、ローカルでは成功していないなら何もしない
                return;
            }
            // ローカルで成功を検知した人はオーナーを取得して同期を送る
            if (!Networking.IsOwner(gameObject))
            {
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
            }

            motion1State = new1;
            motion2State = new2;
            motion3State = new3;

            RequestSerialization(); // ★同期発動（Manualモード）
            UpdateDisplay();        // オーナー自身の画面も即時反映
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
}
