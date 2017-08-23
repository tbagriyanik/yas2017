using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class autoOnOffLight : MonoBehaviour {

	public float onTime = 4f;
	public float offTime = 2f;

	private float timer;
	private Light myLight;
	public bool onOff = false;

	void Start () {
		myLight = GetComponent<Light> ();
	}

	void Update () {
		timer += Time.deltaTime;
		if (timer > onTime && onOff) {
			timer = 0;
			onOff = false;
			myLight.enabled = false;
		} else if (timer > offTime && !onOff) {
			timer = 0;
			onOff = true;
			myLight.enabled = true;
		}
	}
}
