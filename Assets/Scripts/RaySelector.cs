using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RaySelector : MonoBehaviour {

    void Update () {

        Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit)) {
            Transform objectHit = hit.transform;

            if(objectHit.gameObject.name.Contains("Hex")) {
            	HexUnit hex = objectHit.GetComponent<HexUnit>();

            	if(GameScenario.Instance.actionMode == GameScenario.SelectedMode.Bomb || GameScenario.Instance.actionMode == GameScenario.SelectedMode.Rise) {
     				hex.MakeEpicenter();
            		hex.HighlightMouseover();
            		List<Transform> neighbours = TerrainGenerator.Instance.GetHexNeighbours(objectHit);

            		//foreach(Transform t in neighbours) { }
            		for(int i = 0; i < neighbours.Count; i++) {
            			neighbours[i].GetComponent<HexUnit>().HalfHighlight(hex);
            		}
            	}
            	else {
	            	if(hex.IsReserved) {
	            		GameScenario.Instance.ShowUnitInformation(hex.Owner);
	            	}
	            	else hex.HighlightMouseover();
	            }
            }

            if(objectHit.gameObject.name.Contains("Unit")) {
           		GameScenario.Instance.ShowUnitInformation(objectHit.GetComponent<Unit>());
            }

            if(Input.GetMouseButtonUp(0)) {
            	GameScenario.Instance.ProcessClick(objectHit);
            }
        }
    }
}
