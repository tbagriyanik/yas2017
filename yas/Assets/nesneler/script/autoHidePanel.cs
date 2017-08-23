using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class autoHidePanel : MonoBehaviour {

	public float stayingTime = 5.0f;
	public bool timerStop = false;

	void FixedUpdate () {
		if (timerStop == false) {
			stayingTime -= Time.deltaTime;
			if (stayingTime <= 0.0f) {
				timerEnded ();
			}	
		}
	}

	void timerEnded () {
		this.gameObject.SetActive (false);
		timerStop = true;
	}

}
