using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionUI : MonoBehaviour
{
    // ���� ����
    [SerializeField] Image _bgmIcon;
    [SerializeField] Image _efmIcon;
    [SerializeField] Slider _bgmS;
    [SerializeField] Slider _efsS;
    [SerializeField] Text _bgmVol;
    [SerializeField] Text _efsVol;

    [SerializeField] Sprite[] _icons;

    bool _isBGM = false;
    bool _isEFS = false;

    float _bfBGM;
    float _bfEFS;

    void Start()
    {
        // SoundManager�� ����� volume�� �ҷ����� ( ������ ������ ���� )
        _bgmS.value = SoundManager._instance.BgmValue;
        _efsS.value = SoundManager._instance.EfsValue;
    }

    void Update()
    {
        // SoundManager�� ���� volume�� ����
        if(_bgmS.value != 0 && _efsS.value != 0)
        {
            SoundManager._instance.InitializeSet(_bgmS.value, false, _efsS.value, false);
        }
        else if (_bgmS.value == 0 && _efsS.value != 0)
        {
            SoundManager._instance.InitializeSet(_bgmS.value, true, _efsS.value, false);
        }
        else if (_bgmS.value != 0 && _efsS.value == 0)
        {
            SoundManager._instance.InitializeSet(_bgmS.value, false, _efsS.value, true);
        }
        else
        {
            SoundManager._instance.InitializeSet(_bgmS.value, true, _efsS.value, true);
        }

        // ���� volume ��ġ�� ǥ��
        _bgmVol.text = ((int)(_bgmS.value * 100)).ToString();
        _efsVol.text = ((int)(_efsS.value * 100)).ToString();

        _bgmIcon.sprite = (_bgmS.value == 0) ? _icons[1] : _icons[0];
        _efmIcon.sprite = (_efsS.value == 0) ? _icons[3] : _icons[2];
    }

    // UIâ �ݱ� ��ư Ŭ��
    public void QuitBtnClick()
    {
        GameObject go = GameObject.Find("OptionOpenBG");
        go.SetActive(false);
        if (SceneControlManager._instance.NowScene == DefineGameUtil.eSceneType.Lobby)
        {
            GameObject.Find("LobbySceneUI").GetComponent<LobbySceneUI>().RendererValue(1);
        }
        Destroy(gameObject);
    }

    public void BgmRemove()
    {
        if (!_isBGM)
        {
            _isBGM = true;
            _bfBGM = _bgmS.value;
            _bgmS.value = 0;
        }
        else
        {
            _isBGM = false;
            _bgmS.value = _bfBGM;
        }
    }

    public void EfsRemove()
    {
        if (!_isEFS)
        {
            _isEFS = true;
            _bfEFS = _efsS.value;
            _efsS.value = 0;
        }
        else
        {
            _isEFS = false;
            _efsS.value = _bfEFS;
        }
    }
}
