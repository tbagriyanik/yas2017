using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Vehicles.Car;

public class playerLookingAt : MonoBehaviour {

	public Text tipText;
	public GameObject fareImlec;
	public GameObject seciliNesne;

	public GameObject[] securityCameras;

	float mouseDistance = 2f;
	Camera kamera;

	// Use this for initialization
	void Start () {
		kamera = GetComponent<gerekliNesneler> ().Kamera.GetComponent<Camera> ();
		mouseDistance = GetComponent<gerekliNesneler> ().mouseDistance;
	}

	void activateCamera () {
		//playerStatus aslında yer tutucu sadece
		int seciliKamera = (int)seciliNesne.GetComponent <playerStatus> ().playerCarriage;
		securityCameras [seciliKamera].gameObject.SetActive (true);	
	}

	// Update is called once per frame
	void LateUpdate () {
		seciliNesne = getMouseHoverObject (mouseDistance);
		tipText.text = "";

		if (seciliNesne) {
			fareImlec.GetComponent<Animator> ().SetBool ("buyusunMu", true);
			switch (seciliNesne.tag) {
			case "car":
				if (seciliNesne.GetComponent<myCarLights> ().grounded) {
					tipText.text = "E key to drive";		
				} else
					tipText.text = "R to recover the vehicle";		

				break;
			case "building":
				tipText.text = "E key to open the door";		
				break;
			case "item":
				tipText.text = "E key to pick the " + seciliNesne.name;		
				break;
			case "NPC":
				tipText.text = "E key to talk or help";		
				break;
			case "oturak":
				tipText.text = "E key to sit here";		
				break;
			case "drag":
				tipText.text = "Use left mouse to drag";		
				break;
			case "securityCamera":
				activateCamera ();		
				break;
			default:
				break;
			}

		} else
			fareImlec.GetComponent<Animator> ().SetBool ("buyusunMu", false);
	}

	GameObject getMouseHoverObject (float range) {		
		RaycastHit raycastHit;//neye çarptı
		if (!Physics.Raycast (kamera.ScreenPointToRay (Input.mousePosition), out raycastHit, range))
			return null;
		if (!raycastHit.transform.CompareTag ("Player")
		    && !GetComponent<gameControl> ().carDrivingMode
		    && !GetComponent<gameControl> ().oturmaMode
		    && (
		        raycastHit.transform.CompareTag ("car") ||
		        raycastHit.transform.CompareTag ("building") ||
		        raycastHit.transform.CompareTag ("NPC") ||
		        raycastHit.transform.CompareTag ("item") ||
		        raycastHit.transform.CompareTag ("drag") ||
		        raycastHit.transform.CompareTag ("securityCamera") ||
		        raycastHit.transform.CompareTag ("oturak")
		    )) {
			if (raycastHit.collider.gameObject.name == "ColliderTrigger") {
				//1 üst parent Lazım!
				return raycastHit.collider.gameObject.transform.parent.gameObject;	
			}
		}
		return null;
	}
}
