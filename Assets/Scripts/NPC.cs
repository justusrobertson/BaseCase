using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Mediation.Interfaces;
using Mediation.PlanTools;
using System;

public class NPC : MonoBehaviour 
{
	private Animator animator;

	public Vector3 targetPosition;
	
	//The AI's speed per second
	private float speed = 1.25f;
	
	//The max distance from the AI to a waypoint for it to continue to the next waypoint
	public float nextWaypointDistance = .1f;
	
	private List<IOperator> actions = new List<IOperator>();
	
	//The waypoint we are currently moving towards
	private int currentWaypoint = 0;
	
	private StateManager stateManager;
	
	private Pathfinding pathfinder;
	
	private List<LevelTile> path = new List<LevelTile>();
	public void SetPath (List<LevelTile> path)
	{
		this.path = path;	
	}
	
	private HashSet<LevelTile> floorTileList = new HashSet<LevelTile>();
	public HashSet<LevelTile> FloorTileList
	{
		get { return floorTileList; }
		set { }
	}
	
	private Vector3 direction = new Vector3 (0, 0, 0);
	public Vector3 Direction
	{
		get { return direction; }
		set { direction = value; }
	}
	
	private bool executingAction = false;
	public bool ExecutingAction
	{
		get { return executingAction; }
		set { }
	}
	private float finishedActionTime = Time.time;
	public float FinishedActionTime
	{
		get { return finishedActionTime; }
		set { }
	}
	public void FinishAction ()
	{
		executingAction = false;
		finishedActionTime = Time.time;
	}
	
	private bool freeze = false;
	public bool Freeze
	{
		get { return freeze; }
		set { freeze = value; }
	}
	
	private RipleyActionLibrary library;
	private Behaviors behaviors;
	
	public void Awake () 
	{
		animator = this.GetComponent<Animator>();
		animator.speed = 0.5f;
		transform.position = new Vector3 (transform.position.x, transform.position.y, 990);
		
		// Find the level game object.
		GameObject level = GameObject.Find("Level");
		
		// Set the state manager.
		stateManager = level.GetComponent<StateManager>();
		
		// Set the pathfinder.
		pathfinder = level.GetComponent<Pathfinding>();
		
		// Find the mediator's game object.
		GameObject medOB = GameObject.Find("Mediator");
		
		library = medOB.GetComponent<RipleyActionLibrary>();
		
		behaviors = level.GetComponent<Behaviors>();
	}
	
	public void Update ()
	{	
		if (actions.Count > 0)
			ExecuteInstruction();
		else
			ChooseBehavior();
	}
	
	public void FixedUpdate () 
	{			
		if (!freeze)
		{
			MoveAvatar();		
			Animate();
		}
	}
	
	public virtual void ChooseBehavior ()
	{
		behaviors.GuardBehavior(this);
	}
	
	public void AcceptInstruction (IOperator action)
	{
		actions.Add(action);
	}
	
	public void CompleteInstruction (IOperator action)
	{
		actions.Remove(action);
		executingAction = true;
	}
	
	private void ExecuteInstruction ()
	{
		if (!executingAction)
		{
			IOperator action = actions.FirstOrDefault();
			MethodInfo method = library.GetType().GetMethod(action.Name.Replace("-", string.Empty));
			if (method != null)
				method.Invoke(library, new object[] { this, action });
		}
	}
	
	private void Animate ()
	{
		if (direction.y > 0 && Mathf.Abs(direction.y) > Mathf.Abs(direction.x) * 1.5)
			animator.SetInteger("Direction", 0);
		else if (direction.y < 0 && Mathf.Abs(direction.y) > Mathf.Abs(direction.x) * 1.5)
			animator.SetInteger("Direction", 2);
		else if (direction.x > 0)
			animator.SetInteger("Direction", 1);
		else if (direction.x < 0)
			animator.SetInteger("Direction", 3);
	}
	
	private void MoveAvatar ()
	{
		if (path.Count > 0)
		{
			animator.SetBool("Moving", true);
			executingAction = true;
			LevelTile first = path.FirstOrDefault();
			iTween.MoveTo(this.gameObject, iTween.Hash(
				"position", first.getLocation(),
				"easetype", iTween.EaseType.linear,
				"time", .35f
				));
				
			Vector3 newDirection = first.getLocation() - transform.position;
			if (newDirection != Vector3.zero)
				Direction = newDirection;
			if (Vector3.Distance(first.getLocation(), this.transform.position) < this.renderer.bounds.size.x / 10)
				path.RemoveAt(0);
		}
		else
		{
			if (animator.GetBool("Moving"))
				FinishAction();
			
			animator.SetBool("Moving", false);
			return;
		}
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		LevelTile tile = other.GetComponent<LevelTile>();
		if (tile != null)
			floorTileList.Add(tile);
	}
	
	void OnTriggerExit2D(Collider2D other)
	{
		LevelTile tile = other.GetComponent<LevelTile>();
		if (tile != null)
			floorTileList.Remove(tile);
	}
}
