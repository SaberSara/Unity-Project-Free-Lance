using UnityEngine;
using System.Collections;

public class ObserveTime : MonoBehaviour {

    private MultiplayerNetworkConnection mp;
    private MultiPlayerShapesManager mpsm;
    private GameManager gm;
    private StatManager sm;
    private PhotonView photonView;

    // Use this for initialization
    void Start () {
        gm = (GameManager)FindObjectOfType(typeof(GameManager));
        sm = (StatManager)FindObjectOfType(typeof(StatManager));
        photonView = GetComponent<PhotonView>();
        mp = (MultiplayerNetworkConnection)FindObjectOfType(typeof(MultiplayerNetworkConnection));
        mpsm = (MultiPlayerShapesManager)FindObjectOfType(typeof(MultiPlayerShapesManager));
    }

    public void Call_RPC_forTimer()
    {
        sm.setTimerCounter(sm.getTimerCounter() - Time.deltaTime);
        this.transform.GetChild(1).GetComponent<UILabel>().text = sm.getTimerCounter().ToString("F0");
        photonView.RPC("newFunc", PhotonTargets.AllBufferedViaServer, new object[] {
                int.Parse (this.transform.GetChild (1).GetComponent<UILabel> ().text)
            });
    }
    

    [PunRPC]
    public void newFunc(int time)
    {
        if (PhotonNetwork.player != PhotonNetwork.masterClient)
        {
            sm.setTimerCounter(time);
            this.transform.GetChild(1).GetComponent<UILabel>().text = time.ToString();

            if (time <= 0)
            {
                mpsm.movesOver = true;
                gm.GameOver();
            }
        }
    }
    
    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    }

}
