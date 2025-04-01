using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;


[ExecuteInEditMode]
public class AtlasUnpacker : MonoBehaviour {

    [MenuItem("Assets/Atlas To Pngs(file)", false, 22)]
    public static void ExportSprites() {
        UnityEngine.Object[] selected = Selection.objects;
        if (selected.Length != 1) {
            Debug.LogError("Select 1 png");
            return;
        }
        string texPath = AssetDatabase.GetAssetPath(selected[0]);
        Texture2D texture = selected[0] as Texture2D;
        TextureImporter textureImporter = AssetImporter.GetAtPath(texPath) as TextureImporter;
        SpriteMetaData[] spriteLists = textureImporter.spritesheet;
        string outPath = texPath.Substring(0, texPath.LastIndexOf('/')).Replace("Assets/", "");
        for (int i = 0; i < spriteLists.Length; i++)
            SavePriteToPNG_Meta(texture, spriteLists[i], Application.dataPath + "/" + outPath);
        AssetDatabase.Refresh();
        Debug.Log("done: " + Application.dataPath + "/" + outPath);
    }

    static bool SavePriteToPNG_Meta(Texture2D sourceImg, SpriteMetaData metaData, string outPath) {
        if (sourceImg == null)
            return false;
        int ww = (int)metaData.rect.width;
        int hh = (int)metaData.rect.height;
        Texture2D myImage = new Texture2D(ww, hh, sourceImg.format, false);
        myImage.SetPixels(sourceImg.GetPixels((int)metaData.rect.x, (int)metaData.rect.y, ww, hh));
        myImage.Apply();
        string outputPath = outPath + "/" + sourceImg.name;
        if (!Directory.Exists(outputPath))
            Directory.CreateDirectory(outputPath);
        File.WriteAllBytes(outputPath + "/" + metaData.name, myImage.EncodeToPNG());
        return true;
    }
}