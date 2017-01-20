using UnityEngine;
using System.Collections;

public class UI : MonoBehaviour 
{
	// The State Manager object used to answer state questions.
	private StateManager stateManager;
	
	private InventoryManager inventoryManager;

	// Use this for initialization
	void Start () 
	{
		// Find the level game object.
		GameObject level = GameObject.Find("Level");
		
		// Set the state manager.
		stateManager = level.GetComponent<StateManager>();
		
		inventoryManager = this.GetComponentInChildren<InventoryManager>();
	}
	
	// Update is called once per frame
	public void GenerateUI()
	{
		if (stateManager.IsInventory())
		{
			Camera.main.rect = new Rect (0, 0, 1, 0.75f);
			inventoryManager.CreateInventory();
		}
	}
}
