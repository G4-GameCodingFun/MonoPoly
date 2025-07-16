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

        if (hasHotel) return data.rentByHouse[5];
        return data.rentByHouse[Mathf.Clamp(houseCount, 0, 4)];
    }

    public int GetHouseCost() => data?.houseCost ?? 0;
    public int GetHotelCost() => data?.hotelCost ?? 0;
    public override int GetMortgageValue() => data?.mortgageValue ?? 0;

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
            int price = GetPrice();
            // Chỉ bot mới tự động mua khi đi qua
            if (player.isBot && player.CanPay(price))
            {
                player.BuyProperty(this);
                Debug.Log($"{player.playerName} đã mua {tileName} với giá {price}$");
                shouldUpdateVisuals = true;
            }
            else
            {
                Debug.Log($"{player.playerName} chưa mua {tileName} (giá {price}$), chỉ hiện panel nếu là người chơi thường");
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
            }
            else
            {
                Debug.Log($"{player.playerName} không đủ tiền trả {rent}$ tiền thuê cho {owner.playerName}");
                player.GoBankrupt(owner);
            }
        }
        // Nếu là chủ sở hữu
        else
        {
            Debug.Log($"{player.playerName} đang đứng trên đất của mình: {tileName}");

            // Gợi ý xây nhà nếu có thể
            if (houseCount < 4 && player.CanPay(GetHouseCost()))
            {
                houseCount++;
                player.money -= GetHouseCost();
                Debug.Log($"{player.playerName} xây thêm nhà. Tổng số nhà: {houseCount}");
                shouldUpdateVisuals = true;
            }
            else if (houseCount == 4 && !hasHotel && player.CanPay(GetHotelCost()))
            {
                hasHotel = true;
                houseCount = 0;
                player.money -= GetHotelCost();
                Debug.Log($"{player.playerName} đã xây khách sạn tại {tileName}");
                shouldUpdateVisuals = true;
            }
            else
            {
                Debug.Log($"Không thể xây thêm nhà hoặc khách sạn tại {tileName}");
            }
        }

        if (shouldUpdateVisuals)
        {
            UpdateVisuals();
        }
    }

    private void Awake()
    {
        if (GetComponent<BoxCollider2D>() == null)
        {
            gameObject.AddComponent<BoxCollider2D>();
        }
    }

    private void UpdateVisuals()
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
        var detailsPanel = FindObjectOfType<DetailsPanelController>();
        if (detailsPanel != null && GameManager.Instance != null && GameManager.Instance.players.Count > 0)
        {
            var player = GameManager.Instance.players[GameManager.Instance.currentPlayerIndex];
            detailsPanel.Show(this, player);
        }
    }
}