using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class basicAI : MonoBehaviour {
	public string playerName = "Ali";

	public Transform[] points;
	public GameObject code;
	public AudioClip[] childSounds;

	public float rotationSpeed = 10f;
	public bool aracaMiCarpti = false;
	public bool followPlayerMode = false;

	bool birKereSesCalis = false;

	private int destPoint = 0;
	private NavMeshAgent agent;

	GameObject player;

	void Start () {
		agent = GetComponent<NavMeshAgent> ();
		player = code.GetComponent<gerekliNesneler> ().player;
		//agent.autoBraking = false;

		GotoNextPoint ();
	}

	void GotoNextPoint () {
		// Returns if no points have been set up
		if (points.Length == 0)
			return;
		
		// Set the agent to go to the currently selected destination.
		agent.destination = points [destPoint].position;

		// Choose the next point in the array as the destination,
		// cycling to the start if necessary.
		destPoint = (destPoint + 1) % points.Length;

	}

	private void RotateTowards (Transform target) {
//		Vector3 direction = (target.position - transform.position).normalized;
//		Quaternion lookRotation = Quaternion.LookRotation (direction);
//		transform.rotation = Quaternion.Slerp (transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);

		Vector3 targetPostition = new Vector3 (target.position.x, 
			                          this.transform.position.y, 
			                          target.position.z);
		this.transform.LookAt (targetPostition);
	}

	void OnTriggerEnter (Collider other) {		
		if (this.tag == "NPC" && (other.tag == "car" || other.tag == "rock")
		    && !aracaMiCarpti) {
			//Debug.Log (other.gameObject.GetComponent<Rigidbody> ().velocity.magnitude * 3.6f);
			if (other.gameObject.GetComponent<Rigidbody> ().velocity.magnitude * 3.6f > 15f) {
				agent.isStopped = true;
				agent.GetComponent<Animator> ().SetBool ("dieArtik", true);
				//OUCH
				agent.GetComponent<AudioSource> ().PlayOneShot (childSounds [1]);
				code.GetComponent<gameControl> ().showSubtitle (playerName, "Ouch!", 2f);	
				aracaMiCarpti = true;	
			}
		}
	}

	void LateUpdate () {
		// Choose the next destination point when the agent gets
		// close to the current one.

		if (aracaMiCarpti) {
			return;
		}

		float distance = Vector3.Distance (player.transform.position, transform.position);

		if (distance <= 2f && !code.GetComponent<gameControl> ().carDrivingMode &&
		    !code.GetComponent<gameControl> ().oturmaMode) {
			//2 metre yakınında ajanı durdurup bize baktır
			agent.isStopped = true;
			if (!birKereSesCalis) {
				//HELLO
				if (!agent.GetComponent<AudioSource> ().isPlaying && !followPlayerMode) {
					agent.GetComponent<AudioSource> ().PlayOneShot (childSounds [0]);	
				}
				if (code && !followPlayerMode) {
					code.GetComponent<gameControl> ().showSubtitle (playerName, "Hello!", 2f);	
				}
				birKereSesCalis = true;
				agent.GetComponent<Animator> ().SetBool ("durArtik", true);
			}

			RotateTowards (player.transform);
			//agent.SetDestination (player.transform.position);
		} else {
			agent.isStopped = false;
			birKereSesCalis = false;
			agent.GetComponent<Animator> ().SetBool ("durArtik", false);
			if (followPlayerMode &&
			    !code.GetComponent<gameControl> ().carDrivingMode &&
			    !code.GetComponent<gameControl> ().oturmaMode) {
				agent.SetDestination (player.transform.position);
			} else if (!agent.pathPending && agent.remainingDistance < 0.5f)
				GotoNextPoint ();
		}

	}
}
