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

    [SerializeField] GameObject _giftBlockParent; // �ʿ� �����Ǵ� �ν����� ��� ������
    [SerializeField] Transform[] _spawnPoint; // ĳ���͵��� ���� �� �ڸ�

    string[] _balls = new string[] { "StarBall", "PoisonBall", "SnowBall" }; // �� ����
    string[] _characters = new string[] { "PLAYERChef", "PLAYERKnight", "PLAYERSleep" }; // ĳ���� ����
    int _myCharacter;

    NetworkManager networkManager;
    PlayerCtrl playerCtrl;
    InGameUI InGameUI;

    eGameState _nowState = eGameState.WAIT; // ���� ���������Ȳ
    int[] spawn = new int[4]; // �� �÷��̾�� ������ �ڸ��� �ڽ� ������Ʈ�� �����ϱ� ����

    bool _isPlayerSetting = false; // ��� �÷��̾� ���� Ȯ��
    bool _isBallSetting = false; // ��� �÷��̾�� �� ���� Ȯ��


    void Start()
    {
        SoundManager._instance.PlayBGMSound(eBGM.Ingame, true);
        networkManager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
        InGameUI = GameObject.Find("InGameUI").GetComponent<InGameUI>();
        if (PhotonNetwork.IsMasterClient)
        {
            if (SceneControlManager._instance.NowScene >= eSceneType.SnowMap) // �ʿ� ���� ��ġ�� ��� ����
            {
                for (int i = 0; i < _giftBlockParent.transform.childCount; i++)
                {
                    PhotonNetwork.Instantiate("GiftBlock", _giftBlockParent.transform.GetChild(i).transform.position, _giftBlockParent.transform.GetChild(i).transform.rotation);
                }
            }
            for (int n = 0; n < PhotonNetwork.CurrentRoom.PlayerCount; n++) // �÷��̾� ������Ʈ ���� ���
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
    public void GameStart()// ���ӽ���
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
    public void SpawnPlayer(int spawn, int character) // �÷��̾� ���� �� ���� �÷��̾� ����
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
