using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class ActionManager : UdonSharpBehaviour
{
    public ActionResponder[] responders;
    public BaseActionDetector[] detectors;
    private string[] actionHistory = new string[5];
    private int historyIndex = 0;

    void Update()
    {
        var player = Networking.LocalPlayer;
        if (player == null) return;

        foreach (var detector in detectors)
        {
            if (detector == null) continue;
            detector.CheckAction(player, this);
        }
    }

    public void TriggerAction(string actionName, VRCPlayerApi player)
    {
        // 履歴に追加
        actionHistory[historyIndex] = actionName;
        historyIndex = (historyIndex + 1) % actionHistory.Length;

        foreach (var responder in responders)
        {
            responder.OnPlayerAction(actionName, player);
        }
    }

    public bool CheckSequence(string[] sequence)
    {
        int count = sequence.Length;
        for (int i = 0; i < count; i++)
        {
            int index = (historyIndex - count + i + actionHistory.Length) % actionHistory.Length;
            if (actionHistory[index] != sequence[i]) return false;
        }
        return true;
    }
}
