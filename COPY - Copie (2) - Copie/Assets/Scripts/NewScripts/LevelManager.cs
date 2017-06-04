using UnityEngine;
using System.Collections;

public class LevelManager : MonoBehaviour {

	private StatManager st;
	private GameManager gm;
    public bool isLocked = false;

	// Use this for initialization
	void Start () {
		st = (StatManager)FindObjectOfType (typeof(StatManager));
		gm = (GameManager)FindObjectOfType (typeof(GameManager));
	}

	public void MoveBaseLevel(int level){
        if (level <= st.sc.getLevelsUnlocked())
        {
            st.setCurrentLevel(level);
            st.setTotalMoves(GameObject.Find(level.ToString()).GetComponent<LevelManagerParameters>().moves);
            gm.levelToLoad = "mainGame";
            gm.loading_screen();
        }
	}
	public void TimeBaseLevel(int level){
        if (level <= st.sc.getLevelsUnlocked())
        {
            st.setCurrentLevel(level);
            Debug.Log("Current Level: " + st.getCurrentLevel());
            st.setTotalTimeInSeconds(GameObject.Find(level.ToString()).GetComponent<LevelManagerParameters>().time);
            gm.levelToLoad = "mainGameTimeBased";
            gm.loading_screen();
        }
	}
}
