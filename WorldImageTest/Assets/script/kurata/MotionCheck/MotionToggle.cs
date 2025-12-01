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

    // すでに処理済みかどうかのフラグ
    private bool triggeredA = false;
    private bool triggeredB = false;
    private bool triggeredC = false;

    void Update()
    {
        // --- A ---
        if (!triggeredA && gimmickA != null && gimmickA.IsMotion1Success())
        {
            triggeredA = true;
            if (objectA != null) objectA.SetActive(false);
            if (objectB != null) objectB.SetActive(true);
        }

        // --- B ---
        if (!triggeredB && gimmickB != null && gimmickB.IsMotion1Success())
        {
            triggeredB = true;
            if (objectB != null) objectB.SetActive(false);
            if (objectC != null) objectC.SetActive(true);
        }

        // --- C ---
        if (!triggeredC && gimmickC != null && gimmickC.IsMotion1Success())
        {
            triggeredC = true;
            if (objectC != null) objectC.SetActive(false);
        }
    }
}
