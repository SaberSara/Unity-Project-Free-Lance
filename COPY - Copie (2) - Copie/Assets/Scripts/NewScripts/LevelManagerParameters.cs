using UnityEngine;
using System.Collections;

public class LevelManagerParameters : MonoBehaviour {

	public int moves;
	public float time;
    public int candiesAllowed = 6;
    private StatManager st;
    private GameManager gm;
    private LevelManager lm;
    public int levelNumber;

	// Use this for initialization
	void Start () {
        st = (StatManager)FindObjectOfType(typeof(StatManager));
        gm = (GameManager)FindObjectOfType(typeof(GameManager));
        lm = (LevelManager)FindObjectOfType(typeof(LevelManager));
        levelNumber = int.Parse(this.gameObject.name);
        
        // Disable if not level is not unlocked
        if (levelNumber > st.sc.getLevelsUnlocked())
        {
            this.GetComponent<UIButton>().isEnabled = false;
        }

    }

    public void setCandies()
    {
        string MapName = this.transform.parent.parent.name;

        if (MapName == "Map5")
        {
            st.setCandiesAllowed(4);
            gm.setBackgroundSelectedInt(0);
        }
        else if (MapName == "Map4")
        {
            st.setCandiesAllowed(5);
            gm.setBackgroundSelectedInt(1);
        }
        else if (MapName == "Map3"){
            st.setCandiesAllowed(5);
            gm.setBackgroundSelectedInt(2);
        }
        else if (MapName == "Map2")
        {
            st.setCandiesAllowed(6);
            gm.setBackgroundSelectedInt(3);
        }
        else if (MapName == "Map1")
        {
            st.setCandiesAllowed(6);
            gm.setBackgroundSelectedInt(4);
        }
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
