using UnityEngine;
using System.Collections;
using System;

public class MultiplayerNetworkConnection : MonoBehaviour
{

    private string VERSION = "v1.0";
    private bool playerWaiting = false;
    public bool gameStarted = false;
    private bool isRoomJoined = false;
    private float HEIGHT = 1080f;
    private float WIDTH = 1920f;

    private ShapesManager sm;
    private MultiPlayerShapesManager mp_sm;
    private GameManager gm;
    private StatManager st;

    private Vector3 LeftScorePos;
    private Vector3 RightScorePos;

    private Vector3 LeftPlayerPos;
    private Vector3 RightPlayerPos;

    private Vector3 LeftCharPos;
    private Vector3 RightCharPos;

    //  private Vector3 LeftMovesBox;
    //private Vector3 RightMovesBox;

    private Quaternion LeftCharRot;
    private Quaternion RightCharRot;

    private Vector3 LeftShufflePos;
    private Vector3 RightShufflePos;

    private GameObject RevokeInput;
    private bool isInitialized = false;

    bool justOneTimebaby = true;

    private ItemActivator characterActivation;
    private ObserveCharacter OpponentCharacter;

    private ObservePlayerScore obps;

    private GMCaller gmc;

    public GameObject status;
    public UILabel text;

    private AsyncOperation progress;
    private bool enough = false;
    // Use this for initialization
    void Start()
    {
        Time.timeScale = 0;

        OpponentCharacter = (ObserveCharacter)FindObjectOfType(typeof(ObserveCharacter));
        characterActivation = (ItemActivator)FindObjectOfType(typeof(ItemActivator));


        LeftScorePos = GameObject.Find("Score1").transform.position;
        RightScorePos = GameObject.Find("Score2").transform.position;

        LeftPlayerPos = GameObject.Find("Player1").transform.position;
        RightPlayerPos = GameObject.Find("Player2").transform.position;

        //   LeftShufflePos = GameObject.Find("NoMoreMovesG1").transform.position;
        //  RightShufflePos = GameObject.Find("NoMoreMovesG2").transform.position;

        RevokeInput = GameObject.Find("Blocker");
        //	sm = (ShapesManager)FindObjectOfType (typeof(ShapesManager));
        gm = (GameManager)FindObjectOfType(typeof(GameManager));
        st = (StatManager)FindObjectOfType(typeof(StatManager));

        obps = (ObservePlayerScore)FindObjectOfType(typeof(ObservePlayerScore));

        InitializeVariables();

        if (gm.randomMode)
        {
            status.SetActive(true);
            startMatchmaking();
        }
        else
            popUpJoinOrCreate();

    }

    public void InitializeVariables()
    {
        mp_sm = (MultiPlayerShapesManager)FindObjectOfType(typeof(MultiPlayerShapesManager));
    }

    void OnGUI()
    {
        GUIStyle myStyle = new GUIStyle(GUI.skin.window);
        myStyle.fontSize = (int)getScreenWidth(60);
        //      GUI.Label(new Rect(0, 0, 400, 50), PhotonNetwork.connectionStateDetailed.ToString());
        if (playerWaiting)
        {
            GUI.Window(0, new Rect(((Screen.width / 2) - getScreenWidth(450)), ((Screen.height / 2) - getScreenHeight(0)), getScreenWidth(900), getScreenHeight(120)), newFunc, "Waiting for other player to join!", myStyle);
        }
    }

    bool dusreKaBhiKardo = false;


    // Update is called once per frame
    void Update()
    {
        if (gm.randomMode)
        {
            text.text = PhotonNetwork.connectionStateDetailed.ToString();
        }
        else if (enough)
        {
            gmc.Status.gradientTop = Color.blue;
            gmc.Status.gradientBottom = Color.blue;
            gmc.Status.text = PhotonNetwork.connectionStateDetailed.ToString();
        }


        if (!gm.randomMode)
        {
            if (progress.progress >= 1f && !enough)
            {
                gmc = (GMCaller)FindObjectOfType(typeof(GMCaller));
                enough = true;
            }
        }
        if (!gameStarted && isRoomJoined)
        {
            if (PhotonNetwork.room.playerCount == 2)
            {
                Time.timeScale = 1;
                isRoomJoined = false;
                gameStarted = true;
                playerWaiting = false;
                mp_sm.StartFunction();
                SwapPlayerView();
            }
        }
        if (OpponentCharacter.allowedCharacterChange && justOneTimebaby)
        {
            Debug.Log("inside Update");
            justOneTimebaby = false;
            OpponentCharacter.allowedCharacterChange = false;

            characterActivation.DeActivateAll();
            characterActivation.ActivateOrDeactivateBoth(gm.getCharacterSelectedInt() + 1, gm.OppoCharacterID + 1, false);

            obps.oppoAnim = (CharacterActionsOpponent)FindObjectOfType(typeof(CharacterActionsOpponent));
            Debug.Log("Opponent character name : " + obps.oppoAnim.gameObject.name);


            /*  if (st.playerNumber == "Player1")
              {
                  characterActivation.DeActivateAll();
                  characterActivation.ActivateOrDeactivateBoth(gm.getCharacterSelectedInt() + 1, gm.OppoCharacterID + 1, false);
              }*/
            if (st.playerNumber == "Player2")
            {
                LeftCharPos = GameObject.Find("UserCharacter").transform.GetChild(gm.getCharacterSelectedInt() + 1).position;
                RightCharPos = GameObject.Find("OpponentCharacters").transform.GetChild(gm.OppoCharacterID + 1).position;

                LeftCharRot = GameObject.Find("UserCharacter").transform.GetChild(gm.getCharacterSelectedInt() + 1).rotation;
                RightCharRot = GameObject.Find("OpponentCharacters").transform.GetChild(gm.OppoCharacterID + 1).rotation;

                GameObject.Find("UserCharacter").transform.GetChild(gm.getCharacterSelectedInt() + 1).transform.position = RightCharPos;
                GameObject.Find("UserCharacter").transform.GetChild(gm.getCharacterSelectedInt() + 1).transform.Rotate(0f, 180f, 0f);
                GameObject.Find("OpponentCharacters").transform.GetChild(gm.OppoCharacterID + 1).transform.position = LeftCharPos;
                GameObject.Find("OpponentCharacters").transform.GetChild(gm.OppoCharacterID + 1).transform.Rotate(0f, 180f, 0f);
            }
        }
    }

    public void startMatchmaking()
    {
        PhotonNetwork.ConnectUsingSettings(VERSION);
        PhotonNetwork.JoinLobby(TypedLobby.Default);
    }

    void OnJoinedLobby()
    {
        
        if (gm.randomMode)
        {
            PhotonNetwork.JoinRandomRoom();
        }
        else {

            if (gm.createRoom)
            {
                RoomOptions roomOptions = new RoomOptions() { isVisible = false, maxPlayers = 2 };
                //            if (gmc.CheckRoomNames())
                //             {
                PhotonNetwork.CreateRoom(gm.RoomName, roomOptions, TypedLobby.Default);
                //              }
                //            else
                //                PhotonNetwork.Disconnect();
            }
            else if (gm.joinRoom)
            {
                //               if (gmc.checkIfRoomAvailable())
                //              {
                PhotonNetwork.JoinRoom(gm.RoomName);

                //              }
                //              else
                //                  PhotonNetwork.Disconnect();
            }
        }
    }

    void popUpJoinOrCreate()
    {
        progress = Application.LoadLevelAdditiveAsync("JoinAndCreate");
    }

    void OnPhotonRandomJoinFailed()
    {
        Debug.Log("Random Room joining failed");
        RoomOptions roomOptions = new RoomOptions() { isVisible = true, maxPlayers = 2 };
        PhotonNetwork.CreateRoom(null, roomOptions, TypedLobby.Default);

    }
    void OnPhotonCreateRoomFailed()
    {
        gmc.DisplayMessage("Failed to create room", Color.red);
        PhotonNetwork.Disconnect();
    }

    void OnPhotonJoinRoomFailed()
    {
        gmc.DisplayMessage("Cannot find Room", Color.red);
        PhotonNetwork.Disconnect();
    }

    void OnJoinedRoom()
    {
        if (gm.randomMode)
            status.SetActive(false);
        else
            gmc.quitPopUp();

        isRoomJoined = true;
        if (PhotonNetwork.room.playerCount == 1)
        {
            playerWaiting = true;
            st.playerNumber = "Player1";
            SwapPlayerView();
            Debug.Log("ROOM NAME: " + PhotonNetwork.room.name);
            //        Debug.Log ("<========== WAITING =========>");
        }
        else if (PhotonNetwork.room.playerCount == 2)
        {
            Time.timeScale = 1;
            st.playerNumber = "Player2";

            mp_sm.StartFunction();
            SwapPlayerView();
        }
    }
    void newFunc(int wid) { }

    void OnPhotonPlayerDisconnected(PhotonPlayer player)
    {
        MobileNativeMessage msg = new MobileNativeMessage("Disconnected", "Other Player left the game!", "OK");
        msg.OnComplete += OnMessageClose;
    }

    private void OnMessageClose()
    {
        PhotonNetwork.Disconnect();
        Time.timeScale = 1;
        gm.game_menu();
    }

    float getScreenHeight(int value)
    {
        return ((Screen.height / HEIGHT) * value);
    }
    float getScreenWidth(int value)
    {
        return ((Screen.width / WIDTH) * value);
    }

    void SwapPlayerView()
    {
        OpponentCharacter.Call_RPC_forCharacterID();


        if (st.playerNumber == "Player1")
        {
            mp_sm.BottomRight = Constants.BottomRightPlayerOne;
            RevokeInput.transform.position = Constants.inputBlockerGrid2;

        }
        else if (st.playerNumber == "Player2")
        {
            Debug.Log("Inside player 2 check in swap function");



            GameObject.Find("Score1").transform.position = RightScorePos;
            GameObject.Find("Score2").transform.position = LeftScorePos;

            GameObject.Find("NoMoreMovesG1").transform.position = RightShufflePos;
            GameObject.Find("NoMoreMovesG2").transform.position = LeftShufflePos;

            GameObject.Find("Player1").transform.position = RightPlayerPos;
            GameObject.Find("Player2").transform.position = LeftPlayerPos;

            //  GameObject.Find("Moves1").transform.GetChild(1).transform.position = RightMovesBox;
            //GameObject.Find("Moves2").transform.GetChild(1).transform.position = LeftMovesBox;

            //            GameObject.Find("Moves1").transform.GetChild(1).transform.position = RightMovesBox;
            //          GameObject.Find("Moves2").transform.GetChild(1).transform.position = LeftMovesBox;

            RevokeInput.transform.position = Constants.inputBlockerGrid1;
            mp_sm.BottomRight = Constants.BottomRightPlayerTwo;
        }

    }
}
