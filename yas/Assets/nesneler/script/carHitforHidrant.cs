using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class carHitforHidrant : MonoBehaviour {

	public GameObject fiskiye;
	public GameObject suBirikintisi;

	void OnTriggerEnter (Collider other) {
		if (other.gameObject.CompareTag ("car") ||
		    other.gameObject.CompareTag ("rock")) {
			fiskiye.SetActive (true);
			suBirikintisi.SetActive (true);
			GetComponent<AudioSource> ().enabled = true;
			GetComponent<carHitforHidrant> ().enabled = false;
			suBirikintisi.SetActive (true);
		}
	}

}
