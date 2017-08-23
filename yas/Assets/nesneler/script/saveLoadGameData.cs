using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AC.LSky;
using UnityEngine.SceneManagement;

public class saveLoadGameData : MonoBehaviour {

	public GameObject[] Cars;
	public GameObject[] Children;
	public GameObject saatNesnesi;

	public float appleCount = 0;
	public float breadCount = 0;
	public float waterCount = 0;
	public float moneyCount = 0;

	public bool settingsSound = true;
	public bool settingsEffects = true;
	public bool settingsFPS = true;

	GameObject skyManager;
	GameObject player;

	gerekliNesneler gerekliNesnelerComponent;
	gameControl gameControlComponent;
	playerStatus playerStatusComponent;
	childrenWorkTime childrenWorkTimeComponent;

	void Awake () {
		gerekliNesnelerComponent = GetComponent<gerekliNesneler> ();
		gameControlComponent = GetComponent<gameControl> ();
		playerStatusComponent = GetComponent<playerStatus> ();
		childrenWorkTimeComponent = GetComponent<childrenWorkTime> ();
	}

	public void savePlayerCoordinate () {
		player = gerekliNesnelerComponent.player;
		if (gameControlComponent.crouchMode) {
			gameControlComponent.oyuncuKalk ();	
		}

		if (SceneManager.GetActiveScene ().name == "anaHarita") {
			PlayerPrefs.SetFloat ("x", player.transform.position.x);
			PlayerPrefs.SetFloat ("y", player.transform.position.y);
			PlayerPrefs.SetFloat ("z", player.transform.position.z);

			PlayerPrefs.SetFloat ("pxr", player.transform.rotation.x);
			PlayerPrefs.SetFloat ("pyr", player.transform.rotation.y);
			PlayerPrefs.SetFloat ("pzr", player.transform.rotation.z);

			saveOtherObjects ();
		} else
			saveOtherObjects ();
	}

	public void saveOtherObjects () {
		skyManager = gerekliNesnelerComponent.SkyManager;

		PlayerPrefs.SetFloat ("bread", breadCount);
		PlayerPrefs.SetFloat ("water", waterCount);
		PlayerPrefs.SetFloat ("money", moneyCount);
		PlayerPrefs.SetFloat ("apple", appleCount);

		PlayerPrefs.SetInt ("sound", settingsSound ? 1 : 0);
		PlayerPrefs.SetInt ("effects", settingsEffects ? 1 : 0);
		PlayerPrefs.SetInt ("fps", settingsFPS ? 1 : 0);

		PlayerPrefs.SetFloat ("playerCarriage", playerStatusComponent.playerCarriage);

		int itemNo = 0;
		foreach (var item in Cars) {
			itemNo++;
			PlayerPrefs.SetFloat ("xcar" + itemNo.ToString (), item.transform.position.x);
			PlayerPrefs.SetFloat ("ycar" + itemNo.ToString (), item.transform.position.y);
			PlayerPrefs.SetFloat ("zcar" + itemNo.ToString (), item.transform.position.z);
		}
		itemNo = 0;
		foreach (var item in Children) {
			itemNo++;
			PlayerPrefs.SetFloat ("xchildren" + itemNo.ToString (), item.transform.position.x);
			PlayerPrefs.SetFloat ("ychildren" + itemNo.ToString (), item.transform.position.y);
			PlayerPrefs.SetFloat ("zchildren" + itemNo.ToString (), item.transform.position.z);
			PlayerPrefs.SetInt ("followMode" + itemNo.ToString (), item.GetComponent<basicAI> ().followPlayerMode ? 1 : 0);
		}

		if (skyManager) {
			PlayerPrefs.SetFloat ("time", skyManager.GetComponent <LSkyTOD> ().timeline);	
		}
	}

	public void loadPlayerCoordinate () {		
		player = gerekliNesnelerComponent.player;
		if (PlayerPrefs.HasKey ("x")) {

			if (SceneManager.GetActiveScene ().name == "anaHarita") {
				player.transform.position = new Vector3 (PlayerPrefs.GetFloat ("x"),
					PlayerPrefs.GetFloat ("y") + 1, PlayerPrefs.GetFloat ("z"));


				// oyuncu ve arabada dönüş sorunu var
				//player.transform.rotation = new Quaternion (PlayerPrefs.GetFloat ("pxr"),
				//	PlayerPrefs.GetFloat ("pyr"), PlayerPrefs.GetFloat ("pzr"), 0);			
				//araba dönüşü problem!
				//car.transform.rotation = new Quaternion (PlayerPrefs.GetFloat ("xcr"),
				//	PlayerPrefs.GetFloat ("ycr"), PlayerPrefs.GetFloat ("zcr"), 0);	
				loadOtherObjects ();
			} else
				loadOtherObjects ();
			
		}
	}

	public void loadOtherObjects () {	
		skyManager = gerekliNesnelerComponent.SkyManager;	

		if (PlayerPrefs.HasKey ("x")) {
			
			breadCount = PlayerPrefs.GetFloat ("bread");
			waterCount = PlayerPrefs.GetFloat ("water");
			moneyCount = PlayerPrefs.GetFloat ("money");
			appleCount = PlayerPrefs.GetFloat ("apple");

			settingsSound = (PlayerPrefs.GetInt ("sound") == 1) ? true : false;
			settingsEffects = (PlayerPrefs.GetInt ("effects") == 1) ? true : false;
			settingsFPS = (PlayerPrefs.GetInt ("fps") == 1) ? true : false;

			playerStatusComponent.playerCarriage	= PlayerPrefs.GetFloat ("playerCarriage");

			int itemNo = 0;
			foreach (var item in Cars) {
				itemNo++;
				item.transform.position = new Vector3 (PlayerPrefs.GetFloat ("xcar" + itemNo.ToString ()),
					PlayerPrefs.GetFloat ("ycar" + itemNo.ToString ()), 
					PlayerPrefs.GetFloat ("zcar" + itemNo.ToString ()));				
			}
			itemNo = 0;
			foreach (var item in Children) {
				itemNo++;
				item.transform.position = new Vector3 (PlayerPrefs.GetFloat ("xchildren" + itemNo.ToString ()),
					PlayerPrefs.GetFloat ("ychildren" + itemNo.ToString ()), 
					PlayerPrefs.GetFloat ("zchildren" + itemNo.ToString ()));	

				item.GetComponent<basicAI> ().followPlayerMode = PlayerPrefs.GetInt ("followMode" + itemNo.ToString ()) == 1 ? true : false;
			}	
		
		}
		if (PlayerPrefs.HasKey ("time")) {
			float geciciFloat = PlayerPrefs.GetFloat ("time");
			if (skyManager) {
				skyManager.GetComponent <LSkyTOD> ().timeline = geciciFloat;	
			}
			saatNesnesi.GetComponent<Clock> ().hour = Mathf.FloorToInt (geciciFloat);
			saatNesnesi.GetComponent<Clock> ().minutes = (int)Mathf.Abs (
				(geciciFloat - Mathf.FloorToInt (geciciFloat)) * 60);			
		} else {
			if (skyManager) {
				skyManager.GetComponent <LSkyTOD> ().timeline = 12f;	
			}
			saatNesnesi.GetComponent<Clock> ().hour = 12;
			saatNesnesi.GetComponent<Clock> ().minutes = 0;			
		}
		if (skyManager && skyManager.GetComponent <LSkyTOD> ().timeline >= childrenWorkTimeComponent.startHour &&
		    skyManager.GetComponent <LSkyTOD> ().timeline <= childrenWorkTimeComponent.endHour) {
			if (childrenWorkTimeComponent) {
				childrenWorkTimeComponent.gunduz_AktifOlacaklar ();	
			}
		} else {
			if (childrenWorkTimeComponent) {
				childrenWorkTimeComponent.gece_AktifOlacaklar ();	
			}
		}
	}
}
