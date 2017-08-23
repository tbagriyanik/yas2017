using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.PostProcessing;
using UnityStandardAssets.Vehicles.Car;
using UnityStandardAssets.Characters.FirstPerson;
using UnityStandardAssets.Utility;
using UnityEngine.AI;
using AC.LSky;
using TMPro;

public class gameControl : MonoBehaviour {

	public GameObject consolePanelObject;
	public InputField consoleInput;

	public TextMeshProUGUI consoleLogText;
	public Text tipText;
	//e için olan tipText aslında playerLookingAt dedir

	public Text warningText;
	public Text subtitleText;
	public Text speedText;
	public Scrollbar verticalScrollBar;
	public GameObject missionPanel;

	public GameObject fareImlec;
	public GameObject spawnParent;
	public GameObject flashLight;
	public GameObject handsFPS;
	public Animator handsFPSAnimator;

	bool consoleOn = false;
	public float warningTextTime = 5.0f;

	string oncekiKomut = "";
	bool timerStop = false;

	GameObject fps;
	GameObject vehicleDriving;
	GameObject siraNesnesi;
	GameObject skyManager;
	GameObject player;
	GameObject playerItemDropPoint;
	GameObject kamera;
	GameObject yansimaNesnesi;

	string[] commandDictionary = new string[] {
		"clear",
		"drop",
		"help",
		"quit",
		"reset",
		"status",
		"time",
		"use",
		"toggle",
		"yes"
	};

	ArrayList possibleCommandsWords;

	public AudioClip[] gameSounds;

	public bool carDrivingMode = false;
	public bool oturmaMode = false;
	public bool crouchMode = false;
	bool zoomIn = false;

	public bool altYaziMesgul = false;

	public Transform[] Cars;
	public Transform[] itemsToCollect;
	public PostProcessingProfile[] kameraEfektProfilleri;

	saveLoadGameData saveLoadComponent;
	throwStone throwStoneComponent;
	childrenWorkTime childrenWorkTimeComponent;
	playerStatus playerStatusComponent;
	dialogPanelControl dialogPanelControlComponent;
	playerLookingAt playerLookingAtComponent;

	// Use this for initialization
	void Start () {
		//component cache
		saveLoadComponent = GetComponent<saveLoadGameData> ();
		throwStoneComponent = GetComponent<throwStone> ();
		childrenWorkTimeComponent = GetComponent<childrenWorkTime> ();
		playerStatusComponent = GetComponent<playerStatus> ();
		dialogPanelControlComponent = GetComponent<dialogPanelControl> ();
		playerLookingAtComponent = GetComponent<playerLookingAt> ();
				
		//ana harita kontrolü load ederken var
		saveLoadComponent.loadPlayerCoordinate ();

		skyManager = GetComponent<gerekliNesneler> ().SkyManager;
		player = GetComponent<gerekliNesneler> ().player;
		playerItemDropPoint = GetComponent<gerekliNesneler> ().itemDropPoint;
		kamera = GetComponent<gerekliNesneler> ().Kamera;
		yansimaNesnesi = GetComponent<gerekliNesneler> ().yansimaNesnesi;
		fps = GetComponent<gerekliNesneler> ().fpsText;

		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		consolePanelObject.SetActive (false);	

		warningText.text = "<b>Tip:</b> You can press TAB key for the console screen...";
		consoleLogText.text = "Hello player! Write <b>h</b> or <b>help</b> for help\n";

		Time.timeScale = 1f;

		consoleInput.onValueChanged.AddListener (delegate {
			ValueChangeCheck ();
		});
		possibleCommandsWords = new ArrayList (commandDictionary.Length);
	}

	void ValueChangeCheck () {
		possibleCommandsWords.Clear ();
		tipText.text = "";

		//Go through all words in your database (could be slow with many words)
		for (int i = 0; i < commandDictionary.Length; i++) {
			//Check if the words start matches the start from the input
			if (commandDictionary [i].StartsWith (consoleInput.text, true, null))
				possibleCommandsWords.Add (i);
		}
		// Iterate through all possible words
		if (consoleInput.text != "") {
			for (int i = 0; i < possibleCommandsWords.Count; i++) {
				tipText.text += " " + commandDictionary [(int)possibleCommandsWords [i]]; // do something with the word
				//may display it
			}
		}
	}

	//konuşma altyazıları
	public void showSubtitle (string playerName, string message, float delay) {
		if (!altYaziMesgul) {
			StartCoroutine (showSubtitleCoroutine (playerName + ": " + message, delay));	
		}
	}

	IEnumerator showSubtitleCoroutine (string message, float delay) {
		altYaziMesgul = true;
		subtitleText.text = message;
		subtitleText.enabled = true;
		yield return new WaitForSeconds (delay);
		altYaziMesgul = false;
		subtitleText.enabled = false;
	}

	public void oyuncuKalk () {
		player.GetComponent<Animator> ().SetBool ("oyuncuEgil", false);
		player.transform.Translate (Vector3.up * 1.0f, Space.World);
		crouchMode = false;
	}

	public void oyuncuEgil () {		
		player.GetComponent<Animator> ().SetBool ("oyuncuEgil", true);
		crouchMode = true;
	}

	public void kameraZumIn () {
		kamera.GetComponent<Animator> ().SetBool ("zoomIn", true);
		kamera.GetComponent<PostProcessingBehaviour> ().profile = kameraEfektProfilleri [1];
		zoomIn = true;
	}

	public void kameraZumOut () {
		kamera.GetComponent<Animator> ().SetBool ("zoomIn", false);
		kamera.GetComponent<PostProcessingBehaviour> ().profile = kameraEfektProfilleri [0];
		zoomIn = false;
	}

	void arabayaBin (GameObject araba) {
		oyuncuKalk ();
		kameraZumOut ();

		vehicleDriving = araba;
		carDrivingMode = true;

		fareImlec.SetActive (false);
		handsFPS.SetActive (false);

		araba.GetComponent<CarController> ().enabled = true;
		araba.GetComponent<CarUserControl> ().enabled = true;
		araba.GetComponent<CarAudio> ().enabled = true;

		player.GetComponent<RigidbodyFirstPersonController> ().enabled = false;
		kamera.GetComponent<HeadBob> ().enabled = false;

		player.GetComponent<Rigidbody> ().isKinematic = true;
		player.GetComponent<Rigidbody> ().useGravity = false;
		player.GetComponent<CapsuleCollider> ().enabled = false;

		player.transform.SetParent (araba.transform.GetChild (0).transform);
		player.transform.position = araba.transform.GetChild (0).transform.position; //sit pointe gider
		player.transform.rotation = araba.transform.GetChild (0).transform.rotation;

		kamera.transform.rotation = Quaternion.identity;
		kamera.GetComponent<SimpleMouseRotator> ().enabled = true;

		foreach (AudioSource ses in araba.GetComponents<AudioSource> ()) {
			ses.enabled = true;
			ses.volume = 0.2f;
		}

		throwStoneComponent.enabled = false;

		flashLight.SetActive (false);	//flash varsa kapanır

		player.GetComponent<AudioSource> ().PlayOneShot (gameSounds [0]); // kapı sesi

		speedText.text = "0 Kmh";
	}

	void arabadanIn (GameObject araba) {
		
		araba.GetComponent<CarController> ().enabled = false;
		araba.GetComponent<CarUserControl> ().enabled = false;
		araba.GetComponent<CarAudio> ().enabled = false;

		//araba DURMUYO
		araba.GetComponent<CarController> ().Move (0, 0, 0, 0);

		player.GetComponent<RigidbodyFirstPersonController> ().enabled = true;
		kamera.GetComponent<HeadBob> ().enabled = true;
		player.transform.SetParent (null);
		player.GetComponent<Rigidbody> ().isKinematic = false;
		player.GetComponent<Rigidbody> ().useGravity = true;
		player.GetComponent<CapsuleCollider> ().enabled = true;

		kamera.transform.rotation = Quaternion.identity;
		kamera.GetComponent<SimpleMouseRotator> ().enabled = false;

		player.transform.position = araba.transform.GetChild (1).transform.position; //exit pointe gider
		player.transform.rotation = araba.transform.GetChild (1).transform.rotation;

		foreach (var ses in araba.GetComponents<AudioSource> ()) {
			ses.enabled = false;
		}

		player.GetComponent<AudioSource> ().PlayOneShot (gameSounds [0]); //kapı sesi

		throwStoneComponent.enabled = true;

		speedText.text = "";

		vehicleDriving = null;
		carDrivingMode = false;

		fareImlec.SetActive (true);
		handsFPS.SetActive (true);
	}

	void arabayiKurtar (GameObject araba) {
		//recover flipped or damaged car

		Vector3 eskiPosition = araba.transform.position;
		Quaternion eskiRotation = araba.transform.localRotation;

		string carType = araba.name;
		Transform clone;

		DestroyObject (araba);

		player.GetComponent<AudioSource> ().PlayOneShot (gameSounds [4]); // basarili kurtar

		switch (carType) {
		case "redCar":
			clone = Instantiate (Cars [2], eskiPosition, Quaternion.identity);
			clone.name = carType;
			saveLoadComponent.Cars [0] = clone.gameObject;
			if (skyManager.GetComponent <LSkyTOD> ().timeline >= childrenWorkTimeComponent.startHour &&
			    skyManager.GetComponent <LSkyTOD> ().timeline <= childrenWorkTimeComponent.endHour) {
				clone.GetComponent<myCarLights> ().lamp1.SetActive (false);
				clone.GetComponent<myCarLights> ().lamp2.SetActive (false);
			} else {
				clone.GetComponent<myCarLights> ().lamp1.SetActive (true);
				clone.GetComponent<myCarLights> ().lamp2.SetActive (true);
			}
			childrenWorkTimeComponent.geceYanacakLambalar [5] = clone.GetComponent<myCarLights> ().lamp1;
			childrenWorkTimeComponent.geceYanacakLambalar [6] = clone.GetComponent<myCarLights> ().lamp2;
			break;
		case "blueCar":
			clone = Instantiate (Cars [0], eskiPosition, Quaternion.identity);
			clone.name = carType;
			saveLoadComponent.Cars [2] = clone.gameObject;
			if (skyManager.GetComponent <LSkyTOD> ().timeline >= childrenWorkTimeComponent.startHour &&
			    skyManager.GetComponent <LSkyTOD> ().timeline <= childrenWorkTimeComponent.endHour) {
				clone.GetComponent<myCarLights> ().lamp1.SetActive (false);
				clone.GetComponent<myCarLights> ().lamp2.SetActive (false);
			} else {
				clone.GetComponent<myCarLights> ().lamp1.SetActive (true);
				clone.GetComponent<myCarLights> ().lamp2.SetActive (true);
			}
			childrenWorkTimeComponent.geceYanacakLambalar [9] = clone.GetComponent<myCarLights> ().lamp1;
			childrenWorkTimeComponent.geceYanacakLambalar [10] = clone.GetComponent<myCarLights> ().lamp2;
			break;
		case "orangeCar":
			clone = Instantiate (Cars [1], eskiPosition, Quaternion.identity);
			clone.name = carType;
			saveLoadComponent.Cars [1] = clone.gameObject;
			if (skyManager.GetComponent <LSkyTOD> ().timeline >= childrenWorkTimeComponent.startHour &&
			    skyManager.GetComponent <LSkyTOD> ().timeline <= childrenWorkTimeComponent.endHour) {
				clone.GetComponent<myCarLights> ().lamp1.SetActive (false);
				clone.GetComponent<myCarLights> ().lamp2.SetActive (false);
			} else {
				clone.GetComponent<myCarLights> ().lamp1.SetActive (true);
				clone.GetComponent<myCarLights> ().lamp2.SetActive (true);
			}
			childrenWorkTimeComponent.geceYanacakLambalar [7] = clone.GetComponent<myCarLights> ().lamp1;
			childrenWorkTimeComponent.geceYanacakLambalar [8] = clone.GetComponent<myCarLights> ().lamp2;
			break;
		default:
			break;
		}
	}

	void itemCollect (GameObject item) {
		//nesne toplama ve inventory ekleme
		if (playerStatusComponent.playerCarriage > 30f) {
			//30 KG ileride XP ile artabilir
			player.GetComponent<AudioSource> ().PlayOneShot (gameSounds [7]); // block
			return;
		}

		handsFPSAnimator.SetBool ("grabbing", true); 
		player.GetComponent<AudioSource> ().PlayOneShot (gameSounds [6]); // item pick
		playerStatusComponent.playerCarriage += item.GetComponent<itemProperties> ().weight;
			
		switch (item.name) {
		case "apple":
			saveLoadComponent.appleCount += item.GetComponent<itemProperties> ().amount;
			break;
		case "bread":
			saveLoadComponent.breadCount += item.GetComponent<itemProperties> ().amount;
			break;
		case "water":
			saveLoadComponent.waterCount += item.GetComponent<itemProperties> ().amount;
			break;
		case "money":
			saveLoadComponent.moneyCount += item.GetComponent<itemProperties> ().amount;
			break;
		default:
			break;
		}

		Destroy (item);

		handsFPSAnimator.SetBool ("grabbing", false); 
	}

	void sirayaOtur (GameObject sira) {
		oyuncuKalk ();
		kameraZumOut ();

		siraNesnesi = sira;
		oturmaMode = true;

		fareImlec.SetActive (false);

		player.GetComponent<RigidbodyFirstPersonController> ().enabled = false;
		kamera.GetComponent<HeadBob> ().enabled = false;

		player.transform.SetParent (siraNesnesi.transform.GetChild (1).transform);
		player.transform.position = siraNesnesi.transform.GetChild (1).transform.position; //sit pointe gider

		siraNesnesi.transform.GetChild (3).gameObject.SetActive (true);
		kamera.SetActive (false);

		player.GetComponent<Rigidbody> ().isKinematic = true;
		player.GetComponent<Rigidbody> ().useGravity = false;
		player.GetComponent<CapsuleCollider> ().enabled = false;

		kamera.transform.rotation = Quaternion.identity;
		kamera.GetComponent<SimpleMouseRotator> ().enabled = true;

		flashLight.SetActive (false);	//flash varsa kapanır

		player.GetComponent<AudioSource> ().PlayOneShot (gameSounds [5]); // otur sesi
		throwStoneComponent.enabled = false;

		Time.timeScale = 4;
	}

	void siradanKalk (GameObject sira) {
		

		player.GetComponent<RigidbodyFirstPersonController> ().enabled = true;
		kamera.GetComponent<HeadBob> ().enabled = true;

		player.GetComponent<Rigidbody> ().isKinematic = false;
		player.GetComponent<Rigidbody> ().useGravity = true;
		player.GetComponent<CapsuleCollider> ().enabled = true;

		kamera.transform.rotation = Quaternion.identity;
		kamera.GetComponent<SimpleMouseRotator> ().enabled = false;

		player.transform.position = sira.transform.GetChild (2).transform.position; //exit pointe gider
		player.transform.rotation = sira.transform.GetChild (2).transform.rotation;

		player.GetComponent<AudioSource> ().PlayOneShot (gameSounds [5]); //otur sesi

		player.transform.SetParent (null);

		siraNesnesi.transform.GetChild (3).gameObject.SetActive (false);
		kamera.SetActive (true);

		siraNesnesi = null;
		oturmaMode = false;

		fareImlec.SetActive (true);
		throwStoneComponent.enabled = true;

		Time.timeScale = 1;
	}

	public void npcFollowModeToggle () {
		if (dialogPanelControlComponent.selectedNPC.GetComponent<basicAI> ().followPlayerMode) {
			dialogPanelControlComponent.selectedNPC.GetComponent<basicAI> ().followPlayerMode = false;
			dialogPanelControlComponent.followButtonText.text = "Follow Me";
		} else {
			dialogPanelControlComponent.selectedNPC.GetComponent<basicAI> ().followPlayerMode = true;
			dialogPanelControlComponent.followButtonText.text = "Don't Follow Me";
		}
	}

	void diyalogAc () {		
		kameraZumOut ();

		dialogPanelControlComponent.npcName.text = dialogPanelControlComponent.selectedNPC.GetComponent<basicAI> ().playerName;

		if (dialogPanelControlComponent.selectedNPC.GetComponent<basicAI> ().followPlayerMode) {
			dialogPanelControlComponent.followButtonText.text = "Don't Follow Me";
		} else {
			dialogPanelControlComponent.followButtonText.text = "Follow Me";
		}

		dialogPanelControlComponent.dialogPanelMode = true;
		dialogPanelControlComponent.dialogPanelObject.SetActive (true);
		fareImlec.SetActive (false);
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
		playerLookingAtComponent.enabled = false;
		throwStoneComponent.enabled = false;

		subtitleText.text = "";
		Time.timeScale = 0;
	}

	void diyalogKapat () {
		dialogPanelControlComponent.dialogPanelMode = false;
		dialogPanelControlComponent.dialogPanelObject.SetActive (false);
		fareImlec.SetActive (true);
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		playerLookingAtComponent.enabled = true;
		throwStoneComponent.enabled = true;
		Time.timeScale = 1;
	}

	void cocukDiyalog () {
		GameObject cocuk = playerLookingAtComponent.seciliNesne;
		dialogPanelControlComponent.selectedNPC = cocuk;
		if (cocuk.GetComponent<basicAI> ().aracaMiCarpti) {
			//THANKS
			showSubtitle (cocuk.GetComponent<basicAI> ().playerName, "Thanks!", 2f);
			cocuk.GetComponent<AudioSource> ().PlayOneShot (gameSounds [(Random.value < 0.5f) ? 1 : 2]);
			cocuk.GetComponent<Animator> ().SetBool ("dieArtik", false);
			cocuk.GetComponent<NavMeshAgent> ().isStopped = false;
			cocuk.GetComponent<basicAI> ().aracaMiCarpti = false;
		} else
			diyalogAc ();
	}

	float CarSpeed (GameObject car) {
		float speed = car.GetComponent<Rigidbody> ().velocity.magnitude;
		float yonu = Vector3.Angle (car.transform.forward, car.GetComponent<Rigidbody> ().velocity);
		speed *= 3.6f; //KMH için
		if (yonu > 150 && yonu <= 180) {
			speed = -1;
		}
		return speed;
	}

	void FixedUpdate () {
		if (vehicleDriving) {
			float speed = CarSpeed (vehicleDriving);
			if (speed < 0) {
				speedText.text = "R";			
			} else if (speed >= 0 && speed <= 1) {
				speedText.text = "";			
			} else
				speedText.text = (Mathf.Ceil (speed)).ToString () + " Kmh";			
		}
	}

	void OnApplicationQuit () {
		if (saveLoadComponent != null) {
			saveLoadComponent.savePlayerCoordinate ();	
		}
	}

	// Update is called once per frame
	void Update () {

		handsFPSAnimator.SetBool ("running", player.GetComponent<RigidbodyFirstPersonController> ().Running); 
		
		if (Input.GetKeyDown (KeyCode.Escape)) {
			if (consoleOn)
				closeConsolePanel ();
			else if (dialogPanelControlComponent.dialogPanelMode)
				diyalogKapat ();
			else if (SceneManager.GetActiveScene ().name == "anaHarita") {
				saveLoadComponent.savePlayerCoordinate ();
				SceneManager.LoadScene (0);
			} else
				SceneManager.LoadScene (0);
			return;
		}

		if (Input.GetKeyDown (KeyCode.Tab) && !oturmaMode &&
		    !dialogPanelControlComponent.dialogPanelMode) {
			consoleOn = !consoleOn;
			if (consoleOn) {
				openConsolePanel ();
			} else {				
				closeConsolePanel ();
			}
			return;
		}

		if (Input.GetKeyDown (KeyCode.F) &&
		    !consolePanelObject.activeSelf &&
		    !dialogPanelControlComponent.dialogPanelMode &&
		    !carDrivingMode && !oturmaMode) {
			player.GetComponent<AudioSource> ().PlayOneShot (gameSounds [3]); // flash lamba sesi
			if (flashLight) {
				if (flashLight.activeSelf) {
					throwStoneComponent.enabled = true;
					flashLight.SetActive (false);	
				} else {
					throwStoneComponent.enabled = false;
					flashLight.SetActive (true);
				}
			}
			return;
		}

		if (Input.GetKeyDown (KeyCode.M) &&
		    !consolePanelObject.activeSelf && missionPanel &&
		    !dialogPanelControlComponent.dialogPanelMode) {
			if (!missionPanel.activeSelf) {
				missionPanel.GetComponent<autoHidePanel> ().stayingTime = 5f;
				missionPanel.GetComponent<autoHidePanel> ().timerStop = false;
				missionPanel.SetActive (true);
			}
			return;
		}

		//crouch
		if (Input.GetButtonDown ("Duck") &&
		    !carDrivingMode && !oturmaMode &&
		    !consolePanelObject.activeSelf &&
		    player.GetComponent<RigidbodyFirstPersonController> ().Grounded) {			
			if (crouchMode) {
				oyuncuKalk ();
			} else {
				oyuncuEgil ();
			}
			return;
		}

		if (Input.GetMouseButtonDown (1) &&
		    !carDrivingMode && !oturmaMode &&
		    !consolePanelObject.activeSelf) {
			if (zoomIn) {
				kameraZumOut ();
			} else {
				kameraZumIn ();
			}
		} 
		
		if (Input.GetKeyDown (KeyCode.E) && dialogPanelControlComponent.dialogPanelMode) {
			diyalogKapat ();
			return;
		}			

		if (Input.GetKeyDown (KeyCode.E) &&
		    !consolePanelObject.activeSelf &&
		    !dialogPanelControlComponent.dialogPanelMode &&
		    playerLookingAtComponent.seciliNesne) {
			switch (playerLookingAtComponent.seciliNesne.tag) {
			case "car":
				if (playerLookingAtComponent.seciliNesne.gameObject.GetComponent<myCarLights> ().grounded) {
					arabayaBin (playerLookingAtComponent.seciliNesne);	
				}
				break;
			case "building":				
				if (SceneManager.GetActiveScene ().name == "anaHarita") {
					//ana haritadayken bir binaya girdi
					player.GetComponent<AudioSource> ().PlayOneShot (gameSounds [9]); // kapı sesi

					saveLoadComponent.savePlayerCoordinate ();
					SceneManager.LoadScene (2);
				} else {
					//bir binadayken ana haritaya döner
					player.GetComponent<AudioSource> ().PlayOneShot (gameSounds [9]); // kapı sesi

					saveLoadComponent.savePlayerCoordinate ();
					SceneManager.LoadScene (1);
				}
				break;
			case "NPC":
				cocukDiyalog ();
				break;
			case "oturak":
				sirayaOtur (playerLookingAtComponent.seciliNesne);
				break;
			case "item":				
				itemCollect (playerLookingAtComponent.seciliNesne);
				break;
			default:
				break;
			}
			return;
		}

		if (Input.GetKeyDown (KeyCode.E) &&
		    !consolePanelObject.activeSelf &&
		    !playerLookingAtComponent.seciliNesne) {
			//arabadan in
			if (vehicleDriving)
				arabadanIn (vehicleDriving);
			else if (siraNesnesi)
				siradanKalk (siraNesnesi);			
			return;
		}

		if (Input.GetKeyDown (KeyCode.R) &&
		    !consolePanelObject.activeSelf &&
		    playerLookingAtComponent.seciliNesne) {
			//araba recover - tamir ve ters dönmüş ise kurtarır
			switch (playerLookingAtComponent.seciliNesne.tag) {
			case "car":
				arabayiKurtar (playerLookingAtComponent.seciliNesne);
				break;
			default:
				break;
			}
			return;
		}

		if (Input.GetKeyDown (KeyCode.UpArrow) &&
		    consolePanelObject.activeSelf &&
		    consoleInput.isFocused) {
			if (oncekiKomut != "") {
				consoleInput.text = oncekiKomut;	
				consoleInput.MoveTextEnd (false);
			}
			return;
		}

		if (Input.GetKeyDown (KeyCode.DownArrow) &&
		    consolePanelObject.activeSelf &&
		    consoleInput.isFocused) {
			consoleInput.text = "";	
			return;
		}

		if (consolePanelObject.activeSelf && consoleInput.text != "") {  
			Canvas.ForceUpdateCanvases ();
			verticalScrollBar.value = 1f;
			Canvas.ForceUpdateCanvases ();
			return;
		}

		if (timerStop == false) {
			warningTextTime -= Time.deltaTime;
			if (warningTextTime <= 0.0f) {
				timerEnded ();
			}	
		}
	}

	void timerEnded () {
		warningText.text = "";
		timerStop = true;
	}

	public void openConsolePanel () {
		//konsol açıl
		consoleOn = true;
		Time.timeScale = 0f;

		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
		consolePanelObject.SetActive (true);
		consoleInput.Select ();
		consoleInput.text = "";
		warningText.text = "";
		consoleInput.ActivateInputField ();

		//space ile zıplamasın
		player.GetComponent<RigidbodyFirstPersonController> ().enabled = false;
		if (throwStoneComponent) {
			throwStoneComponent.enabled = false;	
		}
	}

	public void closeConsolePanel () {
		consoleOn = false;
		//konsol kapan
		Time.timeScale = 1f;

		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		consoleInput.text = "";
		consolePanelObject.SetActive (false);	

		//space ile zıplamasın
		player.GetComponent<RigidbodyFirstPersonController> ().enabled = true;
		if (throwStoneComponent) {
			throwStoneComponent.enabled = true;	
		}
	}

	public string GetTimeString () {
		LSkyTOD TOD = skyManager.GetComponent<LSkyTOD> ();
		string h = TOD.CurrentHour < 10 ? "0" + TOD.CurrentHour.ToString () : TOD.CurrentHour.ToString ();
		string m = TOD.CurrentMinute < 10 ? "0" + TOD.CurrentMinute.ToString () : TOD.CurrentMinute.ToString ();
		return h + ":" + m;
	}

	public void dropAnItem (string item) {
		Transform newObject;
		GameObject newDroppedObject;
		Vector3 position;
		switch (item) {
		case "apple":
			newDroppedObject = itemsToCollect [0].gameObject;
			position = new Vector3 (playerItemDropPoint.transform.position.x, 
				playerItemDropPoint.transform.position.y, 
				playerItemDropPoint.transform.position.z);				
			newObject = Instantiate (newDroppedObject.transform, position, playerItemDropPoint.transform.rotation);
			newObject.SetParent (spawnParent.transform);
			newObject.gameObject.name = newDroppedObject.name;
			break;
		case "bread":
			newDroppedObject = itemsToCollect [1].gameObject;
			position = new Vector3 (playerItemDropPoint.transform.position.x, 
				playerItemDropPoint.transform.position.y, 
				playerItemDropPoint.transform.position.z);				
			newObject = Instantiate (newDroppedObject.transform, position, playerItemDropPoint.transform.rotation);
			newObject.SetParent (spawnParent.transform);
			newObject.gameObject.name = newDroppedObject.name;
			break;
		case "water":
			newDroppedObject = itemsToCollect [3].gameObject;
			position = new Vector3 (playerItemDropPoint.transform.position.x, 
				playerItemDropPoint.transform.position.y, 
				playerItemDropPoint.transform.position.z);				
			newObject = Instantiate (newDroppedObject.transform, position, playerItemDropPoint.transform.rotation);
			newObject.SetParent (spawnParent.transform);
			newObject.gameObject.name = newDroppedObject.name;
			break;
		default:
			break;
		}
	}

	public string komutDenetle (string gelen) {
		

		string cevap = "";
		gelen = gelen.ToLower ();

		string[] gelenler = gelen.Split (char.Parse (" "));

		switch (gelenler [0]) {
		case "drop":
		case "d":
			if (gelenler.Length > 1) {
				switch (gelenler [1]) {
				case "apple":
					if (saveLoadComponent.appleCount > 0) {
						saveLoadComponent.appleCount -= 1;
						if (saveLoadComponent.appleCount < 0) {
							saveLoadComponent.appleCount = 0;
						}
						playerStatusComponent.playerCarriage -= 0.2f;
						if (playerStatusComponent.playerCarriage < 0) {
							playerStatusComponent.playerCarriage = 0;
						}
						//player.GetComponent<AudioSource> ().PlayOneShot (gameSounds [8]); // gulp
						cevap = "You dropped one apple <sprite index=15>\n";
						cevap += "You have " + System.Math.Round (saveLoadComponent.appleCount, 1) +
						" apples(s) <sprite index=15> now...\n ";
						dropAnItem ("apple");
					} else
						cevap = "You have no <color=red>apples</color> <sprite index=15>!\n";
					break;
				case "bread":
					if (saveLoadComponent.breadCount > 0) {
						saveLoadComponent.breadCount -= 1;
						if (saveLoadComponent.breadCount < 0) {
							saveLoadComponent.breadCount = 0;
						}
						playerStatusComponent.playerCarriage -= 0.3f;
						if (playerStatusComponent.playerCarriage < 0) {
							playerStatusComponent.playerCarriage = 0;
						}
						//player.GetComponent<AudioSource> ().PlayOneShot (gameSounds [8]); // gulp
						cevap = "You dropped one bread <sprite index=16>\n";
						cevap += "You have " + System.Math.Round (saveLoadComponent.breadCount, 1) +
						" bread(s)  <sprite index=16> now...\n ";
						dropAnItem ("bread");
					} else
						cevap = "You have no <color=red>breads  <sprite index=16></color>!\n";
					break;
				case "water":
					if (saveLoadComponent.waterCount > 0) {
						saveLoadComponent.waterCount -= 0.5f;
						if (saveLoadComponent.waterCount < 0) {
							saveLoadComponent.waterCount = 0;
						}
						playerStatusComponent.playerCarriage -= 0.5f;
						if (playerStatusComponent.playerCarriage < 0) {
							playerStatusComponent.playerCarriage = 0;
						}
						//player.GetComponent<AudioSource> ().PlayOneShot (gameSounds [8]); // gulp
						cevap = "You dropped one bottle of water <sprite index=17>\n";
						cevap += "You have " + System.Math.Round (saveLoadComponent.waterCount, 1) +
						" liter(s) of water <sprite index=17> now...\n ";
						dropAnItem ("water");
					} else
						cevap = "You have no <color=red>water</color> <sprite index=17>!\n";
					break;
				case "money":
					cevap = "You can not drop <color=red>money</color> <sprite index=18>\n";
					break;
				default:
					cevap = "<color=red>Unknown Command</color> <i>You can use: apple, bread or water</i>\n";
					break;
				}	
			} else
				cevap = "<color=red>Parameter missing</color> <i>You can use: apple, bread or water</i>\n";
			break;
		case "use":
		case "u":
			if (gelenler.Length > 1) {
				switch (gelenler [1]) {
				case "apple":
					if (saveLoadComponent.appleCount > 0) {
						saveLoadComponent.appleCount -= 1;
						if (saveLoadComponent.appleCount < 0) {
							saveLoadComponent.appleCount = 0;
						}
						playerStatusComponent.playerCarriage -= 0.2f;
						if (playerStatusComponent.playerCarriage < 0) {
							playerStatusComponent.playerCarriage = 0;
						}
						player.GetComponent<AudioSource> ().PlayOneShot (gameSounds [8]); // gulp
						cevap = "You ate one apple <sprite index=15>\n";
						cevap += "You have " + System.Math.Round (saveLoadComponent.appleCount, 1) +
						" apples(s) <sprite index=15> now...\n ";
					} else
						cevap = "You have no <color=red>apples</color> <sprite index=15>!\n";
					break;
				case "bread":
					if (saveLoadComponent.breadCount > 0) {
						saveLoadComponent.breadCount -= 1;
						if (saveLoadComponent.breadCount < 0) {
							saveLoadComponent.breadCount = 0;
						}
						playerStatusComponent.playerCarriage -= 0.3f;
						if (playerStatusComponent.playerCarriage < 0) {
							playerStatusComponent.playerCarriage = 0;
						}
						player.GetComponent<AudioSource> ().PlayOneShot (gameSounds [8]); // gulp
						cevap = "You ate one bread <sprite index=16>\n";
						cevap += "You have " + System.Math.Round (saveLoadComponent.breadCount, 1) +
						" bread(s) <sprite index=16> now...\n ";
					} else
						cevap = "You have no <color=red>breads</color> <sprite index=16>!\n";
					break;
				case "water":
					if (saveLoadComponent.waterCount > 0) {
						saveLoadComponent.waterCount -= 0.5f;
						if (saveLoadComponent.waterCount < 0) {
							saveLoadComponent.waterCount = 0;
						}
						playerStatusComponent.playerCarriage -= 0.5f;
						if (playerStatusComponent.playerCarriage < 0) {
							playerStatusComponent.playerCarriage = 0;
						}
						player.GetComponent<AudioSource> ().PlayOneShot (gameSounds [8]); // gulp
						cevap = "You drank one bottle of water <sprite index=17>\n";
						cevap += "You have " + System.Math.Round (saveLoadComponent.waterCount, 1) +
						" liter(s) of water <sprite index=17> now...\n ";
					} else
						cevap = "You have no <color=red>water</color> <sprite index=17>!\n";
					break;
				case "money":
					cevap = "You can not use <color=red>money</color> <sprite index=18> like this\n";
					break;
				default:
					cevap = "<color=red>Unknown Command</color> <i>You can write: apple, bread or water</i>\n";
					break;
				}	
			} else
				cevap = "<color=red>Parameter missing</color> <i>You can write: apple, bread or water</i>\n";
			break;
		case "quit":
		case "q":
			//need confirmation for quitting
			cevap = "Are you sure to <color=red>quit to menu</color>? (y / yes)";
			break;
		case "status":
		case "s":
			cevap = "<i>Here is your status:</i>\n";
			cevap += "You have ";
			if (saveLoadComponent.breadCount > 0) {
				cevap += "\n" + System.Math.Round (saveLoadComponent.breadCount, 1) + " bread(s) <sprite index=16> ";
			}
			if (saveLoadComponent.appleCount > 0) {
				cevap += "\n" + System.Math.Round (saveLoadComponent.appleCount, 1) + " apple(s) <sprite index=15> ";
			}
			if (saveLoadComponent.waterCount > 0) {
				cevap += "\n" + System.Math.Round (saveLoadComponent.waterCount, 1) + " liter(s) of water <sprite index=17> ";
			}
			if (saveLoadComponent.moneyCount > 0) {
				cevap += "\n" + System.Math.Round (saveLoadComponent.moneyCount, 1) + " TL money <sprite index=18> ";
			}
			if (saveLoadComponent.breadCount + saveLoadComponent.waterCount +
			    saveLoadComponent.appleCount + saveLoadComponent.moneyCount == 0) {
				cevap += "no items";
			}
			if (playerStatusComponent.playerCarriage > 0) {
				cevap += "\nYour total load is " + System.Math.Round (playerStatusComponent.playerCarriage, 2) +
				" kilogram(s)\n";	
			}

			break;
		case "time":
		case "t":			
			if (skyManager) {
				cevap = "It's " + GetTimeString () + "\n";						
			} else
				cevap = "Clock is not working";
			break;
		case "ts":
			if (saveLoadComponent.settingsSound) {
				PlayerPrefs.SetInt ("sound", 0);
				saveLoadComponent.settingsSound = false;
				AudioListener.pause = true;
			} else {
				PlayerPrefs.SetInt ("sound", 1);
				saveLoadComponent.settingsSound = true;
				AudioListener.pause = false;
			}

			cevap = "Sound is toggled\n";
			break;
		case "te":			
			if (saveLoadComponent.settingsEffects) {
				PlayerPrefs.SetInt ("effects", 0);
				kamera.GetComponent<PostProcessingBehaviour> ().enabled = false;
				saveLoadComponent.settingsEffects = false;
				yansimaNesnesi.SetActive (false);
			} else {
				PlayerPrefs.SetInt ("effects", 1);
				kamera.GetComponent<PostProcessingBehaviour> ().enabled = true;
				saveLoadComponent.settingsEffects = true;
				yansimaNesnesi.SetActive (true);
			}
			
			cevap = "Effects are toggled\n";
			break;
		case "tf":
			if (saveLoadComponent.settingsFPS) {
				PlayerPrefs.SetInt ("fps", 0);
				saveLoadComponent.settingsFPS = false;
				fps.SetActive (false);
			} else {
				PlayerPrefs.SetInt ("fps", 1);
				saveLoadComponent.settingsFPS = true;
				fps.SetActive (true);
			}
			
			cevap = "FPS text is toggled\n";
			break;
		case "reset":
		case "r":
			//need confirmation for deleting
			cevap = "<sprite index=13> Are you sure to <color=red>delete all game data and restart</color>? (y / yes)";
			break;
		case "yes":
		case "y":
			if (oncekiKomut.Trim ().ToLower () == "r" ||
			    oncekiKomut.Trim ().ToLower () == "reset") {
				PlayerPrefs.DeleteAll ();
				SceneManager.LoadScene (0);
				cevap = "You deleted all game data. You need to restart";	
			} else if (oncekiKomut.Trim ().ToLower () == "q" ||
			           oncekiKomut.Trim ().ToLower () == "quit") {
				if (SceneManager.GetActiveScene ().name == "anaHarita")
					saveLoadComponent.savePlayerCoordinate ();
				SceneManager.LoadScene (0);
			} else {
				cevap = "<sprite index=6> Nothing to confirm";	
			}			  
			break;
		case "help":
		case "?":
		case "h":
			cevap = "<color=blue>Help (Game keys and Console commands)</color>\n";
			cevap += "F: Activate flash light\n";
			cevap += "M: Show your missions\n";
			cevap += "Q: Throw a stone\n";
			cevap += "R: Recover a vehicle\n";
			cevap += "Left Ctrl: Crouch\n";
			cevap += "Mouse Right Click: Camera Zoom\n";
			cevap += "F9: Take screenshot\n";
			cevap += "-------------------\n";
			cevap += "c / clear: Clear console\n";
			cevap += "d / drop: Drop an item. Example: <color=#005500>drop bread</color>\n";
			cevap += "h / ? / help: Show available commands\n";
			cevap += "q / quit: Quit to main menu\n";
			cevap += "r / reset: Delete data for a new game!\n";
			cevap += "s / status: Show player status\n";
			cevap += "t / time: Show current time\n";
			cevap += "t : Toggles these: <color=red>s</color> sound, <color=red>e</color> effects, " +
			"<color=red>f</color> FPS text. Example: <color=#005500>ts</color>\n";
			cevap += "u / use: Use an item. Example: <color=#005500>use bread</color>\n";
			cevap += "y / yes: Confirmation for reset and quit";
			break;
		default:
			cevap = "<sprite index=6> <color=red>Unknown Command</color> <i>You can write: h / ? / help</i>";
			break;
		}

		return cevap;
	}

	public void consoleInputSubmitCallBack () {
		if (consoleInput.text.Trim () != "") {			

			consoleLogText.text = consoleInput.text + "\n" + komutDenetle (consoleInput.text) +
			"\n" + consoleLogText.text;
			oncekiKomut = consoleInput.text; //reset/quit.. için alt menü
			if (consoleLogText.text.Length > 3000) {
				consoleLogText.text = "<sprite index=4> Too much text, automatically cleared\n";
			}

			if (consoleInput.text.Trim ().ToLower () == "c" ||
			    consoleInput.text.Trim ().ToLower () == "clear") {
				consoleLogText.text = "\n";
			}
			consoleInput.text = ""; //Clear Inputfield text
			consoleInput.ActivateInputField (); //Re-focus on the input field
			consoleInput.Select ();//Re-focus on the input field	

		}
		Canvas.ForceUpdateCanvases ();
		verticalScrollBar.value = 1f; //0f aşağı
		Canvas.ForceUpdateCanvases ();
	}
}