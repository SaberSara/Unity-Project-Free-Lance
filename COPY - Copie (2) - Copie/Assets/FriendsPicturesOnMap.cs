using UnityEngine;
using System.Collections;
using Facebook.Unity;
using System.Collections.Generic;

public class FriendsPicturesOnMap : MonoBehaviour {

    private StatManager scores;
    private FacebookConnection fbc;
    public GameObject userDpPrefab;
    private Vector3 levelPos;
    private bool oneTimeBabe = false;
    private List<UserData> userData;

    // Use this for initialization
    void Start () {
        scores = (StatManager)FindObjectOfType(typeof(StatManager));
        fbc = (FacebookConnection)FindObjectOfType(typeof(FacebookConnection));
        userData = new List<UserData>();
       // scores.QueryScores();
        
    }

    void Update()
    {
        if (scores.FBloginMahoolSuccess && !oneTimeBabe)
        {
            oneTimeBabe = true;
            SetFriendsPictures();
        }
    }

    public void SetFriendsPictures()
    {
        float count = 0.1f;
        //Debug.Log("YOOOOOOOOOOOOOOOOOOOOO: " + scores.userList.Count);
        foreach (var i in scores.userList)
        {
            Debug.Log("Level Score Facebook: "+i.score);

            levelPos = GameObject.Find(i.score).transform.position;
            levelPos.y = levelPos.y + 0.21f;
            levelPos.x = levelPos.x - count;

            GameObject tempObject = Instantiate(userDpPrefab) as GameObject;
            tempObject.transform.position = levelPos;
            tempObject.transform.parent = GameObject.Find(i.score).transform;
            tempObject.transform.localScale = new Vector3(2.5f, 1.8f, 2f);

            UserData ud = new UserData();
            ud.obj = tempObject;
            ud.uid = i.u_id;
            userData.Add(ud);

            /*UI2DSprite tempSprite = UI2DSprite.Instantiate(userDpPrefab.GetComponent<UI2DSprite>()) as UI2DSprite;
            tempSprite.transform.position = levelPos;
            tempSprite.transform.parent = GameObject.Find(i.score).transform;
            tempSprite.transform.localScale = new Vector3(2.5f,1.8f,2f);*/
            // fbc.ShowProfilePictureForPopupScreens(tempObject.GetComponent<UI2DSprite>(),i.u_id);
            count = count + 0.025f;
        }
        fbc.GenerateFriendsPicturesOnMap(userData);
    }
}

public class UserData
{
    public GameObject obj;
    public string uid;
}
