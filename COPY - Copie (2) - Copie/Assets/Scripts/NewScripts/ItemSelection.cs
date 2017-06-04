using UnityEngine;
using System.Collections;

public class ItemSelection : MonoBehaviour {

	private UIGrid grid;
	private GameManager gm;

	// Use this for initialization
	void Start () {
		grid = (UIGrid)FindObjectOfType (typeof(UIGrid));
		gm = (GameManager)FindObjectOfType (typeof(GameManager));
	}

	public void OnCharacterClicked(int intParam, string stringParam){
		if(grid.name == "CharacterGrid"){
            if (intParam != -1)
            {
                gm.setCharacterSelectedInt(intParam);
                gm.setCharacterSelectedString(stringParam);
                gm.loading_screen();
            }
		}else if(grid.name == "BackgroundGrid"){
            if (intParam != -1)
            {
                gm.setBackgroundSelectedInt(intParam);
                gm.setBackgroundSelectedString(stringParam);
                Application.LoadLevel("Menu");
            }
		}
	}
}
