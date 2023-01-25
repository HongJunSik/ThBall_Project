using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallCtrl : MonoBehaviour
{
    public string _ballMaster
    {
        get; set;
    }

    int _myName;
    public int GetBallName
    {
        get { return _myName; }
    }

    void Awake()
    {
        _ballMaster = string.Empty;
        _myName = (name.Equals("StarBall(Clone)")) ? 0 : name.Equals("PoisonBall(Clone)") ? 1 : 2;
    }

}
