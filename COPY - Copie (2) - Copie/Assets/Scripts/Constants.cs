using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UI;


public static class Constants
{
    public static readonly int Rows = 9;
    public static readonly int Columns = 5;
    public static readonly float AnimationDuration = 0.2f;

    public static readonly float MoveAnimationMinDuration = 0.1f;

    public static readonly float ExplosionDuration = 0.3f;
    public static readonly float ElectricChargeDuration = 0.6f;

    public static readonly float WaitForShuffle = 2f;

    public static readonly float WaitBeforePotentialMatchesCheck = 1.5f;
    public static readonly float WaitBeforeHint = 2f;
    public static readonly float OpacityAnimationFrameDelay = 0.05f;

    public static readonly int MinimumMatches = 3;
    public static readonly int MinimumMatchesForBonus = 4;
    public static readonly int MatchCountForPower = 5;

    public static readonly int candyScore = 10;
    public static readonly int bombScore = 50;
    public static readonly int BoosterScore = 40;
    public static readonly int ElectroScore = 100;


    public static readonly int minAttackScore = 400;
    public static readonly int minSuperAttackScore = 800;

    public static readonly int minAttackScoreLevel2 = 150;
    public static readonly int minSuperAttackScoreLevel2 = 400;


    public static readonly Vector2 BottomRightPlayerOne = new Vector2(-20.8f, -8.6f);
    public static readonly Vector2 BottomRightPlayerTwo = new Vector2(0.4f, -8.6f);

    public static readonly Vector3 inputBlockerGrid1 = new Vector3(-15.5f, 0f, -8f);
    public static readonly Vector3 inputBlockerGrid2 = new Vector3(5.5f, 0f, -8f);

    public static readonly Vector2 CandySize = new Vector2(2.1f, 2.1f);
}



