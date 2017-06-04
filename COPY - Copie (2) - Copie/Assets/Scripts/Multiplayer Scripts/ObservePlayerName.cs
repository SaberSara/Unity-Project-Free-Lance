using UnityEngine;
using System.Collections;

public class ObservePlayerName : MonoBehaviour
{

    private PhotonView photonView;
    private MultiplayerNetworkConnection mp;
    private StatManager sm;

    void Start()
    {
        sm = (StatManager)FindObjectOfType(typeof(StatManager));
        photonView = GetComponent<PhotonView>();
        mp = (MultiplayerNetworkConnection)FindObjectOfType(typeof(MultiplayerNetworkConnection));
    }

    public void Call_RPC_forPlayerName()
    {
        if (mp.gameStarted)
        {
            photonView.RPC("newFunc", PhotonTargets.OthersBuffered, new object[] {
                this.GetComponent<UILabel> ().text
            });
        }
    }

    [PunRPC]
    public void newFunc(string userName)
    {
        if (PhotonNetwork.player == PhotonNetwork.masterClient)
            Debug.Log("Player 1");
        else
            Debug.Log("Player 2");

        Debug.Log("Second PlayerName: "+userName);
       GameObject.Find("Player2").GetComponent<UILabel>().text = userName;
    }

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    { }
}