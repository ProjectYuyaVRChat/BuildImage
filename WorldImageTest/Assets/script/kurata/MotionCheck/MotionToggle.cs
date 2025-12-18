using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
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

    [Header("SE")]
    public AudioClip successSE_A;
    public AudioClip successSE_B;
    public AudioClip successSE_C;

    AudioSource audioSource;


    // --- 同期用 ---
    [UdonSynced] private bool stepACompleted;
    [UdonSynced] private bool stepBCompleted;
    [UdonSynced] private bool stepCCompleted;

    private bool prevA;
    private bool prevB;
    private bool prevC;

    private bool lastStepA;
    private bool lastStepB;
    private bool lastStepC;

    private bool initialized = false;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }
    void Update()
    {

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

        // A
        if (!stepACompleted && nowA && !prevA)
        {
            if (!Networking.IsOwner(gameObject))
            {
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
            }

            stepACompleted = true;
            RequestSerialization();
            if (successSE_A) audioSource.PlayOneShot(successSE_A);
            ApplyState();
        }

        // B
        if (stepACompleted && !stepBCompleted && nowB && !prevB)
        {
            if (!Networking.IsOwner(gameObject))
            {
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
            }

            stepBCompleted = true;
            RequestSerialization();
            if (successSE_B) audioSource.PlayOneShot(successSE_B);
            ApplyState();
        }

        // C
        if (stepBCompleted && !stepCCompleted && nowC && !prevC)
        {
            if (!Networking.IsOwner(gameObject))
            {
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
            }

            stepCCompleted = true;
            RequestSerialization();
            if (successSE_C) audioSource.PlayOneShot(successSE_C);
            ApplyState();
        }

        prevA = nowA;
        prevB = nowB;
        prevC = nowC;
    }

    public override void OnDeserialization()
    {
        // 初回同期は無視
        if (!initialized)
        {
            lastStepA = stepACompleted;
            lastStepB = stepBCompleted;
            lastStepC = stepCCompleted;
            ApplyState();
            initialized = true;
            return;
        }

        // A 成功時
        if (!lastStepA && stepACompleted)
        {
            if (successSE_A) audioSource.PlayOneShot(successSE_A);
        }

        // B 成功時
        if (!lastStepB && stepBCompleted)
        {
            if (successSE_B) audioSource.PlayOneShot(successSE_B);
        }

        // C 成功時
        if (!lastStepC && stepCCompleted)
        {
            if (successSE_C) audioSource.PlayOneShot(successSE_C);
        }

        ApplyState();

        lastStepA = stepACompleted;
        lastStepB = stepBCompleted;
        lastStepC = stepCCompleted;
    }


    private void ApplyState()
    {
        if (objectA) objectA.SetActive(!stepACompleted);
        if (objectB) objectB.SetActive(stepACompleted && !stepBCompleted);
        if (objectC) objectC.SetActive(stepBCompleted && !stepCCompleted);
    }
}