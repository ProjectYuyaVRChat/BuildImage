using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

public abstract class MotionDetectorBase : UdonSharpBehaviour
{
    protected VRCPlayerApi localPlayer;

    [UdonSynced] private string NoticeText = "";

    [SerializeField]  protected TextMeshProUGUI debugText;

    protected virtual void Start()
    {
        localPlayer = Networking.LocalPlayer;
    }

    protected virtual void Update()
    {
        DetectMotion();
    }

    protected abstract void DetectMotion();

    public void SetDebugText(TextMeshProUGUI text)
    {
        debugText = text;
    }

    protected void ShowMotionMessage(string motionName)
    {
        string role = "Unknown";
        if (roleManager != null)
        {
            role = roleManager.GetPlayerRole(localPlayer);
        }

        string message = $"{role} が {motionName} しました！";
        SetGlobalNotice(message);
        Debug.Log(message);
    }

    protected void SetGlobalNotice(string msg)
    {
        if (!Networking.IsOwner(gameObject))
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }

        NoticeText = msg;
        RequestSerialization();
    }

    public override void OnDeserialization()
    {
        base.OnDeserialization();
        if (debugText != null)
        {
            debugText.text = NoticeText;
        }
    }

    public PlayerRoleManagerSimple roleManager;
}
