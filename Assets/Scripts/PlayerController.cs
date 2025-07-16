using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public string playerName;
    public int money = 2000;
    public int currentTileIndex = 0;
    public bool inJail = false;
    public int jailTurns = 0;
    public bool skipNextTurn = false;
    public bool cannotBuyNextTurn = false;
    public bool canBuyDiscountProperty = false;
    public bool hasGetOutOfJailFreeCard = false;
    public bool isBot;

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
            money -= amount;
        else
            Debug.LogWarning($"{playerName} không đủ tiền để trả {amount}$");
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
            Debug.Log($"{playerName} đã mua {tile.tileName} với giá {price}$");
        }
        else
        {
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
            Debug.Log($"{playerName} đã bán {tile.tileName} với giá {sellPrice}$");
        }
    }

    public void MoveToStart()
    {
        currentTileIndex = 0;
        transform.position = GameManager.Instance.mapTiles[0].position;
    }

    public void MoveSteps(int steps)
    {
        currentTileIndex = (currentTileIndex + steps) % GameManager.Instance.mapTiles.Count;
        transform.position = GameManager.Instance.mapTiles[currentTileIndex].position;
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
                return;
            }
        }
    }

    public void GoToJail()
    {
        if (GameManager.Instance.jailPosition != null)
        {
            int jailIndex = GameManager.Instance.mapTiles.FindIndex(t => t == GameManager.Instance.jailPosition);
            if (jailIndex != -1)
            {
                currentTileIndex = jailIndex;
                transform.position = GameManager.Instance.jailPosition.position;
                inJail = true;
                jailTurns = 3;

                Debug.Log($"{playerName} vào tù 3 lượt");
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
        inJail = false;
        jailTurns = 0;
        Debug.Log($"✅ {playerName} đã ra tù");
    }

    public void PayRent(PlayerController owner, int amount)
    {
        if (money >= amount)
        {
            money -= amount;
            owner.money += amount;
            Debug.Log($"{playerName} đã trả {amount}$ tiền thuê cho {owner.playerName}");
        }
        else
        {
            Debug.LogWarning($"{playerName} không đủ tiền trả {amount}$ tiền thuê cho {owner.playerName}");
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