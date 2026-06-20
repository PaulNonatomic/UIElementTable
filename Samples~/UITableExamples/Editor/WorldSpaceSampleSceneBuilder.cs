using System;
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
	/// One-click builder for a WORLD-SPACE version of the runtime table sample. Creates a
	/// PanelSettings with renderMode = WorldSpace, a scene with a UIDocument that reuses
	/// RuntimeUITableSample, a Panel Input Configuration for world-space clicks, and a camera
	/// framing the panel. World-space placement is visual, so expect to nudge the transform,
	/// the PanelSettings Pixels Per Unit, or the fixed size after the first run.
	/// </summary>
	public static class WorldSpaceSampleSceneBuilder
	{
		private const string RuntimeFolder = "Assets/UITableExamples/Runtime";
		private const string PanelSettingsPath = RuntimeFolder + "/UITableWorldSpacePanelSettings.asset";
		private const string ScenePath = RuntimeFolder + "/UITableWorldSpaceScene.unity";

		[MenuItem("Window/UI Table Examples/Create World Space Sample Scene")]
		public static void Create()
		{
			if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

			var theme = FindTheme();
			if (theme == null)
			{
				EditorUtility.DisplayDialog("UITable world space",
					"No ThemeStyleSheet found. Run 'Create Runtime Sample Scene' first (it creates one), then try again.",
					"OK");
				return;
			}

			// World-space PanelSettings. renderMode is a property on the asset and persists on save.
			var panelSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
			if (panelSettings == null)
			{
				panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
				AssetDatabase.CreateAsset(panelSettings, PanelSettingsPath);
			}

			panelSettings.themeStyleSheet = theme;
			panelSettings.renderMode = PanelRenderMode.WorldSpace;
			EditorUtility.SetDirty(panelSettings);
			AssetDatabase.SaveAssets();

			// Fresh scene. A ground plane plus an angled camera make the world-space placement
			// obvious; a head-on camera makes any world-space panel look like a flat 2D overlay.
			var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
			GameObject.CreatePrimitive(PrimitiveType.Plane).name = "Ground";
			if (Camera.main != null)
			{
				Camera.main.transform.position = new Vector3(3.5f, 2.6f, -4.5f);
				Camera.main.transform.LookAt(new Vector3(0f, 1.5f, 0f));
			}

			// UIDocument placed in front of the camera, reusing the runtime sample.
			var uiGo = new GameObject("World UI Table");
			uiGo.transform.position = new Vector3(0f, 1.5f, 0f);
			var doc = uiGo.AddComponent<UIDocument>();
			uiGo.AddComponent<RuntimeUITableSample>();

			// World-space size fields (value types persist fine via SerializedObject).
			var so = new SerializedObject(doc);
			SetEnum(so, "m_WorldSpaceSizeMode", 1); // 0 = Dynamic, 1 = Fixed
			SetFloat(so, "m_WorldSpaceWidth", 560f);
			SetFloat(so, "m_WorldSpaceHeight", 360f);
			so.ApplyModifiedPropertiesWithoutUndo();

			// World-space input routing + an EventSystem for the input pipeline.
			ConfigureWorldSpaceInput();
			CreateEventSystem();

			// Assign Panel Settings last, after the other components, so nothing clears it; then verify.
			AssignPanelSettings(doc, panelSettings);

			EditorSceneManager.SaveScene(scene, ScenePath);
			AssetDatabase.Refresh();

			EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath));
			EditorUtility.DisplayDialog("UITable world space",
				"World-space sample scene created at\n" + ScenePath +
				"\n\nPress Play. If the table faces away or is not visible, rotate 'World UI Table' 180 on Y. " +
				"Adjust its position/scale, the fixed size on the UIDocument, or PanelSettings > Pixels Per Unit to taste.",
				"OK");
		}

		private static ThemeStyleSheet FindTheme()
		{
			var guids = AssetDatabase.FindAssets("t:ThemeStyleSheet");
			return guids.Length > 0
				? AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(AssetDatabase.GUIDToAssetPath(guids[0]))
				: null;
		}

		private static void AssignPanelSettings(UIDocument doc, PanelSettings panelSettings)
		{
			var so = new SerializedObject(doc);
			var prop = so.FindProperty("m_PanelSettings");
			if (prop == null)
			{
				Debug.LogError("[UITable world space] UIDocument has no 'm_PanelSettings' field; assign Panel Settings manually.");
				return;
			}

			prop.objectReferenceValue = panelSettings;
			so.ApplyModifiedPropertiesWithoutUndo();
			EditorUtility.SetDirty(doc);

			var verify = new SerializedObject(doc);
			if (verify.FindProperty("m_PanelSettings").objectReferenceValue == null)
			{
				Debug.LogWarning("[UITable world space] Panel Settings did not persist programmatically. " +
					"Drag 'UITableWorldSpacePanelSettings' onto World UI Table > UIDocument > Panel Settings.");
			}
			else
			{
				Debug.Log("[UITable world space] Panel Settings assigned to the UIDocument.");
			}
		}

		private static void SetEnum(SerializedObject so, string prop, int index)
		{
			var p = so.FindProperty(prop);
			if (p != null) p.enumValueIndex = index;
		}

		private static void SetFloat(SerializedObject so, string prop, float value)
		{
			var p = so.FindProperty(prop);
			if (p != null) p.floatValue = value;
		}

		private static void SetBool(SerializedObject so, string prop, bool value)
		{
			var p = so.FindProperty(prop);
			if (p != null) p.boolValue = value;
		}

		private static void SetInt(SerializedObject so, string prop, int value)
		{
			var p = so.FindProperty(prop);
			if (p != null) p.intValue = value;
		}

		private static void ConfigureWorldSpaceInput()
		{
			var config = new GameObject("Panel Input Configuration").AddComponent<PanelInputConfiguration>();

			// AddComponent does not run a component's Reset(), so the inspector defaults that make
			// world-space input work are not applied. Set them explicitly.
			var so = new SerializedObject(config);
			SetBool(so, "m_ProcessWorldSpaceInput", true);
			SetBool(so, "m_DefaultEventCameraIsMainCamera", true);
			SetBool(so, "m_AutoCreatePanelComponents", true);
			SetEnum(so, "m_PanelInputRedirection", 1); // AutoSwitch
			SetInt(so, "m_InteractionLayers", -1);      // all layers
			SetFloat(so, "m_MaxInteractionDistance", 1000f);
			so.ApplyModifiedPropertiesWithoutUndo();
			EditorUtility.SetDirty(config);
		}

		private static void CreateEventSystem()
		{
			var esGo = new GameObject("EventSystem");
			esGo.AddComponent<EventSystem>();

			var moduleType = Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
			if (moduleType != null)
			{
				var module = esGo.AddComponent(moduleType);
				moduleType.GetMethod("AssignDefaultActions", BindingFlags.Public | BindingFlags.Instance)
					?.Invoke(module, null);
			}
		}
	}
}
