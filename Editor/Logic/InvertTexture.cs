using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace MaterialCreator {
    public class InvertTexture {

        public void Invert() {
            if (!(Selection.activeObject is Texture2D)) {
                Debug.LogError("Selected object is not a texture.");
                return;
            }

            try {
                AssetDatabase.StartAssetEditing();

                //get the path of the texture by removing everything after the last backslash
                string assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);

                string folderPath = assetPath.Substring(0, assetPath.LastIndexOf("/"));
                var file = Path.Combine(folderPath, Selection.activeObject.name);

                var texture = Selection.activeObject as Texture2D;

                Texture2D tempTexture = MC_Utils.DuplicateTexture(texture);
                string extension = Path.GetExtension(assetPath);
                InvertAndSave(file, tempTexture, extension);
            } finally {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.SaveAssets();
            }
        }

        void InvertAndSave(string path, Texture2D rgb, string extension) {
            var tex = new Texture2D[] { rgb, rgb, rgb };
            var chn = new int[] { 0, 1, 2 };
            var inv = new bool[] { true, true, true };

            var lut = new float[4, 256];
            var curve = AnimationCurve.Linear(0, 0, 1, 1);
            for (int i = 0; i < 256; i++) {
                var v = Mathf.Clamp01(curve.Evaluate(i / 255f));
                lut[0, i] = v;
                lut[1, i] = v;
                lut[2, i] = v;
                lut[3, i] = v;
            }

            Texture2D image = RemapRGB(tex, chn, inv, lut);
            byte[] data = null;

            switch (extension) {
                case ".png":
                    data = image.EncodeToPNG();
                    path = Path.ChangeExtension(path, ".png");
                    break;
                case ".jpg":
                    data = image.EncodeToJPG();
                    path = Path.ChangeExtension(path, ".jpg");
                    break;
                case ".tga":
                    data = image.EncodeToTGA();
                    path = Path.ChangeExtension(path, ".tga");
                    break;
                default:
                    break;
            }

            File.WriteAllBytes(path, data);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            UnityEngine.Object.DestroyImmediate(image);

            //Debug.Log("texture saved");
        }

        public Texture2D RemapRGB(Texture2D[] tex, int[] chn, bool[] inv, float[,] lut) {
            int w, h;

            w = h = 0;
            for (int i = 0; i < tex.Length; i++)
                if (tex[i] != null) {
                    w = Math.Max(w, tex[i].width);
                    h = Math.Max(h, tex[i].height);
                }

            var rdata = MC_Utils.GetTextureData(tex[0], w, h, Color.black);
            var gdata = tex[1] == tex[0] ? rdata : MC_Utils.GetTextureData(tex[1], w, h, Color.black);
            var bdata = tex[2] == tex[0] ? rdata : tex[2] == tex[1] ? gdata : MC_Utils.GetTextureData(tex[2], w, h, Color.black);

            float r, g, b;
            var image = new Texture2D(w, h, TextureFormat.ARGB32, false, false);
            var data = image.GetPixels();

            for (int i = 0; i < w * h; i++) {
                r = MC_Utils.GetChannelColor(chn[0], rdata[i]);
                g = MC_Utils.GetChannelColor(chn[1], gdata[i]);
                b = MC_Utils.GetChannelColor(chn[2], bdata[i]);

                r = lut[0, (int)(r * 255)];
                g = lut[1, (int)(g * 255)];
                b = lut[2, (int)(b * 255)];

                if (inv[0]) r = 1 - r;
                if (inv[1]) g = 1 - g;
                if (inv[2]) b = 1 - b;
                data[i] = new Color(r, g, b);
            }
            image.SetPixels(data);
            image.Apply();
            return image;
        }
    } 
}
