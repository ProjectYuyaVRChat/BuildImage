using UdonSharp;
using UnityEngine;

public class CounterMotionHandler : UdonSharpBehaviour
{
    [SerializeField] private bool showDebugInfo = true;
    
    // 内部状態
    private int motionCounter = 0;
    private int requiredCount = 0;
    private bool[] motionAchievedCounter = new bool[3];
    
    // 外部データ
    private bool[] motionStates = new bool[19];
    private MotionType[] requiredMotions = new MotionType[3];
    private bool[] useMotions = new bool[3];
    
    public void Initialize(bool[] states, MotionType[] motions, bool[] uses)
    {
        motionStates = states;
        requiredMotions = motions;
        useMotions = uses;
        
        // カウンターモード用の初期化
        motionCounter = 0;
        requiredCount = 0;
        for (int i = 0; i < motionAchievedCounter.Length; i++)
        {
            motionAchievedCounter[i] = false;
        }
        
        if (useMotions[0]) requiredCount++;
        if (useMotions[1]) requiredCount++;
        if (useMotions[2]) requiredCount++;
    }
    
    public bool CheckRequirements()
    {
        // 現在満たされているモーションの数をカウント
        int currentCount = 0;
        
        // 各モーションをチェックし、一度でも達成したものはカウント
        if (useMotions[0] && (motionStates[(int)requiredMotions[0]] || motionAchievedCounter[0]))
        {
            if (motionStates[(int)requiredMotions[0]])
            {
                motionAchievedCounter[0] = true; // 一度達成したことを記録
            }
            currentCount++;
        }
        
        if (useMotions[1] && (motionStates[(int)requiredMotions[1]] || motionAchievedCounter[1]))
        {
            if (motionStates[(int)requiredMotions[1]])
            {
                motionAchievedCounter[1] = true; // 一度達成したことを記録
            }
            currentCount++;
        }
        
        if (useMotions[2] && (motionStates[(int)requiredMotions[2]] || motionAchievedCounter[2]))
        {
            if (motionStates[(int)requiredMotions[2]])
            {
                motionAchievedCounter[2] = true; // 一度達成したことを記録
            }
            currentCount++;
        }
        
        // カウンターが更新された場合
        if (currentCount > motionCounter)
        {
            motionCounter = currentCount;
            if (showDebugInfo)
            {
                Debug.Log($"[CounterMotion] カウンター更新: {motionCounter}/{requiredCount}");
            }
        }
        
        return motionCounter >= requiredCount;
    }
    
    public void ResetCounter()
    {
        motionCounter = 0;
        for (int i = 0; i < motionAchievedCounter.Length; i++)
        {
            motionAchievedCounter[i] = false;
        }
        
        if (showDebugInfo)
        {
            Debug.Log("[CounterMotion] カウンターをリセットしました");
        }
    }
    
    // 外部からアクセス可能なプロパティ
    public int MotionCounter => motionCounter;
    public int RequiredCount => requiredCount;
    public bool[] MotionAchievedCounter => motionAchievedCounter;
} 