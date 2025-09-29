
using UdonSharp;
using UnityEngine;
using TMPro;
using VRC.SDKBase;
using VRC.Udon;

public class KanaPassword : UdonSharpBehaviour
{
    [Header("パスワード設定")]
    [Tooltip("正しいカタカナパスワードを設定（例：アイウエオ）")]
    public string correctPassword = "アイウエオ";
    
    [Header("UI設定")]
    [Tooltip("入力中のカタカナを表示するUI")]
    public TextMeshProUGUI displayText;
    
    [Header("成功時の処理")]
    [Tooltip("パスワード成功時にTrueを受け取るスクリプト")]
    public UdonBehaviour targetScript;
    [Tooltip("呼び出すメソッド名")]
    public string targetMethodName = "OnPasswordSuccess";
    
    [Header("オブジェクト設定")]
    [Tooltip("カタカナ入力用のCubeを格納する配列（50個）")]
    public GameObject[] kanaCubes;
    
    [Header("カタカナ設定")]
    [Tooltip("各Cubeに割り当てるカタカナ文字を設定")]
    [SerializeField]
    private string[] kanaValues = {
        // 基本のカタカナ50音
        "ア", "イ", "ウ", "エ", "オ",
        "カ", "キ", "ク", "ケ", "コ",
        "サ", "シ", "ス", "セ", "ソ",
        "タ", "チ", "ツ", "テ", "ト",
        "ナ", "ニ", "ヌ", "ネ", "ノ",
        "ハ", "ヒ", "フ", "ヘ", "ホ",
        "マ", "ミ", "ム", "メ", "モ",
        "ヤ", "ユ", "ヨ", "ラ", "リ",
        "ル", "レ", "ロ", "ワ", "ヲ",
        "ン", "ー", "ッ", "ャ", "ュ",
        "ョ", "゛", "゜", "確定", "削除", "クリア", "スペース"
    };
    
    private string inputPassword = "";
    private bool isPasswordCorrect = false;
    private string pendingKana = ""; // 濁点・半濁点待ちの文字
    
    [Header("デバッグ設定")]
    [Tooltip("デバッグ情報を表示するか")]
    public bool showDebugInfo = true;

    void Start()
    {
        // 配列の長さチェック
        if (kanaValues.Length != kanaCubes.Length)
        {
            Debug.LogWarning($"[KanaPassword] kanaValuesの長さ({kanaValues.Length})とkanaCubesの長さ({kanaCubes.Length})が一致しません。");
        }
        
        // 各Cubeにカタカナ文字を割り当て
        for (int i = 0; i < kanaCubes.Length; i++)
        {
            string assignedKana = "";
            
            if (i < kanaValues.Length && !string.IsNullOrEmpty(kanaValues[i]))
            {
                assignedKana = kanaValues[i];
            }
            else
            {
                assignedKana = "？"; // デフォルト値
            }

            // KanaCubeスクリプトを取得して設定
            KanaCube cubeScript = kanaCubes[i].GetComponent<KanaCube>();
            if (cubeScript != null)
            {
                cubeScript.assignedKana = assignedKana;
                cubeScript.parentScript = this;
                
                if (showDebugInfo)
                {
                    Debug.Log($"[KanaPassword] Cube{i}: '{assignedKana}' を設定しました。");
                }
            }
            else
            {
                Debug.LogWarning($"[KanaPassword] KanaCube script is missing on: {kanaCubes[i].name}");
            }
        }
        
        // 初期表示
        UpdateDisplay();
    }

    /// <summary>
    /// カタカナ文字が入力された時の処理
    /// </summary>
    public void AppendKana(string kana)
    {
        if (kana == "確定")
        {
            CheckPassword();
        }
        else if (kana == "削除")
        {
            // 最後の文字を削除
            if (inputPassword.Length > 0)
            {
                inputPassword = inputPassword.Substring(0, inputPassword.Length - 1);
            }
            pendingKana = ""; // 待機中の文字もクリア
        }
        else if (kana == "クリア")
        {
            // 全ての入力をクリア
            inputPassword = "";
            pendingKana = "";
        }
        else if (kana == "スペース")
        {
            // スペースを追加
            if (!string.IsNullOrEmpty(pendingKana))
            {
                inputPassword += pendingKana;
                pendingKana = "";
            }
            inputPassword += " ";
        }
        else if (kana == "゛") // 濁点
        {
            if (!string.IsNullOrEmpty(pendingKana))
            {
                // 待機中の文字に濁点を付ける
                string dakutenKana = ConvertToDakuten(pendingKana);
                if (!string.IsNullOrEmpty(dakutenKana))
                {
                    inputPassword += dakutenKana;
                    pendingKana = "";
                }
                else
                {
                    // 濁点が付けられない場合は通常の文字を追加
                    inputPassword += pendingKana;
                    pendingKana = "";
                }
            }
        }
        else if (kana == "゜") // 半濁点
        {
            if (!string.IsNullOrEmpty(pendingKana))
            {
                // 待機中の文字に半濁点を付ける
                string handakutenKana = ConvertToHandakuten(pendingKana);
                if (!string.IsNullOrEmpty(handakutenKana))
                {
                    inputPassword += handakutenKana;
                    pendingKana = "";
                }
                else
                {
                    // 半濁点が付けられない場合は通常の文字を追加
                    inputPassword += pendingKana;
                    pendingKana = "";
                }
            }
        }
        else
        {
            // 通常のカタカナ文字
            if (!string.IsNullOrEmpty(pendingKana))
            {
                // 前の文字が待機中なら先に追加
                inputPassword += pendingKana;
            }
            pendingKana = kana; // 新しい文字を待機状態にする
        }

        UpdateDisplay();
    }

    /// <summary>
    /// 濁点に変換
    /// </summary>
    private string ConvertToDakuten(string kana)
    {
        switch (kana)
        {
            case "カ": return "ガ";
            case "キ": return "ギ";
            case "ク": return "グ";
            case "ケ": return "ゲ";
            case "コ": return "ゴ";
            case "サ": return "ザ";
            case "シ": return "ジ";
            case "ス": return "ズ";
            case "セ": return "ゼ";
            case "ソ": return "ゾ";
            case "タ": return "ダ";
            case "チ": return "ヂ";
            case "ツ": return "ヅ";
            case "テ": return "デ";
            case "ト": return "ド";
            case "ハ": return "バ";
            case "ヒ": return "ビ";
            case "フ": return "ブ";
            case "ヘ": return "ベ";
            case "ホ": return "ボ";
            default: return ""; // 濁点が付けられない文字
        }
    }

    /// <summary>
    /// 半濁点に変換
    /// </summary>
    private string ConvertToHandakuten(string kana)
    {
        switch (kana)
        {
            case "ハ": return "パ";
            case "ヒ": return "ピ";
            case "フ": return "プ";
            case "ヘ": return "ペ";
            case "ホ": return "ポ";
            default: return ""; // 半濁点が付けられない文字
        }
    }

    /// <summary>
    /// パスワードをチェックする
    /// </summary>
    public void CheckPassword()
    {
        // 待機中の文字があれば先に追加
        if (!string.IsNullOrEmpty(pendingKana))
        {
            inputPassword += pendingKana;
            pendingKana = "";
        }

        if (inputPassword == correctPassword)
        {
            // 正しいパスワードが入力された
            isPasswordCorrect = true;
            
            if (displayText != null)
            {
                displayText.text = "正解！";
                displayText.color = Color.green;
            }
            
            // 外部スクリプトにTrueを送信
            SendSuccessToExternalScript();
            
            if (showDebugInfo)
            {
                Debug.Log("[KanaPassword] 正しいパスワード！");
            }
        }
        else
        {
            // パスワードが間違っていた
            isPasswordCorrect = false;
            
            if (displayText != null)
            {
                displayText.text = "不正解...";
                displayText.color = Color.red;
            }
            
            if (showDebugInfo)
            {
                Debug.Log($"[KanaPassword] 間違ったパスワード: {inputPassword} (正解: {correctPassword})");
            }
            
            // 2秒後に入力をクリア
            SendCustomEventDelayedSeconds(nameof(ClearInputDelayed), 2.0f);
        }
    }

    /// <summary>
    /// 外部スクリプトに成功を通知
    /// </summary>
    private void SendSuccessToExternalScript()
    {
        if (targetScript != null && !string.IsNullOrEmpty(targetMethodName))
        {
            targetScript.SendCustomEvent(targetMethodName);
            
            if (showDebugInfo)
            {
                Debug.Log($"[KanaPassword] {targetScript.name}の{targetMethodName}を呼び出しました。");
            }
        }
    }

    /// <summary>
    /// 表示を更新
    /// </summary>
    private void UpdateDisplay()
    {
        if (displayText != null)
        {
            string displayText_content = inputPassword;
            if (!string.IsNullOrEmpty(pendingKana))
            {
                displayText_content += pendingKana + "?"; // 待機中の文字を表示
            }
            displayText.text = displayText_content;
            displayText.color = Color.white;
        }
    }

    /// <summary>
    /// 遅延実行用のクリア処理
    /// </summary>
    public void ClearInputDelayed()
    {
        inputPassword = "";
        pendingKana = "";
        UpdateDisplay();
    }

    #region 外部から呼び出せるメソッド

    /// <summary>
    /// パスワードが正解かどうかを取得
    /// </summary>
    public bool IsPasswordCorrect()
    {
        return isPasswordCorrect;
    }

    /// <summary>
    /// 正解のパスワードを変更
    /// </summary>
    public void SetCorrectPassword(string newPassword)
    {
        correctPassword = newPassword;
        
        if (showDebugInfo)
        {
            Debug.Log($"[KanaPassword] パスワードを'{newPassword}'に変更しました。");
        }
    }

    /// <summary>
    /// 現在の入力をクリア
    /// </summary>
    public void ClearInput()
    {
        inputPassword = "";
        pendingKana = "";
        isPasswordCorrect = false;
        UpdateDisplay();
        
        if (showDebugInfo)
        {
            Debug.Log("[KanaPassword] 入力をクリアしました。");
        }
    }

    /// <summary>
    /// 指定したCubeのカタカナを変更
    /// </summary>
    public void ChangeKanaValue(int cubeIndex, string newKana)
    {
        if (cubeIndex < 0 || cubeIndex >= kanaValues.Length)
        {
            Debug.LogWarning($"[KanaPassword] 無効なCubeインデックス: {cubeIndex}");
            return;
        }
        
        kanaValues[cubeIndex] = newKana;
        
        // 対応するKanaCubeの値も更新
        if (cubeIndex < kanaCubes.Length)
        {
            KanaCube cubeScript = kanaCubes[cubeIndex].GetComponent<KanaCube>();
            if (cubeScript != null)
            {
                cubeScript.assignedKana = newKana;
                
                if (showDebugInfo)
                {
                    Debug.Log($"[KanaPassword] Cube{cubeIndex}の値を'{newKana}'に変更しました。");
                }
            }
        }
    }

    /// <summary>
    /// 現在の入力内容を取得
    /// </summary>
    public string GetCurrentInput()
    {
        return inputPassword;
    }

    /// <summary>
    /// 正解のパスワードを取得
    /// </summary>
    public string GetCorrectPassword()
    {
        return correctPassword;
    }

    #endregion
}
