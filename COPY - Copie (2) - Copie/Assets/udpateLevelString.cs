using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class udpateLevelString : MonoBehaviour {

	// Use this for initialization
	void Start () {
		GetComponent <Text>().text = "Level " + 
                                ((StatManager)(FindObjectOfType(typeof(StatManager)))).getCurrentLevel().ToString();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
