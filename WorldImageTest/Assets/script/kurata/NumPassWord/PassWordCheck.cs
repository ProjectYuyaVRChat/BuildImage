
using UdonSharp;
using UnityEngine;
using TMPro;
using VRC.SDKBase;
using VRC.Udon;

public class PassWordCheck : UdonSharpBehaviour
{
    [Header("パスワード設定")]
    public string correctPassword = "1234"; // 正しいパスワードを設定
    
    [Header("UI設定")]
    public TextMeshProUGUI displayText; // UI用のText
    
    [Header("オブジェクト設定")]
    public GameObject door; // 扉のGameObject
    public GameObject[] numberCubes; // 数字や操作用のCubeを格納する配列
    
    [Header("ボタン設定")]
    [Tooltip("各ボタンに割り当てる値を設定（配列の順番でnumberCubesと対応）\n0-9: 数字\nEnter: 確定\nDelete: クリア\nClear: 削除\n?: デフォルト値")]
    [SerializeField]
    private string[] buttonValues = {
        "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "Enter", "Delete", "Clear"
    };
    
    [Tooltip("ボタンの数を設定（配列の長さを調整する場合）")]
    [SerializeField, Range(1, 20)]
    private int buttonCount = 13;

    private string inputPassword = ""; // 入力中のパスワード

    private void Start()
    {
        // 配列の長さチェック
        if (buttonValues.Length != numberCubes.Length)
        {
            Debug.LogWarning($"[PassWordCheck] buttonValuesの長さ({buttonValues.Length})とnumberCubesの長さ({numberCubes.Length})が一致しません。");
        }
        
        for (int i = 0; i < numberCubes.Length; i++)
        {
            string assignedValue = "";
            
            // インスペクタで設定された値を使用
            if (i < buttonValues.Length && !string.IsNullOrEmpty(buttonValues[i]))
            {
                assignedValue = buttonValues[i];
            }
            else
            {
                // デフォルト値（元のswitch文の内容）
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
                    case 12: assignedValue = "Clear"; break;
                    default: assignedValue = "?"; break;
                }
                
                if (showDebugInfo)
                {
                    Debug.LogWarning($"[PassWordCheck] ボタン{i}の値が設定されていないため、デフォルト値'{assignedValue}'を使用します。");
                }
            }

            // 事前にアタッチされたNumberCubeを取得
            NumberCube cubeScript = numberCubes[i].GetComponent<NumberCube>();

            if (cubeScript != null)
            {
                cubeScript.assignedValue = assignedValue;
                cubeScript.parentScript = this;
                
                if (showDebugInfo)
                {
                    Debug.Log($"[PassWordCheck] ボタン{i}: '{assignedValue}' を設定しました。");
                }
            }
            else
            {
                Debug.LogWarning($"[PassWordCheck] NumberCube script is missing on: {numberCubes[i].name}");
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
        else if (number == "Clear")
        {
            // Clearが押されたら全文字を削除
            inputPassword = "";
            
            if (showDebugInfo)
            {
                Debug.Log("[PassWordCheck] 全入力をクリアしました。");
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
            
            if (showDebugInfo)
            {
                Debug.Log("[PassWordCheck] 正しいパスワード！扉を開きました。");
            }
        }
        else
        {
            // パスワードが間違っていた時の処理
            displayText.text = "Incorrect Password!";
            
            if (showDebugInfo)
            {
                Debug.Log($"[PassWordCheck] 間違ったパスワード: {inputPassword}");
            }
        }

        inputPassword = ""; // 入力中のパスワードをリセット
    }
    
    #region 外部から呼び出せるメソッド
    
    /// <summary>
    /// 指定したボタンの値を変更
    /// </summary>
    /// <param name="buttonIndex">ボタンのインデックス</param>
    /// <param name="newValue">新しい値</param>
    public void ChangeButtonValue(int buttonIndex, string newValue)
    {
        if (buttonIndex < 0 || buttonIndex >= buttonValues.Length)
        {
            Debug.LogWarning($"[PassWordCheck] 無効なボタンインデックス: {buttonIndex}");
            return;
        }
        
        buttonValues[buttonIndex] = newValue;
        
        // 対応するNumberCubeの値も更新
        if (buttonIndex < numberCubes.Length)
        {
            NumberCube cubeScript = numberCubes[buttonIndex].GetComponent<NumberCube>();
            if (cubeScript != null)
            {
                cubeScript.assignedValue = newValue;
                
                if (showDebugInfo)
                {
                    Debug.Log($"[PassWordCheck] ボタン{buttonIndex}の値を'{newValue}'に変更しました。");
                }
            }
        }
    }
    
    /// <summary>
    /// すべてのボタンの値を一括変更
    /// </summary>
    /// <param name="newValues">新しい値の配列</param>
    public void ChangeAllButtonValues(string[] newValues)
    {
        if (newValues == null || newValues.Length == 0)
        {
            Debug.LogWarning("[PassWordCheck] 新しい値の配列が無効です。");
            return;
        }
        
        // 配列の長さが異なる場合は警告を表示（UdonSharpでは配列の長さを変更できない）
        if (newValues.Length != buttonValues.Length)
        {
            Debug.LogWarning($"[PassWordCheck] 配列の長さが一致しません。buttonValues: {buttonValues.Length}, newValues: {newValues.Length}");
            Debug.LogWarning("[PassWordCheck] UdonSharpでは配列の長さを変更できないため、設定可能な範囲内でのみ更新します。");
        }
        
        // 利用可能な範囲内で値をコピー
        int copyLength = Mathf.Min(newValues.Length, buttonValues.Length);
        for (int i = 0; i < copyLength; i++)
        {
            buttonValues[i] = newValues[i];
        }
        
        // NumberCubeの値も更新
        RefreshButtonValues();
        
        if (showDebugInfo)
        {
            Debug.Log($"[PassWordCheck] {copyLength}個のボタンの値を更新しました。");
        }
    }
    
    /// <summary>
    /// ボタンの値を再設定（既存の設定を反映）
    /// </summary>
    public void RefreshButtonValues()
    {
        for (int i = 0; i < numberCubes.Length && i < buttonValues.Length; i++)
        {
            NumberCube cubeScript = numberCubes[i].GetComponent<NumberCube>();
            if (cubeScript != null)
            {
                cubeScript.assignedValue = buttonValues[i];
            }
        }
        
        if (showDebugInfo)
        {
            Debug.Log("[PassWordCheck] ボタンの値を再設定しました。");
        }
    }
    
    /// <summary>
    /// 現在のボタン設定を取得
    /// </summary>
    /// <returns>ボタン設定の文字列</returns>
    public string GetButtonConfiguration()
    {
        string config = "ボタン設定:\n";
        for (int i = 0; i < buttonValues.Length; i++)
        {
            config += $"ボタン{i}: {buttonValues[i]}\n";
        }
        return config;
    }
    
    /// <summary>
    /// ボタンの数を設定（インスペクタから呼び出し可能）
    /// </summary>
    /// <param name="count">ボタンの数</param>
    public void SetButtonCount(int count)
    {
        if (count < 1 || count > 20)
        {
            Debug.LogWarning($"[PassWordCheck] 無効なボタン数: {count} (1-20の範囲で設定してください)");
            return;
        }
        
        buttonCount = count;
        
        if (showDebugInfo)
        {
            Debug.Log($"[PassWordCheck] ボタン数を{count}に設定しました。");
        }
    }
    
    /// <summary>
    /// ボタンの数を取得
    /// </summary>
    /// <returns>ボタンの数</returns>
    public int GetButtonCount()
    {
        return buttonCount;
    }
    
    /// <summary>
    /// パスワードを変更
    /// </summary>
    /// <param name="newPassword">新しいパスワード</param>
    public void ChangePassword(string newPassword)
    {
        correctPassword = newPassword;
        
        if (showDebugInfo)
        {
            Debug.Log($"[PassWordCheck] パスワードを'{newPassword}'に変更しました。");
        }
    }
    
    /// <summary>
    /// 入力中のパスワードをクリア
    /// </summary>
    public void ClearInput()
    {
        inputPassword = "";
        if (displayText != null)
        {
            displayText.text = "";
        }
        
        if (showDebugInfo)
        {
            Debug.Log("[PassWordCheck] 入力をクリアしました。");
        }
    }
    
    #endregion

    [Header("デバッグ設定")]
    [Tooltip("デバッグ情報を表示するか")]
    public bool showDebugInfo = true;
}
