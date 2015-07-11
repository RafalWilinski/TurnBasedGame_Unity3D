using UnityEngine;
using System.Collections;
using Pathfinding;

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

	void Start () {
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
			}
		}
	}

	private void Update() {
		if(oldPerlinAmplify != perlinAmplify || oldPerlinOffset != perlinOffset || oldPerlinScale != perlinScale) {
			UpdateHexPositions();
		}
	}
}
