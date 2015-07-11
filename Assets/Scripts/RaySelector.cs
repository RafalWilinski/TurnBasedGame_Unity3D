using UnityEngine;
using System.Collections;

public class RaySelector : MonoBehaviour {

    void Update () {

        Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit)) {
            Transform objectHit = hit.transform;

            if(objectHit.gameObject.name.Contains("Hex"))
            	objectHit.GetComponent<HexUnit>().HighlightMouseover();

            if(Input.GetMouseButton(0)) {
            	TerrainGenerator.Instance.HighlightAvailableToMoveTiles(objectHit, 4, 0);
            }
        }
    }
}
