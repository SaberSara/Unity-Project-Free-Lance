using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UI;
//using Soomla.Store;
//using Soomla.Store.CombatCrush;
//126 125 532 uncomment to finish 

public class ShapesManager : MonoBehaviour
{

    #region Variables  
    public Text DebugText;
    public bool ShowDebugInfo = false;
    public Text NewScoreText;
    private CharacterActions anim;
    private ShapesArray shapes;
    private GameManager gm;
    private StatManager st;
    private GameObject shuffleAlert;
    private int score, prevScore;
    private bool movesOver = false;
    private ObservePlayerScore obps;
    private ObserveMoves obms;

    //public readonly Vector2 BottomRight = new Vector2(-2.37f, -4.27f);
    public Vector2 BottomRight = new Vector2(-20.8f, -8.6f);
    public Vector2 CandySize = new Vector2(2.1f, 2.1f);

    private GameState state = GameState.None;
    private GameObject hitGo = null;
    private Vector2[] SpawnPositions;

    //prefabs list
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
    public List<GameObject> BonusPrefabs;
	public List<GameObject> HorizontalPrefabsSpecial;
	public List<GameObject> VerticalPrefabsSpecial;

    public List<GameObject> BombPrefabs;

	public GameObject boltPrefab;
    public GameObject ElectroPrefab;
    public GameObject gotPowerAnimPrefab;
    public GameObject brickPrefab;
    public GameObject stoneBlast;
    public GameObject electricCharge;
    public GameObject electroExplosion;
    public GameObject rowColExplosion;

    private IEnumerator CheckPotentialMatchesCoroutine;
    private IEnumerator AnimatePotentialMatchesCoroutine;

    IEnumerable<GameObject> potentialMatches;
    private bool gameStarted = false;
    private SoundManager soundManager;

    private List<GameObject> powerUpsCopy = null;

    int[,] spawnPositionForPower;
    int[,] spawnPositionForBrick;

    int bricksCount = 0; //number of bricks to spawn on the level start
                         // int bricksQueued = 15; //test number of bricks to add instead of randowm candies
    private bricksCounter bc;


    public Text bombsCount;
	public Text boostersCount;
	public Text electroCount;

    bool BombSelected = false;
    bool BoosterSelected = false;
    bool ElectroSelected = false;
    bool HammerSelected = false;

	public bool exterminateCandysBonus = false;

    bool electricMatch = false;

    bool lastWasBrick = false;

    public bool gameStartFirstTime = false;

    public bool readyGoState = false;
	public bool isVerticalBonus;
    private GameObject readyGo;
    private bool opponentCanAddBrick = true;

    private int attackScore =Constants.minAttackScore;
    private int superAttackScore = Constants.minSuperAttackScore;
    #endregion

    void Awake()
    {
        //NewScoreText = GameObject.Find("Score1").transform.GetChild(1).GetComponent<UILabel>();

    }

	IEnumerator WaitTenSeconds() {
		yield return new WaitForSeconds (rndFloatSec);
			exterminateCandysBonus = true ;
	}
	float rndFloatSec;
    void Start()
    {
		rndFloatSec = Random.Range (10, 20);
		StartCoroutine(WaitTenSeconds());
        gameStarted = true;
        gameStartFirstTime = false;
        readyGoState = false;
        bc = (bricksCounter)FindObjectOfType(typeof(bricksCounter));
        powerUpsCopy = new List<GameObject>();
        gm = (GameManager)FindObjectOfType(typeof(GameManager));
        st = (StatManager)FindObjectOfType(typeof(StatManager));

        gm.isCongratsMenuOpen = false;
        gm.isLevelFailedMenuOpen = false;

        if (Application.loadedLevelName == "mainGameTimeBased")
        {
            st.setTimerLabelObject(GameObject.Find("Timer").transform.GetChild(1).GetComponent<Text>());
            st.setTimerLabel(st.getTotalTimeInSeconds().ToString());
        }
        else if (Application.loadedLevelName == "mainGame")
        {
            st.setUserMovesUILabelObject(GameObject.Find("Moves1").transform.GetChild(1).GetComponent<Text>());
            //st.setUserMovesLabel(st.getTotalMoves().ToString());
        }

        while (st.getCandiesAllowed() < CandyPrefabs.Count)
        {
            CandyPrefabs.Remove(CandyPrefabs.Last());
            BombPrefabs.Remove(BombPrefabs.Last());
            BonusPrefabs.Remove(BonusPrefabs.Last());
        }

//        //Finding text labels
//        bombsCount = GameObject.Find("Bomb").transform.GetChild(1).GetComponent<UILabel>();
//        boostersCount = GameObject.Find("Booster").transform.GetChild(1).GetComponent<UILabel>();
//        electroCount = GameObject.Find("Electro").transform.GetChild(1).GetComponent<UILabel>();

        //updatePowerUpCounters()
        UpdatePowerUpLabels();


        obps = (ObservePlayerScore)FindObjectOfType(typeof(ObservePlayerScore));
        obms = (ObserveMoves)FindObjectOfType(typeof(ObserveMoves));



        shuffleAlert = GameObject.Find("NoMoreMovesG1");
        anim = (CharacterActions)FindObjectOfType(typeof(CharacterActions));
        InitializeTypesOnPrefabShapesAndBonuses();
        InitializeCandyAndSpawnPositions(true);

        readyGo = GameObject.Find("ReadyGo");

        StartCheckForPotentialMatches();
        soundManager = (SoundManager)FindObjectOfType(typeof(SoundManager));
        soundManager.InitializeUserSounds();
        if(st.getCurrentLevel()>16)
        {
            attackScore = Constants.minAttackScoreLevel2;
            superAttackScore = Constants.minSuperAttackScoreLevel2;
        }
    }

    //to update the counter of power up
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
        //        int[,] positionToGenerateBrick;
        //        positionToGenerateBrick = new int[bc.getOppoBricks(), 2];
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
            //     Debug.Log(item.name);
        }
        //assign the name of the respective "normal" candy as the type of the Bonus
        foreach (var item in BonusPrefabs)
        {
            item.GetComponent<Shape>().Type = CandyPrefabs.
                Where(x => x.GetComponent<Shape>().Type.Contains(item.name.Split('_')[1].Trim())).Single().name;

            //         Debug.Log(item.GetComponent<Shape>().Type);
        }

        //assign the name of the respective "normal" candy as the type of the Bomb Bonus
        foreach (var item in BombPrefabs)
        {
            item.GetComponent<Shape>().Type = CandyPrefabs.
                Where(x => x.GetComponent<Shape>().Type.Contains(item.name.Split('_')[1].Trim())).Single().name;
        }

		foreach (var item in HorizontalPrefabsSpecial)
		{
			try {
			item.GetComponent<Shape>().Type = CandyPrefabs.
				Where(x => x.GetComponent<Shape>().Type.Contains(item.name.Split('_')[1].Trim())).Single().name;
			}
			catch {
			}
			//         Debug.Log(item.GetComponent<Shape>().Type);
		}

		foreach (var item in VerticalPrefabsSpecial)
		{
			try {
			item.GetComponent<Shape>().Type = CandyPrefabs.
				Where(x => x.GetComponent<Shape>().Type.Contains(item.name.Split('_')[1].Trim())).Single().name;
			}
			catch {
			}
			//         Debug.Log(item.GetComponent<Shape>().Type);
		}

        ElectroPrefab.GetComponent<Shape>().Type = "ElectroPower";
        brickPrefab.GetComponent<Shape>().Type = brickPrefab.name;

    }

    //not bieng used, to spawn the matched candies close to eachother
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

        //order is important for two functions below
        setRandomPositionsForPowerUps();
        setRandomPositionsForBricks();

        for (int row = 0; row < Constants.Rows; row++)
        {
            for (int column = 0; column < Constants.Columns; column++)
            {

                GameObject newCandy = null;
                bool candyAssigned = false;

                //if we have powerups in list
                //         Debug.Log("Power up count is : " + powerUpsCopy.Count);
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
        GameObject go = Instantiate(newCandy,
            BottomRight + new Vector2(column * CandySize.x, row * CandySize.y), Quaternion.identity)
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
                Destroy(shapes[row, column]);
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
				if(true) //(StoreInventory.GetItemBalance(CombatCrushAssets.BOOSTER.ItemId) > 0)
                    {
                        state = GameState.SelectionStarted;
                        BoosterSelected = true;
                    }
//                    else if (StoreInventory.GetItemBalance(CombatCrushAssets.BOOSTER.ItemId) == 0)
//                        gm.shop_popup();
                    break;

                case "Bomb":
				if (true) //(StoreInventory.GetItemBalance(CombatCrushAssets.BOMB.ItemId) > 0)
                    {
                        state = GameState.SelectionStarted;
                        BombSelected = true;
                    }
//                    else if (StoreInventory.GetItemBalance(CombatCrushAssets.BOMB.ItemId) == 0)
//                        gm.shop_popup();
                    break;

                case "Electro":
				if (true) //(StoreInventory.GetItemBalance(CombatCrushAssets.ELECTRO.ItemId) > 0)
                    {
                        state = GameState.SelectionStarted;
                        ElectroSelected = true;
                    }
//                    else if (StoreInventory.GetItemBalance(CombatCrushAssets.ELECTRO.ItemId) == 0)
//                        gm.shop_popup();
                    break;

                case "Hammer":
				if (true) //(StoreInventory.GetItemBalance(CombatCrushAssets.HAMMER.ItemId) > 0)
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

    // Update is called once per frame
    void Update()
    {
        if (gameStarted)
        {
            if (!gameStartFirstTime)
            {
                StartCoroutine(WaitForFewSecBeforeGameStarts(2));
            }
            else
            {
                UpdateInputFunction();
                if (opponentCanAddBrick)
                    ReplaceRandomCandyWithBrick();
            }

        }
    }

    IEnumerator WaitForFewSecBeforeGameStarts(int secs)
    {
        yield return new WaitForSeconds(secs);
        StartCoroutine(WaitForFewSecGameStart(1));
    }

    IEnumerator WaitForFewSecGameStart(int secs)
    {
        readyGo.transform.GetChild(1).GetComponent<Text>().text = "GO!";
        yield return new WaitForSeconds(secs);
        UpdateInputFunction();

    }

	private bool isHorizontalMove = false;
    void UpdateInputFunction()
    {
        //Time.timeScale = 1;
        gameStartFirstTime = true;
        readyGoState = true;
        readyGo.transform.GetChild(0).gameObject.SetActive(false);
        readyGo.transform.GetChild(1).gameObject.SetActive(false);
        if (state == GameState.None)
        {
            //user has clicked or touched
            if (Input.GetMouseButtonDown(0))
            {
                //get the hit position
                var hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
                if (hit.collider != null && hit.collider.gameObject.name != "Blocker") //we have a hit!!!
                {
                    hitGo = hit.collider.gameObject;
                    state = GameState.SelectionStarted;
                }

            }
        }
        //if its a second object
        else if (state == GameState.SelectionStarted)
        {

            if (Input.GetMouseButton(0) && !movesOver)
            {
                var hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

                //if user clicked on any of the icons to choose the bought power ups
                if (BombSelected || BoosterSelected || ElectroSelected || HammerSelected || exterminateCandysBonus)
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

						if (exterminateCandysBonus) {
							exterminateCandysBonus = false;
							CreateExterminatorBonus (hitGoCache);
						}
                        else if (BombSelected)
                        {
                           // StoreInventory.TakeItem(CombatCrushAssets.BOMB.ItemId, 1);
                            CreateBombBonus(hitGoCache);

                        }
                        else if (BoosterSelected)
                        {
                            //StoreInventory.TakeItem(CombatCrushAssets.BOOSTER.ItemId, 1);
                            CreateBonus(hitGoCache);

                        }
                        else if (ElectroSelected)
                        {
                          //  StoreInventory.TakeItem(CombatCrushAssets.ELECTRO.ItemId, 1);
                            CreateElectroPower(hitGoCache);

                        }
                        else if (HammerSelected || hit.collider.gameObject.GetComponent<Shape>().Type == brickPrefab.name)
                        {
                           // StoreInventory.TakeItem(CombatCrushAssets.HAMMER.ItemId, 1);
                            ReplaceBrickWithRandomCandy(hit.collider.gameObject);
                            bc.RemoveUsersBricks(1);
                        }
                       //StoreInventory.RefreshLocalInventory();
                        BombSelected = false;
                        BoosterSelected = false;
                        ElectroSelected = false;
                        HammerSelected = false;
                        UpdatePowerUpLabels();

                    }
                    state = GameState.None;
                }

                //we have a hit
                else if (hit.collider != null && hitGo != hit.collider.gameObject && hit.collider.gameObject.name != "Blocker")
                {

                    //user did a hit, no need to show him hints 
                    StopCheckForPotentialMatches();

					if (Utilities.AreHorizontalNeighbors (hitGo.GetComponent<Shape> (),
						    hit.collider.gameObject.GetComponent<Shape> ())) {
						isHorizontalMove = true;
						Debug.Log ("HorizontalNeighborhood");
					} else {
						isHorizontalMove = false;
					}
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
    }

    void FixedUpdate()
    {
        if (Application.loadedLevelName == "mainGameTimeBased" || Application.loadedLevelName == "MultiplayerTimeBased")
        {
            if (readyGoState)
            {
                //================ TIMER ===================================
                if (st.getTimerCounter() >= 0)
                {
                    st.setTimerCounter((float)System.Math.Round(((double)st.getTimerCounter() - Time.fixedDeltaTime), 2));
                    st.setTimerLabel(st.getTimerCounter().ToString("F0"));
                }
                else
                {
                    movesOver = true;
                    gm.GameOver();
                }
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

    /// <summary>
    /// Checks if five matches are on the same row or column
    /// </summary>
    /// <param name="Total matched candies"></param>
    /// 

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

    /// <summary>
    /// Check if the four candies are same, and not have bricks in the matches, to avoid giving the power
    /// </summary>
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
        Debug.Log(count);
        return count;
    }

    private void ReduceMoves()
    {
        if (Application.loadedLevelName == "mainGame" || Application.loadedLevelName == "MultiplayerMoveBased")
        {
            //Warning Caustion Dangerous Area
            //UserMoves Calculat
            if (st.get_userMoves() > 0)
            {
                st.set_userMoves(st.get_userMoves() - 1); // user moves counter set
                st.setUserMovesLabel(st.get_userMoves().ToString());
                if (Application.loadedLevelName == "MultiplayerMoveBased")
                {
                    obms.Call_RPC_forMoves();
                }
            }
            if (st.get_userMoves() <= 0)
            {
                st.setUserMovesLabel(st.get_userMoves().ToString());
                movesOver = true;
                if (st.get_compMoves() <= 0)
                {
                    gm.GameOver();
                }
            }
        }
    }

    private void gotPowerAnimation(GameObject go)
    {
        var newExplosion = Instantiate(gotPowerAnimPrefab, go.transform.position, Quaternion.identity) as GameObject;
        Destroy(newExplosion, Constants.ExplosionDuration);
    }

	bool findMatchesANDCollapseFinished = true ;
    private IEnumerator FindMatchesAndCollapse(RaycastHit2D hit2)
    {
		if (!findMatchesANDCollapseFinished) {
			yield break;
		}

		findMatchesANDCollapseFinished = false;
        #region Variables 
        bool didMatch = false;

		List<GameObject> goToClean = new List<GameObject>();
		IEnumerable<GameObject> totalMatches;
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


        #endregion

        shapes.Swap(hitGo, hitGo2);


        //move the swapped ones
        hitGo.transform.positionTo(Constants.AnimationDuration, hitGo2.transform.position);
        hitGo2.transform.positionTo(Constants.AnimationDuration, hitGo.transform.position);
        yield return new WaitForSeconds(Constants.AnimationDuration);


        opponentCanAddBrick = false;

        #region hasUserDragged a SpecialHorizontalCandy
		bool GotASuperMove = false;
		if (hitGo.GetComponent<Shape>().Bonus == BonusType.DestroyWholeCandies && isHorizontalMove && !isVerticalBonus)
		{
			Debug.Log("DESTROYYYYYYYYYY WHHHHHHHHOLE");
			GotASuperMove = true ;
		}
		else if(hitGo.GetComponent<Shape>().Bonus == BonusType.DestroyWholeCandies && !isHorizontalMove && isVerticalBonus) {
			GotASuperMove = true ;
		}

        if (hitGo.GetComponent<Shape>().Bonus == BonusType.DestroyWholeRowColumn
           && hitGo2.GetComponent<Shape>().Bonus == BonusType.DestroyWholeRowColumn)
        {
            totalMatches = shapes.GetEntireColumn(hitGo2).Union(shapes.GetEntireRow(hitGo2)).Distinct();
            var rowExplosion = Instantiate(RowExplosion, new Vector2(RowExplosion.transform.position.x, hitGo.transform.position.y), Quaternion.identity) as GameObject;
            var colExplosion = Instantiate(ColumnExplosion, new Vector2(hitGo.transform.position.x, RowExplosion.transform.position.y), Quaternion.identity) as GameObject;
            Destroy(rowExplosion, Constants.AnimationDuration);
            Destroy(colExplosion, Constants.AnimationDuration);
            bc.AddToOpponent(1);

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
            bc.AddToOpponent(2);

        }
        #endregion

        #region Matching electros

        else if (hitGo.GetComponent<Shape>().Bonus == BonusType.DestroySameCandies
           && hitGo2.GetComponent<Shape>().Bonus == BonusType.DestroySameCandies)
        {
            totalMatches = shapes.GetAllCandies();
            var charge = Instantiate(WrappedWholeWrapped, hitGo.transform.position, Quaternion.identity) as GameObject;
            Destroy(charge, Constants.ElectricChargeDuration);
            bc.AddToOpponent(3);
        }
        #endregion

        #region Matching boosters with Bomb

        else if (hitGo.GetComponent<Shape>().Bonus == BonusType.DestroyWholeRowColumn
           && hitGo2.GetComponent<Shape>().Bonus == BonusType.DestroyNineCandies)
        {
//			IEnumerable<GameObject> tempMatch;
//			tempMatch = shapes.FindMatchingCandiesInGrid(hitGo,hitGo2).MatchedCandy;
//			int i = 0;
//			foreach(GameObject go in tempMatch) {
//				if(i==0) {
//					tempMatch = tempMatch.Union(shapes.GetEntireColumn(go));
//					i = 1;
//				}
//				else if (i==1) {
//					tempMatch = tempMatch.Union(shapes.GetEntireRow(go));
//					i= 0 ;
//				}
//			}
			totalMatches = shapes.GetEntireColumn(hitGo).Union(shapes.GetNineCandiesAroundBomb(hitGo2)).Distinct();
            var bombExplosion = Instantiate(StripedColorComb, hitGo.transform.position, Quaternion.identity) as GameObject;
            var colExplosion = Instantiate(ColumnExplosion, new Vector2(hitGo.transform.position.x, RowExplosion.transform.position.y), Quaternion.identity) as GameObject;
            Destroy(bombExplosion, 1f);
            Destroy(colExplosion, 1f);
            bc.AddToOpponent(2);
        }
        else if (hitGo.GetComponent<Shape>().Bonus == BonusType.DestroyNineCandies
           && hitGo2.GetComponent<Shape>().Bonus == BonusType.DestroyWholeRowColumn)
        {
//			IEnumerable<GameObject> tempMatch;
//			tempMatch = shapes.FindMatchingCandiesInGrid(hitGo,hitGo2).MatchedCandy;
//			int i = 0;
//			foreach(GameObject go in tempMatch) {
//				if(i==0) {
//					tempMatch = tempMatch.Union(shapes.GetEntireColumn(go));
//					i = 1;
//				}
//				else if (i==1) {
//					tempMatch = tempMatch.Union(shapes.GetEntireRow(go));
//					i= 0 ;
//				}
//			}
			totalMatches = shapes.GetNineCandiesAroundBomb(hitGo).Union(shapes.GetEntireRow(hitGo2)).Distinct();
			var bombExplosion = Instantiate(StripedColorComb, hit2.transform.position, Quaternion.identity) as GameObject;
            var rowExplosion = Instantiate(ColumnExplosion, new Vector2(hitGo2.transform.position.x, RowExplosion.transform.position.y), Quaternion.identity) as GameObject;
            Destroy(bombExplosion, 1f);
            Destroy(rowExplosion, 1f);
            bc.AddToOpponent(2);
        }

        #endregion

        #region Matching Boosters with electro
        else if (hitGo.GetComponent<Shape>().Bonus == BonusType.DestroyWholeRowColumn
          && hitGo2.GetComponent<Shape>().Bonus == BonusType.DestroySameCandies)
        {
            Debug.Log("LIke yea");
			totalMatches = shapes.GetThreeEntireRow(hitGo2).Union(shapes.GetThreeEntireColumn(hitGo)).Union(shapes.FindMatchingCandiesInGrid(hitGo, hitGo2).MatchedCandy).Distinct();
            var electrExplosion = Instantiate(electricCharge, electricCharge.transform.position, Quaternion.identity) as GameObject;
            var colExplosion = Instantiate(StrippedWrappedColAnim, new Vector2(hitGo.transform.position.x, RowExplosion.transform.position.y), Quaternion.identity) as GameObject;
            Destroy(electrExplosion, Constants.ElectricChargeDuration);
			Destroy(colExplosion, 1f);
            bc.AddToOpponent(2);
        }
        else if (hitGo.GetComponent<Shape>().Bonus == BonusType.DestroySameCandies
           && hitGo2.GetComponent<Shape>().Bonus == BonusType.DestroyWholeRowColumn)
        {
            Debug.Log("LIke yea 2");

			totalMatches = (shapes.FindMatchingCandiesInGrid(hitGo2, hitGo).MatchedCandy).Union(shapes.GetThreeEntireColumn(hitGo2)).Union(shapes.GetThreeEntireRow(hitGo2)).Distinct();
            var electrExplosion = Instantiate(electricCharge, electricCharge.transform.position, Quaternion.identity) as GameObject;
			var rowExplosion = Instantiate(StrippedWrappedRowAnim, new Vector2(hitGo2.transform.position.x, RowExplosion.transform.position.y), Quaternion.identity) as GameObject;
            Destroy(electrExplosion, Constants.ElectricChargeDuration);
			Destroy(rowExplosion, 1f);
            bc.AddToOpponent(2);
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
            bc.AddToOpponent(2);
        }
        else if (hitGo.GetComponent<Shape>().Bonus == BonusType.DestroySameCandies
           && hitGo2.GetComponent<Shape>().Bonus == BonusType.DestroyNineCandies)
        {
            totalMatches = (shapes.FindMatchingCandiesInGrid(hitGo2, hitGo).MatchedCandy).Union(shapes.GetNineCandiesAroundBomb(hitGo2)).Distinct();
            var electrExplosion = Instantiate(electricCharge, electricCharge.transform.position, Quaternion.identity) as GameObject;
			var bombExplosion = Instantiate(WrappedColorComb , hitGo.transform.position, Quaternion.identity) as GameObject;
            Destroy(electrExplosion, Constants.ElectricChargeDuration);
            Destroy(bombExplosion, 1f);
            bc.AddToOpponent(2);
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

            //if elecro power ups
            #region Electro Power up
            if (hitGo.GetComponent<Shape>().Type == ElectroPrefab.GetComponent<Shape>().Type)
            {
                totalMatches = shapes.FindMatchingCandiesInGrid(hitGo2, hitGo).MatchedCandy;
                electricMatch = true;

                soundManager.PlayElectro();
                bc.AddToOpponent(3);
                var newExplosion = Instantiate(electricCharge, electricCharge.transform.position, Quaternion.identity) as GameObject;
                Destroy(newExplosion, Constants.ElectricChargeDuration);

            }
            else if (hitGo2.GetComponent<Shape>().Type == ElectroPrefab.GetComponent<Shape>().Type)
            {
                totalMatches = shapes.FindMatchingCandiesInGrid(hitGo, hitGo2).MatchedCandy;
                electricMatch = true;

                soundManager.PlayElectro();
                bc.AddToOpponent(3);
                var newExplosion = Instantiate(electricCharge, electricCharge.transform.position, Quaternion.identity) as GameObject;
                Destroy(newExplosion, Constants.ElectricChargeDuration);
            }
            #endregion

            //else check for other mathced and powers
            #region chekcking for matches and other power
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
					if(GotASuperMove) {
						totalMatches = shapes.GetAllCandies();
					}
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
                        //if 4 matches and no Bonus is contained in the line, we will award a new Bonus
                        addBonus = sameFour == Constants.MinimumMatchesForBonus  &&
                            !BonusTypeUtilities.ContainsDestroyWholeRowColumn(hitGomatchesInfo.BonusesContained);

                        //if 5 matches, and on the same row or column, give electro
                        addElectro = hitGomatchesInfo.MatchedCandy.Count() >= Constants.MatchCountForPower && sameFive && sameColor &&
                            !BonusTypeUtilities.ContainsDestroyWholeRowColumn(hitGomatchesInfo.BonusesContained);

                        //if 5 or more matches and not in same line, can be in L shape arrow shape etc, award bomb
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
                        addBonus1 = sameFour1 == Constants.MinimumMatchesForBonus && 
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

        #endregion
        while (totalMatches.Count() >= Constants.MinimumMatches || electricMatch)
        {

            //  Debug.Log("Inside while loop");
            #region Keep matching if the new candies match again

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
                //   Debug.Log(item.name);
                //if the candies are bricks replace them with random candies
				if(GotASuperMove)
					yield return new WaitForSeconds(0.1f);
                if (item.GetComponent<Shape>().Type == brickPrefab.name)
                {
                    ReplaceBrickWithRandomCandy(item);
                    bricksCount--;
                }
                //just remove
                else
                {
					if(GotASuperMove){
					Vector3 moveDirection =item.transform.position - hitGo.transform.position;
					if(moveDirection != Vector3.zero) {
						float angle = Mathf.Atan2(moveDirection.x,moveDirection.y)*Mathf.Rad2Deg;
						GameObject go = (GameObject)Instantiate(boltPrefab,hitGo.transform.position,Quaternion.AngleAxis(angle,Vector3.forward));
						go.GetComponent<DistanceToScale>().distance = Vector3.Distance(item.transform.position,hitGo.transform.position)/5;
					}
					}
					shapes.Remove(item);
                    RemoveFromScene(item);
					goToClean.Add(item);
                }
            }

            #region Adding Powerups
            if (!electricMatch)
            {
                //check and instantiate Bonus if needed
                if (addBonus)
                {
                    IncreaseScore(Constants.BoosterScore);
                    CreateBonus(hitGoCache);
                    soundManager.PlayBoosterCreationSound();
                }
                if (addElectro)
                {
                    IncreaseScore(Constants.ElectroScore);
                    CreateElectroPower(hitGoCache);
                    soundManager.PlayElectroCreationSound();
                }

                if (addBomb)
                {
                    IncreaseScore(Constants.bombScore);
                    CreateBombBonus(hitGoCache);
                    soundManager.PlayBombCreationSound();
                }

                if (addBonus1)
                {
                    IncreaseScore(Constants.BoosterScore);
                    CreateBonus(hitGoCache1);
                    soundManager.PlayBoosterCreationSound();

                }
                if (addElectro1)
                {
                    IncreaseScore(Constants.ElectroScore);
                    CreateElectroPower(hitGoCache1);
                    soundManager.PlayElectroCreationSound();
                }

                if (addBomb1)
                {
                    IncreaseScore(Constants.bombScore);
                    CreateBombBonus(hitGoCache1);
                    soundManager.PlayBombCreationSound();
                }

                addBonus = false;
                addElectro = false;
                addBomb = false;

                addBonus1 = false;
                addElectro1 = false;
                addBomb1 = false;
            }
            #endregion
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

            timesRun++;
            #endregion
            didMatch = true;
            electricMatch = false;
        }

        #region moves, scoring and throwing bricks

        opponentCanAddBrick = true;
        if (didMatch)
            ReduceMoves();

        if (((st.get_userScore() - prevScore) > attackScore) && ((st.get_userScore() - prevScore) < superAttackScore))
        {
            anim.Attack();
            StartCoroutine(waitFunction(Constants.AnimationDuration));
        }
        else if ((st.get_userScore() - prevScore) >= superAttackScore)
        {
            anim.SuperAttack();
            StartCoroutine(waitFunction(Constants.AnimationDuration));
        }
        prevScore = st.get_userScore();

        #endregion
        state = GameState.None;
        if (!movesOver)
        {
            StartCheckForPotentialMatches();
        }

		foreach (var item in goToClean) {
			Destroy (item);
		}

		findMatchesANDCollapseFinished = true;

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
        GameObject Bonus = Instantiate(GetBonusFromType(hitGoCache.Type), BottomRight
            + new Vector2(hitGoCache.Column * CandySize.x,
                hitGoCache.Row * CandySize.y), Quaternion.identity)
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
        GameObject Bonus = Instantiate(GetBombBonusFromType(hitGoCache.Type), BottomRight
            + new Vector2(hitGoCache.Column * CandySize.x,
                hitGoCache.Row * CandySize.y), Quaternion.identity)
            as GameObject;
        shapes[hitGoCache.Row, hitGoCache.Column] = Bonus;
        var BonusShape = Bonus.GetComponent<Shape>();
        //will have the same type as the "normal" candy
        BonusShape.Assign(hitGoCache.Type, hitGoCache.Row, hitGoCache.Column);
        //add the proper Bonus type
        BonusShape.Bonus |= BonusType.DestroyNineCandies;
    }

	private void CreateExterminatorBonus(Shape hitGoCache)
	{
		if (hitGoCache.Type != "Stone_brick") {
			GameObject Bonus = Instantiate (GetExterminateBonusFromType (hitGoCache.Type), hitGo.transform.position, Quaternion.identity)
			as GameObject;
			shapes [hitGoCache.Row, hitGoCache.Column] = Bonus;
			var BonusShape = Bonus.GetComponent<Shape> ();
			//will have the same type as the "normal" candy
			BonusShape.Assign (hitGoCache.Type, hitGoCache.Row, hitGoCache.Column);
			//add the proper Bonus type
			BonusShape.Bonus |= BonusType.DestroyWholeCandies;
		}
	}

    private void CreateElectroPower(Shape hitGoCache)
    {
        powerUpsCopy.Add(ElectroPrefab);
        //    Debug.Log("electro power added ");
        GameObject Bonus = Instantiate(ElectroPrefab.gameObject, BottomRight + new Vector2(hitGoCache.Column * CandySize.x,
                hitGoCache.Row * CandySize.y), Quaternion.identity)
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

                GameObject newCandy = Instantiate(go, SpawnPositions[column], Quaternion.identity)
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

            GameObject newCandy = Instantiate(go, SpawnPositions[column], Quaternion.identity)
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
        var pos = item.transform.position;
        float animDuration = Constants.ExplosionDuration;

        //remove also from the powerups copy otherwise additional powers will be added after shuffle
        RemoveFromThePowerCopy(item);


        if (item.GetComponent<Shape>().Bonus == BonusType.DestroyWholeRowColumn)
        {
            soundManager.PlayBoosterExplosionSound();
            var newExpl = Instantiate(rowColExplosion, pos, rot) as GameObject;
            Destroy(newExpl, animDuration);
            bc.AddToOpponent(1);

            if (item.GetComponent<Shape>().rowBonus)
            {
                animDuration = Constants.AnimationDuration;
                pos = new Vector2(RowExplosion.transform.position.x, item.transform.position.y);
                explosion = RowExplosion;
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
            bc.AddToOpponent(2);
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
            var newExplosion = Instantiate(explosion, pos, rot) as GameObject;
            Destroy(newExplosion, animDuration);
         
        }

		item.gameObject.SetActive (false);
        //Destroy(item,0.1f);
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
        //ScoreText.text = "Score: " + score.ToString();
        NewScoreText.text = st.get_userScore().ToString();

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

	public GameObject GetExterminateBonusFromType(string type) {
		int i = Random.Range (0, 2);
		if (i == 1) {
			string color = type.Split ('_') [1].Trim (); 
			foreach (var item in HorizontalPrefabsSpecial) {
				if (item.GetComponent<Shape> ().Type.Contains (color)) {
					powerUpsCopy.Add (item);
					isVerticalBonus = false;
					return item;
				}
			}

			throw new System.Exception ("Wrong Type Ab");
		} else {
			string color = type.Split ('_') [1].Trim (); 
			foreach (var item in VerticalPrefabsSpecial) {
				if (item.GetComponent<Shape> ().Type.Contains (color)) {
					powerUpsCopy.Add (item);
					isVerticalBonus = true;
					return item;
				}
			}
			throw new System.Exception ("Wrong Type Ab");
		}
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
        shuffleAlert.GetComponent<Text>().enabled = true;
        shuffleAlert.GetComponent<TweenScale>().PlayForward();
    }

    private void HideShuffleAlert()
    {
        shuffleAlert.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        Destroy(shuffleAlert.GetComponent<TweenScale>());
        shuffleAlert.GetComponent<Text>().enabled = false;
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
