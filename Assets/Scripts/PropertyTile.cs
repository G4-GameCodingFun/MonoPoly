using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class PropertyTile : Tile
{
    public PropertyData data; // Gán bằng tay trong Unity

    public int houseCount = 0;
    public bool hasHotel = false;

    public GameObject playerHousePrefab;
    public GameObject botHousePrefab;
    public GameObject playerHotelPrefab;
    public GameObject botHotelPrefab;


    private List<GameObject> spawnedHouses = new List<GameObject>();
    private GameObject spawnedHotel;

    public override int GetPrice() => data != null ? data.purchasePrice : 0;
    public override int GetRent()
    {
        if (data == null || data.rentByHouse == null)
            return 0;

        // Nếu là hotel đặc biệt (nâng cấp qua offer)
        if (hasHotel && data.groupSize > 0)
            return data.rentByHouse[0] * 5;

        if (hasHotel) return data.rentByHouse[5];
        return data.rentByHouse[Mathf.Clamp(houseCount, 0, 4)];
    }

    public int GetHouseCost() => data?.houseCost ?? 0;
    public int GetHotelCost() => data?.hotelCost ?? 0;
    public override int GetMortgageValue() => data?.mortgageValue ?? 0;

    /// <summary>
    /// Kiểm tra xem có thể xây thêm nhà không (phải sở hữu tất cả nhà cùng type)
    /// </summary>
    public bool CanBuildHouse()
    {
        if (data == null || owner == null) return false;
        
        // Sử dụng groupSize từ data
        int groupSize = data.groupSize > 0 ? data.groupSize : 1;
        int ownedSameTypeCount = 0;
        foreach (var tile in GameManager.Instance.mapTiles)
        {
            var propTile = tile.GetComponent<PropertyTile>();
            if (propTile != null && propTile.data != null && propTile.data.type == data.type)
            {
                if (propTile.owner == owner)
                    ownedSameTypeCount++;
            }
        }
        // Phải sở hữu tất cả nhà cùng type (groupSize) và chưa có hotel
        return ownedSameTypeCount == groupSize && !hasHotel && houseCount < 4;
    }

    /// <summary>
    /// Kiểm tra xem có thể xây hotel không
    /// </summary>
    public bool CanBuildHotel()
    {
        if (data == null || owner == null) return false;
        
        // Sử dụng groupSize từ data
        int groupSize = data.groupSize > 0 ? data.groupSize : 1;
        int ownedSameTypeCount = 0;
        foreach (var tile in GameManager.Instance.mapTiles)
        {
            var propTile = tile.GetComponent<PropertyTile>();
            if (propTile != null && propTile.data != null && propTile.data.type == data.type)
            {
                if (propTile.owner == owner)
                    ownedSameTypeCount++;
            }
        }
        // Phải sở hữu tất cả nhà cùng type (groupSize) và đã có 4 nhà
        return ownedSameTypeCount == groupSize && houseCount == 4 && !hasHotel;
    }

    /// <summary>
    /// Xây thêm 1 nhà
    /// </summary>
    public bool BuildHouse()
    {
        if (!CanBuildHouse() || !owner.CanPay(GetHouseCost())) return false;
        
        owner.money -= GetHouseCost();
        houseCount++;
        UpdateVisuals();
        
        Debug.Log($"{owner.playerName} đã xây thêm nhà tại {tileName}. Tổng: {houseCount} nhà");
        if (GameManager.Instance != null)
            GameManager.Instance.ShowInfoHud($"{owner.playerName} đã xây thêm nhà tại {tileName}. Tổng: {houseCount} nhà");
        
        return true;
    }

    /// <summary>
    /// Nâng cấp lên hotel
    /// </summary>
    public bool BuildHotel()
    {
        if (!CanBuildHotel() || !owner.CanPay(GetHotelCost())) return false;
        
        owner.money -= GetHotelCost();
        houseCount = 0;
        hasHotel = true;
        UpdateVisuals();
        
        Debug.Log($"{owner.playerName} đã nâng cấp lên hotel tại {tileName}");
        if (GameManager.Instance != null)
            GameManager.Instance.ShowInfoHud($"{owner.playerName} đã nâng cấp lên hotel tại {tileName}");
        
        return true;
    }

    /// <summary>
    /// Trả về PropertyData của ô này
    /// </summary>
    public PropertyData GetData()
    {
        return data;
    }
    public override void OnPlayerLanded(PlayerController player)
    {
        if (data == null || player == null) return;

        bool shouldUpdateVisuals = false;

        // Nếu chưa có chủ sở hữu
        if (owner == null)
        {
            int houseCost = GetHouseCost();
            if (player.isBot)
            {
                // Bot tự động mua nhà nếu đủ tiền
                if (player.CanPay(houseCost))
                {
                    player.money -= houseCost;
                    owner = player;
                    player.ownedTiles.Add(this);
                    SetOwner(player);
                    houseCount = 1;
                    Debug.Log($"{player.playerName} (Bot) đã mua {tileName} với giá xây 1 nhà: {houseCost}$");
                    if (GameManager.Instance != null)
                        GameManager.Instance.ShowInfoHud($"{player.playerName} (Bot) đã mua {tileName} với giá xây 1 nhà: {houseCost}$");
                    shouldUpdateVisuals = true;
                }
                else
                {
                    Debug.Log($"{player.playerName} (Bot) không đủ tiền xây nhà tại {tileName} (giá {houseCost}$)");
                    if (GameManager.Instance != null)
                        GameManager.Instance.ShowInfoHud($"{player.playerName} (Bot) không đủ tiền xây nhà tại {tileName} (giá {houseCost}$)");
                }
            }
            else
            {
                // Người chơi thường chỉ hiện panel, không auto mua
                Debug.Log($"{player.playerName} (Người chơi) đến ô {tileName}, chỉ hiện panel, không auto mua.");
            }
        }
        // Nếu đã có chủ khác
        else if (owner != player)
        {
            int rent = GetRent();
            if (player.CanPay(rent))
            {
                player.PayRent(owner, rent);
                Debug.Log($"{player.playerName} trả {rent}$ tiền thuê cho {owner.playerName}");
                if (GameManager.Instance != null)
                    GameManager.Instance.ShowInfoHud($"{player.playerName} trả {rent}$ tiền thuê cho {owner.playerName}");
            }
            else
            {
                Debug.Log($"{player.playerName} không đủ tiền trả {rent}$ tiền thuê cho {owner.playerName}");
                if (GameManager.Instance != null)
                    GameManager.Instance.ShowInfoHud($"{player.playerName} không đủ tiền trả {rent}$ tiền thuê cho {owner.playerName}");
                player.GoBankrupt(owner);
            }
        }
        // Nếu là chủ sở hữu
        else
        {
            Debug.Log($"{player.playerName} đang đứng trên đất của mình: {tileName}");
            // Không tự động xây thêm nhà nữa khi đã sở hữu
        }

        if (shouldUpdateVisuals)
        {
            UpdateVisuals();
        }
    }

    // Gọi offer HUD khi đủ điều kiện sở hữu đủ groupSize nhà cùng màu
    public void CheckAndOfferHotelUpgrade()
    {
        if (data == null || owner == null || hasHotel) return;
        int groupSize = data.groupSize > 0 ? data.groupSize : 1;
        int owned = 0;
        foreach (var tile in GameManager.Instance.mapTiles)
        {
            var prop = tile.GetComponent<PropertyTile>();
            if (prop != null && prop.data != null && prop.data.type == data.type && prop.owner == owner)
                owned++;
        }
        if (owned == groupSize)
        {
            var hud = GameObject.FindAnyObjectByType<HUDOfferController>();
            if (hud != null)
                hud.Show(this, groupSize, data.type);
        }
    }

    private void Awake()
    {
        if (GetComponent<BoxCollider2D>() == null)
        {
            gameObject.AddComponent<BoxCollider2D>();
        }
    }

    public void UpdateVisuals()
    {
        foreach (var obj in spawnedHouses)
            Destroy(obj);
        spawnedHouses.Clear();

        if (spawnedHotel != null)
        {
            Destroy(spawnedHotel);
            spawnedHotel = null;
        }

        bool isBot = owner != null && owner.isBot;

        GameObject housePrefabToUse = isBot ? botHousePrefab : playerHousePrefab;
        GameObject hotelPrefabToUse = isBot ? botHotelPrefab : playerHotelPrefab;

        // 👉 Tạo khách sạn
        if (hasHotel && hotelPrefabToUse != null)
        {
            spawnedHotel = Instantiate(hotelPrefabToUse, transform);
            spawnedHotel.transform.localPosition = new Vector3(0f, 4f, 0f);

            // Nếu là bot: set số bot lên hotel
            if (isBot)
                SetBotNumberLabel(spawnedHotel);
        }
        // 👉 Tạo nhà
        else if (houseCount == 1 && housePrefabToUse != null)
        {
            var house = Instantiate(housePrefabToUse, transform);
            house.transform.localPosition = new Vector3(0f, 3.5f, 0f);
            spawnedHouses.Add(house);

            if (isBot)
                SetBotNumberLabel(house);
        }
        else if (houseCount > 1 && housePrefabToUse != null)
        {
            float startX = -1.5f;
            float gap = 0.95f;
            for (int i = 0; i < houseCount; i++)
            {
                var house = Instantiate(housePrefabToUse, transform);
                house.transform.localPosition = new Vector3(startX + i * gap, 3.5f, 0f);
                spawnedHouses.Add(house);

                if (isBot)
                    SetBotNumberLabel(house);
            }
        }
    }

    private void SetBotNumberLabel(GameObject buildingObject)
    {
        if (owner == null || !owner.isBot) return;

        var text = buildingObject.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null)
        {
            string digits = System.Text.RegularExpressions.Regex.Match(owner.playerName, @"\d+$").Value;
            text.text = string.IsNullOrEmpty(digits) ? "B" : digits;
        }
    }

    private void OnMouseDown()
    {
        // Chỉ cho phép click nếu không bị che bởi UI
        var detailsPanel = GameObject.FindAnyObjectByType<DetailsPanelController>();
        if (detailsPanel != null && GameManager.Instance != null && GameManager.Instance.players.Count > 0)
        {
            var player = GameManager.Instance.players[GameManager.Instance.currentPlayerIndex];
            detailsPanel.Show(this, player);
        }
    }

    public int GetSellPrice()
    {
        // Nếu là hotel đặc biệt (nâng cấp qua offer)
        if (hasHotel && data.groupSize > 0)
            return 1200 * 3;
        // Giá bán mặc định (có thể sửa theo ý bạn)
        return GetPrice() / 2;
    }
}