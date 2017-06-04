using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Bonus types
/// </summary>
[Flags]
public enum BonusType
{
    None,
    DestroyWholeRowColumn,
    DestroyNineCandies,
    DestroySameCandies,
	DestroyWholeCandies,
}


public static class BonusTypeUtilities
{
    /// <summary>
    /// Helper method to check for specific bonus type
    /// </summary>
    /// <param name="bt"></param>
    /// <returns></returns>
    public static bool ContainsDestroyWholeRowColumn(BonusType bt)
    {
        return (bt & BonusType.DestroyWholeRowColumn) 
            == BonusType.DestroyWholeRowColumn;
    }

	public static bool ContainsDestroyWholeCandies(BonusType bt)
	{
		return (bt & BonusType.DestroyWholeRowColumn) 
			== BonusType.DestroyWholeCandies;
	}


    public static bool ContainsDestroyNineCandies(BonusType bt)
    {
        return (bt & BonusType.DestroyNineCandies)
            == BonusType.DestroyNineCandies;
    }
}

/// <summary>
/// Our simple game state
/// </summary>
public enum GameState
{
    None,
    SelectionStarted,
    Animating,
	LevelFinished
}
