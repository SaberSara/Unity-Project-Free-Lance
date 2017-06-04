using UnityEngine;
using System.Collections;

public class loadLevel : MonoBehaviour {

//	private MultiplayerNetworkConnection mnc;
//	private MP_ShapesManager temp;
	public AsyncOperation levelLoadProgress;

	// Use this for initialization
	void Start () {
	//	mnc = (MultiplayerNetworkConnection)FindObjectOfType (typeof(MultiplayerNetworkConnection));

	/*	if (Application.loadedLevelName == "MultiplayerMoveBased" || Application.loadedLevelName == "MultiplayerTimeBased") {
			Application.LoadLevelAdditive ("MultiplayerScene");
			levelLoadProgress =  Application.LoadLevelAdditiveAsync ("MultiplayerScene");
		}else*/ if(Application.loadedLevelName == "mainGame" || Application.loadedLevelName == "mainGameTimeBased"){
			Application.LoadLevelAdditive ("SinglePlayerScene");
		}
	}

	void Update(){
		if(Application.loadedLevelName == "MultiplayerMoveBased" || Application.loadedLevelName == "MultiplayerTimeBased"){
			if (levelLoadProgress.progress == 1f) {
	//			mnc.InitializeVariables ();
			}
		}
	}
}
