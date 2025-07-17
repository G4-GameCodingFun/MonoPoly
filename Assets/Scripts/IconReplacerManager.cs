using UnityEngine;
using TMPro;

/// <summary>
/// Manager để tự động thay thế các emoji icon trong TextMeshPro khi game khởi động
/// </summary>
public class IconReplacerManager : MonoBehaviour
{
    [Header("Icon Replacement Settings")]
    [Tooltip("Phương thức thay thế icon: 0=Remove, 1=Unicode, 2=Descriptive")]
    public int replacementMethod = 0;
    
    [Tooltip("Tự động áp dụng khi Start")]
    public bool autoApplyOnStart = true;
    
    [Tooltip("Áp dụng cho tất cả TextMeshPro trong scene")]
    public bool applyToAllTextMeshPro = true;
    
    [Header("Manual Application")]
    [Tooltip("Danh sách TextMeshPro components cần áp dụng thay thế")]
    public TextMeshProUGUI[] specificTextComponents;

    private void Start()
    {
        if (autoApplyOnStart)
        {
            ApplyIconReplacement();
        }
    }

    /// <summary>
    /// Áp dụng thay thế icon cho tất cả TextMeshPro
    /// </summary>
    [ContextMenu("Apply Icon Replacement")]
    public void ApplyIconReplacement()
    {
        Debug.Log("🔄 Bắt đầu thay thế emoji icon...");
        
        if (applyToAllTextMeshPro)
        {
            IconReplacer.ApplyToAllTextMeshPro(replacementMethod);
        }
        
        if (specificTextComponents != null && specificTextComponents.Length > 0)
        {
            foreach (var textComponent in specificTextComponents)
            {
                if (textComponent != null)
                {
                    IconReplacer.ApplyToTextMeshPro(textComponent, replacementMethod);
                }
            }
        }
        
        Debug.Log($"✅ Đã hoàn thành thay thế emoji icon (Phương thức: {GetMethodName(replacementMethod)})");
    }

    /// <summary>
    /// Áp dụng thay thế cho một TextMeshPro cụ thể
    /// </summary>
    /// <param name="textComponent">TextMeshPro component</param>
    public void ApplyToSpecificText(TextMeshProUGUI textComponent)
    {
        if (textComponent != null)
        {
            IconReplacer.ApplyToTextMeshPro(textComponent, replacementMethod);
            Debug.Log($"✅ Đã áp dụng thay thế cho {textComponent.name}");
        }
    }

    /// <summary>
    /// Thay đổi phương thức thay thế và áp dụng lại
    /// </summary>
    /// <param name="method">Phương thức mới (0: Remove, 1: Unicode, 2: Descriptive)</param>
    public void ChangeReplacementMethod(int method)
    {
        replacementMethod = Mathf.Clamp(method, 0, 2);
        ApplyIconReplacement();
    }

    /// <summary>
    /// Lấy tên phương thức thay thế
    /// </summary>
    /// <param name="method">Số phương thức</param>
    /// <returns>Tên phương thức</returns>
    private string GetMethodName(int method)
    {
        switch (method)
        {
            case 0: return "Remove Emojis";
            case 1: return "Unicode Icons";
            case 2: return "Descriptive Text";
            default: return "Unknown";
        }
    }

    /// <summary>
    /// Test thay thế với text mẫu
    /// </summary>
    [ContextMenu("Test Icon Replacement")]
    public void TestIconReplacement()
    {
        string testText = "⚠️ Cảnh báo! 💀 Phá sản! ✅ Thành công! 🤖 Bot! ⏸️ Tạm dừng! 🎲 Dice! ⏳ Chờ! ⏰ Thời gian! 🎮 Game!";
        string result = "";
        
        switch (replacementMethod)
        {
            case 0:
                result = IconReplacer.ReplaceEmojis(testText);
                break;
            case 1:
                result = IconReplacer.ReplaceWithUnicodeIcons(testText);
                break;
            case 2:
                result = IconReplacer.ReplaceWithDescriptiveText(testText);
                break;
        }
        
        Debug.Log($"🧪 Test Icon Replacement:");
        Debug.Log($"Original: {testText}");
        Debug.Log($"Result: {result}");
    }
} 