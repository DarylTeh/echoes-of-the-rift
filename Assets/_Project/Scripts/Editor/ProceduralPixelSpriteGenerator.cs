using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>Deterministic, aligned layers. Palette markers: red=skin, green=hair, blue=cloth.</summary>
public static class ProceduralPixelSpriteGenerator
{
    private const string Output = "Assets/_Project/Art/Sprites/Generated";
    private static readonly Color32 Clear = new Color32(0, 0, 0, 0);
    private static readonly Dictionary<char, Color32> Palette = new Dictionary<char, Color32>
    {
        { '.', Clear }, { '#', new Color32(30, 24, 44, 255) },
        { 's', new Color32(255, 0, 0, 255) }, { 'h', new Color32(0, 255, 0, 255) },
        { 'c', new Color32(0, 0, 255, 255) }, { 'e', new Color32(111, 210, 126, 255) },
        { 'w', new Color32(244, 233, 200, 255) }
    };

    [MenuItem("Cookie Raid/Generate Pixel Assets")]
    public static void GenerateAllPixelAssetsCLI()
    {
        Directory.CreateDirectory(Output);
        EditorSettings.defaultBehaviorMode = EditorBehaviorMode.Mode2D;
        var templates = Templates();
        int count = 0;
        foreach (var template in templates)
        {
            foreach (int size in new[] { 16, 32 })
            {
                WriteSprite(template.Key, template.Value, size);
                count++;
            }
        }
        GenerateRaceSheetsCLI();
        AssetDatabase.SaveAssets();
        Debug.Log($"PIXEL_ASSETS_OK: Generated and verified {count} sprites.");
    }

    public static void GenerateRaceSheetsCLI()
    {
        string folder="Assets/_Project/Art/RaceSheets";Directory.CreateDirectory(folder);
        string[] layers={"body","head","ears","hair","horns","tail","armour","weapon"};
        for(int race=0;race<9;race++)
        {
            var sheet=new Texture2D(256,96,TextureFormat.RGBA32,false);sheet.SetPixels(new Color[256*96]);
            for(int frame=0;frame<3;frame++)for(int layer=0;layer<8;layer++)
            {
                var sprite=layer==7?PixelArt.Icon("sword",0,frame+3):PixelArt.Character(layers[layer],race,frame,new Color32(225,177,143,255),new Color32(113,72,76,255),Color.cyan);
                for(int y=0;y<32;y++)for(int x=0;x<32;x++)sheet.SetPixel(layer*32+x,frame*32+y,sprite.texture.GetPixel(x,y));
            }
            sheet.Apply();string path=folder+"/race-"+race+"-layers.png";File.WriteAllBytes(path,sheet.EncodeToPNG());UnityEngine.Object.DestroyImmediate(sheet);AssetDatabase.ImportAsset(path);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();
        }
        Debug.Log("RACE_SHEETS_OK: nine sheets, eight aligned 32px layers, three hair/weapon variants.");
    }
    private static void WriteSprite(string name, string[] rows, int size)
    {
        if (rows.Length != 16) throw new InvalidOperationException(name + " must have 16 rows.");
        int scale = size / 16;
        var pixels = new Color32[size * size];
        for (int y = 0; y < 16; y++)
        {
            if (rows[y].Length != 16) throw new InvalidOperationException(name + " has a malformed row.");
            for (int x = 0; x < 16; x++)
                for (int dy = 0; dy < scale; dy++)
                    for (int dx = 0; dx < scale; dx++)
                        pixels[((15 - y) * scale + dy) * size + x * scale + dx] = Palette[rows[y][x]];
        }
        string path = $"{Output}/{name}_{size}.png";
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        try
        {
            texture.SetPixels32(pixels);
            texture.Apply();
            byte[] png = texture.EncodeToPNG();
            // Preserve .meta GUIDs and avoid reimporting identical pixel data.
            if (!File.Exists(path) || !SameBytes(File.ReadAllBytes(path), png)) File.WriteAllBytes(path, png);
        }
        finally { UnityEngine.Object.DestroyImmediate(texture); }
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = size;
        importer.filterMode = FilterMode.Point;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.npotScale = TextureImporterNPOTScale.None;
        var settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteAlignment = (int)SpriteAlignment.BottomCenter;
        settings.spritePivot = new Vector2(0.5f, 0f);
        settings.spriteMeshType = SpriteMeshType.FullRect;
        importer.SetTextureSettings(settings);
        importer.SaveAndReimport();
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite == null || sprite.rect.width != size || sprite.rect.height != size)
            throw new InvalidOperationException("Sprite import verification failed: " + path);
    }

    private static bool SameBytes(byte[] a, byte[] b)
    {
        if (a.Length != b.Length) return false;
        for (int i = 0; i < a.Length; i++) if (a[i] != b[i]) return false;
        return true;
    }

    private static Dictionary<string, string[]> Templates()
    {
        return new Dictionary<string, string[]>
        {
            { "body", new[] {
                "................", "......####......", ".....#ssss#.....", ".....#ssss#.....",
                ".....#s##s#.....", "......#ss#......", ".....#ssss#.....", "....#ssssss#....",
                "....#ssssss#....", "....#ssssss#....", ".....#ssss#.....", ".....#ssss#.....",
                ".....#s##s#.....", ".....#s##s#.....", ".....##..##.....", "................" } },
            { "hair_short", new[] {
                "................", "......####......", ".....#hhhh#.....", ".....#hhhhh#....",
                ".....#h..h#.....", "................", "................", "................",
                "................", "................", "................", "................",
                "................", "................", "................", "................" } },
            { "hair_long", new[] {
                "................", "......####......", ".....#hhhh#.....", "....#hhhhhh#....",
                "....#h....h#....", "....#h....h#....", "....#h....h#....", "....##....##....",
                "................", "................", "................", "................",
                "................", "................", "................", "................" } },
            { "hair_spiked", new[] {
                "......#.#.......", ".....#h#h#......", ".....#hhhh#.....", ".....#hhhhh#....",
                ".....#h..h#.....", "................", "................", "................",
                "................", "................", "................", "................",
                "................", "................", "................", "................" } },
            { "starter_tunic", new[] {
                "................", "................", "................", "................",
                "................", "................", ".....#c..c#.....", "....#cccccc#....",
                "....#cccccc#....", ".....#cccc#.....", ".....######.....", ".....#cccc#.....",
                "................", "................", "................", "................" } },
            { "starter_robe", new[] {
                "................", "................", "................", "................",
                "................", "................", ".....#c..c#.....", "....#cccccc#....",
                "....#cccccc#....", ".....#cccc#.....", ".....######.....", ".....#cccc#.....",
                "....#cccccc#....", "....########....", "................", "................" } },
            { "enemy_slime", new[] {
                "................", "................", "................", "................",
                "................", "................", "......####......", ".....#eeee#.....",
                "....#eeeeee#....", "...#eweeewee#...", "...#e#eee#ee#...", "..#eeeeeeeeee#..",
                "..#eeeeeeeeee#..", "...##########...", "................", "................" } },
            { "enemy_skeleton", new[] {
                "................", "......####......", ".....#wwww#.....", ".....#w##w#.....",
                ".....#wwww#.....", "......####......", ".......ww.......", "....###ww###....",
                "....w.#ww#.w....", "....w.####.w....", "......#ww#......", "......####......",
                "......w..w......", ".....ww..ww.....", "................", "................" } }
        };
    }
}
