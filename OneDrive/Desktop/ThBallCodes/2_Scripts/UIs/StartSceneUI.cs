using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DefineGameUtil;

public class StartSceneUI : MonoBehaviour
{
    // �ɼ� â ����
    [Header("�ɼ�")]
    [SerializeField] GameObject _optionBG;
    [SerializeField] GameObject _optionUI;

    void Start()
    {
        Screen.SetResolution(1920, 1080, false); // SetResolution �Լ� ����� ����ϱ�
        SoundManager._instance.PlayBGMSound(eBGM.Start, true);
    }

    // ���۹�ư Ŭ��
    public void StartBtnClick()
    {
        SoundManager._instance.PlaySFXSoundOneShot(eSFX.BtnClick);
        ServerManager._instance.NetConnect();
    }

    // �ɼǹ�ư Ŭ��
    public void OptionBtnClick()
    {
        SoundManager._instance.PlaySFXSoundOneShot(eSFX.BtnClick);
        _optionBG.SetActive(true);
        Instantiate(_optionUI, _optionBG.transform);
    }

    // �����ư Ŭ��
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
