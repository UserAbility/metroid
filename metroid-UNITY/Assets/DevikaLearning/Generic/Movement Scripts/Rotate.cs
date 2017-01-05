using UnityEngine;
using System.Collections;

namespace Generic.Move
{

public class Rotate : MonoBehaviour {

	public Vector3 RotationSpeed;

	void Update()
	{
		this.transform.Rotate (RotationSpeed);
	}
}

}