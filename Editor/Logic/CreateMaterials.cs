using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MaterialCreator {
    //Controller
    public class CreateMaterials {

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

        public void CreateMaterial(int shaderType, MC_Utils.Pipeline pipeline, string materialName = "") {
            try {
                AssetDatabase.StartAssetEditing();

                List<Texture2D> textures = new();

                if (MC_Utils.AssetIsDirectory(Selection.activeObject)) {
                    foreach (var asset in Selection.objects) {
                        if (MC_Utils.AssetIsDirectory(asset)) {
                            textures = MC_Utils.CollectTexturesInDirectory(asset);

                            if (textures.Count != 0) {
                                SetMaterialTextures(textures, shaderType, pipeline, materialName);
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
                        SetMaterialTextures(textures, shaderType, pipeline, materialName);
                    } else {
                        Debug.LogWarning("No textures found in selection. (Selecting folder in left side of two column layout doesn't work)");
                    }
                }
            } finally {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.SaveAssets();
            }
        }

        //Assign the textures to the correct texture slots
        void SetMaterialTextures(List<Texture2D> textures, int shaderType, MC_Utils.Pipeline pipeline, string materialName) {
            Texture2D albedo = null;
            Texture2D metal_spec = null;
            Texture2D normal = null;
            Texture2D height = null;
            Texture2D ao = null;
            Texture2D emission = null;
            Texture2D detail = null;

            foreach (var tex in textures) {
                if (albedoNames.Any(s => tex.name.ToLower().Contains(s))) {
                    albedo = tex;
                    continue;
                }

                switch (shaderType) {
                    case 0:
                        if (specularNames.Any(s => tex.name.ToLower().Contains(s))) {
                            metal_spec = tex;
                            continue;
                        }
                        break;
                    case 1:
                        if (metallicNames.Any(s => tex.name.ToLower().Contains(s))) {
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

            string textureName = "";

            if (materialName == "") {
                textureName = GetTextureName();
            } else {
                textureName = materialName;
            }

            string path = AssetDatabase.GetAssetPath(textures[0]);
            path = path.Substring(0, path.LastIndexOf("/")) + "/" + textureName + ".mat";

            var materialAsset = AssetDatabase.LoadAssetAtPath(path, typeof(Material)) as Material;

            if (materialAsset != null) {
                FillInTextures(materialAsset);
            } else {
                Material mat = CreateMaterial();
                FillInTextures(mat);
                AssetDatabase.CreateAsset(mat, path);
            }

            void FillInTextures(Material mat) {
                mat.mainTexture = albedo;

                switch (shaderType) {
                    case 0:
                        mat.SetTexture("_SpecGlossMap", metal_spec);
                        break;
                    case 1:
                        mat.SetTexture("_MetallicGlossMap", metal_spec);
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

            Material CreateMaterial() { 
                Material mat = new(LoadShader(shaderType, pipeline));

                //for urp hdrp the specular/metallic needs to be set in the workflow mode
                switch (pipeline) {
                    case MC_Utils.Pipeline.BuiltIn:
                        break;
                    case MC_Utils.Pipeline.URP:
                        if (shaderType == 0)
                            mat.SetFloat("_WorkflowMode", 0);
                            //mat.EnableKeyword("_SPECULAR_SETUP");
                        else {
                            mat.SetFloat("_WorkflowMode", 1);
                            //mat.EnableKeyword("_METALLICGLOSSMAP");
                        }
                        break;
                    case MC_Utils.Pipeline.HDRP:
                        Debug.Log("HDRP Not yet implemented");
                        break;
                    default:
                        break;
                }

                return mat;
            }

            Shader LoadShader(int shaderType, MC_Utils.Pipeline pipeline) {
                switch (pipeline) {
                    case MC_Utils.Pipeline.BuiltIn:
                        switch (shaderType) {
                            case 0:
                                return Shader.Find("Standard (Specular setup)");
                            case 1:
                                return Shader.Find("Standard");
                            default:
                                break;
                        }
                        break;
                    case MC_Utils.Pipeline.URP:
                        return Shader.Find("Universal Render Pipeline/Lit");
                    case MC_Utils.Pipeline.HDRP:
                        return Shader.Find("HDRP/Lit");
                    default:
                        break;
                }

                Debug.LogError("Shader not found");
                return null;
            }

            //get the name of the file by filtering out the texture terms
            string GetTextureName() {
                foreach (var tex in textures) {    
                    string texName = tex.name;

                    if (albedoNames.Any(s => tex.name.ToLower().Contains(s))) {
                        //use albedo name for renaming the file
                        foreach (var albedoName in albedoNames) {
                            texName = texName.Replace(albedoName, "");
                        }

                        foreach (var opacityName in opacityNames) {
                            texName = texName.Replace(opacityName, "");
                        }
                        
                        return texName;
                    }

                    switch (shaderType) {
                        case 0:
                            if (specularNames.Any(s => tex.name.ToLower().Contains(s))) {
                                foreach (var texTerm in specularNames) {
                                    texName = texName.Replace(texTerm, "");
                                }
                                return texName;
                            }
                            break;
                        case 1:
                            if (metallicNames.Any(s => tex.name.ToLower().Contains(s))) {
                                foreach (var texTerm in metallicNames) {
                                    texName = texName.Replace(texTerm, "");
                                }
                                return texName;
                            }
                            break;
                        
                    }

                    if (heightNames.Any(s => tex.name.ToLower().Contains(s))) {
                        foreach (var texTerm in heightNames) {
                            texName = texName.Replace(texTerm, "");
                        }
                        return texName;
                    }

                    if (normalNames.Any(s => tex.name.ToLower().Contains(s))) {
                        foreach (var texTerm in normalNames) {
                            texName = texName.Replace(texTerm, "");
                        }
                        return texName;
                    }

                    if (occlusionNames.Any(s => tex.name.ToLower().Contains(s))) {
                        foreach (var texTerm in occlusionNames) {
                            texName = texName.Replace(texTerm, "");
                        }
                        return texName;
                    }

                    if (emissionNames.Any(s => tex.name.ToLower().Contains(s))) {
                        foreach (var texTerm in emissionNames) {
                            texName = texName.Replace(texTerm, "");
                        }
                        return texName;
                    }

                    if (detailNames.Any(s => tex.name.ToLower().Contains(s))) {
                        foreach (var texTerm in detailNames) {
                            texName = texName.Replace(texTerm, "");
                        }
                        return texName;
                    }
                }

                return "";
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