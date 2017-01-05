using UnityEngine;
using System.Collections;

namespace Generic.Move
{
	
public class MoveBetweenPoints : MonoBehaviour {

	//The two objects it is moving towards
	public Transform transform1, transform2;

	//The speed it moves
	public float speed;

	//The directoin it is mvoing
	public bool forward;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

		//This adjusts the speed based on the framefrate (also know as clockspeed) of the computer
		float step = speed * Time.deltaTime;//deltaTime is the difference in time between each frame. You could also desribe it as how long each frame takes.

		//if we are moving forward
		if (forward) {
			//move towards transform1
			transform.position = Vector3.MoveTowards (this.transform.position, transform1.position, step);
		} else {
			//Move towards transform2
			transform.position = Vector3.MoveTowards (this.transform.position, transform2.position, step);
		}
	}
}

}