using UnityEngine;

/// <summary>
/// AudioManager — Centralised sound playback for all slot machine audio cues.
/// Uses a simple clip-per-event model; swap AudioClips in the Inspector without
/// touching any code.
///
/// Implements a lazy singleton pattern so other scripts can call
/// AudioManager.Instance.Play(...) without a direct Inspector reference.
/// </summary>
public class AudioManager : MonoBehaviour
{
    // ──────────────────────────────────────────────────────────────
    // Singleton
    // ──────────────────────────────────────────────────────────────

    public static AudioManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // persist across scene loads
    }

    // ──────────────────────────────────────────────────────────────
    // Inspector References
    // ──────────────────────────────────────────────────────────────

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;    // one-shot effects
    [SerializeField] private AudioSource musicSource;  // looping background music

    [Header("Sound Effects")]
    [SerializeField] private AudioClip spinStartClip;
    [SerializeField] private AudioClip reelStopClip;
    [SerializeField] private AudioClip winClip;
    [SerializeField] private AudioClip jackpotClip;
    [SerializeField] private AudioClip loseClip;
    [SerializeField] private AudioClip buttonClickClip;
    [SerializeField] private AudioClip coinDropClip;   // plays per-credit animation frame

    [Header("Background Music")]
    [SerializeField] private AudioClip casinoLoopClip;
    [SerializeField] [Range(0f, 1f)] private float musicVolume = 0.4f;

    // ──────────────────────────────────────────────────────────────
    // Unity Lifecycle
    // ──────────────────────────────────────────────────────────────

    private void Start()
    {
        if (musicSource != null && casinoLoopClip != null)
        {
            musicSource.clip   = casinoLoopClip;
            musicSource.loop   = true;
            musicSource.volume = musicVolume;
            musicSource.Play();
        }
    }

    // ──────────────────────────────────────────────────────────────
    // Public API
    // ──────────────────────────────────────────────────────────────

    public void PlaySpinStart()   => PlaySFX(spinStartClip);
    public void PlayReelStop()    => PlaySFX(reelStopClip);
    public void PlayWin()         => PlaySFX(winClip);
    public void PlayJackpot()     => PlaySFX(jackpotClip);
    public void PlayLose()        => PlaySFX(loseClip);
    public void PlayButtonClick() => PlaySFX(buttonClickClip);
    public void PlayCoinDrop()    => PlaySFX(coinDropClip);

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null) musicSource.volume = musicVolume;
    }

    // ──────────────────────────────────────────────────────────────
    // Private Helpers
    // ──────────────────────────────────────────────────────────────

    private void PlaySFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip);
    }
}
