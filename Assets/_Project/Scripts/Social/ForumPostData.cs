using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class ForumPostData : ISerializationCallbackReceiver
{
    public string AuthorId;
    public string Title;
    public string Markdown;
    [SerializeField] private List<string> imageAttachments=new List<string>();
    public IReadOnlyList<string> ImageAttachments=>imageAttachments.AsReadOnly();
    public bool TryAddImage(string reference)
    {
        if(string.IsNullOrWhiteSpace(reference)||imageAttachments.Count>=5) return false;
        imageAttachments.Add(reference); return true;
    }
    public void OnBeforeSerialize()=>Clamp();
    public void OnAfterDeserialize()=>Clamp();
    private void Clamp()
    {
        imageAttachments ??= new List<string>();
        if(imageAttachments.Count>5) imageAttachments.RemoveRange(5,imageAttachments.Count-5);
    }
}
