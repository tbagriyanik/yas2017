using UnityEngine;
using System.Collections;

public class DestroyByContact : MonoBehaviour {
	public GameObject explosion;
	public GameObject spotLight;
	public GameObject lambaTexture;

	void OnTriggerEnter (Collider other) {
		if (other.tag == "rock") {
			if (lambaTexture) {
				lambaTexture.GetComponent<Renderer> ().materials [0].DisableKeyword ("_EMISSION");	
			}
			if (spotLight) {
				spotLight.SetActive (false);	
			}
			GetComponent<AudioSource> ().Play ();
			if (explosion) {
				Instantiate (explosion, transform.position, transform.rotation);	
			}
			Destroy (this.gameObject, 1);
		}
	}
}