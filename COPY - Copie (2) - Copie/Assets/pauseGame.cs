using UnityEngine;
using System.Collections;

public class pauseGame : MonoBehaviour {
	GameManager gm ;
	// Use this for initialization
	void Start () {
		gm = (GameManager)FindObjectOfType(typeof(GameManager));
	}
	public void Pause() {
		gm.pause_game ();
	}
	// Update is called once per frame
	void Update () {
	
	}
}
