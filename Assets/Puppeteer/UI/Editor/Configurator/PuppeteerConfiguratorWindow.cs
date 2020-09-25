using Puppeteer.UI;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace Puppeteer.UI
{
	internal class PuppeteerConfiguratorWindow : EditorWindow
	{
		public PuppeteerConfiguratorWindow()
		{
			m_RestoreTabDict = new Dictionary<Type, System.Action>
			{
				{ typeof(GoalView), OpenTabOfType<GoalView> },
				{ typeof(ActionView), OpenTabOfType<ActionView> },
				{ typeof(SensorView), OpenTabOfType<SensorView> },
				{ typeof(ArchetypeView), OpenTabOfType<ArchetypeView> },
				{ typeof(AgentView), OpenTabOfType<AgentView> },
			};
		}

		public static PuppeteerConfiguratorWindow CreateWindow()
		{
			PuppeteerConfiguratorWindow window = GetWindow<PuppeteerConfiguratorWindow>();
			window.minSize = new Vector2(750, 400);
			return window;
		}

		[MenuItem("Puppeteer/Configurator _&#P", priority = 0)]
		public static void Open()
		{
			CreateWindow();
		}

		[MenuItem("Puppeteer/Actions", priority = 2)]
		public static void OpenActions()
		{
			var window = CreateWindow();
			window.m_TabView.OpenTabOfType<ActionView>();
		}

		[MenuItem("Puppeteer/Agents", priority = 5)]
		public static void OpenAgents()
		{
			var window = CreateWindow();
			window.m_TabView.OpenTabOfType<AgentView>();
		}

		[MenuItem("Puppeteer/Archetypes", priority = 4)]
		public static void OpenArchetypes()
		{
			var window = CreateWindow();
			window.m_TabView.OpenTabOfType<ArchetypeView>();
		}

		[MenuItem("Puppeteer/Goals", priority = 1)]
		public static void OpenGoals()
		{
			var window = CreateWindow();
			window.m_TabView.OpenTabOfType<GoalView>();
		}

		[MenuItem("Puppeteer/Sensors", priority = 3)]
		public static void OpenSensors()
		{
			var window = CreateWindow();
			window.m_TabView.OpenTabOfType<SensorView>();
		}

		public SerialisedConfiguratorState GetConfiguratorStates()
		{
			return m_ConfiguratorStates;
		}

		public void OnDestroy()
		{
			m_TabView.CleanUpAllTabs();
		}

		public void OnEnable()
		{
			titleContent = new GUIContent("Puppeteer Configurator", PuppeteerEditorResourceLoader.ConfiguratorIcon16.texture);

			VisualTreeAsset visualTree = Resources.Load<VisualTreeAsset>("LayoutPuppeteerConfiguratorWindow");
			visualTree.CloneTree(rootVisualElement);

			VisualTreeAsset leftPanelTree = Resources.Load<VisualTreeAsset>("LayoutLeftPanel");
			VisualElement leftPanel = rootVisualElement.Q<VisualElement>(className: "leftPanel");
			leftPanel.styleSheets.Add(Resources.Load<StyleSheet>("StyleLeftPanel"));
			leftPanelTree.CloneTree(leftPanel);

			VisualTreeAsset rightPanelTree = Resources.Load<VisualTreeAsset>("LayoutRightPanel");
			VisualElement rightPanel = rootVisualElement.Q<VisualElement>(className: "rightPanel");
			rightPanel.styleSheets.Add(Resources.Load<StyleSheet>("StyleRightPanel"));
			rightPanelTree.CloneTree(rightPanel);

			m_TabView = rootVisualElement.Q<TabView>(className: "puppeteerTabView");

			var textLogo = rootVisualElement.Q<Image>(className: "textLogo");
			textLogo.image = PuppeteerEditorResourceLoader.TextLogo32.texture;

			m_TabView.SetConfiguratorWindow(this);

			m_TabView.AddTab(new GoalView(rootVisualElement, leftPanel, rightPanel, _updated => m_ConfiguratorStates.LastOpenedGoal = _updated));
			m_TabView.AddTab(new ActionView(rootVisualElement, leftPanel, rightPanel, _updated => m_ConfiguratorStates.LastOpenedAction = _updated));
			m_TabView.AddTab(new SensorView(rootVisualElement, leftPanel, rightPanel, _updated => m_ConfiguratorStates.LastOpenedSensor = _updated));
			m_TabView.AddTab(new ArchetypeView(rootVisualElement, leftPanel, rightPanel, _updated => m_ConfiguratorStates.LastOpenedArchetype = _updated));
			m_TabView.AddTab(new AgentView(rootVisualElement, leftPanel, rightPanel, _updated => m_ConfiguratorStates.LastOpenedAgentObjectID = _updated));

			m_TabView.OnTabListItemSelected = _updated => m_ConfiguratorStates.TabTypeName = _updated;
			m_TabView.Enable();

			RestoreLastOpened();
			AddShortcuts();
			RegisterCallbacks();

			TextElement versionText = rootVisualElement.Q<TextElement>(name: "version");
			versionText.text = PuppeteerEditorHelper.GetVersion();

			StyleSheet styleSheet = Resources.Load<StyleSheet>("StylePuppeteerConfiguratorWindow");
			rootVisualElement.styleSheets.Add(styleSheet);

			StyleSheet themeSheet = Resources.Load<StyleSheet>(EditorGUIUtility.isProSkin ? "StyleDarkTheme" : "StyleLightTheme");
			rootVisualElement.styleSheets.Add(themeSheet);
		}

		public void OpenEntryInTabOfType<T>(int _objectInstanceID) where T : AgentView
		{
			m_TabView?.OpenEntryInTabOfType<T>(_objectInstanceID);
		}

		public void OpenEntryInTabOfType<T>(Guid _guid) where T : PuppeteerView
		{
			m_TabView?.OpenEntryInTabOfType<T>(_guid);
		}

		public void OpenTabOfType<T>() where T : PuppeteerView
		{
			m_TabView?.OpenTabOfType<T>();
		}


		private static string GetRelativeFilePath()
		{
			// We want the state file to be next to this source file and determine the path like this to enable the user to move the toolkit to subfolders.

			string[] result = Directory.GetFiles(Application.dataPath, "PuppeteerConfiguratorWindow.cs", SearchOption.AllDirectories);
			string absolutePath = result[0].Replace("PuppeteerConfiguratorWindow.cs", "").Replace("\\", "/");
			return "Assets" + absolutePath.Substring(Application.dataPath.Length);
		}

		private static void InitConfigStatePath()
		{
			m_ConfigStateFilePath = Path.Combine(GetRelativeFilePath(), "ConfiguratorWindowState.asset");
		}


		[InitializeOnLoadMethod]
#pragma warning disable IDE0051 // Remove unused private members (Method is used by Unity through the attribute)
		private static void OnLoad()
		{
			if (!m_ConfiguratorStates)
			{
				if (m_ConfigStateFilePath == string.Empty)
				{
					InitConfigStatePath();
				}

				m_ConfiguratorStates = AssetDatabase.LoadAssetAtPath<SerialisedConfiguratorState>(m_ConfigStateFilePath);

				if (m_ConfiguratorStates == null)
				{
					m_ConfiguratorStates = CreateInstance<SerialisedConfiguratorState>();

					AssetDatabase.CreateAsset(m_ConfiguratorStates, m_ConfigStateFilePath);
					AssetDatabase.Refresh();
				}
			}
		}

#pragma warning restore IDE0051

		[OnOpenAsset]
#pragma warning disable IDE0051 // Remove unused private members (Method is used by Unity through the attribute)
#pragma warning disable IDE0060 // Remove unused parameter (Method signature is dictated by Unity)
		private static bool OnOpenAsset(int _instanceID, int _line)
		{
			string filePath = AssetDatabase.GetAssetPath(_instanceID);

			switch (Path.GetExtension(filePath))
			{
				case ".pxgo":
					OpenGoals();
					return true;

				case ".pxac":
					OpenActions();
					return true;

				case ".pxar":
					OpenArchetypes();
					return true;

				case ".pxse":
					OpenSensors();
					return true;

				default:
					return false;
			}
		}

#pragma warning restore IDE0060
#pragma warning restore IDE0051

		private void AddShortcuts()
		{
			m_ShortcutHandler.AddShortcuts(new ShortcutHandler.Shortcut[]
			{
			new ShortcutHandler.Shortcut { MainKey = KeyCode.S, CtrlKey = true, OnKeyDown = m_TabView.SaveCurrentTab},
			new ShortcutHandler.Shortcut { MainKey = KeyCode.S, CtrlKey = true, AltKey = true, OnKeyDown = m_TabView.SaveAllTabs, StopEventPropagation = true },
			new ShortcutHandler.Shortcut { MainKey = KeyCode.D, CtrlKey = true, OnKeyDown = m_TabView.DuplicateSelection},
			new ShortcutHandler.Shortcut { MainKey = KeyCode.Return, OnKeyDown = m_TabView.EnterSelection},
			new ShortcutHandler.Shortcut { MainKey = KeyCode.KeypadEnter, OnKeyDown = m_TabView.EnterSelection},
			new ShortcutHandler.Shortcut { MainKey = KeyCode.Escape, OnKeyDown = m_TabView.ClearSelection},
			new ShortcutHandler.Shortcut { MainKey = KeyCode.Delete, OnKeyDown = m_TabView.DeleteSelection},
			new ShortcutHandler.Shortcut { MainKey = KeyCode.Insert, OnKeyDown = m_TabView.AddNewBasedOnSelection},
			new ShortcutHandler.Shortcut { MainKey = KeyCode.UpArrow, OnKeyDown = () => m_TabView.MoveSelection(MoveDirection.Up)},
			new ShortcutHandler.Shortcut { MainKey = KeyCode.DownArrow, OnKeyDown = () => m_TabView.MoveSelection(MoveDirection.Down)},
			new ShortcutHandler.Shortcut { MainKey = KeyCode.LeftArrow, OnKeyDown = () => m_TabView.MoveSelection(MoveDirection.Left)},
			new ShortcutHandler.Shortcut { MainKey = KeyCode.RightArrow, OnKeyDown = () => m_TabView.MoveSelection(MoveDirection.Right)},
			});
		}

		private void RegisterCallbacks()
		{
			rootVisualElement.focusable = true;
			rootVisualElement.Focus();
			rootVisualElement.RegisterCallback<KeyDownEvent>(m_ShortcutHandler.ExecuteForEachShortcut);
			rootVisualElement.RegisterCallback<KeyUpEvent>(_event => m_ShortcutHandler.UnlockKeyDown());
		}

		private void RestoreLastOpened()
		{
			if (m_ConfiguratorStates != null && m_ConfiguratorStates.TabTypeName != null && !m_ConfiguratorStates.TabTypeName.Equals(string.Empty))
			{
				m_RestoreTabDict[Type.GetType(m_ConfiguratorStates.TabTypeName)].Invoke();
			}
			else
			{
				m_TabView.SelectFirst();
			}
		}

		private static string m_ConfigStateFilePath = string.Empty;

		[SerializeField]
		private static SerialisedConfiguratorState m_ConfiguratorStates;

		private readonly Dictionary<Type, System.Action> m_RestoreTabDict;
		private readonly ShortcutHandler m_ShortcutHandler = new ShortcutHandler();

		private TabView m_TabView;
	}
}