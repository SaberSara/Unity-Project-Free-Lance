using UnityEngine;
using System.Collections.Generic;
using Facebook.Unity;
using System;
using UnityEngine.UI;
using Facebook.MiniJSON;
using System.Collections;

public class FacebookConnection : MonoBehaviour
{

    private GameObject loginButton;
    private GameObject logoutButton;

    public bool facebookLoginStatus = false;
    private SpriteRenderer userImage;
    private SpriteRenderer user2Image;
    private UILabel userName;
    private string usr_id;
    public string playerId;
    private SpriteRenderer userImageById;
    private UI2DSprite userImage2D, popupScreenImage;

    private List<UserData> user_data;

    private List<UI2DSprite> userPicList;

    private StatManager st;


    // Use this for initialization
    public static FacebookConnection stInstance;

    public static FacebookConnection instance
    {
        get
        {
            if (stInstance == null)
            {
                stInstance = GameObject.FindObjectOfType<FacebookConnection>();
                DontDestroyOnLoad(stInstance.gameObject);
            }
            return stInstance;
        }
    }


    void Awake()
    {
        if (stInstance == null)
        {
            stInstance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            if (this != stInstance)
            {
                Destroy(this.gameObject);
            }
        }
    }


    // Use this for initialization
    void Start()
    {
        st = (StatManager)FindObjectOfType(typeof(StatManager));
        userPicList = new List<UI2DSprite>();
        loginButton = GameObject.Find("FacebookLogin").transform.GetChild(0).gameObject;
        logoutButton = GameObject.Find("FacebookLogin").transform.GetChild(1).gameObject;
    }

    public void SetFacebookLoginInMaps(GameObject loginB, GameObject logoutB)
    {
        loginButton = loginB;
        logoutButton = logoutB;
    }

    public void FacebookConnectionFunction()
    {
        if (loginButton.GetActive())
        {
            if (!FB.IsInitialized)
            {
                Debug.Log("Initializing sdk");
                // Initialize the Facebook SDK
                FB.Init(OnInitialize, OnGameShown);
            }
            else
            {
                Debug.Log("actiavting, already initialized");
                // Already initialized, signal an app activation App Event
                FB.ActivateApp();
                FBlogin();

            }
        }
        else if (logoutButton.GetActive())
        {
            OnFacebookLogout();
        }
    }

    private void OnInitialize()
    {
        Debug.Log("Facebook Initialized Successfully! =============================>");

        if (FB.IsInitialized)
        {
            // Signal an app activation App Event
            FB.ActivateApp();
            // Continue with Facebook SDK

            if (!FB.IsLoggedIn)
            {
                FBlogin();
            }

        }
        else
        {
            Debug.Log("Failed to Initialize the Facebook SDK");
        }
    }

    private void OnGameShown(bool isGameShown)
    {
        if (!isGameShown)
        {
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
        }
    }

    void FBlogin()
    {
        var permsWithRead = new List<string>() { "public_profile", "email", "user_friends" };
        var permsWithWrite = new List<string>() { "publish_actions" };

        /* string url = "https" + "://graph.facebook.com/" + AccessToken.CurrentAccessToken.UserId + "/scores";
         url += "?access_token=" + AccessToken.CurrentAccessToken.TokenString;
         WWW www = new WWW(url);*/

        FB.LogInWithReadPermissions(permsWithRead, OnAuthCallBack);
        FB.LogInWithPublishPermissions(permsWithWrite, OnAuthCallBack);
    }

    private void OnAuthCallBack(ILoginResult result)
    {
        if (FB.IsLoggedIn)
        {
            HandleFacebookButtonsVisibility(true);
            // Do your work here...
            facebookLoginStatus = true;
            //  MobileNativeMessage message = new MobileNativeMessage("", "", "OK");
            ShowLogedInUserID();
            st.QueryScores();


            if (Application.loadedLevelName == "StartScreen")
            {
                ShowLogedInUserName(GameObject.Find("FacebookLogin").transform.GetChild(1).transform.GetChild(0).GetComponent<UILabel>());

            }
        }
        else
        {
            facebookLoginStatus = false;

            MobileNativeMessage message = new MobileNativeMessage("Alert", "Login Failed!", "OK");
            Debug.Log("Login Failed!");

        }
    }



    public void HandleFacebookButtonsVisibility(bool isLoggedIn)
    {
        if (isLoggedIn)
        {
            logoutButton.SetActive(true);
            loginButton.SetActive(false);
        }
        else
        {
            logoutButton.SetActive(false);
            loginButton.SetActive(true);
        }

    }

    public Sprite ShowLogedInUserProfilePicture(SpriteRenderer imgContainer)
    {
        userImage = imgContainer;
        if (facebookLoginStatus)
        {
            FB.API(Util.GetPictureURL("me", 128, 128), HttpMethod.GET, PictureAPICallback);
        }
        else
        {
            //Default Picture
        }
        return userImage.sprite;
    }

    public void OnFacebookLogout()
    {
        if (FB.IsLoggedIn)
        {
            FB.LogOut();
            StartCoroutine("CheckForSuccussfulLogout");
            st.FBloginMahoolSuccess = false;
        }
    }

    IEnumerator CheckForSuccussfulLogout()
    {
        if (FB.IsLoggedIn)
        {
            yield return new WaitForSeconds(0.1f);
            StartCoroutine("CheckForSuccussfulLogout");
        }
        else
        {
            // Here you have successfully logged out.
            // Do whatever you want as I do, I just enabled Login Button and Disabled
            // logout button through this method.
            //
            HandleFacebookButtonsVisibility(false);
            facebookLoginStatus = false;

            MobileNativeMessage message = new MobileNativeMessage("Logout", "You are Successfully Logout!", "OK");
            Debug.Log("LOGGED OUTT!!!");
        }
    }

    public string ShowLogedInUserName(UILabel userNameContainer)
    {
        userName = userNameContainer;
        if (facebookLoginStatus)
        {
            FB.API("/me?fields=id,first_name", HttpMethod.GET, NameAPICallback);
        }
        else
        {
            //Default Name
            // userName.text = "Player";
            if (st.playerNumber == "Player1")
            {
                userName.text = "Player 1";
            }
            else if (st.playerNumber == "Player2")
            {
                userName.text = "Player 2";
            }
        }

        return userName.text;

    }

    private void PictureAPICallback(IGraphResult result)
    {
        if (result.Error != null)
        {
            // FB.API("/me?fields=id,name,friends.limit(100).fields(name,id)", HttpMethod.GET, APICallback);  
            FB.API(Util.GetPictureURL("me", 128, 128), HttpMethod.GET, PictureAPICallback);
            return;
        }
        userImage.sprite = Sprite.Create(result.Texture, new Rect(0, 0, 128, 128), new Vector2(0.5f, 0.5f));
    }

    private void NameAPICallback(IGraphResult result)
    {
        if (result.Error != null)
        {
            // FB.API("/me?fields=id,name,friends.limit(100).fields(name,id)", HttpMethod.GET, APICallback);  
            FB.API("/me?fields=id,first_name", HttpMethod.GET, NameAPICallback);
            return;
        }

        var profileInfo = Json.Deserialize(result.RawResult) as IDictionary;
        userName.text = profileInfo["first_name"].ToString();
        playerId = profileInfo["id"].ToString();
        // Debug.Log("===================> "+playerId+" <=======================");
    }

    public Sprite ShowLogedInUserProfilePictureByUserID(SpriteRenderer imgContainer, string uid)
    {
        usr_id = uid;
        user2Image = imgContainer;
        if (facebookLoginStatus)
        {
            FB.API(Util.GetPictureURL(usr_id, 128, 128), HttpMethod.GET, PictureAPICallbackByID);
        }
        else
        {
            //Default Picture
        }
        return user2Image.sprite;
    }

    private void PictureAPICallbackByID(IGraphResult result)
    {
        if (result.Error != null)
        {
            // FB.API("/me?fields=id,name,friends.limit(100).fields(name,id)", HttpMethod.GET, APICallback);  
            FB.API(Util.GetPictureURL(usr_id, 128, 128), HttpMethod.GET, PictureAPICallbackByID);
            return;
        }
        user2Image.sprite = Sprite.Create(result.Texture, new Rect(0, 0, 128, 128), new Vector2(0.5f, 0.5f));
    }


    public UI2DSprite ShowProfilePictureForPopupScreens(UI2DSprite imgContainer2D, string uid)
    {
        usr_id = uid;
        popupScreenImage = imgContainer2D;

        if (facebookLoginStatus)
        {
            FB.API(Util.GetPictureURL(usr_id, 128, 128), HttpMethod.GET, PictureAPICallback2DPopup);
        }
        else
        {
            //Default Picture
        }

        return popupScreenImage;
    }
    private void PictureAPICallback2DPopup(IGraphResult result)
    {
        if (result.Error != null)
        {
            // FB.API("/me?fields=id,name,friends.limit(100).fields(name,id)", HttpMethod.GET, APICallback);  
            FB.API(Util.GetPictureURL(usr_id, 128, 128), HttpMethod.GET, PictureAPICallback);
            return;
        }
        popupScreenImage.sprite2D = Sprite.Create(result.Texture, new Rect(0, 0, 128, 128), new Vector2(0.5f, 0.5f));
    }

    public UI2DSprite ShowProfilePictureForMaps(UI2DSprite imgContainer2D)
    {
        userImage2D = imgContainer2D;
        if (facebookLoginStatus)
        {
            FB.API(Util.GetPictureURL("me", 128, 128), HttpMethod.GET, PictureAPICallback2D);
        }
        else
        {
            //Default Picture
        }
        return userImage2D;
    }
    private void PictureAPICallback2D(IGraphResult result)
    {
        if (result.Error != null)
        {
            // FB.API("/me?fields=id,name,friends.limit(100).fields(name,id)", HttpMethod.GET, APICallback);  
            FB.API(Util.GetPictureURL("me", 128, 128), HttpMethod.GET, PictureAPICallback);
            return;
        }
        userImage2D.sprite2D = Sprite.Create(result.Texture, new Rect(0, 0, 128, 128), new Vector2(0.5f, 0.5f));
    }

    public string ShowLogedInUserID()
    {
        if (facebookLoginStatus)
        {
            FB.API("/me?fields=id,first_name", HttpMethod.GET, UserIDAPICallback);
        }

        return usr_id;

    }
    private void UserIDAPICallback(IGraphResult result)
    {
        if (result.Error != null)
        {
            // FB.API("/me?fields=id,name,friends.limit(100).fields(name,id)", HttpMethod.GET, APICallback);  
            FB.API("/me?fields=id,first_name", HttpMethod.GET, NameAPICallback);
            return;
        }

        var profileInfo = Json.Deserialize(result.RawResult) as IDictionary;
        // st.fbc.setUserID(profileInfo["id"].ToString());
        usr_id = profileInfo["id"].ToString();
        st.loggedInUserID = usr_id;
        // Debug.Log("===================> "+ usr_id + " <=======================");
    }

    #region Friesnds Pictures on Maps

    bool jobDone = true;
    UserData index;

    public void GenerateFriendsPicturesOnMap(List<UserData> ud)
    {
        user_data = ud;
        StartCoroutine(WaitForJobDone(jobDone));
    }

    IEnumerator WaitForJobDone(bool jd)
    {
        foreach (var i in user_data)
        {
            if (facebookLoginStatus)
            {
                jobDone = false;
                index = i;
                PicCall(i);
                if (!jobDone)
                {
                    yield return new WaitForSeconds(2f);
                    jobDone = false;
                    PicCall(i);
                }
            }
        }
    }

    private void PicCall(UserData i)
    {
        FB.API(Util.GetPictureURL(i.uid, 128, 128), HttpMethod.GET, FriendsPictureAPICallback);
    }

    private void FriendsPictureAPICallback(IGraphResult result)
    {
        if (result.Error != null)
        {
            // FB.API("/me?fields=id,name,friends.limit(100).fields(name,id)", HttpMethod.GET, APICallback);  
            FB.API(Util.GetPictureURL(usr_id, 128, 128), HttpMethod.GET, PictureAPICallback);
            return;
        }
        if (Application.loadedLevelName == "Maps")
        {
            index.obj.GetComponent<UI2DSprite>().sprite2D = Sprite.Create(result.Texture, new Rect(0, 0, 128, 128), new Vector2(0.5f, 0.5f));
        }
        jobDone = true;
    }


    #endregion

    #region SharePost
    public void ShareWithFriends()
    {
        Uri picLink = new Uri("http://candycrushsaga.com/img/ccs/color-bomb.png");
        Uri shareLink = new Uri("http://app.facebook.com/" + FB.AppId + "/?challenge_brag=" + (FB.IsLoggedIn ? usr_id : "guest"));
        string toId = null,
            linkCaption = "I'm playing Combat Crush, Check this game out.",
            linkName = "Check out this Game",
            linkDescription = null,
            mediaSource = null;

        FB.FeedShare(toId, shareLink, linkName, linkCaption, linkDescription, picLink, mediaSource, FeedCallBack);
    }

    private void FeedCallBack(IShareResult result)
    {
        Debug.Log("FeedCallBack Method Call!");
    }
    #endregion


    public void InviteNewFriends()
    {
        FB.AppRequest(
            "Come play this great game!",
            null, null, null, null, null, null,
            delegate (IAppRequestResult result)
            {
                Debug.Log(result.RawResult);
            }
        );
    }

    public void InviteFriendForMatchmaking()
    {

    }

    #region BragFriends

    public void BragToFriends()
    {
        FB.AppRequest(
            "I beat you!",
            null,
            new List<object>() { "app_users" },
            null, null, null, null,
            delegate (IAppRequestResult result)
            {
                Debug.Log(result.RawResult);
            }
        );
    }

    #endregion
}

