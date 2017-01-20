using UnityEngine;
using System.Collections;

// Handles camera movements.
public class CameraController : MonoBehaviour 
{
	private Mediator mediator;
	private StateManager stateManager;
	
	private GameObject player;
	private Player playerScript;
	private Vector3 targetPosition;
	
	// Use this for initialization
	void Start () 
	{
		GameObject medOB = GameObject.Find("Mediator");
		mediator = medOB.GetComponent<Mediator>();
		GameObject smGO = GameObject.Find("Level");
		stateManager = smGO.GetComponent<StateManager>();
		player = null;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (player != null)
		{
			if (!player.renderer.isVisible && iTween.Count(gameObject) == 0)
			{
				Vector3 topLeft = camera.ScreenToWorldPoint(new Vector3(0,camera.pixelHeight,camera.farClipPlane));
				Vector3 bottomRight = camera.ScreenToWorldPoint(new Vector3(camera.pixelWidth,0,camera.farClipPlane));
				
				if (player.transform.position.x > bottomRight.x)
					targetPosition = new Vector3 (transform.position.x + (Mathf.Abs(transform.position.x - bottomRight.x) * 2), transform.position.y, transform.position.z);
				else if (player.transform.position.x < topLeft.x)
					targetPosition = new Vector3 (transform.position.x - (Mathf.Abs(transform.position.x - bottomRight.x) * 2), transform.position.y, transform.position.z);
				else if (player.transform.position.y > topLeft.y)
					targetPosition = new Vector3 (transform.position.x, transform.position.y + (Mathf.Abs(transform.position.y - bottomRight.y) * 2), transform.position.z);
				else if (player.transform.position.y < bottomRight.y)
					targetPosition = new Vector3 (transform.position.x, transform.position.y - (Mathf.Abs(transform.position.y - bottomRight.y) * 2), transform.position.z);
				else
					return;
				
				GameObject targetRoom = GameObject.Find(Locate(targetPosition));
				Vector3 roomPosition = new Vector3 (targetRoom.transform.position.x, targetRoom.transform.position.y, transform.position.z);
				iTween.MoveTo(gameObject, iTween.Hash ("position", roomPosition, "time", 2, "easetype", "linear"));
				stateManager.RemoveObject(stateManager.Player, Locate(transform.position));
				stateManager.PutObject(player, Locate(roomPosition));
				mediator.PlayerUpdate(Move(transform.position, targetPosition));
			}
		}
		else
		{
			player = GameObject.Find(mediator.problem.player);
		}
	}
	
	private string Locate (Vector3 position)
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
	
	// The player has moved.
	private string Move (Vector3 start, Vector3 end)
	{
		string room1 = Locate(start);
		string room2 = Locate(end);
		if (stateManager.DoorBetween(room1, room2))
			return "(move-location-door " + mediator.problem.player + " " + stateManager.DoorName(room2, room1) + " " + room2 + " " + room1 + ")";
		
		return "(move-location " + mediator.problem.player + " " + room2 + " " + room1 + ")";
	}
}