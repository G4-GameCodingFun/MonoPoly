using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public string playerName;
    public int money = 500;
    public int currentTileIndex = 0;
    public bool inJail = false;
    public int jailTurns = 0;
    public bool skipNextTurn = false;
    public bool cannotBuyNextTurn = false;
    public bool canBuyDiscountProperty = false;
    public bool hasGetOutOfJailFreeCard = false;
    public bool isBot;
    public bool isBankrupt = false; // Thêm thuộc tính để theo dõi trạng thái phá sản

    public GameObject arrowPrefab;
    private GameObject arrowInstance;

    public List<Tile> ownedTiles = new List<Tile>();

    public bool CanPay(int amount) => money >= amount;

    private Animator animator;
    private void Start()
    {
        animator = GetComponent<Animator>();

        if (arrowPrefab != null)
        {
            arrowInstance = Instantiate(arrowPrefab);
            arrowInstance.transform.SetParent(transform);

            SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            float spriteHeight = spriteRenderer != null ? spriteRenderer.bounds.size.y : 1.0f;

            float offsetY = spriteHeight + 5f;

            arrowInstance.transform.position = transform.position + new Vector3(0f, offsetY, 0f);
        }
    }




    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void TryPay(int amount)
    {
        if (money >= amount)
        {
            money -= amount;
            AudioManager.Instance.PlayPayRent();
        }
        else
        {
            // Trừ tiền và kiểm tra phá sản
            money -= amount;
            AudioManager.Instance.PlayErrorMoney();
            Debug.LogWarning($"{playerName} không đủ tiền để trả {amount}$");
            
            // Kiểm tra phá sản sau khi trừ tiền
            if (BankruptcyManager.Instance != null)
            {
                BankruptcyManager.Instance.CheckBankruptcy(this);
            }
            else
            {
                Debug.LogWarning($"⚠ BankruptcyManager.Instance là null! {playerName} có thể bị phá sản mà không được xử lý.");
            }
        }
    }

    public void BuyProperty(Tile tile)
    {
        if (tile == null || tile.owner != null) return;
        
        int price = tile.GetPrice();
        if (money >= price)
        {
            money -= price;
            tile.owner = this;
            ownedTiles.Add(tile);
            tile.SetOwner(this);
            AudioManager.Instance.PlayBuyProperty();
            Debug.Log($"{playerName} đã mua {tile.tileName} với giá {price}$");
        }
        else
        {
            AudioManager.Instance.PlayErrorMoney();
            Debug.LogWarning($"{playerName} không đủ tiền mua {tile.tileName} (giá {price}$)");
        }
    }

    public void BuyProperty(PropertyTile propertyTile)
    {
        BuyProperty(propertyTile as Tile);
    }

    public void SellProperty(PropertyTile tile)
    {
        if (tile.owner == this)
        {
            int sellPrice = tile.GetPrice() / 2;
            money += sellPrice;
            ownedTiles.Remove(tile);
            tile.owner = null;
            AudioManager.Instance.PlayMortgage();
            Debug.Log($"{playerName} đã bán {tile.tileName} với giá {sellPrice}$");
            // Reset lại visual khi bán đất
            tile.houseCount = 0;
            tile.hasHotel = false;
            tile.SetOwner(null);
            tile.UpdateVisuals();
        }
    }

    public void MoveToStart()
    {
        currentTileIndex = 0;
        transform.position = GameManager.Instance.mapTiles[0].position;
        // Đồng bộ lại vị trí trong GameManager để tránh bug cộng dồn bước
        if (GameManager.Instance != null && GameManager.Instance.currentTileIndexes != null)
        {
            int idx = GameManager.Instance.players.IndexOf(this);
            if (idx >= 0 && idx < GameManager.Instance.currentTileIndexes.Length)
                GameManager.Instance.currentTileIndexes[idx] = 0;
        }
    }

    public void MoveSteps(int steps)
    {
        int newIndex = currentTileIndex + steps;
        
        // Xử lý trường hợp di chuyển lùi (steps < 0)
        if (newIndex < 0)
        {
            newIndex = GameManager.Instance.mapTiles.Count + newIndex;
        }
        
        currentTileIndex = newIndex % GameManager.Instance.mapTiles.Count;
        transform.position = GameManager.Instance.mapTiles[currentTileIndex].position;
        
        // Đồng bộ lại vị trí trong GameManager để tránh bug cộng dồn bước
        if (GameManager.Instance != null && GameManager.Instance.currentTileIndexes != null)
        {
            int idx = GameManager.Instance.players.IndexOf(this);
            if (idx >= 0 && idx < GameManager.Instance.currentTileIndexes.Length)
                GameManager.Instance.currentTileIndexes[idx] = currentTileIndex;
        }
    }

    public void MoveToMostExpensiveProperty()
    {
        int maxPrice = -1;
        int targetIndex = -1;
        for (int i = 0; i < GameManager.Instance.mapTiles.Count; i++)
        {
            var prop = GameManager.Instance.mapTiles[i].GetComponent<PropertyTile>();
            if (prop != null && prop.GetPrice() > maxPrice)
            {
                maxPrice = prop.GetPrice();
                targetIndex = i;
            }
        }
        if (targetIndex != -1)
        {
            currentTileIndex = targetIndex;
            transform.position = GameManager.Instance.mapTiles[targetIndex].position;
            
            // Đồng bộ lại vị trí trong GameManager để tránh bug cộng dồn bước
            if (GameManager.Instance != null && GameManager.Instance.currentTileIndexes != null)
            {
                int idx = GameManager.Instance.players.IndexOf(this);
                if (idx >= 0 && idx < GameManager.Instance.currentTileIndexes.Length)
                    GameManager.Instance.currentTileIndexes[idx] = targetIndex;
            }
        }
    }

    public void MoveToNearest(string tag)
    {
        int start = currentTileIndex;
        for (int offset = 1; offset < GameManager.Instance.mapTiles.Count; offset++)
        {
            int index = (start + offset) % GameManager.Instance.mapTiles.Count;
            if (GameManager.Instance.mapTiles[index].CompareTag(tag))
            {
                currentTileIndex = index;
                transform.position = GameManager.Instance.mapTiles[index].position;
                
                // Đồng bộ lại vị trí trong GameManager để tránh bug cộng dồn bước
                if (GameManager.Instance != null && GameManager.Instance.currentTileIndexes != null)
                {
                    int idx = GameManager.Instance.players.IndexOf(this);
                    if (idx >= 0 && idx < GameManager.Instance.currentTileIndexes.Length)
                        GameManager.Instance.currentTileIndexes[idx] = index;
                }
                return;
            }
        }
    }

    public void GoToJail()
    {
        Debug.Log($"🚨 {playerName} bị đưa vào tù!");
        
        if (GameManager.Instance.jailPosition != null)
        {
            int jailIndex = GameManager.Instance.mapTiles.FindIndex(t => t == GameManager.Instance.jailPosition);
            if (jailIndex != -1)
            {
                Debug.Log($"📍 {playerName} di chuyển đến ô tù: {jailIndex}");
                currentTileIndex = jailIndex;
                transform.position = GameManager.Instance.jailPosition.position;
                inJail = true;
                jailTurns = 3;
                AudioManager.Instance.PlayGoToJail();
                // Đồng bộ lại vị trí trong GameManager để tránh bug cộng dồn bước
                if (GameManager.Instance != null && GameManager.Instance.currentTileIndexes != null)
                {
                    int idx = GameManager.Instance.players.IndexOf(this);
                    if (idx >= 0 && idx < GameManager.Instance.currentTileIndexes.Length)
                    {
                        GameManager.Instance.currentTileIndexes[idx] = jailIndex;
                        Debug.Log($"📍 Đồng bộ vị trí {playerName} trong GameManager: {jailIndex}");
                    }
                }

                Debug.Log($"🔒 {playerName} vào tù 3 lượt. Vị trí: {currentTileIndex}, JailTurns: {jailTurns}");
            }
            else
            {
                Debug.LogError("❌ Không tìm thấy jailPosition trong mapTiles!");
            }
        }
        else
        {
            Debug.LogWarning("❌ Jail position chưa được gán trong GameManager");
        }
    }

    public void GetOutOfJail()
    {
        Debug.Log($"🔓 {playerName} được thả ra tù");
        inJail = false;
        jailTurns = 0;
        AudioManager.Instance.PlayGetOutOfJail();
        
        // Đồng bộ lại vị trí trong GameManager
        if (GameManager.Instance != null && GameManager.Instance.currentTileIndexes != null)
        {
            int idx = GameManager.Instance.players.IndexOf(this);
            if (idx >= 0 && idx < GameManager.Instance.currentTileIndexes.Length)
            {
                GameManager.Instance.currentTileIndexes[idx] = currentTileIndex;
            }
        }
    }

    public void UseGetOutOfJailFreeCard()
    {
        if (hasGetOutOfJailFreeCard && inJail)
        {
            Debug.Log($"🎫 {playerName} sử dụng thẻ 'Get Out of Jail Free'");
            hasGetOutOfJailFreeCard = false;
            GetOutOfJail();
        }
        else
        {
            Debug.LogWarning($"⚠️ {playerName} không thể sử dụng thẻ 'Get Out of Jail Free' (không có thẻ hoặc không ở tù)");
        }
    }

    public void PayRent(PlayerController owner, int amount)
    {
        if (money >= amount)
        {
            money -= amount;
            owner.money += amount;
            AudioManager.Instance.PlayPayRent();
            Debug.Log($"{playerName} đã trả {amount}$ tiền thuê cho {owner.playerName}");
        }
        else
        {
            // Trả hết tiền còn lại cho chủ sở hữu
            int remainingMoney = money;
            money = 0;
            owner.money += remainingMoney;
            AudioManager.Instance.PlayErrorMoney();
            Debug.LogWarning($"{playerName} không đủ tiền trả {amount}$ tiền thuê cho {owner.playerName}. Đã trả hết {remainingMoney}$");
            
            // Kiểm tra phá sản
            if (BankruptcyManager.Instance != null)
            {
                BankruptcyManager.Instance.CheckBankruptcy(this);
            }
            else
            {
                Debug.LogWarning($"⚠ BankruptcyManager.Instance là null! {playerName} có thể bị phá sản mà không được xử lý.");
            }
        }
    }

    public void GoBankrupt(PlayerController creditor)
    {
        // Chuyển tất cả tài sản cho creditor
        foreach (var tile in ownedTiles)
        {
            tile.owner = creditor;
            creditor.ownedTiles.Add(tile);
        }
        ownedTiles.Clear();
        
        // Chuyển tiền còn lại cho creditor
        if (money > 0)
        {
            creditor.money += money;
            money = 0;
        }
        
        // Set trạng thái phá sản
        isBankrupt = true;
        
        AudioManager.Instance.PlayBankrupt();
        Debug.Log($"{playerName} đã phá sản! Tất cả tài sản chuyển cho {creditor.playerName}");
    }

    public bool IsBankrupt()
    {
        return money <= 0 && ownedTiles.Count == 0;
    }

    public int TotalAssetValue()
    {
        int total = money;
        foreach (var tile in ownedTiles)
        {
            total += tile.GetPrice() / 2;
        }
        return total;
    }

    public void FaceLeft()
    {
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * -1;
        transform.localScale = scale;
    }

    public void SetWalking(bool walking)
    {
        if (animator != null)
        {
            animator.SetBool("isRunning", walking);
        }
    }
    public void SetArrowVisible(bool visible)
    {
        if (arrowInstance != null)
            arrowInstance.SetActive(visible);
    }
    private void LateUpdate()
    {
        if (arrowInstance != null)
        {
            SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
            float offsetY = sr != null ? sr.bounds.size.y + 1.5f : 1.0f;
            arrowInstance.transform.position = transform.position + new Vector3(0f, offsetY, 0f);
        }
    }
}