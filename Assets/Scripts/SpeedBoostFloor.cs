using UnityEngine;

public class SpeedBoostFloor : MonoBehaviour
{
	#region Properties
	#endregion

	#region Fields
	[Header("Configuración de Aceleración")]
	[SerializeField] private float accelerationForce = 10f;
	[SerializeField] private float maxVelocity = 60f; // Límite para evitar que la bola atraviese el suelo
	#endregion

	#region Unity Callbacks
	void OnTriggerStay(Collider other)
	{
		Rigidbody rb = other.GetComponent<Rigidbody>();
		if (rb == null) return;

		// Solo empujamos mientras no supere la velocidad máxima
		if (rb.linearVelocity.magnitude < maxVelocity)
		{
			// Fuerza en la dirección "forward" del suelo
			rb.AddForce(transform.forward * accelerationForce, ForceMode.Acceleration);
		}
	}
	#endregion

	#region Public Methods
	#endregion

	#region Private Methods
	#endregion
}
