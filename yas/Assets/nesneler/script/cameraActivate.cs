using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraActivate : MonoBehaviour {

	private bool aktifZaten = false;

	void OnEnable () {		
		if (!aktifZaten) {
			StartCoroutine (deactivate ());		
			StartCoroutine (FlashCamera (0.5f)); //take a snapshot every 1/2 second
		}
	}

	IEnumerator deactivate () {
		aktifZaten = true;
		yield return new WaitForSeconds (3);
		aktifZaten = false;
		this.gameObject.SetActive (false);
	}

	IEnumerator FlashCamera (float Delay) {
		while (true && aktifZaten) {
			GetComponent<Camera> ().enabled = true;
			yield return null; //Wait 1 frame
			GetComponent<Camera> ().enabled = false;
			yield return new WaitForSeconds (Delay); //wait delay
		}
	}
}
