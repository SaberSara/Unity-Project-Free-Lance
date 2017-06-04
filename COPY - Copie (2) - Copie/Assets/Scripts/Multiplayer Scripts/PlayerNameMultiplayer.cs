using UnityEngine;
using System.Collections;

public class PlayerNameMultiplayer : MonoBehaviour {

    private FacebookConnection fbc;
    private StatManager sm;

    // Use this for initialization
    void Start () {
        sm = (StatManager)FindObjectOfType(typeof(StatManager));
        fbc = (FacebookConnection)FindObjectOfType(typeof(FacebookConnection));
        sm.fbc.setUserName(fbc.ShowLogedInUserName(this.GetComponent<UILabel>()));
	}
}
