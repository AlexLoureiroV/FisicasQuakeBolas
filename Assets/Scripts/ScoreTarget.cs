using UnityEngine;

public class ScoreTarget : MonoBehaviour
{
	#region Properties
	#endregion

	#region Fields
	[SerializeField] private int pointsValue = 10; // Puntos que da esta zona de la diana
	#endregion

	#region Unity Callbacks
	void OnTriggerEnter(Collider other)
	{
		if (!other.CompareTag("Ball")) return;

		Ball ball = other.GetComponent<Ball>();
		if (ball != null)
		{
			Debug.Log("Diana: zona de " + pointsValue + " puntos (" + name + ")");
			ball.RegisterTargetHit(pointsValue);
		}
	}
	#endregion

	#region Public Methods
	#endregion

	#region Private Methods
	#endregion
}
