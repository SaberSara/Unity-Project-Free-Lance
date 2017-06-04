using UnityEngine;
using System.Collections;

public class ObservePlayerPicture : MonoBehaviour {

    private PhotonView photonView;
    private MultiplayerNetworkConnection mp;
    private StatManager sm;

    void Start()
    {
        sm = (StatManager)FindObjectOfType(typeof(StatManager));
        photonView = GetComponent<PhotonView>();
        mp = (MultiplayerNetworkConnection)FindObjectOfType(typeof(MultiplayerNetworkConnection));
    }

    public void Call_RPC_forPlayerPicture()
    {
        if (mp.gameStarted)
        {
            photonView.RPC("newFunc", PhotonTargets.OthersBuffered, new object[] {
               this.transform.GetChild(0).GetComponent<SpriteRenderer> ().sprite
            });
        }
    }

    [PunRPC]
    public void newFunc(Sprite userPic)
    {
        if (PhotonNetwork.player == PhotonNetwork.masterClient)
            Debug.Log("Player 1");
        else
            Debug.Log("Player 2");


        GameObject.Find("Player2Pic").GetComponent<SpriteRenderer>().sprite = userPic;
    }

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    { }

}
