using UnityEngine;
using System.Collections;

public class RaySelector : MonoBehaviour {

    void Update () {

        Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit)) {
            Transform objectHit = hit.transform;
            objectHit.GetComponent<HexUnit>().HighlightMouseover();
        }
    }
}
