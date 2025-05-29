using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VRC.SDKBase;

public class ActionResponder : UdonSharpBehaviour
{
    public string[] triggerSequence;
    public TextMeshProUGUI text;
    public GameObject targetObject;
    private bool triggered = false;

    public void OnPlayerAction(string actionName, VRCPlayerApi player)
    {
        if (triggered) return;

        ActionManager manager = GetComponentInParent<ActionManager>();

        if (manager == null) return;

        if (manager.CheckSequence(triggerSequence))
        {
            if (text != null)
            {
                text.text = player.displayName + " triggered sequence";
            }

            if (targetObject != null)
            {
                targetObject.SetActive(false);
            }

            triggered = true;
        }
    }
}
