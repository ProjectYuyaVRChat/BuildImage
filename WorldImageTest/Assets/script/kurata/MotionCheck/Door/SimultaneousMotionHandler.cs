using UdonSharp;
using UnityEngine;

public class SimultaneousMotionHandler : UdonSharpBehaviour
{
    [SerializeField] private bool showDebugInfo = true;
    
    // 外部データ
    private bool[] motionStates = new bool[19];
    private MotionType[] requiredMotions = new MotionType[3];
    private bool[] useMotions = new bool[3];
    
    public void Initialize(bool[] states, MotionType[] motions, bool[] uses)
    {
        motionStates = states;
        requiredMotions = motions;
        useMotions = uses;
    }
    
    public bool CheckRequirements()
    {
        bool allMet = true;
        
        if (useMotions[0] && !motionStates[(int)requiredMotions[0]])
        {
            allMet = false;
        }
        
        if (useMotions[1] && !motionStates[(int)requiredMotions[1]])
        {
            allMet = false;
        }
        
        if (useMotions[2] && !motionStates[(int)requiredMotions[2]])
        {
            allMet = false;
        }
        
        if (showDebugInfo && allMet)
        {
            Debug.Log("[SimultaneousMotion] すべてのモーションが同時に満たされました");
        }
        
        return allMet;
    }
    
    public bool AreRequirementsMet()
    {
        if (useMotions[0] && !motionStates[(int)requiredMotions[0]])
        {
            return false;
        }
        
        if (useMotions[1] && !motionStates[(int)requiredMotions[1]])
        {
            return false;
        }
        
        if (useMotions[2] && !motionStates[(int)requiredMotions[2]])
        {
            return false;
        }
        
        return true;
    }
} 