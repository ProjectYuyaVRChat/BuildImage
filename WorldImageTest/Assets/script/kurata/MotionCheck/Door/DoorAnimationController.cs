using UdonSharp;
using UnityEngine;

public class DoorAnimationController : UdonSharpBehaviour
{
    [Header("ドア設定")]
    [SerializeField] private GameObject leftDoor;
    [SerializeField] private GameObject rightDoor;
    [SerializeField] private float openDistance = 1.0f;
    [SerializeField] private float openSpeed = 0.1f;

    [Header("イベント通知先(UdonSharpBehaviour)")]
    [SerializeField] private UdonSharpBehaviour doorOpenEventReceiver;
    [SerializeField] private string doorOpenEventName = "OnDoorOpened";
    [SerializeField] private UdonSharpBehaviour doorCloseEventReceiver;
    [SerializeField] private string doorCloseEventName = "OnDoorClosed";

    // 内部状態
    private bool isDoorOpen = false;
    private bool isOpening = false;
    private bool isClosing = false;
    private float currentOffset = 0f;

    // 初期位置
    private Vector3 leftStartPos;
    private Vector3 rightStartPos;

    void Start()
    {
        if (leftDoor != null)
            leftStartPos = leftDoor.transform.localPosition;

        if (rightDoor != null)
            rightStartPos = rightDoor.transform.localPosition;

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
            currentOffset += openSpeed * Time.deltaTime;
            if (currentOffset >= openDistance)
            {
                currentOffset = openDistance;
                isOpening = false;
                isDoorOpen = true;

                if (doorOpenEventReceiver != null && !string.IsNullOrEmpty(doorOpenEventName))
                {
                    doorOpenEventReceiver.SendCustomEvent(doorOpenEventName);
                }
            }

            UpdateDoorPosition();
        }
        else if (isClosing)
        {
            currentOffset -= openSpeed * Time.deltaTime;
            if (currentOffset <= 0f)
            {
                currentOffset = 0f;
                isClosing = false;
                isDoorOpen = false;

                if (doorCloseEventReceiver != null && !string.IsNullOrEmpty(doorCloseEventName))
                {
                    doorCloseEventReceiver.SendCustomEvent(doorCloseEventName);
                }
            }

            UpdateDoorPosition();
        }
    }

    private void UpdateDoorPosition()
    {
        if (leftDoor != null)
        {
            leftDoor.transform.localPosition =
                leftStartPos + Vector3.left * currentOffset;
        }

        if (rightDoor != null)
        {
            rightDoor.transform.localPosition =
                rightStartPos + Vector3.right * currentOffset;
        }
    }

    public void OpenDoor()
    {
        if (!isOpening && !isDoorOpen)
        {
            isOpening = true;
            isClosing = false;
        }
    }

    public void CloseDoor()
    {
        if (!isClosing && isDoorOpen)
        {
            isClosing = true;
            isOpening = false;
        }
    }

    public void CloseDoorImmediate()
    {
        currentOffset = 0f;
        isOpening = false;
        isClosing = false;
        isDoorOpen = false;
        UpdateDoorPosition();
    }

    public void OpenDoorImmediate()
    {
        currentOffset = openDistance;
        isOpening = false;
        isClosing = false;
        isDoorOpen = true;
        UpdateDoorPosition();
    }

    public bool IsDoorOpen => isDoorOpen;
    public bool IsOpening => isOpening;
    public bool IsClosing => isClosing;
}