using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Explosion : MonoBehaviour {

	public List<ParticleSystem> particleSystems;
	public float dieTime = 5.0f;

	void Start () {
		foreach(ParticleSystem s in particleSystems) {
			s.Clear();
			s.Stop();
			s.Emit(100);
		}

		StartCoroutine("Die");
	}

	IEnumerator Die() {
		yield return new WaitForSeconds(dieTime	);
		Destroy(this.gameObject);
	}
}
