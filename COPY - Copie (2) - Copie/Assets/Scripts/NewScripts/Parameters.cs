using UnityEngine;
using System.Collections;

public class Parameters : MonoBehaviour {

	public int paramInt;
	public string paramString;

    private StatManager st;

    void Start()
    {
        st = (StatManager)FindObjectOfType(typeof(StatManager));
        //if its not the first character
        if(paramInt !=0)
        {

            if (this.transform.parent.name == "BackgroundGrid")
            {
                if (st.sc.isBackgroundUnlocked(paramInt) == 0)
                {
                    this.transform.GetChild(1).gameObject.SetActive(true);
                    paramInt = -1;
                }
            }
            //if this character is unlocked
            else if (this.transform.parent.name == "CharacterGrid")
            {
                if (st.sc.isCharacterUnlocked(paramInt) == 0)
                {
                    this.transform.GetChild(1).gameObject.SetActive(true);
                    paramInt = -1;
                }
            }
        }
    }
}
