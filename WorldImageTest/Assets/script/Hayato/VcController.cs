using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class VcController : UdonSharpBehaviour
{
    // ワールドの音声設定をグローバルにするための距離（事実上の無限大）
    private const float GLOBAL_VOICE_DISTANCE = 10000f;

    // ワールドロード時に一度だけ実行される
    /*void Start()
    {
        // 現在インスタンスにいる全プレイヤーのリストを格納するための配列を準備
        VRCPlayerApi players = new VRCPlayerApi;
        // 配列に全プレイヤーの情報を格納
        VRCPlayerApi.GetPlayers(players);

        // 全プレイヤーに対してグローバルボイス設定を適用
        foreach (VRCPlayerApi player in players)
        {
            // playerがnullまたは無効でないことを確認
            if (Utilities.IsValid(player))
            {
                ApplyGlobalVoiceSettings(player);
            }
        }
    }*/

    // 新しいプレイヤーがインスタンスに参加した時に全クライアントで実行される
    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        // 新しく参加したプレイヤーに対してグローバルボイス設定を適用
        if (Utilities.IsValid(player))
        {
            ApplyGlobalVoiceSettings(player);
        }
    }

    //プレイヤーの音声設定をグローバル化するメソッド
    private void ApplyGlobalVoiceSettings(VRCPlayerApi player)
    {
        // Voice（マイク音声）の設定
        player.SetVoiceDistanceFar(GLOBAL_VOICE_DISTANCE);
        player.SetVoiceDistanceNear(GLOBAL_VOICE_DISTANCE);
        player.SetVoiceLowpass(false); // 遠距離のローパスフィルターを無効化
    
        // Avatar Audio（アバターから出る音）の設定
        player.SetAvatarAudioFarRadius(GLOBAL_VOICE_DISTANCE);
        player.SetAvatarAudioNearRadius(GLOBAL_VOICE_DISTANCE);
    }
}