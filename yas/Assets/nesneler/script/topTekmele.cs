using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class topTekmele : MonoBehaviour {

	private float kickForce = 10;

	private void OnCollisionEnter (Collision other) {
		if (other.transform.CompareTag ("Player") ||
		    other.transform.CompareTag ("NPC") ||
		    other.transform.CompareTag ("car")) {
			
			Vector3 direction = (other.transform.position - transform.position).normalized;
			GetComponent<Rigidbody> ().AddForce (-direction * kickForce, ForceMode.Impulse);
			if (!GetComponent<AudioSource> ().isPlaying) {
				GetComponent<AudioSource> ().Play ();	
			}
		}
	}
}
