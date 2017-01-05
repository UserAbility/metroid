//by Brennan Hatton - May 2016 brennan@brennanhatton.com

using UnityEngine;
using System.Collections;

namespace Generic
{

public class DestroyAfterTime : MonoBehaviour {

	//Starting time the object has to live
	public float lifeTime = 1;

	//How much time left until we kill it
	float timeLeft;

	// Use this for initialization
	void Start () {
		timeLeft = lifeTime;
	}
	
	// Update is called once per frame
	void Update () {

		//if there is no time left
		if (timeLeft <= 0)

			//Kill the object
			Destroy (this.gameObject);

		//update timer
		timeLeft -= Time.deltaTime;
	}
}
}