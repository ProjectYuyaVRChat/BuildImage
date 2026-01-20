using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

/// <summary>
/// 物理的な鍵クラス
/// 鍵のサイズを持ち、対応する鍵穴に挿入して扉を開ける
/// </summary>
public class PhysicalKey : UdonSharpBehaviour
{
    [Header("鍵設定")]
    [Tooltip("鍵のサイズ（小・中・大）")]
    public KeySize keySize = KeySize.Medium;
    
    [Tooltip("鍵の名前（デバッグ用）")]
    public string keyName = "鍵";
    
    [Header("視覚的設定")]
    [Tooltip("鍵の色（デバッグ用）")]
    public Color keyColor = Color.yellow;
    
    private Renderer keyRenderer;
    
    void Start()
    {
        // 鍵の色設定
        keyRenderer = GetComponent<Renderer>();
        if (keyRenderer != null)
        {
            keyRenderer.material.color = keyColor;
        }
        
        // 鍵の名前設定
        if (string.IsNullOrEmpty(keyName))
        {
            keyName = $"鍵({GetKeySizeString()})";
        }
        
        // Colliderの設定を確認
        CheckColliderSetup();
    }
    
    /// <summary>
    /// Colliderの設定を確認
    /// </summary>
    private void CheckColliderSetup()
    {
        Collider keyCollider = GetComponent<Collider>();
        if (keyCollider == null)
        {
            Debug.LogWarning($"[PhysicalKey] {keyName}: Colliderが見つかりません。鍵穴との接触検出ができません。");
        }
        else
        {
            Debug.Log($"[PhysicalKey] {keyName}: Collider設定確認 - IsTrigger: {keyCollider.isTrigger}");
        }
        
        // Rigidbodyの確認
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogWarning($"[PhysicalKey] {keyName}: Rigidbodyが見つかりません。物理的な移動ができません。");
        }
    }
    
    /// <summary>
    /// 鍵のサイズを取得
    /// </summary>
    /// <returns>鍵のサイズ</returns>
    public KeySize GetKeySize()
    {
        return keySize;
    }
    
    /// <summary>
    /// 鍵のサイズを文字列で取得
    /// </summary>
    /// <returns>サイズの文字列</returns>
    public string GetKeySizeString()
    {
        switch (keySize)
        {
            case KeySize.Small:
                return "小";
            case KeySize.Medium:
                return "中";
            case KeySize.Large:
                return "大";
            default:
                return "不明";
        }
    }
    
    /// <summary>
    /// 鍵の情報を取得（デバッグ用）
    /// </summary>
    /// <returns>鍵の情報文字列</returns>
    public string GetKeyInfo()
    {
        return $"{keyName} (サイズ: {GetKeySizeString()})";
    }
} 