using UnityEngine;
using System.Collections;

public class PlayerScoresMultiplayer : MonoBehaviour {

    private FacebookConnection fbc;
    private StatManager st;
    private ObserverPlayerID obpid;

    // Use this for initialization
    void Start () {
        fbc = (FacebookConnection)FindObjectOfType(typeof(FacebookConnection));
        st = (StatManager)FindObjectOfType(typeof(StatManager));
        obpid = (ObserverPlayerID)FindObjectOfType(typeof(ObserverPlayerID));
        DisplayScoresAndPicturesMultiplayer();
    }

    public void DisplayScoresAndPicturesMultiplayer()
    {
        if (st.playerNumber == "Player1")
        {
            fbc.ShowProfilePictureForMaps(this.transform.GetChild(0).GetComponent<UI2DSprite>());
           // fbc.ShowProfilePictureForPopupScreens(this.transform.GetChild(0).GetComponent<UI2DSprite>(),obpid.p1UsrID);
            fbc.ShowProfilePictureForPopupScreens(this.transform.GetChild(1).GetComponent<UI2DSprite>(), obpid.p2UsrID);
            this.transform.GetChild(0).transform.GetChild(1).GetComponent<UILabel>().text = st.get_userScore().ToString();
            this.transform.GetChild(1).transform.GetChild(1).GetComponent<UILabel>().text = st.get_oppoScore().ToString();            
        }
        else if(st.playerNumber == "Player2")
        {
            fbc.ShowProfilePictureForMaps(this.transform.GetChild(1).GetComponent<UI2DSprite>());
            //fbc.ShowProfilePictureForPopupScreens(this.transform.GetChild(1).GetComponent<UI2DSprite>(), obpid.p1UsrID);
            fbc.ShowProfilePictureForPopupScreens(this.transform.GetChild(0).GetComponent<UI2DSprite>(), obpid.p2UsrID);
            this.transform.GetChild(1).transform.GetChild(1).GetComponent<UILabel>().text = st.get_userScore().ToString();
            this.transform.GetChild(0).transform.GetChild(1).GetComponent<UILabel>().text = st.get_oppoScore().ToString();
        }
    }
}
