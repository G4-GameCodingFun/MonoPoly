# Hướng Dẫn Thay Thế Icon Emoji

## Vấn Đề
Các emoji icon (⚠️, 💀, ✅, 🤖) có thể gây ra lỗi hiển thị trong TextMeshPro trên một số thiết bị hoặc font không hỗ trợ emoji.

## Giải Pháp
Hệ thống `IconReplacer` đã được tích hợp để tự động thay thế các emoji bằng Unicode icons hoặc text thông thường.

## Các Phương Thức Thay Thế

### 1. Xóa Hoàn Toàn (Mặc định)
- ⚠️ → "[CẢNH BÁO]"
- 💀 → "[PHÁ SẢN]"
- ✅ → "[THÀNH CÔNG]"
- 🤖 → "[BOT]"
- ⏸️ → "" (xóa hoàn toàn)
- 🎲 → "" (xóa hoàn toàn)
- ⏳ → "" (xóa hoàn toàn)
- ⏰ → "" (xóa hoàn toàn)
- 🎮 → "" (xóa hoàn toàn)

### 2. Text Mô Tả
- ⚠️ → "CẢNH BÁO:"
- 💀 → "PHÁ SẢN:"
- ✅ → "THÀNH CÔNG:"
- 🤖 → "BOT:"
- ⏸️ → "" (xóa hoàn toàn)
- 🎲 → "" (xóa hoàn toàn)
- ⏳ → "" (xóa hoàn toàn)
- ⏰ → "" (xóa hoàn toàn)
- 🎮 → "" (xóa hoàn toàn)

### 3. Unicode Icons
- ⚠️ → ⚠ (Unicode warning sign)
- 💀 → ☠ (Unicode skull)
- ✅ → ✓ (Unicode checkmark)
- 🤖 → ⚙ (Unicode gear)
- ⏸️ → "" (xóa hoàn toàn)
- 🎲 → "" (xóa hoàn toàn)
- ⏳ → "" (xóa hoàn toàn)
- ⏰ → "" (xóa hoàn toàn)
- 🎮 → "" (xóa hoàn toàn)

## Cách Sử Dụng

### Tự Động (Khuyến Nghị)
1. Thêm `IconReplacerManager` component vào GameObject trong scene
2. Thiết lập:
   - `Replacement Method`: Chọn phương thức (0=Remove, 1=Unicode, 2=Descriptive)
   - `Auto Apply On Start`: Bật để tự động áp dụng khi game khởi động
   - `Apply To All TextMeshPro`: Bật để áp dụng cho tất cả TextMeshPro

### Thủ Công
```csharp
// Thay thế một text cụ thể
string newText = IconReplacer.ReplaceWithUnicodeIcons("⚠️ Cảnh báo!");

// Áp dụng cho TextMeshPro component
IconReplacer.ApplyToTextMeshPro(textComponent, 1);

// Áp dụng cho tất cả TextMeshPro trong scene
IconReplacer.ApplyToAllTextMeshPro(1);
```

## Các File Đã Được Cập Nhật

### Scripts
- `BankruptcyManager.cs` - Tất cả thông báo phá sản
- `GameManager.cs` - Thông báo cảnh báo và bot
- `PlayerController.cs` - Debug logs
- `DetailsPanelController.cs` - Thông báo mua/bán
- `CardsManager.cs` - Debug logs
- `IncomeTaxTile.cs` - Debug warnings
- `PropertyTaxTile.cs` - Debug warnings

### Files Mới
- `IconReplacer.cs` - Class chính để thay thế icon
- `IconReplacerManager.cs` - Manager để tự động áp dụng

## Test Hệ Thống

### Trong Unity Editor
1. Chọn GameObject có `IconReplacerManager`
2. Right-click → Context Menu → "Test Icon Replacement"
3. Kiểm tra Console để xem kết quả

### Trong Code
```csharp
// Test với text mẫu
string testText = "⚠️ Cảnh báo! 💀 Phá sản! ✅ Thành công! 🤖 Bot! ⏸️ Tạm dừng! 🎲 Dice! ⏳ Chờ! ⏰ Thời gian! 🎮 Game!";
string result = IconReplacer.ReplaceEmojis(testText);
Debug.Log($"Result: {result}");
// Output: "[CẢNH BÁO] Cảnh báo! [PHÁ SẢN] Phá sản! [THÀNH CÔNG] Thành công! [BOT] Bot!  Tạm dừng!  Dice!  Chờ!  Thời gian!  Game!"
```

## Tùy Chỉnh Thêm

### Thêm Emoji Mới
Trong `IconReplacer.cs`, thêm vào các method:

```csharp
// Trong ReplaceEmojis
result = result.Replace("🎮", "[GAME]");  // Thêm emoji mới

// Trong ReplaceWithDescriptiveText
result = result.Replace("🎮", "GAME:");
```

### Thay Đổi Text Thay Thế
```csharp
// Thay đổi text thay thế
result = result.Replace("⚠️", "[WARNING]");  // Thay [CẢNH BÁO] bằng [WARNING]
```

## Lưu Ý

1. **Performance**: Thay thế icon được thực hiện một lần khi text được set, không ảnh hưởng performance
2. **Compatibility**: Text thông thường tương thích tốt hơn với tất cả font và thiết bị
3. **Maintenance**: Dễ dàng thay đổi phương thức thay thế mà không cần sửa code chính
4. **Debug**: Các Debug.Log vẫn giữ nguyên emoji để dễ đọc trong Console

## Troubleshooting

### Icon Không Hiển Thị
1. Kiểm tra `IconReplacerManager` đã được thêm vào scene chưa
2. Đảm bảo `Auto Apply On Start` đã được bật
3. Kiểm tra Console để xem log thông báo

### Lỗi Compile
1. Đảm bảo `IconReplacer.cs` đã được tạo
2. Kiểm tra namespace và using statements
3. Restart Unity nếu cần

### Không Tự Động Áp Dụng
1. Kiểm tra `Auto Apply On Start` đã được bật
2. Đảm bảo `IconReplacerManager` được khởi tạo trước các script khác
3. Kiểm tra Console để xem log thông báo 