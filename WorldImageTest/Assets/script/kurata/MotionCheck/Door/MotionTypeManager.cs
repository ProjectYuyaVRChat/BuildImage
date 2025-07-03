using UdonSharp;
using UnityEngine;

public class MotionTypeManager : UdonSharpBehaviour
{
    public static string GetMotionName(MotionType motionType)
    {
        switch (motionType)
        {
            case MotionType.Jump:
                return "ジャンプ";
            case MotionType.Crouch:
                return "しゃがむ";
            case MotionType.Prone:
                return "伏せる";
            case MotionType.HeadTiltLeft:
                return "頭を左に傾ける";
            case MotionType.HeadTiltRight:
                return "頭を右に傾ける";
            case MotionType.HeadTiltForward:
                return "頭を前に傾ける";
            case MotionType.HeadTiltBackward:
                return "頭を後ろに傾ける";
            case MotionType.HeadTurnLeft:
                return "頭を左に回す";
            case MotionType.HeadTurnRight:
                return "頭を右に回す";
            case MotionType.BodyLeanLeft:
                return "体を左に傾ける";
            case MotionType.BodyLeanRight:
                return "体を右に傾ける";
            case MotionType.BodyLeanForward:
                return "体を前に傾ける";
            case MotionType.BodyLeanBackward:
                return "体を後ろに傾ける";
            case MotionType.RightHandUp:
                return "右手を上げる";
            case MotionType.RightHandSide:
                return "右手を横に伸ばす";
            case MotionType.RightHandForward:
                return "右手を前に伸ばす";
            case MotionType.LeftHandUp:
                return "左手を上げる";
            case MotionType.LeftHandSide:
                return "左手を横に伸ばす";
            case MotionType.LeftHandForward:
                return "左手を前に伸ばす";
            default:
                return motionType.ToString();
        }
    }
} 