using UnityEngine;
using System;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;

public class GameScenario : MonoBehaviour {

	public TerrainGenerator terrainGen;
	public GameObject unitPrefab;
	public CameraController camControl;

	//Game Settings
	public Vector3 unitSpawnOffset;
	public float turnTime;
	public float unitsSpawnSpread;
	public int unitsPerTeam;
	public int actionBudget;
	public int maxTraverseDistance;

	public Text timeLeftLabel;
	public Text timerLabel;

	//In-Game variables
	public int activePlayer;
	public int turnNumber;

	[Serializable]
	public class Team {
		public List<Unit> units;
		public Color teamColor;
		public Logic logicType;

		public enum Logic {
			Human,
			AI
		}
	}

	public List<Team> teams;

	//Singleton
	private static GameScenario _instance;
	public static GameScenario Instance {
		get {
			return _instance; //A bit unsafe but we assign _instance at start so theres nothing to worry about
		}
	}

	void Start () {
		_instance = this;
		GameStart();

		timerLabel.text = "Preparing level...";
		HideTimeLeftLabel();
	}
	
	private void GameStart() {
		StartCoroutine("UpdateGridAfterDelay");
	}

	private void HideTimeLeftLabel() {
		timeLeftLabel.color = new Color(1,1,1,0);
	}

	private void ShowTimeLeftLabel() {
		timeLeftLabel.color = new Color(1,1,1,1);
	}

	IEnumerator UpdateGridAfterDelay() {
		for(int i = 0; i < 100; i++) {
			terrainGen.perlinAmplify += 1 / (i + 1f) ;
			yield return new WaitForSeconds(0.0333f);
		}

		yield return new WaitForSeconds(0.5f);
		Debug.Log("Rescan...");
		AstarPath.active.Scan();

		CreateGameTeams();
	}

	private void CreateGameTeams() {
		Vector3 previousPos = Vector3.zero;

		for(int i = 0; i < teams.Count; i++) {
			float xCap = TerrainGenerator.Instance.hexWidthSpacing * TerrainGenerator.Instance.levelWidth * 0.9f;
			float zCap = TerrainGenerator.Instance.hexHeightSpacing * TerrainGenerator.Instance.levelHeight * 0.9f;

			int spawnOriginTile;
			if(i == 0) spawnOriginTile = (int) UnityEngine.Random.Range(TerrainGenerator.Instance.levelHeight, TerrainGenerator.Instance.levelHeight * 2);
			else spawnOriginTile = (int) UnityEngine.Random.Range(TerrainGenerator.Instance.levelHeight * (TerrainGenerator.Instance.levelHeight - 4), TerrainGenerator.Instance.levelHeight * (TerrainGenerator.Instance.levelHeight - 1));

			Transform spawnOriginHex = TerrainGenerator.Instance.hexes[spawnOriginTile];
			Vector3 teamSpawnOrigin = spawnOriginHex.position;

			// Vector3 teamSpawnOrigin = new Vector3(UnityEngine.Random.Range(0, xCap), 10, UnityEngine.Random.Range(0, zCap));
			// if(i >= 1) {
			// 	while(Vector3.Distance(previousPos, teamSpawnOrigin) < 20) {
			// 		Debug.Log("Teams placed too tight, reshuffle!");
			// 		teamSpawnOrigin = new Vector3(UnityEngine.Random.Range(0, xCap), 10, UnityEngine.Random.Range(0, zCap));
			// 	}
			// }

			// previousPos = teamSpawnOrigin;

			for(int j = 0; j < unitsPerTeam; j++) {
				// Vector3 spawnVariation = new Vector3(UnityEngine.Random.Range(-unitsSpawnSpread, unitsSpawnSpread), 
				// 	0, UnityEngine.Random.Range(-unitsSpawnSpread,unitsSpawnSpread));
					
				Vector3 spawnPos = TerrainGenerator.Instance.GetNextFreeHex(TerrainGenerator.Instance.hexes[spawnOriginTile]).position;

				Unit u = ((GameObject) Instantiate(unitPrefab, spawnPos + unitSpawnOffset, Quaternion.identity)).GetComponent<Unit>();

				u.AssignValues(i, spawnOriginHex);

				teams[i].units.Add(u);
			}
		}

		NextTurn();
	}

	private void StartExpirationTimer() {
		StopCoroutine("TurnExpirationTimer");
		StartCoroutine("TurnExpirationTimer");
	}

	IEnumerator TurnExpirationTimer() {

		float timeLeft = turnTime;

		HideTimeLeftLabel();
		timerLabel.text = "Player "+activePlayer+" turn!";

		yield return new WaitForSeconds(3f);

		ShowTimeLeftLabel();
		while(timeLeft > 0f) {
			yield return new WaitForEndOfFrame();
			timeLeft -= Time.deltaTime;
			timerLabel.text = "0:"+timeLeft.ToString("f2").Replace(".", ":");
		}

		timeLeftLabel.color = new Color(1,1,1,1);
		Debug.Log("Turn is over!");

		NextTurn();
	}

	private void NextTurn() {
		if(activePlayer == -1) activePlayer = 0;
		else if(activePlayer == 0) activePlayer = 1;
		else if(activePlayer == 1) activePlayer = 0;

		StartExpirationTimer();

		Unit u = teams[activePlayer].units[0];
		camControl.FlyOverPosition(u.transform.position);
	}
}
