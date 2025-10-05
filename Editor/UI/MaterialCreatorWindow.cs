using UnityEditor;
using UnityEngine;

namespace MaterialCreator {
    public class MaterialCreatorWindow : EditorWindow {
        
        public string materialName = "";
        public int shaderType = 0;

        Shader shader;

        CreateMaterials createController;

        [MenuItem("Tools/Material Creator/Make Material")]
        static void Init() {
            // Get existing open window or if none, make a new one:
            MaterialCreatorWindow window = (MaterialCreatorWindow)GetWindowWithRect(typeof(MaterialCreatorWindow), new Rect(0, 0, 548, 383));
            window.Show();
        }

        void OnGUI() {
            createController = new();
            //Debug.Log("Window width: " + position.width);
            //Debug.Log("Window width: " + position.height);
            GUILayout.Label("Info", EditorStyles.boldLabel);
            GUILayout.Label("This tool will create a standard material from a selection of folders or textures through the click of a button.", EditorStyles.wordWrappedLabel);
            GUILayout.Space(10);

            GUILayout.Label("Note", EditorStyles.boldLabel);
            GUILayout.Label("To correctly fill in the textures, the name of map must be in the filename.", EditorStyles.wordWrappedLabel);
            GUILayout.Label("There is a list of predefined map names in Edit/Preferences/Material Creator.", EditorStyles.wordWrappedLabel);
            GUILayout.Label("You can edit the names depending on your needs");
            GUILayout.Space(10);

            GUILayout.Label("Make sure the texture search terms like 'metal' aren't in the base name of your texture.");
            GUILayout.Label("F.e. 'Metal floor 01 albedo' will be selected for the metal texture");
            GUILayout.Space(10);

            var materialTitle = new GUIStyle(GUI.skin.label);
            materialTitle.fontStyle = FontStyle.Bold;
            materialTitle.fontSize = 15;
            GUILayout.Label("Create Material", materialTitle);

            string[] shaderType = System.Enum.GetNames(typeof(MC_Utils.ShaderType));
            this.shaderType = EditorGUILayout.Popup("Shader type", this.shaderType, shaderType);
            GUILayout.Space(10);
                        
            materialName = EditorGUILayout.TextField("Material Name", materialName);
            GUILayout.Label("If left empty the creator takes the name from the texture. \n" +
                "Leave empty when selecting multiple folders", EditorStyles.helpBox);
            GUILayout.Space(10);

            //if you want to assign a custom shader, pass this shader to the create material function
            //shader = (Shader)EditorGUILayout.ObjectField("Shader", shader, typeof(Shader), false);

            var boldtext = new GUIStyle(GUI.skin.button);
            boldtext.fontStyle = FontStyle.Bold;
            boldtext.fontSize = 15;

            if (GUILayout.Button("Create Material", boldtext, GUILayout.Height(40))) {
                createController.CreateMaterial(this.shaderType, MC_Utils.GetPipeline(), materialName);
            }        
        }

        //Assets adds it to the right click menu in projects and the topmenu Assets (which is the same)
        [MenuItem("Assets/Tools/Material Creator/Create Material Standard(Specular)")]
        static void CreateMaterialSpecular() {
            //0 is spec, 1 metal
            CreateMaterials create = new();
            create.CreateMaterial(0, MC_Utils.GetPipeline());
        }

        [MenuItem("Assets/Tools/Material Creator/Create Material Standard")]
        static void CreateMaterialMetal() {
            //0 is spec, 1 metal
            CreateMaterials create = new();
            create.CreateMaterial(1, MC_Utils.GetPipeline());
        }        
    }
}
