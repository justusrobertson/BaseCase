using UnityEngine;
using System;
using System.Threading;
using System.Collections;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

using Mediation.StateSpace;
using Mediation.PlanTools;
using Mediation.Planners;
using Mediation.FileIO;
using Mediation.KnowledgeTools;
using Mediation.Enums;
using Mediation.Interfaces;

// An interface to GME.
public class Mediator : MonoBehaviour 
{
	// The State Manager object used to answer state questions.
	private StateManager stateManager;
	
	// The Level Generator used to modify the world.
	private LevelGenerator generator;
	
	// The UI Generator used to setup the interface.
	private UI uiGenerator;
	
	// The domain name.
	public string domainName;
	
	// The working domain object.
	public Domain domain;
	
	// The working problem object.
	public Problem problem;
	
	// The working plan object.
	public Plan plan;
	
	// The current state.
	public State state;
	
	// The current mediation node.
	public StateSpaceNode root;
	
	// The mediation frontier.
	public Hashtable frontier;
	
	// The frontier thread.
	public Thread frontierThread;
	
	// Is the game in a valid state?
	private bool validState;
	
	private bool goal;

	private bool needUpdate;

	private StateSpaceEdge currentEdge;

	// Use this for initialization
	void Start () 
	{
		Debug.Log (Screen.width + " " + Screen.height);
		int num = UnityEngine.Random.Range(1,4);
		
		domainName += num.ToString();
		
		Debug.Log (domainName + " " + num);
	
		// Set the path to the top directory.
		Parser.path = Path.GetFullPath(".") + "\\";
		
		Debug.Log (Parser.path);
	
		// Parse the domain file.
		domain = Parser.GetDomain(Parser.GetTopDirectory() + @"Benchmarks\" + domainName + @"\domain.pddl", PlanType.StateSpace);
		
		// Parse the problem file.
		problem = Parser.GetProblem(Parser.GetTopDirectory() + @"Benchmarks\" + domainName + @"\prob01.pddl");

		// Find the initial plan.
		plan = FastDownward.Plan(domain, problem);
		
		// A test for the planner.
		if (plan.Steps.Count > 0)
			Debug.Log ("Planner is working.");
		else
		{
			Debug.Log ("Planner is not working or no plan exists.");
			RestartGame();
		}
		
		// Find the first state.
		state = plan.GetFirstState();

		// Find the level game object.
		GameObject level = GameObject.Find("Level");
		
		// Set the state manager.
		stateManager = level.GetComponent<StateManager>();
		
		// Set the level generator.
		generator = level.GetComponent<LevelGenerator>();

		// Set the state manager's predicates.
		stateManager.Predicates = state.Predicates;

		// Find the UI game object.
		GameObject ui = GameObject.Find("UI");
		
		// Set the ui generator.
		uiGenerator = ui.GetComponent<UI>();
		
		// Setup the UI.
		uiGenerator.GenerateUI();

		// Generate the level.
		generator.CreateLevel();
		
		// Create the initial node of mediation.
		root = StateSpaceMediator.BuildTree(domain, problem, plan, state, 0);
		
		// Initialize the frontier.
		frontier = new Hashtable();
		
		// Expand the frontier in a new thread.
		frontierThread = new Thread(ExpandFrontier);
		
		// Start the thread.
		frontierThread.Start();

		// Initialize the camera fade.
		iTween.CameraFadeAdd();
		
		// Initialize the valid state.
		validState = true;
		
		goal = false;

		needUpdate = false;
	}

	public void Update ()
	{
		if (needUpdate) UpdateState();
	}

	private void UpdateState()
	{
		needUpdate = false;

		// Set the state manager's predicates.
		stateManager.Predicates = state.Predicates;
		
		//					Debug.Log ("New Plan");
		//					foreach (Operator step in plan.Steps)
		//					{
		//						Debug.Log (step.ToString());
		//					}
		
		// Initialize the frontier.
		frontier = new Hashtable();
		
		// Expand the frontier in a new thread.
		frontierThread = new Thread(ExpandFrontier);
		
		// Check for goal state.
		if (plan.Steps.Count == 0 && root.problem.initial.Count > 0)
		{
			Debug.Log ("GOAL STATE");
			goal = true;
			validState = false;
		}
		// Check for error state.
		else if (plan.Steps.Count == 0 && root.problem.initial.Count == 0)
		{
			Debug.Log("UNWINNABLE STATE");
			validState = false;
		}
		
		MediatorUpdate();
		
		// Ask the state manager to refresh the world.
		stateManager.Refresh();		

		if (stateManager.At(stateManager.Player).Equals("realescape"))
		{
			EndTheGame();
			return;
		}

		if (validState)		
			// Return control to the player.
			stateManager.PlayerTurn = true;
		else
			RestartGame();
	}
	
	public void JumpToState (List<Predicate> newPredicates)
	{
		// Create and store the new state object.
		state = new State(newPredicates, null, null);
		
		// Create the initial node of mediation.
		root = StateSpaceMediator.BuildTree(domain, problem, plan, state, 0);
		
		// Set the state manager's predicates.
		stateManager.Predicates = state.Predicates;
		
		// Ask the state manager to refresh the world.
		stateManager.Refresh();
	}
	
	// Called when the player requests a move.
	public bool PlayerUpdate (string playerAction)
	{		
		//Debug.Log (playerAction);
		
		// A record for whether this is a valid action.
		bool validAction = false;
		stateManager.PlayerTurn = false;
		StateSpaceEdge matchingEdge = null;
		foreach (StateSpaceEdge edge in root.outgoing)
			if (edge.Action.ToString().Equals(playerAction))
				matchingEdge = edge;
		
		if (matchingEdge != null)
		{
			validAction = true;
			
			currentEdge = matchingEdge;
			Thread updatePlan = new Thread(UpdatePlan);
			updatePlan.Start();
		}
		else
		{
			stateManager.PlayerTurn = true;
		}
		
		return validAction;
	}

	private void UpdatePlan()
	{
		if (frontier.ContainsKey (currentEdge))
		{
			//Debug.Log ("Cached");
			StateSpaceNode oldRoot = root;
			root = frontier[currentEdge] as StateSpaceNode;
			root.parent = oldRoot;
			problem = root.problem;
			plan = root.plan;
			state = root.state;
		}
		else
		{
			//Debug.Log ("Not Cached");
			frontierThread.Abort();
			StateSpaceNode oldRoot = root;
			root = StateSpaceMediator.ExpandTree(domain, problem, plan, state, currentEdge, 0);
			root.parent = oldRoot;
			problem = root.problem;
			plan = root.plan;
			state = root.state;
		}

		needUpdate = true;
	}
	
	// Called when it's the computer's turn.
	private void MediatorUpdate ()
	{
		// Actions that took place.
		List<IOperator> actions = root.systemActions;
		
		// Loop through the system actions...
		foreach (IOperator action in actions)
		{
			string playerLocation = KnowledgeAnnotator.GetLocation(problem.player, problem.initial);
			string npcBefore = KnowledgeAnnotator.GetLocation(action.Actor, root.parent.state.Predicates);
			string npcAfter = KnowledgeAnnotator.GetLocation(action.Actor, problem.initial);
			
			// If the player can see the action taking place...
			if (npcBefore.Equals(playerLocation) || npcAfter.Equals(playerLocation))
			{		
				// If the NPC was at a different location when their action started or when their action ends...		
				if (!npcBefore.Equals(playerLocation) || !npcAfter.Equals(playerLocation))
				{
					// Find all the game objects colocated with the player.
					List<GameObject> objs = stateManager.GetObjectsAt(playerLocation);
					
					// Create a container for the NPC's game object.
					GameObject actor = null;
					
					// If there are objects colocated with the player...
					if (objs != null)
						// try to find the NPC.
						actor = objs.Find(x => x.name.Equals(action.Actor));
						
					// If the NPC was not found...
					if (actor == null)
					{
						// Get the objects at the NPC location where the action started.
						objs = stateManager.GetObjectsAt(npcBefore);
						
						// Of there are objects colocated with the NPC's starting location...
						if (objs != null)
							// try to find the NPC.
							actor = objs.Find(x => x.name.Equals(action.Actor));
					}
					
					// If we found the NPC...
					if (actor != null)
					{
						// Get the NPC's script.
						NPC npc = actor.GetComponent<NPC>();
						
						// Move the NPC to their new location.
						//npc.MoveTo(npcAfter);
						npc.AcceptInstruction(action);
						
						// Remove the NPC from their previous location.
						stateManager.RemoveObject(action.Actor, npcBefore);
					}
					// ...if we didn't find the NPC...
					else
					{
						// Find the two rooms.
						GameObject beforeChunk = GameObject.Find(npcBefore);
						GameObject openTile = stateManager.GetOpenTile(beforeChunk);
						GameObject afterChunk = GameObject.Find(npcAfter);
						
						// Instantiate the actor in the before room.
						actor = Instantiate(Resources.Load(stateManager.Type(action.Actor), typeof(GameObject)), openTile.transform.position, Quaternion.identity) as GameObject;
						
						// Get the NPC's script.
						NPC npc = actor.GetComponent<NPC>();
						
						// Move the NPC to their new location.
						//npc.MoveTo(npcAfter);
						npc.AcceptInstruction(action);
						
						// Set the new object's parent.
						actor.transform.parent = afterChunk.transform;
						
						// Name the NPC game object.
						actor.name = action.Actor;
					}
					
					// Add the NPC game object to the state manager.
					stateManager.PutObject(actor, npcAfter);
				}
			}
		}
		
		frontierThread.Start();
	}
	
	// Expand the frontier.
	private void ExpandFrontier ()
	{	
		foreach (StateSpaceEdge edge in root.outgoing)
		{
			StateSpaceNode newNode = StateSpaceMediator.ExpandTree (domain, problem, plan, state, edge, 0);
			frontier.Add(edge, newNode);
		}
	}
	
	public bool ValidState ()
	{
		return !validState;
	}
	
	public void RestartGame ()
	{
		stateManager.PlayerTurn = false;
		validState = false;
		StartCoroutine(Restart());
	}
	
	public void EndTheGame ()
	{
		stateManager.PlayerTurn = false;
		validState = false;
		StartCoroutine(EndGame());
	}
	
	private IEnumerator Restart()
	{
		yield return new WaitForSeconds(2.5f);
		Application.LoadLevel("Game");
	}
	
	private IEnumerator EndGame()
	{
		yield return new WaitForSeconds(2.5f);
		Application.LoadLevel("End");
	}
}
