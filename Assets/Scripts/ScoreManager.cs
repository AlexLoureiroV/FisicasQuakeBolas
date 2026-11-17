using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
	#region Properties
	// Puntuación total: estática, común a toda la partida aunque se recargue la escena
	public static int TotalScore => totalScore;
	#endregion

	#region Fields
	[SerializeField] private int targetScore = 100;
	[SerializeField] private TextMeshProUGUI scoreText;
	[SerializeField] private TextMeshProUGUI victoryText;
	[SerializeField] private TextMeshProUGUI turboText;

	private static int totalScore = 0;
	private static ScoreManager instance; // El ScoreManager de la escena actual (para refrescar la UI)
	#endregion

	#region Unity Callbacks
	void Awake()
	{
		instance = this;
	}

	// Start is called before the first frame update
	void Start()
	{
		if (scoreText == null) Debug.LogWarning("ScoreManager: el campo Score Text está vacío, no se verá la puntuación.");
		if (victoryText != null && victoryText == scoreText) Debug.LogWarning("ScoreManager: Score Text y Victory Text son el mismo objeto; el marcador se ocultará.");

		if (turboText != null) turboText.text = "";
		RefreshUI();
	}

	void OnDestroy()
	{
		if (instance == this) instance = null;
	}
	#endregion

	#region Public Methods
	public static void AddScore(int amount)
	{
		totalScore += amount;
		Debug.Log("+" + amount + " puntos. Total: " + totalScore);

		if (instance != null) instance.RefreshUI();
	}

	public static void SetTurboText(string text)
	{
		if (instance != null && instance.turboText != null)
		{
			instance.turboText.text = text;
		}
	}

	public static void ResetScore()
	{
		totalScore = 0;

		if (instance != null) instance.RefreshUI();
	}
	#endregion

	#region Private Methods
	private void RefreshUI()
	{
		if (scoreText != null)
		{
			scoreText.text = "Score: " + totalScore + " / " + targetScore;
		}

		bool hasWon = totalScore >= targetScore;
		if (victoryText != null)
		{
			victoryText.gameObject.SetActive(hasWon);
		}

		if (hasWon) Debug.Log("VICTORIA");
	}
	#endregion
}
