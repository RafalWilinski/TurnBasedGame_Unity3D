using UnityEngine;
using System.Collections;

public class HexUnit : MonoBehaviour {

	public Color mouseOverColor;
	public Color availableColor;
	public Color unavailableColor;
	public Color availableToMoveColor;
	public Color enemyOnHexColor;
	
	public float fadeDelay;

	private bool isMouseOver;
	private Color oldColor;
	private Renderer renderer;
	private bool isReserved = false;
	private Unit hexOwner;

	private void Awake() {
		renderer = GetComponent<Renderer>();
	}

	public void HighlightMouseover() {

		if(!isMouseOver) {
			renderer.material.color = mouseOverColor;
		}

		StopCoroutine("CheckMouseOver");
		isMouseOver = true;
		StartCoroutine("CheckMouseOver");
	}

	public void AvailableForMovement() {
		renderer.material.color = availableToMoveColor;
	}

	public void RevertToNormal() {
		if(isReserved)
			renderer.material.color = GameScenario.Instance.teams[hexOwner.teamNumber].teamColor;
		else 
			renderer.material.color = availableColor;
	}

	IEnumerator CheckMouseOver() {
		yield return new WaitForSeconds(fadeDelay);
		isMouseOver = false;

		if(isReserved) renderer.material.color = GameScenario.Instance.teams[hexOwner.teamNumber].teamColor;
		else renderer.material.color = availableColor;
	}

	public bool ReserveHex(Unit u) {
		hexOwner = u;
		if(isReserved) return false;
		else {
			isReserved = true;
			renderer.material.color = GameScenario.Instance.teams[hexOwner.teamNumber].teamColor;
			return true;
		}
	}

	public void FreeHex() {
		isReserved = false;
		renderer.material.color = availableColor;
	}
}
