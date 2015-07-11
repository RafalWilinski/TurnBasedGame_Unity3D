using UnityEngine;
using System.Collections;

public class Unit : MonoBehaviour {


    public int teamNumber;
    public float health;

    private Transform myTransform;
    private Renderer myRenderer;

    public void AssignValues(int team, Transform tile) {

        this.teamNumber = team;
        int retryCount = 0;

        myTransform = transform;

        if(tile.GetComponent<HexUnit>().ReserveHex(this)) {
            Debug.Log("Hex reserved!");
            myTransform.position = tile.position + new Vector3(0, 3.5f, 0);
        }

        else {
            tile = TerrainGenerator.Instance.GetNextFreeHex(tile);

            while( (tile == null || !tile.GetComponent<HexUnit>().ReserveHex(this) ) && retryCount < 10 ) {
                tile = TerrainGenerator.Instance.GetNextFreeHex(tile);
                retryCount++;
            }

            Debug.Log("Place for unit found after " + retryCount + " iterations.");
            tile.GetComponent<HexUnit>().ReserveHex(this);
            myTransform.position = tile.position + new Vector3(0, 3.5f, 0);
        }
    }

    void Start () {
        myTransform = transform;
        myRenderer = GetComponent<Renderer>();
        myRenderer.material.color = GameScenario.Instance.teams[teamNumber].teamColor;

        //StartCoroutine("PlaceOnHex");
    }
}
