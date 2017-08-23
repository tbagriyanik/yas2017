using UnityEngine;
using System.Collections;

public class TakeScreenshot : MonoBehaviour {
	private int screenshotCount = 0;
	public string screenKey = "f9";
	GameObject player;

	// Use this for initialization
	void Start () {
		player = GetComponent<gerekliNesneler> ().player;
	}

	// Check for screenshot key each frame
	void LateUpdate () {
		// take screenshot on up->down transition of F9 key
		if (Input.GetKeyDown (screenKey)) {        
			string screenshotFilename;
			do {
				screenshotCount++;
				screenshotFilename = "screenshot " + screenshotCount +
				System.DateTime.UtcNow.ToString (" dd MM yyyy HH mm") + ".png";				
			} while (System.IO.File.Exists (screenshotFilename));
 
			ScreenCapture.CaptureScreenshot (screenshotFilename);
			player.GetComponent<AudioSource> ().PlayOneShot (GetComponent<gameControl> ().gameSounds [5]); // click sesi
		}
	}
}