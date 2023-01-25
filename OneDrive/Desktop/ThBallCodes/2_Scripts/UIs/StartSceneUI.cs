using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DefineGameUtil;

public class StartSceneUI : MonoBehaviour
{
    // 옵션 창 관련
    [Header("옵션")]
    [SerializeField] GameObject _optionBG;
    [SerializeField] GameObject _optionUI;

    void Start()
    {
        Screen.SetResolution(1920, 1080, false); // SetResolution 함수 제대로 사용하기
        SoundManager._instance.PlayBGMSound(eBGM.Start, true);
    }

    // 시작버튼 클릭
    public void StartBtnClick()
    {
        SoundManager._instance.PlaySFXSoundOneShot(eSFX.BtnClick);
        ServerManager._instance.NetConnect();
    }

    // 옵션버튼 클릭
    public void OptionBtnClick()
    {
        SoundManager._instance.PlaySFXSoundOneShot(eSFX.BtnClick);
        _optionBG.SetActive(true);
        Instantiate(_optionUI, _optionBG.transform);
    }

    // 종료버튼 클릭
    public void QuitBtnClick()
    {
        // TCPClientManager._instance.GracefullyDisconnect();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
