//by Brennan Hatton - May 2016 brennan@brennanhatton.com

using UnityEngine;
using System.Collections;

namespace Generic.Move
{

public class MoveInCircle : MonoBehaviour {

	[Tooltip("The speed each axis is rotated around")]
	public Vector3 RotationSpeed;

	[Tooltip("If this is checked, set a raduis")]
	public bool RotateAroundSelf;

	[Tooltip("A point for be the center of the rotation. Only use if not rotating around self.")]
	public Transform CenterPoint;

	[Tooltip("The size of the cirlce when rotating around self")]
	public float Radius;


	// Use this for initialization
	void Start () {

		//If rotating around self
		if (RotateAroundSelf) {
			
			//Make the center point of the circle the position the object currently is
			CenterPoint = this.transform;

			//Now move the object away from the circle center based on the size of the circle.
			this.transform.position = CenterPoint.position + RotationSpeed.normalized * Radius;
		}

	}
	
	// Update is called once per frame
	void Update () {

		//Apply rotation for x axis.
		if (RotationSpeed.x > 0)
			this.transform.RotateAround (CenterPoint.position, Vector3.left, RotationSpeed.x * Time.deltaTime);

		//Apply rotation for y axis.
		if (RotationSpeed.y > 0)
			this.transform.RotateAround (CenterPoint.position, Vector3.up, RotationSpeed.y * Time.deltaTime);

		//Apply rotation for z axis.
		if (RotationSpeed.z > 0)
			this.transform.RotateAround (CenterPoint.position, Vector3.forward, RotationSpeed.z * Time.deltaTime);
	}
}

}