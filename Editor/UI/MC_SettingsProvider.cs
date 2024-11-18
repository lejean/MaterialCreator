using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace MaterialCreator {
    class MC_SettingsProvider : SettingsProvider {
        private SerializedObject m_CustomSettings;

        class Styles {
            public static GUIContent albedo = new GUIContent("Albedo Names");
            public static GUIContent opacity = new GUIContent("Opacity Names");
            public static GUIContent metallic = new GUIContent("Metallic Names");
            public static GUIContent specular = new GUIContent("Specular Names");
            public static GUIContent smootness = new GUIContent("Smootness Names");
            public static GUIContent height = new GUIContent("Height Names");
            public static GUIContent normal = new GUIContent("Normal Names");
            public static GUIContent occlusion = new GUIContent("Occlusion Names");
            public static GUIContent emission = new GUIContent("Emission Names");
            public static GUIContent detail = new GUIContent("Detail Names");
        }

        public MC_SettingsProvider(string path, SettingsScope scope = SettingsScope.User)
            : base(path, scope) { }

        public static bool IsSettingsAvailable() {
            return File.Exists(MC_Settings.k_MyCustomSettingsPath);
        }

        public override void OnActivate(string searchContext, VisualElement rootElement) {
            // This function is called when the user clicks on the MyCustom element in the Settings window.
            m_CustomSettings = MC_Settings.GetSerializedSettings();
        }

        public override void OnGUI(string searchContext) {
            // Use IMGUI to display UI:
            EditorGUILayout.PropertyField(m_CustomSettings.FindProperty("albedoNames"), Styles.albedo);
            EditorGUILayout.PropertyField(m_CustomSettings.FindProperty("opacityNames"), Styles.opacity);
            EditorGUILayout.PropertyField(m_CustomSettings.FindProperty("metallicNames"), Styles.metallic);
            EditorGUILayout.PropertyField(m_CustomSettings.FindProperty("specularNames"), Styles.specular);
            EditorGUILayout.PropertyField(m_CustomSettings.FindProperty("smoothnessNames"), Styles.smootness);
            EditorGUILayout.PropertyField(m_CustomSettings.FindProperty("heightNames"), Styles.height);
            EditorGUILayout.PropertyField(m_CustomSettings.FindProperty("normalNames"), Styles.normal);
            EditorGUILayout.PropertyField(m_CustomSettings.FindProperty("occlusionNames"), Styles.occlusion);
            EditorGUILayout.PropertyField(m_CustomSettings.FindProperty("emissionNames"), Styles.emission);
            EditorGUILayout.PropertyField(m_CustomSettings.FindProperty("detailNames"), Styles.detail);
            m_CustomSettings.ApplyModifiedPropertiesWithoutUndo();
        }

        // Register the SettingsProvider
        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider() {
            if (IsSettingsAvailable()) {
                var provider = new MC_SettingsProvider("Preferences/Material Creator", SettingsScope.User);

                // Automatically extract all keywords from the Styles.
                provider.keywords = GetSearchKeywordsFromGUIContentProperties<Styles>();
                return provider;
            }

            // Settings Asset doesn't exist yet; no need to display anything in the Settings window.
            return null;
        }
    }

}