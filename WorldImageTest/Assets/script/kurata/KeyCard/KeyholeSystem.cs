using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

/// <summary>
/// 鍵のサイズが一致した場合に扉を開ける
/// </summary>
public class KeyholeSystem : UdonSharpBehaviour
{
    [Header("鍵穴設定")]
    [Tooltip("鍵穴のサイズ（小・中・大）")]
    public KeySize requiredKeySize = KeySize.Medium;
    
    [Tooltip("鍵穴の名前")]
    public string keyholeName = "鍵穴";
    
    [Header("扉設定")]
    [Tooltip("左側の扉")]
    public Transform leftDoor;
    [Tooltip("右側の扉")]
    public Transform rightDoor;
    
    [Header("扉の開き方")]
    [Tooltip("左扉の開く方向")]
    public Vector3 leftOpenOffset = new Vector3(-1f, 0f, 0f);
    [Tooltip("右扉の開く方向")]
    public Vector3 rightOpenOffset = new Vector3(1f, 0f, 0f);
    [Tooltip("扉を開く速度")]
    public float doorOpenSpeed = 2f;
    
    [Header("UI設定")]
    [Tooltip("状態表示用テキスト")]
    public TMP_Text statusText;
    
    [Header("視覚的設定")]
    [Tooltip("鍵穴の色")]
    public Color keyholeColor = Color.gray;
    [Tooltip("正しい鍵が挿入された時の色")]
    public Color correctKeyColor = Color.green;
    [Tooltip("間違った鍵が挿入された時の色")]
    public Color wrongKeyColor = Color.red;
    
    // 内部変数だからいじらんといて
    private Vector3 leftClosedPos;
    private Vector3 rightClosedPos;
    private bool doorOpened = false;
    private Renderer keyholeRenderer;
    private bool isKeyInserted = false;
    
    void Start()
    {
        // 扉の初期位置用だからあとで好きなようにしてもろて
        if (leftDoor != null) leftClosedPos = leftDoor.position;
        if (rightDoor != null) rightClosedPos = rightDoor.position;
        
        // 鍵穴の色を設定
        keyholeRenderer = GetComponent<Renderer>();
        if (keyholeRenderer != null)
        {
            keyholeRenderer.material.color = keyholeColor;
        }
        
        // Colliderの設定用
        CheckColliderSetup();
        
        UpdateUI();
    }
    
    /// <summary>
    /// Colliderの設定を確認
    /// </summary>
    private void CheckColliderSetup()
    {
        Collider keyholeCollider = GetComponent<Collider>();
        if (keyholeCollider == null)
        {
            Debug.LogError($"[KeyholeSystem] {keyholeName}: Colliderが見つかりません。鍵との接触検出ができません。");
        }
        else if (!keyholeCollider.isTrigger)
        {
            Debug.LogWarning($"[KeyholeSystem] {keyholeName}: ColliderがTriggerではありません。IsTriggerをtrueに設定してください。");
        }
        else
        {
            Debug.Log($"[KeyholeSystem] {keyholeName}: Collider設定確認 - IsTrigger: {keyholeCollider.isTrigger}");
        }
        
        // 扉の設定確認
        if (leftDoor == null && rightDoor == null)
        {
            Debug.LogError($"[KeyholeSystem] {keyholeName}: 扉が設定されていません。");
        }
        else
        {
            Debug.Log($"[KeyholeSystem] {keyholeName}: 扉設定確認 - 左扉: {(leftDoor != null ? "設定済み" : "未設定")}, 右扉: {(rightDoor != null ? "設定済み" : "未設定")}");
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[KeyholeSystem] Trigger Enter: {other.name}");
        
        var key = other.GetComponent<PhysicalKey>();
        if (key == null) 
        {
            Debug.Log($"[KeyholeSystem] PhysicalKey component not found on {other.name}");
            return;
        }
        
        // 鍵が挿入された
        isKeyInserted = true;
        KeySize insertedKeySize = key.GetKeySize();
        
        Debug.Log($"[KeyholeSystem] 鍵が挿入されました: {key.GetKeyInfo()}");
        Debug.Log($"[KeyholeSystem] 必要なサイズ: {GetKeySizeString(requiredKeySize)}, 挿入されたサイズ: {GetKeySizeString(insertedKeySize)}");
        
        // 鍵のサイズチェック
        if (insertedKeySize == requiredKeySize)
        {
            // 正
            Debug.Log($"[KeyholeSystem] 正しい鍵です！扉を開きます。");
            SetKeyholeColor(correctKeyColor);
            OpenDoor();
        }
        else
        {
            // 間違い
            Debug.Log($"[KeyholeSystem] 間違った鍵です。必要なサイズ: {GetKeySizeString(requiredKeySize)}");
            SetKeyholeColor(wrongKeyColor);
            UpdateUI();
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        var key = other.GetComponent<PhysicalKey>();
        if (key == null) return;
        
        // 鍵が抜かれた
        isKeyInserted = false;
        SetKeyholeColor(keyholeColor);
        UpdateUI();
    }
    
    /// <summary>
    /// 扉を開く
    /// </summary>
    private void OpenDoor()
    {
        if (doorOpened) return;
        
        doorOpened = true;
        Debug.Log("扉を開きます");
        
        // 扉を開く処理を開始
        SendCustomEventDelayedSeconds(nameof(OpenDoors), 0.1f);
    }
    
    /// <summary>
    /// 扉を開く処理（既存のCardReaderのやつを再利用）
    /// </summary>
    public void OpenDoors()
    {
        if (leftDoor != null)
        {
            leftDoor.position = Vector3.Lerp(leftDoor.position, leftClosedPos + leftOpenOffset, doorOpenSpeed * Time.deltaTime);
        }
        if (rightDoor != null)
        {
            rightDoor.position = Vector3.Lerp(rightDoor.position, rightClosedPos + rightOpenOffset, doorOpenSpeed * Time.deltaTime);
        }
    }
    
    void Update()
    {
        // 扉が開いている間、継続的に移動
        if (doorOpened)
        {
            if (leftDoor != null)
            {
                leftDoor.position = Vector3.MoveTowards(leftDoor.position, leftClosedPos + leftOpenOffset, doorOpenSpeed * Time.deltaTime);
            }
            if (rightDoor != null)
            {
                rightDoor.position = Vector3.MoveTowards(rightDoor.position, rightClosedPos + rightOpenOffset, doorOpenSpeed * Time.deltaTime);
            }
        }
    }
    
    /// <summary>
    /// 鍵穴の色を設定
    /// </summary>
    /// <param name="color">設定する色</param>
    private void SetKeyholeColor(Color color)
    {
        if (keyholeRenderer != null)
        {
            keyholeRenderer.material.color = color;
        }
    }
    
    /// <summary>
    /// UIを更新用
    /// </summary>
    private void UpdateUI()
    {
        if (statusText == null) return;
        
        if (doorOpened)
        {
            statusText.text = "扉が開きました";
        }
        else if (isKeyInserted)
        {
            statusText.text = $"間違った鍵です\n必要なサイズ: {GetKeySizeString(requiredKeySize)}";
        }
        else
        {
            statusText.text = $"鍵を挿入してください\n必要なサイズ: {GetKeySizeString(requiredKeySize)}";
        }
    }
    
    /// <summary>
    /// 鍵のサイズを文字列で取得
    /// </summary>
    /// <param name="size">鍵のサイズ</param>
    /// <returns>サイズの文字列</returns>
    private string GetKeySizeString(KeySize size)
    {
        switch (size)
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
    /// 鍵穴の情報を取得（デバッグ用）
    /// </summary>
    /// <returns>鍵穴の情報文字列</returns>
    public string GetKeyholeInfo()
    {
        return $"{keyholeName} (必要なサイズ: {GetKeySizeString(requiredKeySize)})";
    }
    
    #region 外部から呼び出せるメソッド
    
    /// <summary>
    /// 扉を強制的に開く（デバッグ用）
    /// </summary>
    public void ForceOpenDoor()
    {
        Debug.Log($"[KeyholeSystem] {keyholeName}: 強制開放を実行");
        OpenDoor();
    }
    
    /// <summary>
    /// 扉の状態をリセット（デバッグ用）
    /// </summary>
    public void ResetDoor()
    {
        Debug.Log($"[KeyholeSystem] {keyholeName}: 扉をリセット");
        doorOpened = false;
        isKeyInserted = false;
        SetKeyholeColor(keyholeColor);
        
        if (leftDoor != null) leftDoor.position = leftClosedPos;
        if (rightDoor != null) rightDoor.position = rightClosedPos;
        
        UpdateUI();
    }
    
    /// <summary>
    /// テスト用：鍵のサイズをチェック（デバッグ用）
    /// </summary>
    /// <param name="testKeySize">テストする鍵のサイズ</param>
    public void TestKeySize(KeySize testKeySize)
    {
        Debug.Log($"[KeyholeSystem] {keyholeName}: テスト実行");
        Debug.Log($"[KeyholeSystem] 必要なサイズ: {GetKeySizeString(requiredKeySize)}");
        Debug.Log($"[KeyholeSystem] テストサイズ: {GetKeySizeString(testKeySize)}");
        
        if (testKeySize == requiredKeySize)
        {
            Debug.Log($"[KeyholeSystem] サイズ一致！扉を開きます。");
            OpenDoor();
        }
        else
        {
            Debug.Log($"[KeyholeSystem] サイズ不一致。扉は開きません。");
        }
    }
    
    #endregion
} 