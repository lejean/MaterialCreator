using UnityEditor;
using UnityEngine;

namespace MaterialCreator {
    public class TextureToolWindow : EditorWindow {
        
        public string textureName = "";
        public int shaderType = 0;
        public int textureType = 0;
        public bool InvertRoughness = false;    //when you have a roughness map instead of the gloss map that unity expects you can invert it
        public bool RemoveOriginal = false;     //deletes the metal/spec, roughness/gloss, color, opacity map after merging them

        [MenuItem("Tools/Material Creator/Texture Tools")]
        static void Init() {
            // Get existing open window or if none, make a new one:
            TextureToolWindow window = (TextureToolWindow)GetWindowWithRect(typeof(TextureToolWindow), new Rect(0, 0, 548, 600));
            window.Show();
        }

        void OnGUI() {
            GUILayout.Label("Info", EditorStyles.boldLabel);
            GUILayout.Label("These tools allow you to merge 2 textures into 1 RGBA texture without third party tools.", EditorStyles.wordWrappedLabel);
            GUILayout.Label("For example merging your Albedo/Opacity texture or your Metallic(Specular)/Smoothness texture.", EditorStyles.wordWrappedLabel);
            GUILayout.Label("Just select both textures and push the buttons below.", EditorStyles.wordWrappedLabel);
            GUILayout.Space(10);
            GUILayout.Label("Note", EditorStyles.boldLabel);
            GUILayout.Label("To correctly merge the textures, the name of map must be in the filename.", EditorStyles.wordWrappedLabel);
            GUILayout.Label("There is a list of predefined map names in Edit/Preferences/Material Creator.", EditorStyles.wordWrappedLabel);
            GUILayout.Label("You can edit the names depending on your needs");
            GUILayout.Space(10);

            var textureTitle = new GUIStyle(GUI.skin.label);
            textureTitle.fontStyle = FontStyle.Bold;
            textureTitle.fontSize = 15;
            GUILayout.Label("Texture Tools",textureTitle);
            GUILayout.Space(10);

            string[] options = new string[] { "Metallic", "Specular" };
            shaderType = EditorGUILayout.Popup("Shader type", shaderType, options);
            GUILayout.Space(10);
                        
            textureName = EditorGUILayout.TextField("Texture Name", textureName);
            GUILayout.Label("Name of your texture, texture slots get added automatically to the name when merging. \n" +
                "Leave empty when selecting multiple folders", EditorStyles.helpBox);
            GUILayout.Space(10);

            string[] textureOptions = new string[] { "PNG", "TGA" };
            textureType = EditorGUILayout.Popup("Texture type", textureType, textureOptions);
            GUILayout.Space(10);

            InvertRoughness = EditorGUILayout.Toggle("Invert Smoothness", InvertRoughness);
            GUILayout.Label("Your smoothness texture will be automatically inverted when merging with your metal/specular texture", EditorStyles.helpBox);
            GUILayout.Space(10);

            RemoveOriginal = EditorGUILayout.Toggle("Delete Original Textures", RemoveOriginal);
            GUILayout.Label("Delete the original textures after merging, USE WITH CAUTION!!", EditorStyles.helpBox);
            GUILayout.Space(10);

            if (GUILayout.Button("Merge Metal/spec-smoothness texture")) {
                new MergeSmoothness().Merge(CreateModel(), InvertRoughness);
            }
            GUILayout.Space(10);

            if (GUILayout.Button("Merge Albedo-opacity texture")) {
                new MergeOpacity().Merge(CreateModel());
            }
            GUILayout.Space(10);

            if (GUILayout.Button("Invert Texture")) {
                new InvertTexture().Invert();
            }
            GUILayout.Label("Select texture and invert manually", EditorStyles.helpBox);            
        }

        [MenuItem("Assets/Tools/Material Creator/Invert Texture")]
        static void InvertTexture() {
            new InvertTexture().Invert();
        }

        MergeTextureModel CreateModel() {
            return new(textureName, shaderType, textureType, RemoveOriginal);
        }
    }
}