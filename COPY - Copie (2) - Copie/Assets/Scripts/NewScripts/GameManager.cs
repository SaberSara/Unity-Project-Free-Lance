using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{

    //public int TOTAL_MOVES = 5;
    private const int DEFAULT_CHARACTER_INT = 0;
    private const string DEFAULT_CHARACTER_STRING = "Character1";
    private const int DEFAULT_BACKGROUND_INT = 0;
    private const string DEFAULT_BACKGROUND_STRING = "Nature";

    private float WIDTH = 1920f;
    private float HEIGHT = 1080f;

    private StatManager st;
    public SoundManager sound;

    private bool isPauseMenuOpen = false;
    public  bool isCongratsMenuOpen = false;
    public  bool isLevelFailedMenuOpen = false;
    private bool music = true;
    private bool sfx = true;
    private int characterSelectedInt, backgroundSelectedInt;
    private string characterSelectedString, backgroundSelectedString;
    public int OppoCharacterID = 3;

    private CharacterActions ca;
    private CharacterActionsOpponent cao;

    private bool isTryingToExit = false;
   
    public string previousLevel;
    public string levelToLoad=null;

    public bool multiplayerSceneSelected = false;

    public static GameManager gmInstance;

    public bool joinRoom = false;
    public bool createRoom = false;

    public bool randomMode = true;

    public string RoomName = "";

    public static GameManager instance
    {
        get
        {
            if (gmInstance == null)
            {
                gmInstance = GameObject.FindObjectOfType<GameManager>();
                DontDestroyOnLoad(gmInstance.gameObject);
            }
            return gmInstance;
        }
    }

    void Awake()
    {
        if (gmInstance == null)
        {
            gmInstance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            if (this != gmInstance)
            {
                Destroy(this.gameObject);
            }
        }
    }

    void Start()
    {
        Screen.sleepTimeout = (int)SleepTimeout.NeverSleep;

        st = (StatManager)FindObjectOfType(typeof(StatManager));
        sound = (SoundManager)FindObjectOfType(typeof(SoundManager));

        characterSelectedInt = DEFAULT_CHARACTER_INT;
        characterSelectedString = DEFAULT_CHARACTER_STRING;
        backgroundSelectedInt = DEFAULT_BACKGROUND_INT;
        backgroundSelectedString = DEFAULT_BACKGROUND_STRING;

        st.set_userMoves(st.getTotalMoves());
        st.set_compMoves(st.getTotalMoves());
        st.set_userScore(0);
        st.set_oppoScore(0);

        st.setTimerCounter(st.getTotalTimeInSeconds());
	//	sound.playBackgroundMusic ();
    }

    void OnGUI()
    {
        if (isTryingToExit)
        {
            GUIStyle fontSize = new GUIStyle(GUI.skin.window);
            fontSize.fontSize = 20;
            GUI.Window(0, new Rect(Screen.width / 2 - GetWidth(50), Screen.height / 2 - GetHeight(50), GetWidth(1000), GetHeight(200)), ButtonFunction, "Do you want to sex leave the Game?", fontSize);
        }
    }

    void ButtonFunction(int wid)
    {
        if (GUI.Button(new Rect(Screen.width / 2, Screen.height / 2, GetWidth(100), GetHeight(50)), "Leave"))
        {
            Debug.Log("Leave the mahool!");
            isTryingToExit = false;
        }
    }

    float GetHeight(float value)
    {
        return (Screen.height / HEIGHT) * value;
    }

    float GetWidth(float value)
    {
        return (Screen.width / WIDTH) * value;
    }

    private void OnDialogClose(MNDialogResult result)
    {
        //parsing result
        switch (result)
        {
            case MNDialogResult.YES:
                Debug.Log("Yes button pressed");
                PhotonNetwork.Disconnect();
                game_menu();
                break;
            case MNDialogResult.NO:
                Debug.Log("No button pressed");
                break;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Application.loadedLevelName == "StartScreen")
            {
                exit_game();
            }
            else if (Application.loadedLevelName == "Menu")
            {
                Application.LoadLevel("Maps");
            }
            else if (Application.loadedLevelName == "CharacterSelectionMenu")
            {
                Application.LoadLevel(Application.loadedLevel - 1);
            }
            else if (Application.loadedLevelName == "BackgroundSelection")
            {
                Application.LoadLevel("Menu");
            }
            else if (Application.loadedLevelName == "ShopPopup")
            {
                //Application.LoadLevel ();
            }
            else if (Application.loadedLevelName == "ShopList")
            {
                Application.LoadLevel("Menu");
            }
            else if (Application.loadedLevelName == "mainGame" || Application.loadedLevelName == "mainGameTimeBased")
            {
                main_game_play_exit();
            }
            else if (Application.loadedLevelName == "MultiplayerMoveBased")
            {
                //isTryingToExit = true;
                MobileNativeDialog dialog = new MobileNativeDialog("WARNING", "You will be disconnected from the game. Are you sure?", "Disconnect", "NO");
                dialog.OnComplete += OnDialogClose;
            }
        }
    }

    //=========================== METHODS =====================================

    public void GameOver()
    {
        ca = (CharacterActions)FindObjectOfType(typeof(CharacterActions));
        cao = (CharacterActionsOpponent)FindObjectOfType(typeof(CharacterActionsOpponent));

        if (Application.loadedLevelName == "mainGame")
        {
            //       Debug.Log("AnderAgya");
            

            if (st.get_userMoves() == 0 && st.get_compMoves() == 0)
            {
                if (st.compare_scores() && !isPauseMenuOpen)
                {
                    isPauseMenuOpen = true;
                    cao.Die();
                    StartCoroutine(WaitForSecondsOpponentLevelFailedFunction(1.5f));
                    //Congrats_Screen ();
                }
                else
                {
                    if (!isPauseMenuOpen)
                    {
                        isPauseMenuOpen = true;
                        ca.Die();
                        StartCoroutine(WaitForSecondsLevelFailedFunction(1.5f));
                    }
                }

            }
        }
        if (Application.loadedLevelName == "mainGameTimeBased")
        {
            ca = (CharacterActions)FindObjectOfType(typeof(CharacterActions));
            cao = (CharacterActionsOpponent)FindObjectOfType(typeof(CharacterActionsOpponent));

            if (st.getTimerCounter() <= 0)
            {
                if (st.compare_scores())
                {
                    if (!isCongratsMenuOpen)
                    {
                        cao.Die();
                        StartCoroutine(WaitForSecondsOpponentLevelFailedFunction(1.5f));
                        isCongratsMenuOpen = true;
                        //Congrats_Screen ();
                    }
                }
                else
                {
                    if (!isLevelFailedMenuOpen)
                    {
                        ca.Die();
                        StartCoroutine(WaitForSecondsLevelFailedFunction(1.5f));
                        isLevelFailedMenuOpen = true;
                    }
                }
            }
        }
        if (Application.loadedLevelName == "MultiplayerMoveBased")
        {
            ca = (CharacterActions)FindObjectOfType(typeof(CharacterActions));
            cao = (CharacterActionsOpponent)FindObjectOfType(typeof(CharacterActionsOpponent));

            if (st.getTimerCounter() <= 0)
            {
                if (st.compare_scores())
                {
                    Time.timeScale = 1;
                    if (!isCongratsMenuOpen)
                    {
                        cao.Die();
                        StartCoroutine(WaitForSecondsOpponentLevelFailedFunction(1.5f));
                        isCongratsMenuOpen = true;
                    }
                }
                else
                {
                    Time.timeScale = 1;
                    if (!isLevelFailedMenuOpen)
                    {
                        ca.Die();
                        StartCoroutine(WaitForSecondsLevelFailedFunction(1.5f));
                        isLevelFailedMenuOpen = true;
                    }
                }
            }
        }
    }

    public IEnumerator WaitForSecondsLevelFailedFunction(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if(Application.loadedLevelName == "MultiplayerMoveBased")
        {
            level_failed_screen_multiplayer();
        }
        else
        {
            level_failed();
        }
        
    }
    public IEnumerator WaitForSecondsOpponentLevelFailedFunction(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (Application.loadedLevelName == "MultiplayerMoveBased")
        {
            congrats_screen_multiplayer();
        }
        else
        {
            Congrats_Screen();
        }
        
    }

    public void congrats_screen_multiplayer()
    {
 //       Time.timeScale = 0;
        isCongratsMenuOpen = true;
        Application.LoadLevelAdditive("CongratsScreenNew");
    }

    public void level_failed_screen_multiplayer()
    {
 //       Time.timeScale = 0;
        isCongratsMenuOpen = true;
        Application.LoadLevelAdditive("LevelFailed");
    }

    public void start_screen()
    {
        isPauseMenuOpen = false;
        Application.LoadLevel("StartScreen");
    }

    public void play_game()
    {
        Time.timeScale = 1;
        isPauseMenuOpen = false;
        //isCongratsMenuOpen = false;
        //isLevelFailedMenuOpen = false;
        UnityEngine.SceneManagement.SceneManager.LoadScene("Maps");
    }

    public void main_game_play_exit()
    {
        if (!isPauseMenuOpen)
        {
            isPauseMenuOpen = true;
            pause_game();
        }

       // MobileNativeDialog dialog = new MobileNativeDialog("LEAVE GAME", "Are you sure you want to Leave the Game?", "Leave", "Exit");
       // dialog.OnComplete += OnGameLeave;
    }
   /* private void OnGameLeave(MNDialogResult result)
    {
        //parsing result
        switch (result)
        {
            case MNDialogResult.YES:
                Debug.Log("Yes button pressed");
                play_game();
                break;
            case MNDialogResult.NO:
                Debug.Log("No button pressed");
                break;
        }
    }*/

    public void time_based_level()
    {
        Time.timeScale = 1;
        Application.LoadLevel("mainGameTimeBased");
    }

    public void game_menu()
    {
        PhotonNetwork.Disconnect();
        Time.timeScale = 1;
        SceneManager.LoadScene("Menu");
        Time.timeScale = 1;

    }

    public void character_selection()
    {
        Application.LoadLevel("CharacterSelectionMenu");
    }

    public void pause_game()
    {
        sound.pauseMusic();
        Time.timeScale = 0;
        Application.LoadLevelAdditive("PauseGame");
    }

    public void background_selection()
    {
        Application.LoadLevel("BackgroundSelection");
    }

    public void mode_selection()
    {
        Application.LoadLevelAdditive("SelectMode");
    }

    public void game_settings()
    {
        Application.LoadLevelAdditive("Settings");
    }

    public void shop_popup()
    {
        Time.timeScale = 0;
        isPauseMenuOpen = true;
        Application.LoadLevelAdditive("ShopPopup");
    }

    public void shop_list()
    {
        Application.LoadLevel("ShopList");
    }

    public void single_player_game()
    {
        Time.timeScale = 1;
        st.set_userMoves(st.getTotalMoves());
        st.set_compMoves(st.getTotalMoves());
        //    st.setUserMovesLabel(st.get_userMoves().ToString());
        //   st.setCompMovesLabel(st.get_compMoves().ToString());
        Application.LoadLevel("mainGame");
    }
    public void single_player_game_time_based()
    {
        Time.timeScale = 1;
        st.setTimerCounter(st.getTotalTimeInSeconds());
        Application.LoadLevel("mainGameTimeBased");
    }

    public void multi_player_game()
    {
        //Application.LoadLevel ("mainGame");
        Time.timeScale = 1;
        st.setTimerCounter(st.getTotalTimeInSeconds());
        Application.LoadLevel("MultiplayerMoveBased");
    }
    public void exit_game()
    {
        Application.Quit();
    }

    public void level_failed()
    {
 //       Time.timeScale = 0;
        isLevelFailedMenuOpen = true;
        Application.LoadLevelAdditive("LevelFailed");
    }

    public void Congrats_Screen()
    {
 //       Time.timeScale = 0;
        isCongratsMenuOpen = true;
        st.CheckForUnlockingStuff();
        Application.LoadLevelAdditive("CongratsScreen");

    }

    public int getBackgroundSelectedInt()
    {
        return backgroundSelectedInt;
    }

    public void setBackgroundSelectedInt(int temp)
    {
        backgroundSelectedInt = temp;
    }

    public string getBackgroundSelectedString()
    {
        return backgroundSelectedString;
    }

    public void setBackgroundSelectedString(string temp)
    {
        backgroundSelectedString = temp;
    }

    public int getCharacterSelectedInt()
    {
        return characterSelectedInt;
    }

    public void setCharacterSelectedInt(int temp)
    {
        characterSelectedInt = temp;
    }

    public string getCharacterSelectedString()
    {
        return characterSelectedString;
    }

    public void setCharacterSelectedString(string temp)
    {
        characterSelectedString = temp;
    }

    public void setIsPauseMenuOpen(bool temp)
    {
        isPauseMenuOpen = temp;
    }
    public bool getIsPauseMenuOpen()
    {
       return isPauseMenuOpen;
    }

    public bool getMusicBool()
    {
        return music;
    }

    public void setMusicBool(bool temp)
    {
        music = temp;
    }

    public bool getSFXBool()
    {
        return sfx;
    }

    public void setSFXBool(bool temp)
    {
        sfx = temp;
    }

    public void loading_screen()
    {
        previousLevel = Application.loadedLevelName;
        
        //Changed _ SABER SARA _ APRIL 2017 _ TO MOVE TO THE GAME MENU SCENE INSTEAD
        Application.LoadLevel("LoadingScreen");
        //____________________
    }
}
