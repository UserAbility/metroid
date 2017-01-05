using UnityEngine;
using System.Collections;

//[AddComponentMenu("Devika/Transform/LockTransform")] //this addes the compent to the Add Component Menu
public class LockTransform : MonoBehaviour {

	//Locks the position
	public bool lockPosition = false;

	//Locks the rotation
	public bool lockRotation = true;

	//Do we save the local rotation to lock, or global?
	public bool SaveLocalRotation = true;

	//The starting transform rotation
	Quaternion startRotation;

	//The starting transform postiion
	Vector3 startPosition;

	void Start()
	{

		//If we save the local rotation to lock
		if (SaveLocalRotation) {
			
			//Save the starting rotation as local
			startRotation = this.transform.localRotation;
		} else {

			//Save the starting rotation as global
			startRotation = this.transform.rotation;
		}

		//Save the starting position
		startPosition = this.transform.position;
	}
	
	// Update is called once per frame
	void Update () {

		//if we are locking the rotation
		if (lockRotation) 
		{
			//set rotation to start rotation
			this.transform.rotation = startRotation;
		}

		//if we are locking the postiion
		if (lockPosition) 
		{
			//set the position to starting position
			this.transform.position = startPosition;
		}
	}
}
