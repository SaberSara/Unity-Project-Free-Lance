using UnityEngine;
using System.Collections;

public class EnableRoot : MonoBehaviour {

    public GameObject root;
	// Use this for initialization
	void Start () {
        Debug.Log("courotine start");
        StartCoroutine(ActivateRoot(2f));
	}
    
    IEnumerator ActivateRoot(float secs)
    {
        yield return new WaitForSeconds(secs);
        Debug.Log("courotine end");
        //root.SetActive(true);
        //Time.timeScale = 0;
		UnityEngine.SceneManagement.SceneManager.LoadScene("Maps");
        gameObject.SetActive(false);
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
