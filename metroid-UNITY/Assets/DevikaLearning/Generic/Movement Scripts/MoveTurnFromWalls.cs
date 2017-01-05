using UnityEngine;
using System.Collections;

namespace Generic.Move
{

public class MoveTurnFromWalls : MonoBehaviour {

	//Speed object moves
	public float speed = 3;

	//reference to ridigbody
	Rigidbody rigidbody;

	//has it turned around?
	bool forward = true;

	// Use this for initialization
	void Start () {

		//Get the rigidbody component
		rigidbody = this.GetComponent<Rigidbody> ();

		//If there is no rigibody componet
		if (rigidbody == null)

			//add the rigidbody component
			rigidbody = this.gameObject.AddComponent<Rigidbody> ();
		
	}

	// Update is called once per frame
	void Update () {
		
		//If we hit a wall
		if (rigidbody.velocity.magnitude <= speed - rigidbody.drag)
		{
			//Turn the object around
			if (forward == true)
				forward = false;
			else
				forward = true;
		}

		//IF we are moving forward
		if (forward == true)
			//Keep moving forward
			rigidbody.velocity = Vector3.left*speed;
		else
			//move backward
			rigidbody.velocity = Vector3.left*speed * -1;
			

	}
}

}