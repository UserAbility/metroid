//by Brennan Hatton - May 2016 brennan@brennanhatton.com

using UnityEngine;
using System.Collections;

namespace Generic.Move
{

public class MoveDirection : MonoBehaviour {

	//Thi holds the direction and speed you want object to move
	public Vector3 Direction;

	Rigidbody rigidbody;

	void Start()
	{
		//Get the rigidbody component
		rigidbody = this.GetComponent<Rigidbody> ();

		//If there is no rigibody componet
		if (rigidbody == null) {

			Debug.LogWarning ("PlayerMovePlatformer component requires Ridigbody on " + this.gameObject.name + ". One was added");

			//add the rigidbody component
			rigidbody = this.gameObject.AddComponent<Rigidbody> ();
		}
	}

	// Update is called once per frame
	void Update () {
	
		//Add the movement Direction to the curret position. Moving it in that direction
		rigidbody.velocity = Direction;

	}
}
}
