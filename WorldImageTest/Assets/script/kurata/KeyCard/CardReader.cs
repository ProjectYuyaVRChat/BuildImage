
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

public class CardReader : UdonSharpBehaviour
{
    public TMP_Text cardCountText;
    public Transform leftDoor;
    public Transform rightDoor;
    public int requiredCardCount = 4; // 必要なカードの枚数

    public Vector3 leftOpenOffset = new Vector3(-1f, 0f, 0f);
    public Vector3 rightOpenOffset = new Vector3(1f, 0f, 0f);
    public float doorOpenSpeed = 2f;

    private Vector3 leftClosedPos;
    private Vector3 rightClosedPos;
    private bool[] cardFlags;
    private bool doorOpened = false;

    public Animator GateAnimator;
    
　　[Header("SE")]
    public AudioClip successSE_A;
    
    AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        leftClosedPos = leftDoor.position;
        rightClosedPos = rightDoor.position;
        cardFlags = new bool[requiredCardCount];
        UpdateUI();
    }

    private void OnTriggerEnter(Collider other)
    {
        var card = other.GetComponent<CardKey>();
        if (card == null) return;

        int id = card.keyID;
        if (id < 1 || id > requiredCardCount) return;

        int index = id - 1;

        if (cardFlags[index]) return;

        cardFlags[index] = true;
        Debug.Log($"カードキー {id} 読み取り成功");
        UpdateUI();
        CheckAllCards();
        if (successSE_A) audioSource.PlayOneShot(successSE_A);
    }



   void UpdateUI()
{
    int heldCount = 0;
    foreach (bool flag in cardFlags)
    {
        if (flag) heldCount++;
    }

    cardCountText.text = $"カードキー：{heldCount}/{requiredCardCount}枚";
}


    private void CheckAllCards()
    {
        if (doorOpened) return;

        foreach (bool flag in cardFlags)
        {
            if (!flag) return; // まだ足りない
        }

        doorOpened = true;
        Debug.Log("全カード読み取り完了 → ドア開く");

        // 左右のドアをずらすだけなんであとで好きなようにしてもろて
        SendCustomEventDelayedSeconds(nameof(OpenDoors), 0.1f);
    }

    public void OpenDoors()
    {
        /*leftDoor.position = Vector3.Lerp(leftDoor.position, leftClosedPos + leftOpenOffset, doorOpenSpeed * Time.deltaTime);
        rightDoor.position = Vector3.Lerp(rightDoor.position, rightClosedPos + rightOpenOffset, doorOpenSpeed * Time.deltaTime);*/
        GateAnimator.SetTrigger("DoorOpenTrigger");
        // Lerpだけだと一回しか実行されないから、継続的に動かすには Update 内で処理してもOK
    }

    void Update()
    {
        if (doorOpened)
        {
            leftDoor.position = Vector3.MoveTowards(leftDoor.position, leftClosedPos + leftOpenOffset, doorOpenSpeed * Time.deltaTime);
            rightDoor.position = Vector3.MoveTowards(rightDoor.position, rightClosedPos + rightOpenOffset, doorOpenSpeed * Time.deltaTime);
        }
    }
}