
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class KanaCube : UdonSharpBehaviour
{
    [HideInInspector]
    public string assignedKana; // このCubeに割り当てられたカタカナ文字
    
    [HideInInspector]
    public KanaPassword parentScript; // 親スクリプトへの参照
    
    [Header("視覚的フィードバック")]
    [Tooltip("クリック時の色変更")]
    public Color clickColor = Color.yellow;
    [Tooltip("元の色")]
    public Color originalColor = Color.white;
    
    private Renderer cubeRenderer;
    private bool isClicked = false;

    void Start()
    {
        // レンダラーを取得
        cubeRenderer = GetComponent<Renderer>();
        if (cubeRenderer != null)
        {
            originalColor = cubeRenderer.material.color;
        }
    }

    /// <summary>
    /// Cubeがクリックされた時の処理
    /// </summary>
    public override void Interact()
    {
        if (parentScript != null && !string.IsNullOrEmpty(assignedKana))
        {
            // 親スクリプトのAppendKanaを呼び出す
            parentScript.AppendKana(assignedKana);
            
            // 視覚的フィードバック
            ClickFeedback();
            
            if (parentScript.showDebugInfo)
            {
                Debug.Log($"[KanaCube] '{assignedKana}' がクリックされました。");
            }
        }
        else
        {
            Debug.LogWarning("[KanaCube] parentScriptまたはassignedKanaが設定されていません。");
        }
    }

    /// <summary>
    /// クリック時の視覚的フィードバック
    /// </summary>
    private void ClickFeedback()
    {
        if (cubeRenderer != null)
        {
            // 色を変更
            cubeRenderer.material.color = clickColor;
            isClicked = true;
            
            // 0.1秒後に元の色に戻す
            SendCustomEventDelayedSeconds(nameof(ResetColor), 0.1f);
        }
    }

    /// <summary>
    /// 色を元に戻す（UdonSharp用の遅延実行）
    /// </summary>
    public void ResetColor()
    {
        if (cubeRenderer != null)
        {
            cubeRenderer.material.color = originalColor;
            isClicked = false;
        }
    }

    /// <summary>
    /// 割り当てられたカタカナ文字を取得
    /// </summary>
    public string GetAssignedKana()
    {
        return assignedKana;
    }

    /// <summary>
    /// 割り当てられたカタカナ文字を設定
    /// </summary>
    public void SetAssignedKana(string kana)
    {
        assignedKana = kana;
    }

    /// <summary>
    /// 親スクリプトを設定
    /// </summary>
    public void SetParentScript(KanaPassword script)
    {
        parentScript = script;
    }

    /// <summary>
    /// クリック色を変更
    /// </summary>
    public void SetClickColor(Color color)
    {
        clickColor = color;
    }

    /// <summary>
    /// 元の色を変更
    /// </summary>
    public void SetOriginalColor(Color color)
    {
        originalColor = color;
        if (cubeRenderer != null && !isClicked)
        {
            cubeRenderer.material.color = color;
        }
    }

    /// <summary>
    /// 現在クリック中かどうかを取得
    /// </summary>
    public bool IsClicked()
    {
        return isClicked;
    }
}
