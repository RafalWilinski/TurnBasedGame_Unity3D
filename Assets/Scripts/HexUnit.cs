﻿using UnityEngine;
using System.Collections;

public class HexUnit : MonoBehaviour {

	public Color mouseOverColor;
	public Color availableColor;
	public Color halfHighlightColor;
	public float tintFactor;
	public float fadeDelay;

	private bool isReserved = false;
	private bool isMouseOver;
	private bool isHalfHighlight;
	private bool isEpicenter;

	[HideInInspector]
	public bool isAvailableForMovement;

	private Color oldColor;
	private Renderer myRenderer;
	private Transform myTransform;
	private Unit hexOwner;
	private HexUnit halfHighlightSource;

	private float riseAmount;

	public Transform MyTransform {
		get { return myTransform; }
	}

	public bool IsEpicenter {
		get { return isEpicenter; }
		set { isEpicenter = value; }
	}

	public bool IsReserved {
		get { return isReserved; }
	}

	public Unit Owner {
		get { return hexOwner; }
	}

	private void Awake() {
		myRenderer = GetComponent<Renderer>();
		myTransform = transform;
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

	public void HalfHighlight(HexUnit src) {
		if(!isReserved) {
			isHalfHighlight = true;
			halfHighlightSource = src;

			StopCoroutine("CheckHighlightSource");
			myRenderer.material.color = halfHighlightColor;
			StartCoroutine("CheckHighlightSource");
		}
	}

	public void MakeEpicenter() {
		isEpicenter = true;
		StopCoroutine("CheckEpicenter");
		StartCoroutine("CheckEpicenter");
	}

	IEnumerator CheckEpicenter() {
		yield return new WaitForSeconds(0.03f);
		isEpicenter = false;
		RevertToNormal();
	}

	IEnumerator CheckHighlightSource() {
		yield return new WaitForSeconds(0.03f);
		if(!halfHighlightSource.IsEpicenter) {
			isHalfHighlight = false;
			RevertToNormal();
		}
	}

	public bool AvailableForMovement() {
		if(!isReserved) {
			if(isAvailableForMovement) return false;

			isAvailableForMovement = true;
			myRenderer.material.color = new Color(GameScenario.Instance.teams[GameScenario.Instance.activePlayer].teamColor.r + ((1.0f - GameScenario.Instance.teams[GameScenario.Instance.activePlayer].teamColor.r) * tintFactor), 
				GameScenario.Instance.teams[GameScenario.Instance.activePlayer].teamColor.g + ((1.0f - GameScenario.Instance.teams[GameScenario.Instance.activePlayer].teamColor.g) * tintFactor), 
				GameScenario.Instance.teams[GameScenario.Instance.activePlayer].teamColor.b + ((1.0f - GameScenario.Instance.teams[GameScenario.Instance.activePlayer].teamColor.b) * tintFactor), 1);
			return true;
		}
		else return true;
	}

	private void DeselectTile() {
		isAvailableForMovement = false;
		RevertToNormal();
	}

	public void RevertToNormal() {
		if(isReserved)
			myRenderer.material.color = GameScenario.Instance.teams[hexOwner.teamNumber].teamColor;
		else if(isAvailableForMovement) 
			myRenderer.material.color = new Color(GameScenario.Instance.teams[GameScenario.Instance.activePlayer].teamColor.r + (1.0f - GameScenario.Instance.teams[GameScenario.Instance.activePlayer].teamColor.r) * tintFactor, 
				GameScenario.Instance.teams[GameScenario.Instance.activePlayer].teamColor.g + (1.0f - GameScenario.Instance.teams[GameScenario.Instance.activePlayer].teamColor.g) * tintFactor, 
				GameScenario.Instance.teams[GameScenario.Instance.activePlayer].teamColor.b + (1.0f - GameScenario.Instance.teams[GameScenario.Instance.activePlayer].teamColor.b) * tintFactor, 1);
		else if(isHalfHighlight) 
			myRenderer.material.color = halfHighlightColor;
		else 
			myRenderer.material.color = availableColor;
	}

	IEnumerator CheckMouseOver() {
		yield return new WaitForSeconds(fadeDelay);
		isMouseOver = false;

		RevertToNormal();
	}

	public bool ReserveHex(Unit u) {
		if(isReserved) return false;
		else {
			isReserved = true;
			hexOwner = u;
			myRenderer.material.color = GameScenario.Instance.teams[hexOwner.teamNumber].teamColor;
			return myRenderer;
		}
	}

	public void FreeHex() {
		isReserved = false;
		RevertToNormal();
	}

	public void Rise(float amount) {
		riseAmount = amount;
		StartCoroutine("RiseCoroutine");
	}

	IEnumerator RiseCoroutine() {
		for(int i = 0; i < 50; i++) {
			myTransform.localPosition = new Vector3(myTransform.localPosition.x, myTransform.localPosition.y + riseAmount, myTransform.localPosition.z);
			if(Owner != null) Owner.ChangeYAxisOffset();
			yield return new WaitForEndOfFrame();
		}
	}
}
