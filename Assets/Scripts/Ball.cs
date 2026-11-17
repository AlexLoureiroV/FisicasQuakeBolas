using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class Ball : MonoBehaviour
{
	#region Properties
	// True desde que la bola cruza el portal y pasa a controlarla el jugador
	public bool IsPlayerControlled => isPlayerControlled;
	public int TurbosLeft => turbosLeft;
	public int FlashesLeft => flashesLeft;
	#endregion

	#region Fields
	[Header("Configuración de Bola")]
	[SerializeField] private float destroyY = -100f;
	[SerializeField] private bool debugMode = true;
	[SerializeField] private float manualMoveSpeed = 5f;

	[Header("Configuración de Salto")]
	[SerializeField] private float jumpForce = 6f;
	[SerializeField] private float jumpCooldown = 1.0f;

	[Header("Destello (tecla P)")]
	[SerializeField] private int maxFlashes = 3;                     // Destellos por bola
	[FormerlySerializedAs("explosionRadius")]
	[SerializeField] private float flashRadius = 12f;                // Radio máximo del halo
	[FormerlySerializedAs("explosionForce")]
	[SerializeField] private float flashForce = 500f;                // Empuje a los objetos con Rigidbody
	[SerializeField] private float flashExpandTime = 0.4f;           // Segundos que tarda el halo en crecer
	[SerializeField] private Material haloMaterial;                  // Material transparente del halo (opcional)
	[SerializeField] private Color haloColor = new Color(0f, 1f, 1f, 0.35f);

	[Header("Frenado de Barro")]
	[SerializeField] private float mudFriction = 2f;

	[Header("Turbo (flecha abajo)")]
	[SerializeField] private int maxTurbos = 3;              // Turbos por bola
	[SerializeField] private float stopSpeedThreshold = 5f;   // Por debajo de esta velocidad se puede cargar el turbo
	[SerializeField] private float minTurboImpulse = 4f;     // Impulso con un toque corto
	[SerializeField] private float maxTurboImpulse = 15f;    // Impulso con la carga completa
	[SerializeField] private float maxTurboChargeTime = 1.5f;
	[SerializeField] private Color turboGlowColor = new Color(1f, 0.4f, 0f); // Color al que va la bola al cargar
	[SerializeField] private float turboGlowIntensity = 20f;                 // Brillo con la carga completa

	[Header("Diana")]
	[SerializeField] private float targetResolveDelay = 0.05f;

	private Rigidbody rb;
	private float lastJumpTime;
	private bool isPlayerControlled = false;
	private bool isInMud = false;
	private bool isBeingDestroyed = false;
	private bool isResolvingTarget = false;
	private int bestTargetPoints = 0;
	private int turbosLeft;
	private bool isChargingTurbo = false;
	private float turboChargeTime = 0f;
	private Vector3 trackForward = Vector3.back;          // "Hacia delante" del circuito: el turbo solo empuja hacia aquí
	private int flashesLeft;
	private GameObject currentHalo;                      // Halo activo (solo uno a la vez)
	private Material ballMaterial;                       // Copia del material para cambiar su brillo
	private Color originalEmission = Color.black;
	#endregion

	#region Unity Callbacks
	void Awake()
	{
		gameObject.tag = "Ball";
		rb = GetComponent<Rigidbody>();
		lastJumpTime = -jumpCooldown;
		turbosLeft = maxTurbos;
		flashesLeft = maxFlashes;

		Renderer ballRenderer = GetComponent<Renderer>();
		if (ballRenderer != null)
		{
			ballMaterial = ballRenderer.material;
			ballMaterial.EnableKeyword("_EMISSION");
			if (ballMaterial.HasProperty("_EmissionColor"))
			{
				originalEmission = ballMaterial.GetColor("_EmissionColor");
			}
		}
	}

	// Update is called once per frame
	void Update()
	{
		if (isBeingDestroyed) return;

		// Reinicio seguro: solo si cae muy abajo
		if (transform.position.y < destroyY)
		{
			isBeingDestroyed = true;
			RestartLevel();
			return;
		}

		if (isPlayerControlled)
		{
			HandleManualMovement();
			HandleJump();
			HandleTurbo();

			if (Input.GetKeyDown(KeyCode.P))
			{
				TryFlash();
			}
		}
	}

	void FixedUpdate()
	{
		if (isInMud && rb != null)
		{
			Vector3 brakingForce = -rb.linearVelocity * mudFriction;
			rb.AddForce(brakingForce, ForceMode.Acceleration);
		}
	}

	void OnDestroy()
	{
		if (isPlayerControlled) ScoreManager.SetTurboText("");
		if (currentHalo != null) Destroy(currentHalo);
	}

	void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Mud")) isInMud = true;
	}

	void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("Mud")) isInMud = false;
	}
	#endregion

	#region Public Methods
	// Lo llama el SceneController al teletransportar la bola.
	// exitDirection = hacia dónde sale la bola del portal de destino; eso marca "delante".
	public void EnterPlayerControl(Vector3 exitDirection)
	{
		isPlayerControlled = true;

		Vector3 flatDirection = new Vector3(exitDirection.x, 0f, exitDirection.z);
		if (flatDirection.sqrMagnitude > 0.01f)
		{
			trackForward = flatDirection.normalized;
		}

		UpdateTurboText();
	}

	// Lo llama cada zona de la diana. Si la bola toca varias zonas a la vez,
	// se queda con la de más puntos y solo puntúa una vez.
	public void RegisterTargetHit(int points)
	{
		if (isBeingDestroyed && !isResolvingTarget) return;

		bestTargetPoints = Mathf.Max(bestTargetPoints, points);

		if (isResolvingTarget) return;

		isResolvingTarget = true;
		isBeingDestroyed = true;
		Invoke(nameof(ResolveTargetHit), targetResolveDelay);
	}
	#endregion

	#region Private Methods
	private void HandleManualMovement()
	{
		float moveInput = Input.GetAxis("Horizontal") * -1f;
		Vector3 currentVel = rb.linearVelocity;
		rb.linearVelocity = new Vector3(moveInput * manualMoveSpeed, currentVel.y, currentVel.z);
	}

	private void HandleJump()
	{
		// Salto con cooldown para evitar volar infinitamente
		if (Input.GetKeyDown(KeyCode.Space) && Time.time >= lastJumpTime + jumpCooldown)
		{
			rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
			lastJumpTime = Time.time;

			if (debugMode) Debug.Log("¡Salto!");
		}
	}

	// Flecha abajo con la bola casi parada: mantener para cargar, soltar para lanzar el turbo
	private void HandleTurbo()
	{
		if (turbosLeft <= 0) return;

		bool isStopped = rb.linearVelocity.magnitude < stopSpeedThreshold;

		if (Input.GetKeyDown(KeyCode.DownArrow) && isStopped)
		{
			isChargingTurbo = true;
			turboChargeTime = 0f;
		}

		if (!isChargingTurbo) return;

		turboChargeTime += Time.deltaTime;
		float charge = Mathf.Clamp01(turboChargeTime / maxTurboChargeTime);
		UpdateTurboText(charge);
		SetTurboGlow(charge);

		if (Input.GetKeyUp(KeyCode.DownArrow))
		{
			isChargingTurbo = false;
			turbosLeft--;
			SetTurboGlow(0f);

			float impulse = Mathf.Lerp(minTurboImpulse, maxTurboImpulse, charge);
			// Solo hacia delante: primero quitamos la velocidad que lleve hacia atrás
			float forwardSpeed = Vector3.Dot(rb.linearVelocity, trackForward);
			if (forwardSpeed < 0f)
			{
				rb.linearVelocity -= trackForward * forwardSpeed;
			}
			rb.AddForce(trackForward * impulse, ForceMode.Impulse);

			if (debugMode) Debug.Log($"¡TURBO! Impulso {impulse:F1}. Quedan {turbosLeft}");
			UpdateTurboText();
		}
	}

	// La bola pasa de su brillo normal al color del turbo según la carga (0 = normal, 1 = máximo)
	private void SetTurboGlow(float charge)
	{
		if (ballMaterial == null) return;

		Color target = turboGlowColor * turboGlowIntensity;
		ballMaterial.SetColor("_EmissionColor", Color.Lerp(originalEmission, target, charge));
	}

	private void UpdateTurboText(float charge = -1f)
	{
		string text = "Turbos: " + turbosLeft + " / " + maxTurbos;
		if (charge >= 0f) text += "  [" + Mathf.RoundToInt(charge * 100f) + "%]";
		text += "\nDestellos: " + flashesLeft + " / " + maxFlashes;

		ScoreManager.SetTurboText(text);
	}

	// P: lanza un halo que crece desde la bola y destruye los bloques que alcanza.
	// La bola no se destruye; hay un número limitado de destellos.
	private void TryFlash()
	{
		if (flashesLeft <= 0 || currentHalo != null) return;

		flashesLeft--;
		UpdateTurboText();
		StartCoroutine(FlashRoutine());

		if (debugMode) Debug.Log("¡Destello! Quedan " + flashesLeft);
	}

	private IEnumerator FlashRoutine()
	{
		currentHalo = CreateHalo(transform.position);
		Renderer haloRenderer = currentHalo.GetComponent<Renderer>();
		HashSet<Collider> alreadyHit = new HashSet<Collider>();

		float elapsed = 0f;
		while (elapsed < flashExpandTime)
		{
			elapsed += Time.deltaTime;
			float t = Mathf.Clamp01(elapsed / flashExpandTime);
			float radius = Mathf.Lerp(0.1f, flashRadius, t);

			// El halo acompaña a la bola mientras crece
			Vector3 center = transform.position;
			currentHalo.transform.position = center;

			// Visual: la esfera crece (diámetro = 2 x radio) y se va desvaneciendo
			currentHalo.transform.localScale = Vector3.one * radius * 2f;
			SetHaloAlpha(haloRenderer, haloColor.a * (1f - t));

			HitObjectsInRadius(center, radius, alreadyHit);
			yield return null;
		}

		Destroy(currentHalo);
		currentHalo = null;
	}

	// Física: todo lo que el halo alcanza en este radio recibe el golpe una sola vez
	private void HitObjectsInRadius(Vector3 center, float radius, HashSet<Collider> alreadyHit)
	{
		Collider[] colliders = Physics.OverlapSphere(center, radius);
		foreach (Collider hit in colliders)
		{
			if (hit == null || alreadyHit.Contains(hit) || hit.attachedRigidbody == rb) continue;
			alreadyHit.Add(hit);

			DestructibleBlock block = hit.GetComponent<DestructibleBlock>();
			if (block != null) block.OnExplosionHit(center);

			Rigidbody targetRb = hit.attachedRigidbody;
			if (targetRb != null && !targetRb.isKinematic)
			{
				targetRb.AddExplosionForce(flashForce, center, flashRadius);
			}
		}
	}

	private GameObject CreateHalo(Vector3 center)
	{
		GameObject halo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		halo.name = "Halo";
		halo.transform.position = center;
		halo.transform.localScale = Vector3.zero;

		// Sin collider: el halo es solo visual, la detección la hace OverlapSphere
		Destroy(halo.GetComponent<Collider>());

		Renderer haloRenderer = halo.GetComponent<Renderer>();
		haloRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		if (haloMaterial != null)
		{
			haloRenderer.material = new Material(haloMaterial);
		}
		else
		{
			// Sin material asignado: un shader transparente que no necesita configuración
			Shader shader = Shader.Find("Sprites/Default");
			if (shader == null) shader = Shader.Find("Universal Render Pipeline/Unlit");
			haloRenderer.material = new Material(shader);
		}
		SetHaloAlpha(haloRenderer, haloColor.a);

		return halo;
	}

	private void SetHaloAlpha(Renderer haloRenderer, float alpha)
	{
		Color color = new Color(haloColor.r, haloColor.g, haloColor.b, alpha);
		Material mat = haloRenderer.material;

		if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color); // URP
		if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);         // Sprites/Default
	}

	private void ResolveTargetHit()
	{
		ScoreManager.AddScore(bestTargetPoints);
		Destroy(gameObject);
	}

	private void RestartLevel()
	{
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}
	#endregion
}
