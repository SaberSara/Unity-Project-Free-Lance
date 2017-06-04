using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class SP_Shape : MonoBehaviour
{
    public BonusType Bonus { get; set; }
    public int Column { get; set; }
    public int Row { get; set; }

    public bool rowBonus = false;
    public bool colBonus = false;
    public string Type { get; set; }

    public SP_Shape()
    {
        Bonus = BonusType.None;
    }

    /// <summary>
    /// Checks if the current shape is of the same type as the parameter
    /// </summary>
    /// <param name="otherShape"></param>
    /// <returns></returns>
    public bool IsSameType(SP_Shape otherShape)
    {
        if (otherShape == null || !(otherShape is SP_Shape))
            throw new ArgumentException("otherShape");

        return string.Compare(this.Type, (otherShape as SP_Shape).Type) == 0;
    }

    /// <summary>
    /// Constructor alternative
    /// </summary>
    /// <param name="type"></param>
    /// <param name="row"></param>
    /// <param name="column"></param>
    public void Assign(string type, int row, int column)
    {

        if (string.IsNullOrEmpty(type))
            throw new ArgumentException("type");

        Column = column;
        Row = row;
        Type = type;
    }

    /// <summary>
    /// Swaps properties of the two shapes
    /// We could do a shallow copy/exchange here, but anyway...
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    public static void SwapColumnRow(SP_Shape a, SP_Shape b)
    {
        int temp = a.Row;
        a.Row = b.Row;
        b.Row = temp;

        temp = a.Column;
        a.Column = b.Column;
        b.Column = temp;
    }
}



