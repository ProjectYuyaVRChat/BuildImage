using UdonSharp;
using UnityEngine;

public class BoxCheckManager : UdonSharpBehaviour
{
    [Header("箱スロット（5個）")]
    public BoxSlot[] boxSlots;

    [Header("消したい壁")]
    public GameObject wall;

    [SerializeField] private GimmickManager gimmickManager;
    private bool isCleared = false;

    void OnCorrectAction()
    {
        if (isCleared) return;

        isCleared = true;
        gimmickManager.ReportClear();
    }
    
    private void Update()
    {
        for (int i = 0; i < boxSlots.Length; i++)
        {
            if (!boxSlots[i].isCorrect)
            {
                return; // 1個でも間違ってたら終了
            }
        }

        // 全部正解
        wall.SetActive(false);
        enabled = false; // 以後チェックしない
    }
}
