using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DefineGameUtil;

public class LobbySceneUI : MonoBehaviour
{
    //�ʼ��� ����
    [SerializeField] Sprite[] _mapImages;
    [SerializeField] GameObject _mapSelectWndBtn;
    [SerializeField] GameObject _mapSelectWnd;
    [SerializeField] Image _mainMapWnd;
    [SerializeField] Image _selectMapWnd;
    [SerializeField] Button _mapPreBtn;
    [SerializeField] Button _mapNextBtn;
    int _mapindex = 0;
    int _maxMapindex = 1;

    // �������� ����â
    [SerializeField] GameObject _infoTextWnd;
    [SerializeField] Text _infoText;

    [SerializeField] GameObject QuitBtn;
    // �ɼǰ���
    [Header("�ɼ�")]
    [SerializeField] GameObject _optionBG;
    [SerializeField] GameObject _optionUI;

    // �г��� ���� â
    [Header("�г���")]
    [SerializeField] GameObject _nickNameSelectWnd;
    [SerializeField] InputField _nickNameInput;

    // ��Ʈ��ũ �Ŵ��� ���ø��(LOBBY)
    [Header("��Ʈ��ũ ���ø��(�κ�)")]
    [SerializeField] Text _welcomeText;
    [SerializeField] Text _nowInfoText;
    [SerializeField] InputField _roomNameInput;
    [SerializeField] Button[] _cells;
    [SerializeField] Button _preBtn;
    [SerializeField] Button _nextBtn;
    [SerializeField] EventTrigger _roomCreateBtn;
    [SerializeField] EventTrigger _randomRoomBtn;

    // ��Ʈ��ũ �Ŵ��� ���ø��(ROOM)
    [Header("��Ʈ��ũ ���ø��(��)")]
    [SerializeField] GameObject _roomPanel;
    [SerializeField] Text _inPlayerList;
    [SerializeField] Text _roomInfoText;
    [SerializeField] GameObject _chatText;
    [SerializeField] InputField _chatInput;
    [SerializeField] ScrollRect _chatView;
    [SerializeField] GameObject[] _playercells;
    [SerializeField] GameObject[] _profiles;
    [SerializeField] EventTrigger _gameStartBtn;
    [SerializeField] EventTrigger _gameReadyBtn;

    NetworkManager networkManager;

    public bool _isGameReady = false;

    void Start()
    {
        _mainMapWnd.sprite = _mapImages[_mapindex];

        SoundManager._instance.PlayBGMSound(eBGM.Lobby, true);
        networkManager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
        _preBtn.enabled = false;
        _nextBtn.enabled = false;
        _roomCreateBtn.enabled = false;
        _randomRoomBtn.enabled = false;
        _gameStartBtn.gameObject.SetActive(false);
        _gameReadyBtn.gameObject.SetActive(false);

        networkManager.InLobbyScene(_welcomeText, _nowInfoText, _roomNameInput, _cells, _preBtn, _nextBtn);
        networkManager.RoomSet(_inPlayerList, _roomInfoText, _chatText, _chatInput, _chatView, _roomPanel, _playercells, _profiles, _gameStartBtn, _gameReadyBtn, _mapSelectWndBtn, _mainMapWnd, _mapImages);

        if (SceneControlManager._instance.SetNickName)
        {
            _nickNameSelectWnd.SetActive(true);
        }
        else
        {
            _nickNameSelectWnd.SetActive(false);
            networkManager.NicknameSet();
        }

        for(int n = 0; n < _cells.Length; n++)
        {
            _cells[n].interactable = false;
        }

        if (SceneControlManager._instance._gameOver)
        {
            QuitBtn.SetActive(false);
            networkManager.GoRoomFromIngame();
            SceneControlManager._instance._gameOver = false;
        }
    }

    public void MapSelectWndOpen()
    {
        RendererValue(0);
        _mapSelectWnd.SetActive(true);
    }
    public void MapPreNextBtnClick(int index)
    {
        _mapindex += index;
        _mapindex = (_mapindex > _maxMapindex) ? 0 : (_mapindex < 0) ? _maxMapindex : _mapindex;
        _selectMapWnd.sprite = _mapImages[_mapindex];
    }

    public void MapSelectBtnClick()
    {
        _mapSelectWnd.SetActive(false);
        RendererValue(1);
        networkManager.SendMapChange(_mapindex);
    }

    // ����â ����
    public void ShowInfoText(string s)
    {
        RendererValue(0);
        _infoText.text = s;
        _infoTextWnd.SetActive(true);
    }
    // ����â �ݱ�
    public void CloseInfoText()
    {
        RendererValue(1);
        _infoText.text = string.Empty;
        _infoTextWnd.SetActive(false);
    }

    public void RendererValue(int index)
    {
        for (int n = 0; n < _profiles.Length; n++)
        {
            SortingGroup renderer = _profiles[n].GetComponent<SortingGroup>();
            renderer.sortingOrder = index;
        }
    }

    public void LeaveRoomBtnClick()
    {
        SoundManager._instance.PlaySFXSoundOneShot(eSFX.BtnClick);
        _isGameReady = false;
        QuitBtn.SetActive(true);
        networkManager.LeaveRoom();
    }

    public void LobbyEnter()
    {
        _preBtn.enabled = true;
        _nextBtn.enabled = true;
        _roomCreateBtn.enabled = true;
        _randomRoomBtn.enabled = true;
    }

    // �г��� ���� â �ݱ�
    void CloseNicknameUI()
    {
        _nickNameInput.text = string.Empty;
        _nickNameSelectWnd.SetActive(false);
    }

    // �г��� ����
    public void SelectNicknameClick()
    {
        SoundManager._instance.PlaySFXSoundOneShot(eSFX.BtnClick);
        if (_nickNameInput.text.Length < 4)
        {
            ShowInfoText("�г����� �ּ� 4�� �̻��Դϴ�.");
        }
        else if (_nickNameInput.text.Length > 12)
        {
            ShowInfoText("�г����� �ִ� 12�� �̻��Դϴ�.");
        }
        else
        {
            UserInfos._instance.SetNickNameInfo(_nickNameInput.text);
            ServerManager._instance.SendSetNickname(UserInfos._instance._myNick);
        }
    }

    public void NickSetResult(bool result)
    {
        if (result)
        {
            CloseNicknameUI();
            networkManager.NicknameSet();
        }
        else
        {
            ShowInfoText("�̹� ������� �г����Դϴ�.");
        }
    }
    // �� ����
    public void CreateRoom()
    {
        SoundManager._instance.PlaySFXSoundOneShot(eSFX.BtnClick);
        _roomCreateBtn.enabled = false;
        QuitBtn.SetActive(false);
        networkManager.CreateRoom();
        _roomCreateBtn.enabled = true;
    }

    // �����ϰ� �� ����
    public void RandomJoinRoom()
    {
        SoundManager._instance.PlaySFXSoundOneShot(eSFX.BtnClick);
        _randomRoomBtn.enabled = false;
        QuitBtn.SetActive(false);
        networkManager.JoinRandomRoom();
        _randomRoomBtn.enabled = true;
    }

    public void EnterRoom(int n)
    {
        QuitBtn.SetActive(false);
        networkManager.MyListClick(n);
    }

    public void SendBtnClick()
    {
        SoundManager._instance.PlaySFXSoundOneShot(eSFX.BtnClick);
        if (_chatInput.text != "")
        {
            networkManager.Send();
        }
    }
    public void GameReadyBtnClick()
    {
        SoundManager._instance.PlaySFXSoundOneShot(eSFX.BtnClick);
        _isGameReady = !_isGameReady;
        networkManager.GameReadyBtnClick(_isGameReady,PhotonNetwork.NickName);
        Text text = _gameReadyBtn.transform.GetChild(0).GetComponent<Text>();
        text.text = (_isGameReady) ? "�غ�Ϸ�" : "�غ��ư";
    }
    public void GameStartBtnClick()
    {
        if (networkManager._gameStart)
        {
            SoundManager._instance.PlaySFXSoundOneShot(eSFX.BtnClick);
            networkManager.NextSceneWithString((eSceneType)(_mapindex + 3));
        }
        else
        {
            ShowInfoText("��� �ο��� �غ� ���� �ʾҽ��ϴ�.");
        }
    }
    public void ProfileLBtnClick()
    {
        SoundManager._instance.PlaySFXSoundOneShot(eSFX.BtnClick);
        networkManager.LProfileBtnClick();
    }

    public void ProfileRBtnClick()
    {
        SoundManager._instance.PlaySFXSoundOneShot(eSFX.BtnClick);
        networkManager.RProfileBtnClick();
    }
    // �ɼǹ�ư Ŭ��
    public void OptionBtnClick()
    {
        RendererValue(0);
        SoundManager._instance.PlaySFXSoundOneShot(eSFX.BtnClick);
        _optionBG.SetActive(true);
        Instantiate(_optionUI, _optionBG.transform);
    }

    // �����ư Ŭ��
    public void QuitBtnClick()
    {
        networkManager.Disconnect();
        // TCPClientManager._instance.GracefullyDisconnect();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

}
