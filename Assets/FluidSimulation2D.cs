using System.Collections.Generic;
using System.Linq;

using Unity.VisualScripting;

using UnityEngine;
using UnityEngine.Rendering;

using static UnityEngine.ParticleSystem;

public class FluidSimulation2D : MonoBehaviour {
	public Texture2D renderTexture;
	public int numParticles = 256;
	SimulatedParticle[] particles;

	private ParticleIndex particleIndex;

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start () {
		// Set up a new texture to render to
		renderTexture = new Texture2D(256, 256);
		renderTexture.filterMode = FilterMode.Point;
		renderTexture.wrapMode = TextureWrapMode.Clamp;

		Renderer renderer = gameObject.GetComponent<Renderer>();
		if (renderer != null) {
			renderer.material.mainTexture = renderTexture;
		}

		// Create particles for 2D Simulation
		particles = new SimulatedParticle[numParticles];
		for (int i = 0; i < numParticles; i++) {
			particles[i] = new SimulatedParticle(renderTexture.width, renderTexture.height);
			particleIndex = new ParticleIndex(renderTexture.width, renderTexture.height);
		}

		// Reset the simulation periodically every 10 seconds after 10 seconds
		InvokeRepeating("ResetParticle", 10.0f, 10.0f);
	}

	// Update is called once per frame
	void Update () {

		// Clear the texture
		for (int y = 0; y < renderTexture.height; y++) {
			for (int x = 0; x < renderTexture.width; x++) {
				renderTexture.SetPixel(x, y, Color.black);
			}
		}

		// Display and simulat the particles
		foreach (SimulatedParticle particle in particles) {
			// Update the index arrays
			UpdateParticleIndices(particle);

			// render the particle's current position
			renderTexture.SetPixel(
				(int) particle.position.x,
				(int) particle.position.y,
				new Color(Mathf.Abs(particle.velocity.y), 1, Mathf.Abs(particle.velocity.x))
			);

			// apply movement to the particle
			particle.Move();

			// Check for collisions and apply forces
			Collision(particle); // this is WIP and doesn't work
		}

		renderTexture.Apply();
	}

	public void ResetParticle () {
		// Display and simulat the particles
		foreach (SimulatedParticle particle in particles) {
			particle.ResetSimulation(renderTexture.width, renderTexture.height);
		}
	}

	private void Collision (SimulatedParticle particle) {
		int kernelSize = 1;

		// Check for all other particles which may occupy the same space
		for (int y = -kernelSize; y < kernelSize; y++) {
			for (int x = -kernelSize; x < kernelSize; x++) {
				// check against the merged together x-indexed and y-indexed arrays
				foreach (SimulatedParticle other in particleIndex.particlesX[x].Concat(particleIndex.particlesY[y])) {
					if (other.position == particle.position) {
						// We have a collision! Apply forces
						Vector2 particleVelocity = particle.velocity;
						particle.velocity += -other.velocity;
						other.velocity += -particleVelocity;
					}
				}
			}
		}
	}

	private void UpdateParticleIndices(SimulatedParticle particle) {
		// Create a fresh array
		particleIndex = null;
		particleIndex = new ParticleIndex(renderTexture.width, renderTexture.height);

		// Update the x-indexed array
		// Slot the particle into the x-index based on its X-position
		particleIndex.particlesX[(int) particle.position.x].Add(particle);

		// Update the y-indexed array
		// Slot the partticle into the y-index based on its Y-position
		particleIndex.particlesY[(int) particle.position.y].Add(particle);
	}
}

class ParticleIndex {
	public List<SimulatedParticle>[] particlesX; // an X-indexed array of particles for quick lookup
	public List<SimulatedParticle>[] particlesY; // a Y-indexed array of particles for quick lookup

	public ParticleIndex(int width, int height) {
		particlesX = new List<SimulatedParticle>[width];
		particlesY = new List<SimulatedParticle>[height];
	}
}

class SimulatedParticle {
	public Vector2 position;
	public Vector2 velocity;
	public float gravityConstant = -1;
	int boundaryX;
	int boundaryY;
	public float drag = 0.9f;
	public float friction = 0.1f;
	public SimulatedParticle (int width, int height) {
		ResetSimulation(width, height);

		boundaryX = width;
		boundaryY = height;
	}

	public void ResetSimulation(int width, int height) {
		position = new Vector2(Random.Range(0, width), Random.Range(0, height));
		velocity = Vector2.zero;
		velocity.x = Random.Range(-1, 1);
	}

	public void Move () {
		velocity.y += gravityConstant;

		position.x += velocity.x;
		position.y += velocity.y;

		// If we hit the sides, bounce back
		if (position.x < 0) {
			velocity.x = -velocity.x * drag;
			position.x = 0;
		} else
		if (position.x > boundaryX) {
			velocity.x = -velocity.x * drag;
			position.x = boundaryX;
		}

		// if we hit the top or bottom, bounce back
		if (position.y < 0) {
			velocity.y = -velocity.y * drag;
			position.y = 0;
		} else
		if (position.y > boundaryY) {
			velocity.y = -position.y * drag;
			position.y = boundaryY;
		}

		// apply friction on bottom
		if (position.y == boundaryY) {
			// apply friction to x
			velocity.x = velocity.x * friction;
		}
	}
}

class JustForFun {
	public Texture2D renderTexture;

	void InitializeToRandomPixels () {
		// Set the pixels to random colours
		for (int y = 0; y < renderTexture.width; y++) {
			for (int x = 0; x < renderTexture.height; x++) {
				renderTexture.SetPixel(x, y, new Color(Random.value, Random.value, Random.value));
			}
		}
	}

	void BoxBlur () {
		// Using a 3x3 kernel, sample the surrounding pixels and average them each frame
		// this will eventually cause it to smear into a grey texture
		for (int y = 0; y < renderTexture.width; y++) {
			for (int x = 0; x < renderTexture.height; x++) {
				float averageR = 0;
				float averageG = 0;
				float averageB = 0;

				Color color = renderTexture.GetPixel(x, y);
				for (int k = -1; k <= 1; k++) {
					for (int j = -1; j <= 1; j++) {
						color = renderTexture.GetPixel(x + k, y + j);
						if (k == 0 && j == 0) {
							averageR += color.r * 2;
							averageG += color.g * 2;
							averageB += color.b * 2;
						} else {
							averageR += color.r;
							averageG += color.g;
							averageB += color.b;
						}
					}
				}

				averageR /= 9;
				averageG /= 9;
				averageB /= 9;
				renderTexture.SetPixel(x, y, new Color(averageR, averageG, averageB));
			}
		}
	}
}
