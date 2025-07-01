using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;


namespace script.Hayato
{
    public class VcController : UdonSharpBehaviour
    {
        // ★変更点: インスペクターからロビーチャンネルを指定できるようにする
    [Header("初期設定")]
    [Tooltip("プレイヤーがワールド参加時に自動で割り当てられるチャンネル番号 (1以上を推奨)")]
    public int lobbyChannel = 1;

    // [UdonSynced] をつけることで、この変数は全プレイヤーで同期されます。
    // 配列のインデックスはプレイヤーIDに対応します。
    [UdonSynced]
    private int[] _playerChannels;

    private VRCPlayerApi _localPlayer;

    void Start()
    {
        _localPlayer = Networking.LocalPlayer;

        // ワールドの最大人数より大きいサイズで配列を初期化します。
        // マスタークライアント（ワールドに最初にいた人）だけが初期化処理を行います。
        if (Networking.IsMaster)
        {
            // VRChatのワールドの最大人数は現状80人なので、少し余裕を持たせて81にします。
            _playerChannels = new int[81]; 
            
            // ★変更点: 全員を指定されたロビーチャンネルに設定
            for (int i = 0; i < _playerChannels.Length; i++)
            {
                _playerChannels[i] = lobbyChannel;
            }
            // 同期リクエスト
            RequestSerialization();
        }

        // 念のため、少し待ってから自分のボイスを更新
        SendCustomEventDelayedSeconds(nameof(UpdateAllPlayerVoices), 3.0f);
    }

    // プレイヤーがワールドに参加したときの処理
    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        if (Networking.IsMaster)
        {
            // ★変更点: 新しく入った人を指定されたロビーチャンネルに割り当て
            _playerChannels[player.playerId] = lobbyChannel; 
            RequestSerialization();
        }
        // 全員のボイス設定を更新
        UpdateAllPlayerVoices();
    }

    // プレイヤーがワールドから退出したときの処理
    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        // マスターが情報をクリアする（必須ではないが、綺麗な状態を保つため）
        if (Networking.IsMaster)
        {
            if(player != null)
            {
                // ★変更点: 退出したプレイヤーのチャンネル情報をロビーチャンネルに戻す
                // (誰かが再利用する可能性を考慮)
                _playerChannels[player.playerId] = lobbyChannel;
                RequestSerialization();
            }
        }
    }

    // 同期変数が他のプレイヤーから送られてきたときに実行される処理
    public override void OnDeserialization()
    {
        // 誰かのチャンネル情報が変わったので、自分のボイス設定を更新する
        UpdateAllPlayerVoices();
    }

    // チャンネルを切り替えるためのメイン関数（ボタンから呼び出す）
    public void SwitchChannel(int channelId)
    {
        // オーナーシップを自分に設定してから同期変数を変更する
        Networking.SetOwner(_localPlayer, this.gameObject);

        _playerChannels[_localPlayer.playerId] = channelId;

        // 変更を他のプレイヤーに通知
        RequestSerialization();

        // 自分のボイス設定を即時更新
        UpdateAllPlayerVoices();
    }

    // 全プレイヤーのボイス設定を更新する関数
    public void UpdateAllPlayerVoices()
    {
        if (_playerChannels == null) return; // 初期化前なら何もしない

        int myPlayerId = _localPlayer.playerId;
        int myChannel = _playerChannels[myPlayerId];

        // ワールドにいる全プレイヤーを取得
        VRCPlayerApi[] players = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
        VRCPlayerApi.GetPlayers(players);
        
        foreach (VRCPlayerApi targetPlayer in players)
        {
            // プレイヤーが有効で、自分自身ではない場合のみ処理
            if (targetPlayer == null || !targetPlayer.IsValid() || targetPlayer.isLocal)
            {
                continue;
            }

            int targetPlayerId = targetPlayer.playerId;
            int targetChannel = _playerChannels[targetPlayerId];

            // --- ボイス制御のロジック (変更なし) ---
            // 条件：
            // 1. 自分と相手が同じプライベートチャンネルにいる (myChannel != 0)
            // 2. 自分と相手が同じ全体VCチャンネル(0)にいる
            bool shouldHear = (myChannel != 0 && myChannel == targetChannel) || (myChannel == 0 && targetChannel == 0);

            if (shouldHear)
            {
                // 声が聞こえるように設定 (VRChatのデフォルト値に近い設定)
                targetPlayer.SetVoiceDistanceNear(0);
                targetPlayer.SetVoiceDistanceFar(25);
                targetPlayer.SetVoiceGain(15);
            }
            else
            {
                // 声が聞こえないように設定 (聞こえる最大距離を0にする)
                targetPlayer.SetVoiceDistanceNear(0);
                targetPlayer.SetVoiceDistanceFar(0);
                targetPlayer.SetVoiceGain(15);
            }
        }
    }
        
    }
}