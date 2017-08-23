using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AC.LSky;

public class childrenWorkTime : MonoBehaviour {

	public int startHour = 8;
	public int endHour = 18;

	public GameObject childrenObject;
	public GameObject terrainObject;
	public GameObject martilarObject;
	public GameObject kedilerObject;
	public GameObject[] geceYanacakLambalar;

	GameObject skyTimeObject;

	// Use this for initialization
	void Start () {
		skyTimeObject = GetComponent<gerekliNesneler> ().SkyManager;
	}

	public void gunduz_AktifOlacaklar () {
		if (!childrenObject) {
			return;
		}
		childrenObject.SetActive (true);
		AudioSource[] sesler = terrainObject.GetComponents <AudioSource> ();
		sesler [0].enabled = true;
		sesler [1].enabled = false;
		martilarObject.SetActive (true);
		kedilerObject.SetActive (true);
		foreach (var lamba in geceYanacakLambalar) {
			if (lamba) {
				lamba.SetActive (false);	
			}
		}
	}

	public void gece_AktifOlacaklar () {
		if (!childrenObject) {
			return;
		}
		childrenObject.SetActive (false);
		AudioSource[] sesler = terrainObject.GetComponents <AudioSource> ();
		sesler [1].enabled = true;
		sesler [0].enabled = false;
		martilarObject.SetActive (false);
		kedilerObject.SetActive (false);
		foreach (var lamba in geceYanacakLambalar) {
			if (lamba) {
				lamba.SetActive (true);	
			}
		}
	}

	// Update is called once per frame
	void LateUpdate () {
		//8 ile 20 arasında öğrenciler var olsun
		if (skyTimeObject.GetComponent<LSkyTOD> ().timeline >= startHour &&
		    skyTimeObject.GetComponent<LSkyTOD> ().timeline <= startHour + 0.01f) {
			gunduz_AktifOlacaklar ();
		} else if (skyTimeObject.GetComponent<LSkyTOD> ().timeline >= endHour &&
		           skyTimeObject.GetComponent<LSkyTOD> ().timeline <= endHour + 0.01f) {
			gece_AktifOlacaklar ();
		}
	}
}
