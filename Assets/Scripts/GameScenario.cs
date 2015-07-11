using UnityEngine;
using System.Collections;
using Pathfinding;

public class GameScenario : MonoBehaviour {

	public TerrainGenerator terrainGen;



	void Start () {
		GameStart();
	}
	
	private void GameStart() {
		StartCoroutine("UpdateGridAfterDelay");
	}

	IEnumerator UpdateGridAfterDelay() {
		for(int i = 0; i < 100; i++) {
			terrainGen.perlinAmplify += 1 / (i + 1f) ;
			yield return new WaitForSeconds(0.0333f);
		}

		yield return new WaitForSeconds(0.5f);
		Debug.Log("Rescan...");
		AstarPath.active.Scan();
	}
}
