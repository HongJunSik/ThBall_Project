using DefineGameUtil;
using LitJson;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager instance;

    Sprite[] _mapImages;
    int _mapindex = 0;

    #region[ĳ���� �� �ɷ�ġ ����]
    CharacterAbilities[] _characterAbility;
    public CharacterAbilities[] Characters
    {
        get { return _characterAbility; }
    }
    string _charAbilPath;
    JsonData _charAbilData;
    #endregion

    #region[���ӽ��� �غ�Ȯ��]
    int _ready = 0;
    int _ballSetting = 0;
    #endregion

    #region[�÷��̾� ���� ����]
    GameObject[] players;
    int[] _ballIndex = new int[] { 0, 0, 0, 0 };
    int[] _proindex = new int[] { 0, 0, 0, 0 };
    int[] _actNum = new int[] { 0, 0, 0, 0 };
    string[] _nickName = new string[] { "", "", "", "" };
    bool[] _readyInRoom = new bool[] { false, false, false, false };
    public string[] GetNicknames
    {
        get { return _nickName; }
    }
    #endregion

    bool _isLobby = false; // �κ� ����Ȯ��

    [Header("LobbyPanel")]
    Text WelcomeText;
    Text LobbyInfoText;
    InputField RoomInput;
    Button[] CellBtn;
    Button PreviousBtn;
    Button NextBtn;

    [Header("RoomPanel")]
    GameObject RoomPanel;
    GameObject[] PlayerCells;
    Text ListText;
    Text RoomInfoText;
    GameObject ChatText;
    InputField ChatInput;
    ScrollRect ChatView;
    GameObject[] _profile;
    EventTrigger _gameStartBtn;
    EventTrigger _gameReadyBtn;
    GameObject _mapSelectWndOpenBtn;
    Image _mainMapWnd;

    [Header("ETC")]
    PhotonView PV;
    public PhotonView GetPV
    {
        get { return PV; }
    }

    List<RoomInfo> myList = new List<RoomInfo>();
    int currentPage = 1, maxPage, multiple;

    public bool _gameStart = false;

    void Awake()
    {
        instance = this;
        _charAbilPath = File.ReadAllText(Application.streamingAssetsPath + "/CharacterAbilities.json");
        DontDestroyOnLoad(gameObject);
    }
    void Start()
    {
        PV = GetComponent<PhotonView>();

        _charAbilData = JsonMapper.ToObject(_charAbilPath);
        _characterAbility = new CharacterAbilities[_charAbilData.Count];
        SetCharAbilities();
    }
    void Update()
    {
        if (_isLobby)
        {
            LobbyInfoText.text = (PhotonNetwork.CountOfPlayers - PhotonNetwork.CountOfPlayersInRooms) + "�κ� / " + PhotonNetwork.CountOfPlayers + "����";
        }
    }

    #region �渮��Ʈ ����
    // ����ư -2 , ����ư -1 , �� ����
    public void MyListClick(int num)
    {
        if (num == -2) --currentPage;
        else if (num == -1) ++currentPage;
        else PhotonNetwork.JoinRoom(myList[multiple + num].Name);
        MyListRenewal();
    }

    void MyListRenewal()
    {
        // �ִ�������
        maxPage = (myList.Count % CellBtn.Length == 0) ? myList.Count / CellBtn.Length : myList.Count / CellBtn.Length + 1;

        // ����, ������ư
        PreviousBtn.interactable = (currentPage <= 1) ? false : true;
        NextBtn.interactable = (currentPage >= maxPage) ? false : true;

        // �������� �´� ����Ʈ ����
        multiple = (currentPage - 1) * CellBtn.Length;
        for (int i = 0; i < CellBtn.Length; i++)
        {
            CellBtn[i].interactable = (multiple + i < myList.Count);
            CellBtn[i].transform.GetChild(0).GetComponent<Text>().text = (multiple + i < myList.Count) ? myList[multiple + i].Name : "";
            CellBtn[i].transform.GetChild(1).GetComponent<Text>().text = (multiple + i < myList.Count) ? myList[multiple + i].PlayerCount + "/" + myList[multiple + i].MaxPlayers : "";
        }
        GameObject.Find("LobbySceneUI").GetComponent<LobbySceneUI>().LobbyEnter();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        int roomCount = roomList.Count;
        for (int i = 0; i < roomCount; i++)
        {
            if (!roomList[i].RemovedFromList)
            {
                if (!myList.Contains(roomList[i])) myList.Add(roomList[i]);
                else myList[myList.IndexOf(roomList[i])] = roomList[i];
            }
            else if (myList.IndexOf(roomList[i]) != -1) myList.RemoveAt(myList.IndexOf(roomList[i]));
        }
        MyListRenewal();
    }
    #endregion

    #region ��������
    public void Connect()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.GameVersion = "1.0";
        PhotonNetwork.ConnectUsingSettings();
    }
    public override void OnConnectedToMaster() => PhotonNetwork.JoinLobby();
    public void Disconnect() => PhotonNetwork.Disconnect();

    public override void OnDisconnected(DisconnectCause cause)
    {

    }
    #endregion

    #region[�κ�]
    public void InLobbyScene(Text wel, Text info, InputField rinput, Button[] cells, Button prebtn, Button nextbtn)
    {
        WelcomeText = wel;
        LobbyInfoText = info;
        RoomInput = rinput;
        CellBtn = cells;
        PreviousBtn = prebtn;
        NextBtn = nextbtn;
    }
    public void RoomSet(Text Ltext, Text RInfoText, GameObject chatText, InputField chatInput, ScrollRect chatView, GameObject roompanel, GameObject[] playercells, GameObject[] profiles, EventTrigger startBtn, EventTrigger readyBtn, GameObject mapWndopenBtn, Image mapWnd,Sprite[] mapImages)
    {
        ListText = Ltext;
        RoomInfoText = RInfoText;
        ChatText = chatText;
        ChatInput = chatInput;
        ChatView = chatView;
        RoomPanel = roompanel;
        PlayerCells = playercells;
        _profile = profiles;
        _gameStartBtn = startBtn;
        _gameReadyBtn = readyBtn;
        _mapSelectWndOpenBtn = mapWndopenBtn;
        _mainMapWnd = mapWnd;
        _mapImages = mapImages;
    }
    public void NicknameSet()
    {
        _isLobby = true;
        OnJoinedLobby();
    }
    public override void OnJoinedLobby()
    {
        if (!_isLobby)
        {
            return;
        }
        PhotonNetwork.LocalPlayer.NickName = UserInfos._instance._myNick;
        WelcomeText.text = PhotonNetwork.LocalPlayer.NickName + "�� ȯ���մϴ�";
        myList.Clear();
    }
    #endregion

    #region ��

    // �游���
    public void CreateRoom() => PhotonNetwork.CreateRoom(RoomInput.text == "" ? "Room" + Random.Range(0, 100) : RoomInput.text, new RoomOptions { MaxPlayers = 4 });
    // �ƹ� ���̳� ���� Ȥ�� ����
    public void JoinRandomRoom() => PhotonNetwork.JoinRandomRoom();
    // ���� �濡 �����ߴٸ�
    public override void OnJoinedRoom()
    {
        SoundManager._instance.PlayBGMSound(eBGM.Room, true);
        RoomInput.text = "";
        RoomPanel.SetActive(true);
        if (PhotonNetwork.IsMasterClient)
        {
            PlayerCells[0].transform.GetChild(3).gameObject.SetActive(true);
            _actNum[0] = 1;
            _nickName[0] = PhotonNetwork.NickName;
            Setting(_actNum, _nickName, _proindex, _readyInRoom);
            _gameStart = false;
        }
        RoomRenewal();
        ChatInput.text = "";
    }
    // �� ������
    public void LeaveRoom()
    {
        _isGaming = false;
        // ä�ñ�� ����
        for (int n = 0; n < ChatView.content.childCount; n++)
        {
            Destroy(ChatView.content.GetChild(n).gameObject);
        }
        // �ε��� �ʱ�ȭ
        for (int n = 0; n < _nickName.Length; n++)
        {
            _ballIndex[n] = 0;
            _proindex[n] = 0;
            _actNum[n] = 0;
            _nickName[n] = "";
            _readyInRoom[n] = false;
        }
        GameObject.Find("LobbySceneUI").GetComponent<LobbySceneUI>()._isGameReady = false;
        _dyingPlayer.Clear();
        RoomPanel.SetActive(false);
        PhotonNetwork.LeaveRoom();
    }
    // �� ����� ����
    public override void OnCreateRoomFailed(short returnCode, string message) { RoomInput.text = ""; CreateRoom(); }
    // ���� ���� ����
    public override void OnJoinRandomFailed(short returnCode, string message) { RoomInput.text = ""; CreateRoom(); }
    // �ٸ��̰� ���� ������ ��
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            for (int n = 0; n < _actNum.Length; n++)
            {
                if (_actNum[n] == 0)
                {
                    _actNum[n] = newPlayer.ActorNumber;
                    _nickName[n] = newPlayer.NickName;
                    _proindex[n] = 0;
                    _readyInRoom[n] = false;
                    PV.RPC("Setting", RpcTarget.All, _actNum, _nickName, _proindex, _readyInRoom);
                    PV.RPC("MapImgSetting", RpcTarget.Others, _mapindex);
                    break;
                }
            }
        }
        RoomRenewal();
        ChatRPC("<color=red>" + newPlayer.NickName + "���� �����ϼ̽��ϴ�</color>");
    }
    [PunRPC]
    void MapImgSetting(int index)
    {
        _mapindex = index;
        _mainMapWnd.sprite = _mapImages[index];
    }
    // �ٸ��̰� ���� ������ ��
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        for (int n = 0; n < _actNum.Length; n++)
        {
            if (_actNum[n] == otherPlayer.ActorNumber)
            {
                _actNum[n] = 0;
                _nickName[n] = "";
                _proindex[n] = 0;
                _readyInRoom[n] = false;
            }
            else if (_nickName[n] == PhotonNetwork.NickName)
            {
                _readyInRoom[n] = false;
            }
        }
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.DestroyPlayerObjects(otherPlayer);

            if (SceneControlManager._instance.NowScene == eSceneType.Lobby)
            {
                _gameStart = false;
                GameObject.Find("LobbySceneUI").GetComponent<LobbySceneUI>()._isGameReady = false;

                PV.RPC("Setting", RpcTarget.All, _actNum, _nickName, _proindex, _readyInRoom);
                RoomRenewal();
                ChatRPC("<color=red>" + otherPlayer.NickName + "���� �����ϼ̽��ϴ�</color>");
            }
        }
    }

    // ���ӽ��� �� �� �̵�
    public void NextSceneWithString(eSceneType sceneType)
    {
        PV.RPC("NextSceneReady", RpcTarget.All, (int)sceneType);
    }
    [PunRPC]
    void NextSceneReady(int sceneIndex)
    {
        SceneControlManager._instance._gameOver = SceneControlManager._instance.NowScene >= eSceneType.SnowMap && sceneIndex == (int)eSceneType.Lobby;
        SceneControlManager._instance.NowScene = (eSceneType)sceneIndex;
        PV.RPC("NextSceneGo", RpcTarget.MasterClient, sceneIndex);
    }
    int _goRoomReady = 0;
    [PunRPC]
    void NextSceneGo(int sceneIndex)
    {
        _goRoomReady++;
        if (_goRoomReady == PhotonNetwork.CurrentRoom.PlayerCount)
        {
            _goRoomReady = 0;
            PhotonNetwork.CurrentRoom.IsVisible = (SceneControlManager._instance.NowScene >= eSceneType.SnowMap) ? false : true;
            PhotonNetwork.LoadLevel(sceneIndex);
        }
    }
    [PunRPC]
    void NextSceneMove(int sceneIndex)
    {
        SceneControlManager._instance.GameStart(sceneIndex);
    }
    public void GoRoomFromIngame()
    {
        RoomPanel.SetActive(true);
        if (PhotonNetwork.IsMasterClient)
        {
            _gameStart = false;
            GameObject.Find("LobbySceneUI").GetComponent<LobbySceneUI>()._isGameReady = false;

            for (int n = 0; n < _actNum.Length; n++)
            {
                if (_nickName[n] == PhotonNetwork.NickName)
                {
                    _readyInRoom[n] = false;
                    PV.RPC("Setting", RpcTarget.All, _actNum, _nickName, _proindex, _readyInRoom);
                    break;
                }
            }

        }
        else
        {
            Text text = _gameReadyBtn.transform.GetChild(0).GetComponent<Text>();
            text.text = "�غ��ư";
        }
        RoomRenewal();
        ChatInput.text = "";
    }
    // ������ �̹��� ����
    public void LProfileBtnClick()
    {
        for (int n = 0; n < _actNum.Length; n++)
        {
            if (_actNum[n] == PhotonNetwork.LocalPlayer.ActorNumber)
            {
                _proindex[n] = (_proindex[n] == 0) ? 2 : _proindex[n] - 1; break;
            }
        }
        PV.RPC("Changeprofile", RpcTarget.All, _proindex);
    }
    // ������ �̹��� ����
    public void RProfileBtnClick()
    {
        for (int n = 0; n < _actNum.Length; n++)
        {
            if (_actNum[n] == PhotonNetwork.LocalPlayer.ActorNumber)
            {
                _proindex[n] = (_proindex[n] == 2) ? 0 : _proindex[n] + 1; break;
            }
        }
        PV.RPC("Changeprofile", RpcTarget.All, _proindex);
    }
    // �� ��Ȳ ����
    void RoomRenewal()
    {
        ListText.text = "";
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            ListText.text += PhotonNetwork.PlayerList[i].NickName + ((i + 1 == PhotonNetwork.PlayerList.Length) ? "" : ", ");
        RoomInfoText.text = PhotonNetwork.CurrentRoom.Name + " / " + PhotonNetwork.CurrentRoom.PlayerCount + "�� / " + PhotonNetwork.CurrentRoom.MaxPlayers + "�ִ�";
    }

    [PunRPC]
    void Setting(int[] act, string[] nick, int[] proindex, bool[] readyState)
    {
        _actNum = act;
        _nickName = nick;
        _proindex = proindex;
        _readyInRoom = readyState;
        int index = 0;
        if (SceneControlManager._instance.NowScene == eSceneType.Lobby)
        {
            for (int n = 0; n < PhotonNetwork.CurrentRoom.MaxPlayers; n++)
            {
                index = (_readyInRoom[n] == true) ? index + 1 : index;
                PlayerCells[n].transform.GetChild(2).gameObject.SetActive(_readyInRoom[n]);

                Text nickname = PlayerCells[n].transform.GetChild(0).GetComponent<Text>();
                nickname.text = _nickName[n]; // �г��� �����ϱ�

                PlayerCells[n].transform.GetChild(3).gameObject.SetActive(_nickName[n] == PhotonNetwork.MasterClient.NickName);

                _profile[n].transform.GetChild(3).gameObject.SetActive(_actNum[n] == PhotonNetwork.LocalPlayer.ActorNumber);
                _profile[n].transform.GetChild(4).gameObject.SetActive(_actNum[n] == PhotonNetwork.LocalPlayer.ActorNumber);

                if (_actNum[n] == 0) // ������ �ڸ��� �÷��̾ ���ٸ�
                {
                    _profile[n].SetActive(false); // �� �ڸ� ��Ȱ��ȭ
                }
                else
                {
                    _profile[n].SetActive(true);
                    for (int m = 0; m < _charAbilData.Count; m++) // ������ �̹���
                    {
                        if (m == _proindex[n])
                        {
                            _profile[n].transform.GetChild(m).gameObject.SetActive(true); // ������ ������ �̹����� Ȱ��ȭ
                        }
                        else
                        {
                            _profile[n].transform.GetChild(m).gameObject.SetActive(false); // ������ ��Ȱ��ȭ
                        }
                    }
                }
            }

            if (index != 0 && PhotonNetwork.IsMasterClient)
            {
                _gameStart = (index == PhotonNetwork.CurrentRoom.PlayerCount - 1);
            }

            _gameReadyBtn.gameObject.SetActive(!PhotonNetwork.IsMasterClient);
            _gameStartBtn.gameObject.SetActive(PhotonNetwork.IsMasterClient);
            _mapSelectWndOpenBtn.gameObject.SetActive(PhotonNetwork.IsMasterClient);
        }
    }
    [PunRPC]
    void Changeprofile(int[] proindex)
    {
        Setting(_actNum, _nickName, proindex, _readyInRoom);
        _proindex = proindex;
    }

    #endregion

    #region ä��
    public void Send()
    {
        if (PhotonNetwork.LocalPlayer.Equals(PhotonNetwork.MasterClient))
        {
            PV.RPC("ChatRPC", RpcTarget.All, "<color=blue>(����)</color>" + PhotonNetwork.NickName + " : " + ChatInput.text);
            ChatInput.text = "";
        }
        else
        {
            PV.RPC("ChatRPC", RpcTarget.All, PhotonNetwork.NickName + " : " + ChatInput.text);
            ChatInput.text = "";
        }
    }
    [PunRPC] // RPC�� �÷��̾ �����ִ� �� ��� �ο����� �����Ѵ�
    void ChatRPC(string msg)
    {
        GameObject go = Instantiate(ChatText, ChatView.content);
        Text txtLine = go.GetComponent<Text>();
        txtLine.text = msg;
    }
    #endregion

    // �ٸ��̵鿡�� �ΰ��� �� ���� �̵���Ȳ ����
    public void SendMyTurn(string nick, int index)
    {
        PV.RPC("ChangeRotation", RpcTarget.Others, nick, index);
    }
    [PunRPC]
    void ChangeRotation(string nick, int index)
    {
        GameObject[] go = GameObject.FindGameObjectsWithTag("Player");
        for (int n = 0; n < go.Length; n++)
        {
            PlayerCtrl playerCtrl = go[n].GetComponent<PlayerCtrl>();
            playerCtrl.Turn(nick, index);
        }
    }

    // ��ο��� �÷��̾� ���� ���
    public void RandomSpawnPlayer(int[] value)
    {
        PV.RPC("SendSpawnPoint", RpcTarget.All, value);
    }
    [PunRPC]
    void SendSpawnPoint(int[] value)
    {
        int my = 0;
        for (int n = 0; n < _actNum.Length; n++)
        {
            if (_actNum[n].Equals(PhotonNetwork.LocalPlayer.ActorNumber))
            {
                my = n;
                break;
            }
        }
        GameObject.Find("IngameManager").GetComponent<IngameManager>().SpawnPlayer(value[my], _proindex[my]);
    }

    // ��ο��� ������ �´� �� ���� ���
    public void SendBallSpawn(string nick, int index)
    {
        PV.RPC("BallSetting", RpcTarget.All, nick, index);
    }
    [PunRPC]
    void BallSetting(string nick, int index)
    {
        for (int n = 0; n < _nickName.Length; n++)
        {
            if (_nickName[n].Equals(nick))
            {
                _ballIndex[n] = index;
                break;
            }
        }
        if (PhotonNetwork.IsMasterClient)
        {
            _ready++;
            if (_ready.Equals(PhotonNetwork.PlayerList.Length))
            {
                players = GameObject.FindGameObjectsWithTag("Player");
                string[] s = new string[players.Length];
                for (int n = 0; n < players.Length; n++)
                {
                    s[n] = players[n].name;
                }
                PV.RPC("SetPlayers", RpcTarget.All, s);
            }
            else if (_ready > PhotonNetwork.PlayerList.Length)
            {
                PV.RPC("PickUpBall", RpcTarget.Others, nick, index);
            }
        }
    }
    [PunRPC]
    void PickUpBall(string nick, int index)
    {
        for (int n = 0; n < players.Length; n++)
        {
            if (_dyingPlayer.Contains(n))
            {
                continue;
            }
            PlayerCtrl player = players[n].GetComponent<PlayerCtrl>();
            if (player.GetPlayerNickName.Equals(nick))
            {
                player.PickUpBall(index);
                break;
            }
        }
    }
    [PunRPC]
    void SetPlayers(string[] s)
    {
        GameObject[] gos = GameObject.FindGameObjectsWithTag("Player");
        players = new GameObject[s.Length];
        int add = 0;
        while (add < s.Length)
        {
            for (int n = 0; n < gos.Length; n++)
            {
                if (s[add] == gos[n].name)
                {
                    if (players.Contains(gos[n]))
                    {
                        continue;
                    }
                    players[add] = gos[n];
                    add++;
                    break;
                }
            }
        }
        if (!_isGaming)
        {
            OtherBallSetting();
        }
    }
    [PunRPC]
    void OtherBallSetting()
    {
        for (int n = 0; n < players.Length; n++)
        {
            if (_dyingPlayer.Contains(n))
            {
                continue;
            }
            PlayerCtrl player = players[n].GetComponent<PlayerCtrl>();
            if (player.GetPlayerNickName != PhotonNetwork.NickName)
            {
                for (int m = 0; m < _nickName.Length; m++)
                {
                    if (_nickName[m] == player.GetPlayerNickName)
                    {
                        player.BallSettingOther(_ballIndex[m]);
                    }
                }
            }
        }
    }

    bool _isGaming = false;
    // ��ο��� ���ӽ��� ���
    public void StartBallSettingClear()
    {
        PV.RPC("GoGameStart", RpcTarget.MasterClient);
    }
    [PunRPC]
    void GoGameStart()
    {
        _ballSetting++;
        if (_ballSetting.Equals(PhotonNetwork.PlayerList.Length))
        {
            PV.RPC("StartGame", RpcTarget.All);
        }
    }
    [PunRPC]
    void StartGame()
    {
        _isGaming = true;
        GameObject.Find("IngameManager").GetComponent<IngameManager>().GameStart();
    }

    // ���� �����ٸ� ���� ���� ������ ���
    public void SendNoneBall(string nick)
    {
        PV.RPC("NoneBall", RpcTarget.Others, nick);
    }
    [PunRPC]
    void NoneBall(string nick)
    {
        for (int n = 0; n < players.Length; n++)
        {
            if (_dyingPlayer.Contains(n))
            {
                continue;
            }
            PlayerCtrl player = players[n].GetComponent<PlayerCtrl>();
            if (player.GetPlayerNickName.Equals(nick))
            {
                player.DonHaveBall();
                break;
            }
        }
    }

    public void DestroyObj(PhotonView pv)
    {
        PV.RPC("Destroyer", RpcTarget.All, pv.Owner.NickName, pv.ViewID, pv.gameObject.tag);
    }
    [PunRPC]
    void Destroyer(string nick, int viewid, string tag)
    {
        if (PhotonNetwork.NickName == nick)
        {
            GameObject[] gos = GameObject.FindGameObjectsWithTag(tag);
            for (int n = 0; n < gos.Length; n++)
            {
                PhotonView pv = gos[n].GetPhotonView();
                if (pv.ViewID == viewid)
                {
                    if (tag == "Gift")
                    {
                        string item = DropRandomItem();
                        if (item != "")
                        {
                            PhotonNetwork.Instantiate(item, gos[n].transform.position, gos[n].transform.rotation);
                        }
                    }
                    PhotonNetwork.Destroy(gos[n]);
                    return;
                }
            }
        }
    }
    string DropRandomItem()
    {
        int rand = Random.Range(0, 100);
        if (rand < 7)
        {
            return eItem.StarBall.ToString();
        }
        else if (rand < 14)
        {
            return eItem.PoisonBall.ToString();
        }
        else if (rand < 21)
        {
            return eItem.SnowBall.ToString();
        }
        else if (rand < 28)
        {
            return eItem.ItemPotionObj.ToString();
        }
        else if (rand < 35)
        {
            return eItem.ItemEyeObj.ToString();
        }
        else if (rand < 40)
        {
            return eItem.ItemTelePortObj.ToString();
        }
        else if (rand < 60)
        {
            return eItem.ItemSpeedUpObj.ToString();
        }
        else if (rand < 80)
        {
            return eItem.ItemPowerUpObj.ToString();
        }
        else
        {
            return "";
        }
    }

    public void HitOnBall(PhotonView CharPV, AttackBallCtrl attackBall)
    {
        PV.RPC("HitDamage", RpcTarget.All, CharPV.Owner.NickName, attackBall.Damage, attackBall.GetBallName);
        PV.RPC("HitEffect", RpcTarget.All, CharPV.Owner.NickName, attackBall.GetBallName);
    }
    [PunRPC]
    void HitDamage(string nick, float damage, int ballIndex)
    {
        for (int n = 0; n < players.Length; n++)
        {
            if (_dyingPlayer.Contains(n))
            {
                continue;
            }
            PhotonView pv = players[n].GetPhotonView();
            if (pv.Owner.NickName == nick)
            {
                PlayerCtrl player = players[n].GetComponent<PlayerCtrl>();
                player.HitOnBalls(damage, ballIndex);
                return;
            }
        }
    }
    [PunRPC]
    void HitEffect(string nick, int ballIndex)
    {
        for (int n = 0; n < players.Length; n++)
        {
            if (_dyingPlayer.Contains(n))
            {
                continue;
            }
            PhotonView pv = players[n].GetPhotonView();
            if (pv.Owner.NickName == nick)
            {
                PlayerCtrl player = players[n].GetComponent<PlayerCtrl>();
                player.BallEffectOn(ballIndex);
                return;
            }
        }
    }

    public void BallEffectEnd(string nick)
    {
        PV.RPC("EffectOff", RpcTarget.All, nick);
    }
    [PunRPC]
    void EffectOff(string nick)
    {
        for (int n = 0; n < players.Length; n++)
        {
            if (_dyingPlayer.Contains(n))
            {
                continue;
            }
            PhotonView pv = players[n].GetPhotonView();
            if (pv.Owner.NickName == nick)
            {
                PlayerCtrl player = players[n].GetComponent<PlayerCtrl>();
                player.BallEffectOff();
                return;
            }
        }
    }

    // Json ���Ϸ� ĳ���� �ɷ�ġ �޾ƿ���
    public void SetCharAbilities()
    {
        for (int n = 0; n < _charAbilData.Count; n++)
        {
            CharacterAbilities CharacterInfo = new CharacterAbilities();

            CharacterInfo.SetCharAbil(_charAbilData[n][0].ToString(), float.Parse(_charAbilData[n][1].ToString()), float.Parse(_charAbilData[n][2].ToString()), float.Parse(_charAbilData[n][3].ToString()));
            _characterAbility[n] = CharacterInfo;
        }
    }

    public void SendEyeItemUse(string nick)
    {
        PV.RPC("EyeItemUse", RpcTarget.Others, nick);
    }
    [PunRPC]
    void EyeItemUse(string nick)
    {
        for (int n = 0; n < players.Length; n++)
        {
            if (_dyingPlayer.Contains(n))
            {
                continue;
            }
            PhotonView pv = players[n].GetPhotonView();
            if (pv.Owner.NickName == nick)
            {
                PlayerCtrl player = players[n].GetComponent<PlayerCtrl>();
                player.EyeItemUsed();
                return;
            }
        }
    }

    public void SendPotionItemUse(string nick)
    {
        PV.RPC("PotionItemUse", RpcTarget.All, nick);
    }
    [PunRPC]
    void PotionItemUse(string nick)
    {
        for (int n = 0; n < players.Length; n++)
        {
            if (_dyingPlayer.Contains(n))
            {
                continue;
            }
            PhotonView pv = players[n].GetPhotonView();
            if (pv.Owner.NickName == nick)
            {
                PlayerCtrl player = players[n].GetComponent<PlayerCtrl>();
                player.PotionItemUsed();
                return;
            }
        }
    }

    public void SendHitPoisonBall(string nick)
    {
        PV.RPC("HitPoisonBall", RpcTarget.All, nick);
    }
    [PunRPC]
    void HitPoisonBall(string nick)
    {
        for (int n = 0; n < players.Length; n++)
        {
            if (_dyingPlayer.Contains(n))
            {
                continue;
            }
            PhotonView pv = players[n].GetPhotonView();
            if (pv.Owner.NickName == nick)
            {
                PlayerCtrl player = players[n].GetComponent<PlayerCtrl>();
                player.HitPoisonBall();
                return;
            }
        }
    }

    List<int> _dyingPlayer = new List<int>();
    public void SendPlayerDie(string nick)
    {
        PV.RPC("PlayerDie", RpcTarget.All, nick);
    }
    [PunRPC]
    void PlayerDie(string nick)
    {
        for (int n = 0; n < players.Length; n++)
        {
            if (_dyingPlayer.Contains(n))
            {
                continue;
            }
            PhotonView pv = players[n].GetPhotonView();
            if (pv.Owner.NickName == nick)
            {
                _dyingPlayer.Add(n);
                if (pv.IsMine)
                {
                    GameObject.Find("IngameManager").GetComponent<IngameManager>().PlayerDied();
                }
                return;
            }
        }
    }

    public void SendGameOver(string nick)
    {
        PV.RPC("GameOver", RpcTarget.All, nick);
    }
    [PunRPC]
    void GameOver(string nick)
    {
        _isGaming = false;
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.DestroyAll();
        }
        GameObject.Find("IngameManager").GetComponent<IngameManager>().GameOver(nick);
        _ready = 0;
        _dyingPlayer.Clear();
        _ballSetting = 0;
        _readyInRoom = new bool[] { false, false, false, false };
    }

    public void GameReadyBtnClick(bool ready, string nick)
    {
        for (int n = 0; n < _nickName.Length; n++)
        {
            if (_nickName[n] == nick)
            {
                PV.RPC("PlayerClickReadyBtn", RpcTarget.MasterClient, ready, n);
                return;
            }
        }
    }
    [PunRPC]
    void PlayerClickReadyBtn(bool ready, int index)
    {
        _readyInRoom[index] = ready;
        PV.RPC("Setting", RpcTarget.All, _actNum, _nickName, _proindex, _readyInRoom);
    }

    public void AttackBallMasterEmpty(PhotonView pv)
    {
        PV.RPC("BallMasterEmpty", RpcTarget.All, pv.ViewID);
    }
    [PunRPC]
    void BallMasterEmpty(int viewID)
    {
        GameObject[] gos = GameObject.FindGameObjectsWithTag("AttackBall");
        for (int n = 0; n < gos.Length; n++)
        {
            if (gos[n].GetPhotonView().ViewID == viewID)
            {
                AttackBallCtrl ball = gos[n].GetComponent<AttackBallCtrl>();
                ball.MasterName = string.Empty;
                return;
            }
        }
    }

    public void PlayerToPlayerTriggerOn(PhotonView myPV, PhotonView yourPV)
    {
        PV.RPC("PlayerTrrigerOn", RpcTarget.All, myPV.ViewID, yourPV.ViewID);
    }
    [PunRPC]
    void PlayerTrrigerOn(int myID, int yourID)
    {
        GameObject[] gos = GameObject.FindGameObjectsWithTag("Player");
        for (int n = 0; n < gos.Length; n++)
        {
            if (gos[n].GetPhotonView().ViewID == myID || gos[n].GetPhotonView().ViewID == yourID)
            {
                CapsuleCollider2D collider = gos[n].GetComponent<CapsuleCollider2D>();
                collider.isTrigger = true;
            }
        }
    }

    public void PlayerToPlayerTriggerOff(PhotonView myPV, PhotonView yourPV)
    {
        PV.RPC("PlayerTrrigerOff", RpcTarget.All, myPV.ViewID, yourPV.ViewID);
    }
    [PunRPC]
    void PlayerTrrigerOff(int myID, int yourID)
    {
        GameObject[] gos = GameObject.FindGameObjectsWithTag("Player");
        for (int n = 0; n < gos.Length; n++)
        {
            if (gos[n].GetPhotonView().ViewID == myID || gos[n].GetPhotonView().ViewID == yourID)
            {
                CapsuleCollider2D collider = gos[n].GetComponent<CapsuleCollider2D>();
                collider.isTrigger = false;
            }
        }
    }

    public void SendMapChange(int spriteIndex)
    {
        PV.RPC("MapImgChange", RpcTarget.All, spriteIndex);
    }
    [PunRPC]
    void MapImgChange(int index)
    {
        _mapindex = index;
        _mainMapWnd.sprite = _mapImages[index];
    }
}
