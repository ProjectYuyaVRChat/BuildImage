using UdonSharp;
using UnityEngine;

public class MotionSuccess : UdonSharpBehaviour
{
    [Header("ドアギミック本体")]
    public DoorGimmickSystemNew gimmick;

    [Header("成功時に ON になるオブジェクト")]
    public GameObject motion1Object;
    public GameObject motion2Object;
    public GameObject motion3Object;

    void Update()
    {
        if (gimmick == null) return;

        // 1つ目のモーション
        if (motion1Object != null)
            motion1Object.SetActive(gimmick.IsMotion1Success());

        // 2つ目のモーション
        if (motion2Object != null)
            motion2Object.SetActive(gimmick.IsMotion2Success());

        // 3つ目のモーション
        if (motion3Object != null)
            motion3Object.SetActive(gimmick.IsMotion3Success());
    }
}
