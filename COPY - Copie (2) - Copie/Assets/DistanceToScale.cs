using UnityEngine;
using System.Collections;

public class DistanceToScale : MonoBehaviour {

	public float distance =10f;
	// Use this for initialization
	void Start () {
		StartCoroutine (ScaleMeter ());
	}
		
	IEnumerator ScaleMeter() {
		while (transform.localScale.y < distance) {
			yield return new WaitForSeconds (0.05f);
			transform.localScale = new Vector3 (transform.localScale.x,transform.localScale.y + 0.1f,transform.localScale.z);
		}
		Destroy (gameObject, 0.01f);
	}

	// Update is called once per frame
	void Update () {
	
	}
}
