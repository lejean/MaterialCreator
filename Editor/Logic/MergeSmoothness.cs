using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MaterialCreator {
    public class MergeSmoothness {
        
        List<string> metallicNames => MC_Settings.GetOrCreateSettings().metallicNames;
        List<string> specularNames => MC_Settings.GetOrCreateSettings().specularNames;
        List<string> smoothnessNames => MC_Settings.GetOrCreateSettings().smoothnessNames;
        
        public void Merge(MergeTextureModel mergeModel, bool invertRoughness) {
            try {
                AssetDatabase.StartAssetEditing();

                List<Texture2D> textures = new List<Texture2D>();

                if (MC_Utils.AssetIsDirectory(Selection.activeObject)) {
                    foreach (var asset in Selection.objects) {
                        if (MC_Utils.AssetIsDirectory(asset)) {
                            textures = MC_Utils.CollectTexturesInDirectory(asset);

                            if (textures.Count != 0) {
                                CreateSmoothness(textures, mergeModel, invertRoughness);
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
                        CreateSmoothness(textures, mergeModel, invertRoughness);
                    } else {
                        Debug.Log("no textures found in selection");
                    }
                }
            } finally {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.SaveAssets();
            }
        }

        void CreateSmoothness(List<Texture2D> textures, MergeTextureModel mergeModel, bool invertRoughness) {
            Texture2D metal_spec = null;
            Texture2D smoothness = null;

            string textureName = "";
            string metalSpecPath = "";
            string glossPath = "";

            foreach (var tex in textures) {
                switch (mergeModel.ShaderType) {
                    case 0:
                        if (specularNames.Any(s => tex.name.ToLower().Contains(s))) {
                            textureName = tex.name;
                            metalSpecPath = AssetDatabase.GetAssetPath(tex);
                            metal_spec = MC_Utils.DuplicateTexture(tex);
                            continue;
                        }
                        break;
                    case 1:
                        if (metallicNames.Any(s => tex.name.ToLower().Contains(s))) {
                            textureName = tex.name;
                            metalSpecPath = AssetDatabase.GetAssetPath(tex);
                            metal_spec = MC_Utils.DuplicateTexture(tex);
                            continue;
                        }
                        break;                    
                }

                if (smoothnessNames.Any(s => tex.name.ToLower().Contains(s))) {
                    if (string.IsNullOrEmpty(textureName)) {
                        textureName = tex.name;
                    }
                    glossPath = AssetDatabase.GetAssetPath(tex);
                    smoothness = MC_Utils.DuplicateTexture(tex);
                    continue;
                }
            }

            //rename the file with the correct suffix
            switch (mergeModel.ShaderType) {
                case 0:
                    if (mergeModel.MaterialName == "") {
                        //remove the specular names from the texture name
                        foreach (var specularName in specularNames) {
                            textureName = textureName.Replace(specularName, "");
                        }

                        //remove the smoothness names from the texture name
                        foreach (var smoothnessName in smoothnessNames) {
                            textureName = textureName.Replace(smoothnessName, "");
                        }

                        textureName += " spec smoothness";
                    } else {
                        textureName = mergeModel.MaterialName + " spec smoothness";
                    }
                    break;
                case 1:
                    if (mergeModel.MaterialName == "") {
                        //remove the metallic names from the texture name
                        foreach (var metalName in metallicNames) {
                            textureName = textureName.Replace(metalName, "");
                        }

                        //remove the smoothness names from the texture name
                        foreach (var smoothnessName in smoothnessNames) {
                            textureName = textureName.Replace(smoothnessName, "");
                        }

                        textureName += " metal smoothness";
                    } else {
                        textureName = mergeModel.MaterialName + " metal smoothness";
                    }
                    break;
                default:
                    break;
            }

            //get the path of the texture by removing everything after the last backslash
            string texturePath = AssetDatabase.GetAssetPath(textures.ElementAt(0));
            texturePath = texturePath.Substring(0, texturePath.LastIndexOf("/"));
            var file = Path.Combine(texturePath, textureName);

            Debug.Log("SaveToFile");
            MC_Utils.MergeAndSave(file, metal_spec, smoothness, invertRoughness, mergeModel.TextureType);

            if (mergeModel.RemoveOriginal) {
                AssetDatabase.DeleteAsset(metalSpecPath);
                AssetDatabase.DeleteAsset(glossPath);
            }

            //Metal-spec map doesn't get assigned to the material after creation, need to use asset postprocessor to do everything in 1 click
            //CreateMaterial();
        }
    } 
}
