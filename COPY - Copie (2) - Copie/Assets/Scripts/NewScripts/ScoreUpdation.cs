using UnityEngine;
using System.Collections;

public class ScoreUpdation : MonoBehaviour {

	private StatManager st;

	// Use this for initialization
	void Start () {
		st = (StatManager)FindObjectOfType (typeof(StatManager));
		this.GetComponent<UILabel> ().text = st.get_userScore ().ToString();
        st.sc.setLevelScore(st.getCurrentLevel(), st.get_userScore());
    }
}
