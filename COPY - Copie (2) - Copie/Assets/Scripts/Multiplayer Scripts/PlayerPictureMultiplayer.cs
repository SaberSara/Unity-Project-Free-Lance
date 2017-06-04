using UnityEngine;

public class PlayerPictureMultiplayer : MonoBehaviour {

    private FacebookConnection fbc;
    private StatManager sm;

    // Use this for initialization
    void Start()
    {
        sm = (StatManager)FindObjectOfType(typeof(StatManager));
        fbc = (FacebookConnection)FindObjectOfType(typeof(FacebookConnection));
        sm.fbc.setUserProfilePicture(fbc.ShowLogedInUserProfilePicture(this.gameObject.GetComponent<SpriteRenderer>()));
    }
}
