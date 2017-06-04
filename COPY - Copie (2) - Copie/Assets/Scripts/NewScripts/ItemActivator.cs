using UnityEngine;
using System.Collections;

public class ItemActivator : MonoBehaviour {

	private GameManager gm;

	// Use this for initialization
	void Start () {
		gm = (GameManager)FindObjectOfType (typeof(GameManager));
        DeActivateAll();
		ChangeItem ();
	}

    public void DeActivateAll()
    {
        for (int i = 1; i < this.transform.childCount; i++)
        {
            ActivateOrDeactivate(i, false);
            // ActivateOrDeactivateForCharacter(i, false);
            DeactivateOpponentCharactersList(i);
        }
    }

	public void ChangeItem(){
		if (this.gameObject.name == "UserCharacter") {
			ActivateOrDeactivate (gm.getCharacterSelectedInt () + 1, true); //+1 due to an extra gameobhject in the usercharacter grid
		} else if (this.gameObject.name == "GameBackground") {
			ActivateOrDeactivate (gm.getBackgroundSelectedInt (), true);
		} else if (this.gameObject.name == "OpponentCharacters") {
            ActivateOrDeactivateForCharacter(gm.getCharacterSelectedInt() + 1, true);			
		}
	}

	private void ActivateOrDeactivate(int index, bool state){
		this.transform.GetChild (index).gameObject.SetActive (state);
	}

	public void ActivateOrDeactivateBoth(int userIndex, int oppoIndex, bool swap)
	{
      /*  DeActivateAll();
           Debug.Log("Activation function called");
            if (swap)
            {
            DeActivateAll();*/
           // Debug.Log("Swap and name changed");
            //    GameObject.Find("OpponentCharacters").transform.GetChild(userIndex).gameObject.SetActive(true);
              //  GameObject.Find("UserCharacter").transform.GetChild(oppoIndex).gameObject.SetActive(true);

           //    GameObject userTemp = GameObject.Find("UserCharacter");
           //    GameObject.Find("OpponentCharacters").name = "UserCharacter";
           //    userTemp.name = "OpponentCharacters";
           // }
           // else
           // {
           // DeActivateAll();
           // Debug.Log("activating normally");
               GameObject.Find("UserCharacter").transform.GetChild(userIndex).gameObject.SetActive(true);
               GameObject.Find("OpponentCharacters").transform.GetChild(oppoIndex).gameObject.SetActive(true);
            
	}

	private void ActivateOrDeactivateForCharacter(int index, bool state){
		int temp ;
		if (index == 1 || index == 6) {
			temp = Random.Range (2, 5);
			this.transform.GetChild (temp).gameObject.SetActive (state);
            gm.OppoCharacterID = temp;

		} else if (index == 2 || index == 3 || index == 4 || index == 5) {
            temp = Random.Range(1,2);
            if (temp == 2)
            {
                temp = 6;
            }
            this.transform.GetChild (temp).gameObject.SetActive (state);
            gm.OppoCharacterID = temp;
         
        }
	}

    private void DeactivateOpponentCharactersList(int index)
    {
        this.transform.GetChild(index).gameObject.SetActive(false);
    }
}
