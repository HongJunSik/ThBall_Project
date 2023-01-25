using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using DefineGameUtil;

public class SoundManager : MonoBehaviour
{
    static SoundManager _uniqueInstance;

    [SerializeField] AudioClip[] _bgmClips;
    [SerializeField] AudioClip[] _sfxClips;

    AudioSource _bgmPlayer;
    AudioSource _sfxPlayer;

    public float BgmValue
    {
        get; set;
    }

    public float EfsValue
    {
        get; set;
    }

    public static SoundManager _instance
    {
        get { return _uniqueInstance; }
    }

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        _uniqueInstance = this;
        _bgmPlayer = transform.GetChild(0).GetComponent<AudioSource>();
        _sfxPlayer = transform.GetChild(1).GetComponent<AudioSource>();
        InitializeSet();
    }

    public void InitializeSet(float bv = 0.2f, bool bm = false, float fv = 0.2f, bool fm = false)
    {
        BgmValue = bv;
        EfsValue = fv;
        _bgmPlayer.volume = bv;
        _bgmPlayer.mute = bm;
        _sfxPlayer.volume = fv;
        _sfxPlayer.mute = fm;
    }

    public void PlayBGMSound(eBGM type, bool isLoop = false)
    {
        _bgmPlayer.clip = _bgmClips[(int)type];
        _bgmPlayer.loop = isLoop;
        _bgmPlayer.Play();
    }

    public void PlaySFXSoundOneShot(eSFX type, bool isLoop = false)
    {
        _sfxPlayer.loop = isLoop;
        _sfxPlayer.PlayOneShot(_sfxClips[(int)type]);
    }
}
