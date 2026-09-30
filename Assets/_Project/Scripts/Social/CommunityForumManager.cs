using System.Text;
using TMPro;
using UnityEngine;

public sealed class CommunityForumManager : MonoBehaviour
{
    public void RenderPost(ForumPostData post,TMP_Text target)
    {
        target.richText=false;
        target.text=post.Title+"\nby "+post.AuthorId+"\n\n"+PlainMarkdown(post.Markdown)+$"\n\nAttachments: {post.ImageAttachments.Count}/5";
    }
    public void RenderLoadout(InventoryState inventory,ItemData[] definitions,TMP_Text target)
    {
        var text=new StringBuilder("WAYFARER LOADOUT\n");
        for(int i=0;i<3;i++) { var item=System.Array.Find(definitions,x=>x.Id==inventory.EquippedIds[i]);int enhancement=inventory.EquippedEnhancementLevels!=null&&i<inventory.EquippedEnhancementLevels.Length?inventory.EquippedEnhancementLevels[i]:inventory.EquippedTiers[i]; text.AppendLine($"{(EquipmentSlot)i}: {(item!=null?item.DisplayName:"None")} · +{enhancement}"); }
        target.richText=false; target.text=text.ToString();
    }
    public void RenderWiki(string markdown,TMP_Text target) { target.richText=false; target.text=PlainMarkdown(markdown); }
    public static string PlainMarkdown(string markdown)
    {
        if(string.IsNullOrEmpty(markdown))return "";
        var lines=markdown.Replace("\r","").Split('\n');
        for(int i=0;i<lines.Length;i++) lines[i]=lines[i].TrimStart('#',' ').Replace("**","").Replace("`","");
        return string.Join("\n",lines);
    }
}
