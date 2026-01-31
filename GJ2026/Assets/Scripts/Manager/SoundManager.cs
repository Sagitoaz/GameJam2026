using UnityEngine;
using System.Collections.Generic;

public class SoundManager : Singleton<SoundManager>
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    
    [Header("Volume Settings")]
    [SerializeField] private float masterVolume = 1f;
    [SerializeField] private float musicVolume = 0.7f;
    [SerializeField] private float sfxVolume = 1f;
    
    private Dictionary<string, AudioClip> loadedClips = new Dictionary<string, AudioClip>();

    public override void Awake()
    {
        base.Awake();
        
        // Tạo AudioSource nếu chưa có
        if (musicSource == null)
        {
            GameObject musicObj = new GameObject("MusicSource");
            musicObj.transform.SetParent(transform);
            musicSource = musicObj.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }
        
        if (sfxSource == null)
        {
            GameObject sfxObj = new GameObject("SFXSource");
            sfxObj.transform.SetParent(transform);
            sfxSource = sfxObj.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }
        
        UpdateVolumes();
    }

    #region Music
    
    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (clip == null) return;
        
        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.Play();
    }
    
    public void PlayMusic(string clipName, bool loop = true)
    {
        AudioClip clip = LoadClip(clipName);
        if (clip != null)
        {
            PlayMusic(clip, loop);
        }
    }
    
    public void StopMusic()
    {
        musicSource.Stop();
    }
    
    public void PauseMusic()
    {
        musicSource.Pause();
    }
    
    public void ResumeMusic()
    {
        musicSource.UnPause();
    }
    
    #endregion

    #region SFX
    
    public void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip, volumeScale);
    }
    
    public void PlaySFX(string clipName, float volumeScale = 1f)
    {
        AudioClip clip = LoadClip(clipName);
        if (clip != null)
        {
            PlaySFX(clip, volumeScale);
        }
    }
    
    public void PlaySFXAtPoint(AudioClip clip, Vector3 position, float volumeScale = 1f)
    {
        if (clip == null) return;
        AudioSource.PlayClipAtPoint(clip, position, sfxVolume * masterVolume * volumeScale);
    }
    
    #endregion

    #region Volume Control
    
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
    }
    
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
    }
    
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
    }
    
    private void UpdateVolumes()
    {
        if (musicSource != null)
            musicSource.volume = musicVolume * masterVolume;
        
        if (sfxSource != null)
            sfxSource.volume = sfxVolume * masterVolume;
    }
    
    #endregion

    #region Resource Loading
    
    private AudioClip LoadClip(string clipName)
    {
        // Check cache trước
        if (loadedClips.ContainsKey(clipName))
        {
            return loadedClips[clipName];
        }
        
        // Load từ Resources/Audio
        AudioClip clip = Resources.Load<AudioClip>($"Audio/{clipName}");
        
        if (clip != null)
        {
            loadedClips[clipName] = clip;
        }
        else
        {
            Debug.LogWarning($"[SoundManager] Không tìm thấy audio clip: {clipName} trong Resources/Audio/");
        }
        
        return clip;
    }
    
    public void UnloadClip(string clipName)
    {
        if (loadedClips.ContainsKey(clipName))
        {
            loadedClips.Remove(clipName);
        }
    }
    
    public void UnloadAllClips()
    {
        loadedClips.Clear();
    }
    
    #endregion
}
