
using UdonSharp;
using VRC.SDKBase;
using VRC.Udon;

public class BaseActionDetector : UdonSharpBehaviour
{
    // abstractの代わりにvirtualにする
    public virtual void CheckAction(VRCPlayerApi player, ActionManager manager)
    {
        // 基底は空でも良い
    }
}
