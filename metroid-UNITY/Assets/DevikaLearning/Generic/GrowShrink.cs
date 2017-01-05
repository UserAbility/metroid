//by Brennan Hatton - May 2016 brennan@brennanhatton.com

using UnityEngine;
using System.Collections;

namespace Generic
{

public class GrowShrink : MonoBehaviour {

	//Maximum size reached
	public Vector3 maxSize = Vector3.one * 2;

	//Minimum size reached
	public Vector3 minSize = Vector3.one;

	//Speed it changes size
	public float speed = 1;

	//is it currently growing or shrinking
	bool growing = true;

	float startTime;
	float journeyLength;

	void Start()
	{
		startTime = Time.time;
		journeyLength = Vector3.Distance(maxSize, minSize);
		this.transform.localScale = minSize;
	}
	
	// Update is called once per frame
	void Update () {
		
		float distCovered = (Time.time - startTime) * speed;
		float fracJourney = distCovered / journeyLength;

		if (growing) 
		{
			if (this.transform.localScale.x < maxSize.x)
				this.transform.localScale = Vector3.Lerp (minSize, maxSize, fracJourney);
			else {
				startTime = Time.time;
				growing = false;
			}
		} 
		else 
		{

			if (this.transform.localScale.x > minSize.x)
				this.transform.localScale = Vector3.Lerp (maxSize, minSize, fracJourney);
			else{
				startTime = Time.time;
				growing = true;
			}
		}
	
	}
}
}