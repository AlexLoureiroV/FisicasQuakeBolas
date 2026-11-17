using UnityEngine;

public class BallCamera : MonoBehaviour
{
	#region Properties
	#endregion

	#region Fields
	[SerializeField] private Vector3 offset = new Vector3(0, 5, -10); // Distancia de la cámara
	[SerializeField] private float smoothSpeed = 0.125f;              // Suavizado del movimiento

	private Transform target;          // La bola que vamos a seguir
	private bool isFollowing = false;
	private Vector3 startPosition;     // Vista inicial (la del cañón)
	private Quaternion startRotation;
	#endregion

	#region Unity Callbacks
	// Start is called before the first frame update
	void Start()
	{
		startPosition = transform.position;
		startRotation = transform.rotation;
	}

	void LateUpdate()
	{
		if (!isFollowing) return;

		// Si la bola ha sido destruida, volvemos a la vista del cañón
		if (target == null)
		{
			isFollowing = false;
			transform.SetPositionAndRotation(startPosition, startRotation);
			return;
		}

		// Posición deseada basada en el offset
		Vector3 desiredPosition = target.position + offset;
		// Interpolación suave (Lerp) para que la cámara no sea brusca
		transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
		// La cámara siempre mira a la bola
		transform.LookAt(target);
	}
	#endregion

	#region Public Methods
	// Método para que el SceneController active la cámara
	public void StartFollowing(Transform ballTransform)
	{
		target = ballTransform;
		isFollowing = true;
	}
	#endregion

	#region Private Methods
	#endregion
}
