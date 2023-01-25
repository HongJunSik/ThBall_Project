using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class GiftBlockCtrl : MonoBehaviourPunCallbacks
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Punch")
        {
            GameObject.Find("NetworkManager").GetComponent<NetworkManager>().DestroyObj(gameObject.GetPhotonView());
        }
    }
}
