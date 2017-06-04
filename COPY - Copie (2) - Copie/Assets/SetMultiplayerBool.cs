using UnityEngine;
using System.Collections;

public class SetMultiplayerBool : MonoBehaviour {

    private GameManager gm;

	// Use this for initialization
	public void SetMultiplayerSceneBoolTrue () {
        gm = (GameManager)FindObjectOfType(typeof(GameManager));
        gm.multiplayerSceneSelected = true;
	}
}
