﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// Custom class to accomodate useful stuff for our shapes array
/// </summary>
public class SP_ShapesArray
{
    private GameObject[,] shapes = new GameObject[Constants.Rows, Constants.Columns];

    /// <summary>
    /// Indexer
    /// </summary>
    /// <param name="row"></param>
    /// <param name="column"></param>
    /// <returns></returns>
    public GameObject this[int row, int column]
    {
        get
        {
            try
            {
                return shapes[row, column];
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        set
        {
            shapes[row, column] = value;
        }
    }

    /// <summary>
    /// Swaps the position of two items, also keeping a backup
    /// </summary>
    /// <param name="g1"></param>
    /// <param name="g2"></param>
    public void Swap(GameObject g1, GameObject g2)
    {
        //hold a backup in case no match is produced
        backupG1 = g1;
        backupG2 = g2;

        var g1Shape = g1.GetComponent<SP_Shape>();
        var g2Shape = g2.GetComponent<SP_Shape>();

        //get array indexes
        int g1Row = g1Shape.Row;
        int g1Column = g1Shape.Column;
        int g2Row = g2Shape.Row;
        int g2Column = g2Shape.Column;

        //swap them in the array
        var temp = shapes[g1Row, g1Column];
        shapes[g1Row, g1Column] = shapes[g2Row, g2Column];
        shapes[g2Row, g2Column] = temp;

        //swap their respective properties
        SP_Shape.SwapColumnRow(g1Shape, g2Shape);

    }


    public IEnumerable<GameObject> GetAllCandies()
    {
        List<GameObject> matches = new List<GameObject>();
        foreach (var item in shapes)
        {
            matches.Add(item);
        }
        return matches;
    }

    /// <summary>
    /// Undoes the swap
    /// </summary>
    public void UndoSwap()
    {
        if (backupG1 == null || backupG2 == null)
            throw new Exception("Backup is null");

        Swap(backupG1, backupG2);
    }

    private GameObject backupG1;
    private GameObject backupG2;




    /// <summary>
    /// Returns the matches found for a list of GameObjects
    /// MatchesInfo class is not used as this method is called on subsequent collapses/checks, 
    /// not the one inflicted by user's drag
    /// </summary>
    /// <param name="gos"></param>
    /// <returns></returns>
    public IEnumerable<GameObject> GetMatches(IEnumerable<GameObject> gos)
    {
        List<GameObject> matches = new List<GameObject>();
        foreach (var go in gos)
        {
            if (go != null)
            {
                if (go.GetComponent<SP_Shape>().Type != "stone_brick1")
                    matches.AddRange(GetMatches(go).MatchedCandy);
            }
        }
        return matches.Distinct();
    }

    public List<GameObject> GetLastRow()
    {
        List<GameObject> matches = new List<GameObject>();
        for (int a = 0; a < Constants.Columns; a++)
        {
            matches.Add(shapes[0, a]);
        }
        return matches;
    }

    /// <summary>
    /// Finds the power up with the given type
    /// </summary>
    /// <param name="typeToFind"></param>
    /// <returns>
    /// first matching candy in grid of the type provided
    /// </returns>
    public List<GameObject> FindObjectByType(String typeToFind)
    {
        List<GameObject> matches = new List<GameObject>();
        foreach (var item in shapes)
        {
            if (item.GetComponent<SP_Shape>().Type == typeToFind)
            {
                matches.Add(item);
                var col = item.GetComponent<SP_Shape>().Column;
                var row = item.GetComponent<SP_Shape>().Row;
                if(row < Constants.Rows-1)
                {
                    matches.Add(shapes[row + 1, col]);
                }
                else if(col < Constants.Columns-1)
                {
                    matches.Add(shapes[row, col+1]);
                }

                break;
            }
        }
   //     Debug.Log("Electric power matches "+matches.Count());
        return matches;
    }

    public GameObject getCandy(int row, int col)
    {
        return shapes[row, col];
    }

    /// <summary>
    /// Returns the matches found for a single GameObject
    /// </summary>
    /// <param name="go"></param>
    /// <returns></returns>
    public MatchesInfo GetMatches(GameObject go)
    {
        MatchesInfo matchesInfo = new MatchesInfo();

       
        var horizontalMatches = GetMatchesHorizontally(go);

        GameObject match4Power = ContainsDestroyRowColumnBonus(horizontalMatches);
        if (match4Power != null)
        {
            horizontalMatches = GetEntireRow(go);
            if (!BonusTypeUtilities.ContainsDestroyWholeRowColumn(matchesInfo.BonusesContained))
                matchesInfo.BonusesContained |= BonusType.DestroyWholeRowColumn;

            match4Power.GetComponent<SP_Shape>().rowBonus = true;
            match4Power = null;
        }

        GameObject bombCandy = ContainsDestroyNineCandiesBonus(horizontalMatches);

        if (bombCandy != null)
        {
            var nineCandiesList = GetNineCandiesAroundBomb(bombCandy);
            matchesInfo.AddObjectRange(nineCandiesList);
            bombCandy = null;
        }
        matchesInfo.AddObjectRange(horizontalMatches);

        var verticalMatches = GetMatchesVertically(go);

        match4Power = ContainsDestroyRowColumnBonus(verticalMatches);
        if (match4Power != null)
        {
            verticalMatches = GetEntireColumn(go);
            if (!BonusTypeUtilities.ContainsDestroyWholeRowColumn(matchesInfo.BonusesContained))
                matchesInfo.BonusesContained |= BonusType.DestroyWholeRowColumn;

            match4Power.GetComponent<SP_Shape>().colBonus = true;
            match4Power = null;
        }

        bombCandy = ContainsDestroyNineCandiesBonus(verticalMatches);
        if (bombCandy != null)
        {
            var nineCandiesList = GetNineCandiesAroundBomb(bombCandy);
            matchesInfo.AddObjectRange(nineCandiesList);
            bombCandy = null;
        }
        matchesInfo.AddObjectRange(verticalMatches);
        var bricksAround = nieghbouringBricks(matchesInfo.MatchedCandy);

        matchesInfo.AddObjectRange(bricksAround);
        return matchesInfo;
    }

    public IEnumerable<GameObject> nieghbouringBricks(IEnumerable<GameObject> MatchesTocheck)
    {
        List<GameObject> matches = new List<GameObject>();
        foreach (var go in MatchesTocheck)
        {
            if (go.GetComponent<SP_Shape>().Type != "stone_brick1")
            {
                var niegbours = getNieghbours(go);
                foreach (var item in niegbours.MatchedCandy)
                {
                    //  Debug.Log("Checking for bricks");
                    if (item.GetComponent<SP_Shape>().Type == "stone_brick1")
                    {
                        //        Debug.Log("Brick in nieghbour");
                        matches.Add(item);
                    }
                }
            }
        }
        return matches.Distinct();
    }

    public MatchesInfo getNieghbours(GameObject go)
    {
        MatchesInfo matchesInfo = new MatchesInfo();

        //if the item is not at the last column, add the niegbour in next column
        if (go.GetComponent<SP_Shape>().Column < Constants.Columns - 1)
        {
            matchesInfo.AddObject(shapes[go.GetComponent<SP_Shape>().Row, go.GetComponent<SP_Shape>().Column + 1]);
        }

        //if the item is not at the column 0, add the nieghbour in the previous column
        if (go.GetComponent<SP_Shape>().Column > 0)
        {
            matchesInfo.AddObject(shapes[go.GetComponent<SP_Shape>().Row, go.GetComponent<SP_Shape>().Column - 1]);
        }

        //if the item is not at the top row, add the item in the next top row
        if (go.GetComponent<SP_Shape>().Row < Constants.Rows - 1)
        {
            matchesInfo.AddObject(shapes[go.GetComponent<SP_Shape>().Row + 1, go.GetComponent<SP_Shape>().Column]);
        }

        //if the item is not at the first row (row=0), add the item in previous row
        if (go.GetComponent<SP_Shape>().Row > 0)
        {
            matchesInfo.AddObject(shapes[go.GetComponent<SP_Shape>().Row - 1, go.GetComponent<SP_Shape>().Column]);
        }

        return matchesInfo;

    }
    public MatchesInfo FindMatchingCandiesInGrid(GameObject go, GameObject power)
    {
        MatchesInfo matchesInfo = new MatchesInfo();
        matchesInfo.AddObject(power);
        foreach (var item in shapes)
        {
            if (item.GetComponent<SP_Shape>().Type == go.GetComponent<SP_Shape>().Type
                && go.GetComponent<SP_Shape>().Bonus == BonusType.None)
            {
                matchesInfo.AddObject(item);
            }
        }
        return matchesInfo;
    }
    private GameObject ContainsDestroyNineCandiesBonus(IEnumerable<GameObject> matches)
    {
        if (matches.Count() >= Constants.MinimumMatches)
        {
            foreach (var go in matches)
            {
                if (BonusTypeUtilities.ContainsDestroyNineCandies
                    (go.GetComponent<SP_Shape>().Bonus))
                    return go;
            }
        }

        return null;
    }

    private GameObject ContainsDestroyRowColumnBonus(IEnumerable<GameObject> matches)
    {
        if (matches.Count() >= Constants.MinimumMatches)
        {
            foreach (var go in matches)
            {
                if (BonusTypeUtilities.ContainsDestroyWholeRowColumn
                    (go.GetComponent<SP_Shape>().Bonus))
                {
                    return go;
                }
            }
        }

        return null;
    }

    public IEnumerable<GameObject> GetNineCandiesAroundBomb(GameObject go)
    {
        List<GameObject> matches = new List<GameObject>();
        int row = go.GetComponent<SP_Shape>().Row;
        int col = go.GetComponent<SP_Shape>().Column;
        /*
                * * *
                * & *
                * * *
        */
        int rowStart = row - 1, rowEnd = row + 1, colStart = col - 1, colEnd = col + 1;
        if (row - 1 < 0)
            rowStart = row;
        if (row + 1 >= Constants.Rows)
            rowEnd = row;
        if (col - 1 < 0)
            colStart = col;
        if (col + 1 >= Constants.Columns)
            colEnd = col;

        for (int r = rowStart; r <= rowEnd; r++)
        {
            for (int c = colStart; c <= colEnd; c++)
                matches.Add(shapes[r, c]);
        }
        /*
                matches.Add(shapes[row+1, col-1]);
                matches.Add(shapes[row+1, col]);
                matches.Add(shapes[row+1, col+1]);
                matches.Add(shapes[row, col-1]);
                matches.Add(shapes[row, col]);
                matches.Add(shapes[row, col+1]);
                matches.Add(shapes[row-1, col-1]);
                matches.Add(shapes[row-1, col]);
                matches.Add(shapes[row-1, col+1]);*/

        return matches;
    }


    public IEnumerable<GameObject> GetEntireRow(GameObject go)
    {
        List<GameObject> matches = new List<GameObject>();
        int row = go.GetComponent<SP_Shape>().Row;
        for (int column = 0; column < Constants.Columns; column++)
        {
            matches.Add(shapes[row, column]);
        }
        return matches;
    }

    public IEnumerable<GameObject> GetEntireColumn(GameObject go)
    {
        List<GameObject> matches = new List<GameObject>();
        int column = go.GetComponent<SP_Shape>().Column;
        for (int row = 0; row < Constants.Rows; row++)
        {
            matches.Add(shapes[row, column]);
        }
        return matches;
    }

    /// <summary>
    /// Searches horizontally for matches
    /// </summary>
    /// <param name="go"></param>
    /// <returns></returns>
    private IEnumerable<GameObject> GetMatchesHorizontally(GameObject go)
    {
        List<GameObject> matches = new List<GameObject>();
        matches.Add(go);
        var shape = go.GetComponent<SP_Shape>();
        //check left
        if (shape.Column != 0)
            for (int column = shape.Column - 1; column >= 0; column--)
            {
                if (shapes[shape.Row, column].GetComponent<SP_Shape>().IsSameType(shape))
                {
                    matches.Add(shapes[shape.Row, column]);
                }
                else
                    break;
            }

        //check right
        if (shape.Column != Constants.Columns - 1)
            for (int column = shape.Column + 1; column < Constants.Columns; column++)
            {
                if (shapes[shape.Row, column].GetComponent<SP_Shape>().IsSameType(shape))
                {
                    matches.Add(shapes[shape.Row, column]);
                }
                else
                    break;
            }

        //we want more than three matches
        if (matches.Count < Constants.MinimumMatches)
            matches.Clear();

        return matches.Distinct();
    }

    /// <summary>
    /// Searches vertically for matches
    /// </summary>
    /// <param name="go"></param>
    /// <returns></returns>
    private IEnumerable<GameObject> GetMatchesVertically(GameObject go)
    {
        List<GameObject> matches = new List<GameObject>();
        matches.Add(go);
        var shape = go.GetComponent<SP_Shape>();
        //check bottom
        if (shape.Row != 0)
            for (int row = shape.Row - 1; row >= 0; row--)
            {
                if (shapes[row, shape.Column] != null &&
                    shapes[row, shape.Column].GetComponent<SP_Shape>().IsSameType(shape))
                {
                    matches.Add(shapes[row, shape.Column]);
                }
                else
                    break;
            }

        //check top
        if (shape.Row != Constants.Rows - 1)
            for (int row = shape.Row + 1; row < Constants.Rows; row++)
            {
                if (shapes[row, shape.Column] != null &&
                    shapes[row, shape.Column].GetComponent<SP_Shape>().IsSameType(shape))
                {
                    matches.Add(shapes[row, shape.Column]);
                }
                else
                    break;
            }


        if (matches.Count < Constants.MinimumMatches)
            matches.Clear();

        return matches.Distinct();
    }

    /// <summary>
    /// Removes (sets as null) an item from the array
    /// </summary>
    /// <param name="item"></param>
    public void Remove(GameObject item)
    {
        shapes[item.GetComponent<SP_Shape>().Row, item.GetComponent<SP_Shape>().Column] = null;
    }

    /// <summary>
    /// Collapses the array on the specific columns, after checking for empty items on them
    /// </summary>
    /// <param name="columns"></param>
    /// <returns>Info about the GameObjects that were moved</returns>
    public AlteredCandyInfo Collapse(IEnumerable<int> columns)
    {
        AlteredCandyInfo collapseInfo = new AlteredCandyInfo();


        ///search in every column
        foreach (var column in columns)
        {
            //begin from bottom row
            for (int row = 0; row < Constants.Rows - 1; row++)
            {
                //if you find a null item
                if (shapes[row, column] == null)
                {
                    //start searching for the first non-null
                    for (int row2 = row + 1; row2 < Constants.Rows; row2++)
                    {
                        //if you find one, bring it down (i.e. replace it with the null you found)
                        if (shapes[row2, column] != null)
                        {
                            shapes[row, column] = shapes[row2, column];
                            shapes[row2, column] = null;

                            //calculate the biggest distance
                            if (row2 - row > collapseInfo.MaxDistance)
                                collapseInfo.MaxDistance = row2 - row;

                            //assign new row and column (name does not change)
                            shapes[row, column].GetComponent<SP_Shape>().Row = row;
                            shapes[row, column].GetComponent<SP_Shape>().Column = column;

                            collapseInfo.AddCandy(shapes[row, column]);
                            break;
                        }
                    }
                }
            }
        }

        return collapseInfo;
    }


    public AlteredCandyInfo CollapseForBrick(int column)
    {
        AlteredCandyInfo collapseInfo = new AlteredCandyInfo();

        //begin from bottom row
        for (int row = 0; row < Constants.Rows - 1; row++)
        {
            //if you find a null item
            if (shapes[row, column] == null)
            {
                //start searching for the first non-null
                for (int row2 = row + 1; row2 < Constants.Rows; row2++)
                {
                    //if you find one, bring it down (i.e. replace it with the null you found)
                    if (shapes[row2, column] != null)
                    {
                        shapes[row, column] = shapes[row2, column];
                        shapes[row2, column] = null;

                        //calculate the biggest distance
                        if (row2 - row > collapseInfo.MaxDistance)
                            collapseInfo.MaxDistance = row2 - row;

                        //assign new row and column (name does not change)
                        shapes[row, column].GetComponent<SP_Shape>().Row = row;
                        shapes[row, column].GetComponent<SP_Shape>().Column = column;

                        collapseInfo.AddCandy(shapes[row, column]);
                        break;
                    }
                }
            }
        }


        return collapseInfo;
    }



    /// <summary>
    /// Searches the specific column and returns info about null items
    /// </summary>
    /// <param name="column"></param>
    /// <returns></returns>
    public IEnumerable<ShapeInfo> GetEmptyItemsOnColumn(int column)
    {
        List<ShapeInfo> emptyItems = new List<ShapeInfo>();
        for (int row = 0; row < Constants.Rows; row++)
        {
            if (shapes[row, column] == null)
                emptyItems.Add(new ShapeInfo() { Row = row, Column = column });
        }
        return emptyItems;
    }
}
