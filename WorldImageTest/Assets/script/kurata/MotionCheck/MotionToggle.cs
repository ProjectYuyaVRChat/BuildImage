using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class MotionToggle : UdonSharpBehaviour
{
    [Header("ギミック本体")]
    public DoorGimmickSystemNew gimmickA;
    public DoorGimmickSystemNew gimmickB;
    public DoorGimmickSystemNew gimmickC;

    [Header("制御するオブジェクト")]
    public GameObject objectA;
    public GameObject objectB;
    public GameObject objectC;

    private bool triggeredA = false;
    private bool triggeredB = false;
    private bool triggeredC = false;

    private bool prevA = false;
    private bool prevB = false;
    private bool prevC = false;

    void Update()
    {
        // A条件：３モーション全部成功した時のみtrue
        bool nowA = gimmickA != null
            && gimmickA.IsMotion1Success()
            && gimmickA.IsMotion2Success()
            && gimmickA.IsMotion3Success();

        bool nowB = gimmickB != null
            && gimmickB.IsMotion1Success()
            && gimmickB.IsMotion2Success()
            && gimmickB.IsMotion3Success();
        bool nowC = gimmickC != null
            && gimmickC.IsMotion1Success()
            && gimmickC.IsMotion2Success()
            && gimmickC.IsMotion3Success();

        // --- Aが初めて3条件すべて成功した瞬間 ---
        if (!triggeredA && nowA && !prevA)
        {
            triggeredA = true;
            if (objectA) objectA.SetActive(false);
            if (objectB) objectB.SetActive(true);
        }

        // --- B ---
        if (!triggeredB && nowB && !prevB)
        {
            triggeredB = true;
            if (objectB) objectB.SetActive(false);
            if (objectC) objectC.SetActive(true);
        }

        // --- C ---
        if (!triggeredC && nowC && !prevC)
        {
            triggeredC = true;
            if (objectC) objectC.SetActive(false);
        }

        // 状態記録
        prevA = nowA;
        prevB = nowB;
        prevC = nowC;
    }
}