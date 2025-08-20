
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class SEManager : UdonSharpBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource seSource;
    
    [Header("BGM Clips")]
    [SerializeField] private AudioClip[] bgmClips;
    
    [Header("SE Clips")]
    [SerializeField] private AudioClip[] seClips;
    
    [Header("Volume Settings")]
    [SerializeField, Range(0f, 1f)] private float bgmVolume = 0.5f;
    [SerializeField, Range(0f, 1f)] private float seVolume = 0.7f;
    
    [Header("BGM Settings")]
    [SerializeField] private bool loopBGM = true;
    [SerializeField] private bool fadeInBGM = true;
    [SerializeField] private float fadeInDuration = 1f;
    
    private int currentBGMIndex = -1;
    private bool isFading = false;
    private float fadeStartTime;
    private float fadeStartVolume;
    private float targetVolume;
    
    void Start()
    {
        InitializeAudioSources();
    }
    
    void Update()
    {
        HandleBGM();
    }
    
    private void InitializeAudioSources()
    {
        // AudioSourceが設定されていない場合は警告を表示
        if (bgmSource == null)
        {
            Debug.LogWarning("BGM AudioSource is not assigned! Please assign it in the inspector.");
            return;
        }
        
        if (seSource == null)
        {
            Debug.LogWarning("SE AudioSource is not assigned! Please assign it in the inspector.");
            return;
        }
        
        // 初期設定
        bgmSource.playOnAwake = false;
        bgmSource.loop = loopBGM;
        bgmSource.volume = 0f; // 初期は無音
        
        seSource.playOnAwake = false;
        seSource.loop = false;
        seSource.volume = seVolume;
    }
    
    private void HandleBGM()
    {
        if (!isFading || bgmSource == null) return;
        
        float elapsedTime = Time.time - fadeStartTime;
        float progress = elapsedTime / fadeInDuration;
        
        if (progress >= 1f)
        {
            // フェード完了
            bgmSource.volume = targetVolume;
            isFading = false;
        }
        else
        {
            // フェード中
            bgmSource.volume = Mathf.Lerp(fadeStartVolume, targetVolume, progress);
        }
    }
    
    #region BGM Methods
    
    /// <summary>
    /// BGMを再生する
    /// </summary>
    /// <param name="clipIndex">BGMクリップのインデックス</param>
    public void PlayBGM(int clipIndex)
    {
        if (bgmSource == null)
        {
            Debug.LogWarning("BGM AudioSource is not assigned!");
            return;
        }
        
        if (clipIndex < 0 || clipIndex >= bgmClips.Length || bgmClips[clipIndex] == null)
        {
            Debug.LogWarning($"Invalid BGM clip index: {clipIndex}");
            return;
        }
        
        currentBGMIndex = clipIndex;
        bgmSource.clip = bgmClips[clipIndex];
        
        if (fadeInBGM)
        {
            StartFadeIn();
        }
        else
        {
            bgmSource.volume = bgmVolume;
        }
        
        bgmSource.Play();
    }
    
    /// <summary>
    /// 現在のBGMを停止する
    /// </summary>
    public void StopBGM()
    {
        if (bgmSource == null) return;
        
        bgmSource.Stop();
        currentBGMIndex = -1;
        isFading = false;
    }
    
    /// <summary>
    /// BGMを一時停止する
    /// </summary>
    public void PauseBGM()
    {
        if (bgmSource == null) return;
        
        bgmSource.Pause();
    }
    
    /// <summary>
    /// BGMを再開する
    /// </summary>
    public void ResumeBGM()
    {
        if (bgmSource == null) return;
        
        bgmSource.UnPause();
    }
    
    /// <summary>
    /// BGMの音量を設定する
    /// </summary>
    /// <param name="volume">音量 (0.0 - 1.0)</param>
    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        if (!isFading && bgmSource != null)
        {
            bgmSource.volume = bgmVolume;
        }
    }
    
    /// <summary>
    /// BGMをフェードアウトする
    /// </summary>
    /// <param name="duration">フェード時間</param>
    public void FadeOutBGM(float duration)
    {
        if (bgmSource == null || !bgmSource.isPlaying) return;
        
        StartFadeOut(duration);
    }
    
    private void StartFadeIn()
    {
        isFading = true;
        fadeStartTime = Time.time;
        fadeStartVolume = 0f;
        targetVolume = bgmVolume;
    }
    
    private void StartFadeOut(float duration)
    {
        if (bgmSource == null) return;
        
        isFading = true;
        fadeStartTime = Time.time;
        fadeStartVolume = bgmSource.volume;
        targetVolume = 0f;
        fadeInDuration = duration;
    }
    
    #endregion
    
    #region SE Methods
    
    /// <summary>
    /// SEを再生する
    /// </summary>
    /// <param name="clipIndex">SEクリップのインデックス</param>
    public void PlaySE(int clipIndex)
    {
        if (seSource == null)
        {
            Debug.LogWarning("SE AudioSource is not assigned!");
            return;
        }
        
        if (clipIndex < 0 || clipIndex >= seClips.Length || seClips[clipIndex] == null)
        {
            Debug.LogWarning($"Invalid SE clip index: {clipIndex}");
            return;
        }
        
        seSource.PlayOneShot(seClips[clipIndex], seVolume);
    }
    
    /// <summary>
    /// SEを再生する（音量指定）
    /// </summary>
    /// <param name="clipIndex">SEクリップのインデックス</param>
    /// <param name="volume">音量 (0.0 - 1.0)</param>
    public void PlaySE(int clipIndex, float volume)
    {
        if (seSource == null)
        {
            Debug.LogWarning("SE AudioSource is not assigned!");
            return;
        }
        
        if (clipIndex < 0 || clipIndex >= seClips.Length || seClips[clipIndex] == null)
        {
            Debug.LogWarning($"Invalid SE clip index: {clipIndex}");
            return;
        }
        
        float adjustedVolume = Mathf.Clamp01(volume) * seVolume;
        seSource.PlayOneShot(seClips[clipIndex], adjustedVolume);
    }
    
    /// <summary>
    /// SEの音量を設定する
    /// </summary>
    /// <param name="volume">音量 (0.0 - 1.0)</param>
    public void SetSEVolume(float volume)
    {
        seVolume = Mathf.Clamp01(volume);
        if (seSource != null)
        {
            seSource.volume = seVolume;
        }
    }
    
    /// <summary>
    /// 現在再生中のSEを停止する
    /// </summary>
    public void StopSE()
    {
        if (seSource == null) return;
        
        seSource.Stop();
    }
    
    #endregion
    
    #region Utility Methods
    
    /// <summary>
    /// 現在再生中のBGMのインデックスを取得
    /// </summary>
    /// <returns>BGMインデックス（-1は再生中でない）</returns>
    public int GetCurrentBGMIndex()
    {
        return currentBGMIndex;
    }
    
    /// <summary>
    /// BGMが再生中かどうかを取得
    /// </summary>
    /// <returns>再生中ならtrue</returns>
    public bool IsBGMPlaying()
    {
        return bgmSource != null && bgmSource.isPlaying;
    }
    
    /// <summary>
    /// SEが再生中かどうかを取得
    /// </summary>
    /// <returns>再生中ならtrue</returns>
    public bool IsSEPlaying()
    {
        return seSource != null && seSource.isPlaying;
    }
    
    /// <summary>
    /// BGMの総数を取得
    /// </summary>
    /// <returns>BGMクリップの数</returns>
    public int GetBGMCount()
    {
        return bgmClips.Length;
    }
    
    /// <summary>
    /// SEの総数を取得
    /// </summary>
    /// <returns>SEクリップの数</returns>
    public int GetSECount()
    {
        return seClips.Length;
    }
    
    #endregion
}
