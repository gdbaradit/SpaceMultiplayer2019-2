using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class ShipGameManager : MonoBehaviourPunCallbacks
{

    [SerializeField]
    GameObject playerPrefab;  

    // Start is called before the first frame update
    void Start()
    {
        
        if (PhotonNetwork.IsConnected)
        {
            if (playerPrefab != null)
            {
                int randomPoint = Random.Range(-20, 20);

                PhotonNetwork.Instantiate(playerPrefab.name, new Vector2(randomPoint, 0),Quaternion.identity) ;
            }
        }


    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void OnJoinedRoom()
    {
        Debug.Log(PhotonNetwork.NickName + " is joined to " + PhotonNetwork.CurrentRoom.Name);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log(newPlayer.NickName + " joined to " + PhotonNetwork.CurrentRoom.Name + " " + PhotonNetwork.CurrentRoom.PlayerCount);
    }

}
