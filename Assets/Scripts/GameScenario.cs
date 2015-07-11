using UnityEngine;
using System;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class GameScenario : MonoBehaviour {

    //Referencies
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
    public CanvasGroup unitControllerCanvas;

    //In-Game variables
    public int activePlayer;
    public int activeUnit;
    public int turnNumber;
    public SelectedMode actionMode = SelectedMode.None;

    public enum SelectedMode {
        None,
        Move,
        Attack,
        Bomb,
        Rise
    }

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

    public delegate void ClearTilesSelection();
    public static event ClearTilesSelection OnClearTilesSelection;

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
        timeLeftLabel.color = new Color(1, 1, 1, 0);
    }

    private void ShowTimeLeftLabel() {
        timeLeftLabel.color = new Color(1, 1, 1, 1);
    }

    IEnumerator UpdateGridAfterDelay() {
        yield return new WaitForSeconds(1f);

        for(int i = 0; i < 100; i++) {
            terrainGen.perlinAmplify += 1 / (i + 1f) ;
            yield return new WaitForSeconds(0.0333f);
        }

        yield return new WaitForSeconds(0.5f);
        Debug.Log("Rescan...");
        //AstarPath.active.Scan();

        CreateGameTeams();
    }

    private void CreateGameTeams() {
        for(int i = 0; i < teams.Count; i++) {

            int spawnOriginTile;
            if(i == 0) spawnOriginTile = (int) UnityEngine.Random.Range(TerrainGenerator.Instance.levelHeight, TerrainGenerator.Instance.levelHeight * 2);
            else spawnOriginTile = (int) UnityEngine.Random.Range(TerrainGenerator.Instance.levelHeight * (TerrainGenerator.Instance.levelHeight - 4), TerrainGenerator.Instance.levelHeight * (TerrainGenerator.Instance.levelHeight - 1));

            Transform spawnOriginHex = TerrainGenerator.Instance.hexes[spawnOriginTile];

            for(int j = 0; j < unitsPerTeam; j++) {

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
        timerLabel.text = "Player " + (activePlayer + 1) + " turn!";

        yield return new WaitForSeconds(3f);

        ShowTimeLeftLabel();
        while(timeLeft > 0f) {
            yield return new WaitForEndOfFrame();
            timeLeft -= Time.deltaTime;
            timerLabel.text = "0:" + timeLeft.ToString("f2").Replace(".", ":");
        }

        timeLeftLabel.color = new Color(1, 1, 1, 1);
        Debug.Log("Turn is over!");

        NextTurn();
    }

    private void NextTurn() {

    	OnClearTilesSelection();

        if(activePlayer == -1) {
            activePlayer = 0;
            unitControllerCanvas.alpha = 1;
            unitControllerCanvas.interactable = true;
            unitControllerCanvas.blocksRaycasts = true;
        }

        else if(activePlayer == 0) activePlayer = 1;
        else if(activePlayer == 1) activePlayer = 0;

        StartExpirationTimer();

        Unit u = teams[activePlayer].units[activeUnit];
        camControl.FlyOverPosition(u.transform.position);
    }

    public void ProcessClick(Transform target) {
    	if(!EventSystem.current.IsPointerOverGameObject()) {
	        if(activePlayer == 0) {
	        	switch(actionMode) {

	        		case(SelectedMode.Move):
	        			if(target.GetComponent<HexUnit>().isAvailableForMovement) {
	        				teams[activePlayer].units[activeUnit].GetComponent<Unit>().MoveToTile(target);
	        				OnClearTilesSelection();
	        			} else {
	        				Debug.Log("Selected tile is not available or too far!");
	        			}
	        			break;

	        		case(SelectedMode.Attack):
	        			OnClearTilesSelection();
	        			break;

	        		case(SelectedMode.Bomb):
	        			OnClearTilesSelection();
	        			break;

	        		case(SelectedMode.Rise):
	        			OnClearTilesSelection();
	        			break;

	        		default:
	        			break;
	        	}
	        }
	    }
    }

    public void OnModeUpdated() {
        Debug.Log("Mode updated: " + actionMode.ToString());

        switch(actionMode) {
    		case(SelectedMode.Move):
    			TerrainGenerator.Instance.HighlightAvailableToMoveTiles(teams[activePlayer].units[activeUnit].tileOwned, 4, 0);
    			break;

    		case(SelectedMode.Attack):
    			OnClearTilesSelection();
    			break;

    		case(SelectedMode.Bomb):
    			OnClearTilesSelection();
    			break;

    		case(SelectedMode.Rise):
    			OnClearTilesSelection();
    			break;

    		default:
    			break;
	    }
    }

    public void SelectMove() {
        actionMode = SelectedMode.Move;
        OnModeUpdated();
    }

    public void SelectNone() {
        actionMode = SelectedMode.None;
        OnModeUpdated();
    }

    public void SelectAttack() {
        actionMode = SelectedMode.Attack;
        OnModeUpdated();
    }

    public void SelectBomb() {
        actionMode = SelectedMode.Bomb;
        OnModeUpdated();
    }

    public void SelectRise() {
        actionMode = SelectedMode.Rise;
        OnModeUpdated();
    }

    private void OnUnitChange() {
    	Unit u = teams[activePlayer].units[activeUnit];
        camControl.FlyOverPosition(u.transform.position);
    }

    public void NextUnit() {
    	OnClearTilesSelection();
    	if(activeUnit < teams[activePlayer].units.Count - 1) {
    		activeUnit++;
    	}
    	else {
    		activeUnit = 0;
    	}

    	OnUnitChange();
    }

    public void PreviousUnit() {
    	OnClearTilesSelection();
    	if(activeUnit > 0) {
    		activeUnit--;
    	}
    	else {
    		activeUnit = teams[activePlayer].units.Count - 1;
    	}

    	OnUnitChange();
    }
}
