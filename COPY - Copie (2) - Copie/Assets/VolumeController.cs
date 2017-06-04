using UnityEngine;
using System.Collections;

public class VolumeController : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        AudioListener.volume = this.GetComponent<UIScrollBar>().value;
    }
}
