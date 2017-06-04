using UnityEngine;
using System.Collections;

public class bricksCounter : MonoBehaviour {

    private int user=0;
    private int opponent=0;

    private PhotonView photonView;
    private MultiplayerNetworkConnection mp;


    // Use this for initialization
    void Start () {
        user = 0;
        opponent = 0;

        photonView = GetComponent<PhotonView>();
        mp = (MultiplayerNetworkConnection)FindObjectOfType(typeof(MultiplayerNetworkConnection));
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    public int getUserBricks() { return user;  }
    public int getOppoBricks() { return opponent;  }

    public void AddToUser(int value) { user += value;  }
    public void AddToOpponent(int value) { opponent += value; }

    public void RemoveUsersBricks(int value) { user -= value;  }
    public void RemoveOppoBricks(int value) { opponent -= value; }



    // Update is called once per frame
    public void Call_RPC_forBricks()
    {
        if (mp.gameStarted)
        {
            photonView.RPC("RecieveBricks", PhotonTargets.OthersBuffered, new object[] {
                opponent
            });
        }
        opponent = 0;
    }

    [PunRPC]
    public void RecieveBricks(int bricks)
    {
        user = bricks;
    }

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    }


}