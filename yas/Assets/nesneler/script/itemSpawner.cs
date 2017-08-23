using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class itemSpawner : MonoBehaviour {

	public int delay = 5;
	//seconds

	public Transform[] items;
	public GameObject spawnParent;
	public bool randomPlace = true;
	public int spawnCount = 1;
	public bool spawnContinuously = true;

	// Use this for initialization
	void Start () {
		StartCoroutine ("delayCall");
	}

	IEnumerator  delayCall () {
		Transform newObject;
		GameObject randomObject;
		for (int i = 0; i < spawnCount; i++) {
			randomObject = items [Random.Range (0, items.Length)].gameObject;

			Vector3 position;

			if (randomPlace) {
				position = new Vector3 (Random.value / 120 * transform.position.x, 
					transform.position.y, 
					Random.value / 120 * transform.position.z);				
				position = transform.TransformPoint (position);
			} else {
				position = new Vector3 (transform.position.x, 
					transform.position.y, 
					transform.position.z);				
			}

			newObject = Instantiate (randomObject.transform, position, transform.rotation);
		
			newObject.SetParent (spawnParent.transform);
			newObject.gameObject.name = randomObject.name;	
		}
		yield return new WaitForSeconds (delay);
		if (spawnContinuously) {
			StartCoroutine ("delayCall");	
		}
	}

}
