using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardManager : MonoBehaviour
{
    public List<Sprite> coHoiCards;   // Load tự động
    public List<Sprite> khiVanCards;  // Load tự động

    public Image cardDisplay;         // UI Image để hiện thẻ
    public GameObject cardPanel;      // UI Panel chứa card

    public GameObject drawCoHoiButton;    // Nút Bốc Cơ Hội
    public GameObject drawKhiVanButton;   // Nút Bốc Khí Vận

    void Start()
    {
        LoadCards();
    }

    void LoadCards()
    {
        Sprite[] coHoi = Resources.LoadAll<Sprite>("Sprites/CoHoiCard");
        if (coHoi.Length == 0) Debug.LogWarning("Không load được thẻ Cơ Hội!");
        coHoiCards = new List<Sprite>(coHoi);

        Sprite[] khiVan = Resources.LoadAll<Sprite>("Sprites/KhiVanCard");
        if (khiVan.Length == 0) Debug.LogWarning("Không load được thẻ Khí Vận!");
        khiVanCards = new List<Sprite>(khiVan);

        Debug.Log("Đã load " + coHoiCards.Count + " thẻ Cơ Hội.");
        Debug.Log("Đã load " + khiVanCards.Count + " thẻ Khí Vận.");
    }

    // Hàm random 1 thẻ Cơ Hội
    public void DrawCoHoiCard()
    {
        if (coHoiCards.Count == 0)
        {
            Debug.LogWarning("Chưa có thẻ Cơ Hội!");
            return;
        }

        int randomIndex = Random.Range(0, coHoiCards.Count);
        Sprite pickedCard = coHoiCards[randomIndex];
        ShowCard(pickedCard);
    }

    // Hàm random 1 thẻ Khí Vận
    public void DrawKhiVanCard()
    {
        if (khiVanCards.Count == 0)
        {
            Debug.LogWarning("Chưa có thẻ Khí Vận!");
            return;
        }

        int randomIndex = Random.Range(0, khiVanCards.Count);
        Sprite pickedCard = khiVanCards[randomIndex];
        ShowCard(pickedCard);
    }

    void ShowCard(Sprite card)
    {
        cardDisplay.sprite = card;
        cardPanel.SetActive(true);

        // Ẩn 2 nút khi hiện thẻ
        drawCoHoiButton.SetActive(false);
        drawKhiVanButton.SetActive(false);
    }

    public void CloseCardPanel()
    {
        cardPanel.SetActive(false);

        // Hiện lại 2 nút
        drawCoHoiButton.SetActive(true);
        drawKhiVanButton.SetActive(true);
    }
}
