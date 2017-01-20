using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Mediation.PlanTools;

// Generates the map from a PDDL representation.
// Good luck!
public class LevelGenerator : MonoBehaviour 
{
	private StateManager stateManager;
	private Hashtable map;
	public Vector3 unit;
	public Vector2 chunkSize;
	private int tileCount = 0;
	private List<LevelTile> connectors = new List<LevelTile>();
	private List<LevelTile> unlockers = new List<LevelTile>();
	
	private bool IsConnector (string tag)
	{
		return (tag.Equals("Door") || tag.Equals("Navigation"));
	}
	
	private GameObject FindAt (Vector3 position)
	{
		Collider2D[] colliders;
		GameObject closest = null;
		
		if ((colliders = Physics2D.OverlapCircleAll(position, unit.x)).Length > 1)
		{
			float distance = float.MaxValue;
			foreach (Collider2D collider in colliders)
			{
				float thisDistance = Vector3.Distance(position, collider.transform.position);
				if (thisDistance < distance)
				{
					distance = thisDistance;
					closest = collider.gameObject;
				}
			}
		}
		
		return closest;
	}
	
	public void CreateLevel ()
	{
		stateManager = this.gameObject.GetComponent<StateManager>();

		GameObject rock = Instantiate(Resources.Load("Rock", typeof(GameObject))) as GameObject;
		Vector3 topLeft = Camera.main.ScreenToWorldPoint(new Vector3(0,Camera.main.pixelHeight,Camera.main.farClipPlane));
		Vector3 bottomRight = Camera.main.ScreenToWorldPoint(new Vector3(Camera.main.pixelWidth,0,Camera.main.farClipPlane));
		unit = rock.renderer.bounds.size;
		chunkSize = new Vector2 (Mathf.Abs(topLeft.x - bottomRight.x), Mathf.Abs(topLeft.y - bottomRight.y));

		map = CreateMap();

		List<string> unplaced = new List<string>();
		List<string> placed = new List<string>();
		foreach (string location in stateManager.Locations)
		{
			GameObject room = new GameObject();
			room.name = location;
			room.tag = "Level Chunk";
			room.transform.parent = this.transform;

			if (!stateManager.At(stateManager.Player).Equals(location))
			    unplaced.Add(location);
			else
				placed.Add(location);

			if (stateManager.Type(location).Equals("sand"))
				CreateSand (unit, topLeft, bottomRight, room);
			else if (stateManager.Type(location).Equals("woods"))
				CreateWoods (unit, topLeft, bottomRight, room);
			else if (stateManager.Type(location).Equals("cave"))
				CreateCave (unit, topLeft, bottomRight, room);
			else if (stateManager.Type(location).Equals("base"))
				CreateBase (unit, topLeft, bottomRight, room);
			else if (stateManager.Type(location).Equals("specialbase"))
				CreateSpecialBase (unit, topLeft, bottomRight, room);
				
			room.transform.position = new Vector2 (Random.Range (-10000, 10000), Random.Range (-10000, 10000));
        }
        
        foreach (string location in stateManager.Locations)
        {
        	GameObject room = GameObject.Find(location);
        	room.transform.position = new Vector2(0,0);
        }

		bool found = true;
		while (unplaced.Count > 0 && found)
		{
			found = false;
			List<string> add = new List<string>();
			List<string> rem = new List<string>();
			foreach (string location in placed)
			{
				GameObject placedObject = GameObject.Find(location);
				MapNode node = map[location] as MapNode;
				if (unplaced.Contains(node.Up))
				{
					GameObject unplacedObject = GameObject.Find(node.Up);
					unplacedObject.transform.position = placedObject.transform.position + Mathf.Abs(topLeft.y - bottomRight.y) * Vector3.up;
					unplaced.Remove(node.Up);
					add.Add(node.Up);
					found = true;
				}

				if (unplaced.Contains(node.Down))
				{
					GameObject unplacedObject = GameObject.Find(node.Down);
					unplacedObject.transform.position = placedObject.transform.position + Mathf.Abs(topLeft.y - bottomRight.y) * Vector3.down;
					unplaced.Remove(node.Down);
					add.Add(node.Down);
                    found = true;
                }

				if (unplaced.Contains(node.Left))
				{
					GameObject unplacedObject = GameObject.Find(node.Left);
					unplacedObject.transform.position = placedObject.transform.position + Mathf.Abs(topLeft.x - bottomRight.x) * Vector3.left;
					unplaced.Remove(node.Left);
					add.Add(node.Left);
                    found = true;
                }

				if (unplaced.Contains(node.Right))
				{
					GameObject unplacedObject = GameObject.Find(node.Right);
					unplacedObject.transform.position = placedObject.transform.position + Mathf.Abs(topLeft.x - bottomRight.x) * Vector3.right;
					unplaced.Remove(node.Right);
					add.Add(node.Right);
                    found = true;
                }

				rem.Add(location);
            }
				
			if (unplaced.Count > 0 && !found)
			{
				string location = unplaced[0];
				GameObject unplacedObject = GameObject.Find (location);
				unplacedObject.transform.position = new Vector2 (Random.Range (-10000, 10000), Random.Range (-10000, 10000));
				unplaced.Remove(location);
				add.Add (location);
				found = true;
			}
			
			foreach (string addition in add)
				placed.Add(addition);
			
			foreach (string removal in rem)
				placed.Remove(removal);
		}
		
		ConnectGraphs();
		
		foreach (LevelTile tile in unlockers)
			tile.unlock();
		
        stateManager.Refresh();
        
        Object.Destroy(rock);
    }
    
    public void ConnectGraphs()
    {
    	foreach (LevelTile tile in connectors)
    	{
    		if (!tile.hasUp())
    		{
				GameObject upGO = FindAt(tile.getLocation() + new Vector3(0, unit.y, 0));
				if (upGO != null)
				{
					LevelTile upTile = upGO.GetComponent<LevelTile>();
					if (upTile != null)
						tile.setUp(upTile);
				}
			}
			
			if (!tile.hasLeft())
			{
				GameObject leftGO = FindAt(tile.getLocation() - new Vector3(unit.x, 0, 0));
				if (leftGO != null)
				{
					LevelTile leftTile = leftGO.GetComponent<LevelTile>();
					if (leftTile != null)
						tile.setLeft(leftTile);
				}
			}
    	}
    }
    
    private Hashtable CreateMap ()
	{
		Hashtable map = new Hashtable();
		List<string> completed = new List<string>();
		foreach (string location in stateManager.Locations)
		{
			if (!map.ContainsKey(location))
			{
				MapNode node = new MapNode();
				node.Name = location;
				List<string> connections = stateManager.Connections(location);
				foreach (string connection in connections)
				{
					if (!map.ContainsKey(connection))
					{
						MapNode connectedNode = new MapNode();
						connectedNode.Name = connection;
						if (node.Up.Equals(""))
						{
							node.Up = connection;
							connectedNode.Down = location;
						}
						else if (node.Down.Equals(""))
						{
							node.Down = connection;
							connectedNode.Up = location;
						}
						else if (node.Left.Equals(""))
						{
							node.Left = connection;
							connectedNode.Right = location;
						}
                        else if (node.Right.Equals(""))
						{
                            node.Right = connection;
							connectedNode.Left = location;
						}
						map.Add(connection, connectedNode);
                    }
					else
					{
						MapNode connectedNode = map[connection] as MapNode;
						if (node.Up.Equals("") && connectedNode.Down.Equals(""))
						{
							node.Up = connection;
							connectedNode.Down = location;
						}
						else if (node.Down.Equals("") && connectedNode.Up.Equals(""))
						{
							node.Down = connection;
							connectedNode.Up = location;
						}
						else if (node.Left.Equals("") && connectedNode.Right.Equals(""))
						{
							node.Left = connection;
                            connectedNode.Right = location;
                        }
						else if (node.Right.Equals("") && connectedNode.Left.Equals(""))
                        {
                            node.Right = connection;
                            connectedNode.Left = location;
                        }
						map[connection] = connectedNode;
                    }
                }
                
                map.Add(location, node);
			}
			else
			{
				MapNode node = map[location] as MapNode;
				List<string> connections = stateManager.Connections(location);
				foreach (string connection in connections)
				{
					if (!map.ContainsKey(connection))
					{
						MapNode connectedNode = new MapNode();
						connectedNode.Name = connection;
						if (node.Up.Equals(""))
						{
							node.Up = connection;
							connectedNode.Down = location;
						}
						else if (node.Down.Equals(""))
						{
							node.Down = connection;
							connectedNode.Up = location;
						}
						else if (node.Left.Equals(""))
						{
							node.Left = connection;
							connectedNode.Right = location;
						}
						else if (node.Right.Equals(""))
						{
							node.Right = connection;
							connectedNode.Left = location;
						}
						map.Add(connection, connectedNode);
					}
					else if (!completed.Contains(connection))
					{
						MapNode connectedNode = map[connection] as MapNode;
						if (node.Up.Equals("") && connectedNode.Down.Equals(""))
						{
							node.Up = connection;
							connectedNode.Down = location;
						}
						else if (node.Down.Equals("") && connectedNode.Up.Equals(""))
                        {
                            node.Down = connection;
                            connectedNode.Up = location;
                        }
						else if (node.Left.Equals("") && connectedNode.Right.Equals(""))
                        {
                            node.Left = connection;
                            connectedNode.Right = location;
                        }
						else if (node.Right.Equals("") && connectedNode.Left.Equals(""))
                        {
                            node.Right = connection;
                            connectedNode.Left = location;
                        }
                        map[connection] = connectedNode;
                    }
                }
                
				map[location] = node;
            }

			completed.Add(location);
        }

		return map;
    }
    
    private void CreateSand (Vector3 unit, Vector3 topLeft, Vector3 bottomRight, GameObject room)
    {    	
		CreateGeneric(unit, topLeft, bottomRight, room, "Sand", "Rock", "SandDoor");
    }
    
    private void CreateWoods (Vector3 unit, Vector3 topLeft, Vector3 bottomRight, GameObject room)
    {
		CreateDistinctWalls(unit, topLeft, bottomRight, room, "Sand", "Bush", "Bush", "Bush", "BaseDoor");
    }
    
	private void CreateCave (Vector3 unit, Vector3 topLeft, Vector3 bottomRight, GameObject room)
	{
		CreateGeneric(unit, topLeft, bottomRight, room, "CaveFloor", "CaveRock", "CaveDoor");
	}
	
	private void CreateBase (Vector3 unit, Vector3 topLeft, Vector3 bottomRight, GameObject room)
	{
		CreateDistinctWalls(unit, topLeft, bottomRight, room, "BaseFloor", "TopBaseWall", "LeftBaseWall", "CornerBaseWall", "BaseDoor");
	}
	
	private void CreateSpecialBase (Vector3 unit, Vector3 topLeft, Vector3 bottomRight, GameObject room)
	{
		CreateDistinctWalls(unit, topLeft, bottomRight, room, "BaseFloor", "TopBaseWall", "LeftBaseWall", "CornerBaseWall", "RedBaseDoor");
	}
	
	private void CreateGeneric (Vector3 unit, Vector3 topLeft, Vector3 bottomRight, GameObject room, string floor, string walls, string doors)
	{		
		MapNode node = map[room.name] as MapNode;
		float max = Mathf.Abs(topLeft.x - bottomRight.x) / unit.x;
		float maxY = Mathf.Abs(topLeft.y - bottomRight.y) / unit.y;
		GameObject thingGO = null;
		for (int i = 0; i < max; i++)
		{
			if (node.Up.Equals(""))
			{
				thingGO = Instantiate(Resources.Load(walls, typeof(GameObject)), new Vector3 (topLeft.x + (unit.x / 2) + (i * unit.x), topLeft.y - (unit.y / 2), topLeft.z), Quaternion.identity) as GameObject;
				thingGO.transform.parent = this.transform.FindChild(room.name);
				thingGO.name = thingGO.name + tileCount++;
			}
			else
			{
				if (!stateManager.DoorBetween(room.name, node.Up))
				{
					if (i == 0 || i > (Mathf.Abs(topLeft.x - bottomRight.x) / unit.x) - 1.5)
					{
						thingGO = Instantiate(Resources.Load(walls, typeof(GameObject)), new Vector3 (topLeft.x + (unit.x / 2) + (i * unit.x), topLeft.y - (unit.y / 2), topLeft.z), Quaternion.identity) as GameObject;
						thingGO.transform.parent = this.transform.FindChild(room.name);
						thingGO.name = thingGO.name + tileCount++;
					}
					else
					{
						thingGO = Instantiate(Resources.Load(floor, typeof(GameObject)), new Vector3 (topLeft.x + (unit.x / 2) + (i * unit.x), topLeft.y - (unit.y / 2), topLeft.z), Quaternion.identity) as GameObject;
						thingGO.transform.parent = this.transform.FindChild(room.name);
						thingGO.name = thingGO.name + tileCount++;
					}
				}
				else
				{
					if (i < (max / 2) - 1 || i > max / 2 + 1 || ((i + 1) + 1 == max / 2 + 1))
					{
						thingGO = Instantiate(Resources.Load(walls, typeof(GameObject)), new Vector3 (topLeft.x + (unit.x / 2) + (i * unit.x), topLeft.y - (unit.y / 2), topLeft.z), Quaternion.identity) as GameObject;
						thingGO.transform.parent = this.transform.FindChild(room.name);
						thingGO.name = thingGO.name + tileCount++;
					}
					else
					{
						thingGO = Instantiate(Resources.Load(doors, typeof(GameObject)), new Vector3 (topLeft.x + (unit.x / 2) + (i * unit.x), topLeft.y - (unit.y / 2), topLeft.z), Quaternion.identity) as GameObject;
						if (i + 1 > max / 2 + 1)
							thingGO.transform.Rotate(0, 180, 0);
						thingGO.transform.parent = this.transform.FindChild(room.name);
						thingGO.name = stateManager.DoorName(room.name, node.Up);
						stateManager.AddObjectToLocation(thingGO, room.name);
					}
				}
			}
			bool doorRight = stateManager.DoorBetween(room.name, node.Right);
			bool doorLeft = stateManager.DoorBetween(room.name, node.Left);
            for (int j = 1; j < (Mathf.Abs(topLeft.y - bottomRight.y) / unit.y) - 1; j++)
			{
				if ((i == 0 && node.Left.Equals("")) || (i > (Mathf.Abs(topLeft.x - bottomRight.x) / unit.x) - 1.5 && node.Right.Equals("")))
				{
					thingGO = Instantiate(Resources.Load(walls, typeof(GameObject)), new Vector3 (topLeft.x + (unit.x / 2) + (i * unit.x), topLeft.y - (unit.y / 2) - (j * unit.y), topLeft.z), Quaternion.identity) as GameObject;
					thingGO.transform.parent = this.transform.FindChild(room.name);
					thingGO.name = thingGO.name + tileCount++;
				}
				else if ((i == 0 && doorLeft) || (i > (Mathf.Abs(topLeft.x - bottomRight.x) / unit.x) - 1.5 && doorRight))
				{
					if (j < (maxY / 2) - 1 || j > maxY / 2 + 1)
					{
						thingGO = Instantiate(Resources.Load(walls, typeof(GameObject)), new Vector3 (topLeft.x + (unit.x / 2) + (i * unit.x), topLeft.y - (unit.y / 2) - (j * unit.y), topLeft.z), Quaternion.identity) as GameObject;
						thingGO.transform.parent = this.transform.FindChild(room.name);
						thingGO.name = thingGO.name + tileCount++;
					}
					else
					{
						thingGO = Instantiate(Resources.Load(doors, typeof(GameObject)), new Vector3 (topLeft.x + (unit.x / 2) + (i * unit.x), topLeft.y - (unit.y / 2) - (j * unit.y), topLeft.z), Quaternion.identity) as GameObject;
						if (j + 1 > maxY / 2 + 1)
							thingGO.transform.Rotate (0, 0, 90);
						else
							thingGO.transform.Rotate (0, 0, 270);
						thingGO.transform.parent = this.transform.FindChild(room.name);
						if (i == 0)
							thingGO.name = stateManager.DoorName(room.name, node.Left);
						else
							thingGO.name = stateManager.DoorName(room.name, node.Right);
						stateManager.AddObjectToLocation(thingGO, room.name);
					}
				}
				else
				{
					thingGO = Instantiate(Resources.Load(floor, typeof(GameObject)), new Vector3 (topLeft.x + (unit.x / 2) + (i * unit.x), topLeft.y - (unit.y / 2) - (j * unit.y), topLeft.z), Quaternion.identity) as GameObject;
					thingGO.transform.parent = this.transform.FindChild(room.name);
					thingGO.name = thingGO.name + tileCount++;
				}
			}
			if (node.Down.Equals(""))
			{
				thingGO = Instantiate(Resources.Load(walls, typeof(GameObject)), new Vector3 (topLeft.x + (unit.x / 2) + (i * unit.x), bottomRight.y + (unit.y / 2), topLeft.z), Quaternion.identity) as GameObject;
				thingGO.transform.parent = this.transform.FindChild(room.name);
				thingGO.name = thingGO.name + tileCount++;
			}
			else
			{
				if (!stateManager.DoorBetween(room.name, node.Down))
				{
					if (i == 0 || i > (Mathf.Abs(topLeft.x - bottomRight.x) / unit.x) - 1.5)
					{
						thingGO = Instantiate(Resources.Load(walls, typeof(GameObject)), new Vector3 (topLeft.x + (unit.x / 2) + (i * unit.x), bottomRight.y + (unit.y / 2), topLeft.z), Quaternion.identity) as GameObject;
						thingGO.transform.parent = this.transform.FindChild(room.name);
						thingGO.name = thingGO.name + tileCount++;
					}
					else
					{
						thingGO = Instantiate(Resources.Load(floor, typeof(GameObject)), new Vector3 (topLeft.x + (unit.x / 2) + (i * unit.x), bottomRight.y + (unit.y / 2), topLeft.z), Quaternion.identity) as GameObject;
						thingGO.transform.parent = this.transform.FindChild(room.name);
						thingGO.name = thingGO.name + tileCount++;
					}
				}
				else
				{
					if (i < (max / 2) - 1 || i > max / 2 + 1 || ((i + 1) + 1 == max / 2 + 1))
					{
						thingGO = Instantiate(Resources.Load(walls, typeof(GameObject)), new Vector3 (topLeft.x + (unit.x / 2) + (i * unit.x), bottomRight.y + (unit.y / 2), topLeft.z), Quaternion.identity) as GameObject;
						thingGO.transform.parent = this.transform.FindChild(room.name);
						thingGO.name = thingGO.name + tileCount++;
					}
					else
					{
						thingGO = Instantiate(Resources.Load(doors, typeof(GameObject)), new Vector3 (topLeft.x + (unit.x / 2) + (i * unit.x), bottomRight.y + (unit.y / 2), topLeft.z), Quaternion.identity) as GameObject;
						if (i + 1 > max / 2 + 1)
							thingGO.transform.Rotate(0, 180, 0);
						thingGO.transform.parent = this.transform.FindChild(room.name);
						thingGO.name = stateManager.DoorName(room.name, node.Down);
						stateManager.AddObjectToLocation(thingGO, room.name);
					}
				}
			}
        }
    }
    
	private void CreateDistinctWalls (Vector3 unit, Vector3 topLeft, Vector3 bottomRight, GameObject room, string floor, string top, string left, string corner, string doors)
	{		
		MapNode node = map[room.name] as MapNode;
		float max = Mathf.Abs(topLeft.x - bottomRight.x) / unit.x;
		float maxY = Mathf.Abs(topLeft.y - bottomRight.y) / unit.y;
		GameObject thingGO = null;
		LevelTile tile = null;
		
		for (int i = 0; i < max; i++)
		{
			if (node.Up.Equals(""))
				if (i == 0)
				{
					thingGO = Instantiate(Resources.Load(corner, typeof(GameObject)), new Vector3 (topLeft.x + (unit.x / 2) + (i * unit.x), topLeft.y - (unit.y / 2), topLeft.z), Quaternion.identity) as GameObject;
					thingGO.name = thingGO.name + tileCount++;
					thingGO.transform.parent = this.transform.FindChild(room.name);
					thingGO.transform.localScale = Vector3.Scale(thingGO.transform.localScale, new Vector3(-1, 1, 1));
				}
				else if (i > (Mathf.Abs(topLeft.x - bottomRight.x) / unit.x) - 1.5)
				{
					thingGO = Instantiate(Resources.Load(corner, typeof(GameObject)), new Vector3 (topLeft.x + (unit.x / 2) + (i * unit.x), topLeft.y - (unit.y / 2), topLeft.z), Quaternion.identity) as GameObject;
					thingGO.transform.parent = this.transform.FindChild(room.name);
					thingGO.name = thingGO.name + tileCount++;
				}
				else
				{
					thingGO = Instantiate(Resources.Load(top, typeof(GameObject)), new Vector3 (topLeft.x + (unit.x / 2) + (i * unit.x), topLeft.y - (unit.y / 2), topLeft.z), Quaternion.identity) as GameObject;
					thingGO.transform.parent = this.transform.FindChild(room.name);
					thingGO.name = thingGO.name + tileCount++;
				}
			else
			{
				if (!stateManager.DoorBetween(room.name, node.Up))
				{
					if (i == 0)
					{
						thingGO = Instantiate(Resources.Load(left, typeof(GameObject)), new Vector3 (topLeft.x + (unit.x / 2) + (i * unit.x), topLeft.y - (unit.y / 2), topLeft.z), Quaternion.identity) as GameObject;
						thingGO.transform.parent = this.transform.FindChild(room.name);
						thingGO.name = thingGO.name + tileCount++;
					}
					else if (i > (Mathf.Abs(topLeft.x - bottomRight.x) / unit.x) - 1.5)
					{
						thingGO = Instantiate(Resources.Load(left, typeof(GameObject)), new Vector3 (topLeft.x + (unit.x / 2) + (i * unit.x), topLeft.y - (unit.y / 2), topLeft.z), Quaternion.Euler(0, 0, 180)) as GameObject;
						thingGO.transform.parent = this.transform.FindChild(room.name);
						thingGO.name = thingGO.name + tileCount++;
					}
					else
					{
						thingGO = Instantiate(Resources.Load(floor, typeof(GameObject)), new Vector3 (topLeft.x + (unit.x / 2) + (i * unit.x), topLeft.y - (unit.y / 2), topLeft.z), Quaternion.identity) as GameObject;
						thingGO.transform.parent = this.transform.FindChild(room.name);
						thingGO.name = thingGO.name + tileCount++;
					}
				}
				else
				{
					if (i == 0)
					{
						thingGO = Instantiate(Resources.Load(corner, typeof(GameObject)), new Vector3 (topLeft.x + (unit.x / 2) + (i * unit.x), topLeft.y - (unit.y / 2), topLeft.z), Quaternion.identity) as GameObject;
						thingGO.transform.parent = this.transform.FindChild(room.name);
						thingGO.name = thingGO.name + tileCount++;
						thingGO.transform.localScale = Vector3.Scale(thingGO.transform.localScale, new Vector3(-1, 1, 1));
					}
					else if (i > (Mathf.Abs(topLeft.x - bottomRight.x) / unit.x) - 1.5)
					{
						thingGO = Instantiate(Resources.Load(corner, typeof(GameObject)), new Vector3 (topLeft.x + (unit.x / 2) + (i * unit.x), topLeft.y - (unit.y / 2), topLeft.z), Quaternion.identity) as GameObject;
						thingGO.transform.parent = this.transform.FindChild(room.name);
						thingGO.name = thingGO.name + tileCount++;
					}
					else if (i < (max / 2) - 1 || i > max / 2 + 1 || ((i + 1) + 1 == max / 2 + 1))
					{
						thingGO = Instantiate(Resources.Load(top, typeof(GameObject)), new Vector3 (topLeft.x + (unit.x / 2) + (i * unit.x), topLeft.y - (unit.y / 2), topLeft.z), Quaternion.identity) as GameObject;
						thingGO.transform.parent = this.transform.FindChild(room.name);
						thingGO.name = thingGO.name + tileCount++;
					}
					else
					{
						thingGO = Instantiate(Resources.Load(doors, typeof(GameObject)), new Vector3 (topLeft.x + (unit.x / 2) + (i * unit.x), topLeft.y - (unit.y / 2), topLeft.z), Quaternion.identity) as GameObject;
						if (i + 1 > max / 2 + 1)
							thingGO.transform.Rotate(0, 180, 0);
						thingGO.transform.parent = this.transform.FindChild(room.name);
						thingGO.name = stateManager.DoorName(room.name, node.Up);
						stateManager.AddObjectToLocation(thingGO, room.name);
					}
				}
			}
			
			if (thingGO != null)
			{
				tile = thingGO.GetComponent<LevelTile>();
				if (tile != null && i > 0)
				{
					GameObject leftGO = FindAt(thingGO.transform.position - new Vector3(unit.x, 0, 0));
					if (leftGO != null)
					{
						LevelTile leftTile = leftGO.GetComponent<LevelTile>();
						if (leftTile != null)
							tile.setLeft(leftTile);
					}
				}
				
				if (IsConnector(thingGO.tag))
					connectors.Add(tile);
				
				thingGO = null;
			}
			
			bool doorRight = stateManager.DoorBetween(room.name, node.Right);
			bool doorLeft = stateManager.DoorBetween(room.name, node.Left);
			for (int j = 1; j < (Mathf.Abs(topLeft.y - bottomRight.y) / unit.y) - 1; j++)
			{
				if (i == 0 && node.Left.Equals(""))
				{
					thingGO = Instantiate(Resources.Load(left, typeof(GameObject)), new Vector3 (topLeft.x + (unit.x / 2) + (i * unit.x), topLeft.y - (unit.y / 2) - (j * unit.y), topLeft.z), Quaternion.identity) as GameObject;
					thingGO.transform.parent = this.transform.FindChild(room.name);
					thingGO.name = thingGO.name + tileCount++;
				}
				else if (i > (Mathf.Abs(topLeft.x - bottomRight.x) / unit.x) - 1.5 && node.Right.Equals(""))
				{
					thingGO = Instantiate(Resources.Load(left, typeof(GameObject)), new Vector3 (topLeft.x + (unit.x / 2) + (i * unit.x), topLeft.y - (unit.y / 2) - (j * unit.y), topLeft.z), Quaternion.Euler(0, 0, 180)) as GameObject;
					thingGO.transform.parent = this.transform.FindChild(room.name);
					thingGO.name = thingGO.name + tileCount++;
				}
				else if (i == 0 && doorLeft)
				{
					if (j < (maxY / 2) - 1 || j > maxY / 2 + 1)
					{
						thingGO = Instantiate(Resources.Load(left, typeof(GameObject)), new Vector3 (topLeft.x + (unit.x / 2) + (i * unit.x), topLeft.y - (unit.y / 2) - (j * unit.y), topLeft.z), Quaternion.identity) as GameObject;
						thingGO.transform.parent = this.transform.FindChild(room.name);
						thingGO.name = thingGO.name + tileCount++;
					}
					else
					{
						thingGO = Instantiate(Resources.Load(doors, typeof(GameObject)), new Vector3 (topLeft.x + (unit.x / 2) + (i * unit.x), topLeft.y - (unit.y / 2) - (j * unit.y), topLeft.z), Quaternion.identity) as GameObject;
						if (j + 1 > maxY / 2 + 1)
							thingGO.transform.Rotate (0, 0, 90);
						else
						{
							thingGO.transform.Rotate (0, 0, 90);
							thingGO.transform.localScale = Vector3.Scale(thingGO.transform.localScale, new Vector3(-1, 1, 1));
						}
						thingGO.transform.parent = this.transform.FindChild(room.name);
						thingGO.name = stateManager.DoorName(room.name, node.Left);
						stateManager.AddObjectToLocation(thingGO, room.name);
					}
				}
				else if (i > (Mathf.Abs(topLeft.x - bottomRight.x) / unit.x) - 1.5 && doorRight)
				{
					if (j < (maxY / 2) - 1 || j > maxY / 2 + 1)
					{
						thingGO = Instantiate(Resources.Load(left, typeof(GameObject)), new Vector3 (topLeft.x + (unit.x / 2) + (i * unit.x), topLeft.y - (unit.y / 2) - (j * unit.y), topLeft.z), Quaternion.Euler(0, 0, 180)) as GameObject;
						thingGO.transform.parent = this.transform.FindChild(room.name);
						thingGO.name = thingGO.name + tileCount++;
					}
					else
					{
						thingGO = Instantiate(Resources.Load(doors, typeof(GameObject)), new Vector3 (topLeft.x + (unit.x / 2) + (i * unit.x), topLeft.y - (unit.y / 2) - (j * unit.y), topLeft.z), Quaternion.identity) as GameObject;
						if (j + 1 > maxY / 2 + 1)
						{
							thingGO.transform.Rotate (0, 0, 270);
							thingGO.transform.localScale = Vector3.Scale(thingGO.transform.localScale, new Vector3(-1, 1, 1));
						}
						else
							thingGO.transform.Rotate (0, 0, 270);
							
						thingGO.transform.parent = this.transform.FindChild(room.name);
						thingGO.name = stateManager.DoorName(room.name, node.Right);
						stateManager.AddObjectToLocation(thingGO, room.name);
					}
				}
				else
				{
					thingGO = Instantiate(Resources.Load(floor, typeof(GameObject)), new Vector3 (topLeft.x + (unit.x / 2) + (i * unit.x), topLeft.y - (unit.y / 2) - (j * unit.y), topLeft.z), Quaternion.identity) as GameObject;
					thingGO.transform.parent = this.transform.FindChild(room.name);
					thingGO.name = thingGO.name + tileCount++;
				}
				
				if (thingGO != null)
				{
					tile = thingGO.GetComponent<LevelTile>();
					if (tile != null)
					{
						if (i > 0)
						{
							GameObject leftGO = FindAt(thingGO.transform.position - new Vector3(unit.x, 0, 0));
							if (leftGO != null)
							{
								LevelTile leftTile = leftGO.GetComponent<LevelTile>();
								if (leftTile != null)
									tile.setLeft(leftTile);
							}
						}
						else
						{
							if (IsConnector(thingGO.tag))
								connectors.Add(tile);
						}
						
						GameObject upGO = FindAt(thingGO.transform.position + new Vector3(0, unit.y, 0));
						if (upGO != null)
						{
							LevelTile upTile = upGO.GetComponent<LevelTile>();
							if (upTile != null)
								tile.setUp(upTile);
						}
					}
					thingGO = null;
				}
			}
			if (node.Down.Equals(""))
			{
				if (i == 0)
				{
					thingGO = Instantiate(Resources.Load(corner, typeof(GameObject)), new Vector3 (topLeft.x + (unit.x / 2) + (i * unit.x), bottomRight.y + (unit.y / 2), topLeft.z), Quaternion.Euler(0, 0, 180)) as GameObject;
					thingGO.transform.parent = this.transform.FindChild(room.name);
					thingGO.name = thingGO.name + tileCount++;
				}
				else if (i > (Mathf.Abs(topLeft.x - bottomRight.x) / unit.x) - 1.5)
				{
					thingGO = Instantiate(Resources.Load(corner, typeof(GameObject)), new Vector3 (topLeft.x + (unit.x / 2) + (i * unit.x), bottomRight.y + (unit.y / 2), topLeft.z), Quaternion.Euler(0, 0, 180)) as GameObject;
					thingGO.name = thingGO.name + tileCount++;
					thingGO.transform.parent = this.transform.FindChild(room.name);
					thingGO.transform.localScale = Vector3.Scale(thingGO.transform.localScale, new Vector3(-1, 1, 1));
				}
				else
				{
					thingGO = Instantiate(Resources.Load(top, typeof(GameObject)), new Vector3 (topLeft.x + (unit.x / 2) + (i * unit.x), bottomRight.y + (unit.y / 2), topLeft.z), Quaternion.Euler(0, 0, 180)) as GameObject;
					thingGO.transform.parent = this.transform.FindChild(room.name);
					thingGO.name = thingGO.name + tileCount++;
				}
			}
			else
			{
				if (!stateManager.DoorBetween(room.name, node.Down))
				{
					if (i == 0) 
					{
						thingGO = Instantiate(Resources.Load(left, typeof(GameObject)), new Vector3 (topLeft.x + (unit.x / 2) + (i * unit.x), bottomRight.y + (unit.y / 2), topLeft.z), Quaternion.identity) as GameObject;
						thingGO.transform.parent = this.transform.FindChild(room.name);
						thingGO.name = thingGO.name + tileCount++;
					}
					else if (i > (Mathf.Abs(topLeft.x - bottomRight.x) / unit.x) - 1.5)
					{
						thingGO = Instantiate(Resources.Load(left, typeof(GameObject)), new Vector3 (topLeft.x + (unit.x / 2) + (i * unit.x), bottomRight.y + (unit.y / 2), topLeft.z), Quaternion.Euler(0, 0, 180)) as GameObject;
						thingGO.transform.parent = this.transform.FindChild(room.name);
						thingGO.name = thingGO.name + tileCount++;
					}
					else
					{
						thingGO = Instantiate(Resources.Load(floor, typeof(GameObject)), new Vector3 (topLeft.x + (unit.x / 2) + (i * unit.x), bottomRight.y + (unit.y / 2), topLeft.z), Quaternion.identity) as GameObject;
						thingGO.transform.parent = this.transform.FindChild(room.name);
						thingGO.name = thingGO.name + tileCount++;
					}
				}
				else
				{
					if (i == 0) 
					{
						thingGO = Instantiate(Resources.Load(corner, typeof(GameObject)), new Vector3 (topLeft.x + (unit.x / 2) + (i * unit.x), bottomRight.y + (unit.y / 2), topLeft.z), Quaternion.Euler(0, 0, 180)) as GameObject;
						thingGO.transform.parent = this.transform.FindChild(room.name);
						thingGO.name = thingGO.name + tileCount++;
					}
					else if (i > (Mathf.Abs(topLeft.x - bottomRight.x) / unit.x) - 1.5)
					{
						thingGO = Instantiate(Resources.Load(corner, typeof(GameObject)), new Vector3 (topLeft.x + (unit.x / 2) + (i * unit.x), bottomRight.y + (unit.y / 2), topLeft.z), Quaternion.Euler(0, 0, 180)) as GameObject;
						thingGO.name = thingGO.name + tileCount++;
						thingGO.transform.parent = this.transform.FindChild(room.name);
						thingGO.transform.localScale = Vector3.Scale(thingGO.transform.localScale, new Vector3(-1, 1, 1));
					}
					else if (i < (max / 2) - 1 || i > max / 2 + 1 || ((i + 1) + 1 == max / 2 + 1))
					{
						thingGO = Instantiate(Resources.Load(top, typeof(GameObject)), new Vector3 (topLeft.x + (unit.x / 2) + (i * unit.x), bottomRight.y + (unit.y / 2), topLeft.z), Quaternion.Euler(0, 0, 180)) as GameObject;
						thingGO.transform.parent = this.transform.FindChild(room.name);
						thingGO.name = thingGO.name + tileCount++;
					}
					else
					{
						thingGO = Instantiate(Resources.Load(doors, typeof(GameObject)), new Vector3 (topLeft.x + (unit.x / 2) + (i * unit.x), bottomRight.y + (unit.y / 2), topLeft.z), Quaternion.identity) as GameObject;
						thingGO.transform.localScale = Vector3.Scale(thingGO.transform.localScale, new Vector3(-1, 1, 1));
						thingGO.transform.Rotate(0, 0, 180);
						if (i + 1 > max / 2 + 1)
							thingGO.transform.Rotate(0, 180, 0);
						thingGO.transform.parent = this.transform.FindChild(room.name);
						thingGO.name = stateManager.DoorName(room.name, node.Down);
						stateManager.AddObjectToLocation(thingGO, room.name);
					}
				}
			}
			
			if (thingGO != null)
			{
				tile = thingGO.GetComponent<LevelTile>();
				if (tile != null)
				{
					if (i > 0)
					{
						GameObject leftGO = FindAt(thingGO.transform.position - new Vector3(unit.x, 0, 0));
						if (leftGO != null)
						{
							LevelTile leftTile = leftGO.GetComponent<LevelTile>();
							if (leftTile != null)
								tile.setLeft(leftTile);
						}
					}
					else
					{
						if (IsConnector(thingGO.tag))
							connectors.Add(tile);
					}
					
					GameObject upGO = FindAt(thingGO.transform.position + new Vector3(0, unit.y, 0));
					if (upGO != null)
					{
						LevelTile upTile = upGO.GetComponent<LevelTile>();
						if (upTile != null)
							tile.setUp(upTile);
					}
				}
				thingGO = null;
			}
		}
		
		unlockers.Add(tile);
	}
}

public class MapNode
{
    private string name;
	public string Name
	{
		get { return name; }
		set { name = value; }
	}

	private string up;
	public string Up
	{
		get { return up; }
		set { up = value; }
    }

	private string down;
	public string Down
	{
		get { return down; }
		set { down = value; }
    }

	private string left;
	public string Left
	{
		get { return left; }
		set { left = value; }
    }

	private string right;
	public string Right
	{
		get { return right; }
		set { right = value; }
    }

	public MapNode ()
	{
		name = "";
		up = "";
		down = "";
		left = "";
		right = "";
	}
}