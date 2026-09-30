using UnityEngine;
public sealed class HeroPortrait : MonoBehaviour
{
    private static int sequence;
    private GameObject model;
    private Camera cameraRig;
    private RenderTexture texture;
    private ItemData[] items;
    public void Configure(GameObject prefab,InventoryState profile,ItemData[] definitions)
    {
        items=definitions;float x=2000+(++sequence)*10;model=Instantiate(prefab);model.transform.position=new Vector3(x,1000,0);
        cameraRig=new GameObject("HeroPortraitCamera").AddComponent<Camera>();cameraRig.orthographic=true;cameraRig.orthographicSize=1.6f;cameraRig.transform.position=new Vector3(x,1001,-10);cameraRig.cullingMask=1<<30;cameraRig.clearFlags=CameraClearFlags.SolidColor;cameraRig.backgroundColor=new Color32(20,28,42,255);cameraRig.enabled=false;
        texture=new RenderTexture(256,256,16);texture.filterMode=FilterMode.Point;texture.Create();cameraRig.targetTexture=texture;
        var image=gameObject.AddComponent<UnityEngine.UI.RawImage>();image.texture=texture;image.raycastTarget=false;Apply(profile);
    }
    public void Apply(InventoryState profile)
    {
        if(model==null)return;var custom=model.GetComponent<CharacterCustomizer>();custom.PresentationScale=1;custom.Apply(profile.Appearance);int best=1;for(int i=0;i<profile.EquippedTiers.Length;i++){int enhancement=profile.EquippedEnhancementLevels!=null&&i<profile.EquippedEnhancementLevels.Length?profile.EquippedEnhancementLevels[i]:profile.EquippedTiers[i];best=Mathf.Max(best,enhancement);}custom.SetEquipmentTier(best);
        var weapon=System.Array.Find(items,x=>x.Id==profile.EquippedIds[0]);int weaponEnhancement=profile.EquippedEnhancementLevels!=null&&profile.EquippedEnhancementLevels.Length>0?profile.EquippedEnhancementLevels[0]:profile.EquippedTiers[0];custom.SetWeapon(weapon!=null?weapon.Family:"sword",weaponEnhancement);
        foreach(Transform child in model.GetComponentsInChildren<Transform>(true))child.gameObject.layer=30;
        model.GetComponentInChildren<WeaponTrailVFX>().Demonstrate=true;cameraRig.Render();
    }
    private float nextPreview;
    private void LateUpdate(){if(cameraRig!=null&&isActiveAndEnabled&&Time.unscaledTime>=nextPreview){nextPreview=Time.unscaledTime+1f/24;cameraRig.Render();}}
    private void OnDestroy(){if(model!=null)Destroy(model);if(cameraRig!=null)Destroy(cameraRig.gameObject);if(texture!=null){texture.Release();Destroy(texture);}}
}
