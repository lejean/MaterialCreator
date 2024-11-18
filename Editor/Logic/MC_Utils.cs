using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MaterialCreator {
    public class MC_Utils {

        //Check if the selected object is a directory
        public static bool AssetIsDirectory(UnityEngine.Object selection) {
            string filePath = AssetDatabase.GetAssetPath(selection);
            FileAttributes attr = File.GetAttributes(filePath);

            //check if the selected object is a folder
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory) {
                return true;
            } else {
                return false;
            }
        }

        
        //Return a list of textures from a directory
        public static List<Texture2D> CollectTexturesInDirectory(UnityEngine.Object directory) {
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

        /// <summary>
        /// If the texture is not read/write enabled, we duplicate the texture to a rendertexture and use that
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static Texture2D DuplicateTexture(Texture2D source) {
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

        //Filter all textures from your selection if it's not a directory
        public static List<Texture2D> FilterTextures() {
            List<Texture2D> textures;

            var filteredSelection = Selection.GetFiltered(typeof(Texture2D), SelectionMode.Assets).Cast<Texture2D>();
            textures = filteredSelection.ToList();

            return textures;
        }

        //create the texture and save it to the given path
        public static void MergeAndSave(string path, Texture2D rgb, Texture2D alpha, bool invertAlpha, int textureType) {
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
                    image = RemapRGBA(tex, chn, inv, lut);
                    data = image.EncodeToPNG();
                    //var data = image.EncodeToTGA();
                    break;
                case 1:
                    //tga
                    path = Path.ChangeExtension(path, ".tga");
                    image = RemapRGBA(tex, chn, inv, lut);
                    data = image.EncodeToTGA();
                    break;
                default:
                    break;
            }

            File.WriteAllBytes(path, data);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            UnityEngine.Object.DestroyImmediate(image);

            //Debug.Log("texture saved");
        }
            
        
         static Texture2D RemapRGBA(Texture2D[] tex, int[] chn, bool[] inv, float[,] lut) {
            int w, h;

            w = h = 0;
            for (int i = 0; i < tex.Length; i++)
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

         public static float GetChannelColor(int chn, Color color) {
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

        public static Color[] GetTextureData(Texture2D tex, int width, int height, Color fill) {
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
        
    } 
}
