using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    #region Singleton
    public static AudioManager instance;
    private void Awake()
    {
        // ทำ Singleton Pattern เพื่อให้มี AudioManager ตัวเดียวตลอดทั้งเกม
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // ห้ามทำลายเมื่อเปลี่ยนฉาก
            InitializeAudioSources();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    #region Fields
    [Header("Audio Mixer Settings")]
    [SerializeField] private AudioMixer audioMixer;

    [SerializeField] private AudioMixerGroup musicGroup;
    [SerializeField] private AudioMixerGroup sfxGroup;

    // ชื่อ Parameter ที่ต้องตรงกับที่ Expose ไว้ใน Audio Mixer
    private const string MIXER_MUSIC_PARAM = "MusicVolume";
    private const string MIXER_SFX_PARAM = "SFXVolume";

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource weaponLoopSource;

    [Header("Default Clips")]
    [SerializeField] private AudioClip startingBGM;

    // Key สำหรับบันทึกค่าลง PlayerPrefs
    private const string MUSIC_KEY = "MusicVolume";
    private const string SFX_KEY = "SFXVolume";
    #endregion

    #region Unity Methods
    private void Start()
    {
        // โหลดค่าความดังเสียงที่บันทึกไว้เมื่อเริ่มเกม
        LoadVolume();

        if (startingBGM != null)
        {
            PlayBGM(startingBGM);
        }
    }
    #endregion

    #region Core Logic
    /// <summary>
    /// สร้างและตั้งค่า AudioSource ถ้ายังไม่มี พร้อมกำหนด Output Group ให้ถูกต้อง
    /// </summary>
    private void InitializeAudioSources()
    {
        if (musicSource == null) musicSource = gameObject.AddComponent<AudioSource>();
        if (musicGroup != null) musicSource.outputAudioMixerGroup = musicGroup;

        if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();
        if (sfxGroup != null) sfxSource.outputAudioMixerGroup = sfxGroup;

        if (weaponLoopSource == null) weaponLoopSource = gameObject.AddComponent<AudioSource>();
        if (sfxGroup != null) weaponLoopSource.outputAudioMixerGroup = sfxGroup;
    }

    private void LoadVolume()
    {
        float musicVol = PlayerPrefs.GetFloat(MUSIC_KEY, 1f);
        float sfxVol = PlayerPrefs.GetFloat(SFX_KEY, 1f);

        SetMusicVolume(musicVol);
        SetSFXVolume(sfxVol);
    }

    /// <summary>
    /// แปลงค่า Linear (0-1) เป็น Decibel (-80 ถึง 0) สำหรับใช้กับ AudioMixer
    /// </summary>
    private float LinearToDecibel(float linear)
    {
        float dB;
        if (linear != 0)
            dB = 20.0f * Mathf.Log10(linear);
        else
            dB = -144.0f; // ค่าต่ำสุดเพื่อปิดเสียงสนิท

        return dB;
    }
    #endregion

    #region Public Methods (BGM & SFX)
    public void PlayBGM(AudioClip clip)
    {
        if (clip == null) return;
        // เช็คเพื่อไม่ให้เพลงเริ่มใหม่ถ้าเป็นเพลงเดิม
        if (musicSource.clip == clip && musicSource.isPlaying) return;

        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void StopBGM()
    {
        musicSource.Stop();
    }

    /// <summary>
    /// เล่นเสียงเอฟเฟกต์แบบ OneShot (ซ้อนกันได้)
    /// </summary>
    public void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip, volumeScale);
    }

    /// <summary>
    /// เล่นเสียงวนลูป (สำหรับปืนกลหรือเลเซอร์) โดยจะเล่นแค่คลิปเดียวในเวลาเดียวกัน
    /// </summary>
    public void PlayWeaponLoop(AudioClip clip)
    {
        if (clip == null) return;
        if (weaponLoopSource.isPlaying && weaponLoopSource.clip == clip) return;

        weaponLoopSource.clip = clip;
        weaponLoopSource.loop = true;
        weaponLoopSource.Play();
    }

    public void StopWeaponLoop()
    {
        if (weaponLoopSource.isPlaying)
        {
            weaponLoopSource.Stop();
            weaponLoopSource.clip = null;
        }
    }
    #endregion

    #region Volume Control via Mixer
    /// <summary>
    /// ตั้งค่าความดังเสียงเพลง (รับค่า 0-1)
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        float vol = Mathf.Clamp01(volume);

        PlayerPrefs.SetFloat(MUSIC_KEY, vol);

        if (audioMixer != null)
        {
            audioMixer.SetFloat(MIXER_MUSIC_PARAM, LinearToDecibel(vol));
        }
    }

    /// <summary>
    /// ตั้งค่าความดังเสียงเอฟเฟกต์ (รับค่า 0-1)
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        float vol = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(SFX_KEY, vol);

        if (audioMixer != null)
        {
            audioMixer.SetFloat(MIXER_SFX_PARAM, LinearToDecibel(vol));
        }
    }

    public float GetMusicVolume() => PlayerPrefs.GetFloat(MUSIC_KEY, 1f);
    public float GetSFXVolume() => PlayerPrefs.GetFloat(SFX_KEY, 1f);
    #endregion
}