using UnityEngine;
using System.Collections;

public class Unit : MonoBehaviour {

	public Vector3 unitPlaceOffset = new Vector3(0, 3.5f, 0);

    public int teamNumber;
    public int energyLeft;
    public int attackPower;

    public float health;
    public float moveSpeed = 1.2f;
    public float moveAnimCurveHeightAmplifier;
    public float shadeFactor;

    public Transform tileOwned;
    public AnimationCurve moveAnimCurve;

    private Transform myTransform;
    private Renderer myRenderer;

    private void OnEnable() {
        GameScenario.OnUnitSelected += OnUnitSelected;
    }

    private void OnDisable() {
        GameScenario.OnUnitSelected -= OnUnitSelected;
    }

    private void OnUnitSelected(Unit u) {

        if(myRenderer == null) myRenderer = GetComponent<Renderer>();

        if(u == this) {
            myRenderer.material.color = new Color( (1.0f - shadeFactor) * GameScenario.Instance.teams[teamNumber].teamColor.r, 
                (1.0f - shadeFactor) * GameScenario.Instance.teams[teamNumber].teamColor.g, 
                (1.0f - shadeFactor) * GameScenario.Instance.teams[teamNumber].teamColor.b, 1);
        }
        else {
            myRenderer.material.color = GameScenario.Instance.teams[teamNumber].teamColor;
        }
    }

    public void ChangeYAxisOffset() {
        myTransform.position = tileOwned.position + unitPlaceOffset;
    }

    public void AssignValues(int team, Transform tile) {
        int retryCount = 0;

        myTransform = transform;
        teamNumber = team;
        attackPower = Random.Range(15,30);
        health = 100;

        //Ensure this tile is not reserved. If it is, find next free. Allow only 10 tries.
        if(tile.GetComponent<HexUnit>().ReserveHex(this)) {
            Debug.Log("Hex reserved!");
        }

        else {
            tile = TerrainGenerator.Instance.GetNextFreeHex(tile);

            while( (tile == null || !tile.GetComponent<HexUnit>().ReserveHex(this) ) && retryCount < 10 ) {
                tile = TerrainGenerator.Instance.GetNextFreeHex(tile);
                retryCount++;
            }

//            Debug.Log("Place for unit found after " + retryCount + " iterations.");
            tile.GetComponent<HexUnit>().ReserveHex(this);
        }

        myTransform.position = tile.position + unitPlaceOffset;
        tileOwned = tile;
    }

    public void MoveToTile(Transform newTile) {
    	int moveCost = TerrainGenerator.Instance.CalculatePathDistance(TerrainGenerator.Instance.FindTileIndex(tileOwned), TerrainGenerator.Instance.FindTileIndex(newTile), 0);
		
		if(moveCost <= energyLeft) {
			energyLeft -= moveCost;

	    	tileOwned.GetComponent<HexUnit>().FreeHex();
	    	tileOwned = newTile;
	    	tileOwned.GetComponent<HexUnit>().ReserveHex(this);
	    	
	    	StopCoroutine("MoveToTarget");
	    	StartCoroutine("MoveToTarget", newTile.position + unitPlaceOffset);

	    	GameScenario.Instance.OnUnitChange(false);
	    }
    }

    public void ReceiveDamage(int damage) {
    	health -= damage;

    	if(health <= 0) {
            tileOwned.GetComponent<HexUnit>().FreeHex();
    		GameScenario.Instance.teams[teamNumber].units.Remove(this);
    	}

    	GameScenario.Instance.OnUnitChange(false);
    }

    public void SetEnergy(int e) {
    	energyLeft = e;
    }

    void Start () {
        myTransform = transform;
        myRenderer = GetComponent<Renderer>();
        myRenderer.material.color = GameScenario.Instance.teams[teamNumber].teamColor;
    }

    IEnumerator MoveToTarget(Vector3 targetPos) {
    	Debug.Log("Moving to target "+targetPos);
    	float totalDistance = Vector3.Distance(targetPos, myTransform.position);
        Vector3 startPosition = myTransform.position;
    	float distance = totalDistance;

        for(int i = 0; i < 50; i++) {
            GameScenario.Instance.aimLine.SetPosition(0, myTransform.position);
            myTransform.position = Vector3.Lerp(myTransform.position, startPosition + new Vector3(0, 3, 0), Time.deltaTime * moveSpeed);
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(0.1f);

    	while(distance > 0.1f) {
            GameScenario.Instance.aimLine.SetPosition(0, myTransform.position);
    		myTransform.position = Vector3.Lerp(myTransform.position, targetPos + new Vector3(0, 3, 0), Time.deltaTime * moveSpeed);
    		distance = Vector2.Distance( new Vector2(targetPos.x, targetPos.z), new Vector2(myTransform.position.x, myTransform.position.z));

    		//myTransform.position += new Vector3(0, moveAnimCurve.Evaluate( distance / totalDistance), 0) * moveAnimCurveHeightAmplifier;

    		yield return new WaitForEndOfFrame();
    	}

        yield return new WaitForSeconds(0.1f);

        for(int i = 0; i < 50; i++) {
            GameScenario.Instance.aimLine.SetPosition(0, myTransform.position);
            myTransform.position = Vector3.Lerp(myTransform.position, targetPos, Time.deltaTime * moveSpeed);
            yield return new WaitForEndOfFrame();
        }

        Debug.Log("Move Ended!");
    }
}
