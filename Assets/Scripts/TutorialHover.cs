using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TutorialHover : MonoBehaviour {
	
	public Text hoverText;
	public string moveInfo;
	public string attackInfo;
	public string bombInfo;
	public string riseInfo;
	
	public void Awake() {
		Time.timeScale = 0;
	}
	
	public void MoveInfo() {
		hoverText.text = moveInfo;
	}
	
	public void AttackInfo() {
		hoverText.text = attackInfo;
	}
	
	public void BombInfo() {
		hoverText.text = bombInfo;
	}
	
	public void RiseInfo() {
		hoverText.text = riseInfo;
	}
	
}
