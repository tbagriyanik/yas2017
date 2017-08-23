using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class throwStone : MonoBehaviour {

	public GameObject[] stones;
	public AudioClip[] throwSounds;
	public int throwSpeed = 20;
	//atarken ve tekmelerkenki hız
	float throwStartTime;
	// atış için geçen zaman
	private float fireSpellStart = 0f;
	public float fireSpellCooldown = 1f;

	GameObject throwPoint;
	GameObject player;

	void Awake () {		
		throwPoint = GetComponent<gerekliNesneler> ().itemDropPoint;
		player = GetComponent<gerekliNesneler> ().player;
	}

	void throwIt (int thSpeed) {
		Transform newObject;
		newObject = Instantiate (stones [0].transform, throwPoint.transform.position, throwPoint.transform.rotation);
		newObject.GetComponent<Rigidbody> ().AddForceAtPosition (Camera.main.transform.forward * thSpeed, 
			throwPoint.transform.position, 
			ForceMode.Impulse);	

		fireSpellStart = Time.time;
	}

	// Update is called once per frame
	void Update () {

		if (GetComponent<dialogPanelControl> ().dialogPanelMode) {
			return;
		}

		if (Time.time > fireSpellStart + fireSpellCooldown) {				
			if (Input.GetKeyDown (KeyCode.Q)) {
				throwStartTime = Time.time;
			}
			if (Input.GetKeyUp (KeyCode.Q)) {
			
				float gecenZaman = (Time.time - throwStartTime);
				if (gecenZaman < 0.2f)
					throwSpeed = 20;
				else
					throwSpeed = (int)(((int)(gecenZaman * 1000)) / 20) + 20;
				if (throwSpeed > 120)
					throwSpeed = 120;

				player.GetComponent<AudioSource> ().PlayOneShot (throwSounds [(int)Random.Range (0, 2)]); // swing

				throwIt (throwSpeed);
			}
		}
	}
}
