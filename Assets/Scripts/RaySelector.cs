using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RaySelector : MonoBehaviour {

    void Update () {

        Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit)) {
            Transform objectHit = hit.transform;

            if(!GameScenario.Instance.aimLine.enabled && GameScenario.Instance.actionMode == GameScenario.SelectedMode.Attack)
            	GameScenario.Instance.aimLine.enabled = true;

            if(objectHit.gameObject.name.Contains("Hex")) {
            	HexUnit hex = objectHit.GetComponent<HexUnit>();

            	GameScenario.Instance.aimLine.SetPosition(1, hit.transform.position + new Vector3(0,3.5f,0));

            	if(GameScenario.Instance.actionMode == GameScenario.SelectedMode.Bomb || GameScenario.Instance.actionMode == GameScenario.SelectedMode.Rise) {
     				hex.MakeEpicenter();
            		hex.HighlightMouseover();
            		List<Transform> neighbours = TerrainGenerator.Instance.GetHexNeighbours(objectHit);

            		//foreach(Transform t in neighbours) { }
            		for(int i = 0; i < neighbours.Count; i++) {
            			neighbours[i].GetComponent<HexUnit>().HalfHighlight(hex);
            		}
            	}
            	else if(GameScenario.Instance.actionMode == GameScenario.SelectedMode.Attack) {
            		if(objectHit.GetComponent<HexUnit>().Owner != null)
            			GameScenario.Instance.ShowAttackInformation(objectHit.GetComponent<HexUnit>().Owner);
            	}
            	else {
	            	if(hex.IsReserved) {
	            		GameScenario.Instance.ShowUnitInformation(hex.Owner);
	            	}
	            	else hex.HighlightMouseover();
	            }
            }

            if(objectHit.gameObject.name.Contains("Unit")) {

            	GameScenario.Instance.aimLine.SetPosition(1, hit.transform.position);
            	
            	if(GameScenario.Instance.actionMode != GameScenario.SelectedMode.Attack) {
           			GameScenario.Instance.ShowUnitInformation(objectHit.GetComponent<Unit>());
           		}
           		else {
           			GameScenario.Instance.ShowAttackInformation(objectHit.GetComponent<Unit>());
           		}
            }

            if(Input.GetMouseButtonUp(0)) {
            	GameScenario.Instance.ProcessClick(objectHit);
            }
        }

        else {
        	GameScenario.Instance.aimLine.enabled = false;
        }
    }
}
