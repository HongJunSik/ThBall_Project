using DefineGameUtil;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCtrl : MonoBehaviourPunCallbacks
{
    [SerializeField] GameObject _hitEffect;
    [SerializeField] Transform[] _throwPoints; // 공이 생성되는 위치
    [SerializeField] Transform[] _ballSpawnPoint; // 공 세팅 위치
    [SerializeField] Canvas _InfoWnd;
    [SerializeField] Text _nickTxt;
    [SerializeField] Slider _hpBAR;

    float _maxHp;
    float _nowHp;
    float _maxSpeed;
    float _speed;
    float _origin_speed;
    float _maxPower;
    float _power;
    float _shield;

    bool _isEye = false;

    bool _charging = false;
    float _chargingTime = 0;
    int _gauge = 1;
    float _maxCharge = 3.0f;

    string _myNick;
    public string GetPlayerNickName
    {
        get { return _myNick; }
    }

    public Transform[] BallSpawn
    {
        get { return _ballSpawnPoint; }
    }

    public float HpCheck
    {
        get { return _nowHp; }
    }
    Rigidbody2D rigid;
    Animator animator;
    eState _nowstate = eState.IDLE;
    int _nowIndex = 0;
    bool _ballCatch = true;

    InGameUI _gameUI;
    GameObject _gaugeObj;
    int _gaugeIndex = 0;
    int _maxGaugeIndex = 9;
    float _nextGauge = 0.15f;

    string _myball;

    public bool _isStart
    {
        get; set;
    }

    bool _useAni = false;
    bool _isBlock = false;
    int _poisonTime = 0;
    float _slowSpeed = 0;
    bool _isSlow = false;

    bool _punching = false;
    float _punchCoolTime = 1.0f;

    void Awake()
    {
        _myNick = gameObject.GetPhotonView().Owner.NickName;
    }
    void Start()
    {
        _gameUI = GameObject.Find("InGameUI").GetComponent<InGameUI>();
        _gaugeObj = _gameUI._shootGauge;
        animator = GetComponent<Animator>();
        rigid = GetComponent<Rigidbody2D>();
        _nickTxt.text = (photonView.IsMine) ? "<color=red>" + _myNick + "</color>" : _myNick;
        _hpBAR.value = 1;
        _isStart = false;

        transform.GetChild(_nowIndex).gameObject.SetActive(true);
        SetMyAbilities();
        PunchGift();
    }

    void Update()
    {
        // 내 오브젝트가 아니라면 제어하지 않음
        if ((photonView.IsMine == false && PhotonNetwork.IsConnected == true))
        {
            return;
        }

        // 캐릭터 사망 시 조작 불가
        if (_nowstate == eState.DEATH)
        {
            return;
        }
        if (_nowHp < 0)
        {
            _nowstate = eState.DEATH;
            animator.SetInteger("State", (int)_nowstate);
            _useAni = true;
            NetworkManager.instance.SendPlayerDie(PhotonNetwork.NickName);
            GetComponent<CapsuleCollider2D>().isTrigger = true;
        }
        // 게임 종료 시, 기절 시 조작 불가
        if (_nowstate == eState.WIN || _isBlock)
        {
            return;
        }

        if (!_useAni)
        {
            animator.SetInteger("State", (int)_nowstate);
        }

        if (_punching)
        {
            _punchCoolTime -= Time.deltaTime;
            if(_punchCoolTime < 0)
            {
                _punching = false;
                _punchCoolTime = 1.0f;
            }
        }

        if (_isStart && _nowstate != eState.DEATH)
        {
            MovePlayer(); // 이동 함수
            UseItemValue(); // 아이템 사용 함수

            if (_charging)
            {
                _chargingTime += Time.deltaTime;
                _gaugeObj.transform.GetChild(_gaugeIndex).gameObject.SetActive(true);

                if (_chargingTime > _nextGauge)
                {
                    _nextGauge += 0.15f;
                    _gaugeIndex = (_gaugeIndex != _maxGaugeIndex) ? _gaugeIndex + 1 : _gaugeIndex;
                }
                else if (_chargingTime > _maxCharge)
                {
                    GoShoot();
                }
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (_ballCatch)
                {
                    _charging = true;
                    if (!_isSlow)
                    {
                        _speed = (_speed == _origin_speed) ? _speed / 2.0f : _speed;
                    }
                    else
                    {
                        _speed = _slowSpeed / 2.0f;
                    }
                    _nowstate = eState.THROW;
                    animator.SetInteger("State", (int)_nowstate);
                    _useAni = true;
                }
                else
                {
                    if (_punching)
                    {
                        return;
                    }
                    _punching = true;
                    for (int n = 0; n < 4; n++)
                    {
                        transform.GetChild(n).GetChild(3).gameObject.SetActive(true);
                    }
                    Invoke(nameof(PunchGift), 0.1f);
                }
            }
            else if (Input.GetKeyUp(KeyCode.Space))
            {
                if (!_ballCatch || !_charging)
                {
                    return;
                }
                GoShoot();
            }
        }
    }
    void PunchGift()
    {
        for (int n = 0; n < 4; n++)
        {
            transform.GetChild(n).GetChild(3).gameObject.SetActive(false);
        }
    }
    void GoShoot()
    {
        _useAni = false;
        _charging = false;
        _ballCatch = false;
        _gauge = (_gaugeIndex <= 1) ? 1 : (_gaugeIndex <= 3) ? 2 : (_gaugeIndex <= 5) ? 3 : (_gaugeIndex <= 7) ? 4 : 5;
        _nextGauge = 0.15f;
        _gaugeIndex = 0;
        _chargingTime = 0;
        for (int i = 0; i < _ballSpawnPoint.Length; i++)
        {
            for (int n = 0; n < _ballSpawnPoint[i].childCount; n++)
            {
                _ballSpawnPoint[i].GetChild(n).gameObject.SetActive(false);
                _ballSpawnPoint[i].GetChild(n).gameObject.SetActive(false);
            }
        }
        for (int i = 0; i < _gaugeObj.transform.childCount; i++)
        {
            _gaugeObj.transform.GetChild(i).gameObject.SetActive(false);
        }
        NetworkManager.instance.SendNoneBall(_myNick);
        if (!_isSlow)
        {
            _speed = _origin_speed;
        }
        else
        {
            _speed = _slowSpeed;
        }
        GameObject go = PhotonNetwork.Instantiate("Attack" + _myball, _throwPoints[_nowIndex].position, _throwPoints[_nowIndex].rotation);
        AttackBallCtrl ballCtrl = go.GetComponent<AttackBallCtrl>();
        ballCtrl.Damage = _power;
        switch (_nowIndex)
        {
            case 0:
                {
                    ballCtrl.Shoot(Vector3.down, _gauge);
                    break;
                }
            case 1:
                {
                    ballCtrl.Shoot(Vector3.up, _gauge);
                    break;
                }
            case 2:
                {
                    ballCtrl.Shoot(Vector3.left, _gauge);
                    break;
                }
            case 3:
                {
                    ballCtrl.Shoot(Vector3.right, _gauge);
                    break;
                }
        }
    }

    public void GameOver()
    {
        _isStart = false;
        _nowstate = eState.WIN;
        animator.SetInteger("State", (int)_nowstate);
        _useAni = true;
    }

    void UseItemValue()
    {
        string item = "";
        if (Input.GetKeyDown(KeyCode.Z))
        {
            item = _gameUI.UseItem(0);
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            item = _gameUI.UseItem(1);
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {
            item = _gameUI.UseItem(2);
        }
        switch (item)
        {
            case "SlotItemPotionObj":
                {
                    NetworkManager.instance.SendPotionItemUse(PhotonNetwork.NickName);
                    return;
                }
            case "SlotItemEyeObj":
                {
                    _isEye = true;
                    NetworkManager.instance.SendEyeItemUse(PhotonNetwork.NickName);
                    Invoke("EyeItemFinish", 7.0f);
                    return;
                }
            case "SlotItemTeleportObj":
                {
                    transform.position = GameObject.Find("IngameManager").GetComponent<IngameManager>().TeleportPoint(Random.Range(0, 4)).position;
                    return;
                }
            default:
                {
                    return;
                }
        }
    }
    public void PotionItemUsed()
    {
        if (_nowHp == _maxHp)
        {
            _shield = 15;
        }
        _nowHp = (_nowHp + 15 > _maxHp) ? _maxHp : _nowHp + 15;
        _hpBAR.value = _nowHp / _maxHp;
    }
    public void EyeItemUsed()
    {
        for (int n = 0; n < transform.childCount - 1; n++)
        {
            transform.GetChild(n).gameObject.SetActive(false);
        }
        Invoke("OthersEyeItemFinish", 7.0f);
    }
    void OthersEyeItemFinish()
    {
        transform.GetChild(_nowIndex).gameObject.SetActive(true);
        transform.GetChild(4).gameObject.SetActive(true);
        transform.GetChild(5).gameObject.SetActive(true);
        transform.GetChild(6).gameObject.SetActive(true);
    }
    void EyeItemFinish()
    {
        _isEye = false;
    }
    void SetMyAbilities()
    {
        _origin_speed = _speed = 0.06f;
        _power = 12;
        CharacterAbilities[] characters = NetworkManager.instance.Characters;
        for (int n = 0; n < characters.Length; n++)
        {
            int index = name.LastIndexOf("(Clone)");
            string objname = name.Remove(index);
            if (objname.Equals(characters[n].GetName))
            {
                _maxHp = _nowHp = characters[n].GetHp;
                _maxSpeed = characters[n].GetSpeed;
                _maxPower = characters[n].GetPower;
                return;
            }
        }
    }

    void MovePlayer()
    {
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            if (!_isEye)
                NetworkManager.instance.SendMyTurn(PhotonNetwork.NickName, 2);
            transform.GetChild(_nowIndex).gameObject.SetActive(false);
            _nowIndex = 2;
            transform.GetChild(_nowIndex).gameObject.SetActive(true);
            _nowstate = (_isSlow) ? eState.WALK : eState.RUN;
            Vector2 nextVec = new Vector2(-_speed, 0);
            rigid.MovePosition(rigid.position + nextVec);
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            if (!_isEye)
                NetworkManager.instance.SendMyTurn(PhotonNetwork.NickName, 3);
            transform.GetChild(_nowIndex).gameObject.SetActive(false);
            _nowIndex = 3;
            transform.GetChild(_nowIndex).gameObject.SetActive(true);
            _nowstate = (_isSlow) ? eState.WALK : eState.RUN;
            Vector2 nextVec = new Vector2(_speed, 0);
            rigid.MovePosition(rigid.position + nextVec);
        }
        else if (Input.GetKey(KeyCode.UpArrow))
        {
            if (!_isEye)
                NetworkManager.instance.SendMyTurn(PhotonNetwork.NickName, 1);
            transform.GetChild(_nowIndex).gameObject.SetActive(false);
            _nowIndex = 1;
            transform.GetChild(_nowIndex).gameObject.SetActive(true);
            _nowstate = (_isSlow) ? eState.WALK : eState.RUN;
            Vector2 nextVec = new Vector2(0, _speed);
            rigid.MovePosition(rigid.position + nextVec);
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            if (!_isEye)
                NetworkManager.instance.SendMyTurn(PhotonNetwork.NickName, 0);
            transform.GetChild(_nowIndex).gameObject.SetActive(false);
            _nowIndex = 0;
            transform.GetChild(_nowIndex).gameObject.SetActive(true);
            _nowstate = (_isSlow) ? eState.WALK : eState.RUN;
            Vector2 nextVec = new Vector2(0, -_speed);
            rigid.MovePosition(rigid.position + nextVec);
        }
        else
        {
            _nowstate = eState.IDLE;
        }
    }
    public void Turn(string nick, int index)
    {
        if (photonView.Owner.NickName.Equals(nick))
        {
            transform.GetChild(_nowIndex).gameObject.SetActive(false);
            _nowIndex = index;
            transform.GetChild(_nowIndex).gameObject.SetActive(true);
        }
    }

    public void BallSetting(int ball)
    {
        _ballCatch = true;
        for (int i = 0; i < _ballSpawnPoint.Length; i++)
        {
            for (int n = 0; n < _ballSpawnPoint[i].childCount; n++)
            {
                if (n == ball)
                {
                    _ballSpawnPoint[i].GetChild(n).gameObject.SetActive(true);
                }
                else
                {
                    _ballSpawnPoint[i].GetChild(n).gameObject.SetActive(false);
                }
            }
        }
        _myball = (ball == 0) ? "StarBall" : (ball == 1) ? "PoisonBall" : "SnowBall";
        NetworkManager.instance.SendBallSpawn(_myNick, ball);
    }
    public void PickUpBall(int ball)
    {
        _ballCatch = true;
        for (int i = 0; i < _ballSpawnPoint.Length; i++)
        {
            for (int n = 0; n < _ballSpawnPoint[i].childCount; n++)
            {
                if (n == ball)
                {
                    _ballSpawnPoint[i].GetChild(n).gameObject.SetActive(true);
                }
                else
                {
                    _ballSpawnPoint[i].GetChild(n).gameObject.SetActive(false);
                }
            }
        }
        _myball = (ball == 0) ? "StarBall" : (ball == 1) ? "PoisonBall" : "SnowBall";
    }
    public void BallSettingOther(int ball)
    {
        _ballCatch = true;
        for (int i = 0; i < _ballSpawnPoint.Length; i++)
        {
            for (int n = 0; n < _ballSpawnPoint[i].childCount; n++)
            {
                if (n == ball)
                {
                    _ballSpawnPoint[i].GetChild(n).gameObject.SetActive(true);
                }
                else
                {
                    _ballSpawnPoint[i].GetChild(n).gameObject.SetActive(false);
                }
            }
        }
        _myball = (ball == 0) ? "StarBall" : (ball == 1) ? "PoisonBall" : "SnowBall";
        NetworkManager.instance.StartBallSettingClear();
    }

    public void DonHaveBall()
    {
        for (int i = 0; i < _ballSpawnPoint.Length; i++)
        {
            for (int n = 0; n < _ballSpawnPoint[i].childCount; n++)
            {
                _ballSpawnPoint[i].GetChild(n).gameObject.SetActive(false);
                _ballSpawnPoint[i].GetChild(n).gameObject.SetActive(false);
            }
        }
    }

    public void BallEffectOn(int index)
    {
        for (int n = 0; n < _hitEffect.transform.childCount; n++)
        {
            _hitEffect.transform.GetChild(n).gameObject.SetActive(n == index);
        }
    }
    public void BallEffectOff()
    {
        for (int n = 0; n < _hitEffect.transform.childCount; n++)
        {
            _hitEffect.transform.GetChild(n).gameObject.SetActive(false);
        }
    }
    #region[공 종류별 함수]
    // 스타 공
    void HitStarBall()
    {
        _isBlock = true;
        Invoke("StarBallFinish", 2.0f);
    }
    void StarBallFinish()
    {
        NetworkManager.instance.BallEffectEnd(PhotonNetwork.NickName);
        _isBlock = false;
        _useAni = false;
        _nowstate = eState.IDLE;
        animator.SetInteger("State", (int)_nowstate);
    }

    // 독 공
    public void HitPoisonBall()
    {
        InvokeRepeating("HitPoison", 0, 1.0f);
    }
    void HitPoison()
    {
        if (_poisonTime == 4)
        {
            CancelInvoke("HitPoison");
            NetworkManager.instance.BallEffectEnd(PhotonNetwork.NickName);
            _poisonTime = 0;
            return;
        }
        _poisonTime++;
        _nowHp -= 2.0f;
        _hpBAR.value = _nowHp / _maxHp;
        return;
    }

    // 스노우 공
    void HitSnowBall()
    {
        _isSlow = true;
        _slowSpeed = _origin_speed * 0.3f;
        _speed *= 0.3f;
        Invoke("SnowBallFinish", 4.0f);
    }
    void SnowBallFinish()
    {
        NetworkManager.instance.BallEffectEnd(PhotonNetwork.NickName);
        _isSlow = false;
        if (_charging)
        {
            _speed = _origin_speed / 2;
        }
        else
        {
            _speed = _origin_speed;
        }
    }
    #endregion

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("AttackBall"))
        {
            AttackBallCtrl attackBall = collision.gameObject.GetComponent<AttackBallCtrl>();
            if (attackBall.MasterName != _myNick && attackBall.MasterName != string.Empty)
            {
                PhotonView ballPV = attackBall.gameObject.GetPhotonView();
                NetworkManager.instance.AttackBallMasterEmpty(ballPV);
                SoundManager._instance.PlaySFXSoundOneShot(eSFX.Hit_Player);
                if (ballPV.IsMine)
                {
                    NetworkManager.instance.HitOnBall(gameObject.GetPhotonView(), attackBall);
                }
            }
        }
    }
    public void HitOnBalls(float damage, int ballIndex)
    {
        if (_shield == 0)
        {
            _nowHp -= damage;
        }
        else
        {
            _shield -= damage;
            if (_shield < 0)
            {
                float trueDamage = Mathf.Abs(_shield);
                _shield = 0;
                _nowHp -= trueDamage;
            }
            else
            {
                return;
            }
        }
        _hpBAR.value = _nowHp / _maxHp;
        if ((photonView.IsMine == false && PhotonNetwork.IsConnected == true))
        {
            return;
        }
        _nowstate = (ballIndex == 0) ? eState.BLOCK : eState.HIT;
        animator.SetInteger("State", (int)_nowstate);
        _useAni = true;
        switch (ballIndex)
        {
            case 0:
                {
                    if (!_isBlock)
                    {
                        HitStarBall();
                    }
                    break;
                }
            case 1:
                {
                    NetworkManager.instance.SendHitPoisonBall(PhotonNetwork.NickName);
                    break;
                }
            case 2:
                {
                    if (!_isSlow)
                    {
                        HitSnowBall();
                    }
                    break;
                }
        }
    }
    void UseAniFalse()
    {
        _useAni = false;
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        // 내 오브젝트가 아니라면 제어하지 않음
        if ((photonView.IsMine == false && PhotonNetwork.IsConnected == true))
        {
            return;
        }
        if (collision.CompareTag("Ball"))
        {
           
            if (!_ballCatch)
            {
                BallCtrl ball = collision.transform.GetComponent<BallCtrl>();
                BallSetting(ball.GetBallName);
                PhotonView ballPV = ball.gameObject.GetPhotonView();
                NetworkManager.instance.DestroyObj(ballPV);
            }
        }
        else if (collision.CompareTag("Item"))
        {
            int index = collision.name.LastIndexOf("(Clone)");
            switch (collision.name.Remove(index))
            {
                case "ItemPotionObj":
                    {
                        _gameUI.GetItem(0);
                        NetworkManager.instance.DestroyObj(collision.gameObject.GetPhotonView());
                        return;
                    }
                case "ItemEyeObj":
                    {
                        _gameUI.GetItem(1);
                        NetworkManager.instance.DestroyObj(collision.gameObject.GetPhotonView());
                        return;
                    }
                case "ItemTeleportObj":
                    {
                        _gameUI.GetItem(2);
                        NetworkManager.instance.DestroyObj(collision.gameObject.GetPhotonView());
                        return;
                    }
            }
        }
        else if (collision.CompareTag("AbilItem"))
        {
            int index = collision.name.LastIndexOf("(Clone)");
            switch (collision.name.Remove(index))
            {
                case "ItemSpeedUpObj":
                    {
                        NetworkManager.instance.DestroyObj(collision.gameObject.GetPhotonView());
                        if (_origin_speed >= _maxSpeed)
                        {
                            return;
                        }
                        _origin_speed = (_origin_speed >= _maxSpeed) ? _maxSpeed : _origin_speed + 0.01f;
                        _speed = (_nowstate == eState.THROW) ? _origin_speed / 2 : _origin_speed;
                        return;
                    }
                case "ItemPowerUpObj":
                    {
                        NetworkManager.instance.DestroyObj(collision.gameObject.GetPhotonView());
                        if (_power == _maxPower)
                        {
                            return;
                        }
                        _power += 2;
                        return;
                    }
            }
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        //if (collision.CompareTag("Player"))
        //{
        //    networkManager.PlayerToPlayerTriggerOff(gameObject.GetPhotonView(), collision.gameObject.GetPhotonView());
        //}
    }
}
