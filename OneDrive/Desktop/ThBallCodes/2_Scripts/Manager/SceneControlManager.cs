using DefineGameUtil;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneControlManager : TSingleton<SceneControlManager>
{
    eSceneType _nowSceneType;
    eSceneType _oldSceneType;

    public eSceneType NowScene
    {
        get { return _nowSceneType; }
        set
        {
            _oldSceneType = _nowSceneType;
            _nowSceneType = value;
        }
    }

    public bool SetNickName
    {
        get; set;
    }

    public bool _gameOver = false;

    public eSceneType _currentScene
    {
        get { return _nowSceneType; }
    }
    protected override void Init()
    {
        base.Init();
    }

    void Start()
    {
        _nowSceneType = eSceneType.Start;
    }

    public void StartLoginScene()
    {
        _oldSceneType = _nowSceneType;
        _nowSceneType = eSceneType.Login;
        SceneManager.LoadScene((int)_nowSceneType);
    }
    public void LoginToLobbyScene(string nick)
    {
        SetNickName = (nick.Equals("")) ? true : false;
        _nowSceneType = eSceneType.Lobby;
        _oldSceneType = _nowSceneType;
        SceneManager.LoadScene((int)_nowSceneType);
    }
    public void DisconnectGoStartScene()
    {
        _nowSceneType = eSceneType.Start;
        _oldSceneType = _nowSceneType;
        SceneManager.LoadScene((int)_nowSceneType);
    }
    public void GameStart(int index)
    {
        SceneManager.LoadScene((int)_nowSceneType);
    }
}
