using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class SlotManager : UdonSharpBehaviour
{
    [Header("吸着スロット一覧")]
    public AttractionSlot[] slots;

    [Header("全部揃ったら消すオブジェクト")]
    public GameObject objectToHide;

    private bool triggered = false;

    private void Start()
    {
        // 各スロットに Manager を登録
        foreach (var slot in slots)
        {
            slot.SetManager(this);
        }
    }

    public void CheckAllSlots()
    {
        if (triggered) return;

        foreach (var slot in slots)
        {
            if (!slot.isPlaced) return;
        }

        // 全て揃った！
        triggered = true;
        Debug.Log("🎉 すべての正解オブジェクトが正しい位置にセットされました！");

        if (objectToHide != null)
        {
            objectToHide.SetActive(false);
        }
    }
}
