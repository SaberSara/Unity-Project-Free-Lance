﻿using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UI;


public class SP_ShapesManager : MonoBehaviour
{
    public Text DebugText;
    public bool ShowDebugInfo = false;
    private Text newScoreText;
    private GameObject hitGo, hitGo2;
    private GameObject shuffleAlert;
    private GameManager gm;
    private StatManager st;
    private bool movesOver = false;

    private SP_ShapesArray shapes;

    private int score, prevScore;

    //public readonly Vector2 BottomRight = new Vector2(-2.37f, -4.27f);
    public Vector2 BottomRight = new Vector2(-20.37f, -8.27f);
    public Vector2 CandySize = new Vector2(2f, 2f);

    private GameState state = GameState.None;
    private Vector2[] SpawnPositions;

    //prefabs
    public List<GameObject> CandyPrefabs;
    public List<GameObject> ExplosionPrefabs;
    public GameObject BombExplosion;
    public GameObject ColumnExplosion;
    public GameObject RowExplosion;
    public GameObject rowColExplosion;
    public List<GameObject> BonusPrefabs;
    public List<GameObject> BombPrefabs;
    public GameObject ElectroPrefab;
    public GameObject gotPowerAnimPrefab;
    public GameObject brickPrefab;
    public GameObject stoneBlast;
    public GameObject electricCharge;
    public GameObject electroExplosion;


    private CharacterActionsOpponent anim;

    private IEnumerator CheckPotentialMatchesCoroutine;
    private IEnumerator AnimatePotentialMatchesCoroutine;
    IEnumerable<GameObject> potentialMatches;
    private bool gameStarted = false;
    private SoundManager soundManager;

    private List<GameObject> powerUpsCopy;

    int[,] spawnPositionForPower;
    int[,] spawnPositionForBrick;

    int bricksCount = 0; //number of bricks to spawn on the level start
                         // int bricksQueued = 15; //test number of bricks to add instead of randowm candies
    private bricksCounter bc;
    bool lastWasBrick = false;
    private ShapesManager sm;

    bool firstTimeJanu = true;
    bool electricMatch = false;
    bool powerUpsUpdated = true;
    private bool opponentCanAddBrick=true;
    private int attackScore = Constants.minAttackScore;
    private int superAttackScore = Constants.minSuperAttackScore;

    void Awake()
    {
        DebugText.enabled = ShowDebugInfo;
        newScoreText = GameObject.Find("Score2").transform.GetChild(1).GetComponent<Text>();
    }

    // Use this for initialization
    void Start()
    {
        gameStarted = true;
        bc = (bricksCounter)FindObjectOfType(typeof(bricksCounter));

        powerUpsCopy = new List<GameObject>();

        gm = (GameManager)FindObjectOfType(typeof(GameManager));
        st = (StatManager)FindObjectOfType(typeof(StatManager));
        sm = (ShapesManager)FindObjectOfType(typeof(ShapesManager));


        while (st.getCandiesAllowed() < CandyPrefabs.Count)
        {
            CandyPrefabs.Remove(CandyPrefabs.Last());
            BombPrefabs.Remove(BombPrefabs.Last());
            BonusPrefabs.Remove(BonusPrefabs.Last());
        }


        if (Application.loadedLevelName == "mainGameTimeBased")
        {
            //st.setCompMovesUILabelObject (GameObject.Find ("Moves2").transform.GetChild (1).GetComponent<UILabel> ());
        }
        else if (Application.loadedLevelName == "mainGame")
        {
            st.setCompMovesUILabelObject(GameObject.Find("Moves2").transform.GetChild(1).GetComponent<Text>());
            st.setCompMovesLabel(st.getTotalMoves().ToString());
        }

        shuffleAlert = GameObject.Find("NoMoreMovesG2");
        anim = (CharacterActionsOpponent)FindObjectOfType(typeof(CharacterActionsOpponent));
        InitializeTypesOnPrefabShapesAndBonuses();
        InitializeCandyAndSpawnPositions(true);

        soundManager = (SoundManager)FindObjectOfType(typeof(SoundManager));
        soundManager.InitializeComputerSounds();

        if (st.getCurrentLevel() > 16)
        {
            attackScore = Constants.minAttackScoreLevel2;
            superAttackScore = Constants.minSuperAttackScoreLevel2;
        }
    }



    /// <summary>
    /// Replaces the random candy from grid with brick
    /// </summary>
    void ReplaceRandomCandyWithBrick()
    {
        //        int[,] positionToGenerateBrick;
        //        positionToGenerateBrick = new int[bc.getOppoBricks(), 2];
        for (int i = 0; i < bc.getOppoBricks(); i++)
        {
            int row4Brick = Random.Range(1, Constants.Rows);
            int col4Brick = Random.Range(0, Constants.Columns);

            while (shapes[row4Brick, col4Brick].GetComponent<SP_Shape>().Type == brickPrefab.name)
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
            bc.RemoveOppoBricks(1);
            bricksCount++;
        }

        //random Row
        // spawnPositionForBrick[i, 0] = Random.Range(1, Constants.Rows - 1);
        //random column
        //  spawnPositionForBrick[i, 1] = Random.Range(0, Constants.Columns - 1);

    }

    void setRandomPositionsForPowerUps()
    {
        int temp = 2;
        //  powerupsCount = electroCount + BoosterCount + BombCount;
        //      Debug.Log("Power up count is : " + powerUpsCopy.Count);
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


    void setRandomPositionsForBricks()
    {
        spawnPositionForBrick = new int[bricksCount, 2];

        for (int i = 0; i < bricksCount; i++)
        {
            //random Row
            spawnPositionForBrick[i, 0] = Random.Range(1, Constants.Rows);
            //random column
            spawnPositionForBrick[i, 1] = Random.Range(0, Constants.Columns);

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


    /// <summary>
    /// Initialize shapes
    /// </summary>
    private void InitializeTypesOnPrefabShapesAndBonuses()
    {
        brickPrefab.GetComponent<SP_Shape>().Type = brickPrefab.name;

        //just assign the name of the prefab
        foreach (var item in CandyPrefabs)
        {
            item.GetComponent<SP_Shape>().Type = item.name;
        }

        //assign the name of the respective "normal" candy as the type of the Bonus
        foreach (var item in BonusPrefabs)
        {
            item.GetComponent<SP_Shape>().Type = CandyPrefabs.
                Where(x => x.GetComponent<SP_Shape>().Type.Contains(item.name.Split('_')[1].Trim())).Single().name;
        }

        //assign the name of the respective "normal" candy as the type of the Bomb Bonus
        foreach (var item in BombPrefabs)
        {
            item.GetComponent<SP_Shape>().Type = CandyPrefabs.
                Where(x => x.GetComponent<SP_Shape>().Type.Contains(item.name.Split('_')[1].Trim())).Single().name;
        }
        ElectroPrefab.GetComponent<SP_Shape>().Type = "ElectroPower";
    }

    public void InitializeCandyAndSpawnPositionsFromPremadeLevel()
    {
        InitializeVariables(true);
        var premadeLevel = SP_DebugUtilities.FillShapesArrayFromResourcesData();
        if (shapes != null)
            DestroyAllCandy();

        shapes = new SP_ShapesArray();
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

        shapes = new SP_ShapesArray();
        SpawnPositions = new Vector2[Constants.Columns];

        setRandomPositionsForPowerUps();
        setRandomPositionsForBricks();

        for (int row = 0; row < Constants.Rows; row++)
        {
            for (int column = 0; column < Constants.Columns; column++)
            {

                GameObject newCandy = null;
                bool candyAssigned = false;

                //if we have powerups in list
                //           Debug.Log("Power up count is : " + powerUpsCopy.Count);
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

                //     if (newCandy == null)
                //         Debug.Log("new candy is null, never assigned");

                //check if two previous horizontal are of the same type
                while (column >= 2 && shapes[row, column - 1].GetComponent<SP_Shape>()
                    .IsSameType(newCandy.GetComponent<SP_Shape>())
                    && shapes[row, column - 2].GetComponent<SP_Shape>().IsSameType(newCandy.GetComponent<SP_Shape>()))
                {
                    newCandy = GetRandomCandy();
                }

                //check if two previous vertical are of the same type
                while (row >= 2 && shapes[row - 1, column].GetComponent<SP_Shape>()
                    .IsSameType(newCandy.GetComponent<SP_Shape>())
                    && shapes[row - 2, column].GetComponent<SP_Shape>().IsSameType(newCandy.GetComponent<SP_Shape>()))
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
        go.GetComponent<SP_Shape>().Assign(newCandy.GetComponent<SP_Shape>().Type, row, column);
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


    // Update is called once per frame
    void Update()
    {
        if (sm.gameStartFirstTime && firstTimeJanu)
        {
            firstTimeJanu = false;
            StartCheckForPotentialMatches();
        }
        else if (opponentCanAddBrick)

                ReplaceRandomCandyWithBrick();
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
                if (item.GetComponent<SP_Shape>().Row == totalMatches.ElementAt(i).GetComponent<SP_Shape>().Row)
                    sameCount++;
                else
                    sameCount = 0;

                if (sameCount >= 5)
                    return true;
            }
            sameCount = 0;

            for (int j = 0; j < totalMatches.Count(); j++)
            {
                if (item.GetComponent<SP_Shape>().Column == totalMatches.ElementAt(j).GetComponent<SP_Shape>().Column)
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
            if (item.GetComponent<SP_Shape>().Type == prevItem.GetComponent<SP_Shape>().Type)
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
            if (item.GetComponent<SP_Shape>().Type == brickPrefab.name)
            {
                count--;
            }
        }
        return count;
    }

    private void ReduceMoves()
    {
        //Warning Caustion Dangerous Area
        //UserMoves Calculate
        if (Application.loadedLevelName == "mainGame")
        {
            if (st.get_compMoves() > 0)
            {
                st.set_compMoves(st.get_compMoves() - 1); // user moves counter set
                st.setCompMovesLabel(st.get_compMoves().ToString());
            }
            if (st.get_compMoves() <= 0)
            {
                movesOver = true;
                if (st.get_userMoves() <= 0)
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


    private IEnumerator FindMatchesAndCollapse(IEnumerable<GameObject> potentialMatches)
    {
        StopCheckForPotentialMatches();
        int i = 1;
        foreach (var item in potentialMatches)
        {
            if (i == 1)
                hitGo = item;
            if (i == 2)
                hitGo2 = item;

            i++;
        }

        SP_Shape hitGoCache = null;
        SP_Shape hitGoCache1 = null;


        bool addBomb = false;
        bool addElectro = false;
        bool addBonus = false;

        bool addBomb1 = false;
        bool addElectro1 = false;
        bool addBonus1 = false;

        bool bricks = false;
        bool bricks1 = false;

        bool electricMatch = false;
        IEnumerable<GameObject> totalMatches;
        MatchesInfo hitGomatchesInfo, hitGo2matchesInfo;


        //get the second item that was part of the swipe
        shapes.Swap(hitGo, hitGo2);

        //move the swapped ones
        hitGo.transform.positionTo(Constants.AnimationDuration, hitGo2.transform.position);
        hitGo2.transform.positionTo(Constants.AnimationDuration, hitGo.transform.position);
        yield return new WaitForSeconds(Constants.AnimationDuration);

        opponentCanAddBrick = false;


        #region matching boosters

        if (hitGo.GetComponent<SP_Shape>().Bonus == BonusType.DestroyWholeRowColumn
           && hitGo2.GetComponent<SP_Shape>().Bonus == BonusType.DestroyWholeRowColumn)
        {
            totalMatches = shapes.GetEntireColumn(hitGo2).Union(shapes.GetEntireRow(hitGo2)).Distinct();
            var rowExplosion = Instantiate(RowExplosion, new Vector2(RowExplosion.transform.position.x, hitGo.transform.position.y), Quaternion.identity) as GameObject;
            var colExplosion = Instantiate(ColumnExplosion, new Vector2(hitGo.transform.position.x, RowExplosion.transform.position.y), Quaternion.identity) as GameObject;
            Destroy(rowExplosion, Constants.AnimationDuration);
            Destroy(colExplosion, Constants.AnimationDuration);

            bc.AddToUser(1);

        }
        #endregion

        #region Matching bombs
        else if (hitGo.GetComponent<SP_Shape>().Bonus == BonusType.DestroyNineCandies
           && hitGo2.GetComponent<SP_Shape>().Bonus == BonusType.DestroyNineCandies)
        {
            totalMatches = shapes.GetNineCandiesAroundBomb(hitGo2).Union(shapes.GetNineCandiesAroundBomb(hitGo)).Distinct();
            var rowExplosion = Instantiate(BombExplosion, hitGo.transform.position, Quaternion.identity) as GameObject;
            var colExplosion = Instantiate(BombExplosion, hitGo2.transform.position, Quaternion.identity) as GameObject;
            Destroy(rowExplosion, Constants.AnimationDuration);
            Destroy(colExplosion, Constants.AnimationDuration);

            bc.AddToUser(2);
        }
        #endregion

        #region Matching electros

        else if (hitGo.GetComponent<SP_Shape>().Bonus == BonusType.DestroySameCandies
           && hitGo2.GetComponent<SP_Shape>().Bonus == BonusType.DestroySameCandies)
        {
            totalMatches = shapes.GetAllCandies();
            var charge = Instantiate(electricCharge, electricCharge.transform.position, Quaternion.identity) as GameObject;
            Destroy(charge, Constants.ElectricChargeDuration);

            bc.AddToUser(3);

        }
        #endregion

        #region Matching boosters with Bomb

        else if (hitGo.GetComponent<SP_Shape>().Bonus == BonusType.DestroyWholeRowColumn
           && hitGo2.GetComponent<SP_Shape>().Bonus == BonusType.DestroyNineCandies)
        {
            totalMatches = shapes.GetEntireColumn(hitGo).Union(shapes.GetNineCandiesAroundBomb(hitGo2)).Distinct();
            var bombExplosion = Instantiate(BombExplosion, BombExplosion.transform.position, Quaternion.identity) as GameObject;
            var colExplosion = Instantiate(ColumnExplosion, new Vector2(hitGo.transform.position.x, RowExplosion.transform.position.y), Quaternion.identity) as GameObject;
            Destroy(bombExplosion, Constants.AnimationDuration);
            Destroy(colExplosion, Constants.AnimationDuration);

            bc.AddToUser(2);
        }
        else if (hitGo.GetComponent<SP_Shape>().Bonus == BonusType.DestroyNineCandies
           && hitGo2.GetComponent<SP_Shape>().Bonus == BonusType.DestroyWholeRowColumn)
        {
            totalMatches = shapes.GetNineCandiesAroundBomb(hitGo).Union(shapes.GetEntireRow(hitGo2)).Distinct();
            var bombExplosion = Instantiate(BombExplosion, BombExplosion.transform.position, Quaternion.identity) as GameObject;
            var rowExplosion = Instantiate(ColumnExplosion, new Vector2(hitGo2.transform.position.x, RowExplosion.transform.position.y), Quaternion.identity) as GameObject;
            Destroy(bombExplosion, Constants.AnimationDuration);
            Destroy(rowExplosion, Constants.AnimationDuration);

            bc.AddToUser(2);
        }

        #endregion

        #region Matching Boosters with electro
        else if (hitGo.GetComponent<SP_Shape>().Bonus == BonusType.DestroyWholeRowColumn
          && hitGo2.GetComponent<SP_Shape>().Bonus == BonusType.DestroySameCandies)
        {
            Debug.Log("LIke yea");
            totalMatches = shapes.GetEntireColumn(hitGo).Union(shapes.FindMatchingCandiesInGrid(hitGo, hitGo2).MatchedCandy).Distinct();
            var electrExplosion = Instantiate(electricCharge, electricCharge.transform.position, Quaternion.identity) as GameObject;
            var colExplosion = Instantiate(ColumnExplosion, new Vector2(hitGo.transform.position.x, RowExplosion.transform.position.y), Quaternion.identity) as GameObject;
            Destroy(electrExplosion, Constants.ElectricChargeDuration);
            Destroy(colExplosion, Constants.AnimationDuration);

            bc.AddToUser(2);
        }
        else if (hitGo.GetComponent<SP_Shape>().Bonus == BonusType.DestroySameCandies
           && hitGo2.GetComponent<SP_Shape>().Bonus == BonusType.DestroyWholeRowColumn)
        {
            Debug.Log("LIke yea 2");

            totalMatches = (shapes.FindMatchingCandiesInGrid(hitGo2, hitGo).MatchedCandy).Union(shapes.GetEntireRow(hitGo2)).Distinct();
            var electrExplosion = Instantiate(electricCharge, electricCharge.transform.position, Quaternion.identity) as GameObject;
            var rowExplosion = Instantiate(ColumnExplosion, new Vector2(hitGo2.transform.position.x, RowExplosion.transform.position.y), Quaternion.identity) as GameObject;
            Destroy(electrExplosion, Constants.ElectricChargeDuration);
            Destroy(rowExplosion, Constants.AnimationDuration);

            bc.AddToUser(2);
        }
        #endregion

        #region Matching Bombs with electro
        else if (hitGo.GetComponent<SP_Shape>().Bonus == BonusType.DestroyNineCandies
          && hitGo2.GetComponent<SP_Shape>().Bonus == BonusType.DestroySameCandies)
        {
            totalMatches = shapes.GetNineCandiesAroundBomb(hitGo).Union(shapes.FindMatchingCandiesInGrid(hitGo, hitGo2).MatchedCandy).Distinct();
            var electrExplosion = Instantiate(electricCharge, electricCharge.transform.position, Quaternion.identity) as GameObject;
            var bombExplosion = Instantiate(BombExplosion, BombExplosion.transform.position, Quaternion.identity) as GameObject;
            Destroy(electrExplosion, Constants.ElectricChargeDuration);
            Destroy(bombExplosion, Constants.AnimationDuration);

            bc.AddToUser(2);
        }
        else if (hitGo.GetComponent<SP_Shape>().Bonus == BonusType.DestroySameCandies
           && hitGo2.GetComponent<SP_Shape>().Bonus == BonusType.DestroyNineCandies)
        {
            totalMatches = (shapes.FindMatchingCandiesInGrid(hitGo2, hitGo).MatchedCandy).Union(shapes.GetNineCandiesAroundBomb(hitGo2)).Distinct();
            var electrExplosion = Instantiate(electricCharge, electricCharge.transform.position, Quaternion.identity) as GameObject;
            var bombExplosion = Instantiate(BombExplosion, BombExplosion.transform.position, Quaternion.identity) as GameObject;
            Destroy(electrExplosion, Constants.ElectricChargeDuration);
            Destroy(bombExplosion, Constants.AnimationDuration);

            bc.AddToUser(2);
        }
        #endregion

        else
        {


            #region Bricks Check
            //if the object clicked to swap is a brick
            if (hitGo.GetComponent<SP_Shape>().Type == brickPrefab.name)
            {
                bricks = true;
            }


            //if the object bieng swapped with the clicked one is a brick
            if (hitGo2.GetComponent<SP_Shape>().Type == brickPrefab.name)
            {
                bricks1 = true;
            }
            #endregion

            if (hitGo.GetComponent<SP_Shape>().Type == ElectroPrefab.GetComponent<SP_Shape>().Type)
            {
                totalMatches = shapes.FindMatchingCandiesInGrid(hitGo2, hitGo).MatchedCandy;
                electricMatch = true;
                bc.AddToOpponent(3);
                var newExplosion = Instantiate(electricCharge, electricCharge.transform.position, Quaternion.identity) as GameObject;
                Destroy(newExplosion, Constants.ElectricChargeDuration);
            }
            else if (hitGo2.GetComponent<SP_Shape>().Type == ElectroPrefab.GetComponent<SP_Shape>().Type)
            {
                totalMatches = shapes.FindMatchingCandiesInGrid(hitGo, hitGo2).MatchedCandy;
                electricMatch = true;
                bc.AddToOpponent(3);
                var newExplosion = Instantiate(electricCharge, electricCharge.transform.position, Quaternion.identity) as GameObject;
                Destroy(newExplosion, Constants.ElectricChargeDuration);
            }

            else
            {
                //get the matches via the helper methods


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
                    ReduceMoves();

                    bool sameFive = false;
                    bool sameColor = false;

                    bool sameFive1 = false;
                    bool sameColor1 = false;


                    int sameFour = 0;
                    int sameFour1 = 0;


                    if (hitGomatchesInfo.MatchedCandy.Count() == 4 && !bricks)
                    {
                        sameFour = CheckFourCandiesSame(hitGomatchesInfo.MatchedCandy);
                    }

                    if (hitGo2matchesInfo.MatchedCandy.Count() == 4 && !bricks1)
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
                            hitGoCache = new SP_Shape();
                            //get the game object that was of the same type
                            //      var sameTypeGo = hitGomatchesInfo.MatchedCandy.Count() > 0 ? hitGo : hitGo2;
                            var sameTypeGo = hitGo;
                            var shape = sameTypeGo.GetComponent<SP_Shape>();
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
                            hitGoCache1 = new SP_Shape();
                            //get the game object that was of the same type
                            //      var sameTypeGo = hitGomatchesInfo.MatchedCandy.Count() > 0 ? hitGo : hitGo2;
                            var sameTypeGo = hitGo2;
                            var shape = sameTypeGo.GetComponent<SP_Shape>();
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
            /*    if (timesRun == 1)
                    prevScore = st.get_userScore();*/

            if (!(bricks || bricks1))
            {
                //increase score
                IncreaseScore(totalMatches.Count() * Constants.candyScore * timesRun);
            }


            soundManager.PlayCrincle();

            //remove the candies that were matched
            foreach (var item in totalMatches)
            {
                //if the candies are bricks replace them with random candies
                if (item.GetComponent<SP_Shape>().Type == brickPrefab.name)
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

                //check and instantiate Bonus if needed
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
            var columns = totalMatches.Select(go => go.GetComponent<SP_Shape>().Column).Distinct();

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
                    if (lastRow.ElementAt(a).GetComponent<SP_Shape>().Type == brickPrefab.name)
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
            totalMatches = shapes.GetMatches(alteredCandies);
            timesRun++;
            electricMatch = false;

        }

        opponentCanAddBrick = true;
        //    Debug.Log("Previous score: " + prevScore);
        if ((st.get_oppoScore() - prevScore) > attackScore && (st.get_oppoScore() - prevScore) < superAttackScore)
        {
            Debug.Log("Adding brick");
            anim.Attack();
            //    soundManager.PlayOppoAttackSound();
            StartCoroutine(waitFunction(Constants.AnimationDuration));
        }
        else if ((st.get_oppoScore() - prevScore) >= superAttackScore)
        {
            anim.SuperAttack();
            //    soundManager.PlayOppoSuperAttackSound();
            StartCoroutine(waitFunction(Constants.AnimationDuration));
        }
        prevScore = st.get_oppoScore();
        prevScore = st.get_oppoScore();
        //    Debug.Log("New score: " + st.get_oppoScore());
        state = GameState.None;
        if (!movesOver && st.getTimerCounter() > 0f)
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
    private void CreateBonus(SP_Shape hitGoCache)
    {
        GameObject Bonus = Instantiate(GetBonusFromType(hitGoCache.Type), BottomRight
            + new Vector2(hitGoCache.Column * CandySize.x,
                hitGoCache.Row * CandySize.y), Quaternion.identity)
            as GameObject;
        shapes[hitGoCache.Row, hitGoCache.Column] = Bonus;
        var BonusShape = Bonus.GetComponent<SP_Shape>();
        //will have the same type as the "normal" candy
        BonusShape.Assign(hitGoCache.Type, hitGoCache.Row, hitGoCache.Column);
        //add the proper Bonus type
        BonusShape.Bonus |= BonusType.DestroyWholeRowColumn;
        powerUpsCopy.Add(Bonus);

    }
    private void CreateBombBonus(SP_Shape hitGoCache)
    {
        GameObject Bonus = Instantiate(GetBombBonusFromType(hitGoCache.Type), BottomRight
            + new Vector2(hitGoCache.Column * CandySize.x,
                hitGoCache.Row * CandySize.y), Quaternion.identity)
            as GameObject;
        shapes[hitGoCache.Row, hitGoCache.Column] = Bonus;
        var BonusShape = Bonus.GetComponent<SP_Shape>();
        //will have the same type as the "normal" candy
        BonusShape.Assign(hitGoCache.Type, hitGoCache.Row, hitGoCache.Column);
        //add the proper Bonus type
        BonusShape.Bonus |= BonusType.DestroyNineCandies;

        powerUpsCopy.Add(Bonus);
    }
    private void CreateElectroPower(SP_Shape hitGoCache)
    {
        //   Debug.Log("electro power added ");
        GameObject Bonus = Instantiate(ElectroPrefab.gameObject, BottomRight + new Vector2(hitGoCache.Column * CandySize.x,
                hitGoCache.Row * CandySize.y), Quaternion.identity)
            as GameObject;

        shapes[hitGoCache.Row, hitGoCache.Column] = Bonus;
        var BonusShape = Bonus.GetComponent<SP_Shape>();
        //will have the same type as the "normal" candy
        BonusShape.Assign(ElectroPrefab.GetComponent<SP_Shape>().Type, hitGoCache.Row, hitGoCache.Column);
        //add the proper Bonus types
        BonusShape.Bonus |= BonusType.DestroyWholeRowColumn;
        powerUpsCopy.Add(Bonus);

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
                GameObject go;

                go = GetRandomCandy();


                GameObject newCandy = Instantiate(go, SpawnPositions[column], Quaternion.identity)
                    as GameObject;

                newCandy.GetComponent<SP_Shape>().Assign(go.GetComponent<SP_Shape>().Type, item.Row, item.Column);

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
            GameObject go;
            if (bc.getOppoBricks() > 0 && !lastWasBrick)
            {
                lastWasBrick = true;
                go = brickPrefab;
                bc.RemoveOppoBricks(1);
                bricksCount++;
            }
            else
            {
                lastWasBrick = false;
                go = GetRandomCandy();
            }

            GameObject newCandy = Instantiate(go, SpawnPositions[column], Quaternion.identity)
                as GameObject;

            newCandy.GetComponent<SP_Shape>().Assign(go.GetComponent<SP_Shape>().Type, item.Row, item.Column);

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
                new Vector2(item.GetComponent<SP_Shape>().Column * CandySize.x, item.GetComponent<SP_Shape>().Row * CandySize.y));
        }
    }


    void RemoveFromThePowerCopy(GameObject item)
    {
        //if there are even any powerups
        if (powerUpsCopy.Count >= 1)
        {
            //if the item type is bonus, otherwise no need to iterate the list
            if (item.GetComponent<SP_Shape>().Bonus != BonusType.None)
            {
                foreach (var powerupItem in powerUpsCopy)
                {
                    //           Debug.Log("Power up items name to compare" + powerupItem + " : " + item);
                    if (powerupItem.GetComponent<SP_Shape>().Type == item.GetComponent<SP_Shape>().Type)
                    {
                        //            Debug.Log("Power up removed from the copy");
                        powerUpsCopy.Remove(powerupItem);
                        break;
                    }
                }
            }
        }
    }




    void ReplaceBrickWithRandomCandy(GameObject item)
    {

        if (item.GetComponent<SP_Shape>().Type == brickPrefab.name)
        {
            var column = item.GetComponent<SP_Shape>().Column;
            var row = item.GetComponent<SP_Shape>().Row;


            /*  var hitGoCache = new Shape();
                var shape = item.GetComponent<SP_Shape>();
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
                while (shapes[row, column - 1].GetComponent<SP_Shape>()
                    .IsSameType(newCandy.GetComponent<SP_Shape>())
                    && shapes[row, column - 2].GetComponent<SP_Shape>().IsSameType(newCandy.GetComponent<SP_Shape>()))
                {
                    newCandy = GetRandomCandy();
                }
            }

            // & * *
            //check if the two forward ones are same in column
            if ((column < Constants.Columns - 2 && shapes[row, column + 1] != null && shapes[row, column + 2] != null))
            {

                //check if two previous horizontal are of the same type
                while (shapes[row, column + 1].GetComponent<SP_Shape>()
                    .IsSameType(newCandy.GetComponent<SP_Shape>())
                    && shapes[row, column + 2].GetComponent<SP_Shape>().IsSameType(newCandy.GetComponent<SP_Shape>()))
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
                while (shapes[row - 1, column].GetComponent<SP_Shape>()
                    .IsSameType(newCandy.GetComponent<SP_Shape>())
                    && shapes[row - 2, column].GetComponent<SP_Shape>().IsSameType(newCandy.GetComponent<SP_Shape>()))
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
                while (shapes[row + 1, column].GetComponent<SP_Shape>()
                    .IsSameType(newCandy.GetComponent<SP_Shape>())
                    && shapes[row + 2, column].GetComponent<SP_Shape>().IsSameType(newCandy.GetComponent<SP_Shape>()))
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
                while (shapes[row + 1, column].GetComponent<SP_Shape>()
                    .IsSameType(newCandy.GetComponent<SP_Shape>())
                    && shapes[row - 1, column].GetComponent<SP_Shape>().IsSameType(newCandy.GetComponent<SP_Shape>()))
                {
                    newCandy = GetRandomCandy();
                }
            }

            // * & *
            //check if the forward one and backward one are same in column
            if ((column >= 1 && column < Constants.Columns - 1 && shapes[row, column + 1] != null && shapes[row, column - 1] != null))
            {

                //check if two previous horizontal are of the same type
                while (shapes[row, column + 1].GetComponent<SP_Shape>()
                    .IsSameType(newCandy.GetComponent<SP_Shape>())
                    && shapes[row, column - 1].GetComponent<SP_Shape>().IsSameType(newCandy.GetComponent<SP_Shape>()))
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
        //       powerUpsUpdated = true;

        if (item.GetComponent<SP_Shape>().Bonus == BonusType.DestroyWholeRowColumn)
        {
            soundManager.PlayBoosterExplosionSound();
            var newExpl = Instantiate(rowColExplosion, pos, rot) as GameObject;
            Destroy(newExpl, animDuration);

            bc.AddToUser(1);

            if (item.GetComponent<SP_Shape>().rowBonus)
            {
                animDuration = Constants.AnimationDuration;
                pos = new Vector2(RowExplosion.transform.position.x, item.transform.position.y);
                explosion = RowExplosion;
            }
            else if (item.GetComponent<SP_Shape>().colBonus)
            {
                animDuration = Constants.AnimationDuration + 0.1f;
                pos = new Vector2(item.transform.position.x, ColumnExplosion.transform.position.y);
                explosion = ColumnExplosion;
            }
        }

        else if (item.GetComponent<SP_Shape>().Bonus == BonusType.DestroyNineCandies)
        {
            soundManager.PlayBombExplosionSound();
            explosion = BombExplosion;

            bc.AddToUser(2);
        }
        else if (item.GetComponent<SP_Shape>().Type == brickPrefab.name)
        {
            soundManager.PlayBricksSmashingSound();
            explosion = stoneBlast;
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
        Destroy(item);
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
            st.set_oppoScore(0);

        }
        ShowScore();
    }

    private void IncreaseScore(int amount)
    {
        st.set_oppoScore(st.get_oppoScore() + amount);
        // score += amount;
        ShowScore();
    }

    private void ShowScore()
    {
        newScoreText.text = st.get_oppoScore().ToString();
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
            if (item.GetComponent<SP_Shape>().Type.Contains(color))
            {
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
            if (item.GetComponent<SP_Shape>().Type.Contains(color))
            {
                //          Debug.Log(item.name + "Added to the copy of powerup");
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

    /// <summary>
    /// Finds potential matches
    /// </summary>
    /// <returns></returns>
    /// 

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
    private IEnumerator CheckPotentialMatches()
    {
        yield return new WaitForSeconds(Constants.WaitBeforePotentialMatchesCheck);
        opponentCanAddBrick = false;
        GameObject power = null;
        bool found = false;
        
        //if two powerups are together then match them first before any other candy
        for (int i = 0; i < powerUpsCopy.Count(); i++)
        {
            if (found)
                break;
            for (int j = i + 1; j < powerUpsCopy.Count(); j++)
            {
                var s1 = powerUpsCopy.ElementAt(i);
                var s2 = powerUpsCopy.ElementAt(j);
                //     Debug.Log("Row : " + s1.Row + "Column: " + s1.Column + "Matched with, Row: " + s2.Row + "Column: " + s2.Column);
                if (SP_Utilities.AreVerticalOrHorizontalNeighbors(s1.GetComponent<SP_Shape>(), s2.GetComponent<SP_Shape>()))
                {
                    List<GameObject> pot = new List<GameObject>();
                    pot.Add(shapes.getCandy(s1.GetComponent<SP_Shape>().Row, s1.GetComponent<SP_Shape>().Column));
                    pot.Add(shapes.getCandy(s2.GetComponent<SP_Shape>().Row, s2.GetComponent<SP_Shape>().Column));
                    powerUpsCopy.Remove(s1);
                    powerUpsCopy.Remove(s2);
                    StartCoroutine(FindMatchesAndCollapse(pot));
                    powerUpsUpdated = false;
                    found = true;
                    yield return new WaitForSeconds(Constants.WaitBeforePotentialMatchesCheck);
                    break;
                }
            }
        }
        //   }


        foreach (var item in powerUpsCopy)
        {
            if (item.GetComponent<SP_Shape>().Type == "ElectroPower")
            {
                power = item;
                break;
            }
        }

        if (power != null && !found)
        {
            var go = shapes.FindObjectByType("ElectroPower");
            if (go != null)
            {
                StartCoroutine(FindMatchesAndCollapse(go));
            }
            yield return new WaitForSeconds(Constants.WaitBeforePotentialMatchesCheck);
        }
        else
        {
            yield return new WaitForSeconds(Constants.WaitBeforePotentialMatchesCheck);

            potentialMatches = SP_Utilities.GetPotentialMatches(shapes);

            while (potentialMatches == null)
            {
                ShowShuffleAlert();
                yield return new WaitForSeconds(Constants.WaitForShuffle);
                HideShuffleAlert();
                InitializeCandyAndSpawnPositions(false);   //false for not resetting the score
                potentialMatches = SP_Utilities.GetPotentialMatches(shapes);

            }
            if (potentialMatches != null)
            {
                StartCoroutine(FindMatchesAndCollapse(potentialMatches));
                yield return new WaitForSeconds(Constants.WaitBeforePotentialMatchesCheck);

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
                if (item.GetComponent<SP_Shape>().Type.Contains(tokens[0].Trim()))
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
