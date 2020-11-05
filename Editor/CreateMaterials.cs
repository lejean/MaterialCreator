using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MaterialCreator {
    public class CreateMaterials : EditorWindow {

        /*

        TODO:
        Scan for shader type automatically    
        Handle when textures contain variations in same folder
        One button for merging + making material

        DONE:
        Scan if directory or texture selected
        Fix read/write error
        Multiple folders
        Texture Types

         */

        Shader shader;

        public string materialName = "";
        public int shaderType = 0;
        public int textureType = 0;
        //public bool SelectionIsFolder = false;  //you can select the folder containing the textures instead of selecting the separate textures
        public bool InvertRoughness = false;    //when you have a roughness map instead of the gloss map that unity expects you can invert it
        public bool RemoveOriginal = false;     //deletes the metal/spec, roughness/gloss, color, opacity map after merging them

        //lower case search terms, all strings get checked when lower case
        List<string> albedoNames = new List<string> { "base", "albedo", "diffuse", "dfs", "color" };
        List<string> opacityNames = new List<string> { "opacity", "alpha" };
        List<string> metallicNames = new List<string> { "metallic", "metal" };
        List<string> specularNames = new List<string> { "spec", "reflection" };
        List<string> smoothnessNames = new List<string> { "gloss", "smoothness", "roughness" };
        List<string> heightNames = new List<string> { "height" };
        List<string> normalNames = new List<string> { "normal", "nrm" };
        List<string> occlusionNames = new List<string> { "occlusion", "ao" };
        List<string> emissionNames = new List<string> { "emission" };
        List<string> detailNames = new List<string> { "detail" };

        //path for using in assetpostprocessor
        //public static string specMetalPath = "test1";

        // Add menu named "My Window" to the Window menu
        [MenuItem("Tools/Material Creator")]
        static void Init() {
            // Get existing open window or if none, make a new one:
            CreateMaterials window = (CreateMaterials)GetWindowWithRect(typeof(CreateMaterials), new Rect(0, 0, 548, 683));
            window.Show();
        }

        void OnGUI() {
            //Debug.Log("Window width: " + position.width);
            //Debug.Log("Window width: " + position.height);
            GUILayout.Label("Info", EditorStyles.boldLabel);
            GUILayout.Label("This tool will create a standard material from a selection of folders or textures through the click of a button.", EditorStyles.wordWrappedLabel);
            GUILayout.Label("You can also merge your metal/spec-gloss and albedo-opacity textures without using third party software.", EditorStyles.wordWrappedLabel);
            GUILayout.Space(10);
            GUILayout.Label("Make sure the texture search terms like 'metal' aren't in the base name of your texture.");
            GUILayout.Label("F.e. 'Metal floor 01 albedo' will be selected for the metal texture");
            GUILayout.Space(10);
            GUILayout.Label("You can add more search terms to the list in the CreateMaterials script.");
            GUILayout.Label("(Close and reopen this window after compiling for it to take effect)");


            GUILayout.Label("General Settings", EditorStyles.boldLabel);
            string[] options = new string[] { "Metallic", "Specular" };
            shaderType = EditorGUILayout.Popup("Shader type", shaderType, options);
            GUILayout.Space(10);
            
            materialName = EditorGUILayout.TextField("Object Name", materialName);
            GUILayout.Label("Name of your material/texture, texture slots get added automatically to the name when merging. \n" +
                "If left empty the creator tries to make the name automatically but could be wrong. \n" +
                "Leave empty when selecting multiple folders", EditorStyles.helpBox);
            GUILayout.Space(10);

            GUILayout.Label("Texture Settings", EditorStyles.boldLabel);

            string[] textureOptions = new string[] { "PNG", "TGA" };
            textureType = EditorGUILayout.Popup("Texture type", textureType, textureOptions);
            GUILayout.Space(10);

            InvertRoughness = EditorGUILayout.Toggle("Invert Roughness", InvertRoughness);
            GUILayout.Label("Invert your roughness map to a gloss map", EditorStyles.helpBox);
            GUILayout.Space(10);

            RemoveOriginal = EditorGUILayout.Toggle("Delete Original Textures", RemoveOriginal);
            GUILayout.Label("Delete the original textures after merging, USE WITH CAUTION!!", EditorStyles.helpBox);

            //GUI.DrawTexture(new Rect(10, 10, 60, 60), EditorGUIUtility.whiteTexture);

            GUILayout.Space(20);
            if (GUILayout.Button("Merge Metal/spec-smoothness texture")) {
                MergeSmoothness();
            }

            if (GUILayout.Button("Merge Albedo-opacity texture")) {
                MergeOpacity();
            }

            GUILayout.Space(20);
            //GUILayout.Label("Select the folder or input maps for the material to create and autofill the material", EditorStyles.helpBox);

            var boldtext = new GUIStyle(GUI.skin.button);
            boldtext.fontStyle = FontStyle.Bold;
            boldtext.fontSize = 20;

            if (GUILayout.Button("Create Material", boldtext, GUILayout.Height(50))) {
                CreateMaterial();
            }
        }

        #region metal-spec-gloss texture
        void MergeSmoothness() {
            try {
                AssetDatabase.StartAssetEditing();

                List<Texture2D> textures = new List<Texture2D>();

                if (AssetIsDirectory(Selection.activeObject)) {
                    foreach (var asset in Selection.objects) {
                        if (AssetIsDirectory(asset)) {
                            textures = CollectTexturesInDirectory(asset);

                            if (textures.Count != 0) {
                                CreateSmoothness(textures);
                            }
                            else {
                                Debug.Log("no textures found in selection");
                            }
                        }
                        else {
                            Debug.Log("Item in selection isn't a directory");
                        }
                    }
                }
                else {
                    textures = FilterTextures();

                    if (textures.Count != 0) {
                        CreateSmoothness(textures);
                    }
                    else {
                        Debug.Log("no textures found in selection");
                    }
                }
            }
            finally {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.SaveAssets();
            }
        }

        void CreateSmoothness(List<Texture2D> textures) {
            Texture2D metal_spec = null;
            Texture2D smoothness = null;

            string name = "";
            string metalSpecPath = "";
            string glossPath = "";

            foreach (var tex in textures) {
                switch (shaderType) {
                    case 0:
                        if (metallicNames.Any(s => tex.name.ToLower().Contains(s))) {
                            name = tex.name;
                            metalSpecPath = AssetDatabase.GetAssetPath(tex);
                            metal_spec = duplicateTexture(tex);
                            continue;
                        }
                        break;

                    case 1:
                        if (specularNames.Any(s => tex.name.ToLower().Contains(s))) {
                            name = tex.name;
                            metalSpecPath = AssetDatabase.GetAssetPath(tex);
                            metal_spec = duplicateTexture(tex);
                            continue;
                        }
                        break;
                }

                if (smoothnessNames.Any(s => tex.name.ToLower().Contains(s))) {
                    glossPath = AssetDatabase.GetAssetPath(tex);
                    smoothness = duplicateTexture(tex);
                    continue;
                }
            }

            //get the path of the texture by removing everything after the last backslash
            string texturePath = AssetDatabase.GetAssetPath(textures.ElementAt(0));
            texturePath = texturePath.Substring(0, texturePath.LastIndexOf("/"));

            //rename the file with the correct suffix
            switch (shaderType) {
                case 0:
                    if (materialName == "") {
                        foreach (var metalName in metallicNames) {
                            name = name.Replace(metalName, "");
                        }
                        name += " metal gloss";
                    }
                    else {
                        name = materialName + " metal gloss";
                    }
                    break;
                case 1:
                    if (materialName == "") {
                        foreach (var specularName in specularNames) {
                            name = name.Replace(specularName, "");
                        }
                        name += " spec gloss";
                    }
                    else {
                        name = materialName + " spec gloss";
                    }
                    break;
                default:
                    break;
            }

            var file = Path.Combine(texturePath, name);
            //specMetalPath = file;
            Debug.Log("SaveToFile");
            SaveToFile(file, metal_spec, smoothness, InvertRoughness);

            if (RemoveOriginal) {
                AssetDatabase.DeleteAsset(metalSpecPath);
                AssetDatabase.DeleteAsset(glossPath);
            }

            //Metal-spec map doesn't get assigned to the material after creation, need to use asset postprocessor to do everything in 1 click
            //CreateMaterial();
        }
        #endregion

        #region albedo-opacity texture
        void MergeOpacity() {
            try {
                AssetDatabase.StartAssetEditing();

                List<Texture2D> textures = new List<Texture2D>();

                if (AssetIsDirectory(Selection.activeObject)) {
                    foreach (var asset in Selection.objects) {
                        if (AssetIsDirectory(asset)) {
                            textures = CollectTexturesInDirectory(asset);

                            if (textures.Count != 0) {
                                CreateOpacity(textures);
                            }
                            else {
                                Debug.Log("no textures found in selection");
                            }
                        }
                        else {
                            Debug.Log("Item in selection isn't a directory");
                        }
                    }
                }
                else {
                    textures = FilterTextures();

                    if (textures.Count != 0) {
                        CreateOpacity(textures);
                    }
                    else {
                        Debug.Log("no textures found in selection");
                    }
                }
            }
            finally {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.SaveAssets();
            }
        }

        void CreateOpacity(List<Texture2D> textures) {
            Texture2D albedo = null;
            Texture2D opacity = null;

            string name = "";
            string albedoPath = "";
            string opacityPath = "";

            foreach (var tex in textures) {
                if (albedoNames.Any(s => tex.name.ToLower().Contains(s))) {
                    name = tex.name;
                    albedoPath = AssetDatabase.GetAssetPath(tex);
                    albedo = duplicateTexture(tex);
                    continue;
                }

                if (opacityNames.Any(s => tex.name.ToLower().Contains(s))) {
                    opacityPath = AssetDatabase.GetAssetPath(tex);
                    opacity = duplicateTexture(tex);
                    continue;
                }
            }

            string texturePath = AssetDatabase.GetAssetPath(textures[0]);
            texturePath = texturePath.Substring(0, texturePath.LastIndexOf("/"));

            //rename the file with the correct suffix
            if (materialName == "") {
                //remove all the names from the albedo list so that you're left with the base name of the object
                foreach (var albedoName in albedoNames) {
                    name = name.Replace(albedoName, "");
                }
                name += " albedo opacity";
            }
            else {
                name = materialName + " albedo opacity";
            }

            var file = Path.Combine(texturePath, name);

            SaveToFile(file, albedo, opacity, false);

            if (RemoveOriginal) {
                AssetDatabase.DeleteAsset(albedoPath);
                AssetDatabase.DeleteAsset(opacityPath);
            }
        }
        #endregion

        #region material creation
        void CreateMaterial() {
            try {
                AssetDatabase.StartAssetEditing();

                List<Texture2D> textures = new List<Texture2D>();

                if (AssetIsDirectory(Selection.activeObject)) {
                    foreach (var asset in Selection.objects) {
                        if (AssetIsDirectory(asset)) {
                            textures = CollectTexturesInDirectory(asset);

                            if (textures.Count != 0) {
                                SetMaterialTextures(textures);
                            }
                            else {
                                Debug.Log("no textures found in selection");
                            }
                        }
                        else {
                            Debug.Log("Item in selection isn't a directory");
                        }
                    }
                }
                else {
                    textures = FilterTextures();

                    if (textures.Count != 0) {
                        SetMaterialTextures(textures);
                    }
                    else {
                        Debug.Log("no textures found in selection");
                    }
                }
            }
            finally {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.SaveAssets();
            }
        }

        //Assign the textures to the correct texture slots
        void SetMaterialTextures(List<Texture2D> textures) {
            Texture2D albedo = null;
            Texture2D metal_spec = null;
            Texture2D normal = null;
            Texture2D height = null;
            Texture2D ao = null;
            Texture2D emission = null;
            Texture2D detail = null;

            //int shaderType = 0;

            string name = "";

            switch (shaderType) {
                case 0:
                    shader = Shader.Find("Standard");
                    break;
                case 1:
                    shader = Shader.Find("Standard (Specular setup)");
                    break;
                default:
                    break;
            }

            foreach (var tex in textures) {
                if (albedoNames.Any(s => tex.name.ToLower().Contains(s))) {
                    //use albedo name for renaming the file
                    name = tex.name;
                    albedo = tex;
                    continue;
                }

                switch (shaderType) {
                    case 0:
                        if (metallicNames.Any(s => tex.name.ToLower().Contains(s))) {
                            metal_spec = tex;
                            continue;
                        }
                        break;
                    case 1:
                        if (specularNames.Any(s => tex.name.ToLower().Contains(s))) {
                            metal_spec = tex;
                            continue;
                        }
                        break;
                }

                if (heightNames.Any(s => tex.name.ToLower().Contains(s))) {
                    height = tex;
                    continue;
                }

                if (normalNames.Any(s => tex.name.ToLower().Contains(s))) {
                    normal = tex;
                    continue;
                }

                if (occlusionNames.Any(s => tex.name.ToLower().Contains(s))) {
                    ao = tex;
                    continue;
                }

                if (emissionNames.Any(s => tex.name.ToLower().Contains(s))) {
                    emission = tex;
                    continue;
                }

                if (detailNames.Any(s => tex.name.ToLower().Contains(s))) {
                    detail = tex;
                    continue;
                }
            }

            var mat = new Material(shader);

            mat.mainTexture = albedo;

            switch (shaderType) {
                case 0:
                    mat.SetTexture("_MetallicGlossMap", metal_spec);
                    break;
                case 1:
                    mat.SetTexture("_SpecGlossMap", metal_spec);
                    break;
                default:
                    break;
            }

            mat.SetTexture("_BumpMap", normal);
            mat.SetTexture("_OcclusionMap", ao);
            mat.SetTexture("_ParallaxMap", height);
            mat.SetTexture("_EmissionMap", emission);
            mat.SetTexture("_DetailMask", detail);

            if (materialName == "") {
                //remove all the names from the albedo list so that you're left with the base name of the object
                foreach (var albedoName in albedoNames) {
                    name = name.Replace(albedoName, "");
                }
            }
            else {
                name = materialName;
            }

            string path = AssetDatabase.GetAssetPath(textures[0]);
            path = path.Substring(0, path.LastIndexOf("/")) + "/" + name + ".mat";

            if (AssetDatabase.LoadAssetAtPath(path, typeof(Material)) != null) {
                Debug.LogWarning("Can't create material, it already exists: " + path);
                return;
            }

            AssetDatabase.CreateAsset(mat, path);
        }
        #endregion

        //Check if the selected object is a directory
        bool AssetIsDirectory(UnityEngine.Object selection) {
            string filePath = AssetDatabase.GetAssetPath(selection);
            FileAttributes attr = File.GetAttributes(filePath);

            //check if the selected object is a folder
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory) {
                return true;
            }
            else {
                return false;
            }
        }

        //if the texture is not read/write enabled, we duplicate the texture to a rendertexture and use that
        Texture2D duplicateTexture(Texture2D source) {
            RenderTexture renderTex = RenderTexture.GetTemporary(
                        source.width,
                        source.height,
                        0,
                        RenderTextureFormat.Default,
                        RenderTextureReadWrite.Default);

            Graphics.Blit(source, renderTex);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTex;
            Texture2D readableText = new Texture2D(source.width, source.height);
            readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
            readableText.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTex);
            return readableText;
        }

        //Return a list of textures from a directory
        List<Texture2D> CollectTexturesInDirectory(UnityEngine.Object directory) {
            string[] filePaths = Directory.GetFiles(AssetDatabase.GetAssetPath(directory));
            List<Texture2D> selection = new List<Texture2D>();

            for (int i = 0; i < filePaths.Length; i++) {
                UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(filePaths[i], typeof(Texture2D));

                if (obj is Texture2D) {
                    selection.Add(obj as Texture2D);
                }
            }

            return selection;
        }

        //Filter all textures from your selection if it's not a directory
        List<Texture2D> FilterTextures() {
            //Texture2D[] textures;
            List<Texture2D> textures;

            var filteredSelection = Selection.GetFiltered(typeof(Texture2D), SelectionMode.Assets).Cast<Texture2D>();
            textures = filteredSelection.ToList();

            return textures;
        }

        //create the texture and save it to the given path
        private void SaveToFile(string path, Texture2D rgb, Texture2D alpha, bool invertAlpha) {
            var tex = new Texture2D[] { rgb, rgb, rgb, alpha };
            var chn = new int[] { 0, 1, 2, 0 };
            var inv = new bool[] { false, false, false, invertAlpha };

            var lut = new float[4, 256];
            var curve = AnimationCurve.Linear(0, 0, 1, 1);
            for (int i = 0; i < 256; i++) {
                var v = Mathf.Clamp01(curve.Evaluate(i / 255f));
                lut[0, i] = v;
                lut[1, i] = v;
                lut[2, i] = v;
                lut[3, i] = v;
            }

            Texture2D image = null;
            byte[] data = null;

            switch (textureType) {
                case 0:
                    //png
                    path = Path.ChangeExtension(path, ".png");
                    image = Remap(tex, chn, inv, lut);
                    data = image.EncodeToPNG();
                    //var data = image.EncodeToTGA();
                    break;
                case 1:
                    //tga
                    path = Path.ChangeExtension(path, ".tga");
                    image = Remap(tex, chn, inv, lut);
                    data = image.EncodeToTGA();
                    break;
                default:
                    break;
            }
            //path = Path.ChangeExtension(path, ".tga");
            //var image = Remap(tex, chn, inv, lut);
            //var data = image.EncodeToPNG();
            //var data = image.EncodeToTGA();
            File.WriteAllBytes(path, data);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            DestroyImmediate(image);

            //Debug.Log("texture saved");
        }

        #region modifications

        public Texture2D Remap(Texture2D[] tex, int[] chn, bool[] inv, float[,] lut) {
            int w, h;

            w = h = 0;
            for (int i = 0; i < 4; i++)
                if (tex[i] != null) {
                    w = Math.Max(w, tex[i].width);
                    h = Math.Max(h, tex[i].height);
                }

            var rdata = GetTextureData(tex[0], w, h, Color.black);
            var gdata = tex[1] == tex[0] ? rdata : GetTextureData(tex[1], w, h, Color.black);
            var bdata = tex[2] == tex[0] ? rdata : tex[2] == tex[1] ? gdata : GetTextureData(tex[2], w, h, Color.black);
            var adata = tex[3] == tex[0] ? rdata : tex[3] == tex[1] ? gdata : tex[3] == tex[2] ? bdata : GetTextureData(tex[3], w, h, Color.white);

            float r, g, b, a;
            var image = new Texture2D(w, h, TextureFormat.ARGB32, false, false);
            var data = image.GetPixels();

            for (int i = 0; i < w * h; i++) {
                r = GetChannelColor(chn[0], rdata[i]);
                g = GetChannelColor(chn[1], gdata[i]);
                b = GetChannelColor(chn[2], bdata[i]);
                a = GetChannelColor(chn[3], adata[i]);

                r = lut[0, (int)(r * 255)];
                g = lut[1, (int)(g * 255)];
                b = lut[2, (int)(b * 255)];
                a = lut[3, (int)(a * 255)];

                if (inv[0]) r = 1 - r;
                if (inv[1]) g = 1 - g;
                if (inv[2]) b = 1 - b;
                if (inv[3]) a = 1 - a;
                data[i] = new Color(r, g, b, a);
            }
            image.SetPixels(data);
            image.Apply();
            return image;
        }

        private float GetChannelColor(int chn, Color color) {
            float c;
            switch (chn) {
                case 0: c = color.r; break;
                case 1: c = color.g; break;
                case 2: c = color.b; break;
                case 3: c = color.a; break;
                case 4: c = (color.r + color.g + color.b) / 3; break;
                case 5: c = 0.2126f * color.r + 0.7152f * color.g + 0.0722f * color.b; break;
                default: c = 0; break;
            }
            return c;
        }

        private Color[] GetTextureData(Texture2D tex, int width, int height, Color fill) {
            Color[] data = null;

            if (tex == null) {
                data = new Color[width * height];
                for (int i = 0; i < width * height; i++)
                    data[i] = fill;
                return data;
            }

            var wrapU = tex.wrapMode == TextureWrapMode.Repeat;
            var wrapV = tex.wrapMode == TextureWrapMode.Repeat;
#if UNITY_2017_1_OR_NEWER
            wrapU = tex.wrapModeU == TextureWrapMode.Repeat;
            wrapV = tex.wrapModeV == TextureWrapMode.Repeat;
#endif
            var ts = new TextureSampler(tex, wrapU, wrapV);
            ts.Scale(width, height);
            data = ts.GetData();

            return data;
        }
        #endregion
    } 
}