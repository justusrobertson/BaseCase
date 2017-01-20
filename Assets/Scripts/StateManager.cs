using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Mediation.PlanTools;

// Instantiates and manages Unity game objects based on the underlying predicate state from GME.
public class StateManager : MonoBehaviour 
{
	// The current state of the world from GME.
	private List<Predicate> predicates;
	
	// The last state of the world from GME.
	// A hack to make agent property updates work until I can make GME asynchronous.
	private List<Predicate> lastPredicates;
	
	// A table of Unity game objects, stored by location.
	private Hashtable objects = new Hashtable();
	
	// A table of enabled object properties, stored by object.
	private Hashtable properties = new Hashtable();
	
	// Tracks whether it's the player's turn to act.
	public bool playerTurn = true;
	
	// The relative size of the screen, for inventory management.
	private Vector2 screenSize;
	
	// The unit size of our sprites.
	private Vector3 unit;
	
	// An interface for the player's turn variable.
	public bool PlayerTurn
	{
		get { return playerTurn; }
		set { playerTurn = value; }
	}
	
	// An interface for the current world state predicates.
	public List<Predicate> Predicates
	{
		get { return predicates; }
		set 
		{ 
			// A hack to make agent property updates work until I can make GME asynchronous.
			if (predicates != null)
				lastPredicates = predicates;
			else
				lastPredicates = new List<Predicate>();
				
			predicates = value; 
		}
	}
	
	// Exposes the GetPlayer() method.
	public string Player
	{
		get { return GetPlayer(); }
	}

	// Exposes the GetLocations() method.
	public List<string> Locations
	{
		get { return GetLocations(); }
	}
	
	void Start()
	{
		// ARE YOU READY FOR SOME RELATIVE SCREEN POSITIONING? I HOPE SO!
		// Here we go...
		
		// Find the top left corner of the screen.
		Vector3 topLeft = Camera.main.ScreenToWorldPoint(new Vector3(0,Camera.main.pixelHeight,Camera.main.farClipPlane));
		
		// Find the bottom right corner of the screen.
		Vector3 bottomRight = Camera.main.ScreenToWorldPoint(new Vector3(Camera.main.pixelWidth,0,Camera.main.farClipPlane));
		
		// Calculate the screen size from the top left and bottom right corner positions.
		screenSize = new Vector2 (Mathf.Abs(topLeft.x - bottomRight.x), Mathf.Abs(topLeft.y - bottomRight.y));
		
		// Instantiate a prefab to store information about its size.
		GameObject rock = Instantiate(Resources.Load("Rock", typeof(GameObject))) as GameObject;
		
		// Store the size of the prefab we instantiated.
		unit = rock.renderer.bounds.size;
		
		// Destroy the prefab we instantiated to store the tile unit size.
		Object.Destroy(rock);
	}
	
	// Updates the Unity scene to match the underlying predicate state description.
	public void Refresh ()
	{
		// Syncronize Unity game objects with predicate's state.
		RefreshObjects();
		
		// Syncronize the Unity player inventory with predicate's state.
		RefreshInventory();
		
		// Syncronize Unity game object properties with predicate's state.
		RefreshProperties();
	}
	
	// Updates Unity game objects to match the predicate description.
	public void RefreshObjects ()
	{
		// Store the room the player is currently at.
		//string location = At(Player);

		foreach (string location in GetLocations())
		{
			// Store a list of game objects that are colocated with the player in the Unity scene.
			List<GameObject> locationObjects = GetObjectsAt(location);
			
			// Store a list of names of objects that are colocated with the player in the predicate state.
			List<string> things = ThingsAt(location);
			
			// Create a list of Unity objects to remove from the scene.
			List<GameObject> remove = new List<GameObject>();
			
			// If there are objects colocated with the player.
			if (locationObjects != null)
			{
				// Loop through the colocated objects.
				foreach (GameObject obj in locationObjects)
				{
					// If the current object is not in the underlying predicate state...
					if (!things.Contains(obj.name))
						// Add the object to the list of those to remove.
						remove.Add(obj);
						
					if (obj.tag.Equals("Player"))
					{
						Player script = obj.GetComponent<Player>();
						if (script.disabled)
							remove.Add(obj);
					}
				}
				
				// Loop through the objects to be removed from the scene.
				foreach (GameObject obj in remove)
				{
					// Remove the object from the list of location objects.
					locationObjects.Remove(obj);
					
					// Destory the object.
					Object.Destroy(obj);
				}
			}
			
			// If there are objects colocated with the player in the predicate state...
			if (things != null)
				// Loop through the set of colocated objects.
				foreach (string thing in things)
					// If there are objects colocated with the player in the Unity scene...
					if (locationObjects != null)
					{
						// If the current predicate object is not in the Unity scene...
						if (locationObjects.Find(x => x.name.Equals(thing)) == null)
							// Instantiate the current predicate object as a Unity game object colocated with the player.
							locationObjects.Add(InstantiateAt(thing, location));
					}
					else
					{
						// Create a new list of game objects.
						locationObjects = new List<GameObject>();
						
						// Instantiate the current predicate object as a Unity game object colocated with the player.
						locationObjects.Add(InstantiateAt(thing, location));
					}
			
			// Associate the current location objects with their location.
			SetObjectsAt(location, locationObjects);
		}
	}
	
	// Updates Unity game object properties to match the predicate description.
	public void RefreshProperties ()
	{		
		// Loop through the objects colocated with the player in the current predicate state.
		foreach (string thing in AllThings())
		{
			// Store the current object's properties from the predicate state.
			List<string> currentProperties = Properties(thing);
			
			// Store the current object's stored properties from the last update.
			List<string> propertiesHash = properties[thing] as List<string>;
			
			// A list of properties to add to the object.
			List<string> newProperties = new List<string>();
			
			// A list of properties to remove from the object.
			List<string> oldProperties = new List<string>();
			
			// If the object has previously stored properties...
			if (propertiesHash != null)
			{
				// Loop through each stored property.
				foreach (string property in propertiesHash)
					// If the property does not persist in the current state...
					if (!currentProperties.Contains(property))
						// Add the property to the remove list.
						oldProperties.Add(property);
						
				// Loop through each current property.
				foreach (string property in currentProperties)
					// If the property is new...
					if (!propertiesHash.Contains(property))
						// Add the property to the add list.
						newProperties.Add(property);
			}
			// Otherwise, if there are no stored properties...
			else
				// Add all current properties to the add list.
				newProperties = currentProperties;
			
			// Find the Unity game object that corresponds with the current object.
			List<GameObject> thingGOs = new List<GameObject>();
			
			if (!Type(thing).Equals("door"))
				thingGOs.Add(GameObject.Find(thing));
			else
			{
				foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Door"))
					if (obj.name.Equals(thing))
						thingGOs.Add(obj);
				
				foreach (GameObject obj in GameObject.FindGameObjectsWithTag("DoorNav"))
					if (obj.name.Equals(thing))
						thingGOs.Add(obj);
			}
			
			foreach (GameObject thingGO in thingGOs)
			{				
				if (thingGO != null)
				{
					// Get the game object's animator.
					Animator animator = thingGO.GetComponent<Animator>();
					
					// If the object has an animator...
					if (animator != null)
					{
						// Turn off warning logging in case we touch properties that don't exist.
						animator.logWarnings = false;
						
						// Loop through the new properties.
						foreach (string property in newProperties)
							// Set the new properties to true.
							animator.SetBool(property, true);
							
						// Loop through the old properties.
						foreach (string property in oldProperties)
							// Set the old properties to false.
							animator.SetBool(property, false);
							
						animator.logWarnings = true;
					}
				}
			}
			
			// Store the object's current properties in the hashtable.
			properties[thing] = currentProperties;
		}
	}
	
	// Updates Unity's player inventory to match the predicate description.
	public void RefreshInventory ()
	{
		// Store a list of the current Unity player inventory.
		List<GameObject> inventoryObjects = GetObjectsAt(Player);
		
		// Store a list of the predicate player inventory.
		List<string> things = Has(Player);
		
		// Create a list of objects to remove from Unity's player inventory.
		List<GameObject> remove = new List<GameObject>();
		
		// If the inventory is not empty...
		if (inventoryObjects != null)
		{
			// Loop through the game objects in Unity's inventory.
			foreach (GameObject obj in inventoryObjects)
				// If the current object is not in the predicate list...
				if (!things.Contains(obj.name))
					// Add the object to the remove list.
					remove.Add(obj);
			
			// Loop through the game objects in the remove list.
			foreach (GameObject obj in remove)
			{
				// Remove the object from Unity's player inventory.
				inventoryObjects.Remove(obj);
				
				// Destory the object.
				Object.Destroy(obj);
			}
		}
		
		// If the player has at least one thing in their inventory in the predicate state...
		if (things != null)
			// Loop through the things the player is carrying in the predicate state.
			for (int i = 0; i < things.Count; i++)
				// If the player has at least one thing in their Unity inventory...
				if (inventoryObjects != null)
				{
					// If the current predicate object is not in the user's Unity inventory...
					if (inventoryObjects.Find(x => x.name.Equals(things[i])) == null)
						// Instantiate the object and add it to the inventory list.
						inventoryObjects.Add(InstantiateInventoryItem(things[i], i));
					// Otherwise, if the object is already instantiated...
					else
					{
						// Find the inventory game object.
						GameObject chunk = GameObject.Find("Inventory");
						
						// Find the current inventory item game object.
						GameObject thingGO = inventoryObjects.Find(x => x.name.Equals(things[i]));
						
						// Change the inventory item's position.
						thingGO.transform.position = new Vector3 (chunk.transform.position.x - (screenSize.x / 2) + (unit.x * (i + 1)), chunk.transform.position.y, 0);
					}
				}
				// Otherwise, if the player has nothing in their inventory...
				else
				{
					// Initialize the Unity inventory list.
					inventoryObjects = new List<GameObject>();
					
					// Instantiate the object and add it to the inventory list.
					inventoryObjects.Add(InstantiateInventoryItem(things[i], i));
				}
		
		// Store the inventory objects under the player's name.
		SetObjectsAt(Player, inventoryObjects);
	}
	
	// Returns the location an object is at in the current predicate state.
	public string At (string thing)
	{
		// Loop through the current predicates.
		foreach (Predicate pred in predicates)
			// If the predicate name is 'at'...
			if (pred.Name.Equals("at"))
				// If the first term is equal to the object...
				if (pred.TermAt(0).Equals(thing))
					// Return the second predicate term.
					return pred.TermAt(1);
		
		// We couldn't find what we were looking for.
		return "";
	}
	
	// Returns a list of all inventory items of a character in the current predicate state.
	public List<string> Has (string character)
	{
		// Create a new list to hold the items.
		List<string> inventory = new List<string>();
		
		// Loop through the current predicates.
		foreach (Predicate pred in predicates)
			// If the predicate name is 'has'...
			if (pred.Name.Equals("has"))
				// If the first term is the character...
				if (pred.TermAt(0).Equals(character))
					// Add the second term of the predicate to the list.
					inventory.Add(pred.TermAt(1));
		
		if (inventory != null)
			inventory = inventory.OrderBy(o => o).ToList();
		
		// Return the inventory list.
		return inventory;
	}

	// Returns the game object type of a predicate state object.
	public string Type (string thing)
	{
		// Loops through the current predicates.
		foreach (Predicate pred in predicates)
			// If the predicate name is 'type'...
			if (pred.Name.Equals("type"))
				// If the first term is the object...
				if (pred.TermAt(0).Equals(thing))
					// Return the second term.
					return pred.TermAt(1);
        
		// We couldn't find what we were looking for.
        return "";
	}
	
	// Returns a list of properties of a predicate state object.
	public List<string> Properties (string thing)
	{
		// Create a new list.
		List<string> properties = new List<string>();
		
		// Loop through the current predicates.
		foreach (Predicate pred in predicates)
			// If the predicate's first term equals the object...
			if (pred.TermAt(0).Equals(thing))
				// If the predicate has an arity of one...
				if (pred.Arity == 1)
					// Add the predicate's name to the list.
					properties.Add(pred.Name);
		
		// Return the list of properties.
		return properties;
	}
	
	// Return a list of all things at a location in the predicate state.
	public List<string> ThingsAt (string room)
	{
		// Make a new list.
		List<string> things = new List<string>();
		
		// Loop through the predicates.
		foreach (Predicate pred in predicates)
			// If the predicate's name is 'at'...
			if (pred.Name.Equals("at"))
			{
				// If the second term is equal to the location...
				if (pred.TermAt(1).Equals(room))
					// Add the first term to the list.
					things.Add(pred.TermAt(0));
			}
			else if (pred.Name.Equals("between"))
			{
				// If the second term is equal to the location...
				if (pred.TermAt(1).Equals(room))
					// Add the first term to the list.
					things.Add(pred.TermAt(0));
			}
					
		// Return the list of objects.
		return things;
	}
	
	// Return a list of all things at a location in the current and last predicate state.
	// Kind of a hack because I need to make GME asynchronous.
	public List<string> ThingsAtCurrentAndLast (string room)
	{
		// Make a new list.
		List<string> things = new List<string>();
		
		// Create a combined list of predicates.
		List<Predicate> allPreds = new List<Predicate>();
		
		foreach (Predicate pred in predicates)
			allPreds.Add(pred);
		
		foreach (Predicate pred in lastPredicates)
			allPreds.Add(pred);
		
		// Loop through the predicates.
		foreach (Predicate pred in allPreds)
			// If the predicate's name is 'at'...
			if (pred.Name.Equals("at"))
			{
				// If the second term is equal to the location...
				if (pred.TermAt(1).Equals(room))
					// Add the first term to the list.
					things.Add(pred.TermAt(0));
			}
			else if (pred.Name.Equals("between"))
			{
				// If the second term is equal to the location...
				if (pred.TermAt(1).Equals(room))
					// Add the first term to the list.
					things.Add(pred.TermAt(0));
			}
		
		// Return the list of objects.
		return things;
	}

	// Return a list of all things at a location.
	public List<string> AllThings ()
	{
		// Make a new list.
		List<string> things = new List<string>();
		
		// Create a combined list of predicates.
		List<Predicate> allPreds = new List<Predicate>();
		
		foreach (Predicate pred in predicates)
			allPreds.Add(pred);
		
		foreach (Predicate pred in lastPredicates)
			allPreds.Add(pred);
		
		// Loop through the predicates.
		foreach (Predicate pred in predicates)
			// If the predicate's name is 'at'...
			if (pred.Name.Equals("at"))
			{
				// Add the first term to the list.
				things.Add(pred.TermAt(0));
			}
			else if (pred.Name.Equals("between"))
			{
				// Add the first term to the list.
				things.Add(pred.TermAt(0));
			}
		
		// Return the list of objects.
		return things;
	}
	
	// Checks if there is a door between two locations.
	public bool DoorBetween (string room1, string room2)
	{
		foreach (Predicate pred in predicates)
			if (pred.Name.Equals("between"))
				if (pred.TermAt(1).Equals(room1) && pred.TermAt(2).Equals(room2))
					return true;
					
		return false;
	}
	
	public string DoorConnecting (string door, string room1)
	{
		foreach (Predicate pred in predicates)
			if (pred.Name.Equals("between"))
				if (pred.TermAt(1).Equals(room1) && pred.TermAt(0).Equals(door))
					return pred.TermAt(2);
		
		return "";
	}
	
	public string DoorName (string room1, string room2)
	{
		foreach (Predicate pred in predicates)
			if (pred.Name.Equals("between"))
				if (pred.TermAt(1).Equals(room1) && pred.TermAt(2).Equals(room2))
					return pred.TermAt(0);
		
		return "";
	}
	
	public string GetExit (string entrance)
	{
		foreach (Predicate pred in predicates)
			if (pred.Name.Equals("to"))
				if (pred.TermAt(0).Equals(entrance))
					return pred.TermAt(1);
		
		return "";
	}
	
	// Returns a list of connected rooms in the predicate state.
	public List<string> Connections (string room)
	{
		// Create a new list.
		List<string> connections = new List<string>();
		
		// Loop through the predicates.
		foreach (Predicate pred in predicates)
			// If the predicate name is 'connected'...
			if (pred.Name.Equals("connected"))
				// If the first term is the location...
				if (pred.TermAt(0).Equals(room))
					// Add the second term to the list.
					connections.Add(pred.TermAt(1));
        
        // Return the list of connected locations.
        return connections;
    }
	
	// Creates a hashtable of all connections in the predicate state.
	public Hashtable AllConnections ()
	{
		// Create a new hashtable.
		Hashtable connections = new Hashtable();
		
		// Loop through the locations listed in the state.
		foreach (string location in GetLocations())
			// Add each location, connections pair to the hashtable.
			connections.Add(location, Connections(location));

		// Return the table of connections.
		return connections;
	}
	
	// Place an object at a location.
	public void PutObject (GameObject obj, string location)
	{
		// If the location exists in the objects table...
		if (objects.ContainsKey(location))
		{
			// Pull the location's current objects from the table.
			List<GameObject> locationObjects = objects[location] as List<GameObject>;
			
			// If there are objects at the location...
			if (locationObjects != null)
				// If the current object is not at the location...
				if (!locationObjects.Contains(obj))
				{
					// Add the object to the location.
					locationObjects.Add(obj);
					
					// Push the object list back to the table.
					SetObjectsAt(location, locationObjects);
				}
		}
		else
		{
			// Create a new list of objects.
			List<GameObject> locationObjects = new List<GameObject>();
			
			// Add the current object to the list.
			locationObjects.Add(obj);
			
			// Push the list to the table.
			objects.Add(location, locationObjects);
		}
	}
	
	// Remove a game object from a location in the table.
	public void RemoveObject (string objName, string location)
	{
		// If the table contains the location...
		if (objects.ContainsKey(location))
		{
			// Store the list of objects at that location.
			List<GameObject> locationObjects = objects[location] as List<GameObject>;
			
			// Find the game object in the list.
			GameObject obj = locationObjects.Find(x => x.name.Equals(objName));
			
			// Remove the object from the list.
			locationObjects.Remove(obj);
			
			// Push back the updated list.
			SetObjectsAt(location, locationObjects);
		}
	}
	
	// Returns the game objects at a location from the table.
	public List<GameObject> GetObjectsAt (string location)
	{
		return objects[location] as List<GameObject>;
	}

	// Returns the game objects at the current and connected rooms.
	public List<GameObject> GetConnectedObjectsAt (string location)
	{
		List<string> connectedRooms = Connections (location);
		List<GameObject> objects = GetObjectsAt (location);

		foreach (string room in connectedRooms)
			objects.AddRange (GetObjectsAt (room));

		return objects;
	}
	
	public void AddObjectToLocation (GameObject obj, string location)
	{
		List<GameObject> objs = new List<GameObject>();
		
		if (objects.ContainsKey(location))
			objs = objects[location] as List<GameObject>;
			
		objs.Add(obj);
		SetObjectsAt (location, objs);
	}
	
	// Store a set of objects at a location.
	public void SetObjectsAt (string location, List<GameObject> objs)
	{
		objects[location] = objs;
	}
    
    // Get all the locations from the current predicate state.
    private List<string> GetLocations ()
	{
		// Create a new list.
		List<string> locations = new List<string>();
		
		// Loop through the current predicates.
		foreach (Predicate pred in predicates)
			// If the predicate's name is 'location'...
			if (pred.Name.Equals("location"))
				// Add the first term to the list.
				locations.Add(pred.TermAt(0));
	
		// Return the list.
		return locations;
	}

	// Get the player's name from the current predicate state.
	private string GetPlayer ()
	{
		// Loop through the predicates.
		foreach (Predicate pred in predicates)
			// If the predicate's name is 'player'...
			if (pred.Name.Equals("player"))
				// Return the first term.
				return pred.TermAt(0);
        
		// We couldn't find what we were looking for.
        return "";
    }
    
    // Given a Unity location chunk, returns an open tile.
    public GameObject GetOpenTile(GameObject chunk)
    {
		// Create a list to store open tiles in the Unity location.
		List<GameObject> validTiles = new List<GameObject>();
		
		// For each tile in the player's location.
		foreach (Transform tileT in chunk.transform)
			// If the tile is available...
			if (tileT.tag.Equals("Navigation"))
				// Add the tile to the list of open tiles.
				validTiles.Add(tileT.gameObject);
		
		// Return one tile at random from the list of open tiles we compiled.
		return validTiles[Random.Range(0,validTiles.Count)];
    }
    
    // Given a predicate object name and a location, instantiate a Unity game object at that location.
    private GameObject InstantiateAt(string thing, string location)
    {
		// Find and store the Unity game object for the player's current location.
		GameObject chunk = GameObject.Find(location);
		
		// Instantiate a new game object that matches the predicate object's type at the chosen tile.
		GameObject thingGO = Instantiate(Resources.Load(Type(thing), typeof(GameObject)), GetOpenTile(chunk).transform.position, Quaternion.identity) as GameObject;
		
		// Set the game object's parent to the room.
		thingGO.transform.parent = chunk.transform;
		
		// Set the new game object's name to that of the predicate object.
		thingGO.name = thing;
		
		// Return the new game object.
		return thingGO;
    }
    
    // Given a predicate object name and a position, instantiate it in the player's inventory.
    private GameObject InstantiateInventoryItem(string thing, int position)
    {
		// Find the inventory game object.
		GameObject chunk = GameObject.Find("Inventory");
		
		// Instantiate the inventory object at the specified position.
		GameObject thingGO = Instantiate(Resources.Load(Type(thing), typeof(GameObject)), new Vector3 (chunk.transform.position.x - (screenSize.x / 2) + (unit.x * (position + 1)), chunk.transform.position.y, 0), Quaternion.identity) as GameObject;
		
		// Set the game object's parent to the Inventory game object.
		thingGO.transform.parent = chunk.transform;
		
		// Set the game object's name to the predicate name.
		thingGO.name = thing;
		
		// Return the instantiated object.
		return thingGO;
    }
    
	public string PositionToLocation (Vector3 position)
	{
		// Find all level chunks.
		GameObject[] chunks = GameObject.FindGameObjectsWithTag("Level Chunk");
		
		// The room string.
		string room = "";
		
		// The room distance.
		float distance = Mathf.Infinity;
		
		// Loop through the level chunks.
		foreach (GameObject chunk in chunks)
		{
			// Check the old room distance.
			if (Vector3.Distance(position, chunk.transform.position) < distance)
			{
				room = chunk.name;
				distance = Vector3.Distance(position, chunk.transform.position);
			}
		}
		
		return room;
	}
	
	public bool Colocated (string one, string two)
	{
		return At(one).Equals(At(two));
	}
	
	public bool IsInventory()
	{
		foreach (Predicate pred in predicates)
			if (pred.Name.Equals("inventory"))
				return true;
				
		return false;
	}
}
