using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardManager : MonoBehaviour
{
    public List<Sprite> coHoiCards;
    public List<Sprite> khiVanCards;

    public Image cardDisplay;         // UI Image để hiện thẻ
    public GameObject cardPanel;      // Panel chứa thẻ

    public GameObject drawCoHoiButton;
    public GameObject drawKhiVanButton;

    public Animator cardsBackAnimator;     // Animator gắn trên CardsBackPanel
    public GameObject cardsBackPanel;      // Panel chứa 5 thẻ úp

    private bool isFlipping = false;

    void Start()
    {
        LoadCards();
    }

    void LoadCards()
    {
        // Load tự động các thẻ từ folder Resources/Sprites/...
        Sprite[] coHoi = Resources.LoadAll<Sprite>("Sprites/CoHoiCard");
        coHoiCards = new List<Sprite>(coHoi);

        Sprite[] khiVan = Resources.LoadAll<Sprite>("Sprites/KhiVanCard");
        khiVanCards = new List<Sprite>(khiVan);

        Debug.Log("Đã load " + coHoiCards.Count + " thẻ Cơ Hội.");
        Debug.Log("Đã load " + khiVanCards.Count + " thẻ Khí Vận.");
    }

    public void DrawCoHoiCard()
    {
        if (coHoiCards.Count == 0 || isFlipping) return;
        StartCoroutine(PlayShuffleAndShowCard(coHoiCards));
    }

    public void DrawKhiVanCard()
    {
        if (khiVanCards.Count == 0 || isFlipping) return;
        StartCoroutine(PlayShuffleAndShowCard(khiVanCards));
    }

    IEnumerator PlayShuffleAndShowCard(List<Sprite> cards)
    {
        if (isFlipping) yield break;
        isFlipping = true;

        drawCoHoiButton.SetActive(false);
        drawKhiVanButton.SetActive(false);

        // Hiện 5 thẻ lưng
        cardsBackPanel.SetActive(true);

        // Chạy animation shuffle
        cardsBackAnimator.SetTrigger("Shuffle");

        // Đợi shuffle chạy xong (ví dụ 1s)
        yield return new WaitForSeconds(1f);

        // Tắt 5 thẻ
        cardsBackPanel.SetActive(false);

        // ✨ Thêm nhịp chờ 0.3–0.5s tạo “khoảng lặng” trước khi mở
        yield return new WaitForSeconds(0.5f);

        // Random & gán thẻ
        int index = Random.Range(0, cards.Count);
        cardDisplay.sprite = cards[index];

        // Hiện panel thẻ
        cardPanel.SetActive(true);

        isFlipping = false;
    }

    public void CloseCardPanel()
    {
        // Tắt panel và hiện lại 2 nút
        cardPanel.SetActive(false);
        drawCoHoiButton.SetActive(true);
        drawKhiVanButton.SetActive(true);
    }
}
