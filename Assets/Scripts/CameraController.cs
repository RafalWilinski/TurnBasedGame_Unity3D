using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

	public Transform cameraTransform;
	public float cameraLinearSpeed;
	public float cameraRotationSpeed;
	public float screenActiveEdgesMarginPixelCount = 10;

	private void Awake() {
		cameraTransform = transform;
	}

	void Update () {
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
