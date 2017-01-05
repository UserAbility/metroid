using UnityEngine;
using System.Collections;

namespace Generic.Move
{
	
public class MoveTowardsTarget : MonoBehaviour {

	//This is the object we are following
	[Tooltip("Drag the object to follow here")]
	public Transform Target;

	//The speed moving towards the object
	public float speed;

	//How close the target needs to be before it starts to follow
	[Tooltip("How close the target needs to be before it is followed")]
	public float maximumRange;

	//How close to the target it will get
	[Tooltip("How close it will get to the target before stopping")]
	public float minimumRange;


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		//This adjusts the speed based on the framefrate (also know as clockspeed) of the computer
		float step = speed * Time.deltaTime;//deltaTime is the difference in time between each frame. You could also desribe it as how long each frame takes.

		//If the target is close enough
		if (Vector3.Distance (this.transform.position, Target.position) <= maximumRange) 
		{
			//if the target is not too close
			if (Vector3.Distance (this.transform.position, Target.position) >= minimumRange) 
			{
				//Move towards the target
				this.transform.position = Vector3.MoveTowards (this.transform.position, Target.position, step);
			}
		}
	}
}

}