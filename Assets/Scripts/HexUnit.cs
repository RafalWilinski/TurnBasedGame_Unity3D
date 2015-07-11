using UnityEngine;
using System.Collections;

public class HexUnit : MonoBehaviour {

	public Color mouseOverColor;
	public Color availableColor;
	public Color mountainColor;
	public Color availableToMoveColor;
	
	public float fadeDelay;
	private bool isReserved = false;
	private bool isMouseOver;
	public bool isAvailableForMovement;

	private Color oldColor;
	private Renderer myRenderer;
	private Unit hexOwner;

	private void Awake() {
		myRenderer = GetComponent<Renderer>();
	}

	private void OnEnable() {
		GameScenario.OnClearTilesSelection += DeselectTile;
	}

	private void OnDisable() {
		GameScenario.OnClearTilesSelection -= DeselectTile;
	}

	public void HighlightMouseover() {

		if(!isMouseOver) {
			myRenderer.material.color = mouseOverColor;
		}

		StopCoroutine("CheckMouseOver");
		isMouseOver = true;
		StartCoroutine("CheckMouseOver");
	}

	public void AvailableForMovement() {
		if(!isReserved) {
			isAvailableForMovement = true;
			myRenderer.material.color = availableToMoveColor;
		}
	}

	private void DeselectTile() {
		isAvailableForMovement = false;
		RevertToNormal();
	}

	public void RevertToNormal() {
		if(isReserved)
			myRenderer.material.color = GameScenario.Instance.teams[hexOwner.teamNumber].teamColor;
		else if(isAvailableForMovement) 
			myRenderer.material.color = availableToMoveColor;
		else 
			myRenderer.material.color = availableColor;
	}

	IEnumerator CheckMouseOver() {
		yield return new WaitForSeconds(fadeDelay);
		isMouseOver = false;

		RevertToNormal();
	}

	public bool ReserveHex(Unit u) {
		hexOwner = u;
		if(isReserved) return false;
		else {
			isReserved = true;
			GetComponent<Renderer>().material.color = GameScenario.Instance.teams[hexOwner.teamNumber].teamColor;
			return myRenderer;
		}
	}

	public void FreeHex() {
		isReserved = false;
		myRenderer.material.color = availableColor;
	}
}
