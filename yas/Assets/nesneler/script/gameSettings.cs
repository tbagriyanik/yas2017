using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;
using UnityEngine.SceneManagement;

public class gameSettings : MonoBehaviour {

	GameObject yansimaNesnesi;
	Camera kamera;
	GameObject fps;

	// Use this for initialization
	//	void Start () {
	//
	//	}

	void OnEnable () {
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	void OnDisable () {
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	private void OnSceneLoaded (Scene scene, LoadSceneMode mode) {
		fps = GetComponent<gerekliNesneler> ().fpsText;
		yansimaNesnesi = GetComponent<gerekliNesneler> ().yansimaNesnesi;
		kamera = GetComponent<gerekliNesneler> ().Kamera.GetComponent<Camera> ();

		if (PlayerPrefs.HasKey ("sound")) {
			//ses aç kapa
			AudioListener.pause = ((PlayerPrefs.GetInt ("sound") == 0) ? true : false);
		}
		if (PlayerPrefs.HasKey ("effects")) {
			//efektleri aç kapa
			kamera.GetComponent<PostProcessingBehaviour> ().enabled = ((PlayerPrefs.GetInt ("effects") == 1) ? true : false);
			yansimaNesnesi.SetActive ((PlayerPrefs.GetInt ("effects") == 1) ? true : false);
		}
		if (PlayerPrefs.HasKey ("fps")) {
			//fps aç kapa
			fps.SetActive ((PlayerPrefs.GetInt ("fps") == 1) ? true : false);
		}
	}

}
