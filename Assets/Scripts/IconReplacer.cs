using UnityEngine;
using TMPro;

/// <summary>
/// Script để thay thế các emoji icon trong TextMeshPro bằng text hoặc icon Unicode đơn giản
/// </summary>
public static class IconReplacer
{
    /// <summary>
    /// Thay thế tất cả các emoji icon trong text bằng text thông thường
    /// </summary>
    /// <param name="text">Text gốc chứa emoji</param>
    /// <returns>Text đã được thay thế</returns>
    public static string ReplaceEmojis(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        string result = text;

        // Thay thế các emoji cảnh báo
        result = result.Replace("⚠️", "[CẢNH BÁO]");
        result = result.Replace("💀", "[PHÁ SẢN]");
        result = result.Replace("✅", "[THÀNH CÔNG]");
        result = result.Replace("🤖", "[BOT]");
        
        // Thay thế các emoji countdown (chỉ giữ lại [BOT])
        result = result.Replace("⏸️", "");
        result = result.Replace("🎲", "");
        result = result.Replace("⏳", "");
        result = result.Replace("⏰", "");
        result = result.Replace("🎮", "");

        return result;
    }

    /// <summary>
    /// Thay thế emoji bằng icon Unicode đơn giản
    /// </summary>
    /// <param name="text">Text gốc chứa emoji</param>
    /// <returns>Text đã được thay thế</returns>
    public static string ReplaceWithUnicodeIcons(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        string result = text;

        // Thay thế bằng Unicode symbols đơn giản
        result = result.Replace("⚠️", "⚠");  // Unicode warning sign
        result = result.Replace("💀", "☠");  // Unicode skull
        result = result.Replace("✅", "✓");  // Unicode checkmark
        result = result.Replace("🤖", "⚙");  // Unicode gear
        
        // Thay thế các emoji countdown (chỉ giữ lại [BOT])
        result = result.Replace("⏸️", "");
        result = result.Replace("🎲", "");
        result = result.Replace("⏳", "");
        result = result.Replace("⏰", "");
        result = result.Replace("🎮", "");

        return result;
    }

    /// <summary>
    /// Thay thế emoji bằng text mô tả
    /// </summary>
    /// <param name="text">Text gốc chứa emoji</param>
    /// <returns>Text đã được thay thế</returns>
    public static string ReplaceWithDescriptiveText(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        string result = text;

        // Thay thế bằng text mô tả
        result = result.Replace("⚠️", "CẢNH BÁO:");
        result = result.Replace("💀", "PHÁ SẢN:");
        result = result.Replace("✅", "THÀNH CÔNG:");
        result = result.Replace("🤖", "BOT:");
        
        // Thay thế các emoji countdown (chỉ giữ lại [BOT])
        result = result.Replace("⏸️", "");
        result = result.Replace("🎲", "");
        result = result.Replace("⏳", "");
        result = result.Replace("⏰", "");
        result = result.Replace("🎮", "");

        return result;
    }

    /// <summary>
    /// Áp dụng thay thế cho TextMeshPro component
    /// </summary>
    /// <param name="tmpText">TextMeshPro component</param>
    /// <param name="method">Phương thức thay thế (0: Remove, 1: Unicode, 2: Descriptive)</param>
    public static void ApplyToTextMeshPro(TextMeshProUGUI tmpText, int method = 0)
    {
        if (tmpText == null) return;

        string originalText = tmpText.text;
        string newText = "";

        switch (method)
        {
            case 0:
                newText = ReplaceEmojis(originalText);
                break;
            case 1:
                newText = ReplaceWithUnicodeIcons(originalText);
                break;
            case 2:
                newText = ReplaceWithDescriptiveText(originalText);
                break;
            default:
                newText = ReplaceEmojis(originalText);
                break;
        }

        tmpText.text = newText;
    }

    /// <summary>
    /// Áp dụng thay thế cho tất cả TextMeshPro trong scene
    /// </summary>
    /// <param name="method">Phương thức thay thế (0: Remove, 1: Unicode, 2: Descriptive)</param>
    public static void ApplyToAllTextMeshPro(int method = 0)
    {
        TextMeshProUGUI[] allTexts = Object.FindObjectsOfType<TextMeshProUGUI>();
        
        foreach (var text in allTexts)
        {
            ApplyToTextMeshPro(text, method);
        }

        Debug.Log($"Đã thay thế emoji trong {allTexts.Length} TextMeshPro components");
    }
} 