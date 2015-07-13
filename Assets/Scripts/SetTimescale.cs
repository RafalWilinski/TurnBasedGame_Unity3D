using UnityEngine;
using System.Collections;

public class SetTimescale : MonoBehaviour {

	public void Change(float to) {
		Time.timeScale = to;
	}
}
