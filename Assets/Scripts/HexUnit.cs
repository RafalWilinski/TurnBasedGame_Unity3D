using UnityEngine;
using System.Collections;

public class HexUnit : MonoBehaviour {

	public Color mouseOverColor;
	public Color availableColor;
	public Color unavailableColor;
	public Color playerOnHexColor;
	public Color enemyOnHexColor;
	
	public float fadeDelay;

	private bool isMouseOver;
	private Color oldColor;
	private Renderer renderer;

	private void Awake() {
		renderer = GetComponent<Renderer>();
	}

	public void HighlightMouseover() {

		if(!isMouseOver) {
			oldColor = renderer.material.color;
			renderer.material.color = mouseOverColor;
		}

		StopCoroutine("CheckMouseOver");
		isMouseOver = true;
		StartCoroutine("CheckMouseOver");
	}

	IEnumerator CheckMouseOver() {
		yield return new WaitForSeconds(fadeDelay);
		isMouseOver = false;
		renderer.material.color = oldColor;
	}
}
