using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class randomIdleAnim : StateMachineBehaviour {
	public int animCount = 5;

	// OnStateEnter is called before OnStateEnter is called on any state inside this state machine
	//override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}

	// OnStateUpdate is called before OnStateUpdate is called on any state inside this state machine
	override public void OnStateUpdate (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		if (!animator.GetBool ("durArtik")) {
			animator.Play ("MOVE"); //hem run hem de walk
		}
		if (animator.GetBool ("dieArtik")) {
			animator.SetBool ("durArtik", false);
			animator.Play ("dieAnims"); //öldü
		}
	}

	// OnStateExit is called before OnStateExit is called on any state inside this state machine
	//override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}

	// OnStateMove is called before OnStateMove is called on any state inside this state machine
	//override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}

	// OnStateIK is called before OnStateIK is called on any state inside this state machine
	//override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}

	// OnStateMachineEnter is called when entering a statemachine via its Entry Node
	override public void OnStateMachineEnter (Animator animator, int stateMachinePathHash) {
		int random = Random.Range (0, animCount);
		animator.SetInteger ("idleAnimID", random);
	}

	// OnStateMachineExit is called when exiting a statemachine via its Exit Node
	//	override public void OnStateMachineExit (Animator animator, int stateMachinePathHash) {
	//
	//	}
}
