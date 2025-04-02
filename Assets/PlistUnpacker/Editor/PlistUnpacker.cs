using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Reflection;
using NRatel.TextureUnpacker;

public class PlistUnpacker : MonoBehaviour {
    [MenuItem("Assets/Plist To Atlas", false, 20)]
    public static void MakePlistAtlasToUIAtlas() {
        UnityEngine.Object[] selected = Selection.objects;
        if (selected.Length != 2) {
            Debug.LogError("Select 1 png & 1 txt");
            return;
        }
        string texPath = null;
        string txtPath = null;
        Texture2D bigTexture = null;
        for (int i = 0; i < selected.Length; i++) {
            var item = selected[i];
            string filePath = AssetDatabase.GetAssetPath(item);
            Texture2D tex = item as Texture2D;
            if (tex != null) {
                texPath = filePath;
                bigTexture = tex;
            } else {
                txtPath = filePath;
            }
        }
        Debug.Log("texture: " + texPath + "  plist:" + txtPath);
        //
        TextureImporter textureImporter = AssetImporter.GetAtPath(texPath) as TextureImporter;
        textureImporter.isReadable = true;
        textureImporter.npotScale = TextureImporterNPOTScale.None;
        AssetDatabase.ImportAsset(texPath, ImportAssetOptions.ForceUpdate);
        //
        Plist plist = Loader.LookingForLoader(txtPath).LoadPlist(txtPath);
        int total = plist.frames.Count;
        SpriteMetaData[] sprites = new SpriteMetaData[total];
        int count = 0;
        foreach (var frame in plist.frames) {
            try {
                int sampleHeight = frame.isRotated ? frame.size.width : frame.size.height;
                sprites[count] = new SpriteMetaData();
                if(frame.textureName.Contains('.'))
                    sprites[count].name = frame.textureName.Substring(0, frame.textureName.LastIndexOf('.'));
                else
                    sprites[count].name = frame.textureName;
                sprites[count].border = new Vector4(0, 0, 0, 0);
                sprites[count].pivot = new Vector2(0.5f, 0.5f);
                sprites[count].rect = new Rect(frame.startPos.x,
                                               bigTexture.height - (frame.startPos.y + sampleHeight),
                                               frame.isRotated ? frame.size.height : frame.size.width,
                                               sampleHeight);
                count += 1;
            } catch {
            }
        }
        //
        textureImporter.spritesheet = sprites;
        textureImporter.spriteImportMode = SpriteImportMode.Multiple;
        textureImporter.textureType = TextureImporterType.Sprite;
        textureImporter.SaveAndReimport();
        AssetDatabase.Refresh();
    }

    [MenuItem("Assets/Plist To Pngs(file)", false, 21)]
    public static void DecryptJetportAtlas() {
        UnityEngine.Object[] selected = Selection.objects;
        if (selected.Length != 2) {
            Debug.LogError("Select 1 png & 1 txt");
            return;
        }
        string texPath = null;
        string txtPath = null;
        Texture2D bigTexture = null;
        for (int i = 0; i < selected.Length; i++) {
            var item = selected[i];
            string filePath = AssetDatabase.GetAssetPath(item);
            Debug.Log(filePath);
            Texture2D tex = item as Texture2D;
            if (tex != null) {
                texPath = filePath;
                bigTexture = tex;
            } else {
                txtPath = filePath;
            }
        }
        Debug.Log(texPath + " " + txtPath);
        //
        TextureImporter textureImporter = AssetImporter.GetAtPath(texPath) as TextureImporter;
        textureImporter.isReadable = true;
        textureImporter.npotScale = TextureImporterNPOTScale.None;
        AssetDatabase.ImportAsset(texPath, ImportAssetOptions.ForceUpdate);
        //
        Plist plist = Loader.LookingForLoader(txtPath).LoadPlist(txtPath);
        string outPath = Application.dataPath + "/../Sprites/";
        //
        int total = plist.frames.Count;
        int count = 0;
        foreach (var frame in plist.frames) {
            try {
                SaveSingleSprite(bigTexture, frame, outPath);
                count += 1;
            } catch {
            }
        }
        AssetDatabase.Refresh();
    }

    public static void SaveSingleSprite(Texture2D bigTexture, Frame frame, string outPath) {
        int sampleWidth = frame.size.width;
        int sampleHeight = frame.size.height;
        //旋转时, 采样宽高调换
        if (frame.isRotated) {
            sampleWidth = frame.size.height;
            sampleHeight = frame.size.width;
        }
        Texture2D destTexture = new Texture2D(frame.isRotated ? sampleHeight : sampleWidth, frame.isRotated ? sampleWidth : sampleHeight);
        //起始位置（Y轴需变换，且受旋转影响）。
        int startPosX = frame.startPos.x;
        int startPosY = bigTexture.height - (frame.startPos.y + sampleHeight);
        Debug.Log(frame.textureName + ":  x=" + startPosX + "  y=" + startPosY + "  w="  + sampleWidth + "  h=" + sampleHeight + " " + frame.isRotated);
        Color[] colors = bigTexture.GetPixels(startPosX, startPosY, sampleWidth, sampleHeight);
        //设置像素（将采样像素放到目标图中去）
        for (int w = 0; w < sampleWidth; w++) {
            for (int h = 0; h < sampleHeight; h++) {
                int index = h * sampleWidth + w;
                if (frame.isRotated) {
                    destTexture.SetPixel(sampleHeight - h, w, colors[index]);
                } else {
                    destTexture.SetPixel(w, h, colors[index]);
                }
            }
        }
        destTexture.Apply();
        string outputPath = outPath + "/" + bigTexture.name;
        if (!Directory.Exists(outputPath))
            Directory.CreateDirectory(outputPath);
        //Debug.Log(outputPath + "  " + frame.textureName);
        File.WriteAllBytes(outputPath + "/" + frame.textureName, destTexture.EncodeToPNG());
        DestroyImmediate(destTexture);
        destTexture = null;
    }


}