using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AIPlayer : MonoBehaviour {
	
	public float decisionWaitTime = 3.0f;
	
	private string logPrefix = "[AI] ";
	private static AIPlayer _instance;
	private Unit controlledUnit;
	
	public static AIPlayer Instance {
		get { 
			if(_instance == null) {
				GameObject g = new GameObject();
				_instance = g.AddComponent<AIPlayer>() as AIPlayer;
			}
			return _instance; 
		}
	}
	
	private void Awake() {
		_instance = this;
	}
	
	private void Log(string message) {
		Debug.Log(logPrefix + message);
	}

	public void PosessControl(Unit u) {
		controlledUnit = u;
		
		Log("Controlling unit "+u.unitNumber);
		
		bool shouldEndTurn = true;
		foreach(Unit unit in GameScenario.Instance.teams[GameScenario.Instance.activePlayer].units ) {
			if(unit.energyLeft > 0) shouldEndTurn = false;
		}
		
		if(shouldEndTurn) GameScenario.Instance.EndTurn();
		else StartCoroutine("Behave");
	}
	
	IEnumerator Behave() {

		foreach(Unit target in GameScenario.Instance.GetOpponent().units) { // Check shoot targets
			yield return new WaitForSeconds(decisionWaitTime / 2f);
			if(GameScenario.Instance.ComputeAttackChance(controlledUnit.transform, target.transform) > 50f) { //Shoot for real if chance is bigger than 50%
			
				Log("Chance bigger than 50%, shooting!");
				if(controlledUnit.energyLeft >= GameScenario.Instance.attackCost) {
					GameScenario.Instance.Attack(target);
				}
			}
		}
		
		//All possible shots are made. Spent rest of energy for moving.
		
		yield return new WaitForSeconds(decisionWaitTime);
		
		TerrainGenerator.Instance.HighlightAvailableToMoveTiles(TerrainGenerator.Instance.FindTileIndex(controlledUnit.tileOwned.transform), controlledUnit.energyLeft + 1, 0);
		
		Vector3 centerOfOpponents = Vector3.zero;
		foreach(Unit unitTarget in GameScenario.Instance.GetOpponent().units) {
			centerOfOpponents += unitTarget.transform.position;
		}
		
		centerOfOpponents = centerOfOpponents / GameScenario.Instance.GetOpponent().units.Count;
		
		Transform movementTargetTile = TerrainGenerator.Instance.GetHexAvailableClosestToPos(centerOfOpponents);
		
		controlledUnit.MoveToTile(movementTargetTile);
		
		
		yield return new WaitForSeconds(decisionWaitTime);
		
		Log("Nothing to do, next unit!");
		GameScenario.Instance.NextUnit();
	}
}
