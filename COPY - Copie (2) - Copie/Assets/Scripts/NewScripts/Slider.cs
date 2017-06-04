using UnityEngine;
using System.Collections;

public class Slider : MonoBehaviour {

	private int totalChildren = 0;
	bool allowed=true;
	Vector3 newPosition, startPosition, endPosition;
	int currentIndex = 0;
	private float X =0, Y = 0f, Z = 0f;  //Position

	// Use this for initialization
	void Start () {
		newPosition = this.transform.position;
		startPosition = this.transform.position;
		endPosition = this.transform.position;
		totalChildren = this.transform.childCount;
		Vector3 child1 = this.transform.GetChild (0).transform.position;
		Vector3 child2 = this.transform.GetChild (1).transform.position;
		X = child2.x - child1.x;

		var colliders = this.transform.GetComponentsInChildren<BoxCollider> ();

		foreach (var col in colliders)
			col.enabled = false;

		colliders[0].enabled = true;
		
	}

	public void nextItem(){
		if (allowed) {
			newPosition = this.transform.position;
			if (currentIndex < totalChildren - 1) {
				newPosition.x -= X;
				this.transform.positionTo (0.2f, newPosition);
				StartCoroutine (WaitForSlide(0.2f));
				this.transform.GetChild (currentIndex).transform.GetComponent<BoxCollider> ().enabled = false;
				currentIndex++;
				this.transform.GetChild (currentIndex).transform.GetComponent<BoxCollider> ().enabled = true;
			}
		}
	}

	public void previousItem(){
		if(allowed){
			if (currentIndex > 0) {
				newPosition.x += X;
				this.transform.positionTo (0.2f, newPosition);
				StartCoroutine (WaitForSlide(0.2f));
				this.transform.GetChild (currentIndex).transform.GetComponent<BoxCollider> ().enabled = false;
				currentIndex--;
				this.transform.GetChild (currentIndex).transform.GetComponent<BoxCollider> ().enabled = true;
			}
		}
	}

	IEnumerator WaitForSlide(float seconds){
		allowed = false;
		yield return new WaitForSeconds(seconds);
		allowed = true;
	}
}
