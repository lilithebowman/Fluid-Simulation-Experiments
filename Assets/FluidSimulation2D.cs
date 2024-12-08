using Unity.VisualScripting;

using UnityEngine;
using UnityEngine.Rendering;

public class FluidSimulation2D : MonoBehaviour {
	public Texture2D renderTexture;
	public int numParticles = 256;
	SimulatedParticle[] particles;

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
		}
		Debug.Log(particles);
		Debug.Log(particles.Length);
		Debug.Log(particles[0].position);
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
			// render the particle's current position
			renderTexture.SetPixel(
				(int) particle.position.x,
				(int) particle.position.y,
				new Color(Mathf.Abs(particle.velocity.y), 1, Mathf.Abs(particle.velocity.x))
			);

			// apply movement to the particle
			particle.Move();
		}

		renderTexture.Apply();
	}
}

class SimulatedParticle {
	public Vector2 position;
	public Vector2 velocity;
	float gravityConstant = -1;
	int boundaryX;
	int boundaryY;
	float drag = -0.5f;
	public SimulatedParticle (int width, int height) {
		position = new Vector2(Random.Range(0, width), Random.Range(0, height));
		velocity = Vector2.zero;
		velocity.x = Random.Range(-1, 1);

		boundaryX = width;
		boundaryY = height;
	}

	public void Move() {
		velocity.y += gravityConstant;

		position.x += velocity.x;
		position.y += velocity.y;

		// If we hit the sides, bounce back
		if (position.x < 0) velocity.x = -velocity.x + drag;
		if (position.x > boundaryX) velocity.x = -velocity.x + drag;

		// if we hit the top or bottom, bounce back
		if (position.y < 0) velocity.y = -velocity.y + drag;
		if (position.y > boundaryY) velocity.y = -position.y + drag;
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

		// Add some new random pixels each frame
		for (int i = 0; i < renderTexture.height; i++) {
			renderTexture.SetPixel(Random.Range(0, renderTexture.width), Random.Range(0, renderTexture.height), new Color(Random.value, Random.value, Random.value));
		}
	}
}
