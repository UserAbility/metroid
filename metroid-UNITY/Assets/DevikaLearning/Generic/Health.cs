//by Brennan Hatton - May 2016 brennan@brennanhatton.com

using UnityEngine;
using System.Collections;
using UnityEngine.Events; //We want to calla  custom event from the Unity UI
using UnityEngine.UI; //We tells the game we want to talk to the UI (User Interface). This way we can talk to the Slider UI.

namespace Generic
{

public class Health : MonoBehaviour {

	public float health = 100;
	public Slider healthBar;
	public UnityEvent EventOnDeath;
	public float startHealth;
	
	void Awake()
	{
		startHealth = health;
	}
	
	public void Reset()
	{
		health = startHealth;
	}

	void Update(){

		//check if killing this player does something
		if (EventOnDeath != null)//if the objct has run out of health
		{
			if (IsAlive () == false) 
			{
				//Do something!
				EventOnDeath.Invoke();
			}
		}
		
		//If we are using a healthBar
		if (healthBar != null) {
			healthBar.value = health;
		}
	}

	public bool IsAlive()
	{
		//If the player has health
		if (health > 0)
			//it is alive!
			return true;

		//Once a return has been called, the function is ended
		//This will only be reached if the "return true;" above was not
		return false;
	}
}
}