using UnityEngine;
using System.Collections;

public class RaySelector : MonoBehaviour {

    void Update () {

        Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit)) {
            Transform objectHit = hit.transform;

            if(objectHit.gameObject.name.Contains("Hex")) {
            	HexUnit hex = objectHit.GetComponent<HexUnit>();
            	if(hex.IsReserved) {
            		GameScenario.Instance.ShowUnitInformation(hex.Owner);
            	}
            	else hex.HighlightMouseover();
            }

            if(objectHit.gameObject.name.Contains("Unit")) {
           		GameScenario.Instance.ShowUnitInformation(objectHit.GetComponent<Unit>());
            }

            if(Input.GetMouseButton(0)) {
            	GameScenario.Instance.ProcessClick(objectHit);
            }
        }
    }
}
