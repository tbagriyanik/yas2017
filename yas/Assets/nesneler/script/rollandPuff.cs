using UnityEngine;
using System.Collections;

public class rollandPuff : MonoBehaviour {
	
	public AudioClip[] crashSounds;
	//yere vurduğunda rasgele ses
	public AudioClip[] hitSounds;
	//et 0, metal 1

	public float minHitSpeed = 5f;
	public float minRollSpeed = 3.5f;
	
	public AudioSource audiom;
	public GameObject[] particles;
	//toz 0, kıvılcım 1

	void Start () {
		audiom = gameObject.GetComponent<AudioSource> ();
	}

	void OnCollisionEnter (Collision hit) {
		if (hit.transform.CompareTag ("Player")) {
			return;
		}
		if (hit.relativeVelocity.magnitude >= minHitSpeed) {
			//taş sesi ve puflar
			audiom.Stop ();
			audiom.PlayOneShot (crashSounds [Random.Range (0, crashSounds.Length)], 0.7F);
			Instantiate (particles [0], transform.position, transform.rotation);		
		} 
		if (hit.transform.CompareTag ("marti")) {
			Instantiate (particles [0], transform.position, transform.rotation);
			audiom.Stop ();
			audiom.PlayOneShot (hitSounds [0]);
			Destroy (hit.transform.gameObject);//kuş yok olur
			Destroy (this.transform.gameObject);//taş yok olur, boşuna zıplıyor
		} else if (hit.transform.CompareTag ("NPC")) {
			Instantiate (particles [0], transform.position, transform.rotation);
			audiom.Stop ();
			audiom.PlayOneShot (hitSounds [0]);
//		} else if (hit.gameObject.GetComponent<Renderer> () &&
//		           hit.gameObject.GetComponent<Renderer> ().materials.Length == 2) {			
//			Debug.Log (hit.gameObject.GetComponent<Renderer> ().materials [1].name.Contains ("CamGuzel"));
//			if (hit.gameObject.GetComponent<Renderer> ().materials [0].name.Contains ("CamGuzel") ||
//			    hit.gameObject.GetComponent<Renderer> ().materials [1].name.Contains ("CamGuzel")) {
//				Instantiate (particles [1], transform.position, transform.rotation);
//				audiom.Stop (); //neden sen olmadı anlamadım!
//				audiom.PlayOneShot (hitSounds [2]);
//			}
		} else if (hit.transform.CompareTag ("car") ||
		           hit.transform.name.Contains ("Hidrant") ||
		           hit.transform.name.Contains ("TraficLight") ||
		           hit.transform.name.Contains ("direk") ||
		           hit.transform.name.Contains ("FuseBox") ||
		           hit.transform.name.Contains ("SewerCap") ||
		           hit.transform.name.Contains ("StreetLight") ||
		           hit.transform.name.Contains ("Trashcan")) { 			
			Instantiate (particles [1], transform.position, transform.rotation);            
			audiom.Stop ();
			audiom.PlayOneShot (hitSounds [1]);
		}
	}
}
