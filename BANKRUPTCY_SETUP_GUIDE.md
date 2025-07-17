# Hướng Dẫn Thiết Lập Hệ Thống Phá Sản

## 1. Tạo BankruptcyManager GameObject

1. Trong Unity, tạo một GameObject mới và đặt tên là "BankruptcyManager"
2. Thêm component `BankruptcyManager` vào GameObject này
3. Đảm bảo GameObject này không bị destroy khi chuyển scene

## 2. Tạo UI Panel Phá Sản

### 2.1 Tạo Panel chính
1. Tạo UI Panel mới: Right-click trong Hierarchy → UI → Panel
2. Đặt tên là "BankruptcyPanel"
3. Thiết lập:
   - Anchor: Center
   - Size: 600x400
   - Background: Màu đen với alpha 0.8
4. **Thêm CanvasGroup** (tự động thêm bởi script):
   - Script sẽ tự động thêm CanvasGroup component
   - CanvasGroup giúp animation fade in/out mượt mà

### 2.2 Thêm các thành phần UI

#### Title Text
1. Tạo TextMeshPro - Text (UI) con của BankruptcyPanel
2. Đặt tên "TitleText"
3. Thiết lập:
   - Text: "⚠️ PHÁ SẢN"
   - Font Size: 24
   - Color: Đỏ
   - Alignment: Center
   - Position: Top center

#### Message Text
1. Tạo TextMeshPro - Text (UI) con của BankruptcyPanel
2. Đặt tên "MessageText"
3. Thiết lập:
   - Text: "Bạn đang thiếu tiền! Hãy bán bớt tài sản."
   - Font Size: 16
   - Color: Trắng
   - Alignment: Center
   - Position: Dưới TitleText

#### Properties Dropdown
1. Tạo Dropdown (TMP) con của BankruptcyPanel
2. Đặt tên "PropertiesDropdown"
3. Thiết lập:
   - Size: 500x50
   - Position: Giữa panel
   - Background: Màu trắng
   - Font Size: 14
   - Placeholder: "Chọn tài sản..."

#### Buttons
1. Tạo Button "SellButton":
   - Text: "BÁN TÀI SẢN"
   - Color: Đỏ
   - Position: Bottom left

2. Tạo Button "CancelButton":
   - Text: "HỦY"
   - Color: Xám
   - Position: Bottom right

## 3. Tùy Chỉnh Dropdown (Tùy chọn)

### 3.1 Tùy chỉnh giao diện Dropdown
1. Chọn PropertiesDropdown trong Inspector
2. Tùy chỉnh:
   - **Template**: Có thể thay đổi template để có giao diện đẹp hơn
   - **Item Text**: Font size, color cho text trong dropdown
   - **Caption Text**: Font size, color cho text hiển thị khi chọn

### 3.2 Thêm icon cho dropdown (Tùy chọn)
1. Thêm Image component vào PropertiesDropdown
2. Sử dụng icon mũi tên hoặc icon tài sản
3. Position: Right side của dropdown

## 4. Kết nối BankruptcyManager

1. Chọn BankruptcyManager GameObject
2. Trong Inspector, gán các reference:
   - **Bankruptcy Panel** → BankruptcyPanel
   - **Title Text** → TitleText
   - **Message Text** → MessageText
   - **Properties Dropdown** → PropertiesDropdown
   - **Sell Button** → SellButton
   - **Cancel Button** → CancelButton

## 5. Thiết lập GameManager

1. Đảm bảo GameManager có reference đến BankruptcyManager
2. Kiểm tra rằng BankruptcyManager được khởi tạo trước GameManager

## 6. Test Hệ Thống

### 6.1 Test với tiền thấp
- BankruptcyManager sẽ tự động set tiền gốc = 200$ cho tất cả player
- Để test nhanh, có thể sửa `startingMoney` trong BankruptcyManager

### 6.2 Test phá sản
1. Chạy game
2. Mua một số tài sản
3. Đi vào ô thuế hoặc trả tiền thuê để bị trừ tiền
4. Khi tiền < 0, panel phá sản sẽ hiện

### 6.3 Test bán tài sản
1. Chọn tài sản trong dropdown
2. Bấm "BÁN TÀI SẢN"
3. Kiểm tra tiền được cộng và tài sản bị xóa

## 7. Tùy Chỉnh Thêm

### 7.1 Thêm âm thanh
- Thêm AudioSource vào BankruptcyManager
- Phát âm thanh khi mở panel và khi bán tài sản

### 7.2 Tùy chỉnh animation
- Điều chỉnh `Show Duration` và `Hide Duration` trong Inspector
- Thay đổi `Show Curve` và `Hide Curve` để có hiệu ứng khác
- Animation fade in/out tự động được xử lý bởi script

### 7.3 Thêm thông báo
- Sử dụng GameManager.ShowInfoHud() để hiển thị thông báo
- Thông báo khi player thoát khỏi tình trạng phá sản

### 7.4 Xử lý Icon Emoji
- Hệ thống đã được tích hợp `IconReplacer` để thay thế emoji bằng text thông thường
- Các emoji ⚠️, 💀, ✅, 🤖 sẽ được thay thế bằng [CẢNH BÁO], [PHÁ SẢN], [THÀNH CÔNG], [BOT]
- Các emoji countdown (⏸️, 🎲, ⏳, ⏰, 🎮) sẽ bị xóa hoàn toàn
- Để thay đổi phương thức thay thế, sử dụng `IconReplacerManager` component

## 8. Lưu Ý Quan Trọng

1. **Thứ tự khởi tạo**: BankruptcyManager phải được khởi tạo trước GameManager
2. **UI Layer**: Đảm bảo BankruptcyPanel có Canvas Group hoặc được set active/inactive đúng cách
3. **Memory Management**: Xóa các item cũ trong PropertiesContainer trước khi tạo mới
4. **Error Handling**: Kiểm tra null reference cho tất cả UI components

## 9. Debug

Nếu gặp lỗi:
1. Kiểm tra Console để xem error messages
2. Đảm bảo tất cả references được gán đúng
3. Kiểm tra thứ tự execution trong Start() methods
4. Sử dụng Debug.Log để track flow của code 