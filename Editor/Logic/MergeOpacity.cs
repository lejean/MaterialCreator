using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MaterialCreator {
    public class MergeOpacity {
        List<string> albedoNames => MC_Settings.GetOrCreateSettings().albedoNames;
        List<string> opacityNames => MC_Settings.GetOrCreateSettings().opacityNames;
        
        public void Merge(MergeTextureModel mergeModel) {
            try {
                AssetDatabase.StartAssetEditing();

                List<Texture2D> textures = new List<Texture2D>();

                if (MC_Utils.AssetIsDirectory(Selection.activeObject)) {
                    foreach (var asset in Selection.objects) {
                        if (MC_Utils.AssetIsDirectory(asset)) {
                            textures = MC_Utils.CollectTexturesInDirectory(asset);

                            if (textures.Count != 0) {
                                CreateOpacity(textures, mergeModel);
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
                        CreateOpacity(textures, mergeModel);
                    } else {
                        Debug.Log("no textures found in selection");
                    }
                }
            } finally {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.SaveAssets();
            }
        }

        void CreateOpacity(List<Texture2D> textures, MergeTextureModel mergeModel) {
            Texture2D albedo = null;
            Texture2D opacity = null;

            string name = "";
            string albedoPath = "";
            string opacityPath = "";

            foreach (var tex in textures) {
                if (albedoNames.Any(s => tex.name.ToLower().Contains(s))) {
                    name = tex.name;
                    albedoPath = AssetDatabase.GetAssetPath(tex);
                    albedo = MC_Utils.DuplicateTexture(tex);
                    continue;
                }

                if (opacityNames.Any(s => tex.name.ToLower().Contains(s))) {
                    if (string.IsNullOrEmpty(name)) {
                        name = tex.name;
                    }
                    opacityPath = AssetDatabase.GetAssetPath(tex);
                    opacity = MC_Utils.DuplicateTexture(tex);
                    continue;
                }
            }

            string texturePath = AssetDatabase.GetAssetPath(textures[0]);
            texturePath = texturePath.Substring(0, texturePath.LastIndexOf("/"));

            //rename the file with the correct suffix
            if (mergeModel.MaterialName == "") {
                //remove all the names from the albedo list so that you're left with the base name of the object
                foreach (var albedoName in albedoNames) {
                    name = name.Replace(albedoName, "");
                }

                //remove the opacity names from the texture name
                foreach (var opacityName in opacityNames) {
                    name = name.Replace(opacityName, "");
                }
                name += " albedo opacity";
            } else {
                name = mergeModel.MaterialName + " albedo opacity";
            }

            var file = Path.Combine(texturePath, name);

            MC_Utils.MergeAndSave(file, albedo, opacity, false, mergeModel.TextureType);

            if (mergeModel.RemoveOriginal) {
                AssetDatabase.DeleteAsset(albedoPath);
                AssetDatabase.DeleteAsset(opacityPath);
            }
        }
    } 
}
