using UnityEngine;
using System.Collections;

public class ObserveMoves : MonoBehaviour {
	
	private PhotonView photonView;
	private MultiplayerNetworkConnection mp;
    private StatManager sm;

    // Use this for initialization
    void Start () {
        sm = (StatManager)FindObjectOfType(typeof(StatManager));
        photonView = GetComponent<PhotonView> ();
		mp = (MultiplayerNetworkConnection)FindObjectOfType (typeof(MultiplayerNetworkConnection));
	}

	// Update is called once per frame
	public void Call_RPC_forMoves () {
		if(mp.gameStarted){
			photonView.RPC ("newFunc", PhotonTargets.OthersBuffered, new object[] {
				int.Parse (this.transform.GetChild (1).GetComponent<UILabel> ().text)
			});
		}
	}

	[PunRPC]
	public void newFunc(int moves){
		if (PhotonNetwork.player == PhotonNetwork.masterClient)
			Debug.Log ("Player 1");
		else
			Debug.Log ("Player 2");
		sm.set_compMoves (moves);
        // sm.setCompMovesUILabelObject(GameObject.Find("Moves2").transform.GetChild(1).GetComponent<UILabel>());
        // sm.setCompMovesLabel (moves.ToString());
        GameObject.Find("Moves2").transform.GetChild(1).GetComponent<UILabel>().text = moves.ToString();
    }

	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info){
		/*if (stream.isWriting) {
			if(PhotonNetwork.masterClient){
				//stream.SendNext (this.transform.GetChild (1).GetComponent<UILabel> ().text);
			}
		} else {
			if(!PhotonNetwork.masterClient){
				string temp = (string)stream.ReceiveNext ();
				sm.set_oppoScore(int.Parse(temp));
				Debug.Log ("OPPO: "+sm.get_oppoScore());
				//sm.setOpponentScoreLabel (temp);
			}
		}*/
	}
}
