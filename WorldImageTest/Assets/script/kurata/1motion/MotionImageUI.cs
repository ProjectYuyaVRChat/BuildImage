using UdonSharp;
using UnityEngine;

public class MotionImageUI : UdonSharpBehaviour
{
    [Header("モード別画像UI")]
    public GameObject normalPoseImage;

    [Header("個別モーション画像")]
    public GameObject rightHandImage;
    public GameObject leftHandImage;
    public GameObject headImage;

    [Header("複合モーション画像")]
    public GameObject rightLeftImage;
    public GameObject rightHeadImage;
    public GameObject leftHeadImage;
    public GameObject allImage;

    public void SetImageState(int motionMode, bool[] motionStates)
    {
        // まず全部オフ
        DisableAll();

        bool r = motionStates[(int)MotionType.RightHandUp];
        bool l = motionStates[(int)MotionType.LeftHandUp];
        bool h = (
            motionStates[(int)MotionType.HeadTiltLeft] ||
            motionStates[(int)MotionType.HeadTiltRight] ||
            motionStates[(int)MotionType.HeadTiltForward] ||
            motionStates[(int)MotionType.HeadTiltBackward]
        );

        // --- NORMAL ---
        if (!r && !l && !h)
        {
            Enable(normalPoseImage);
            return;
        }

        // --- 単体 ---
        if (r && !l && !h) Enable(rightHandImage);
        if (!r && l && !h) Enable(leftHandImage);
        if (!r && !l && h) Enable(headImage);

        // --- 複合 ---
        if (r && l && !h) Enable(rightLeftImage);
        if (r && h && !l) Enable(rightHeadImage);
        if (l && h && !r) Enable(leftHeadImage);

        // --- 全部成功 ---
        if (r && l && h) Enable(allImage);
    }

    private void DisableAll()
    {
        Enable(normalPoseImage, false);
        Enable(rightHandImage, false);
        Enable(leftHandImage, false);
        Enable(headImage, false);
        Enable(rightLeftImage, false);
        Enable(rightHeadImage, false);
        Enable(leftHeadImage, false);
        Enable(allImage, false);
    }

    private void Enable(GameObject obj, bool state = true)
    {
        if (obj != null) obj.SetActive(state);
    }
}
