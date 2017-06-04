using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using Facebook.Unity;
using System.Collections.Generic;

public class StatManager : MonoBehaviour {

	private int TOTAL_MOVES = 30;
	private float TOTAL_TIME_IN_SECONDS = 180;

    public Statistics sc ;
    public FacebookDataContainer fbc;
    private List<object> scoreslist = null;
    public List<UserScoreData> userList = null;

    public bool FBloginMahoolSuccess = false;

    private int userScore;
	private int opponentScore;
	private int userMoves;
	private int compMoves;
	private float timerCounter;
    private int candiesAllowed;

   

    private int currentLevel;

    public string playerNumber="NONE";

	public string multiMode = "NONE";

    private FacebookConnection fbConnectObject;

	public string p1score;
    public string loggedInUserID;

	private Text userMovesLabel, compMovesLabel, timerLabel, opponentScoreLabel;

    // Use this for initialization
    public static StatManager stInstance;

	public static StatManager instance
	{
		get
		{
			if(stInstance == null)
			{
				stInstance = GameObject.FindObjectOfType<StatManager>();
				DontDestroyOnLoad(stInstance.gameObject);
			}
			return stInstance;
		}
	}


	void Awake(){
		if (stInstance == null) {
			stInstance = this;
			DontDestroyOnLoad (this);
		} else {
			if(this!=stInstance){
				Destroy(this.gameObject);
			}
		}
	}

	void Start(){
        sc = new Statistics();
        candiesAllowed = 6;
        currentLevel = sc.getLevelsUnlocked();

        fbc = new FacebookDataContainer();
        
        userList = new List<UserScoreData>();

        //QueryScores();
        //Debug.Log("Level 1 Score: "+sc.getLevelScore(1));
	}

    //called only on level success, gm.congrats_screen().. 
    //checks if user has completed the world, if so then reward with one new background and character
    public void CheckForUnlockingStuff()
    {
   //     Debug.Log("Function Called");
     //   Debug.Log("Current level :  " + currentLevel.ToString());
        switch(currentLevel)
        {
           
            case 8:
                sc.UnlockBackground(1);
                sc.unlockCharacter(1);
                break;
            case 16:
                sc.UnlockBackground(2);
                sc.unlockCharacter(2);
                break;

            case 24:
                sc.UnlockBackground(3);
                sc.unlockCharacter(3);
                break;

            case 32:
                sc.UnlockBackground(4);
                sc.unlockCharacter(4);
                break;

            case 40:
                sc.unlockCharacter(5);
                break;

            default:
                break;
                
        }
        if (currentLevel == sc.getLevelsUnlocked())
        {
            sc.setLevelsUnlocked(sc.getLevelsUnlocked() + 1);

            if (FB.IsLoggedIn)
            {
                var scoreData = new Dictionary<string, string>();
                scoreData["score"] = sc.getLevelsUnlocked().ToString();
                SetOwnScore(scoreData);
            }
        }

    }

    public int getCurrentLevel()
    {
        return currentLevel;
    }

    public void setCurrentLevel(int level)
    {
        currentLevel = level;
    }


    public int getCandiesAllowed()
    {
        return candiesAllowed;
    }
    public void setCandiesAllowed(int x)
    {
        candiesAllowed = x;
    }
	public void setUserMovesLabel(string x){
		userMovesLabel.text = x;
	}

	public void setTimerLabel(string x){
		timerLabel.text = x;
	}

	public void setCompMovesLabel(string x){
		compMovesLabel.text = x;
	}

	public float getTimerCounter() {
		return timerCounter;
	}

	public void setTimerCounter(float t) {
		timerCounter = t;
	}

	public int get_userScore() {
		return userScore;
	}

	public void set_userScore(int x) {
		userScore = x;
	}

	public int get_userMoves() {
		return userMoves;
	}

	public void set_userMoves(int x) {
		userMoves = x;
	}

	public int get_compMoves() {
		return compMoves;
	}

	public void set_compMoves(int x) {
		compMoves = x;
	}


	public int get_oppoScore()
	{
		return opponentScore;
	}

	public void set_oppoScore(int x)
	{
		opponentScore = x;
	}

	public int getTotalMoves(){
		return TOTAL_MOVES;
	}
	public void setTotalMoves(int m){
		TOTAL_MOVES = m;
        userMoves = m;
        compMoves = m;
	}

	public float getTotalTimeInSeconds(){
		return TOTAL_TIME_IN_SECONDS;
	}
	public void setTotalTimeInSeconds(float t){
		TOTAL_TIME_IN_SECONDS = t;
        timerCounter = t;
	}
		
	public bool compare_scores(){
		if(userScore <= opponentScore){
			return false;
		}else {
			return true;
		}
	}

	public Text getUserMovesUILabelObject(){
		return userMovesLabel;
	}
	public Text getCompMovesUILabelObject(){
		return compMovesLabel;
	}
	public void setUserMovesUILabelObject(Text temp){
		userMovesLabel = temp;
	}
	public void setCompMovesUILabelObject(Text temp){
		compMovesLabel = temp;
	}

	public Text getTimerLabelObject(){
		return timerLabel;
	}
	public void setTimerLabelObject(Text temp){
		timerLabel = temp;
	}

	public void setOpponentScoreLabel(string temp){
		opponentScoreLabel.text = temp;
	}

	public void setOpponentScoreLabelObejct(Text temp){
		opponentScoreLabel = temp;
	}



    #region ScoreUpdationOnFacebook

    public void QueryScores()
    {
        FB.API("/app/scores?fields=score,user.limit(30)", HttpMethod.GET, ScoresCallback);
    }

    private void ScoresCallback(IGraphResult result)
    {
        if (result.Error != null)
        {
            FB.API("/app/scores?fields=score,user.limit(30)", HttpMethod.GET, ScoresCallback);
            return;
        }
        //Debug.Log("Scores result : " + result.RawResult);
        if (FB.IsLoggedIn)
        {
            // this.transform.GetChild(0).GetComponent<UILabel>().text = result.RawResult;
            scoreslist = Util.DeserializeScores(result.RawResult);
            foreach (var scores in scoreslist)
            {
                var entry = (Dictionary<string, object>)scores;
                var user = (Dictionary<string, object>)entry["user"];  //value "user from jason editor online by putting up the query"

               if (loggedInUserID != user["id"].ToString())
               {
                    UserScoreData temp = new UserScoreData();

                    temp.u_id = user["id"].ToString();
                    temp.userName = user["name"].ToString();

                    if (entry["score"].ToString() == "0")
                    {
                        temp.score = 1.ToString();
                    }
                    else
                    {
                        temp.score = entry["score"].ToString();
                    }
                    Debug.Log("ID: "+user["id"]+" Names: "+user["name"]+"     Query: "+loggedInUserID);
                    userList.Add(temp);
               }
            }
            FBloginMahoolSuccess = true;
        }
    }

    public void SetScoresByUserID(string userId, Dictionary<string, string> scoreData)
    {
        // var scoreData = new Dictionary<string, string>();
        // scoreData["Score"] = 300.ToString();
        FB.API("/" + userId + "/scores", HttpMethod.POST, delegate (IGraphResult result)
        {
            Debug.Log("Score submitted result : " + result.ToString());
        }
        , scoreData);
    }

    public void SetOwnScore(Dictionary<string, string> scoreData)
    {
        // var scoreData = new Dictionary<string, string>();
        // scoreData["Score"] = 300.ToString();
        FB.API("/me/scores", HttpMethod.POST, delegate (IGraphResult result)
        {
            Debug.Log("Score submitted result : " + result.ToString());
        }
        , scoreData);
    }


    #endregion
}

public class UserScoreData
{
    public string userName, score, u_id;

}