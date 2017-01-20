using UnityEngine;
using System.Collections;

public class Navigation : MonoBehaviour 
{
	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}
	
	void OnTriggerEnter2D(Collider2D other)
	{
		if (other.tag != "NPC")
			this.tag = "DisabledNavigation";
	}
	
	void OnTriggerExit2D(Collider2D other)
	{
		if (other.tag != "NPC")
			this.tag = "Navigation";
	}
}
