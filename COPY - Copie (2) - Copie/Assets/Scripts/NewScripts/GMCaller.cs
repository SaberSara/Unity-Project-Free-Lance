using UnityEngine;
using System.Collections;

public class GMCaller : MonoBehaviour {

	private GameManager gm;
	private ShapesManager sm;
	private SP_ShapesManager s_m;
	private UILabel userMoves, compMoves;
    private SoundManager sounds;

    public UILabel Status;
    public UILabel enteredString;
    public UILabel ModeSelected;

    public GameObject gameButtons;

    private MultiplayerNetworkConnection mnc;

    public bool allowChangeStatus=false;
    // Use this for initialization
    void Start () {
        mnc = (MultiplayerNetworkConnection)FindObjectOfType(typeof(MultiplayerNetworkConnection));
        gm = (GameManager)FindObjectOfType (typeof(GameManager));
		sm = (ShapesManager)FindObjectOfType (typeof(ShapesManager));
		s_m = (SP_ShapesManager)FindObjectOfType (typeof(SP_ShapesManager));
        sounds = (SoundManager)FindObjectOfType(typeof(SoundManager));
	}

	public void PlayGame(){
        Time.timeScale = 1;
		sounds.resumeMusic ();
        gm.play_game();
	}

	public void GameMenu(){
		Time.timeScale = 1;
		gm.game_menu ();
	}

	public void CharacterSelection(){
		gm.character_selection ();
	}

	public void PauseGame(){
		gm.pause_game ();
	}

    public void StartScreen()
    {
        gm.start_screen();
    }

	public void SelectMode(){
        if (Application.loadedLevelName == "StartScreen")
            CollidersEnableOrDisable (false, false,false);
       else
            CollidersEnableOrDisable(false, true, false);
        gm.mode_selection ();
	}

	public void GameSettings(){
		CollidersEnableOrDisable (false, true, false);
		gm.game_settings ();
	}

	public void ShopPopup(){
		//CollidersEnableOrDisable (false, true, false);
		gm.shop_popup ();
	}

	public void ShopList(){
		gm.shop_list();
	}

	public void BackgroundSelection(){
		gm.background_selection ();
	}

	public void ExitGame(){
		gm.exit_game();
	}

	public void SinglePlayerGame(){
		gm.single_player_game ();
	}

	public void TimeBasedLevel(){
		gm.time_based_level ();
	}

	public void MultiPlayerGame(){
		gm.multi_player_game();
	}

	public void ExitCurrentLevel(){
        Time.timeScale = 1;
        if (Application.loadedLevelName == "StartScreen")
            CollidersEnableOrDisable(true, false, true);
        else if(Application.loadedLevelName == "Menu")
            CollidersEnableOrDisable(true, true, false);

        gm.setIsPauseMenuOpen(false);
        gm.sound.resumeMusic();
		Destroy (this.transform.parent.gameObject);
	}

	public void GameRestart(){
		gm.setIsPauseMenuOpen(false);
        gm.sound.resumeMusic();
        //sm.readyGoState = false;
        if (Application.loadedLevelName == "mainGame")
            gm.single_player_game();
        else if (Application.loadedLevelName == "mainGameTimeBased")
            gm.single_player_game_time_based();
        else if (Application.loadedLevelName == "MultiplayerMoveBased")
        {
            PhotonNetwork.Disconnect();
            gm.multi_player_game();
        }
	}

	private void CollidersEnableOrDisable(bool check, bool menu, bool cmd){

        if (menu)
        {
            var menuButton = GameObject.Find("MenuButtons").GetComponentsInChildren<BoxCollider>();
            var menuGrid = GameObject.Find("Grid").GetComponentsInChildren<BoxCollider>();

            foreach (var c in menuButton)
                c.enabled = check;

            foreach (var g in menuGrid)
                g.enabled = check;
        }
	}

   
	public void LoadingScreen(){
		gm.loading_screen ();
	}

    public void MainGamePlayExit()
    {
        gm.main_game_play_exit();
    }

    public void setCreateRoom()
    {
        ModeSelected.text = "Create Room";
        gm.createRoom = true;
        gm.joinRoom = false;
    }
    public void setJoinRoom()
    {
        ModeSelected.text = "Join Room";
        gm.createRoom = false;
        gm.joinRoom = true;
    }

    public void enableRandomOrCustom()
    {
        gameButtons.SetActive(true);
      //  GameObject.Find("RandomOrCustom").SetActive(true);
        GameObject.Find("SingleOrMulti").SetActive(false);
        gm.multiplayerSceneSelected = true;
    }

    public void setMultiSceneSelectFalse()
    {
        gm.multiplayerSceneSelected = false;
    }

    public void setRandomMode()
    {
        gm.randomMode = true;
    }

    public void setCustomMode()
    {
        gm.randomMode = false;
    }

    public void SearchForRooms()
    {
        gm.RoomName = enteredString.text;
        allowChangeStatus = true;
        mnc.startMatchmaking();
    }

    /// <summary>
    /// room name must be unique
    /// </summary>
    /// <param name="name"> Name with room need to be created</param>
    /// <returns> </returns>
    public bool CheckRoomNames()
    {
        allowChangeStatus = false;
        var rooms = PhotonNetwork.GetRoomList();
        Debug.Log(PhotonNetwork.countOfRooms);
        
        foreach (var item in rooms)
        {
            Debug.Log("yyo");
            Debug.Log(item.name);
            if (enteredString.text == item.name)
            {
                Status.gradientTop = Color.red;
                Status.gradientBottom = Color.red;
                Status.text = "Room name Already Exists";
                return false;
            }
        }
        gm.RoomName = enteredString.text;
        Destroy(this.transform.parent.gameObject);
        return true;
    }

    public void LoadMenu()
    {
        Application.LoadLevel("Maps");
    }

    /// <summary>
    /// if the room name to join exists or not
    /// </summary>
    /// <returns></returns>
    public bool checkIfRoomAvailable()
    {
        allowChangeStatus = false;
        
        Debug.Log(PhotonNetwork.countOfRooms);
        var rooms = PhotonNetwork.GetRoomList();
        Debug.Log(rooms.Length);
        
        foreach (var item in rooms)
        {
            Debug.Log("yyo");
            Debug.Log(item.name);
            if (enteredString.text == item.name)
            {
                gm.RoomName = enteredString.text;
                Destroy(this.transform.parent.gameObject);
                return true;
            }
        }
        Status.gradientTop = Color.red;
        Status.gradientBottom = Color.red;
        
        Status.text = "Room Not Availabe";
        return false;
    }

    public void DisplayMessage(string msg, Color c)
    {
        allowChangeStatus = false;
        Status.gradientTop = c;
        Status.gradientBottom = c;
        Status.text = msg;
    }

    public void quitPopUp()
    {
        Destroy(this.transform.parent.gameObject);
    }
}
