using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class gameMenuCode : MonoBehaviour {

	public GameObject settingsPanelObject;
	public Toggle soundToggle;
	public Toggle effectsToggle;
	public Toggle fpsToggle;

	// Use this for initialization
	void Start () {
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
		settingsPanelObject.SetActive (false);

	}

	void OnEnable () {
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	void OnDisable () {
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	private void OnSceneLoaded (Scene scene, LoadSceneMode mode) {
		if (PlayerPrefs.HasKey ("sound")) {
			soundToggle.isOn = (PlayerPrefs.GetInt ("sound") == 1) ? true : false;
		}
		if (PlayerPrefs.HasKey ("effects")) {
			effectsToggle.isOn = (PlayerPrefs.GetInt ("effects") == 1) ? true : false;
		}
		if (PlayerPrefs.HasKey ("fps")) {
			fpsToggle.isOn = (PlayerPrefs.GetInt ("fps") == 1) ? true : false;
		}
	}

	void Update () {
		if (Input.GetKeyDown (KeyCode.Escape)) {
			if (settingsPanelObject.activeSelf)
				settingsPanelObject.SetActive (false);
		}
	}

	public void resetGameData () {
		soundToggle.isOn = true;
		fpsToggle.isOn = true;
		effectsToggle.isOn = true;
		PlayerPrefs.DeleteAll ();
	}

	public void startGameLoadLevel1 () {
		SceneManager.LoadScene (1);
	}

	public void quitGame () {
		if (Application.platform == RuntimePlatform.WebGLPlayer ||
		    Application.platform == RuntimePlatform.OSXPlayer ||
		    Application.platform == RuntimePlatform.WindowsPlayer ||
		    Application.platform == RuntimePlatform.LinuxPlayer)
			Application.OpenURL ("http://www.facebook.com/blenderWorks");
		//OpenURL not working in FB
		//ExternalEval ("window.open('http://www.facebook.com/blenderWorks')"); açılır pencere ...

		#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;		
		#else
		Application.Quit ();
		#endif
	}

	public void showSettings () {
		settingsPanelObject.SetActive (true);
	}

	public void closeSettings () {
		settingsPanelObject.SetActive (false);
	}

	public void soundSetting () {
		if (soundToggle.isOn) {
			PlayerPrefs.SetInt ("sound", 1);
		} else {
			PlayerPrefs.SetInt ("sound", 0);
		}
	}

	public void effectsSetting () {
		if (effectsToggle.isOn) {
			PlayerPrefs.SetInt ("effects", 1);
		} else {
			PlayerPrefs.SetInt ("effects", 0);
		}
	}

	public void fpsSetting () {
		if (fpsToggle.isOn) {
			PlayerPrefs.SetInt ("fps", 1);
		} else {
			PlayerPrefs.SetInt ("fps", 0);
		}
	}
}
