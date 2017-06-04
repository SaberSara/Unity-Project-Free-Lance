using UnityEngine;
using UnityEngine.UI;

public class FacebookDataContainer{

    private string userName, userId;
    private Sprite userProfilePicture;

    public FacebookDataContainer()
    {
        userId = "yo";
    }

    public string getUserID()
    {
        return userName;
    }
    public void setUserID(string uid)
    {
        userId = uid;
    }

    public string  getUserName()
    {
        return userName;
    }

    public void setUserName(string uname)
    {
       userName = uname;
    }

    public Sprite getUserProfilePicture()
    {
        return userProfilePicture;
    }

    public void setUserProfilePicture(Sprite upp)
    {
       userProfilePicture = upp;
    }

}
