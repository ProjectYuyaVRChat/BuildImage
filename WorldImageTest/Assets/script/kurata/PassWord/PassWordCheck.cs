
using UdonSharp;
using UnityEngine;
using TMPro;
using VRC.SDKBase;
using VRC.Udon;

public class PassWordCheck : UdonSharpBehaviour
{
    public string correctPassword = "1234"; // 正しいパスワードを設定
    public TextMeshProUGUI displayText; // UI用のText
    public GameObject door; // 扉のGameObject
    public GameObject[] numberCubes; // 数字や操作用のCubeを格納する配列


    private string inputPassword = ""; // 入力中のパスワード

    private void Start()
    {
        for (int i = 0; i < numberCubes.Length; i++)
        {
            string assignedValue = "";
            switch (i)
            {
                case 0: assignedValue = "0"; break;
                case 1: assignedValue = "1"; break;
                case 2: assignedValue = "2"; break;
                case 3: assignedValue = "3"; break;
                case 4: assignedValue = "4"; break;
                case 5: assignedValue = "5"; break;
                case 6: assignedValue = "6"; break;
                case 7: assignedValue = "7"; break;
                case 8: assignedValue = "8"; break;
                case 9: assignedValue = "9"; break;
                case 10: assignedValue = "Enter"; break;
                case 11: assignedValue = "Delete"; break;
            }

            // 事前にアタッチされたNumberCubeを取得
            NumberCube cubeScript = numberCubes[i].GetComponent<NumberCube>();

            if (cubeScript != null)
            {
                cubeScript.assignedValue = assignedValue;
                cubeScript.parentScript = this;
            }
            else
            {
                Debug.LogWarning($"NumberCube script is missing on: {numberCubes[i].name}");
            }
        }
    }

    public void AppendNumber(string number)
    {
        if (number == "Enter")
        {
            CheckPassword(); // Enterが押されたらパスワードを確認
        }
        else if (number == "Delete")
        {
            // Deleteが押されたら最後の文字を削除
            if (inputPassword.Length > 0)
            {
                inputPassword = inputPassword.Substring(0, inputPassword.Length - 1);
            }
        }
        else
        {
            // 数字を追加
            inputPassword += number;
        }

        // 入力内容を更新
        displayText.text = inputPassword;
    }

    public void CheckPassword()
    {
        if (inputPassword == correctPassword)
        {
            // 正しいパスワードが入力された時の処理
            door.SetActive(false); // 扉を非アクティブにする
        }
        else
        {
            // パスワードが間違っていた時の処理
            displayText.text = "Incorrect Password!";
        }

        inputPassword = ""; // 入力中のパスワードをリセット
    }
}
