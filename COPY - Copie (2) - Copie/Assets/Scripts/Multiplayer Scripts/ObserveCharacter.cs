using UnityEngine;
using System.Collections;

public class ObserveCharacter : MonoBehaviour
{
    private StatManager sm;
    private PhotonView photonView;
    private GameManager gm;

	public bool allowedCharacterChange = false;

    // Use this for initialization
    void Start()
    {
        gm = (GameManager)FindObjectOfType(typeof(GameManager));
        photonView = GetComponent<PhotonView>();
        sm = (StatManager)FindObjectOfType(typeof(StatManager));
    }
    public void Call_RPC_forCharacterID()
    {
        //Debug.Log("RPC ka function");
        photonView.RPC("SendCharacterID", PhotonTargets.Others, new object[] {
                gm.getCharacterSelectedInt()
            });
    }

    [PunRPC]
    public void SendCharacterID(int id)
    {
        Debug.Log("YOYO ki beat pe ID: " + id);
        gm.OppoCharacterID = id;
		allowedCharacterChange = true;
    }
    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{}
}

