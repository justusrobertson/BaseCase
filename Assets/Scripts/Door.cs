using UnityEngine;
using System.Collections;

public class Door : MonoBehaviour 
{	
	private Animator animator;
	private bool locked;
	
	// Use this for initialization
	void Start () 
	{
		// Get the game object's animator.
		animator = this.GetComponent<Animator>();
		locked = true;
	}
	
	// Update is called once per frame
	void Update () 
	{
		// If the object has an animator...
		if (animator != null)
		{
			// Turn off warning logging in case we touch properties that don't exist.
			animator.logWarnings = false;
			
			if (animator.GetBool("locked"))
			{
				this.collider2D.isTrigger = false;
				this.gameObject.tag = "Door";
				if (!locked)
				{
					locked = true;
				}
			}
			else
			{
				this.collider2D.isTrigger = true;
				this.gameObject.tag = "DoorNav";
				if (locked)
				{
					locked = false;
				}
			}
			
			animator.logWarnings = true;
		}
	}
}
