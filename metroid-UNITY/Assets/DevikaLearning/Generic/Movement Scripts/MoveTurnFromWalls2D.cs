using UnityEngine;
using System.Collections;

namespace Generic.Move
{

public class MoveTurnFromWalls2D : MonoBehaviour {

	//Speed object moves
	public float speed = 3;

	//reference to ridigbody
	Rigidbody2D rigidbody;

	//
	RectTransform rt;

	//has it turned around?
	bool forward = true;

	// Use this for initialization
	void Start () {

		//Get the rigidbody component
		rigidbody = this.GetComponent<Rigidbody2D> ();

		//If there is no rigibody componet
		if (rigidbody == null)

			//add the rigidbody component
			rigidbody = this.gameObject.AddComponent<Rigidbody2D> ();


		rt = (RectTransform)transform;
	}

	// Update is called once per frame
	void Update () {
		
		//A raycast is like pointing out a lazer from the provided position, in the provided direction for the provided distance and seeing it if hits anything.
		RaycastHit2D hit;

		//If we are moving forward
		if (forward == true)
		{
			//Keep moving forward
			rigidbody.velocity = Vector2.right*speed;
		
			//
			hit = Physics2D.Raycast(new Vector2(transform.position.x,transform.position.y) , Vector2.right, rt.rect.width/2+0.1f, (LayerMask)1);
		}
		else
		{
			//move backward
			rigidbody.velocity = Vector2.left*speed;

			//A raycast is like pointing out a lazer from the provided position (transform.position), in the provided direction (-Vector3.up) for the provided distance (0.5f) and seeing it if hits anything.
			hit = Physics2D.Raycast(new Vector2(transform.position.x,transform.position.y) , Vector2.left, rt.rect.width/2+0.1f, (LayerMask)1);
		}

		//If we hit a wall
		if (hit.collider != null)
		{
			//Turn the object around
			if (forward == true)
				forward = false;
			else
				forward = true;
		}

			

	}
}

}