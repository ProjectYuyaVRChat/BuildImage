using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class MotionObjectActivator : UdonSharpBehaviour
{
    [Header("モーションに対応するオブジェクト")]
    [SerializeField] private GameObject motion1Object;
    [SerializeField] private GameObject motion2Object;
    [SerializeField] private GameObject motion3Object;

    [Header("対応モーションタイプ")]
    [SerializeField] private MotionType motion1 = MotionType.Jump;
    [SerializeField] private MotionType motion2 = MotionType.RightHandUp;
    [SerializeField] private MotionType motion3 = MotionType.HeadTiltLeft;

    // 外部からモーション成功を通知するやつ
    public void SetMotionSuccess(MotionType motionType)
    {
        if (motionType == motion1)
        {
            if (motion1Object != null)
            {
                motion1Object.SetActive(true);
            }
        }
        else if (motionType == motion2)
        {
            if (motion2Object != null)
            {
                motion2Object.SetActive(true);
            }
        }
        else if (motionType == motion3)
        {
            if (motion3Object != null)
            {
                motion3Object.SetActive(true);
            }
        }
    }

    // MotionType ではなく直接モーション番号で設定
    public void SetMotion1Success()
    {
        if (motion1Object != null)
        {
            motion1Object.SetActive(true);
        }
    }

    public void SetMotion2Success()
    {
        if (motion2Object != null)
        {
            motion2Object.SetActive(true);
        }
    }

    public void SetMotion3Success()
    {
        if (motion3Object != null)
        {
            motion3Object.SetActive(true);
        }
    }
}
