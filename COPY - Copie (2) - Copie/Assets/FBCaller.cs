using UnityEngine;
using System.Collections;
using Facebook.Unity;

public class FBCaller : MonoBehaviour {

    private FacebookConnection fbc;
    private Vector2 levelPos;
    private StatManager sm;
    private bool oneTimeBaby = false;


    // Use this for initialization
    void Start () {
        sm = (StatManager)FindObjectOfType(typeof(StatManager));
        fbc = (FacebookConnection)FindObjectOfType(typeof(FacebookConnection));

        levelPos = GameObject.Find(sm.sc.getLevelsUnlocked().ToString()).transform.position;
        levelPos.y = levelPos.y + 0.21f;
        GameObject.Find("UserDPLevel").transform.position = levelPos;
        GameObject.Find("UserDPLevel").transform.parent = GameObject.Find(sm.sc.getLevelsUnlocked().ToString()).transform;

        fbc.SetFacebookLoginInMaps(GameObject.Find("FacebookLogin").transform.GetChild(0).gameObject, GameObject.Find("FacebookLogin").transform.GetChild(1).gameObject);

        if (FB.IsLoggedIn)
        {
            fbc.HandleFacebookButtonsVisibility(true);
           // fbc.ShowProfilePictureForMaps(GameObject.Find("FacebookLogin").transform.GetChild(1).GetComponent<UI2DSprite>());
            fbc.ShowProfilePictureForMaps(GameObject.Find("UserDPLevel").GetComponent<UI2DSprite>());
            fbc.ShowLogedInUserName(GameObject.Find("FacebookLogin").transform.GetChild(1).transform.GetChild(1).GetComponent<UILabel>());
        }
        else
        {
            fbc.HandleFacebookButtonsVisibility(false);
        }
    }
	
	public void FacebookLogin()
    {
        fbc.FacebookConnectionFunction();
    }
    /// <summary>
    /// 
    public void ShowPicture()
    {
        fbc.ShowProfilePictureForMaps(GameObject.Find("UserDPLevel").GetComponent<UI2DSprite>());
    }
    ///
    /// 
    /// 
    /// </summary>

    void Update()
    {
        if (FB.IsLoggedIn && !oneTimeBaby)
        {
            oneTimeBaby = true;
           // fbc.ShowProfilePictureForMaps(GameObject.Find("FacebookLogin").transform.GetChild(1).transform.GetChild(1).GetComponent<UI2DSprite>());
            fbc.ShowProfilePictureForMaps(GameObject.Find("UserDPLevel").GetComponent<UI2DSprite>());
            fbc.ShowLogedInUserName(GameObject.Find("FacebookLogin").transform.GetChild(1).transform.GetChild(1).GetComponent<UILabel>());
        }
    }
}
