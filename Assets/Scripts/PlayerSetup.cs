using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerSetup : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    void Start()
    {
        if(photonView.IsMine)
        {
            transform.GetComponent<PlayerControl>().enabled = true;
        }
        else
        {
            transform.GetComponent<PlayerControl>().enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
