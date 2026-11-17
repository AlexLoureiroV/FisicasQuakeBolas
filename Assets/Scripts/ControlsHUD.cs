using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Panel de ayuda con los controles, abajo a la izquierda.
// Se crea solo al empezar la partida (no hace falta añadirlo a la escena),
// cambia según se controle el cañón o la bola e ilumina cada tecla al pulsarla.
public class ControlsHUD : MonoBehaviour
{
	#region Properties
	#endregion

	#region Fields
	[Header("Referencias (opcionales: se buscan solas)")]
	[SerializeField] private Canvas targetCanvas;
	[SerializeField] private SceneController sceneController;

	[Header("Aspecto")]
	[SerializeField] private float fontSize = 24f;
	[SerializeField] private Vector2 margin = new Vector2(30f, 30f);
	[SerializeField] private float keyColumnWidth = 190f;
	[SerializeField] private Color panelColor = new Color(0f, 0f, 0f, 0.6f);
	[SerializeField] private Color keyColor = new Color(0.18f, 0.18f, 0.24f, 1f);
	[SerializeField] private Color keyPressedColor = new Color(0f, 0.85f, 1f, 1f);
	[SerializeField] private Color textColor = Color.white;
	[SerializeField] private bool useArrowSymbols = true; // Desmárcalo si en vez de flechas salen cuadrados
	[SerializeField] private KeyCode toggleKey = KeyCode.H;

	private GameObject panel;
	private TextMeshProUGUI titleText;
	private GameObject cannonGroup;
	private GameObject ballGroup;
	private bool isBallMode = false;
	private readonly List<KeyVisual> keyVisuals = new List<KeyVisual>();

	// Una tecla que se dibuja: el texto que muestra y las teclas reales que la iluminan
	private struct KeyDef
	{
		public string label;
		public KeyCode[] codes;
	}

	// Una tecla ya creada en pantalla
	private class KeyVisual
	{
		public Image background;
		public TextMeshProUGUI label;
		public KeyCode[] codes;
	}
	#endregion

	#region Unity Callbacks
	// Start is called before the first frame update
	void Start()
	{
		if (targetCanvas == null) targetCanvas = FindFirstObjectByType<Canvas>();
		if (sceneController == null) sceneController = FindFirstObjectByType<SceneController>();

		if (targetCanvas == null)
		{
			Debug.LogWarning("ControlsHUD: no hay ningún Canvas en la escena.");
			enabled = false;
			return;
		}

		BuildPanel();
		SetBallMode(false);
	}

	// Update is called once per frame
	void Update()
	{
		if (panel == null) return;

		if (Input.GetKeyDown(toggleKey)) panel.SetActive(!panel.activeSelf);

		bool ballMode = sceneController != null && sceneController.IsControllingBall;
		if (ballMode != isBallMode) SetBallMode(ballMode);

		UpdateKeyHighlights();
	}

	void OnDestroy()
	{
		if (panel != null) Destroy(panel);
	}
	#endregion

	#region Public Methods
	#endregion

	#region Private Methods
	// Crea el HUD automáticamente al arrancar y cada vez que se recarga la escena
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	private static void Bootstrap()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
		SceneManager.sceneLoaded += OnSceneLoaded;
		EnsureExists();
	}

	private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		EnsureExists();
	}

	private static void EnsureExists()
	{
		// Si ya hay uno en la escena (por ejemplo, añadido a mano para ajustarlo), no se duplica
		if (FindFirstObjectByType<ControlsHUD>() != null) return;

		new GameObject("ControlsHUD").AddComponent<ControlsHUD>();
	}

	private void BuildPanel()
	{
		panel = CreateUIObject("ControlsPanel", targetCanvas.transform);
		RectTransform rect = (RectTransform)panel.transform;
		rect.anchorMin = Vector2.zero;
		rect.anchorMax = Vector2.zero;
		rect.pivot = Vector2.zero;
		rect.anchoredPosition = margin;

		Image background = panel.AddComponent<Image>();
		background.color = panelColor;
		background.raycastTarget = false;

		VerticalLayoutGroup layout = panel.AddComponent<VerticalLayoutGroup>();
		layout.padding = new RectOffset(20, 20, 16, 16);
		layout.spacing = 10f;
		SetLayoutDefaults(layout);

		// El panel se ajusta solo al tamaño de su contenido
		ContentSizeFitter fitter = panel.AddComponent<ContentSizeFitter>();
		fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
		fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

		titleText = CreateText("Title", panel.transform, "", fontSize * 1.1f, FontStyles.Bold);

		KeyDef left = Key(Arrow("←", "Izq"), KeyCode.LeftArrow, KeyCode.A);
		KeyDef right = Key(Arrow("→", "Der"), KeyCode.RightArrow, KeyCode.D);
		KeyDef up = Key(Arrow("↑", "Arr"), KeyCode.UpArrow, KeyCode.W);
		KeyDef down = Key(Arrow("↓", "Abj"), KeyCode.DownArrow, KeyCode.S);
		KeyDef space = Key("Espacio", KeyCode.Space);
		KeyDef restart = Key("R", KeyCode.R);

		cannonGroup = CreateGroup("CannonControls");
		AddRow(cannonGroup, "Girar el cañón", left, right);
		AddRow(cannonGroup, "Subir / bajar el ángulo", up, down);
		AddRow(cannonGroup, "Mantener: cargar · Soltar: disparar", space);
		AddRow(cannonGroup, "Reiniciar la partida", restart);

		ballGroup = CreateGroup("BallControls");
		AddRow(ballGroup, "Mover la bola a los lados", left, right);
		AddRow(ballGroup, "Saltar", space);
		AddRow(ballGroup, "Turbo: mantener y soltar (bola lenta)", Key(Arrow("↓", "Abj"), KeyCode.DownArrow));
		AddRow(ballGroup, "Destello: rompe los obstáculos", Key("P", KeyCode.P));
		AddRow(ballGroup, "Reiniciar la partida", restart);

		TextMeshProUGUI footer = CreateText("Footer", panel.transform,
			toggleKey + ": ocultar / mostrar la ayuda", fontSize * 0.75f, FontStyles.Italic);
		footer.color = new Color(textColor.r, textColor.g, textColor.b, 0.7f);
	}

	private void SetBallMode(bool ballMode)
	{
		isBallMode = ballMode;
		titleText.text = ballMode ? "CONTROLES · BOLA" : "CONTROLES · CAÑÓN";
		cannonGroup.SetActive(!ballMode);
		ballGroup.SetActive(ballMode);
	}

	private void UpdateKeyHighlights()
	{
		foreach (KeyVisual key in keyVisuals)
		{
			bool pressed = false;
			foreach (KeyCode code in key.codes)
			{
				if (Input.GetKey(code))
				{
					pressed = true;
					break;
				}
			}

			key.background.color = pressed ? keyPressedColor : keyColor;
			key.label.color = pressed ? Color.black : textColor;
		}
	}

	private GameObject CreateGroup(string name)
	{
		GameObject group = CreateUIObject(name, panel.transform);
		VerticalLayoutGroup layout = group.AddComponent<VerticalLayoutGroup>();
		layout.spacing = 8f;
		SetLayoutDefaults(layout);
		return group;
	}

	// Una fila: teclas a la izquierda y su descripción a la derecha
	private void AddRow(GameObject group, string description, params KeyDef[] keys)
	{
		GameObject row = CreateUIObject("Row", group.transform);
		HorizontalLayoutGroup rowLayout = row.AddComponent<HorizontalLayoutGroup>();
		rowLayout.spacing = 16f;
		rowLayout.childAlignment = TextAnchor.MiddleLeft;
		SetLayoutDefaults(rowLayout);

		// Columna de teclas con ancho fijo para que las descripciones queden alineadas
		GameObject keysColumn = CreateUIObject("Keys", row.transform);
		HorizontalLayoutGroup keysLayout = keysColumn.AddComponent<HorizontalLayoutGroup>();
		keysLayout.spacing = 6f;
		keysLayout.childAlignment = TextAnchor.MiddleLeft;
		SetLayoutDefaults(keysLayout);
		LayoutElement columnSize = keysColumn.AddComponent<LayoutElement>();
		columnSize.minWidth = keyColumnWidth;

		foreach (KeyDef key in keys)
		{
			CreateKeycap(keysColumn.transform, key);
		}

		CreateText("Description", row.transform, description, fontSize, FontStyles.Normal);
	}

	// Dibuja una tecla: un recuadro con su nombre dentro
	private void CreateKeycap(Transform parent, KeyDef key)
	{
		GameObject cap = CreateUIObject("Key_" + key.label, parent);
		Image background = cap.AddComponent<Image>();
		background.color = keyColor;
		background.raycastTarget = false;

		HorizontalLayoutGroup capLayout = cap.AddComponent<HorizontalLayoutGroup>();
		capLayout.padding = new RectOffset(12, 12, 4, 4);
		capLayout.childAlignment = TextAnchor.MiddleCenter;
		SetLayoutDefaults(capLayout);

		LayoutElement capSize = cap.AddComponent<LayoutElement>();
		capSize.minWidth = fontSize * 1.8f;
		capSize.minHeight = fontSize * 1.8f;

		TextMeshProUGUI label = CreateText("Label", cap.transform, key.label, fontSize, FontStyles.Bold);
		label.alignment = TextAlignmentOptions.Center;

		keyVisuals.Add(new KeyVisual { background = background, label = label, codes = key.codes });
	}

	private TextMeshProUGUI CreateText(string name, Transform parent, string text, float size, FontStyles style)
	{
		GameObject go = CreateUIObject(name, parent);
		TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
		if (TMP_Settings.defaultFontAsset != null) tmp.font = TMP_Settings.defaultFontAsset;
		tmp.text = text;
		tmp.fontSize = size;
		tmp.fontStyle = style;
		tmp.color = textColor;
		tmp.textWrappingMode = TextWrappingModes.NoWrap;
		tmp.raycastTarget = false;
		return tmp;
	}

	private GameObject CreateUIObject(string name, Transform parent)
	{
		GameObject go = new GameObject(name, typeof(RectTransform));
		go.layer = LayerMask.NameToLayer("UI");
		go.transform.SetParent(parent, false);
		return go;
	}

	private void SetLayoutDefaults(HorizontalOrVerticalLayoutGroup layout)
	{
		layout.childControlWidth = true;
		layout.childControlHeight = true;
		layout.childForceExpandWidth = false;
		layout.childForceExpandHeight = false;
	}

	private KeyDef Key(string label, params KeyCode[] codes)
	{
		return new KeyDef { label = label, codes = codes };
	}

	private string Arrow(string symbol, string fallback)
	{
		return useArrowSymbols ? symbol : fallback;
	}
	#endregion
}
