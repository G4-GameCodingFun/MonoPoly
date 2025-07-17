# Hướng Dẫn Troubleshooting Logic Tù

## Vấn Đề Thường Gặp

### 1. Player Bị Stuck Trong Tù
**Triệu chứng:**
- Player không thể ra tù sau khi hết lượt
- `jailTurns` âm hoặc không giảm
- Player không thể di chuyển

**Nguyên nhân có thể:**
- Logic đồng bộ vị trí bị lỗi
- `currentTileIndexes` không được cập nhật đúng
- Player bị skip lượt liên tục

**Giải pháp:**
- Đã thêm debug logs để theo dõi
- Thêm cơ chế tự động thả ra tù khi bị stuck
- Kiểm tra Console để xem debug logs

### 2. Player Không Vào Tù Khi Đi Vào Ô "Đi Tù"
**Triệu chứng:**
- Player đi vào ô "Đi Tù" nhưng không bị vào tù
- Không có thông báo vào tù

**Nguyên nhân có thể:**
- `jailPosition` chưa được gán trong GameManager
- `GoToJailTile` không được setup đúng
- Logic `OnPlayerLanded` không được gọi

**Giải pháp:**
1. Kiểm tra GameManager.jailPosition đã được gán chưa
2. Kiểm tra ô "Đi Tù" có component `GoToJailTile` không
3. Kiểm tra Console để xem debug logs

### 3. Player Ra Tù Nhưng Vẫn Ở Vị Trí Tù
**Triệu chứng:**
- Player được thông báo ra tù nhưng vẫn ở ô tù
- Không thể di chuyển từ ô tù

**Nguyên nhân có thể:**
- `currentTileIndex` không được cập nhật đúng
- `currentTileIndexes` trong GameManager không đồng bộ
- Transform position không được cập nhật

**Giải pháp:**
- Đã thêm logic đồng bộ vị trí sau khi ra tù
- Kiểm tra debug logs để xem vị trí có được cập nhật không

## Debug Logs

### Khi Vào Tù:
```
🚨 [PlayerName] bị đưa vào tù!
📍 [PlayerName] di chuyển đến ô tù: [JailIndex]
📍 Đồng bộ vị trí [PlayerName] trong GameManager: [JailIndex]
🔒 [PlayerName] vào tù 3 lượt. Vị trí: [CurrentIndex], JailTurns: 3
```

### Khi Ở Tù:
```
🔒 [PlayerName] đang ở tù. JailTurns: [RemainingTurns]
🔒 [PlayerName] vẫn ở tù. Còn [RemainingTurns] lượt
```

### Khi Ra Tù:
```
🔓 [PlayerName] hết lượt tù, được thả ra
🔓 [PlayerName] được thả ra tù. Vị trí hiện tại: [CurrentIndex]
📍 Đồng bộ vị trí [PlayerName]: [CurrentIndex]
✓ [PlayerName] đã ra tù. InJail: false, JailTurns: 0
```

### Khi Bị Stuck:
```
⚠️ [PlayerName] bị stuck trong tù! Tự động thả ra...
```

## Cách Kiểm Tra

### 1. Kiểm Tra Console
- Mở Console trong Unity
- Tìm các debug logs với emoji 🔒, 🔓, 🚨, ⚠️
- Kiểm tra thứ tự logs có đúng không

### 2. Kiểm Tra Inspector
- Chọn Player GameObject
- Kiểm tra `inJail` và `jailTurns` trong PlayerController
- Kiểm tra `currentTileIndex` có đúng không

### 3. Kiểm Tra GameManager
- Chọn GameManager GameObject
- Kiểm tra `jailPosition` đã được gán chưa
- Kiểm tra `currentTileIndexes` có đồng bộ không

## Cách Sửa Lỗi Thủ Công

### Nếu Player Bị Stuck Trong Tù:
1. Chọn Player GameObject
2. Trong Inspector, set `inJail = false`
3. Set `jailTurns = 0`
4. Kiểm tra `currentTileIndex` có đúng không

### Nếu JailPosition Chưa Được Gán:
1. Chọn GameManager GameObject
2. Trong Inspector, tìm `Jail Position`
3. Kéo ô tù từ Hierarchy vào slot này

### Nếu GoToJailTile Không Hoạt Động:
1. Chọn ô "Đi Tù" trong Hierarchy
2. Kiểm tra có component `GoToJailTile` không
3. Nếu không có, thêm component này

## Test Cases

### Test 1: Vào Tù Bình Thường
1. Player đi vào ô "Đi Tù"
2. Kiểm tra player có vào tù không
3. Kiểm tra `jailTurns = 3`
4. Kiểm tra player ở đúng vị trí tù

### Test 2: Ở Tù 3 Lượt
1. Player đang ở tù
2. Roll dice 3 lần
3. Kiểm tra `jailTurns` giảm từ 3 → 2 → 1 → 0
4. Kiểm tra player ra tù sau lượt thứ 3

### Test 3: Ra Tù Và Di Chuyển
1. Player vừa ra tù
2. Roll dice
3. Kiểm tra player có di chuyển được không
4. Kiểm tra vị trí có đúng không

## Lưu Ý Quan Trọng

1. **Đồng bộ vị trí**: Luôn đảm bảo `currentTileIndexes` trong GameManager đồng bộ với `currentTileIndex` của Player
2. **Debug logs**: Sử dụng debug logs để theo dõi trạng thái tù
3. **Auto-fix**: Hệ thống có cơ chế tự động thả ra tù khi bị stuck
4. **Jail position**: Đảm bảo `jailPosition` trong GameManager được gán đúng

## Liên Hệ

Nếu vẫn gặp vấn đề, hãy:
1. Kiểm tra Console để xem debug logs
2. Chụp screenshot lỗi
3. Mô tả chi tiết các bước gây ra lỗi 