using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

	public Transform cameraTransform;
	public float cameraLinearSpeed;
	public float cameraRotationSpeed;
	public float screenActiveEdgesMarginPixelCount = 10;
	public float flyOverSpeed;
	public float flyOverRotationSpeed;
	public bool isControlling;
	public Vector3 flyOverOffset;

	private Vector3 tempTargetPosition;

	private void Awake() {
		cameraTransform = transform;
	}

	void Update () {
		if(isControlling) {
			if(Input.mousePosition.x <= screenActiveEdgesMarginPixelCount) {
				cameraTransform.Translate(new Vector3(-1, 0, 0) * cameraLinearSpeed);
			}

			else if(Input.mousePosition.x >= Screen.width - screenActiveEdgesMarginPixelCount) {
				cameraTransform.Translate(new Vector3(1, 0, 0) * cameraLinearSpeed);
			}

			if(Input.mousePosition.y <= screenActiveEdgesMarginPixelCount) {
				cameraTransform.Translate(new Vector3(0, -1 * Mathf.Sin(Mathf.Deg2Rad * cameraTransform.transform.eulerAngles.x), -1) * cameraLinearSpeed, Space.Self);
			}

			else if(Input.mousePosition.y >= Screen.height - screenActiveEdgesMarginPixelCount) {
				cameraTransform.Translate(new Vector3(0, 1 * Mathf.Sin(Mathf.Deg2Rad * cameraTransform.transform.eulerAngles.x), 1) * cameraLinearSpeed, Space.Self);
			}

			if (Input.GetAxis("Mouse ScrollWheel") > 0) {
		         cameraTransform.Translate(new Vector3(0, 0, 1) * cameraLinearSpeed);
		    }
		    else if (Input.GetAxis("Mouse ScrollWheel") < 0) {
		         cameraTransform.Translate(new Vector3(0, 0, -1) * cameraLinearSpeed);
		    }

		    if(Input.GetKey(KeyCode.Q)) {
		    	cameraTransform.Rotate(new Vector3(0, -1, 0) * cameraRotationSpeed, Space.World);
		    }
		    else if(Input.GetKey(KeyCode.E)) {
		    	cameraTransform.Rotate(new Vector3(0, 1, 0) * cameraRotationSpeed, Space.World);
		    }
		}
	}

	public void FlyOverPosition(Vector3 position) {
		tempTargetPosition = position + flyOverOffset;
		isControlling = false;
		StartCoroutine("FlyCoroutine");
	}

	IEnumerator FlyCoroutine() {
		for(int i = 0; i < 100; i++) {
			cameraTransform.position = Vector3.Slerp(cameraTransform.position , tempTargetPosition, Time.deltaTime * flyOverSpeed);

			Vector3 rotPos = tempTargetPosition - cameraTransform.position + new Vector3(0, -2, 0);
			Quaternion newRot = Quaternion.LookRotation(rotPos);
			cameraTransform.rotation = Quaternion.Lerp(cameraTransform.rotation, newRot, Time.deltaTime * flyOverRotationSpeed);
			yield return new WaitForEndOfFrame();
		}

		isControlling = true;
	}
}
