using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadingBar : MonoBehaviour {

	private AsyncOperation loadLevelbar;
	private UIProgressBar loadingBarMahool;
    private string levelName;
    private GameManager gm;

	// Use this for initialization
	void Start () {
        gm = (GameManager)FindObjectOfType(typeof(GameManager));
        if (gm.previousLevel == "StartScreen")
        {
            levelName = "CharacterSelectionMenu";
        }
        else if (gm.previousLevel == "CharacterSelectionMenu" || gm.previousLevel == "mainGame" || gm.previousLevel == "Menu" || gm.previousLevel == "mainGameTimeBased")
        {
            if (gm.multiplayerSceneSelected)
            {
                gm.multiplayerSceneSelected = false;
                levelName = "MultiplayerMoveBased";
            }
            else
            {
                levelName = "Maps";
            }
        }
      /*  else if(gm.previousLevel == "JoinAndCreate")
        {
            levelName = "MultiplayerMoveBased";
        }*/
        else if (gm.previousLevel == "Maps")
        {
            levelName = gm.levelToLoad;
        }
        else if (gm.previousLevel == "MultiplayerMoveBased")
        {
            levelName = "Menu";
        }

        gm.setIsPauseMenuOpen(false);
        Time.timeScale = 1;
        
		loadLevelbar = SceneManager.LoadSceneAsync (levelName);
		//SceneManager.LoadScene (levelName);
		loadingBarMahool = (UIProgressBar)FindObjectOfType (typeof(UIProgressBar));
	}
	
	// Update is called once per frame
	void Update () {
		loadingBarMahool.value = loadLevelbar.progress;
	}
}
