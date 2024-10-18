using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace C98
{
    public static class WhiteTextureEditor
    {
        private const string BackupWhiteTextureEditor = "BackupWhiteTextureEditor";
        [MenuItem("T70/White-Texture")]
        public static void Slice()
        {
            var obj = Selection.activeObject;
            var targetPath = AssetDatabase.GetAssetPath(obj);
            //Undo.RecordObject(targetPath, BackupWhiteTextureEditor);
            var importer = AssetImporter.GetAtPath(targetPath);
            if (importer is TextureImporter textureImporter)
            {
                var cacheReadAndWrite = textureImporter.isReadable;
                
                var fullPath = Path.Combine(Path.GetDirectoryName(Application.dataPath) ?? "", targetPath);
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(targetPath);
                var bytes = File.ReadAllBytes(fullPath);
                
                textureImporter.isReadable = true;
                textureImporter.SaveAndReimport();
                
                texture = texture.ChangeFormat(TextureFormat.BGRA32);
                var pixels = texture.GetPixels();
                var colorName = Color.clear;
                for (var i = 0; i < pixels.Length; i++)
                {
                    if(pixels[i].a > 0)
                    {
                        var c = pixels[i];
                        colorName = c;
                        c.r = 1;
                        c.g = 1;
                        c.b = 1;
                        pixels[i] = c;
                    }
                }

                //Create backup
                {
                    var fileName = Path.GetFileNameWithoutExtension(fullPath);
                    var c32 = (Color32)colorName;
                    var hex = "#" + c32.r.ToString("X2") + c32.g.ToString("X2") + c32.b.ToString("X2");
                    File.WriteAllBytes(Path.Combine(Path.GetDirectoryName(fullPath) ?? "", fileName+"."+ hex + ".original" + Path.GetExtension(fullPath)), bytes);
                }

                texture.SetPixels(pixels);
                //texture.ChangeFormat(cacheFormat);
                texture.Apply();
                textureImporter.isReadable = cacheReadAndWrite;
                textureImporter.SaveAndReimport();

                if (fullPath.EndsWith(".png")) File.WriteAllBytes(fullPath, texture.EncodeToPNG());
                if (fullPath.EndsWith(".jpg")) File.WriteAllBytes(fullPath, texture.EncodeToJPG());
                if (fullPath.EndsWith(".jpeg")) File.WriteAllBytes(fullPath, texture.EncodeToJPG());
                AssetDatabase.Refresh();
            }
            else
            {
                Debug.LogError("Not a texture");
            }
            AssetDatabase.Refresh();
        }
        public static Texture2D ChangeFormat(this Texture2D oldTexture, TextureFormat newFormat)
        {
            //Create new empty Texture
            Texture2D newTex = new Texture2D(oldTexture.width, oldTexture.height, newFormat, false);
            //Copy old texture pixels into new one
            newTex.SetPixels(oldTexture.GetPixels());
            //Apply
            newTex.Apply();

            return newTex;
        }
    }
}