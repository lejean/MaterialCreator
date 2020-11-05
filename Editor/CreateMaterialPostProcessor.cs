using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MaterialCreator {
    public class CreateMaterialPostProcessor : AssetPostprocessor {

        //void OnPostprocessTexture(Texture2D texture) {
        //    Debug.Log("previous path "+ CreateMaterials.specMetalPath);
        //    Debug.Log("imported path "+ assetPath);

        //    // Only post process textures if they are in a folder
        //    // "invert color" or a sub folder of it.
        //    //string lowerCaseAssetPath = assetPath.ToLower();
        //    //if (lowerCaseAssetPath.IndexOf("/invert color/") == -1)
        //    //    return;

        //    //for (int m = 0; m < texture.mipmapCount; m++) {
        //    //    Color[] c = texture.GetPixels(m);

        //    //    for (int i = 0; i < c.Length; i++) {
        //    //        c[i].r = 1 - c[i].r;
        //    //        c[i].g = 1 - c[i].g;
        //    //        c[i].b = 1 - c[i].b;
        //    //    }
        //    //    texture.SetPixels(c, m);
        //    //}
        //    // Instead of setting pixels for each mip map levels, you can also
        //    // modify only the pixels in the highest mip level. And then simply use
        //    // texture.Apply(true); to generate lower mip levels.
        //}

        //static void OnWillCreateAsset(string assetName) {
        //    Debug.Log("OnWillCreateAsset is being called with the following asset: " + assetName + ".");
        //}

        //static string[] OnWillSaveAssets(string[] paths) {
        //    Debug.Log("OnWillSaveAssets");
        //    foreach (string path in paths)
        //        Debug.Log(path);
        //    return paths;
        //}

    } 
}