using UnityEngine;

public class Statistics{

    private int levelsUnlocked;
    private int[] levelScores;
    private int[] charactersUnlocked;
    private int[] backgroundsUnlocked;

    public Statistics()
    {
        levelScores = new int[40];
        charactersUnlocked = new int[6];
        backgroundsUnlocked = new int[5];

        charactersUnlocked[0] = 1;
        backgroundsUnlocked[0] = 1;
        for (int i = 1; i < 5; i++)
        {
       //     backgroundsUnlocked[i] = PlayerPrefs.GetInt("Background" + i.ToString(), 0);
            backgroundsUnlocked[i] = 1;
        }

        for (int i = 1; i < 6; i++)
        {
       //     charactersUnlocked[i] = PlayerPrefs.GetInt("Character" + i.ToString(), 0);
            charactersUnlocked[i] = 1;
        }

     //   levelsUnlocked =  PlayerPrefs.GetInt("LevelsUnlocked", 1);
        levelsUnlocked = 40;

        for(int i=0; i<40; i++)
        {
            levelScores[i] = PlayerPrefs.GetInt("ScoreLevel" + i.ToString(), 0);
        }

    }
    
    public void setLevelsUnlocked(int level)
    {
        levelsUnlocked = level;
        PlayerPrefs.SetInt("LevelsUnlocked", levelsUnlocked);
        PlayerPrefs.Save();
    }

    public int getLevelsUnlocked()
    {
        return levelsUnlocked;
    }
    
    public void setLevelScore(int level, int score)
    {
        level--;
        if (score > levelScores[level])
        {
            levelScores[level] = score;
        }
        PlayerPrefs.SetInt("ScoreLevel" + level.ToString(), score);
        PlayerPrefs.Save();
    }

    public int getLevelScore(int level)
    {
        level--;
        return levelScores[level];
    }

    public void unlockCharacter(int ID)
    {
        charactersUnlocked[ID] = 1;
        PlayerPrefs.SetInt("Character" + ID.ToString(), 1);
    }
    
    public int isCharacterUnlocked(int ID)
    {
        return charactersUnlocked[ID];
    }

    public void UnlockBackground(int ID)
    {
        backgroundsUnlocked[ID] = 1;
        PlayerPrefs.SetInt("Background" + ID.ToString(), 1);
    }

    public int isBackgroundUnlocked(int ID)
    {
        return backgroundsUnlocked[ID];
    }
}
