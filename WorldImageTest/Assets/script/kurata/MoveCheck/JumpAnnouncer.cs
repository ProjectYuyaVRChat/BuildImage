using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class JumpAnnouncer : UdonSharpBehaviour
{
    public TextMeshProUGUI text;
    public GameObject door; // 扉のGameObject


    void Start()
    {
        text.text = "start";
    }

    public void ShowJumpMessage(string playerName)
    {
        text.text = playerName + "is Jump";
        door.SetActive(false);
    }
}
