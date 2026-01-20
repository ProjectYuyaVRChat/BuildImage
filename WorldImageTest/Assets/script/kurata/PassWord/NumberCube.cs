using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class NumberCube : UdonSharpBehaviour
{
    [HideInInspector]
    public string assignedValue; // このCubeに割り当てられた値（数字や操作）
    [HideInInspector]
    public PassWordCheck parentScript; // 親スクリプトいれて

    public override void Interact()
    {
        // Cubeがクリックされたときに親スクリプトのAppendNumberを呼び出すだけ
        parentScript.AppendNumber(assignedValue);
    }
}