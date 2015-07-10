using UnityEngine;
using System.Collections;

public class ParticlesAlongSplineRenderer : MonoBehaviour {

	public ParticleSystem particleSystem;
	public CatmullRomSpline spline;
	public int particlesCount;
	public float debugDelay;

	private ParticleSystem.Particle[] particlesArray;


	//Callback subscription
	private void OnEnable() {
		//CatmullRomSpline.OnSplineUpdated += OnSplineUpdated;
	}

	private void OnDisable() {
		//CatmullRomSpline.OnSplineUpdated -= OnSplineUpdated;
	}

	void Start () {
		particleSystem.Stop();
		particleSystem.Clear();
		particlesArray = new ParticleSystem.Particle[particlesCount];
		particleSystem.maxParticles = particlesCount;
		particleSystem.Emit(particlesCount);
		particleSystem.GetParticles(particlesArray);

		StartCoroutine("RenderWithDelay");

	}

	private IEnumerator RenderWithDelay() {
		yield return new WaitForSeconds(debugDelay);
		OnSplineUpdated(1f);
	}

	private void OnSplineUpdated(float splineTimeLimit) {
		int i = 0;
		for(float f = 0.0f; f < 0.5f; f += (splineTimeLimit / particlesCount)) {
			Debug.Log("Calc: "+f.ToString("f2"));
			// particlesArray[i].position = spline.GetPositionAtTime(f);
			i++;
		}

		particleSystem.SetParticles(particlesArray, particlesArray.Length);
	}
}
