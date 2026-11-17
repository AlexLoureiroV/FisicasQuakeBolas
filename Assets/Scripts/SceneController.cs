using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
	#region Properties
	// True mientras el jugador controla una bola (el cañón está bloqueado)
	public bool IsControllingBall => controlledBall != null;
	#endregion

	#region Fields
	[Header("Jerarquía del Cañón")]
	[SerializeField] private Transform basePivot;
	[SerializeField] private Transform cannonPivot;
	[SerializeField] private Transform shootPoint;

	[Header("Visuales del Cañón")]
	[SerializeField] private Renderer cannonRenderer;
	[SerializeField] private Color neonColor = Color.cyan;
	[SerializeField] private float maxEmissionIntensity = 5f;

	[Header("Puntería (Raycast)")]
	[SerializeField] private float aimRayDistance = 60f;
	[SerializeField] private Color aimColor = Color.green;
	[SerializeField] private float aimEmissionIntensity = 2f;

	[Header("Portales")]
	[SerializeField] private Transform originPortal;
	[SerializeField] private Transform destinationPortal;

	[Header("Prefabs")]
	[SerializeField] private GameObject ballPrefab;

	[Header("Valores de Disparo")]
	[SerializeField] [Range(10f, 200f)] private float minLaunchForce = 20f;
	[SerializeField] [Range(100f, 500f)] private float maxLaunchForce = 150f;
	[SerializeField] private float launchForce = 20f;
	[SerializeField] [Range(5f, 80f)] private float verticalAngle = 45f;
	[SerializeField] [Range(-180f, 180f)] private float horizontalAngle = 0f;
	[SerializeField] private float maxChargeTime = 2.0f;

	[Header("Movimiento Portal Origen")]
	[SerializeField] private bool moveOriginPortal = true;
	[SerializeField] private float portalMoveSpeed = 2f;
	[SerializeField] private float portalMoveRange = 5f;

	[Header("Teletransporte")]
	[SerializeField] private float teleportForceMultiplier = 1.0f;
	[SerializeField] private bool debugMode = false;

	private float portalStartX;
	private bool isCharging = false;
	private float chargeTime = 0f;
	private bool isAimingAtPortal = false;
	private Ball controlledBall;     // Bola que controla el jugador tras cruzar el portal
	private BallCamera ballCamera;
	#endregion

	#region Unity Callbacks
	// Start is called before the first frame update
	void Start()
	{
		if (ballPrefab == null) Debug.LogError("SceneController: falta asignar el Ball Prefab.");
		if (shootPoint == null) Debug.LogError("SceneController: falta asignar el Shoot Point.");

		if (originPortal != null) portalStartX = originPortal.position.x;

		ballCamera = FindFirstObjectByType<BallCamera>();
	}

	// Update is called once per frame
	void Update()
	{
		// R: partida nueva (puntuación a cero y recarga de la escena)
		if (Input.GetKeyDown(KeyCode.R))
		{
			RestartGame();
			return;
		}

		if (moveOriginPortal && originPortal != null) UpdatePortalMovement();

		// Mientras el jugador controla una bola, el cañón queda bloqueado
		if (controlledBall != null)
		{
			SetCannonEmission(neonColor, 0f);
			return;
		}

		UpdateCannonRotation();
		UpdateAim();
		HandleCharging();
	}
	#endregion

	#region Public Methods
	public void Shoot()
	{
		if (ballPrefab == null || shootPoint == null) return;

		GameObject ballObj = Instantiate(ballPrefab, shootPoint.position, shootPoint.rotation);

		Rigidbody rb = ballObj.GetComponent<Rigidbody>();
		if (rb != null)
		{
			rb.AddForce(shootPoint.forward * launchForce, ForceMode.Impulse);
			rb.AddTorque(Random.insideUnitSphere * 10f, ForceMode.Impulse);
		}

		if (debugMode) Debug.Log("Disparo con fuerza " + launchForce.ToString("F1"));
	}

	// Lo llama el portal de origen cuando una bola lo atraviesa
	public void TeleportBall(Ball ball, Transform destination)
	{
		if (ball == null) return;
		if (destination == null) destination = destinationPortal;
		if (destination == null) return;

		Rigidbody rb = ball.GetComponent<Rigidbody>();

		// La bola sale por el eje "up" del portal de destino
		ball.transform.position = destination.position + destination.up * 1.5f;

		if (rb != null)
		{
			rb.linearVelocity = destination.up * (rb.linearVelocity.magnitude * teleportForceMultiplier);
			rb.angularVelocity = Vector3.zero;
		}

		ball.EnterPlayerControl(destination.up);
		controlledBall = ball;
		isCharging = false;

		if (ballCamera != null) ballCamera.StartFollowing(ball.transform);
	}
	#endregion

	#region Private Methods
	private void UpdateCannonRotation()
	{
		horizontalAngle += Input.GetAxis("Horizontal") * 40f * Time.deltaTime;

		verticalAngle += Input.GetAxis("Vertical") * 40f * Time.deltaTime;
		verticalAngle = Mathf.Clamp(verticalAngle, 10f, 80f);

		if (basePivot != null)
		{
			basePivot.localRotation = Quaternion.Euler(0f, horizontalAngle, 0f);
		}

		if (cannonPivot != null)
		{
			cannonPivot.localRotation = Quaternion.Euler(verticalAngle, 0f, 0f);
		}
	}

	// Raycast en línea recta desde la boca del cañón: si toca el portal de origen,
	// el cañón se ilumina con aimColor. Es una ayuda aproximada, porque la bola
	// en realidad describe una parábola.
	private void UpdateAim()
	{
		if (shootPoint == null) return;

		Vector3 origin = shootPoint.position;
		Vector3 direction = shootPoint.forward;

		bool wasAiming = isAimingAtPortal;
		isAimingAtPortal = Physics.Raycast(origin, direction, out RaycastHit hit, aimRayDistance,
				Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide)
			&& hit.transform == originPortal;

		// Visible en la ventana Scene (y en Game con Gizmos activados)
		Debug.DrawRay(origin, direction * aimRayDistance, isAimingAtPortal ? Color.green : Color.red);

		if (debugMode && isAimingAtPortal && !wasAiming) Debug.Log("¡Portal en el punto de mira!");
	}

	private void HandleCharging()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			isCharging = true;
			chargeTime = 0f;
		}

		float chargeProgress = 0f;
		if (isCharging)
		{
			chargeTime += Time.deltaTime;
			chargeProgress = Mathf.Clamp01(chargeTime / maxChargeTime);
			launchForce = Mathf.Lerp(minLaunchForce, maxLaunchForce, chargeProgress);
		}

		if (isCharging && Input.GetKeyUp(KeyCode.Space))
		{
			isCharging = false;
			chargeProgress = 0f;
			Shoot();
		}

		// Brillo del cañón: sube con la carga; en reposo solo brilla si apunta al portal
		Color color = isAimingAtPortal ? aimColor : neonColor;
		float intensity = isCharging ? chargeProgress * maxEmissionIntensity
			: (isAimingAtPortal ? aimEmissionIntensity : 0f);
		SetCannonEmission(color, intensity);
	}

	private void SetCannonEmission(Color color, float intensity)
	{
		if (cannonRenderer == null) return;

		cannonRenderer.material.SetColor("_EmissionColor", color * intensity);
	}

	private void UpdatePortalMovement()
	{
		float x = portalStartX + Mathf.Sin(Time.time * portalMoveSpeed) * portalMoveRange;
		originPortal.position = new Vector3(x, originPortal.position.y, originPortal.position.z);
	}

	private void RestartGame()
	{
		ScoreManager.ResetScore();
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}
	#endregion
}
