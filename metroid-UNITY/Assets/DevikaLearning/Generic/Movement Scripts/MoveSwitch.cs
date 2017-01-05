using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Generic.Move
{

public class MoveSwitch : MonoBehaviour {

		[System.Serializable]
		public class FinishMoving
		{
			public float time;
			public int distance;
			public GameObject target;

		}

		public Dictionary<MoveDirection,int> Direction;
		public MoveBetweenPoints[] BetweenPoints;
		public MoveInCircle[] InCircle;
		public MoveTowardsTarget[] TowardsTarget;
		public MoveTurnFromWalls[] TurnFromWalls;

		[Tooltip("Target self if left blank")]
		public GameObject Target;
		public bool TargetParent;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}

}