using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DefineGameUtil;
using Photon.Pun;
using Photon.Realtime;

public class InGameUI : MonoBehaviour
{
    // ���� ��� â ����
    [Header("���� ���")]
    [SerializeField] Transform _resultPoint;
    [SerializeField] GameObject _winresultWnd;
    [SerializeField] GameObject _loseresultWnd;
    [SerializeField] GameObject[] _winplayerCharacter;
    [SerializeField] GameObject[] _loseplayerCharacter;

    // �ð� ����
    [Header("�ð�")]
    [SerializeField] Text _minutes;
    [SerializeField] Text _seconds;

    // ������ ���� ����
    [Header("������")]
    [SerializeField] GameObject[] _itemPre;
    [SerializeField] Transform _itemSlot;

    // �ɼ� â ����
    [Header("�ɼ�")]
    [SerializeField] GameObject _optionBG;
    [SerializeField] GameObject _optionUI;

    // �� ������
    public GameObject _shootGauge;

    float _totalTime = 180;

    public bool _isResult = false;
    float _resultTime = 0;
    float _waitTime = 5.0f;
    public bool GameStart
    {
        get; set;
    }
    void Start()
    {
        _resultPoint.gameObject.SetActive(false);
        _minutes.text = ((int)(_totalTime / 60.0f)).ToString("D2");
        _seconds.text = ((int)(_totalTime % 60.0f)).ToString("D2");
    }

    void Update()
    {
        if (GameStart)
        {
            _totalTime -= Time.deltaTime;
            _minutes.text = ((int)(_totalTime / 60.0f)).ToString("D2");
            _seconds.text = ((int)(_totalTime % 60.0f)).ToString("D2");
            if (PhotonNetwork.IsMasterClient)
            {
                if (_totalTime <= 0)
                {
                    GameObject.Find("IngameManager").GetComponent<IngameManager>().TimeOver();
                    GameStart = false;
                }
            }
        }
        if (_isResult)
        {
            _resultTime += Time.deltaTime;
            if(_waitTime < _resultTime)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    GameObject.Find("NetworkManager").GetComponent<NetworkManager>().NextSceneWithString(eSceneType.Lobby);
                    _isResult = false;
                }
            }
        }
    }

    public void GameResultOpen(bool result, int charIndex)
    {
        _resultPoint.gameObject.SetActive(true);
        GameStart = false;
        _isResult = true;
        if (result)
        {
            GameObject go = Instantiate(_winresultWnd, _resultPoint);
            Instantiate(_winplayerCharacter[charIndex], go.transform);
        }
        else
        {
            GameObject go = Instantiate(_loseresultWnd, _resultPoint);
            Instantiate(_loseplayerCharacter[charIndex], go.transform);
        }
    }

    public string UseItem(int slotNum)
    {
        if(_itemSlot.GetChild(slotNum).childCount != 1)
        {
            return "";
        }
        int index = _itemSlot.GetChild(slotNum).GetChild(0).name.LastIndexOf("(Clone)");
        string item = _itemSlot.GetChild(slotNum).GetChild(0).name.Remove(index);
        Destroy(_itemSlot.GetChild(slotNum).GetChild(0).gameObject);
        return item;
    }

    public void GetItem(int itemNum)
    {
        for(int n = 0; n < _itemSlot.childCount; n++)
        {
            if(_itemSlot.GetChild(n).childCount == 0)
            {
                Instantiate(_itemPre[itemNum], _itemSlot.GetChild(n));
                return;
            }
        }
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
