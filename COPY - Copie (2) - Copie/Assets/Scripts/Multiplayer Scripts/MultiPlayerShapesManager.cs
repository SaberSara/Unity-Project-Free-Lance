using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UI;
//using Soomla;
//using Soomla.Store;
//using Soomla.Store.CombatCrush;

public class MultiPlayerShapesManager : MonoBehaviour
{

    //public Text DebugText;
    public bool ShowDebugInfo = false;
    private UILabel NewScore1Text;

    private CharacterActions anim;
    private ShapesArray shapes;
    private GameManager gm;
    private StatManager st;
    private GameObject shuffleAlert;
    private int score, prevScore, prevScoreOppo = 0;
    public bool movesOver = false;
    private ObservePlayerScore obps;
    private ObserveMoves obms;

    //public readonly Vector2 BottomRight = new Vector2(-2.37f, -4.27f);
    public Vector2 BottomRight = new Vector2(-20.8f, -8.6f);
    public Vector2 CandySize = new Vector2(2.1f, 2.1f);

    private GameState state = GameState.None;
    private GameObject hitGo = null;
    private Vector2[] SpawnPositions;
    public List<GameObject> CandyPrefabs;
    public List<GameObject> ExplosionPrefabs;
    public GameObject BombExplosion;

	public GameObject StripedColorComb;
	public GameObject WrappedWholeWrapped;
	public GameObject StrippedWrappedRowAnim;
	public GameObject StrippedWrappedColAnim;
	public GameObject WrappedColorComb;

    public GameObject ColumnExplosion;
    public GameObject RowExplosion;
    public GameObject RowExplRight;
    public List<GameObject> BonusPrefabs;
    public List<GameObject> BombPrefabs;
    public GameObject ElectroPrefab;
    public GameObject gotPowerAnimPrefab;
    public GameObject brickPrefab;
    public GameObject stoneBlast;
    public GameObject electricCharge;
    public GameObject electroExplosion;
    public GameObject Right;
    public GameObject rowColExplosion;

    private ObserveTime ot;

    private IEnumerator CheckPotentialMatchesCoroutine;
    private IEnumerator AnimatePotentialMatchesCoroutine;

    string playerCheck;

    IEnumerable<GameObject> potentialMatches;
    private bool gameStarted = false;
    private SoundManager soundManager;

    private List<GameObject> powerUpsCopy;

    int[,] spawnPositionForPower;
    int[,] spawnPositionForBrick;

    int bricksCount = 0; //number of bricks to spawn on the level start
                         // int bricksQueued = 15; //test number of bricks to add instead of randowm candies
    private bricksCounter bc;

    public UILabel bombsCount;
    public UILabel boostersCount;
    public UILabel electroCount;

    bool electricMatch = false;


    bool lastWasBrick = false;

    private string oldTime = "180";

    bool justOneTimeBaby = false;

    bool BombSelected = false;
    bool BoosterSelected = false;
    bool ElectroSelected = false;
    bool HammerSelected = false;

    private MultiplayerNetworkConnection mp;

    private ObservePlayerName obpn;
    private ObservePlayerPicture obpp;
    private ObserverPlayerID obpid;

    FacebookConnection fbp;

    public GameObject player1Pic;
    public GameObject player2Pic;
    private bool opponentCanAddBrick=true;

    void Awake()
    {
        NewScore1Text = GameObject.Find("Score1").transform.GetChild(1).GetComponent<UILabel>();


    }

    void OnApplicationPause(bool pause)
    {

        if (pause)
        {
            if (PhotonNetwork.player == PhotonNetwork.masterClient)
            {

                PhotonNetwork.networkingPeer.SendOutgoingCommands();

                Debug.Log("===================================> Pause!");
                var playerList = PhotonNetwork.playerList;
                foreach (var i in playerList)
                {
                    if (!i.isMasterClient)
                    {
                        Debug.Log("Player Changes");
                        PhotonNetwork.SetMasterClient(i);
                        PhotonNetwork.networkingPeer.SendOutgoingCommands();

                    }
                }
            }
            PhotonNetwork.networkingPeer.SendOutgoingCommands();
        }
        else
        {
            Debug.Log("===================================> Resume!");
        }
    }

    // Use this for initialization
    public void StartFunction()
    {
        gameStarted = true;
        fbp = (FacebookConnection)FindObjectOfType(typeof(FacebookConnection));
        obpn = (ObservePlayerName)FindObjectOfType(typeof(ObservePlayerName));

        bc = (bricksCounter)FindObjectOfType(typeof(bricksCounter));

        obpn.Call_RPC_forPlayerName();

        /*   if (PhotonNetwork.player.isMasterClient)
           {
               // player1Pic = PhotonNetwork.Instantiate("Player1Pic", new Vector3(-9.5f, 1f, 0f), Quaternion.identity, 0);
           }
           else
           {
               player2Pic = PhotonNetwork.Instantiate("Player2Pic", new Vector3(-2.5f, 1f, 0f), Quaternion.identity, 0);
           }
           */

        powerUpsCopy = new List<GameObject>();
        gm = (GameManager)FindObjectOfType(typeof(GameManager));
        st = (StatManager)FindObjectOfType(typeof(StatManager));

        obpid = (ObserverPlayerID)FindObjectOfType(typeof(ObserverPlayerID));
        obpid.Call_RPC_forPlayerID();

        gm.isCongratsMenuOpen = false;
        gm.isLevelFailedMenuOpen = false;
        gm.setIsPauseMenuOpen(false);

        st.setTotalMoves(st.getTotalMoves());
        st.setTotalTimeInSeconds(st.getTotalTimeInSeconds());

        while (st.getCandiesAllowed() < CandyPrefabs.Count)
        {
            CandyPrefabs.Remove(CandyPrefabs.Last());
            BombPrefabs.Remove(BombPrefabs.Last());
            BonusPrefabs.Remove(BonusPrefabs.Last());
        }

        //Finding text labels
        bombsCount = GameObject.Find("Bomb").transform.GetChild(1).GetComponent<UILabel>();
        boostersCount = GameObject.Find("Booster").transform.GetChild(1).GetComponent<UILabel>();
        electroCount = GameObject.Find("Electro").transform.GetChild(1).GetComponent<UILabel>();

        //updatePowerUpCounters()
        UpdatePowerUpLabels();


        st.set_userScore(0);
        st.set_oppoScore(0);
        obps = (ObservePlayerScore)FindObjectOfType(typeof(ObservePlayerScore));

        if (PhotonNetwork.player.isMasterClient)
        {
            st.fbc.setUserProfilePicture(fbp.ShowLogedInUserProfilePicture(GameObject.Find("Player1Pic").GetComponent<SpriteRenderer>()));
        }
        else
        {
            st.fbc.setUserProfilePicture(fbp.ShowLogedInUserProfilePicture(GameObject.Find("Player2Pic").GetComponent<SpriteRenderer>()));
        }



        ot = (ObserveTime)FindObjectOfType(typeof(ObserveTime));

        shuffleAlert = GameObject.Find("NoMoreMovesG1");

        anim = (CharacterActions)FindObjectOfType(typeof(CharacterActions));
        //  obps.oppoAnim = (CharacterActionsOpponent)FindObjectOfType(typeof(CharacterActionsOpponent));
        //        obps.oppoAnim = GameObject.Find("Character1").GetComponent<CharacterActionsOpponent>();

        InitializeTypesOnPrefabShapesAndBonuses();
        InitializeCandyAndSpawnPositions(true);

        StartCheckForPotentialMatches();

        //   StartCoroutine(WaitForRPCToStop(5));
        mp = (MultiplayerNetworkConnection)FindObjectOfType(typeof(MultiplayerNetworkConnection));
        soundManager = (SoundManager)FindObjectOfType(typeof(SoundManager));
    }


    public void UpdatePowerUpLabels()
    {
//        bombsCount.text = StoreInventory.GetItemBalance(CombatCrushAssets.BOMB.ItemId).ToString();
//        boostersCount.text = StoreInventory.GetItemBalance(CombatCrushAssets.BOOSTER.ItemId).ToString();
//        electroCount.text = StoreInventory.GetItemBalance(CombatCrushAssets.ELECTRO.ItemId).ToString();
    }


    /// <summary>
    /// Replaces the random candy from grid with brick
    /// </summary>
    void ReplaceRandomCandyWithBrick()
    {
       
        for (int i = 0; i < bc.getUserBricks(); i++)
        {
            int row4Brick = Random.Range(1, Constants.Rows);
            int col4Brick = Random.Range(0, Constants.Columns);

            while (shapes[row4Brick, col4Brick].GetComponent<Shape>().Type == brickPrefab.name)
            {
                row4Brick = Random.Range(1, Constants.Rows - 1);
                col4Brick = Random.Range(0, Constants.Columns - 1);
            }

            GameObject go;
            go = brickPrefab;

            var newExplosion = Instantiate(stoneBlast, shapes[row4Brick, col4Brick].gameObject.transform.position, Quaternion.identity) as GameObject;
            Destroy(newExplosion, Constants.ExplosionDuration);

            var candyCache = shapes[row4Brick, col4Brick].gameObject;
            shapes.Remove(shapes[row4Brick, col4Brick].gameObject);
            RemoveFromScene(candyCache);

            InstantiateAndPlaceNewCandy(row4Brick, col4Brick, brickPrefab);
            bc.RemoveUsersBricks(1);
            bricksCount++;
        }
    }

    void setRandomPositionsForBricks()
    {
        spawnPositionForBrick = new int[bricksCount, 2];
        for (int i = 0; i < bricksCount; i++)
        {
            //random Row
            spawnPositionForBrick[i, 0] = Random.Range(1, Constants.Rows - 1);
            //random column
            spawnPositionForBrick[i, 1] = Random.Range(0, Constants.Columns - 1);

            //check for the coincidence if same spawn positions are generated as for previous powerups
            for (int j = 0; j < i; j++)
            {
                //if values are same
                if (spawnPositionForBrick[j, 0] == spawnPositionForBrick[i, 0]
                    && spawnPositionForBrick[j, 1] == spawnPositionForBrick[i, 1]
                    )
                {
                    //decrement i, so the value could be assigned again
                    i--;
                }

                for (int a = 0; a < powerUpsCopy.Count && i < powerUpsCopy.Count; a++)
                {
                    if (spawnPositionForBrick[a, 0] == spawnPositionForPower[i, 0]
                        && spawnPositionForBrick[a, 1] == spawnPositionForPower[i, 1])
                    {
                        //decrement the i, so the value could be assigned again
                        i--;
                    }
                }
            }
        }
    }


    void setRandomPositionsForPowerUps()
    {
        int temp = 2;
        //  powerupsCount = electroCount + BoosterCount + BombCount;
        //   Debug.Log("Power up count is : " + powerUpsCopy.Count);
        spawnPositionForPower = new int[powerUpsCopy.Count, temp];


        for (int i = 0; i < powerUpsCopy.Count; i++)
        {
            //random Row
            spawnPositionForPower[i, 0] = Random.Range(0, Constants.Rows - 1);

            //random column
            spawnPositionForPower[i, 1] = Random.Range(0, Constants.Columns - 1);

            //check for the coincidence if same spawn positions are generated as for previous powerups
            for (int j = 0; j < i; j++)
            {
                //if values are same
                if (spawnPositionForPower[j, 0] == spawnPositionForPower[i, 0]
                    && spawnPositionForPower[j, 1] == spawnPositionForPower[i, 1])
                {
                    //decrement the i, so the value could be assigned again
                    i--;
                }
            }
        }
    }

    /// <summary>
    /// Initialize shapes
    /// </summary>
    private void InitializeTypesOnPrefabShapesAndBonuses()
    {
        brickPrefab.GetComponent<Shape>().Type = brickPrefab.name;

        //just assign the name of the prefab
        foreach (var item in CandyPrefabs)
        {
            item.GetComponent<Shape>().Type = item.name;
        }

        //assign the name of the respective "normal" candy as the type of the Bonus
        foreach (var item in BonusPrefabs)
        {
            item.GetComponent<Shape>().Type = CandyPrefabs.
                Where(x => x.GetComponent<Shape>().Type.Contains(item.name.Split('_')[1].Trim())).Single().name;
        }

        //assign the name of the respective "normal" candy as the type of the Bomb Bonus
        foreach (var item in BombPrefabs)
        {
            item.GetComponent<Shape>().Type = CandyPrefabs.
                Where(x => x.GetComponent<Shape>().Type.Contains(item.name.Split('_')[1].Trim())).Single().name;
        }
        ElectroPrefab.GetComponent<Shape>().Type = "ElectroPower";
    }

    public void InitializeCandyAndSpawnPositionsFromPremadeLevel()
    {
        InitializeVariables(true);

        var premadeLevel = DebugUtilities.FillShapesArrayFromResourcesData();

        if (shapes != null)
            DestroyAllCandy();

        shapes = new ShapesArray();
        SpawnPositions = new Vector2[Constants.Columns];

        for (int row = 0; row < Constants.Rows; row++)
        {
            for (int column = 0; column < Constants.Columns; column++)
            {

                GameObject newCandy = null;
                newCandy = GetSpecificCandyOrBonusForPremadeLevel(premadeLevel[row, column]);
                InstantiateAndPlaceNewCandy(row, column, newCandy);
            }
        }

        SetupSpawnPositions();
    }


    public void InitializeCandyAndSpawnPositions(bool restart)
    {
        InitializeVariables(restart);

        if (shapes != null)
            DestroyAllCandy();

        shapes = new ShapesArray();
        SpawnPositions = new Vector2[Constants.Columns];

        setRandomPositionsForPowerUps();
        setRandomPositionsForBricks();

        for (int row = 0; row < Constants.Rows; row++)
        {
            for (int column = 0; column < Constants.Columns; column++)
            {

                GameObject newCandy = null;
                bool candyAssigned = false;

                for (int y = 0; y < powerUpsCopy.Count; y++)
                {
                    //if the randowm positions assigned before are the same as this row and column of loop
                    if (spawnPositionForPower[y, 0] == row
                        && spawnPositionForPower[y, 1] == column)
                    {
                        //get the last powerup from list and assign it to the newcandy
                        newCandy = powerUpsCopy.Last();
                        candyAssigned = true;
                        //remove it from the list and increment the powerup index
                        powerUpsCopy.Remove(powerUpsCopy.Last());
                    }
                }

                for (int x = 0; x < bricksCount; x++)
                {
                    if (spawnPositionForBrick[x, 0] == row
                        && spawnPositionForBrick[x, 1] == column)
                    {
                        //  Debug.Log("Matched");
                        newCandy = brickPrefab;
                        candyAssigned = true;
                    }
                }

                //if the powerup is not assigned, assign random
                if (!candyAssigned)
                    newCandy = GetRandomCandy();

                //    if (newCandy == null)
                //      Debug.Log("new candy is null, never assigned");

                //check if two previous horizontal are of the same type
                while (column >= 2 && shapes[row, column - 1].GetComponent<Shape>()
                    .IsSameType(newCandy.GetComponent<Shape>())
                    && shapes[row, column - 2].GetComponent<Shape>().IsSameType(newCandy.GetComponent<Shape>()))
                {
                    newCandy = GetRandomCandy();
                }

                //check if two previous vertical are of the same type
                while (row >= 2 && shapes[row - 1, column].GetComponent<Shape>()
                    .IsSameType(newCandy.GetComponent<Shape>())
                    && shapes[row - 2, column].GetComponent<Shape>().IsSameType(newCandy.GetComponent<Shape>()))
                {
                    newCandy = GetRandomCandy();
                }

                InstantiateAndPlaceNewCandy(row, column, newCandy);
            }
        }

        SetupSpawnPositions();
    }



    private void InstantiateAndPlaceNewCandy(int row, int column, GameObject newCandy)
    {
        GameObject go = PhotonNetwork.Instantiate(newCandy.name,
            BottomRight + new Vector2(column * CandySize.x, row * CandySize.y), Quaternion.identity, 0)
            as GameObject;

        //assign the specific properties
        go.GetComponent<Shape>().Assign(newCandy.GetComponent<Shape>().Type, row, column);
        shapes[row, column] = go;
    }

    private void SetupSpawnPositions()
    {
        //create the spawn positions for the new shapes (will pop from the 'ceiling')
        for (int column = 0; column < Constants.Columns; column++)
        {
            SpawnPositions[column] = BottomRight
                + new Vector2(column * CandySize.x, Constants.Rows * CandySize.y);
        }
    }




    /// <summary>
    /// Destroy all candy gameobjects
    /// </summary>
    private void DestroyAllCandy()
    {
        for (int row = 0; row < Constants.Rows; row++)
        {
            for (int column = 0; column < Constants.Columns; column++)
            {
                PhotonNetwork.Destroy(shapes[row, column].GetComponent<PhotonView>());
            }
        }
    }

    public void SelectPower(string power)
    {
        if (!gm.getIsPauseMenuOpen())
        {
            switch (power)
            {
                case "Booster":
				if (true)//StoreInventory.GetItemBalance(CombatCrushAssets.BOOSTER.ItemId) > 0)
                    {
                        state = GameState.SelectionStarted;
                        BoosterSelected = true;
                    }
//                    else if (StoreInventory.GetItemBalance(CombatCrushAssets.BOOSTER.ItemId) == 0)
//                        gm.shop_popup();
                    break;

                case "Bomb":
				if (true)//StoreInventory.GetItemBalance(CombatCrushAssets.BOMB.ItemId) > 0)
                    {
                        state = GameState.SelectionStarted;
                        BombSelected = true;
                    }
//                    else if (StoreInventory.GetItemBalance(CombatCrushAssets.BOMB.ItemId) == 0)
//                        gm.shop_popup();
                    break;

                case "Electro":
				if (true)//StoreInventory.GetItemBalance(CombatCrushAssets.ELECTRO.ItemId) > 0)
                    {
                        state = GameState.SelectionStarted;
                        ElectroSelected = true;
                    }
//                    else if (StoreInventory.GetItemBalance(CombatCrushAssets.ELECTRO.ItemId) == 0)
//                        gm.shop_popup();
                    break;

                case "Hammer":
				if (true)//StoreInventory.GetItemBalance(CombatCrushAssets.HAMMER.ItemId) > 0)
                    {
                        state = GameState.SelectionStarted;
                        HammerSelected = true;
                    }
//                    else if (StoreInventory.GetItemBalance(CombatCrushAssets.HAMMER.ItemId) == 0)
//                        gm.shop_popup();
                    break;

                default:
                    break;
            }
        }
    }
    /*
        void OnApplicationPause(bool pause)
        {
            if (!pause)
            { 
                if (PhotonNetwork.player == PhotonNetwork.masterClient)
                {
                    MobileNativeMessage Message = new MobileNativeMessage("Alert", "You have been disconnected from the game", "OK");
                    Message.OnComplete += OnMessageClose; 
                }
            }
        }
        */


    IEnumerator WaitForRPCToStop(int sec)
    {
        yield return new WaitForSeconds(sec);
        if (oldTime == GameObject.Find("Timer").transform.GetChild(1).GetComponent<UILabel>().text)
        {
            Time.timeScale = 0;

        }
        else
        {
            oldTime = GameObject.Find("Timer").transform.GetChild(1).GetComponent<UILabel>().text;
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (gameStarted)
        {
            if (opponentCanAddBrick)
                ReplaceRandomCandyWithBrick();

            if (state == GameState.None)
            {
                //user has clicked or touched
                if (Input.GetMouseButtonDown(0))
                {
                    //get the hit position
                    var hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
                    //			Debug.Log ("First hit: " + hit.collider.gameObject.name);
                    if (hit.collider != null && hit.collider.gameObject.name != "Blocker") //we have a hit!!!
                    {
                        hitGo = hit.collider.gameObject;

                        state = GameState.SelectionStarted;
                    }

                }
            }
            else if (state == GameState.SelectionStarted)
            {
                //user dragged
                if (Input.GetMouseButton(0) && !movesOver)
                {


                    var hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

                    //if user clicked on any of the icons to choose the bought power ups
                    if (BombSelected || BoosterSelected || ElectroSelected)
                    {
                        //and the candy he is trying to power up is a candy and not a blocker or null
                        if (hit.collider != null && hit.collider.gameObject.name != "Blocker")
                        {
                            var hitGoCache = new Shape();
                            var shape = hit.collider.gameObject.GetComponent<Shape>();
                            //cache it
                            hitGoCache.Assign(shape.Type, shape.Row, shape.Column);
                            shapes.Remove(hit.collider.gameObject);
                            RemoveFromScene(hit.collider.gameObject);
                            gotPowerAnimation(hit.collider.gameObject);

                            if (BombSelected)
                            {
                                soundManager.PlayBombCreationSound();
                                CreateBombBonus(hitGoCache);
                                //StoreInventory.TakeItem(CombatCrushAssets.BOMB.ItemId, 1);
                            }
                            else if (BoosterSelected)
                            {
                                soundManager.PlayBoosterCreationSound();
                                CreateBonus(hitGoCache);
                               // StoreInventory.TakeItem(CombatCrushAssets.BOOSTER.ItemId, 1);
                            }
                            else if (ElectroSelected)
                            {
                                soundManager.PlayElectroCreationSound();
								CreateElectroPower (hitGoCache);
                              //  StoreInventory.TakeItem(CombatCrushAssets.ELECTRO.ItemId, 1);
                            }
                            else if (HammerSelected || hit.collider.gameObject.GetComponent<Shape>().Type == brickPrefab.name)
                            {
                               // StoreInventory.TakeItem(CombatCrushAssets.HAMMER.ItemId, 1);
                                ReplaceBrickWithRandomCandy(hit.collider.gameObject);
                                bc.RemoveUsersBricks(1);
                            }
                           // StoreInventory.RefreshLocalInventory();

                            BombSelected = false;
                            BoosterSelected = false;
                            ElectroSelected = false;
                            UpdatePowerUpLabels();
                        }
                        state = GameState.None;
                    }

                    //we have a hit
                    if (hit.collider != null && hitGo != hit.collider.gameObject && hit.collider.gameObject.name != "Blocker")
                    {

                        //user did a hit, no need to show him hints 
                        StopCheckForPotentialMatches();

                        //if the two shapes are diagonally aligned (different row and column), just return
                        if (!Utilities.AreVerticalOrHorizontalNeighbors(hitGo.GetComponent<Shape>(),
                            hit.collider.gameObject.GetComponent<Shape>()))
                        {
                            state = GameState.None;
                        }
                        else
                        {
                            state = GameState.Animating;
                            FixSortingLayer(hitGo, hit.collider.gameObject);
                            StartCoroutine(FindMatchesAndCollapse(hit));
                        }
                    }
                }
            }

            /* if (Application.loadedLevelName == "MultiplayerTimeBased")
             {
                 //================ TIMER ===================================
                 if (st.getTimerCounter() >= 0)
                 {
                     st.setTimerCounter((float)System.Math.Round(((double)st.getTimerCounter() - Time.fixedDeltaTime), 2));
                     st.setTimerLabel(st.getTimerCounter().ToString("F0"));
                 }
                 else
                     movesOver = true;
             }
             */

            if (st.getTimerCounter() >= 0)
            {
                if (st.getTimerCounter() <= 12f && !justOneTimeBaby)
                {
                    soundManager.PlayClockSound();
                    justOneTimeBaby = true;
                }
                if (PhotonNetwork.player.isMasterClient)
                {
                    ot.Call_RPC_forTimer();
                }
            }

            else
            {
                movesOver = true;
                gm.GameOver();
            }

        }
    }


    /// <summary>
    /// Modifies sorting layers for better appearance when dragging/animating
    /// </summary>
    /// <param name="hitGo"></param>
    /// <param name="hitGo2"></param>
    private void FixSortingLayer(GameObject hitGo, GameObject hitGo2)
    {
        SpriteRenderer sp1 = hitGo.GetComponent<SpriteRenderer>();
        SpriteRenderer sp2 = hitGo2.GetComponent<SpriteRenderer>();
        if (sp1.sortingOrder <= sp2.sortingOrder)
        {
            sp1.sortingOrder = 1;
            sp2.sortingOrder = 0;
        }
    }


    private bool checkForElecroPower(IEnumerable<GameObject> totalMatches)
    {
        int sameCount = 0;
        foreach (var item in totalMatches)
        {
            for (int i = 0; i < totalMatches.Count(); i++)
            {
                if (item.GetComponent<Shape>().Row == totalMatches.ElementAt(i).GetComponent<Shape>().Row)
                    sameCount++;
                else
                    sameCount = 0;

                if (sameCount >= 5)
                    return true;
            }
            sameCount = 0;

            for (int j = 0; j < totalMatches.Count(); j++)
            {
                if (item.GetComponent<Shape>().Column == totalMatches.ElementAt(j).GetComponent<Shape>().Column)
                    sameCount++;
                else
                    sameCount = 0;

                if (sameCount >= 5)
                    return true;
            }
            sameCount = 0;
        }
        return false;
    }

    private bool checkIfSameColored(IEnumerable<GameObject> totalMatches)
    {
        var prevItem = totalMatches.ElementAt(0);
        int counter = 0;
        foreach (var item in totalMatches)
        {
            if (item.GetComponent<Shape>().Type == prevItem.GetComponent<Shape>().Type)
                counter++;
            else
                return false;

            if (counter >= 5)
                return true;
            prevItem = item;
        }
        return false;
    }

    int CheckFourCandiesSame(IEnumerable<GameObject> totalMatches)
    {
        int count = 0;
        foreach (var item in totalMatches)
        {
            count++;
            if (item.GetComponent<Shape>().Type == brickPrefab.name)
            {
                count--;
            }
        }
        return count;
    }

    private void ReduceMoves()
    {
        /* if (Application.loadedLevelName == "MultiplayerMoveBased")
         {
             //Warning Caustion Dangerous Area
             //UserMoves Calculat
             if (st.get_userMoves() > 0)
             {
                 st.set_userMoves(st.get_userMoves() - 1); // user moves counter set
                 st.setUserMovesLabel(st.get_userMoves().ToString());
                 obms.Call_RPC_forMoves();
             }
             if (st.get_userMoves() <= 0)
             {
                 st.setUserMovesLabel(st.get_userMoves().ToString());
                 movesOver = true;
                if (st.get_userMoves() <= 0)
                {
                    gm.GameOver();
                }
             }
         }*/
    }

    private void gotPowerAnimation(GameObject go)
    {
        var newExplosion = Instantiate(gotPowerAnimPrefab, go.transform.position, Quaternion.identity) as GameObject;
        Destroy(newExplosion, Constants.ExplosionDuration);
    }
    private IEnumerator FindMatchesAndCollapse(RaycastHit2D hit2)
    {
        //get the second item that was part of the swipe
        var hitGo2 = hit2.collider.gameObject;
        Shape hitGoCache = null;
        Shape hitGoCache1 = null;


        bool addBomb = false;
        bool addElectro = false;
        bool addBonus = false;

        bool addBomb1 = false;
        bool addElectro1 = false;
        bool addBonus1 = false;

        bool bricks = false;
        bool bricks1 = false;

        MatchesInfo hitGomatchesInfo, hitGo2matchesInfo;

        opponentCanAddBrick = false;
        IEnumerable<GameObject> totalMatches;

        shapes.Swap(hitGo, hitGo2);

        //move the swapped ones
        hitGo.transform.positionTo(Constants.AnimationDuration, hitGo2.transform.position);
        hitGo2.transform.positionTo(Constants.AnimationDuration, hitGo.transform.position);
        yield return new WaitForSeconds(Constants.AnimationDuration);


        #region matching boosters

        if (hitGo.GetComponent<Shape>().Bonus == BonusType.DestroyWholeRowColumn
           && hitGo2.GetComponent<Shape>().Bonus == BonusType.DestroyWholeRowColumn)
        {
            totalMatches = shapes.GetEntireColumn(hitGo2).Union(shapes.GetEntireRow(hitGo2)).Distinct();
            var rowExplosion = Instantiate(RowExplosion, new Vector2(RowExplosion.transform.position.x, hitGo.transform.position.y), Quaternion.identity) as GameObject;
            var colExplosion = Instantiate(ColumnExplosion, new Vector2(hitGo.transform.position.x, RowExplosion.transform.position.y), Quaternion.identity) as GameObject;
            Destroy(rowExplosion, Constants.AnimationDuration);
            Destroy(colExplosion, Constants.AnimationDuration);

        }
        #endregion

        #region Matching bombs
        else if (hitGo.GetComponent<Shape>().Bonus == BonusType.DestroyNineCandies
           && hitGo2.GetComponent<Shape>().Bonus == BonusType.DestroyNineCandies)
        {
            totalMatches = shapes.GetNineCandiesAroundBomb(hitGo2).Union(shapes.GetNineCandiesAroundBomb(hitGo)).Distinct();
            var rowExplosion = Instantiate(BombExplosion, hitGo.transform.position, Quaternion.identity) as GameObject;
            var colExplosion = Instantiate(BombExplosion, hitGo2.transform.position, Quaternion.identity) as GameObject;
            Destroy(rowExplosion, Constants.AnimationDuration);
            Destroy(colExplosion, Constants.AnimationDuration);

        }
        #endregion

        #region Matching electros

        else if (hitGo.GetComponent<Shape>().Bonus == BonusType.DestroySameCandies
           && hitGo2.GetComponent<Shape>().Bonus == BonusType.DestroySameCandies)
        {
			totalMatches = shapes.GetAllCandies();
			var charge = Instantiate(WrappedWholeWrapped, hitGo.transform.position, Quaternion.identity) as GameObject;
			Destroy(charge, Constants.ElectricChargeDuration);

        }
        #endregion

        #region Matching boosters with Bomb

        else if (hitGo.GetComponent<Shape>().Bonus == BonusType.DestroyWholeRowColumn
           && hitGo2.GetComponent<Shape>().Bonus == BonusType.DestroyNineCandies)
        {
			totalMatches = shapes.GetEntireColumn(hitGo).Union(shapes.GetNineCandiesAroundBomb(hitGo2)).Distinct();
			var bombExplosion = Instantiate(StripedColorComb, hitGo.transform.position, Quaternion.identity) as GameObject;
			var colExplosion = Instantiate(ColumnExplosion, new Vector2(hitGo.transform.position.x, RowExplosion.transform.position.y), Quaternion.identity) as GameObject;
			Destroy(bombExplosion, 1f);
			Destroy(colExplosion, 1f);
			//bc.AddToOpponent(2);
			
//            totalMatches = shapes.GetEntireColumn(hitGo).Union(shapes.GetNineCandiesAroundBomb(hitGo2)).Distinct();
//            var bombExplosion = Instantiate(BombExplosion, BombExplosion.transform.position, Quaternion.identity) as GameObject;
//            var colExplosion = Instantiate(ColumnExplosion, new Vector2(hitGo.transform.position.x, RowExplosion.transform.position.y), Quaternion.identity) as GameObject;
//            Destroy(bombExplosion, Constants.AnimationDuration);
//            Destroy(colExplosion, Constants.AnimationDuration);
        }
        else if (hitGo.GetComponent<Shape>().Bonus == BonusType.DestroyNineCandies
           && hitGo2.GetComponent<Shape>().Bonus == BonusType.DestroyWholeRowColumn)
        {
			totalMatches = shapes.GetNineCandiesAroundBomb(hitGo).Union(shapes.GetEntireRow(hitGo2)).Distinct();
			var bombExplosion = Instantiate(StripedColorComb, hit2.transform.position, Quaternion.identity) as GameObject;
			var rowExplosion = Instantiate(ColumnExplosion, new Vector2(hitGo2.transform.position.x, RowExplosion.transform.position.y), Quaternion.identity) as GameObject;
			Destroy(bombExplosion, 1f);
			Destroy(rowExplosion, 1f);
			bc.AddToOpponent(2);

//            totalMatches = shapes.GetNineCandiesAroundBomb(hitGo).Union(shapes.GetEntireRow(hitGo2)).Distinct();
//            var bombExplosion = Instantiate(BombExplosion, BombExplosion.transform.position, Quaternion.identity) as GameObject;
//            var rowExplosion = Instantiate(ColumnExplosion, new Vector2(hitGo2.transform.position.x, RowExplosion.transform.position.y), Quaternion.identity) as GameObject;
//            Destroy(bombExplosion, Constants.AnimationDuration);
//            Destroy(rowExplosion, Constants.AnimationDuration);
        }

        #endregion

        #region Matching Boosters with electro
        else if (hitGo.GetComponent<Shape>().Bonus == BonusType.DestroyWholeRowColumn
          && hitGo2.GetComponent<Shape>().Bonus == BonusType.DestroySameCandies)
        {
			totalMatches = shapes.GetThreeEntireRow(hitGo2).Union(shapes.GetThreeEntireColumn(hitGo)).Union(shapes.FindMatchingCandiesInGrid(hitGo, hitGo2).MatchedCandy).Distinct();
			var electrExplosion = Instantiate(electricCharge, electricCharge.transform.position, Quaternion.identity) as GameObject;
			var colExplosion = Instantiate(StrippedWrappedColAnim, new Vector2(hitGo.transform.position.x, RowExplosion.transform.position.y), Quaternion.identity) as GameObject;
			Destroy(electrExplosion, Constants.ElectricChargeDuration);
			Destroy(colExplosion, 1f);
        }
        else if (hitGo.GetComponent<Shape>().Bonus == BonusType.DestroySameCandies
           && hitGo2.GetComponent<Shape>().Bonus == BonusType.DestroyWholeRowColumn)
        {
			totalMatches = (shapes.FindMatchingCandiesInGrid(hitGo2, hitGo).MatchedCandy).Union(shapes.GetThreeEntireColumn(hitGo2)).Union(shapes.GetThreeEntireRow(hitGo2)).Distinct();
			var electrExplosion = Instantiate(electricCharge, electricCharge.transform.position, Quaternion.identity) as GameObject;
			var rowExplosion = Instantiate(StrippedWrappedRowAnim, new Vector2(hitGo2.transform.position.x, RowExplosion.transform.position.y), Quaternion.identity) as GameObject;
			Destroy(electrExplosion, Constants.ElectricChargeDuration);
			Destroy(rowExplosion, 1f);
        }
        #endregion

        #region Matching Bombs with electro
        else if (hitGo.GetComponent<Shape>().Bonus == BonusType.DestroyNineCandies
          && hitGo2.GetComponent<Shape>().Bonus == BonusType.DestroySameCandies)
        {
			totalMatches = shapes.GetNineCandiesAroundBomb(hitGo).Union(shapes.FindMatchingCandiesInGrid(hitGo, hitGo2).MatchedCandy).Distinct();
			var electrExplosion = Instantiate(electricCharge, electricCharge.transform.position, Quaternion.identity) as GameObject;
			var bombExplosion = Instantiate(WrappedColorComb , hitGo.transform.position, Quaternion.identity) as GameObject;
			Destroy(electrExplosion, Constants.ElectricChargeDuration);
			Destroy(bombExplosion, 1f);
        }
        else if (hitGo.GetComponent<Shape>().Bonus == BonusType.DestroySameCandies
           && hitGo2.GetComponent<Shape>().Bonus == BonusType.DestroyNineCandies)
        {
			totalMatches = (shapes.FindMatchingCandiesInGrid(hitGo2, hitGo).MatchedCandy).Union(shapes.GetNineCandiesAroundBomb(hitGo2)).Distinct();
			var electrExplosion = Instantiate(electricCharge, electricCharge.transform.position, Quaternion.identity) as GameObject;
			var bombExplosion = Instantiate(WrappedColorComb , hitGo.transform.position, Quaternion.identity) as GameObject;
			Destroy(electrExplosion, Constants.ElectricChargeDuration);
			Destroy(bombExplosion, 1f);
        }
        #endregion

        else
        {
            #region Bricks Check
            //if the object clicked to swap is a brick
            if (hitGo.GetComponent<Shape>().Type == brickPrefab.name)
            {
                bricks = true;
            }


            //if the object bieng swapped with the clicked one is a brick
            if (hitGo2.GetComponent<Shape>().Type == brickPrefab.name)
            {
                bricks1 = true;
            }
            #endregion


            if (hitGo.GetComponent<Shape>().Type == ElectroPrefab.GetComponent<Shape>().Type)
            {
                soundManager.PlayElectro();
                totalMatches = shapes.FindMatchingCandiesInGrid(hitGo2, hitGo).MatchedCandy;
                electricMatch = true;

                if (st.playerNumber == "Player1")
                {
                    var newExplosion = Instantiate(electricCharge, electricCharge.transform.position, Quaternion.identity) as GameObject;
                    Destroy(newExplosion, Constants.ElectricChargeDuration);

                }
                else if (st.playerNumber == "Player2")
                {
                    var newExplosion = Instantiate(Right, Right.transform.position, Quaternion.identity) as GameObject;
                    Destroy(newExplosion, Constants.ElectricChargeDuration);
                }

            }
            else if (hitGo2.GetComponent<Shape>().Type == ElectroPrefab.GetComponent<Shape>().Type)
            {
                totalMatches = shapes.FindMatchingCandiesInGrid(hitGo, hitGo2).MatchedCandy;
                electricMatch = true;

                var newExplosion = Instantiate(electricCharge, electricCharge.transform.position, Quaternion.identity) as GameObject;
                Destroy(newExplosion, Constants.ElectricChargeDuration);
            }

            else
            {
                //get the matches via the helper methods
                if (!bricks)
                {
                    hitGomatchesInfo = shapes.GetMatches(hitGo);
                }
                else
                    hitGomatchesInfo = new MatchesInfo();
                if (!bricks1)
                {
                    hitGo2matchesInfo = shapes.GetMatches(hitGo2);
                }
                else
                    hitGo2matchesInfo = new MatchesInfo();

                totalMatches = hitGomatchesInfo.MatchedCandy
                    .Union(hitGo2matchesInfo.MatchedCandy).Distinct();

                //if user's swap didn't create at least a 3-match, undo their swap
                if (totalMatches.Count() < Constants.MinimumMatches)
                {
                    hitGo.transform.positionTo(Constants.AnimationDuration, hitGo2.transform.position);
                    hitGo2.transform.positionTo(Constants.AnimationDuration, hitGo.transform.position);
                    yield return new WaitForSeconds(Constants.AnimationDuration);

                    shapes.UndoSwap();
                }

                else
                {

                    bool sameFive = false;
                    bool sameColor = false;

                    bool sameFive1 = false;
                    bool sameColor1 = false;

                    int sameFour = 0;
                    int sameFour1 = 0;


                    if (hitGomatchesInfo.MatchedCandy.Count() >= 4 && !bricks)
                    {
                        sameFour = CheckFourCandiesSame(hitGomatchesInfo.MatchedCandy);
                    }

                    if (hitGo2matchesInfo.MatchedCandy.Count() >= 4 && !bricks1)
                    {
                        sameFour1 = CheckFourCandiesSame(hitGo2matchesInfo.MatchedCandy);
                    }


                    if (hitGomatchesInfo.MatchedCandy.Count() > 4 && !bricks)
                    {
                        sameFive = checkForElecroPower(hitGomatchesInfo.MatchedCandy);
                        sameColor = checkIfSameColored(hitGomatchesInfo.MatchedCandy);

                    }

                    if (hitGo2matchesInfo.MatchedCandy.Count() > 4 && !bricks1)
                    {
                        sameFive1 = checkForElecroPower(hitGo2matchesInfo.MatchedCandy);
                        sameColor1 = checkIfSameColored(hitGo2matchesInfo.MatchedCandy);
                    }

                    if (!bricks)
                    {
                        //if more than 3 matches and no Bonus is contained in the line, we will award a new Bonus
                        addBonus = sameFour == Constants.MinimumMatchesForBonus  &&
                            !BonusTypeUtilities.ContainsDestroyWholeRowColumn(hitGomatchesInfo.BonusesContained);

                        addElectro = hitGomatchesInfo.MatchedCandy.Count() >= Constants.MatchCountForPower && sameFive && sameColor &&
                            !BonusTypeUtilities.ContainsDestroyWholeRowColumn(hitGomatchesInfo.BonusesContained);


                        addBomb = hitGomatchesInfo.MatchedCandy.Count() >= Constants.MatchCountForPower && !sameFive && sameColor &&
                                         !BonusTypeUtilities.ContainsDestroyWholeRowColumn(hitGomatchesInfo.BonusesContained);


                        if (addBonus || addElectro || addBomb)
                        {
                            hitGoCache = new Shape();
                            //get the game object that was of the same type
                            //      var sameTypeGo = hitGomatchesInfo.MatchedCandy.Count() > 0 ? hitGo : hitGo2;
                            var sameTypeGo = hitGo;
                            var shape = sameTypeGo.GetComponent<Shape>();
                            //cache it
                            hitGoCache.Assign(shape.Type, shape.Row, shape.Column);
                            gotPowerAnimation(sameTypeGo);
                        }
                    }

                    if (!bricks1)
                    {

                        //if more than 3 matches and no Bonus is contained in the line, we will award a new Bonus
                        addBonus1 = sameFour1 == Constants.MinimumMatchesForBonus  &&
                            !BonusTypeUtilities.ContainsDestroyWholeRowColumn(hitGo2matchesInfo.BonusesContained);

                        addElectro1 = hitGo2matchesInfo.MatchedCandy.Count() >= Constants.MatchCountForPower && sameFive1 && sameColor1 &&
                            !BonusTypeUtilities.ContainsDestroyWholeRowColumn(hitGo2matchesInfo.BonusesContained);


                        addBomb1 = hitGo2matchesInfo.MatchedCandy.Count() >= Constants.MatchCountForPower && !sameFive1 && sameColor1 &&
                                         !BonusTypeUtilities.ContainsDestroyWholeRowColumn(hitGo2matchesInfo.BonusesContained);


                        if (addBonus1 || addElectro1 || addBomb1)
                        {
                            hitGoCache1 = new Shape();
                            //get the game object that was of the same type
                            //      var sameTypeGo = hitGomatchesInfo.MatchedCandy.Count() > 0 ? hitGo : hitGo2;
                            var sameTypeGo = hitGo2;
                            var shape = sameTypeGo.GetComponent<Shape>();
                            //cache it
                            hitGoCache1.Assign(shape.Type, shape.Row, shape.Column);
                            gotPowerAnimation(sameTypeGo);
                        }
                    }
                }
            }
        }
        int timesRun = 1;
        while (totalMatches.Count() >= Constants.MinimumMatches || electricMatch)
        {
            electricMatch = false;
            if (timesRun == 1)
                prevScore = st.get_userScore();


            if (!(bricks || bricks1))
            {
                //increase score
                IncreaseScore(totalMatches.Count() * Constants.candyScore * timesRun);
            }

            bricks = false;
            bricks1 = false;

            soundManager.PlayCrincle();

            //remove the candies that were matched
            foreach (var item in totalMatches)
            {
                //if the candies are bricks replace them with random candies
                if (item.GetComponent<Shape>().Type == brickPrefab.name)
                {
                    ReplaceBrickWithRandomCandy(item);
                    bricksCount--;
                }
                //just remove
                else
                {
                    shapes.Remove(item);
                    RemoveFromScene(item);
                }
            }

            if (!electricMatch)
            {
                //check and instantiate Bonus if needed
                if (addBonus)
                {
                    soundManager.PlayBoosterCreationSound();
                    IncreaseScore(Constants.BoosterScore);
                    CreateBonus(hitGoCache);
                }
                if (addElectro)
                {
                    soundManager.PlayElectroCreationSound();
                    IncreaseScore(Constants.ElectroScore);
                    CreateElectroPower(hitGoCache);
                }

                if (addBomb)
                {
                    soundManager.PlayBombCreationSound();
                    IncreaseScore(Constants.bombScore);
                    CreateBombBonus(hitGoCache);
                }


                if (addBonus1)
                {
                    soundManager.PlayBoosterCreationSound();
                    IncreaseScore(Constants.BoosterScore);
                    CreateBonus(hitGoCache1);
                }
                if (addElectro1)
                {
                    soundManager.PlayElectroCreationSound();
                    IncreaseScore(Constants.ElectroScore);
                    CreateElectroPower(hitGoCache1);
                }

                if (addBomb1)
                {
                    soundManager.PlayBombCreationSound();
                    IncreaseScore(Constants.bombScore);
                    CreateBombBonus(hitGoCache1);
                }


                addBonus = false;
                addElectro = false;
                addBomb = false;

                addBonus1 = false;
                addElectro1 = false;
                addBomb1 = false;
            }
            //get the columns that we had a collapse
            var columns = totalMatches.Select(go => go.GetComponent<Shape>().Column).Distinct();

            //the order the 2 methods below get called is important!!!
            //collapse the ones gone
            var collapsedCandyInfo = shapes.Collapse(columns);
            //create new ones
            var newCandyInfo = CreateNewCandyInSpecificColumns(columns);

            int maxDistance = Mathf.Max(collapsedCandyInfo.MaxDistance, newCandyInfo.MaxDistance);

            MoveAndAnimate(newCandyInfo.AlteredCandy, maxDistance);
            MoveAndAnimate(collapsedCandyInfo.AlteredCandy, maxDistance);


            var alteredCandies = collapsedCandyInfo.AlteredCandy.Union(newCandyInfo.AlteredCandy).Distinct();

            //will wait for both of the above animations
            yield return new WaitForSeconds(Constants.MoveAnimationMinDuration * maxDistance);

            #region Handling if brick got to the last row
            bool brickInLastRow = false;

            do
            {
                brickInLastRow = false;
                int a = 0;
                var lastRow = shapes.GetLastRow();
                for (a = 0; a < lastRow.Count; a++)
                {
                    if (lastRow.ElementAt(a).GetComponent<Shape>().Type == brickPrefab.name)
                    {
                        //  Debug.Log("Altered candeis count before : " + alteredCandies.Count());
                        //   Debug.Log("han g");
                        // lastRow.Remove(i);
                        shapes.Remove(lastRow.ElementAt(a));
                        Destroy(lastRow.ElementAt(a));
                        bricksCount--;
                        // alteredCandies.ToList().Remove(lastRow.ElementAt(a));
                        //    Debug.Log("Altered candeis count after : " + alteredCandies.Count());

                        brickInLastRow = true;

                        var collapsedCandy = shapes.CollapseForBrick(a);
                        var newCandy = CreateNewCandyInSpecificColumnsAfterBrick(a);

                        int maxDist = Mathf.Max(collapsedCandy.MaxDistance, newCandy.MaxDistance);

                        MoveAndAnimate(newCandy.AlteredCandy, maxDist);
                        MoveAndAnimate(collapsedCandy.AlteredCandy, maxDist);

                        yield return new WaitForSeconds(Constants.MoveAnimationMinDuration * maxDist);

                        alteredCandies = alteredCandies.Union(collapsedCandy.AlteredCandy.Union(newCandy.AlteredCandy).Distinct()).Distinct().Where(u => u != null);
                    }
                }

            } while (brickInLastRow);
            #endregion

            //search if there are matches with the new/collapsed items
            //    Debug.Log( alteredCandies.Count());
            totalMatches = shapes.GetMatches(alteredCandies);

            electricMatch = false;
            timesRun++;
        }
        opponentCanAddBrick = true;

        obps.Call_RPC_forScore();
        // ReduceMoves();
        if ((st.get_userScore() - prevScore) > Constants.minAttackScoreLevel2 && (st.get_userScore() - prevScore) < Constants.minSuperAttackScoreLevel2)
        {
            bc.AddToOpponent(1);
            bc.Call_RPC_forBricks();
            anim.Attack();
            StartCoroutine(waitFunction(Constants.AnimationDuration));
        }
        else if ((st.get_userScore() - prevScore) >= Constants.minSuperAttackScoreLevel2)
        {
            bc.AddToOpponent(2);
            bc.Call_RPC_forBricks();
            anim.SuperAttack();
            StartCoroutine(waitFunction(Constants.AnimationDuration));
        }

        prevScore = st.get_userScore();
        prevScoreOppo = st.get_oppoScore();
        state = GameState.None;

        if (!movesOver)
        {
            StartCheckForPotentialMatches();
        }
    }

    private IEnumerator waitFunction(float x)
    {
        yield return new WaitForSeconds(x);
        anim.Idle();
    }

    /// <summary>
    /// Creates a new Bonus based on the shape parameter
    /// </summary>
    /// <param name="hitGoCache"></param>
    private void CreateBonus(Shape hitGoCache)
    {
        GameObject Bonus = PhotonNetwork.Instantiate(GetBonusFromType(hitGoCache.Type).name, BottomRight
            + new Vector2(hitGoCache.Column * CandySize.x,
                hitGoCache.Row * CandySize.y), Quaternion.identity, 0)
            as GameObject;
        shapes[hitGoCache.Row, hitGoCache.Column] = Bonus;
        var BonusShape = Bonus.GetComponent<Shape>();
        //will have the same type as the "normal" candy
        BonusShape.Assign(hitGoCache.Type, hitGoCache.Row, hitGoCache.Column);
        //add the proper Bonus type
        BonusShape.Bonus |= BonusType.DestroyWholeRowColumn;
    }
    private void CreateBombBonus(Shape hitGoCache)
    {
        GameObject Bonus = PhotonNetwork.Instantiate(GetBombBonusFromType(hitGoCache.Type).name, BottomRight
            + new Vector2(hitGoCache.Column * CandySize.x,
                hitGoCache.Row * CandySize.y), Quaternion.identity, 0)
            as GameObject;
        shapes[hitGoCache.Row, hitGoCache.Column] = Bonus;
        var BonusShape = Bonus.GetComponent<Shape>();
        //will have the same type as the "normal" candy
        BonusShape.Assign(hitGoCache.Type, hitGoCache.Row, hitGoCache.Column);
        //add the proper Bonus type
        BonusShape.Bonus |= BonusType.DestroyNineCandies;
    }
    private void CreateElectroPower(Shape hitGoCache)
    {
        powerUpsCopy.Add(ElectroPrefab);
        //    Debug.Log("electro power added ");
        GameObject Bonus = PhotonNetwork.Instantiate(ElectroPrefab.gameObject.name, BottomRight + new Vector2(hitGoCache.Column * CandySize.x,
            hitGoCache.Row * CandySize.y), Quaternion.identity, 0)
            as GameObject;

        shapes[hitGoCache.Row, hitGoCache.Column] = Bonus;
        var BonusShape = Bonus.GetComponent<Shape>();
        //will have the same type as the "normal" candy
        BonusShape.Assign(ElectroPrefab.GetComponent<Shape>().Type, hitGoCache.Row, hitGoCache.Column);
        //add the proper Bonus types
        BonusShape.Bonus |= BonusType.DestroySameCandies;
    }



    /// <summary>
    /// Spawns new candy in columns that have missing ones
    /// </summary>
    /// <param name="columnsWithMissingCandy"></param>
    /// <returns>Info about new candies created</returns>
    private AlteredCandyInfo CreateNewCandyInSpecificColumns(IEnumerable<int> columnsWithMissingCandy)
    {
        AlteredCandyInfo newCandyInfo = new AlteredCandyInfo();

        //find how many null values the column has
        foreach (int column in columnsWithMissingCandy)
        {
            var emptyItems = shapes.GetEmptyItemsOnColumn(column);
            foreach (var item in emptyItems)
            {
                var go = GetRandomCandy();
                GameObject newCandy = PhotonNetwork.Instantiate(go.name, SpawnPositions[column], Quaternion.identity, 0)
                    as GameObject;

                newCandy.GetComponent<Shape>().Assign(go.GetComponent<Shape>().Type, item.Row, item.Column);

                if (Constants.Rows - item.Row > newCandyInfo.MaxDistance)
                    newCandyInfo.MaxDistance = Constants.Rows - item.Row;

                shapes[item.Row, item.Column] = newCandy;
                newCandyInfo.AddCandy(newCandy);
            }
        }
        return newCandyInfo;
    }



    /// <summary>
    /// Spawns new candy in columns that have missing ones
    /// </summary>
    /// <param name="columnsWithMissingCandy"></param>
    /// <returns>Info about new candies created</returns>
    private AlteredCandyInfo CreateNewCandyInSpecificColumnsAfterBrick(int column)
    {
        AlteredCandyInfo newCandyInfo = new AlteredCandyInfo();

        var emptyItems = shapes.GetEmptyItemsOnColumn(column);
        foreach (var item in emptyItems)
        {
            
               var go = GetRandomCandy();
         

            GameObject newCandy = PhotonNetwork.Instantiate(go.name, SpawnPositions[column], Quaternion.identity,0)
                as GameObject;

            newCandy.GetComponent<Shape>().Assign(go.GetComponent<Shape>().Type, item.Row, item.Column);

            if (Constants.Rows - item.Row > newCandyInfo.MaxDistance)
                newCandyInfo.MaxDistance = Constants.Rows - item.Row;

            shapes[item.Row, item.Column] = newCandy;
            newCandyInfo.AddCandy(newCandy);
        }

        return newCandyInfo;
    }

    /// <summary>
    /// Animates gameobjects to their new position
    /// </summary>
    /// <param name="movedGameObjects"></param>
    private void MoveAndAnimate(IEnumerable<GameObject> movedGameObjects, int distance)
    {
        foreach (var item in movedGameObjects)
        {
            item.transform.positionTo(Constants.MoveAnimationMinDuration * distance, BottomRight +
                new Vector2(item.GetComponent<Shape>().Column * CandySize.x, item.GetComponent<Shape>().Row * CandySize.y));
        }
    }

    void RemoveFromThePowerCopy(GameObject item)
    {
        //if there are even any powerups
        if (powerUpsCopy.Count >= 1)
        {
            //if the item type is bonus, otherwise no need to iterate the list
            if (item.GetComponent<Shape>().Bonus != BonusType.None)
            {
                foreach (var powerupItem in powerUpsCopy)
                {
                    //        Debug.Log("Power up items name to compare" + powerupItem + " : " + item);
                    if (powerupItem.GetComponent<Shape>().Type == item.GetComponent<Shape>().Type)
                    {
                        //             Debug.Log("Power up removed from the copy");
                        powerUpsCopy.Remove(powerupItem);
                        break;
                    }
                }
            }
        }
    }

    void ReplaceBrickWithRandomCandy(GameObject item)
    {

        if (item.GetComponent<Shape>().Type == brickPrefab.name)
        {
            var column = item.GetComponent<Shape>().Column;
            var row = item.GetComponent<Shape>().Row;


            /*  var hitGoCache = new Shape();
                var shape = item.GetComponent<Shape>();
                //cache it
                hitGoCache.Assign(shape.Type, shape.Row, shape.Column);*/
            shapes.Remove(item);
            RemoveFromScene(item);


            var newCandy = GetRandomCandy();


            #region Making sure the random candy is not matching any other candies already arount it

            // * * &

            //check if the two previous ones are same in column
            if ((column >= 2 && shapes[row, column - 1] != null && shapes[row, column - 2] != null))
            {

                //check if two previous horizontal are of the same type
                while (shapes[row, column - 1].GetComponent<Shape>()
                    .IsSameType(newCandy.GetComponent<Shape>())
                    && shapes[row, column - 2].GetComponent<Shape>().IsSameType(newCandy.GetComponent<Shape>()))
                {
                    newCandy = GetRandomCandy();
                }
            }

            // & * *
            //check if the two forward ones are same in column
            if ((column < Constants.Columns - 2 && shapes[row, column + 1] != null && shapes[row, column + 2] != null))
            {

                //check if two previous horizontal are of the same type
                while (shapes[row, column + 1].GetComponent<Shape>()
                    .IsSameType(newCandy.GetComponent<Shape>())
                    && shapes[row, column + 2].GetComponent<Shape>().IsSameType(newCandy.GetComponent<Shape>()))
                {
                    newCandy = GetRandomCandy();
                }
            }

            //  &
            //  *
            //  *
            //check if the two in previous row are same in column
            if (row >= 2 && shapes[row - 1, column] != null && shapes[row - 2, column] != null)
            {
                //check if two previous vertical are of the same type
                while (shapes[row - 1, column].GetComponent<Shape>()
                    .IsSameType(newCandy.GetComponent<Shape>())
                    && shapes[row - 2, column].GetComponent<Shape>().IsSameType(newCandy.GetComponent<Shape>()))
                {
                    newCandy = GetRandomCandy();
                }
            }

            //  *
            //  *
            //  &

            //check if the two in forward row are same in column
            if (row < Constants.Rows - 2 && shapes[row + 1, column] != null && shapes[row + 2, column] != null)
            {
                //check if two previous vertical are of the same type
                while (shapes[row + 1, column].GetComponent<Shape>()
                    .IsSameType(newCandy.GetComponent<Shape>())
                    && shapes[row + 2, column].GetComponent<Shape>().IsSameType(newCandy.GetComponent<Shape>()))
                {
                    newCandy = GetRandomCandy();
                }
            }
            // *
            // &
            // *
            //check if the forward and backward candy in row is same
            if (row >= 1 && row < Constants.Rows - 1 && shapes[row + 1, column] != null && shapes[row - 1, column] != null)
            {
                //check if two previous vertical are of the same type
                while (shapes[row + 1, column].GetComponent<Shape>()
                    .IsSameType(newCandy.GetComponent<Shape>())
                    && shapes[row - 1, column].GetComponent<Shape>().IsSameType(newCandy.GetComponent<Shape>()))
                {
                    newCandy = GetRandomCandy();
                }
            }

            // * & *
            //check if the forward one and backward one are same in column
            if ((column >= 1 && column < Constants.Columns - 1 && shapes[row, column + 1] != null && shapes[row, column - 1] != null))
            {

                //check if two previous horizontal are of the same type
                while (shapes[row, column + 1].GetComponent<Shape>()
                    .IsSameType(newCandy.GetComponent<Shape>())
                    && shapes[row, column - 1].GetComponent<Shape>().IsSameType(newCandy.GetComponent<Shape>()))
                {
                    newCandy = GetRandomCandy();
                }
            }

            #endregion

            InstantiateAndPlaceNewCandy(row, column, newCandy);

        }
    }



    /// <summary>
    /// Destroys the item from the scene and instantiates a new explosion gameobject
    /// </summary>
    /// <param name="item"></param>
    private void RemoveFromScene(GameObject item)
    {
        GameObject explosion = null;
        Quaternion rot = Quaternion.identity;
        float animDuration = Constants.ExplosionDuration;
        var pos = item.transform.position;


        //remove also from the powerups copy otherwise additional powers will be added after shuffle
        RemoveFromThePowerCopy(item);

        if (item.GetComponent<Shape>().Bonus == BonusType.DestroyWholeRowColumn)
        {
            soundManager.PlayBoosterExplosionSound();
            var newExpl = Instantiate(rowColExplosion, pos, rot) as GameObject;
            Destroy(newExpl, animDuration);

            if (item.GetComponent<Shape>().rowBonus)
            {
                animDuration = Constants.AnimationDuration;
                if (st.playerNumber == "Player1")
                {
                    pos = new Vector2(RowExplosion.transform.position.x, item.transform.position.y);
                    explosion = RowExplosion;
                }
                else if (st.playerNumber == "Player2")
                {
                    pos = new Vector2(RowExplRight.transform.position.x, item.transform.position.y);
                    explosion = RowExplRight;
                }
            }
            else if (item.GetComponent<Shape>().colBonus)
            {
                animDuration = Constants.AnimationDuration + 0.1f;
                pos = new Vector2(item.transform.position.x, ColumnExplosion.transform.position.y);
                explosion = ColumnExplosion;
            }
        }
        else if (item.GetComponent<Shape>().Type == brickPrefab.name)
        {
            soundManager.PlayBricksSmashingSound();
            explosion = stoneBlast;
        }

        else if (item.GetComponent<Shape>().Bonus == BonusType.DestroyNineCandies)
        {
            soundManager.PlayBombExplosionSound();
            explosion = BombExplosion;
        }
        else if (electricMatch)
        {
            explosion = electroExplosion;
        }
        else
        {
            explosion = GetRandomExplosion();
        }

        if (explosion != null)
        {
            var newExplosion = Instantiate(explosion, item.transform.position, rot) as GameObject;
            Destroy(newExplosion, animDuration);
        }
        PhotonNetwork.Destroy(item.GetComponent<PhotonView>());
    }

    /// <summary>
    /// Get a random candy
    /// </summary>
    /// <returns></returns>
    private GameObject GetRandomCandy()
    {
        return CandyPrefabs[Random.Range(0, CandyPrefabs.Count)];
    }

    private void InitializeVariables(bool restart)
    {
        if (restart)
        {
            st.set_userScore(0);
        }

        ShowScore();
    }

    private void IncreaseScore(int amount)
    {
        //prevScore = score;
        // score += amount;
        st.set_userScore(st.get_userScore() + amount);
        //prevScore - score;
        ShowScore();
    }

    private void ShowScore()
    {
        NewScore1Text.text = st.get_userScore().ToString();
        //  obps.Call_RPC_forScore();
    }

    /// <summary>
    /// Get a random explosion
    /// </summary>
    /// <returns></returns>
    private GameObject GetRandomExplosion()
    {
        return ExplosionPrefabs[Random.Range(0, ExplosionPrefabs.Count)];
    }

    /// <summary>
    /// Gets the specified Bonus for the specific type
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private GameObject GetBonusFromType(string type)
    {
        string color = type.Split('_')[1].Trim();
        foreach (var item in BonusPrefabs)
        {
            if (item.GetComponent<Shape>().Type.Contains(color))
            {
                powerUpsCopy.Add(item);
                //      Debug.Log(item.name + "Added to the copy of powerup");
                return item;
            }
        }
        throw new System.Exception("Wrong type");
    }

    private GameObject GetBombBonusFromType(string type)
    {
        string color = type.Split('_')[1].Trim();
        foreach (var item in BombPrefabs)
        {
            if (item.GetComponent<Shape>().Type.Contains(color))
            {
                powerUpsCopy.Add(item);
                //       Debug.Log(item.name + "Added to the copy of powerup");
                return item;
            }
        }
        throw new System.Exception("Wrong type");
    }

    /// <summary>
    /// Starts the coroutines, keeping a reference to stop later
    /// </summary>
    private void StartCheckForPotentialMatches()
    {
        StopCheckForPotentialMatches();
        //get a reference to stop it later
        CheckPotentialMatchesCoroutine = CheckPotentialMatches();
        StartCoroutine(CheckPotentialMatchesCoroutine);
    }

    /// <summary>
    /// Stops the coroutines
    /// </summary>
    private void StopCheckForPotentialMatches()
    {
        if (AnimatePotentialMatchesCoroutine != null)
            StopCoroutine(AnimatePotentialMatchesCoroutine);
        if (CheckPotentialMatchesCoroutine != null)
            StopCoroutine(CheckPotentialMatchesCoroutine);
        ResetOpacityOnPotentialMatches();
    }

    /// <summary>
    /// Resets the opacity on potential matches (probably user dragged something?)
    /// </summary>
    private void ResetOpacityOnPotentialMatches()
    {
        if (potentialMatches != null)
            foreach (var item in potentialMatches)
            {
                if (item == null) break;

                Color c = item.GetComponent<SpriteRenderer>().color;
                c.a = 1.0f;
                item.GetComponent<SpriteRenderer>().color = c;
            }
    }


    private void ShowShuffleAlert()
    {
        shuffleAlert.AddComponent<TweenScale>().from = new Vector3(0.5f, 0.5f, 0.5f);
        shuffleAlert.GetComponent<TweenScale>().to = new Vector3(2f, 2f, 2f);
        shuffleAlert.GetComponent<UILabel>().enabled = true;
        shuffleAlert.GetComponent<TweenScale>().PlayForward();
    }

    private void HideShuffleAlert()
    {
        shuffleAlert.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        Destroy(shuffleAlert.GetComponent<TweenScale>());
        shuffleAlert.GetComponent<UILabel>().enabled = false;
    }

    /// <summary>
    /// Finds potential matches
    /// </summary>
    /// <returns></returns>
    /// 
    private IEnumerator CheckPotentialMatches()
    {
        yield return new WaitForSeconds(Constants.WaitBeforeHint);
        potentialMatches = Utilities.GetPotentialMatches(shapes);

        if (potentialMatches == null)
        {
            state = GameState.Animating;
            ShowShuffleAlert();
            yield return new WaitForSeconds(Constants.WaitForShuffle);
            HideShuffleAlert();
            InitializeCandyAndSpawnPositions(false);
            state = GameState.None;
        }
        if (potentialMatches != null)
        {
            while (true)
            {
                AnimatePotentialMatchesCoroutine = Utilities.AnimatePotentialMatches(potentialMatches);
                StartCoroutine(AnimatePotentialMatchesCoroutine);
                yield return new WaitForSeconds(Constants.WaitBeforeHint);
            }
        }
    }

    /// <summary>
    /// Gets a specific candy or Bonus based on the premade level information.
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    private GameObject GetSpecificCandyOrBonusForPremadeLevel(string info)
    {
        var tokens = info.Split('_');

        if (tokens.Count() == 1)
        {
            foreach (var item in CandyPrefabs)
            {
                if (item.GetComponent<Shape>().Type.Contains(tokens[0].Trim()))
                    return item;
            }

        }
        else if (tokens.Count() == 2 && tokens[1].Trim() == "B")
        {
            foreach (var item in BonusPrefabs)
            {
                if (item.name.Contains(tokens[0].Trim()))
                    return item;
            }
        }

        throw new System.Exception("Wrong type, check your premade level");
    }
}
