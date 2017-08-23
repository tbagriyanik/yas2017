using UnityEngine;
using System.Collections;

public class rollingSound : MonoBehaviour {
	
	public AudioClip[] crashSound;
	public float minHitSpeed = 5f;
	public float minRollSpeed = 3.5f;

	AudioSource audiom;

	void Start () {
		audiom = gameObject.GetComponent<AudioSource> ();
	}

	void OnCollisionEnter (Collision hit) {
		if (hit.relativeVelocity.magnitude >= minHitSpeed) {
			//kola kutusu sesi
			audiom.Stop ();
			audiom.PlayOneShot (crashSound [Random.Range (0, crashSound.Length)], 0.7F);
		}
	}
}

