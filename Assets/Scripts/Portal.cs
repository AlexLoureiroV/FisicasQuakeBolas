using UnityEngine;

public class Portal : MonoBehaviour
{
	#region Properties
	#endregion

	#region Fields
	[Header("Configuración del Portal")]
	[SerializeField] private Portal linkedPortal;
	[SerializeField] private bool isDestinationPortal = false;
	[SerializeField] private bool debugMode = true;

	private SceneController sceneController;
	#endregion

	#region Unity Callbacks
	// Start is called before the first frame update
	void Start()
	{
		sceneController = FindFirstObjectByType<SceneController>();

		if (!isDestinationPortal && linkedPortal == null)
		{
			Debug.LogWarning("Portal: falta asignar el Linked Portal en " + name);
		}

		// El portal de destino permanece oculto hasta que una bola lo usa
		if (isDestinationPortal) gameObject.SetActive(false);
	}

	void OnTriggerEnter(Collider other)
	{
		if (isDestinationPortal || linkedPortal == null || sceneController == null) return;
		if (!other.CompareTag("Ball")) return;

		Ball ball = other.GetComponent<Ball>();
		if (ball == null || ball.IsPlayerControlled) return;

		linkedPortal.gameObject.SetActive(true);
		sceneController.TeleportBall(ball, linkedPortal.transform);

		if (debugMode) Debug.Log("Bola teletransportada a " + linkedPortal.name);
	}
	#endregion

	#region Public Methods
	#endregion

	#region Private Methods
	#endregion
}
