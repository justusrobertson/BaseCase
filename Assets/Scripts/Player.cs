using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Handles player behavior and calls the mediator on transition events.
public class Player : MonoBehaviour 
{
	// The mediator object.
	private Mediator mediator;
	
	// The state manager object.
	private StateManager stateManager;
	
	// The inventory manager object.
	private InventoryManager inventoryManager;
	
	// Controls how fast the player's character moves.
	private float playerSpeed = 3f;
	
	// The player's animator.
	private Animator animator;
	
	// Controls whether the player can move their character.
	public bool canMove;
	
	// Game objects the player's character is colliding with.
	public List<GameObject> colliding;
	
	public bool disabled;
	
	// Use this for initialization
	void Start () 
	{
		// Find the mediator's game object.
		GameObject medOB = GameObject.Find("Mediator");
		
		// Store the mediator script.
		mediator = medOB.GetComponent<Mediator>();
		
		// Find the state manager's game object.
		GameObject smOB = GameObject.Find("Level");
		
		// Store the state manager's script.
		stateManager = smOB.GetComponent<StateManager>();
		
		// Find the inventory's game object.
		GameObject invOB = GameObject.Find("Inventory");
		
		// Save the inventory script.
		inventoryManager = invOB.GetComponent<InventoryManager>();
		
		// Store the animator.
		animator = this.GetComponent<Animator>();
		
		// Allow the player to move.
		canMove = true;
		
		// Create an empty list of colliding objects.
		colliding = new List<GameObject>();
		
		disabled = false;
	}
	
	// Update is called once per frame
	void Update () 
	{			
		// If it's the player's turn and the camera is not moving...
		if (stateManager.PlayerTurn && iTween.Count(Camera.main.gameObject) == 0)
		{
			// If the player pressed the 'take' button, can move, and is colliding with exactly one object...
			if (Input.GetButtonDown("Take") && canMove && colliding.Count == 1)
			{
				// Save the object the player is colliding with.
				GameObject taken = colliding[0];
				
				// If the mediator says the player can take the object...
				if (mediator.PlayerUpdate("(take-thing " + stateManager.Player + " " + taken.name + " " + stateManager.At(stateManager.Player) + ")"))
					// Remove the object from the set of colliding objects.
					colliding.Remove(taken);
			}
			
			// If the player pressed the 'drop' button...
			if (Input.GetButtonDown("Drop"))
			{
				// If the player's inventory has been initialized...
				if (stateManager.GetObjectsAt(stateManager.Player) != null)
					// If the player's inventory contains at least one object...
					if (stateManager.GetObjectsAt(stateManager.Player).Count > 0)
						// If the player has selected a valid inventory object...
						if (!inventoryManager.selected.Equals(""))
							// Ask the mediator if the player can drop the object.
							mediator.PlayerUpdate("(drop-thing " + stateManager.Player + " " + inventoryManager.selected + " " + stateManager.At(stateManager.Player) + ")");
			}
			
			if (Input.GetButtonDown("Open") && colliding.Count > 0)
			{
				// Store the object the player is colliding with.
				GameObject door = colliding[0];
				
				// If the player has selected a valid inventory object...
				if (!inventoryManager.selected.Equals(""))
					// Ask the mediator if the player can wake the object.
					if (mediator.PlayerUpdate("(open-door " + stateManager.Player + " " + inventoryManager.selected + " " + door.name + ")"))
						colliding = new List<GameObject>();
			}
			
			// If the player pressed the 'wake' button and is colliding with exactly one object...
			if (Input.GetButtonDown("Wake") && colliding.Count == 1)
			{
				// Store the object the player is colliding with.
				GameObject person = colliding[0];
				
				// Ask the mediator if the player can wake the object.
				mediator.PlayerUpdate("(wake-person " + stateManager.Player + " " + person.name + " " + stateManager.At(stateManager.Player) + ")");
			}
			
			// If the player pressed the 'untie' button and is colliding with exactly one object...
			if (Input.GetButtonDown("Untie") && colliding.Count == 1)
			{
				// Store the object the player is colliding with.
				GameObject person = colliding[0];
				
				// Ask the mediator if the player can wake the object.
				mediator.PlayerUpdate("(untie-person " + stateManager.Player + " " + person.name + " " + stateManager.At(stateManager.Player) + ")");
			}
			
			// If the player pressed the 'disenchant' button...
			if (Input.GetButtonDown("Disenchant"))
			{
				// If the player's inventory has been initialized...
				if (stateManager.GetObjectsAt(stateManager.Player) != null)
					// If the player's inventory is not empty and the player is colliding with one object...
					if (stateManager.GetObjectsAt(stateManager.Player).Count > 0 && colliding.Count == 1)
					{
						// Store the object the player is colliding with.
						GameObject item = colliding[0];
						
						// If the player has selected a valid inventory item...
						if (!inventoryManager.selected.Equals(""))
							// Ask the mediator to disenchant the object with the selected item.
							mediator.PlayerUpdate("(disenchant-thing " + stateManager.Player + " " + item.name + " " + inventoryManager.selected + " " + stateManager.At(stateManager.Player) + ")");
					}
			}
			
			// If the player is sending movement input and can move...	
			if (Mathf.Abs(Input.GetAxis("Horizontal")) + Mathf.Abs(Input.GetAxis("Vertical")) > 0 && canMove)
			{			
				// Remember that the player is moving.
				animator.SetBool("Moving", true);
				
				// If the player is moving horizontally...
				if (Mathf.Abs(Input.GetAxis("Horizontal")) > Mathf.Abs(Input.GetAxis("Vertical")))
				{
					// If the horizontal movement is greater than zero...
					if (Input.GetAxis("Horizontal") > 0) 
					{
						// Move the player.
						rigidbody2D.MovePosition(rigidbody2D.position + new Vector2 (playerSpeed, 0) * Time.deltaTime);
						
						// Animate the player.
						animator.SetInteger("Direction", 1);
					}
					// Otherwise, if it's less than zero...
					else if (Input.GetAxis("Horizontal") < 0)
					{
						// Move the player.
						rigidbody2D.MovePosition(rigidbody2D.position + new Vector2 (playerSpeed * -1, 0) * Time.deltaTime);
						
						// Animate the player.
						animator.SetInteger("Direction", 3);
					}
				}
				// Otherwise, if the player is moving vertically...
				else
				{
					// If the vertical movement is greater than zero...
					if (Input.GetAxis("Vertical") > 0) 
					{
						// Move the player.
						rigidbody2D.MovePosition(rigidbody2D.position + new Vector2 (0, playerSpeed) * Time.deltaTime);
						
						// Animate the player.
						animator.SetInteger("Direction", 0);
					}
					// Otherwise, if the vertical movement is less than zero...
					else if (Input.GetAxis("Vertical") < 0)
					{
						// Move the player.
						rigidbody2D.MovePosition(rigidbody2D.position + new Vector2 (0, playerSpeed * -1) * Time.deltaTime);
						
						// Animate the player.
						animator.SetInteger("Direction", 2);
					}
				}
			}
			// Otherwise, if the player is not moving...
			else
				// Remember the player is not moving.
				animator.SetBool("Moving", false);
		}
		// Otherwise...
		else
			// Remember the player is not moving.
			animator.SetBool("Moving", false);
	}
	
	public void Disable()
	{
		disabled = true;
		canMove = false;
	}
	
	public void Enable()
	{
		disabled = false;
		canMove = true;
	}
	
	// Fires on a collision.
	void OnCollisionEnter2D (Collision2D col)
	{
		// If the collision object is tagged as an item and we have not already recorded it...
		if ((col.gameObject.tag.Equals("Item") || col.gameObject.tag.Equals("Door")) && !colliding.Contains(col.gameObject))
			// Add the object to our list.
			colliding.Add(col.gameObject);
			
		if (col.gameObject.tag.Equals("Entrance"))
		{
			string exit = stateManager.GetExit(col.gameObject.name);
			mediator.PlayerUpdate("(enter-location " + stateManager.Player + " " + col.gameObject.name + " " + exit + " " + stateManager.At(stateManager.Player) + ")");
			Disable();
		}
	}
	
	// Fires on collision exit.
	void OnCollisionExit2D (Collision2D col)
	{
		// If we were colliding with a game object...
		if (colliding.Contains(col.gameObject))
			// Remove it from our list.
			colliding.Remove(col.gameObject);
	}
}
