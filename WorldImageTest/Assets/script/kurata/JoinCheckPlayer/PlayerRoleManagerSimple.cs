using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

public class PlayerRoleManagerSimple : UdonSharpBehaviour
{
    [Header("Role Settings")]
    public string[] roleNames = { "Player1", "Player2" };

    [Header("UI Settings")]
    public GameObject roleDisplayPrefab; // プレハブ必須
    public float uiHeightOffset = 2.0f; // プレイヤーの頭上の高さ

    [Header("Debug Settings")]
    public bool enableDebugLog = true;

    // 同期変数
    [UdonSynced] private string[] playerRoles = new string[0];
    [UdonSynced] private int[] playerIds = new int[0];
    [UdonSynced] private string[] playerNames = new string[0];
    [UdonSynced] private int roleAssignmentCount = 0;

    // ローカル変数
    private GameObject[] roleDisplayObjects;
    private VRCPlayerApi[] trackedPlayers;
    private bool isInitialized = false;

    void Start()
    {
        InitializeSystem();
    }

    void Update()
    {
        if (!isInitialized) return;

        // プレイヤーの位置に合わせてUI位置を更新
        UpdateRoleDisplayPositions();

        // 新しいプレイヤーをチェック
        CheckForNewPlayers();
    }

    private void InitializeSystem()
    {
        if (roleDisplayPrefab == null)
        {
            Debug.LogError("[PlayerRoleManager] Role Display Prefab is required!");
            return;
        }

        // 配列初期化
        playerRoles = new string[roleNames.Length];
        playerIds = new int[roleNames.Length];
        playerNames = new string[roleNames.Length];
        roleDisplayObjects = new GameObject[roleNames.Length];
        trackedPlayers = new VRCPlayerApi[roleNames.Length];

        // 初期化
        for (int i = 0; i < roleNames.Length; i++)
        {
            playerRoles[i] = "";
            playerIds[i] = -1;
            playerNames[i] = "";
        }

        CreateRoleDisplayUI();
        isInitialized = true;

        if (enableDebugLog)
            Debug.Log($"[PlayerRoleManager] System initialized for {roleNames.Length} roles");
    }

    private void CreateRoleDisplayUI()
    {
        for (int i = 0; i < roleNames.Length; i++)
        {
            // プレハブからインスタンス作成
            roleDisplayObjects[i] = Instantiate(roleDisplayPrefab);
            roleDisplayObjects[i].transform.SetParent(transform);

            // UI要素の初期設定
            TextMeshProUGUI textComponent = roleDisplayObjects[i].GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = $"{roleNames[i]}: Waiting...";
            }

            // 初期位置設定
            roleDisplayObjects[i].transform.position = Vector3.up * (i * 0.5f + 3f);
            roleDisplayObjects[i].SetActive(false);
        }
    }

    private void CheckForNewPlayers()
    {
        if (!Networking.IsMaster) return;

        VRCPlayerApi[] currentPlayers = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
        VRCPlayerApi.GetPlayers(currentPlayers);

        foreach (VRCPlayerApi player in currentPlayers)
        {
            if (player == null) continue;

            // 既に登録済みかチェック
            bool isAlreadyAssigned = false;
            for (int i = 0; i < playerIds.Length; i++)
            {
                if (playerIds[i] == player.playerId)
                {
                    isAlreadyAssigned = true;
                    break;
                }
            }

            // 新しいプレイヤーにロールを割り当て
            if (!isAlreadyAssigned && roleAssignmentCount < roleNames.Length)
            {
                AssignRoleToPlayer(player, roleAssignmentCount);
                roleAssignmentCount++;
                RequestSerialization();

                if (enableDebugLog)
                    Debug.Log($"[PlayerRoleManager] Assigned {roleNames[roleAssignmentCount - 1]} to {player.displayName}");
            }
        }
    }

    private void AssignRoleToPlayer(VRCPlayerApi player, int roleIndex)
    {
        if (roleIndex >= 0 && roleIndex < roleNames.Length)
        {
            playerIds[roleIndex] = player.playerId;
            playerNames[roleIndex] = player.displayName;
            playerRoles[roleIndex] = roleNames[roleIndex];
            trackedPlayers[roleIndex] = player;
        }
    }

    private void UpdateRoleDisplayPositions()
    {
        for (int i = 0; i < trackedPlayers.Length; i++)
        {
            if (trackedPlayers[i] != null && trackedPlayers[i].IsValid() && roleDisplayObjects[i] != null)
            {
                // プレイヤーの頭上にUI配置
                Vector3 playerHeadPos = trackedPlayers[i].GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;
                roleDisplayObjects[i].transform.position = playerHeadPos + Vector3.up * uiHeightOffset;

                // カメラの方向を向くように回転
                Vector3 cameraPos = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;
                Vector3 lookDirection = cameraPos - roleDisplayObjects[i].transform.position;
                lookDirection.y = 0; // Y軸回転のみ
                if (lookDirection.magnitude > 0.1f)
                {
                    roleDisplayObjects[i].transform.rotation = Quaternion.LookRotation(lookDirection);
                }

                roleDisplayObjects[i].SetActive(true);

                // テキスト更新
                UpdateRoleDisplayText(i);
            }
            else if (roleDisplayObjects[i] != null)
            {
                roleDisplayObjects[i].SetActive(false);
            }
        }
    }

    private void UpdateRoleDisplayText(int index)
    {
        if (roleDisplayObjects[index] == null) return;

        TextMeshProUGUI textComponent = roleDisplayObjects[index].GetComponentInChildren<TextMeshProUGUI>();
        if (textComponent != null)
        {
            string displayText = "";

            if (!string.IsNullOrEmpty(playerNames[index]))
            {
                displayText = $"{playerRoles[index]}\n{playerNames[index]}";

                // Application.isEditor はUdonで使えないため削除
                // 必要なら代わりに enableDebugLog で制御もできる
                if (enableDebugLog)
                {
                    displayText += $"\nID: {playerIds[index]}";
                }
            }
            else
            {
                displayText = $"{roleNames[index]}: Waiting...";
            }

            textComponent.text = displayText;
        }
    }

    public override void OnDeserialization()
    {
        // 同期データが更新された時の処理
        RefreshTrackedPlayers();
        UpdateAllDisplays();

        if (enableDebugLog)
        {
            Debug.Log($"[PlayerRoleManager] Synced data received. Assignment count: {roleAssignmentCount}");
        }
    }

    private void RefreshTrackedPlayers()
    {
        for (int i = 0; i < playerIds.Length; i++)
        {
            if (playerIds[i] != -1)
            {
                trackedPlayers[i] = VRCPlayerApi.GetPlayerById(playerIds[i]);
            }
        }
    }

    private void UpdateAllDisplays()
    {
        for (int i = 0; i < roleDisplayObjects.Length; i++)
        {
            UpdateRoleDisplayText(i);
        }
    }

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        if (enableDebugLog)
            Debug.Log($"[PlayerRoleManager] Player joined: {player.displayName} (ID: {player.playerId})");

        // マスターの場合のみロール割り当てを実行
        if (Networking.IsMaster)
        {
            SendCustomEventDelayedSeconds(nameof(DelayedPlayerCheck), 1.0f);
        }
    }

    public void DelayedPlayerCheck()
    {
        CheckForNewPlayers();
    }

    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        if (enableDebugLog)
            Debug.Log($"[PlayerRoleManager] Player left: {player.displayName} (ID: {player.playerId})");

        // プレイヤーが退室した場合の処理
        for (int i = 0; i < playerIds.Length; i++)
        {
            if (playerIds[i] == player.playerId)
            {
                if (roleDisplayObjects[i] != null)
                {
                    roleDisplayObjects[i].SetActive(false);
                }
                break;
            }
        }
    }

    // 公開メソッド：外部からロール情報を取得
    public string GetPlayerRole(VRCPlayerApi player)
    {
        for (int i = 0; i < playerIds.Length; i++)
        {
            if (playerIds[i] == player.playerId)
            {
                return playerRoles[i];
            }
        }
        return "";
    }

    //公開メソッド : つけたロールを通知
    public string GetRoleNameByPlayerId(int playerId)
    {
        for (int i = 0; i < playerIds.Length; i++)
        {
            if (playerIds[i] == playerId)
            {
                return playerRoles[i];
            }
        }
        return null;
    }
    public bool IsPlayer1(VRCPlayerApi player)
    {
        return GetPlayerRole(player) == "Player1";
    }

    public bool IsPlayer2(VRCPlayerApi player)
    {
        return GetPlayerRole(player) == "Player2";
    }
}