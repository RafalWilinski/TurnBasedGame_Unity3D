using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour {

	public float bulletSpeed;
	public GameObject explosionHitPrefab;

	private Transform myTransform;
	private Transform victim;
	private float distance;
	private bool shouldHit;

	public void SetTarget (Transform target, bool isSuccess) {
		myTransform = transform;
		shouldHit = isSuccess;
		victim = target;
		myTransform.LookAt(target);

		distance = Vector3.Distance(myTransform.position, target.position);

		StartCoroutine("Move");
	}

	IEnumerator Move() {
		while(distance > 1f) {
			distance = Vector3.Distance(myTransform.position, victim.position);
			myTransform.Translate(Vector3.forward * bulletSpeed * Time.deltaTime);
			yield return new WaitForEndOfFrame();
		}

		if(shouldHit) Instantiate(explosionHitPrefab, victim.position, Quaternion.identity);
		Destroy(this.gameObject);
	}
}
