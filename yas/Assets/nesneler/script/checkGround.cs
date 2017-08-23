using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class checkGround : MonoBehaviour {

	void OnTriggerEnter (Collider other) {		
		if (other.gameObject.name == "Terrain") {
			transform.parent.parent.GetComponent<myCarLights> ().grounded = false;
		} else
			transform.parent.parent.GetComponent<myCarLights> ().grounded = true;
	}
}
