using UnityEngine;
using System.Collections;

public class ObservePlayerScore : MonoBehaviour {

	private StatManager sm;
	private PhotonView photonView;
	private MultiplayerNetworkConnection mp;
    private GameManager gm;
    private UILabel NewScore2Text;
    public CharacterActionsOpponent oppoAnim;

    private int prevScore=0;


    public bool AllowScoreUpdate = false;

	// Use this for initialization
	void Start () {
		sm = (StatManager)FindObjectOfType (typeof(StatManager));
		photonView = GetComponent<PhotonView> ();
		mp = (MultiplayerNetworkConnection)FindObjectOfType (typeof(MultiplayerNetworkConnection));
        gm = (GameManager)FindObjectOfType(typeof(GameManager));
        //      oppoAnim = (CharacterActionsOpponent)FindObjectOfType(typeof(CharacterActionsOpponent));
  //      GameObject.Find("OpponentCharacters").transform.GetChild(gm.OppoCharacterID);
   //     Debug.Log(oppoAnim.gameObject.name);
        NewScore2Text = GameObject.Find("Score2").transform.GetChild(1).GetComponent<UILabel>();
    }


    public void Call_RPC_forScore () {
		if(mp.gameStarted){
			photonView.RPC ("newFunc", PhotonTargets.OthersBuffered, new object[] {
				int.Parse (this.transform.GetChild (1).GetComponent<UILabel> ().text)
			});
		}
	}



    [PunRPC]
	public void newFunc(int score){


        
        //opponent character animation

        sm.set_oppoScore(score);
        NewScore2Text.text = sm.get_oppoScore().ToString();

        if ((score - prevScore) > Constants.minAttackScore && (score - prevScore) < Constants.minSuperAttackScore)
        {
            oppoAnim.Attack();
            StartCoroutine(waitFunction(Constants.AnimationDuration));
        }
        else if ((score - prevScore) >= Constants.minSuperAttackScore)
        {
            oppoAnim.SuperAttack();
            StartCoroutine(waitFunction(Constants.AnimationDuration));
        }

        prevScore = score;


    }

    private IEnumerator waitFunction(float x)
    {
        yield return new WaitForSeconds(x);
        oppoAnim.Idle();
    }

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info){
	}
}
