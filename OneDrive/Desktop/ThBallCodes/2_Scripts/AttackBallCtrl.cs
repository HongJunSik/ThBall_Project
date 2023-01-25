using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackBallCtrl : MonoBehaviourPunCallbacks
{
    string[] _balls = new string[] { "StarBall", "PoisonBall", "SnowBall" }; // °ø Á¾·ù

    public string MasterName;
    //public string BallMaster
    //{
    //    get { return MasterName; }
    //    set
    //    {
    //        MasterName = value;
    //    }
    //}

    float _shootSpeed = 1000;

    int _myName;
    public int GetBallName
    {
        get { return _myName; }
    }

    Rigidbody2D rigid;

    public float Damage
    {
        get; set;
    }

    float _checkTime = 0;
    float _changeTime = 1.0f;

    void Start()
    {
        int index = name.LastIndexOf("(Clone)");
        _myName = (name.Remove(index).Equals("AttackStarBall")) ? 0 : name.Remove(index).Equals("AttackPoisonBall") ? 1 : 2;
    }
    void Update()
    {
        _checkTime += Time.deltaTime;
        if (_checkTime > _changeTime)
        {
            ChangeBall();
        }
    }
    void ChangeBall()
    {
        PhotonView PV = gameObject.GetPhotonView();
        if (PV.IsMine)
        {
            GameObject go = PhotonNetwork.Instantiate(_balls[_myName], transform.position, transform.rotation);
            go.transform.position = new Vector3(go.transform.position.x, go.transform.position.y, 0);
            PhotonNetwork.Destroy(gameObject);
        }
    }

    public void Shoot(Vector2 pos, int gauge)
    {
        _shootSpeed *= (0.5f + gauge * 0.2f);
        Damage = (gauge == 1) ? Damage : (gauge == 2) ? Damage * 1.3f : (gauge == 3) ? Damage * 1.6f : (gauge == 4) ? Damage * 2.0f : Damage * 2.5f;
        rigid = GetComponent<Rigidbody2D>();
        rigid.AddForce(pos * _shootSpeed);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Map"))
        {
            if (MasterName != string.Empty)
            {
                GameObject.Find("NetworkManager").GetComponent<NetworkManager>().AttackBallMasterEmpty(gameObject.GetPhotonView());
                SoundManager._instance.PlaySFXSoundOneShot(DefineGameUtil.eSFX.Hit_Default);
            }
        }
        else if (collision.collider.CompareTag("Gift"))
        {
            if (MasterName == string.Empty)
            {
                return;
            }
            if (MasterName != string.Empty)
            {
                GameObject.Find("NetworkManager").GetComponent<NetworkManager>().AttackBallMasterEmpty(gameObject.GetPhotonView());
                SoundManager._instance.PlaySFXSoundOneShot(DefineGameUtil.eSFX.Hit_Block);
                GameObject.Find("NetworkManager").GetComponent<NetworkManager>().DestroyObj(collision.gameObject.GetPhotonView());
            }
        }
    }

    
    void OnEnable()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, 0);
        MasterName = gameObject.GetPhotonView().Owner.NickName;
    }
}
