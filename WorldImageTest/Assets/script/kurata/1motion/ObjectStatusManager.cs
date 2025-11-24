using UdonSharp;
using UnityEngine;

public class ObjectStatusManager : UdonSharpBehaviour
{
    [Header("成功/失敗を表示するオブジェクト")]
    [SerializeField] private GameObject[] successObjects = new GameObject[3]; // 成功用
    [SerializeField] private GameObject[] failObjects = new GameObject[3];    // 失敗用（不要なら空でもOK）

    // 外部から渡されるデータ
    private int motionMode = 0;
    private bool[] motionStates = new bool[19];
    private MotionType[] requiredMotions = new MotionType[3];
    private bool[] useMotions = new bool[3];
    private bool[] stepCompleted = new bool[3];
    private int currentStep = 0;
    private bool[] achieved = new bool[3];   // Counter モード用

    public void SetData(
        int mode,
        bool[] states,
        MotionType[] motions,
        bool[] uses,
        int step,
        bool[] stepsDone,
        bool[] achievedCounter
    )
    {
        motionMode = mode;
        motionStates = states;
        requiredMotions = motions;
        useMotions = uses;
        currentStep = step;
        stepCompleted = stepsDone;
        achieved = achievedCounter;

        UpdateStatusObjects();
    }

    private void UpdateStatusObjects()
    {
        for (int i = 0; i < 3; i++)
        {
            if (!useMotions[i])
            {
                SetObject(i, false);
                continue;
            }

            bool active = false;

            switch (motionMode)
            {
                case 0: // Simultaneous
                    active = motionStates[(int)requiredMotions[i]];
                    break;

                case 1: // Sequential
                    if (stepCompleted[i])
                        active = true;
                    else if (i == currentStep)
                        active = motionStates[(int)requiredMotions[i]];
                    else
                        active = false;
                    break;

                case 2: // Counter
                    active = achieved[i];
                    break;
            }

            SetObject(i, active);
        }
    }

    private void SetObject(int index, bool success)
    {
        if (successObjects != null && successObjects[index] != null)
            successObjects[index].SetActive(success);

        if (failObjects != null && failObjects[index] != null)
            failObjects[index].SetActive(!success);
    }
}
