using UnityEngine;
using System.Collections;

public class DeactivateAterTime : MonoBehaviour {
	
	public float time;
	
	float timeAlive;
	
	public bool ResetOnEnable = false;
	
	public void OnEnable()
	{
		if(ResetOnEnable)
			Reset();
	}
	
	// Use this for initialization
	void Start () {
		timeAlive = 0;
	}
	
	public void Reset()
	{
		timeAlive = 0;
	}
	
	// Update is called once per frame
	void Update () {
		timeAlive += Time.deltaTime;
		
		if (timeAlive >= time)
			gameObject.SetActive(false);
	}
}
