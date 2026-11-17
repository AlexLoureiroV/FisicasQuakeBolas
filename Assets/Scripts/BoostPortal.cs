using UnityEngine;

public class BoostPortal : MonoBehaviour
{
	#region Properties
	#endregion

	#region Fields
	[Header("Configuración del Impulso")]
	[SerializeField] private float speedMultiplier = 1.5f; // Multiplica la velocidad actual
	[SerializeField] private float minBoostForce = 20f;    // Velocidad mínima asegurada al pasar
	[SerializeField] private bool debugMode = true;
	#endregion

	#region Unity Callbacks
	void OnTriggerEnter(Collider other)
	{
		if (!other.CompareTag("Ball")) return;

		Rigidbody rb = other.GetComponent<Rigidbody>();
		if (rb != null)
		{
			ApplyBoost(rb);
		}
	}
	#endregion

	#region Public Methods
	#endregion

	#region Private Methods
	private void ApplyBoost(Rigidbody rb)
	{
		Vector3 currentVelocity = rb.linearVelocity;
		float speed = currentVelocity.magnitude;

		// Usamos la velocidad más alta entre el multiplicador y la mínima
		float newSpeed = Mathf.Max(speed * speedMultiplier, minBoostForce);

		// Misma dirección que ya llevaba la bola: solo acelera, no cambia de trayectoria
		rb.linearVelocity = currentVelocity.normalized * newSpeed;

		if (debugMode)
		{
			Debug.Log($"BOOST! Velocidad aumentada de {speed:F1} a {newSpeed:F1}");
		}
	}
	#endregion
}
