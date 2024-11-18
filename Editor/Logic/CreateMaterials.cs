using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MaterialCreator {
    //Controller
    public class CreateMaterials {

        /*

        TODO:
        Scan for shader type automatically    
        Handle when textures contain variations in same folder
        One button for merging + making material (requires postprocessor)

        */

        Shader shader;

        //lower case search terms, all strings get checked when lower case
        List<string> albedoNames => MC_Settings.GetOrCreateSettings().albedoNames;
        List<string> opacityNames => MC_Settings.GetOrCreateSettings().opacityNames;
        List<string> metallicNames => MC_Settings.GetOrCreateSettings().metallicNames;
        List<string> specularNames => MC_Settings.GetOrCreateSettings().specularNames;
        List<string> heightNames => MC_Settings.GetOrCreateSettings().heightNames;
        List<string> normalNames => MC_Settings.GetOrCreateSettings().normalNames;
        List<string> occlusionNames => MC_Settings.GetOrCreateSettings().occlusionNames;
        List<string> emissionNames => MC_Settings.GetOrCreateSettings().emissionNames;
        List<string> detailNames => MC_Settings.GetOrCreateSettings().detailNames;

        public void CreateMaterial(int shaderType, string materialName = "") {
            try {
                AssetDatabase.StartAssetEditing();

                LoadShader(shaderType);

                List<Texture2D> textures = new();

                if (MC_Utils.AssetIsDirectory(Selection.activeObject)) {
                    foreach (var asset in Selection.objects) {
                        if (MC_Utils.AssetIsDirectory(asset)) {
                            textures = MC_Utils.CollectTexturesInDirectory(asset);

                            if (textures.Count != 0) {
                                SetMaterialTextures(textures, shaderType, materialName);
                            } else {
                                Debug.Log("no textures found in selection");
                            }
                        } else {
                            Debug.Log("Item in selection isn't a directory");
                        }
                    }
                } else {
                    textures = MC_Utils.FilterTextures();

                    if (textures.Count != 0) {
                        SetMaterialTextures(textures, shaderType, materialName);
                    } else {
                        Debug.LogWarning("No textures found in selection. (Selecting folder in left side of two column layout doesn't work)");
                    }
                }
            } finally {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.SaveAssets();
            }
        }

        void LoadShader(int shaderType) {
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
        }

        //Assign the textures to the correct texture slots
        void SetMaterialTextures(List<Texture2D> textures, int shaderType, string materialName) {
            Texture2D albedo = null;
            Texture2D metal_spec = null;
            Texture2D normal = null;
            Texture2D height = null;
            Texture2D ao = null;
            Texture2D emission = null;
            Texture2D detail = null;

            string textureName = "";

            foreach (var tex in textures) {
                if (albedoNames.Any(s => tex.name.ToLower().Contains(s))) {
                    //use albedo name for renaming the file
                    textureName = tex.name;
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

            if (materialName == "") {
                //remove all the names from the albedo list so that you're left with the base name of the object
                foreach (var albedoName in albedoNames) {
                    textureName = textureName.Replace(albedoName, "");
                }

                foreach (var opacityName in opacityNames) {
                    textureName = textureName.Replace(opacityName, "");
                }
            } else {
                textureName = materialName;
            }

            string path = AssetDatabase.GetAssetPath(textures[0]);
            path = path.Substring(0, path.LastIndexOf("/")) + "/" + textureName + ".mat";

            Material mat = null;

            var materialAsset = AssetDatabase.LoadAssetAtPath(path, typeof(Material)) as Material;

            if (materialAsset != null) {
                FillInTextures(materialAsset);
            } else {
                mat = new(shader);
                FillInTextures(mat);
                AssetDatabase.CreateAsset(mat, path);
            }


            void FillInTextures(Material mat) {
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
            }

            //string path = AssetDatabase.GetAssetPath(textures[0]);
            //path = path.Substring(0, path.LastIndexOf("/")) + "/" + textureName + ".mat";

            //if (AssetDatabase.LoadAssetAtPath(path, typeof(Material)) != null) {
            //    Debug.LogWarning("Can't create material, it already exists: " + path);
            //    return;
            //}

            //AssetDatabase.CreateAsset(mat, path);
        }

    }
}