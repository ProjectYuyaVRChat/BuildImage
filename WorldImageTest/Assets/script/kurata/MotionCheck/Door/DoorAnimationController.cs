using UdonSharp;
using UnityEngine;

public class DoorAnimationController : UdonSharpBehaviour
{
    [Header("ドア設定")]
    [SerializeField] private GameObject leftDoor;
    [SerializeField] private GameObject rightDoor;
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float openSpeed = 2f;

    [Header("イベント通知先(UdonSharpBehaviour)")]
    [SerializeField] private UdonSharpBehaviour doorOpenEventReceiver;
    [SerializeField] private string doorOpenEventName = "OnDoorOpened";
    [SerializeField] private UdonSharpBehaviour doorCloseEventReceiver;
    [SerializeField] private string doorCloseEventName = "OnDoorClosed";
    
    // 内部状態
    private bool isDoorOpen = false;
    private bool isOpening = false;
    private bool isClosing = false;
    private float currentAngle = 0f;
    
    void Start()
    {
        CloseDoorImmediate();
    }
    
    void Update()
    {
        UpdateDoorAnimation();
    }
    
    private void UpdateDoorAnimation()
    {
        if (isOpening)
        {
            currentAngle += openSpeed * Time.deltaTime;
            if (currentAngle >= openAngle)
            {
                currentAngle = openAngle;
                isOpening = false;
                isDoorOpen = true;
                if (doorOpenEventReceiver != null && !string.IsNullOrEmpty(doorOpenEventName))
                {
                    doorOpenEventReceiver.SendCustomEvent(doorOpenEventName);
                }
            }
            UpdateDoorRotation();
        }
        else if (isClosing)
        {
            currentAngle -= openSpeed * Time.deltaTime;
            if (currentAngle <= 0f)
            {
                currentAngle = 0f;
                isClosing = false;
                isDoorOpen = false;
                if (doorCloseEventReceiver != null && !string.IsNullOrEmpty(doorCloseEventName))
                {
                    doorCloseEventReceiver.SendCustomEvent(doorCloseEventName);
                }
            }
            UpdateDoorRotation();
        }
    }
    
    private void UpdateDoorRotation()
    {
        if (leftDoor != null)
        {
            leftDoor.transform.localRotation = Quaternion.Euler(0, -currentAngle, 0);
        }
        if (rightDoor != null)
        {
            rightDoor.transform.localRotation = Quaternion.Euler(0, currentAngle, 0);
        }
    }
    
    public void OpenDoor()
    {
        if (!isOpening && !isDoorOpen)
        {
            isOpening = true;
        }
    }
    
    public void CloseDoor()
    {
        if (!isClosing && isDoorOpen)
        {
            isClosing = true;
        }
    }
    
    public void CloseDoorImmediate()
    {
        currentAngle = 0f;
        isOpening = false;
        isClosing = false;
        isDoorOpen = false;
        UpdateDoorRotation();
    }
    
    public void OpenDoorImmediate()
    {
        currentAngle = openAngle;
        isOpening = false;
        isClosing = false;
        isDoorOpen = true;
        UpdateDoorRotation();
    }
    
    public bool IsDoorOpen => isDoorOpen;
    public bool IsOpening => isOpening;
    public bool IsClosing => isClosing;
} 