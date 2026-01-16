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
    private bool positionsInitialized = false;

    void Awake()
    {
        // Awake()で初期位置を保存（Start()より前に実行されるため、他のスクリプトが位置を変更する前に保存できる）
        if (leftDoor != null)
            leftStartPos = leftDoor.transform.localPosition;

        if (rightDoor != null)
            rightStartPos = rightDoor.transform.localPosition;

        positionsInitialized = true;
    }

    void Start()
    {
        // 初期位置を保存（Awake()が呼ばれない場合に備えてStart()でも実行）
        if (!positionsInitialized)
        {
            if (leftDoor != null)
                leftStartPos = leftDoor.transform.localPosition;

            if (rightDoor != null)
                rightStartPos = rightDoor.transform.localPosition;

            positionsInitialized = true;
            
            Debug.Log($"[DoorAnimationController] Start()で初期位置を保存しました - leftDoor: {(leftDoor != null ? leftDoor.name : "null")}, rightDoor: {(rightDoor != null ? rightDoor.name : "null")}");
        }
        
        // 初期位置を保存しただけなので、ドアの位置は変更しない
        // CloseDoorImmediate()は必要に応じて外部から呼ばれる
        currentOffset = 0f;
        isDoorOpen = false;
        isOpening = false;
        isClosing = false;
    }

    void Update()
    {
        UpdateDoorAnimation();
    }

    private void UpdateDoorAnimation()
    {
        if (isOpening)
        {
            float previousOffset = currentOffset;
            currentOffset += openSpeed * Time.deltaTime;
            
            if (currentOffset >= openDistance)
            {
                currentOffset = openDistance;
                isOpening = false;
                isDoorOpen = true;
                
                Debug.Log($"[DoorAnimationController] ドアが完全に開きました - currentOffset: {currentOffset}");

                if (doorOpenEventReceiver != null && !string.IsNullOrEmpty(doorOpenEventName))
                {
                    doorOpenEventReceiver.SendCustomEvent(doorOpenEventName);
                }
            }

            UpdateDoorPosition();
            
            // デバッグ：5フレームごとにログを出力（更新されているか確認）
            if (Time.frameCount % 60 == 0)
            {
                Debug.Log($"[DoorAnimationController] ドアを開いています... currentOffset: {currentOffset:F3}/{openDistance}, isOpening: {isOpening}");
            }
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
        // 初期位置が保存されている場合のみ位置を更新
        if (!positionsInitialized)
        {
            Debug.LogWarning("[DoorAnimationController] 初期位置が保存されていません！");
            return;
        }

        if (leftDoor != null)
        {
            Vector3 newLeftPos = leftStartPos + Vector3.left * currentOffset;
            leftDoor.transform.localPosition = newLeftPos;
            
            // デバッグ：最初の数回のみログを出力
            if (isOpening && currentOffset < 0.1f)
            {
                Debug.Log($"[DoorAnimationController] 左扉の位置を更新: {newLeftPos} (offset: {currentOffset:F3})");
            }
        }
        else
        {
            Debug.LogWarning("[DoorAnimationController] 左扉が設定されていません！");
        }

        if (rightDoor != null)
        {
            Vector3 newRightPos = rightStartPos + Vector3.right * currentOffset;
            rightDoor.transform.localPosition = newRightPos;
            
            // デバッグ：最初の数回のみログを出力
            if (isOpening && currentOffset < 0.1f)
            {
                Debug.Log($"[DoorAnimationController] 右扉の位置を更新: {newRightPos} (offset: {currentOffset:F3})");
            }
        }
        else
        {
            Debug.LogWarning("[DoorAnimationController] 右扉が設定されていません！");
        }
    }

    public void OpenDoor()
    {
        Debug.Log($"[DoorAnimationController] OpenDoor() called - isOpening: {isOpening}, isDoorOpen: {isDoorOpen}, positionsInitialized: {positionsInitialized}");
        
        // 初期位置が保存されていない場合は今すぐ保存
        if (!positionsInitialized)
        {
            if (leftDoor != null)
                leftStartPos = leftDoor.transform.localPosition;

            if (rightDoor != null)
                rightStartPos = rightDoor.transform.localPosition;

            positionsInitialized = true;
            Debug.Log($"[DoorAnimationController] OpenDoor()内で初期位置を保存しました - leftDoor: {(leftDoor != null ? leftDoor.name : "null")}, rightDoor: {(rightDoor != null ? rightDoor.name : "null")}");
        }
        
        if (!isOpening && !isDoorOpen)
        {
            isOpening = true;
            isClosing = false;
            Debug.Log($"[DoorAnimationController] ドアを開き始めます - leftDoor: {(leftDoor != null ? leftDoor.name : "null")}, rightDoor: {(rightDoor != null ? rightDoor.name : "null")}");
            Debug.Log($"[DoorAnimationController] openSpeed: {openSpeed}, openDistance: {openDistance}, currentOffset: {currentOffset}");
        }
        else
        {
            Debug.Log($"[DoorAnimationController] OpenDoor()が呼ばれましたが、既に開いていますまたは開く途中です");
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