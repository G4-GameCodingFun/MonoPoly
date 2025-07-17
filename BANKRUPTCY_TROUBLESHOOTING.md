# Hướng Dẫn Khắc Phục Sự Cố Hệ Thống Phá Sản

## 🔍 **Kiểm tra BankruptcyManager**

### **1. Đảm bảo BankruptcyManager được tạo:**
1. Trong Unity, tạo GameObject mới tên "BankruptcyManager"
2. Thêm component `BankruptcyManager` vào GameObject
3. Đảm bảo GameObject này **KHÔNG** bị destroy khi chuyển scene
4. Kiểm tra Console để thấy thông báo: "✓ BankruptcyManager đã được khởi tạo thành công!"

### **2. Kiểm tra Instance:**
- Nếu thấy warning: "⚠️ BankruptcyManager.Instance là null!"
- Có nghĩa là BankruptcyManager chưa được khởi tạo hoặc bị destroy

## 🎮 **Test Hệ Thống Phá Sản**

### **Test cho User:**
1. Chạy game với tiền 200$
2. Mua một số tài sản
3. Đi vào ô thuế hoặc trả tiền thuê để bị trừ tiền
4. Khi tiền < 0, panel phá sản sẽ hiện
5. Chọn tài sản và bán để thoát khỏi phá sản

### **Test cho Bot:**
1. Chạy game với tiền 200$
2. Để bot mua một số tài sản
3. Bot đi vào ô thuế hoặc trả tiền thuê
4. Khi bot tiền < 0, bot sẽ tự động bán tài sản
5. Nếu hết tài sản, bot sẽ bị game over

## 🐛 **Các Lỗi Thường Gặp**

### **Lỗi 1: BankruptcyManager.Instance là null**
**Nguyên nhân:** BankruptcyManager chưa được tạo hoặc bị destroy
**Giải pháp:**
- Tạo BankruptcyManager GameObject
- Đảm bảo không bị destroy
- Kiểm tra thứ tự khởi tạo

### **Lỗi 2: Bot không tự động bán tài sản**
**Nguyên nhân:** Logic HandleBotBankruptcy không được gọi
**Giải pháp:**
- Kiểm tra `player.isBot` có đúng không
- Đảm bảo `CheckBankruptcy()` được gọi
- Kiểm tra Console để thấy thông báo bot

### **Lỗi 3: Game không kết thúc khi bot phá sản**
**Nguyên nhân:** Logic HandleGameOver không hoạt động
**Giải pháp:**
- Kiểm tra `GameManager.Instance` có null không
- Đảm bảo player được remove khỏi danh sách
- Kiểm tra `currentPlayerIndex` có được cập nhật không

## 🔧 **Debug Commands**

### **Thêm vào Console để test:**
```csharp
// Test phá sản cho player hiện tại
if (GameManager.Instance != null && GameManager.Instance.players.Count > 0)
{
    var player = GameManager.Instance.players[GameManager.Instance.currentPlayerIndex];
    player.money = -100; // Set tiền âm
    BankruptcyManager.Instance.CheckBankruptcy(player);
}
```

### **Kiểm tra trạng thái:**
```csharp
// Kiểm tra BankruptcyManager
Debug.Log($"BankruptcyManager.Instance: {BankruptcyManager.Instance != null}");

// Kiểm tra player hiện tại
if (GameManager.Instance != null)
{
    var player = GameManager.Instance.players[GameManager.Instance.currentPlayerIndex];
    Debug.Log($"Player: {player.playerName}, Money: {player.money}, IsBot: {player.isBot}");
}
```

## 📋 **Checklist Thiết Lập**

- [ ] BankruptcyManager GameObject được tạo
- [ ] Component BankruptcyManager được thêm
- [ ] GameObject không bị destroy khi chuyển scene
- [ ] Console hiển thị "✓ BankruptcyManager đã được khởi tạo thành công!"
- [ ] UI Panel được thiết lập đúng
- [ ] Tất cả references được gán trong Inspector
- [ ] Tiền mặc định được set = 200$

## 🎯 **Kết Quả Mong Đợi**

### **Khi User phá sản:**
1. Panel phá sản hiện với animation
2. Dropdown hiển thị danh sách tài sản
3. User chọn và bán tài sản
4. Panel đóng sau khi đủ tiền

### **Khi Bot phá sản:**
1. Thông báo: "🤖 [BotName] đang xử lý phá sản..."
2. Bot tự động bán tài sản đắt nhất trước
3. Thông báo: "🤖 [BotName] đã bán [PropertyName] để trả nợ"
4. Nếu đủ tiền: "🤖 [BotName] đã thoát khỏi tình trạng phá sản!"
5. Nếu hết tài sản: "🎮 GAME OVER - [BotName] đã phá sản!"

### **Khi Game kết thúc:**
1. Nếu còn 1 player: "🏆 CHIẾN THẮNG! [WinnerName] là người chiến thắng!"
2. Nếu không còn ai: "🎮 Tất cả người chơi đã phá sản! Game kết thúc." 