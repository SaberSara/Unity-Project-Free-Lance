using UnityEngine;
using System.Collections;

public class FbSocialButtonsHandler : MonoBehaviour {

    FacebookConnection fb;

	// Use this for initialization
	void Start () {
        fb = (FacebookConnection)FindObjectOfType(typeof(FacebookConnection));
        
        
	}
	
    public void ShareOnTheWall()
    {
        fb.ShareWithFriends();
    }

    public void InviteFriends()
    {
        fb.InviteNewFriends();
    }


    // Update is called once per frame
    void Update () {
	
	}
}
