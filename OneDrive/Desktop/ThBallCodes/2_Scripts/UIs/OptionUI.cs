using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionUI : MonoBehaviour
{
    // 사운드 관련
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
        // SoundManager에 저장된 volume값 불러오기 ( 씬마다 공유를 위함 )
        _bgmS.value = SoundManager._instance.BgmValue;
        _efsS.value = SoundManager._instance.EfsValue;
    }

    void Update()
    {
        // SoundManager에 현재 volume값 저장
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

        // 현재 volume 수치로 표시
        _bgmVol.text = ((int)(_bgmS.value * 100)).ToString();
        _efsVol.text = ((int)(_efsS.value * 100)).ToString();

        _bgmIcon.sprite = (_bgmS.value == 0) ? _icons[1] : _icons[0];
        _efmIcon.sprite = (_efsS.value == 0) ? _icons[3] : _icons[2];
    }

    // UI창 닫기 버튼 클릭
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
