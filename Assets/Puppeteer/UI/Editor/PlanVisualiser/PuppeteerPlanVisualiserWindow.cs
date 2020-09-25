using Puppeteer.Core;
using Puppeteer.UI.External.GraphVisualizer;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Puppeteer.UI
{
	internal class PuppeteerPlanVisualiserWindow : EditorWindow
	{
		public static PuppeteerPlanVisualiserWindow CreateWindow()
		{
			PuppeteerPlanVisualiserWindow window = GetWindow<PuppeteerPlanVisualiserWindow>();
			window.minSize = new Vector2(750, 400);
			return window;
		}

		[MenuItem("Puppeteer/PlanVisualiser _&#V", priority = 20)]
		public static void Open()
		{
			CreateWindow();
		}

		public void OnGUI()
		{
			if (!TryDrawGraph())
			{
				ShowDisabledMessage();
			}
		}

		private static void ShowMessage(string _text)
		{
			GUILayout.BeginVertical();
			GUILayout.FlexibleSpace();

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();

			GUILayout.Label(_text);

			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			GUILayout.FlexibleSpace();
			GUILayout.EndVertical();
		}

		private void EditorPlayModeStateChanged(PlayModeStateChange _stateChange)
		{
			TrySelectAgent();
			m_Visualiser.OverrideTargetAgent(m_Agent);
			Repaint();
		}

		private void LazyInitGraphRenderer()
		{
			if (m_GraphRenderer != null)
			{
				return;
			}

			m_GraphRenderer = new PuppeteerPlanGraphRenderer(NodeClicked);
		}

		private void NodeClicked(VisualiserBaseNode _clickedNode)
		{
			m_GraphSettings.ShowInspector = _clickedNode != null;
		}

		private void OnDisable()
		{
			EditorApplication.playModeStateChanged -= EditorPlayModeStateChanged;
			Selection.selectionChanged -= OnSelectionChange;
		}

		private void OnEnable()
		{
			titleContent = new GUIContent("Puppeteer PlanVisualiser", PuppeteerEditorResourceLoader.VisualiserIcon16.texture);

			EditorApplication.playModeStateChanged += EditorPlayModeStateChanged;
			Selection.selectionChanged += OnSelectionChange;

			VisualTreeAsset visualTree = Resources.Load<VisualTreeAsset>("LayoutPuppeteerVisualiserWindow");
			visualTree.CloneTree(rootVisualElement);

			TextElement versionText = rootVisualElement.Q<TextElement>(name: "version");
			versionText.text = PuppeteerEditorHelper.GetVersion();

			StyleSheet styleSheet = Resources.Load<StyleSheet>("StylePuppeteerVisualiserWindow");
			rootVisualElement.styleSheets.Add(styleSheet);

			StyleSheet themeSheet = Resources.Load<StyleSheet>(EditorGUIUtility.isProSkin ? "StyleDarkTheme" : "StyleLightTheme");
			rootVisualElement.styleSheets.Add(themeSheet);

			var textLogo = rootVisualElement.Q<Image>(className: "textLogo");
			textLogo.image = PuppeteerEditorResourceLoader.TextLogo32.texture;

			m_Visualiser.OnVisualiserContentChanged += Repaint;

			if(m_Agent == null)
			{
				OnSelectionChange();
			}
		}

		private void OnSelectionChange()
		{
			GameObject activeGameObject = Selection.activeGameObject;
			if (activeGameObject == null)
			{
				return;
			}

			TrySelectAgent();
			m_Visualiser.OverrideTargetAgent(m_Agent);
			Repaint();
		}

		private void ShowDisabledMessage()
		{
			ShowMessage(EditorApplication.isPlaying ? (m_Visualiser.IsEmpty() ? m_NoPlanText : m_SelectAgentText) : m_EnterPlayModeText);
		}

		private bool TryDrawGraph()
		{
			if (m_Agent == null)
			{
				return false;
			}

			m_Visualiser.Refresh();

			if (m_Visualiser.IsEmpty())
			{
				return false;
			}

			m_GraphLayout.CalculateLayout(m_Visualiser);

			var graphRect = new Rect(0, 0, position.width, position.height);

			LazyInitGraphRenderer();
			m_GraphRenderer.Draw(m_GraphLayout, graphRect, m_GraphSettings, m_Visualiser);

			return true;
		}

		private void TrySelectAgent()
		{
			if (!EditorApplication.isPlayingOrWillChangePlaymode)
			{
				m_Agent = null;
				return;
			}

			GameObject activeGameObject = Selection.activeGameObject;
			if (activeGameObject == null)
			{
				m_Agent = null;
				return;
			}

			m_Agent = activeGameObject.GetComponent<PuppeteerAgent>();
		}

		private const string m_EnterPlayModeText = "Enter play mode to start the AI.";
		private const string m_SelectAgentText = "Select a GameObject that has the PuppeteerAgent component.";
		private const string m_NoPlanText = "The selected PuppeteerAgent has no plan.";


		private readonly IGraphLayout m_GraphLayout = new ReingoldTilford();
		private readonly PuppeteerPlanVisualiser m_Visualiser = new PuppeteerPlanVisualiser();

		private PuppeteerAgent m_Agent = null;
		private PuppeteerPlanGraphRenderer m_GraphRenderer;

		private GraphSettings m_GraphSettings = new GraphSettings()
		{
			NodeAspectRatio = 1.61803398875f, // golden ratio :)
			MaximumNodeSizeInPixels = 100.0f,
			MaximumNormalizedNodeSize = 1f,
			ShowInspector = false,
		};
	}
}