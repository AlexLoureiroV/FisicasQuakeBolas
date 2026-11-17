using UnityEngine;

public class DestructibleBlock : MonoBehaviour
{
	#region Properties
	#endregion

	#region Fields
	[Header("Configuración del Bloque")]
	[SerializeField] private int pointsValue = 10;
	[SerializeField] private bool destroyOnHit = true;
	[SerializeField] private GameObject destroyEffect;
	#endregion

	#region Unity Callbacks
	#endregion

	#region Public Methods
	// Lo llama la bola cuando el halo del destello alcanza este bloque
	public void OnExplosionHit(Vector3 explosionPos)
	{
		ScoreManager.AddScore(pointsValue);

		// Efecto visual de explosión
		if (destroyEffect != null)
		{
			Instantiate(destroyEffect, transform.position, Quaternion.identity);
		}

		if (destroyOnHit)
		{
			Destroy(gameObject);
		}
	}
	#endregion

	#region Private Methods
	#endregion
}
