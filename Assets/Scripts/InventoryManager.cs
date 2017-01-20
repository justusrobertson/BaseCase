using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Manages the inventory game objects.
public class InventoryManager : MonoBehaviour 
{
	private StateManager stateManager;
	private GameObject cursor;
	
	private Vector3 unit;
	private Vector2 screenSize;
	
	public string selected;
	private int position;
	private bool pressed;
	
	// Use this for initialization
	void Start () 
	{
		
	}
	
	public void CreateInventory()
	{
		GameObject smGO = GameObject.Find("Level");
		stateManager = smGO.GetComponent<StateManager>();
		
		Vector3 topLeft = Camera.main.ScreenToWorldPoint(new Vector3(0,Camera.main.pixelHeight,Camera.main.farClipPlane));
		Vector3 bottomRight = Camera.main.ScreenToWorldPoint(new Vector3(Camera.main.pixelWidth,0,Camera.main.farClipPlane));
		screenSize = new Vector2 (Mathf.Abs(topLeft.x - bottomRight.x), Mathf.Abs(topLeft.y - bottomRight.y));
		GameObject rock = Instantiate(Resources.Load("Rock", typeof(GameObject))) as GameObject;
		unit = rock.renderer.bounds.size;
		Object.Destroy(rock);
		
		selected = "";
		
		cursor = Instantiate(Resources.Load("Cursor", typeof(GameObject)), new Vector3 (transform.position.x - (screenSize.x / 2) + unit.x, transform.position.y, 0), Quaternion.identity) as GameObject;
		
		position = 1;
		pressed = false;
	}
	
	// Update is called once per frame
	void Update () 
	{
		List<GameObject> objs = stateManager.GetObjectsAt(stateManager.Player);
		if (objs != null)
			if (position > objs.Count && objs.Count != 0)
			{
				position--;
				cursor.transform.position = new Vector3 (cursor.transform.position.x - unit.x, cursor.transform.position.y, 0);
			}
		
		if (Input.GetAxis("Inventory") > 0 && !pressed) 
		{
			if (objs != null)
				if (position < objs.Count)
				{
					position++;
					cursor.transform.position = new Vector3 (cursor.transform.position.x + unit.x, cursor.transform.position.y, 0);
				}
			pressed = true;
		}
		else if (Input.GetAxis("Inventory") < 0 && !pressed)
		{
			if (position > 1)
			{
				position --;
				cursor.transform.position = new Vector3 (cursor.transform.position.x - unit.x, cursor.transform.position.y, 0);
			}
			pressed = true;
		}
		else if (Input.GetAxisRaw("Inventory") == 0)
		{
			pressed = false;
		}
	}
}
