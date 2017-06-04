using UnityEngine;
using System.Collections;

public class ObserverPlayerID : MonoBehaviour {

    private PhotonView photonView;
    private MultiplayerNetworkConnection mp;
    private StatManager sm;
    private FacebookConnection fbc;

    public string p1UsrID, p2UsrID;

    // Use this for initialization
    void Start()
    {
        sm = (StatManager)FindObjectOfType(typeof(StatManager));
        photonView = GetComponent<PhotonView>();
        mp = (MultiplayerNetworkConnection)FindObjectOfType(typeof(MultiplayerNetworkConnection));
        fbc = (FacebookConnection)FindObjectOfType(typeof(FacebookConnection));
    }

    // Update is called once per frame
    public void Call_RPC_forPlayerID()
    {
        photonView.RPC("newFunc", PhotonTargets.OthersBuffered, new object[] {
               fbc.playerId
            });
    }

    [PunRPC]
    public void newFunc(string userId)
    {
        Debug.Log("========================== " + userId + " ===============================");

        if (PhotonNetwork.player.isMasterClient)
        {
            fbc.ShowLogedInUserProfilePictureByUserID(GameObject.Find("Player2Pic").GetComponent<SpriteRenderer>(), userId);
           
        }
        else if(!PhotonNetwork.player.isMasterClient)
        {
            fbc.ShowLogedInUserProfilePictureByUserID(GameObject.Find("Player1Pic").GetComponent<SpriteRenderer>(), userId);
            
        }

        if (sm.playerNumber == "Player1")
        {
            p1UsrID = userId;
        }
        else if (sm.playerNumber == "Player2")
        {
            p2UsrID = userId;
        }
    }

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    { }
}