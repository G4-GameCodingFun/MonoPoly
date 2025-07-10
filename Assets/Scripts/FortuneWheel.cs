using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FortuneWheel : MonoBehaviour
{
    [Header("Dữ liệu thẻ")]
    public List<Sprite> coHoiCards;     // Gán trong Inspector
    public List<Sprite> khiVanCards;    // Sẽ tự load tự động

    [Header("UI")]
    public GameObject cardPrefab;       // Prefab thẻ (UI Image)
    public Transform cardsPanel;        // Panel chứa thẻ (GridLayoutGroup)

    void Start()
    {
        LoadKhiVanCards();
        ShowAllCards();
    }

    void LoadKhiVanCards()
    {
        // Load tất cả sprite trong Resources/Sprites/TheKhiVan
        Sprite[] loadedCards = Resources.LoadAll<Sprite>("Sprites/TheKhiVan");
        khiVanCards = new List<Sprite>(loadedCards);

        Debug.Log("Đã load " + khiVanCards.Count + " thẻ Khí Vận từ folder.");
    }

    public void SpinAndPick()
    {
        List<Sprite> allCards = new List<Sprite>();
        allCards.AddRange(coHoiCards);
        allCards.AddRange(khiVanCards);

        if (allCards.Count == 0)
        {
            Debug.LogWarning("Không có thẻ nào để quay!");
            return;
        }

        int randomIndex = Random.Range(0, allCards.Count);
        Sprite pickedCard = allCards[randomIndex];

        Debug.Log("Đã random được thẻ: " + pickedCard.name);
        ShowPickedCard(pickedCard);
    }

    void ShowPickedCard(Sprite cardSprite)
    {
        // Tạo 1 card UI từ prefab và hiển thị sprite
        GameObject cardObj = Instantiate(cardPrefab, cardsPanel);
        Image img = cardObj.GetComponent<Image>();
        if (img != null)
        {
            img.sprite = cardSprite;
        }
        else
        {
            Debug.LogWarning("Prefab chưa có component Image!");
        }
    }

    void ShowAllCards()
    {
        // Hiển thị tất cả thẻ lên UI (panel)
        List<Sprite> allCards = new List<Sprite>();
        allCards.AddRange(coHoiCards);
        allCards.AddRange(khiVanCards);

        foreach (Sprite cardSprite in allCards)
        {
            GameObject cardObj = Instantiate(cardPrefab, cardsPanel);
            Image img = cardObj.GetComponent<Image>();
            if (img != null)
            {
                img.sprite = cardSprite;
            }
        }
    }
}
