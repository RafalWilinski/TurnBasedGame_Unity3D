using UnityEngine;
using System.Collections;

public class TerrainGenerator : MonoBehaviour {

    public GameObject hexBlock;
    public Transform[] hexes;
    public int levelWidth;
    public int levelHeight;
    public float hexWidthSpacing = 1.75f;
    public float hexHeightSpacing = 1.5f;
    public float hexCrookWidth = 0.87f;

    public float perlinScale;
    public float perlinAmplify;
    public Vector2 perlinOffset;

    private Vector2 oldPerlinOffset;
    private float oldPerlinAmplify;
    private float oldPerlinScale;

    private Transform myTransform;

    //Singleton
    private static TerrainGenerator _instance;
    public static TerrainGenerator Instance {
        get {
            return _instance;
        }
    }

    void Start () {
        _instance = this;
        myTransform = transform;

        oldPerlinOffset = perlinOffset;
        oldPerlinAmplify = perlinAmplify;
        oldPerlinScale = perlinScale;

        hexes = new Transform[levelHeight * levelWidth];

        for(int i = 0; i < levelWidth; i++) {
            for(int j = 0; j < levelHeight; j++) {
                float xPos = j * hexWidthSpacing + (i % 2 == 1 ? hexCrookWidth : 0);
                float zPos = i * hexHeightSpacing;
                float yPos = Mathf.PerlinNoise(xPos * perlinScale + perlinOffset.x, zPos * perlinScale + perlinOffset.y) * perlinAmplify;

                Vector3 position = new Vector3(xPos, yPos, zPos);

                GameObject t = Instantiate(hexBlock, position, Quaternion.Euler(new Vector3(-90, 0, 0))) as GameObject;
                hexes[i * levelWidth + j] = t.transform;
            }
        }
    }

    private void UpdateHexPositions() {
        for(int i = 0; i < levelWidth; i++) {
            for(int j = 0; j < levelHeight; j++) {
                float yPos = Mathf.PerlinNoise(hexes[i * levelWidth + j].position.x * perlinScale + perlinOffset.x, hexes[i * levelWidth + j].position.z * perlinScale + perlinOffset.y) * perlinAmplify;

                hexes[i * levelWidth + j].position = new Vector3(hexes[i * levelWidth + j].position.x, yPos, hexes[i * levelWidth + j].position.z);
                hexes[i * levelWidth + j].parent = myTransform;
            }
        }
    }

    private void Update() {
        if(oldPerlinAmplify != perlinAmplify || oldPerlinOffset != perlinOffset || oldPerlinScale != perlinScale) {
            UpdateHexPositions();
        }
    }

    public Transform GetNextFreeHex(Transform hex) {
        for(int i = 0; i < levelHeight * levelWidth; i++) {
            if(hexes[i] == hex) {
                return hexes[i + 1];
            }
        }

        Debug.Log("No next free hex found!");
        return null;
    }

    public int FindTileIndex(Transform tile) {
    	for(int i = 0; i < levelHeight * levelWidth; i++) {
            if(hexes[i] == tile) {
            	return i;
            }
        }

        Debug.LogWarning("Selected tile not found! Are you sure it belongs to grid?");
        return -1;
    }

    public int CalculatePathDistance(int baseIndex, int targetIndex, int distanceAccumulator) {
        int pathDistance = 0;
        int shortestPathDistance = 100;
        distanceAccumulator++;

        if(baseIndex == targetIndex) {
        	return distanceAccumulator - 1;
        }

        if(distanceAccumulator < 6) { //Stop calculating after 10 recursions
	        if((baseIndex / levelWidth) % 2 == 0) {
	            if(baseIndex - 1 - levelWidth >= 0) {
	            	pathDistance = CalculatePathDistance(baseIndex - 1 - levelWidth, targetIndex, distanceAccumulator);
	            	if(pathDistance < shortestPathDistance) shortestPathDistance = pathDistance;
	            }

	            if(baseIndex - levelWidth >= 0) {
	            	pathDistance = CalculatePathDistance(baseIndex - levelWidth, targetIndex, distanceAccumulator);
	            	if(pathDistance < shortestPathDistance) shortestPathDistance = pathDistance;
	            }

	            if(baseIndex - 1 >= 0) {
	            	pathDistance = CalculatePathDistance(baseIndex - 1, targetIndex, distanceAccumulator);
	            	if(pathDistance < shortestPathDistance) shortestPathDistance = pathDistance;
	            }

	            if(baseIndex + 1 >= 0) {
	            	pathDistance = CalculatePathDistance(baseIndex + 1, targetIndex, distanceAccumulator);
	            	if(pathDistance < shortestPathDistance) shortestPathDistance = pathDistance;
	            }

	            if(baseIndex + levelWidth >= 0) {
	            	pathDistance = CalculatePathDistance(baseIndex + levelWidth, targetIndex, distanceAccumulator);
	            	if(pathDistance < shortestPathDistance) shortestPathDistance = pathDistance;
	            }

	            if(baseIndex - 1 + levelWidth >= 0) {
	            	pathDistance = CalculatePathDistance(baseIndex - 1 + levelWidth, targetIndex, distanceAccumulator);
	            	if(pathDistance < shortestPathDistance) shortestPathDistance = pathDistance;
	            }
	        } 
	        else {

	        	if(baseIndex + 1 - levelWidth >= 0) {
	            	pathDistance = CalculatePathDistance(baseIndex + 1 - levelWidth, targetIndex, distanceAccumulator);
	            	if(pathDistance < shortestPathDistance) shortestPathDistance = pathDistance;
	            }

	            if(baseIndex - levelWidth >= 0) {
	            	pathDistance = CalculatePathDistance(baseIndex - levelWidth, targetIndex, distanceAccumulator);
	            	if(pathDistance < shortestPathDistance) shortestPathDistance = pathDistance;
	            }

	            if(baseIndex - 1 >= 0) {
	            	pathDistance = CalculatePathDistance(baseIndex - 1, targetIndex, distanceAccumulator);
	            	if(pathDistance < shortestPathDistance) shortestPathDistance = pathDistance;
	            }
	            if(baseIndex + 1 >= 0) {
	            	pathDistance = CalculatePathDistance(baseIndex + 1, targetIndex, distanceAccumulator);
	            	if(pathDistance < shortestPathDistance) shortestPathDistance = pathDistance;
	            }

	            if(baseIndex + levelWidth >= 0) {
	            	pathDistance = CalculatePathDistance(baseIndex + levelWidth, targetIndex, distanceAccumulator);
	            	if(pathDistance < shortestPathDistance) shortestPathDistance = pathDistance;
	            }

	            if(baseIndex + 1 + levelWidth >= 0) {
	            	pathDistance = CalculatePathDistance(baseIndex + 1 + levelWidth, targetIndex, distanceAccumulator);
	            	if(pathDistance < shortestPathDistance) shortestPathDistance = pathDistance;
	            }
	        }
	    }

	    return shortestPathDistance;
	    //Not found!
    }

    public void HighlightAvailableToMoveTiles(Transform baseTile, int traverseDepth, int recursionDepth) {
        int baseIndex = 0;
        recursionDepth++;

        for(int i = 0; i < levelHeight * levelWidth; i++) {
            if(hexes[i] == baseTile) {
                baseIndex = i;
            }
        }

        hexes[baseIndex].GetComponent<HexUnit>().AvailableForMovement();

        if(recursionDepth < traverseDepth) {
            if((baseIndex / levelWidth) % 2 == 0) {
                if(baseIndex - 1 - levelWidth >= 0) HighlightAvailableToMoveTiles(hexes[baseIndex - 1 - levelWidth], traverseDepth, recursionDepth);
                if(baseIndex - levelWidth >= 0) HighlightAvailableToMoveTiles(hexes[baseIndex - levelWidth], traverseDepth, recursionDepth);
                if(baseIndex - 1 >= 0) HighlightAvailableToMoveTiles(hexes[baseIndex - 1], traverseDepth, recursionDepth);

                if(baseIndex + 1 < levelWidth * levelHeight) HighlightAvailableToMoveTiles(hexes[baseIndex + 1], traverseDepth, recursionDepth);
                if(baseIndex + levelWidth < levelWidth * levelHeight) HighlightAvailableToMoveTiles(hexes[baseIndex + levelWidth], traverseDepth, recursionDepth);
                if(baseIndex - 1 + levelWidth < levelWidth * levelHeight) HighlightAvailableToMoveTiles(hexes[baseIndex - 1 + levelWidth], traverseDepth, recursionDepth);
            } 
            else {
                if(baseIndex + 1 - levelWidth >= 0) HighlightAvailableToMoveTiles(hexes[baseIndex + 1 - levelWidth], traverseDepth, recursionDepth);
                if(baseIndex - levelWidth >= 0) HighlightAvailableToMoveTiles(hexes[baseIndex - levelWidth], traverseDepth, recursionDepth);
                if(baseIndex - 1 >= 0) HighlightAvailableToMoveTiles(hexes[baseIndex - 1], traverseDepth, recursionDepth);

                if(baseIndex + 1 < levelWidth * levelHeight) HighlightAvailableToMoveTiles(hexes[baseIndex + 1], traverseDepth, recursionDepth);
                if(baseIndex + levelWidth < levelWidth * levelHeight) HighlightAvailableToMoveTiles(hexes[baseIndex + levelWidth], traverseDepth, recursionDepth);
                if(baseIndex + 1 + levelWidth < levelWidth * levelHeight) HighlightAvailableToMoveTiles(hexes[baseIndex + 1 + levelWidth], traverseDepth, recursionDepth);
            }
        }

    }
}
