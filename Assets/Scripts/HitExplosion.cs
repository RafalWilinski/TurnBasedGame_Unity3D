using UnityEngine;
using System.Collections;

public class HitExplosion : Explosion {

	ParticleSystem.Particle[] particles;

	public void Awake() {
		Color c = GameScenario.Instance.GetOpponent().teamColor;
		foreach(ParticleSystem p in particleSystems) {
			p.startColor = c;
			particles = new ParticleSystem.Particle[p.maxParticles];
			p.GetParticles(particles);
			for(int i = 0; i < particles.Length; i++) {
				particles[i].color = c;
			}
			p.SetParticles(particles, particles.Length);
		}
	}
}
 