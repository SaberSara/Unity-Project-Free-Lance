using UnityEngine;
using System.Collections;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.SocialPlatforms;

public class PlayServicesConnection : MonoBehaviour {

    private StatManager st;

	public static PlayServicesConnection playServiceInstance;
	public static PlayServicesConnection instance
	{
		get
		{
			if(playServiceInstance == null)
			{
				playServiceInstance = GameObject.FindObjectOfType<PlayServicesConnection>();
				DontDestroyOnLoad(playServiceInstance.gameObject);
			}
			return playServiceInstance;
		}
	}
	
	void Awake () {	

		if (playServiceInstance == null) {
			playServiceInstance = this;
			DontDestroyOnLoad (this);
		} else {
			if(this!=playServiceInstance){
				Destroy(this.gameObject);
			}
		}

		PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder().Build();
		PlayGamesPlatform.InitializeInstance(config);
		// recommended for debugging:
		PlayGamesPlatform.DebugLogEnabled = true;
		// Activate the Google Play Games platform
		PlayGamesPlatform.Activate();
		 
	}

	void Start(){
        st = (StatManager)FindObjectOfType(typeof(StatManager));
		//gm = (gameManager)FindObjectOfType(typeof(gameManager));
		LoginFunction ();
	}

	public void LoginFunction(){
		// authenticate user:
		Social.localUser.Authenticate((bool success) => {
			if(success){
				//playServiceLogIn.enabled = false;
			//	gm.SaveGameScores();
				Debug.Log("Successfully LogedIn!");
			}else{
				//playServiceLogIn.enabled = true;
				Debug.Log("Login Unsuccessfull!");
			}
		});
	}

	public void LogoutFuntion(){
		PlayGamesPlatform.Instance.SignOut();
	}

	public void UnlockAchievement(string achievementID){
		Social.ReportProgress(achievementID, 100.0f, (bool success) => {
			if(success){
				Debug.Log("Achievement successfully Unlocked!");
			}else{

				Debug.Log("Achievement Unlocked Unsuccessfull!");
			}
		});
	}

	public void PostingSocreOnLeaderBoard(string leaderBoardID, int score){
		// post score 12345 to leaderboard ID "Cfji293fjsie_QA")
		Social.ReportScore(score, leaderBoardID, (bool success) => {
            // handle success or failure
            if (success)
            {
                Debug.Log("Score Posted successfully!");
            }
            else
            {

                Debug.Log("Score Posting Unsuccessfull!");
            }
        });
	}

	public void DisplayAchievementsUI(){
		Social.ShowAchievementsUI();
	}

	public void DisplayLeaderBoardUI(){
		Social.ShowLeaderboardUI();
	}
}
