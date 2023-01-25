using DefineGameUtil;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngameManager : MonoBehaviourPunCallbacks
{
    public enum eGameState
    {
        WAIT = 0,
        START,
        END,
    }

    [SerializeField] GameObject _giftBlockParent; // 맵에 생성되는 부숴지는 블록 프리팹
    [SerializeField] Transform[] _spawnPoint; // 캐릭터들이 생성 될 자리

    string[] _balls = new string[] { "StarBall", "PoisonBall", "SnowBall" }; // 공 종류
    string[] _characters = new string[] { "PLAYERChef", "PLAYERKnight", "PLAYERSleep" }; // 캐릭터 종류
    int _myCharacter;

    NetworkManager networkManager;
    PlayerCtrl playerCtrl;
    InGameUI InGameUI;

    eGameState _nowState = eGameState.WAIT; // 현재 게임진행상황
    int[] spawn = new int[4]; // 각 플레이어에게 스폰될 자리의 자식 오브젝트를 설정하기 위함

    bool _isPlayerSetting = false; // 모든 플레이어 생성 확인
    bool _isBallSetting = false; // 모든 플레이어에게 공 생성 확인


    void Start()
    {
        SoundManager._instance.PlayBGMSound(eBGM.Ingame, true);
        networkManager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
        InGameUI = GameObject.Find("InGameUI").GetComponent<InGameUI>();
        if (PhotonNetwork.IsMasterClient)
        {
            if (SceneControlManager._instance.NowScene >= eSceneType.SnowMap) // 맵에 따라 위치에 블록 생성
            {
                for (int i = 0; i < _giftBlockParent.transform.childCount; i++)
                {
                    PhotonNetwork.Instantiate("GiftBlock", _giftBlockParent.transform.GetChild(i).transform.position, _giftBlockParent.transform.GetChild(i).transform.rotation);
                }
            }
            for (int n = 0; n < PhotonNetwork.CurrentRoom.PlayerCount; n++) // 플레이어 오브젝트 생성 명령
            {
                if (!RandomPoint(spawn, n))
                {
                    n--;
                }
            }
            networkManager.RandomSpawnPlayer(spawn);
        }
    }

    void Update()
    {
        switch (_nowState)
        {
            case eGameState.WAIT:
                {
                    if (GameObject.FindGameObjectsWithTag("Player").Length.Equals(PhotonNetwork.CurrentRoom.PlayerCount))
                    {
                        _isPlayerSetting = true;
                        if (_isPlayerSetting)
                        {
                            if (_isBallSetting)
                            {
                                return;
                            }
                            int rand = Random.Range(0, _balls.Length);
                            playerCtrl.BallSetting(rand);
                            _isBallSetting = true;
                        }
                    }
                    break;
                }
            case eGameState.START:
                {
                    if (PhotonNetwork.IsMasterClient)
                    {
                        GameObject[] winner = GameObject.FindGameObjectsWithTag("Player");
                        if (winner.Length == 1)
                        {
                            networkManager.SendGameOver(winner[0].GetPhotonView().Owner.NickName);
                            _nowState = eGameState.END;
                        }
                    }
                    break;
                }
            case eGameState.END:
                {
                    break;
                }
        }
    }
    public void TimeOver()
    {
        GameObject[] winner = GameObject.FindGameObjectsWithTag("Player");
        List<float> hpList = new List<float>();
        for(int n =0; n < winner.Length; n++)
        {
            PlayerCtrl player = winner[n].GetComponent<PlayerCtrl>();
            hpList.Add(player.HpCheck);
        }
        hpList.Sort();
        for(int n = 0; n < hpList.Count; n++)
        {
            PlayerCtrl player = winner[n].GetComponent<PlayerCtrl>();
            if(hpList[hpList.Count-1] == player.HpCheck)
            {
                networkManager.SendGameOver(winner[n].GetPhotonView().Owner.NickName);
                _nowState = eGameState.END;
                return;
            }
        }
    }
    public void PlayerDied()
    {
        Invoke("ThreeSecondsWaitDestroy", 3.0f);
    }
    void ThreeSecondsWaitDestroy()
    {
        PhotonNetwork.Destroy(playerCtrl.gameObject);
    }
    public Transform TeleportPoint(int index)
    {
        return _spawnPoint[index];
    }
    public void GameStart()// 게임시작
    {
        _nowState = eGameState.START;
        playerCtrl._isStart = true;
        InGameUI.GameStart = true;
    }
    public void GameOver(string winner)
    {
        if (playerCtrl != null)
        {
            playerCtrl.GameOver();
        }
        if (PhotonNetwork.NickName == winner)
        {
            InGameUI.GameResultOpen(true, _myCharacter);
        }
        else
        {
            InGameUI.GameResultOpen(false, _myCharacter);
        }
    }
    public void SpawnPlayer(int spawn, int character) // 플레이어 생성 및 나의 플레이어 저장
    {
        _myCharacter = character;
        playerCtrl = PhotonNetwork.Instantiate(_characters[character], _spawnPoint[spawn].position, _spawnPoint[spawn].rotation).GetComponent<PlayerCtrl>();
    }
    bool RandomPoint(int[] spawn, int index)
    {
        int rand = Random.Range(0, 4);
        if (!spawn.Contains(rand))
        {
            spawn[index] = rand;
            return true;
        }
        return false;
    }
}
