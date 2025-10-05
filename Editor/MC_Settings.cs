using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace MaterialCreator {
    // Create a new type of Settings Asset.
    class MC_Settings : ScriptableObject {
        public const string k_MyCustomSettingsPath = k_MyCustomSettingsFilePath + k_MyCustomSettingsFileName;
        public const string k_MyCustomSettingsFilePath = "Assets/Settings/MaterialCreator/";
        public const string k_MyCustomSettingsFileName = "MC_Settings.asset";

        [Header("Material Creator")]
        //lower case search terms, all strings get checked when lower case
        public List<string> albedoNames = new List<string> { "base", "albedo", "diffuse", "dfs", "color" };
        public List<string> opacityNames = new List<string> { "opacity", "alpha" };
        public List<string> metallicNames = new List<string> { "metallic", "metal" };
        public List<string> specularNames = new List<string> { "spec", "reflection" };
        public List<string> smoothnessNames = new List<string> { "gloss", "smoothness", "roughness" };
        public List<string> heightNames = new List<string> { "height" };
        public List<string> normalNames = new List<string> { "normal", "nrm" };
        public List<string> occlusionNames = new List<string> { "occlusion", "ao" };
        public List<string> emissionNames = new List<string> { "emission", "emissive" };
        public List<string> detailNames = new List<string> { "detail" };

        internal static MC_Settings GetOrCreateSettings() {
            if (!AssetDatabase.AssetPathExists(k_MyCustomSettingsPath)) {
                Directory.CreateDirectory(Path.GetDirectoryName(k_MyCustomSettingsFilePath));
            }
                
            var settings = AssetDatabase.LoadAssetAtPath<MC_Settings>(k_MyCustomSettingsPath);
            if (settings == null) {
                settings = ScriptableObject.CreateInstance<MC_Settings>();
                AssetDatabase.CreateAsset(settings, k_MyCustomSettingsPath);
                AssetDatabase.SaveAssets();
            }
            return settings;
        }

        internal static SerializedObject GetSerializedSettings() {
            return new SerializedObject(GetOrCreateSettings());
        }
    }
}