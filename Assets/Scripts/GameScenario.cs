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
    public Camera cam;

    //Game Settings
    public Vector3 unitSpawnOffset;
    public float turnTime;
    public float unitsSpawnSpread;
    public int unitsPerTeam;
    public int actionBudget;
    public int maxTraverseDistance;

    //INTERFACE Referencies
    public Text timeLeftLabel;
    public Text timerLabel;

    public CanvasGroup unitInformationCanvas;
    public Text unitInformationHealthLabel;
    public Text unitInformationHealthBackgroundLabel;
    public Text unitInformationAttackLabel;

    public CanvasGroup unitAttackTargetCanvas;
    public Text unitAttackTargetHealthLabel;
    public Text unitAttackTargetHealthBackgroundLabel;
    public Text unitAttackTargetChanceToHitLabel;

    public CanvasGroup unitControllerCanvas;
    public Text energyLeftLabel;
    public Text healthLabel;
    public Text attackLabel;
    public Image moveIcon;
    public Image attackIcon;
    public Image bombIcon;
    public Image riseIcon;
    public Color normalColor;
    public Color activeColor;

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

    #region Singleton
    private static GameScenario _instance;
    public static GameScenario Instance {
        get {
            return _instance; //A bit unsafe but we assign _instance at start so theres nothing to worry about
        }
    }

    #endregion


    #region Events

    public delegate void ClearTilesSelection();
    public static event ClearTilesSelection OnClearTilesSelection;

    #endregion

    void Start () {
        _instance = this;
        GameStart();

        timerLabel.text = "Preparing level...";
        HideTimeLeftLabel();
    }

    
    #region Turns

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
        }

        else if(activePlayer == 0) activePlayer = 1;
        else if(activePlayer == 1) activePlayer = 0;

        if(teams[activePlayer].logicType == Team.Logic.Human) {
        	unitControllerCanvas.alpha = 1;
            unitControllerCanvas.interactable = true;
            unitControllerCanvas.blocksRaycasts = true;
        }
        else {
        	unitControllerCanvas.alpha = 0;
            unitControllerCanvas.interactable = false;
            unitControllerCanvas.blocksRaycasts = false;
        }

        StartExpirationTimer();

        OnUnitChange(true);
    }

    #endregion



    #region UI
    
    public void ShowUnitInformation(Unit unit) {
    	StopCoroutine("HideUnitInformation");

    	unitInformationCanvas.alpha = 1;

    	unitInformationCanvas.transform.position = cam.WorldToScreenPoint(unit.transform.position);
    	unitInformationHealthLabel.text = unit.health.ToString("f0") + " / 100";
    	unitInformationHealthBackgroundLabel.text = unit.health.ToString("f0") + " / 100";
    	unitInformationHealthLabel.GetComponent<RectTransform>().rect.Set(0, 0, unit.health, 40);
    	unitInformationAttackLabel.text = "DMG: "+unit.attackPower;

    	StartCoroutine("HideUnitInformation");
    }

    IEnumerator HideUnitInformation() {
    	yield return new WaitForSeconds(0.03f);
    	unitInformationCanvas.alpha = 0;
    }

    public void ProcessClick(Transform target) {
    	if(!EventSystem.current.IsPointerOverGameObject()) {
	        if(teams[activePlayer].logicType == Team.Logic.Human) {
	        	switch(actionMode) {

	        		case(SelectedMode.Move):
	        			if(target.GetComponent<HexUnit>().isAvailableForMovement) {
	        				teams[activePlayer].units[activeUnit].GetComponent<Unit>().MoveToTile(target);
	        				OnClearTilesSelection();
	        			} else {
//	        				Debug.Log("Selected tile is not available or too far!");
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
	        //This player is controller by AI!
	        else {

	        }
	    }
    }

    public void OnModeUpdated() {
        Debug.Log("Mode updated: " + actionMode.ToString());

        switch(actionMode) {
    		case(SelectedMode.Move):
    			TerrainGenerator.Instance.HighlightAvailableToMoveTiles(teams[activePlayer].units[activeUnit].tileOwned, teams[activePlayer].units[activeUnit].energyLeft + 1, 0);
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

    public void EndTurn() {
    	NextTurn();
    }

    #endregion
    
    #region UnitManagement

    public void OnUnitChange(bool flyOverUnit = true) {
    	Unit u = teams[activePlayer].units[activeUnit];
        if(flyOverUnit) camControl.FlyOverPosition(u.transform.position);

        attackLabel.text = u.attackPower.ToString();
        healthLabel.text = u.health.ToString() + " / 100";
        energyLeftLabel.text = u.energyLeft.ToString();

        if(u.energyLeft > 1) energyLeftLabel.color = Color.white;
        else if(u.energyLeft == 1) energyLeftLabel.color = Color.yellow;
        else if(u.energyLeft == 0) energyLeftLabel.color = Color.red;
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

    #endregion


    #region HelperFunctions
    
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
    #endregion
}
