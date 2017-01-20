using UnityEngine;
using System.Collections;

// A cursor for the inventory.
public class Cursor : MonoBehaviour 
{
	private InventoryManager inv;
	
	// Use this for initialization
	void Start () 
	{
		GameObject invGO = GameObject.Find("Inventory");
		inv = invGO.GetComponent<InventoryManager>();
	}	
	
	void OnTriggerEnter2D (Collider2D coll)
	{
		inv.selected = coll.gameObject.name;
	}
	
	void OnTriggerExit2D (Collider2D coll)
	{
		if (coll.gameObject.name.Equals(inv.selected))
			inv.selected = "";
	}
}
