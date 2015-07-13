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
    public GameObject explosionPrefab;
    public GameObject bulletPrefab;
    public GameObject hitPrefab;
    public CameraController camControl;
    public Camera cam;
    public LineRenderer aimLine;

    //Game Settings
    public Vector3 unitSpawnOffset;
    public float turnTime;
    public float maxAttackDistance;
    public int unitsPerTeam;
    public int energyBudget;
    public int bombReach;
    public int riseReach;
    public int attackCost;
    public int bombCost;
    public int riseCost;

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

    public CanvasGroup toastCanvas;
    public Text toastText;
    public float toastShowTime;

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
    public Text attackCostLabel;
    public Text bombCostLabel;
    public Text riseCostLabel;

    public CanvasGroup victoryCanvas;
    public Text victoryTeamText;

    private float attackHitChance;
    private bool gameStarted;

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

    public delegate void SelectUnit(Unit u);
    public static event SelectUnit OnUnitSelected;

    #endregion

    void Start () {
        _instance = this;

        timerLabel.text = "Preparing level...";
        riseCostLabel.text = riseCost.ToString();
        bombCostLabel.text = bombCost.ToString();
        attackCostLabel.text = attackCost.ToString();

        GameStart();

        HideTimeLeftLabel();
    }

    
    #region Turns

    private void CreateGameTeams() {
        for(int i = 0; i < teams.Count; i++) {

            int spawnOriginTile;
            if(i == 0) spawnOriginTile = (int) UnityEngine.Random.Range(TerrainGenerator.Instance.levelHeight, TerrainGenerator.Instance.levelHeight * 2 - unitsPerTeam);
            else spawnOriginTile = (int) UnityEngine.Random.Range(TerrainGenerator.Instance.levelHeight * (TerrainGenerator.Instance.levelHeight - 4 + unitsPerTeam), TerrainGenerator.Instance.levelHeight * (TerrainGenerator.Instance.levelHeight - 1 - unitsPerTeam));

            Transform spawnOriginHex = TerrainGenerator.Instance.hexes[spawnOriginTile];

            for(int j = 0; j < unitsPerTeam; j++) {

                Vector3 spawnPos = TerrainGenerator.Instance.GetNextFreeHex(TerrainGenerator.Instance.hexes[spawnOriginTile]).position;

                Unit u = ((GameObject) Instantiate(unitPrefab, spawnPos + unitSpawnOffset, Quaternion.identity)).GetComponent<Unit>();

                u.AssignValues(i, spawnOriginHex, i * unitsPerTeam + j);

                teams[i].units.Add(u);
            }
        }

        gameStarted = true;
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
    	aimLine.enabled = false;

    	OnClearTilesSelection();

        if(activePlayer == -1) {
            activePlayer = 0;
        }

        else if(activePlayer == 0) activePlayer = 1;
        else if(activePlayer == 1) activePlayer = 0;

        for(int i = 0; i < teams[activePlayer].units.Count; i++) {
    		teams[activePlayer].units[i].SetEnergy(energyBudget);
    	}

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

        activeUnit = 0;
        OnUnitChange(true);
        
        actionMode = SelectedMode.None;
        OnModeUpdated();
        
        if(teams[activePlayer].logicType == Team.Logic.AI) AIPlayer.Instance.PosessControl(teams[activePlayer].units[0]);
    }

    public void GameOver(int teamWonIndex) {
    	victoryCanvas.interactable = true;
    	victoryCanvas.blocksRaycasts = true;
    	victoryCanvas.alpha = 1;

    	victoryTeamText.text = "Team "+(teamWonIndex+1) + " won!";
    	victoryTeamText.color = teams[teamWonIndex].teamColor;
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
    
    public float ComputeAttackChance(Transform attacker, Transform victim) {
        float distance = Vector3.Distance(attacker.position, victim.position);
    	float chance = Mathf.Clamp(1 - (distance / maxAttackDistance), 0, 1) * 100;
        attackHitChance = chance;
        
    	if (Physics.Linecast(attacker.position, victim.position, 1 << 9)) {
    		chance = -1;
        }
        
        if(actionMode == SelectedMode.Bomb) chance = 100;
        
        return chance;
    }

    public void ShowAttackInformation(Unit unit) {
   
    	StopCoroutine("HideAttackInformation");

        float chance = ComputeAttackChance(teams[activePlayer].units[activeUnit].transform, unit.transform);
        
    	unitAttackTargetCanvas.alpha = 1;
    	unitAttackTargetCanvas.transform.position = cam.WorldToScreenPoint(unit.transform.position);
    	unitAttackTargetChanceToHitLabel.text = chance < 0 ? "NaN" : (chance.ToString("f2") + "%");
    	unitAttackTargetHealthLabel.text = unit.health.ToString("f0") + " / 100";
    	unitAttackTargetHealthBackgroundLabel.text = unit.health.ToString("f0") + " / 100";
    	unitAttackTargetHealthLabel.GetComponent<RectTransform>().rect.Set(0, 0, unit.health, 40);

    	StartCoroutine("HideAttackInformation");
    }
    
    IEnumerator HideAttackInformationLonger() {
        
        yield return new WaitForSeconds(1f);
        unitAttackTargetCanvas.alpha = 0;
    }

    IEnumerator HideAttackInformation() {
    	yield return new WaitForSeconds(0.03f);
    	unitAttackTargetCanvas.alpha = 0;
    }
    
    public void Attack(Unit targetUnit) {
        bool isHit = (UnityEngine.Random.Range(0, 100) < attackHitChance);
		GameObject b = Instantiate(bulletPrefab, teams[activePlayer].units[activeUnit].transform.position, Quaternion.identity) as GameObject;
		b.GetComponent<Bullet>().SetTarget(targetUnit.transform, isHit);
        
        

		if(isHit) {
			Debug.Log("Hit! Damage: "+teams[activePlayer].units[activeUnit].attackPower);
			targetUnit.ReceiveDamage(teams[activePlayer].units[activeUnit].attackPower);
		}
		else { //MISS!
			ShowToast("Miss!");
		}

		teams[activePlayer].units[activeUnit].energyLeft -= attackCost;
    }

    public void ProcessClick(Transform target) {
    	if(gameStarted) {
	    	int currentUnitIndex = TerrainGenerator.Instance.FindTileIndex(teams[activePlayer].units[activeUnit].tileOwned);
	    	int targetIndex = 0;

	    	if(target.gameObject.name.Contains("Hex")) targetIndex = TerrainGenerator.Instance.FindTileIndex(target);
	    	else if(target.gameObject.name.Contains("Unit")) {
	    		Debug.Log("Clicked unit, finding owned tile instead.");
	    		targetIndex = TerrainGenerator.Instance.FindTileIndex(target.GetComponent<Unit>().tileOwned);
	    	}

	    	if(!EventSystem.current.IsPointerOverGameObject()) {
		        if(teams[activePlayer].logicType == Team.Logic.Human) {
		        	switch(actionMode) {
		        		case(SelectedMode.Move):
		        			aimLine.enabled = false;
		        			if(target.gameObject.name.Contains("Hex") && target.GetComponent<HexUnit>().isAvailableForMovement) {
		        				teams[activePlayer].units[activeUnit].GetComponent<Unit>().MoveToTile(target);
		        				OnClearTilesSelection();
                                
                                actionMode = SelectedMode.None;
                                OnModeUpdated();
		        			} else {
		        				ShowToast("Not Enough energy!");
		        			}
		        			break;

		        		case(SelectedMode.Attack):
		        			OnClearTilesSelection();
		        			if(teams[activePlayer].units[activeUnit].energyLeft >= attackCost) {
			        			Unit targetUnit = TerrainGenerator.Instance.hexes[targetIndex].GetComponent<HexUnit>().Owner;
			        			if(targetUnit != null) {
    			        			Attack(targetUnit);
				        		}
				        	}
				        	else {
				        		ShowToast("Not Enough energy!");
				        	}

		        			break;

		        		case(SelectedMode.Bomb):
		        			Debug.Log("Bomb!");
		        			OnClearTilesSelection();
		        			aimLine.enabled = false;

		        			if(teams[activePlayer].units[activeUnit].energyLeft >= bombCost) {
			        			if(TerrainGenerator.Instance.CalculatePathDistance(currentUnitIndex, targetIndex, 0) <= bombReach) {
			        				Transform epicenter = TerrainGenerator.Instance.hexes[targetIndex];
			        				epicenter.GetComponent<HexUnit>().Rise(-0.05f);

                                    if(epicenter.GetComponent<HexUnit>().Owner != null) epicenter.GetComponent<HexUnit>().Owner.ReceiveDamage(15);

			        				foreach(Transform t in TerrainGenerator.Instance.GetHexNeighbours(epicenter)) {
                                        HexUnit hex = t.GetComponent<HexUnit>();

			        					hex.Rise(UnityEngine.Random.Range(-0.03f, -0.02f));
                                        if(hex.Owner != null) hex.Owner.ReceiveDamage(15);
			        				}

			        				Instantiate(explosionPrefab, new Vector3(0, 2, 0) + TerrainGenerator.Instance.hexes[targetIndex].position, Quaternion.identity);
			        				teams[activePlayer].units[activeUnit].energyLeft -= bombCost;
			        			}
			        			else {
			        				ShowToast("Too far!");
			        			}
			        		}
			        		else {
			        			ShowToast("Not Enough energy!");
			        		}

		        			break;

		        		case(SelectedMode.Rise):
		        			Debug.Log("Rise!");
		        			OnClearTilesSelection();
		        			aimLine.enabled = false;
		        			
		        			if(teams[activePlayer].units[activeUnit].energyLeft >= bombCost) {
			        			if(TerrainGenerator.Instance.CalculatePathDistance(currentUnitIndex, targetIndex, 0) <= riseReach) {
			        				Transform epicenter = TerrainGenerator.Instance.hexes[targetIndex];
			        				epicenter.GetComponent<HexUnit>().Rise(0.05f);

			        				foreach(Transform t in TerrainGenerator.Instance.GetHexNeighbours(epicenter)) {
			        					t.GetComponent<HexUnit>().Rise(UnityEngine.Random.Range(0.03f, 0.02f));
			        				}

			        				teams[activePlayer].units[activeUnit].energyLeft -= bombCost;
			        			}
			        			else {
			        				ShowToast("Too far!");
			        			}
			        		}
			        		else {
			        			ShowToast("Not Enough energy!");
			        		}
		        			break;

		        		default:
		        			break;
		        	}
		        }
		        //This player is controller by AI!
		        else {

		        }
		    }

		    OnUnitChange(false);
		}
    }

    public void OnModeUpdated() {
        Debug.Log("Mode updated: " + actionMode.ToString());
        OnClearTilesSelection();

        int activeUnitTile = TerrainGenerator.Instance.FindTileIndex(teams[activePlayer].units[activeUnit].tileOwned);
        
        HighlightIcon(actionMode);
        
        switch(actionMode) {
    		case(SelectedMode.Move):
    			TerrainGenerator.Instance.HighlightAvailableToMoveTiles(activeUnitTile, teams[activePlayer].units[activeUnit].energyLeft + 1, 0);
    			break;

    		case(SelectedMode.Attack):
    			aimLine.enabled = true;
    			break;

    		case(SelectedMode.Bomb):
    			TerrainGenerator.Instance.HighlightAvailableToMoveTiles(activeUnitTile, bombReach + 1, 0);
    			break;

    		case(SelectedMode.Rise):
    			TerrainGenerator.Instance.HighlightAvailableToMoveTiles(activeUnitTile, riseReach + 1, 0);
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
    	if(teams[activePlayer].units[activeUnit].energyLeft >= attackCost)
        	actionMode = SelectedMode.Attack;
        else ShowToast("Not enough energy!");
        OnModeUpdated();
    }

    public void SelectBomb() {
    	if(teams[activePlayer].units[activeUnit].energyLeft >= bombCost)
        	actionMode = SelectedMode.Bomb;
        else ShowToast("Not enough energy!");
        OnModeUpdated();
    }

    public void SelectRise() {
    	if(teams[activePlayer].units[activeUnit].energyLeft >= riseCost)
        	actionMode = SelectedMode.Rise;
        else ShowToast("Not enough energy!");
        OnModeUpdated();
    }

    public void EndTurn() {
    	NextTurn();
    }
    
    private void HighlightIcon(SelectedMode mode) {
        
        attackIcon.color = normalColor;
        bombIcon.color = normalColor;
        moveIcon.color = normalColor;
        riseIcon.color = normalColor;
        
        if(mode == SelectedMode.Attack) attackIcon.color = activeColor;
        else if(mode == SelectedMode.Move) moveIcon.color = activeColor;
        else if(mode == SelectedMode.Bomb) bombIcon.color = activeColor;
        else if(mode == SelectedMode.Rise) riseIcon.color = activeColor;
    }

    #endregion
    
    #region UnitManagement
    
    private void Update() {
        if(gameStarted) {
            if(Input.GetKeyUp(KeyCode.A)) {
    	    	PreviousUnit();
    	    }
    	    else if(Input.GetKeyUp(KeyCode.D)) {
    	    	NextUnit();
    	    }
        }
    }

    public void OnUnitChange(bool flyOverUnit = true) {
        aimLine.enabled = false;
        
    	if(teams[0].units.Count == 0) { //Team 2 wins
    		gameStarted = false;

    		GameOver(1);
    	}
    	else if(teams[1].units.Count == 0) { //Team 1 wins
    		gameStarted = false;

    		GameOver(0);
    	}
    	else {

	    	Unit u = teams[activePlayer].units[activeUnit];
	        if(flyOverUnit) camControl.FlyOverPosition(u.transform.position);

	        OnUnitSelected(u);

	        attackLabel.text = u.attackPower.ToString();
	        healthLabel.text = u.health.ToString() + " / 100";
	        energyLeftLabel.text = u.energyLeft.ToString();

	        if(u.energyLeft > 1) energyLeftLabel.color = Color.white;
	        else if(u.energyLeft == 1) energyLeftLabel.color = Color.yellow;
	        else if(u.energyLeft == 0) energyLeftLabel.color = Color.red;

	        aimLine.SetPosition(0, u.transform.position);
            
	    }
    }

    public void NextUnit() {
        actionMode = SelectedMode.None;
        OnModeUpdated();
        
    	OnClearTilesSelection();
    	if(activeUnit < teams[activePlayer].units.Count - 1) {
    		activeUnit++;
    	}
    	else {
    		activeUnit = 0;
    	}

    	OnUnitChange(false);
        
        if(teams[activePlayer].logicType == Team.Logic.AI) AIPlayer.Instance.PosessControl(teams[activePlayer].units[activeUnit]);
    }

    public void PreviousUnit() {
    	OnClearTilesSelection();
    	if(activeUnit > 0) {
    		activeUnit--;
    	}
    	else {
    		activeUnit = teams[activePlayer].units.Count - 1;
    	}

    	OnUnitChange(false);
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

    public void ShowToast(string message) {
    	toastText.text = message;
    	StopCoroutine("ToastAnimation");
    	StartCoroutine("ToastAnimation");
    }

    public Team GetOpponent() {
    	if(activePlayer == 1) return teams[0];
    	else return teams[1];
    }

    IEnumerator ToastAnimation() {
    	for(int i = 0; i < 25; i++) {
    		toastCanvas.alpha = 0.04f * i;
    		yield return new WaitForEndOfFrame();
    	}

    	yield return new WaitForSeconds(toastShowTime);

    	for(int i = 0; i < 25; i++) {
    		toastCanvas.alpha = 1 - (0.04f * i);
    		yield return new WaitForEndOfFrame();
    	}
    }

    public void RestartScene() {
    	Application.LoadLevel(Application.loadedLevel);
    }

    #endregion
}
