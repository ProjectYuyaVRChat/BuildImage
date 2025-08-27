using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

/// <summary>
/// ライトとテキストの交互表示システム
/// 左右のライトが交互に光り、その下のテキストに文字を1文字ずつ表示
/// </summary>
public class LightTextDisplaySystem : UdonSharpBehaviour
{
    [Header("ライト設定")]
    [Tooltip("左側のライト（Cube）")]
    public GameObject leftLight;
    [Tooltip("右側のライト（Cube）")]
    public GameObject rightLight;
    
    [Header("テキスト設定")]
    [Tooltip("左側のテキスト表示")]
    public TMP_Text leftText;
    [Tooltip("右側のテキスト表示")]
    public TMP_Text rightText;
    
    [Header("表示設定")]
    [Tooltip("表示する文章")]
    [TextArea(3, 5)]
    public string displayText = "こんにちは";
    [Tooltip("文字の表示間隔（秒）")]
    public float displayInterval = 1.0f;
    [Tooltip("ライトが光っている時間（秒）")]
    public float lightDuration = 0.8f;
    
    [Header("ライトの色設定")]
    [Tooltip("ライトがオフの時の色")]
    public Color lightOffColor = Color.gray;
    [Tooltip("ライトがオンの時の色")]
    public Color lightOnColor = Color.yellow;
    
    [Header("デバッグ設定")]
    [Tooltip("デバッグ情報を表示するか")]
    public bool showDebugInfo = true;
    
    // 内部変数
    private Renderer leftLightRenderer;
    private Renderer rightLightRenderer;
    private int currentCharIndex = 0;
    private float timer = 0f;
    private bool isLeftLightOn = false;
    private bool isRightLightOn = false;
    private bool isDisplaying = false;
    
    void Start()
    {
        InitializeSystem();
    }
    
    void Update()
    {
        if (isDisplaying)
        {
            UpdateDisplay();
        }
    }
    
    /// <summary>
    /// システムを初期化
    /// </summary>
    private void InitializeSystem()
    {
        // ライトのRendererを取得
        if (leftLight != null)
        {
            leftLightRenderer = leftLight.GetComponent<Renderer>();
        }
        if (rightLight != null)
        {
            rightLightRenderer = rightLight.GetComponent<Renderer>();
        }
        
        // 初期状態を設定
        SetLightOff();
        ClearText();
        
        if (showDebugInfo)
        {
            Debug.Log($"[LightTextDisplay] システム初期化完了");
            Debug.Log($"[LightTextDisplay] 表示テキスト: {displayText}");
            Debug.Log($"[LightTextDisplay] 文字数: {displayText.Length}");
        }
    }
    
    /// <summary>
    /// 表示を開始
    /// </summary>
    public void StartDisplay()
    {
        if (isDisplaying)
        {
            if (showDebugInfo)
            {
                Debug.Log("[LightTextDisplay] 既に表示中です");
            }
            return;
        }
        
        currentCharIndex = 0;
        timer = 0f;
        isDisplaying = true;
        
        if (showDebugInfo)
        {
            Debug.Log("[LightTextDisplay] 表示を開始します");
        }
        
        // 最初の文字を表示
        DisplayNextCharacter();
    }
    
    /// <summary>
    /// 表示を停止
    /// </summary>
    public void StopDisplay()
    {
        isDisplaying = false;
        SetLightOff();
        ClearText();
        
        if (showDebugInfo)
        {
            Debug.Log("[LightTextDisplay] 表示を停止しました");
        }
    }
    
    /// <summary>
    /// 表示を更新
    /// </summary>
    private void UpdateDisplay()
    {
        timer += Time.deltaTime;
        
        // ライトの点灯時間をチェック
        if (isLeftLightOn && timer >= lightDuration)
        {
            SetLeftLightOff();
        }
        else if (isRightLightOn && timer >= lightDuration)
        {
            SetRightLightOff();
        }
        
        // 次の文字の表示タイミングをチェック
        if (timer >= displayInterval)
        {
            DisplayNextCharacter();
            timer = 0f;
        }
    }
    
    /// <summary>
    /// 次の文字を表示
    /// </summary>
    private void DisplayNextCharacter()
    {
        if (currentCharIndex >= displayText.Length)
        {
            // 文章が終わったら最初から繰り返し
            currentCharIndex = 0;
            if (showDebugInfo)
            {
                Debug.Log("[LightTextDisplay] 文章が終了、最初から繰り返します");
            }
        }
        
        char currentChar = displayText[currentCharIndex];
        
        // 左右交互に表示
        if (currentCharIndex % 2 == 0)
        {
            // 右側のライトを点灯
            SetRightLightOn();
            SetLeftLightOff();
            DisplayText(rightText, currentChar);
            
            if (showDebugInfo)
            {
                Debug.Log($"[LightTextDisplay] 右ライト: '{currentChar}' を表示");
            }
        }
        else
        {
            // 左側のライトを点灯
            SetLeftLightOn();
            SetRightLightOff();
            DisplayText(leftText, currentChar);
            
            if (showDebugInfo)
            {
                Debug.Log($"[LightTextDisplay] 左ライト: '{currentChar}' を表示");
            }
        }
        
        currentCharIndex++;
    }
    
    /// <summary>
    /// 左ライトを点灯
    /// </summary>
    private void SetLeftLightOn()
    {
        if (leftLightRenderer != null)
        {
            leftLightRenderer.material.color = lightOnColor;
            isLeftLightOn = true;
        }
    }
    
    /// <summary>
    /// 右ライトを点灯
    /// </summary>
    private void SetRightLightOn()
    {
        if (rightLightRenderer != null)
        {
            rightLightRenderer.material.color = lightOnColor;
            isRightLightOn = true;
        }
    }
    
    /// <summary>
    /// 左ライトを消灯
    /// </summary>
    private void SetLeftLightOff()
    {
        if (leftLightRenderer != null)
        {
            leftLightRenderer.material.color = lightOffColor;
            isLeftLightOn = false;
        }
    }
    
    /// <summary>
    /// 右ライトを消灯
    /// </summary>
    private void SetRightLightOff()
    {
        if (rightLightRenderer != null)
        {
            rightLightRenderer.material.color = lightOffColor;
            isRightLightOn = false;
        }
    }
    
    /// <summary>
    /// 両方のライトを消灯
    /// </summary>
    private void SetLightOff()
    {
        SetLeftLightOff();
        SetRightLightOff();
    }
    
    /// <summary>
    /// テキストに文字を表示
    /// </summary>
    /// <param name="textComponent">テキストコンポーネント</param>
    /// <param name="character">表示する文字</param>
    private void DisplayText(TMP_Text textComponent, char character)
    {
        if (textComponent != null)
        {
            textComponent.text = character.ToString();
        }
    }
    
    /// <summary>
    /// テキストをクリア
    /// </summary>
    private void ClearText()
    {
        if (leftText != null)
        {
            leftText.text = "";
        }
        if (rightText != null)
        {
            rightText.text = "";
        }
    }
    
    #region 外部から呼び出せるメソッド
    
    /// <summary>
    /// 表示する文章を変更
    /// </summary>
    /// <param name="newText">新しい文章</param>
    public void SetDisplayText(string newText)
    {
        displayText = newText;
        
        if (showDebugInfo)
        {
            Debug.Log($"[LightTextDisplay] 表示テキストを変更: {newText}");
        }
    }
    
    /// <summary>
    /// 表示間隔を変更
    /// </summary>
    /// <param name="newInterval">新しい間隔（秒）</param>
    public void SetDisplayInterval(float newInterval)
    {
        displayInterval = Mathf.Max(0.1f, newInterval);
        
        if (showDebugInfo)
        {
            Debug.Log($"[LightTextDisplay] 表示間隔を変更: {displayInterval}秒");
        }
    }
    
    /// <summary>
    /// ライトの点灯時間を変更
    /// </summary>
    /// <param name="newDuration">新しい点灯時間（秒）</param>
    public void SetLightDuration(float newDuration)
    {
        lightDuration = Mathf.Max(0.1f, newDuration);
        
        if (showDebugInfo)
        {
            Debug.Log($"[LightTextDisplay] ライト点灯時間を変更: {lightDuration}秒");
        }
    }
    
    /// <summary>
    /// 現在の表示状態を取得
    /// </summary>
    /// <returns>表示中ならtrue</returns>
    public bool IsDisplaying()
    {
        return isDisplaying;
    }
    
    /// <summary>
    /// 現在の文字インデックスを取得
    /// </summary>
    /// <returns>現在の文字インデックス</returns>
    public int GetCurrentCharIndex()
    {
        return currentCharIndex;
    }
    
    #endregion
} 