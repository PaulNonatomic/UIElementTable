using System;
using System.IO;
using System.Reflection;
using Nonatomic.UIElements.Examples;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace Nonatomic.UIElements.ExamplesEditor
{
	/// <summary>
	/// One-click builder for the runtime UITable sample. Creates a theme (if the project has
	/// none), a PanelSettings asset, and a scene wired with a UIDocument + RuntimeUITableSample
	/// + an EventSystem, so the table can be exercised in Play mode. The input module is added
	/// reflectively so this editor assembly never hard-depends on the Input System package.
	/// </summary>
	public static class RuntimeSampleSceneBuilder
	{
		private const string ExamplesFolder = "Assets/UITableExamples";
		private const string RuntimeFolder = ExamplesFolder + "/Runtime";
		private const string ThemePath = RuntimeFolder + "/UITableSampleTheme.tss";
		private const string PanelSettingsPath = RuntimeFolder + "/UITableSamplePanelSettings.asset";
		private const string ScenePath = RuntimeFolder + "/UITableSampleScene.unity";

		[MenuItem("Window/UI Table Examples/Create Runtime Sample Scene")]
		public static void Create()
		{
			if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

			EnsureFolders();

			var theme = GetOrCreateTheme();
			if (theme == null)
			{
				EditorUtility.DisplayDialog("UITable sample",
					"Could not find or create a runtime theme. Create one via Assets > Create > UI Toolkit > Panel Settings, then run this again.",
					"OK");
				return;
			}

			var panelSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
			if (panelSettings == null)
			{
				panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
				panelSettings.themeStyleSheet = theme;
				AssetDatabase.CreateAsset(panelSettings, PanelSettingsPath);
			}
			else
			{
				panelSettings.themeStyleSheet = theme;
				EditorUtility.SetDirty(panelSettings);
			}

			AssetDatabase.SaveAssets();

			var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

			var uiGo = new GameObject("UI Table Sample");
			var doc = uiGo.AddComponent<UIDocument>();
			// Assign through SerializedObject: setting UIDocument.panelSettings via the property
			// in an editor script does not reliably persist into the saved scene.
			var docSerialized = new SerializedObject(doc);
			docSerialized.FindProperty("m_PanelSettings").objectReferenceValue = panelSettings;
			docSerialized.ApplyModifiedPropertiesWithoutUndo();
			uiGo.AddComponent<RuntimeUITableSample>();

			CreateEventSystem();

			EditorSceneManager.SaveScene(scene, ScenePath);
			AssetDatabase.Refresh();

			EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath));
			EditorUtility.DisplayDialog("UITable sample",
				"Runtime sample scene created at\n" + ScenePath + "\n\nIt is open now. Press Play to try the table.",
				"OK");
		}

		private static void CreateEventSystem()
		{
			var esGo = new GameObject("EventSystem");
			esGo.AddComponent<EventSystem>();

			// New Input System UI module, resolved by name so this assembly does not hard-depend
			// on the package at compile time.
			var moduleType = Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
			if (moduleType != null)
			{
				var module = esGo.AddComponent(moduleType);
				moduleType.GetMethod("AssignDefaultActions", BindingFlags.Public | BindingFlags.Instance)
					?.Invoke(module, null);
				return;
			}

			// Fall back to the legacy module if the Input System package is absent.
			var legacyType = Type.GetType("UnityEngine.EventSystems.StandaloneInputModule, UnityEngine.UI");
			if (legacyType != null) esGo.AddComponent(legacyType);
			else Debug.LogWarning("[UITable sample] No input module found; runtime clicks may not work.");
		}

		private static void EnsureFolders()
		{
			if (!AssetDatabase.IsValidFolder(ExamplesFolder))
				AssetDatabase.CreateFolder("Assets", "UITableExamples");
			if (!AssetDatabase.IsValidFolder(RuntimeFolder))
				AssetDatabase.CreateFolder(ExamplesFolder, "Runtime");
		}

		private static ThemeStyleSheet GetOrCreateTheme()
		{
			var existing = AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(ThemePath);
			if (existing != null) return existing;

			// Any theme already in the project will do.
			var guids = AssetDatabase.FindAssets("t:ThemeStyleSheet");
			if (guids.Length > 0)
				return AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(AssetDatabase.GUIDToAssetPath(guids[0]));

			// Otherwise write a .tss that imports Unity's built-in default runtime theme.
			File.WriteAllText(ThemePath, "@import url(\"unity-theme://default\");\n");
			AssetDatabase.ImportAsset(ThemePath, ImportAssetOptions.ForceSynchronousImport);
			return AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(ThemePath);
		}
	}
}
