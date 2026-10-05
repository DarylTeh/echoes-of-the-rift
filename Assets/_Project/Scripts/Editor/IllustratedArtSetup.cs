using UnityEditor;
using UnityEngine;
public static class IllustratedArtSetup
{
 public static void BuildCLI()
 {
  foreach(string name in new[]{"Equipment","Heroes","HeroSide","HeroBack","HeroFrontWalk","HeroSideWalk","HeroBackWalk","Expansion","Monsters","DungeonTiles","UtilityIcons","CombatEffects","BossAttacks"})
  {
   string path="Assets/_Project/Resources/Illustrated/"+name+".png";
   AssetDatabase.ImportAsset(path);
   var importer=(TextureImporter)AssetImporter.GetAtPath(path);
   importer.textureType=TextureImporterType.Default;importer.filterMode=FilterMode.Point;
   importer.mipmapEnabled=false;importer.alphaIsTransparency=true;importer.wrapMode=TextureWrapMode.Clamp;
   importer.textureCompression=TextureImporterCompression.Uncompressed;importer.npotScale=TextureImporterNPOTScale.None;
   importer.maxTextureSize=2048;importer.isReadable=false;importer.SaveAndReimport();
  }
  ProjectValidation.EnsureRuntimeShaders();Debug.Log("ILLUSTRATED_ART_OK: shared atlases; point sampling; original PNGs preserved.");
 }
}
