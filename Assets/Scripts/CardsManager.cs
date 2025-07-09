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

    public Animator cardAnimator;     // Animator gắn vào CardDisplay
    private bool isFlipping = false;
    private bool hasFlippedOnce = false;
    void Start()
    {
        LoadCards();  // Lúc bắt đầu game → load asset
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
        if (coHoiCards.Count == 0) return;
        StartCoroutine(PlayFlipAndShowCard(coHoiCards));
    }

    public void DrawKhiVanCard()
    {
        if (khiVanCards.Count == 0) return;
        StartCoroutine(PlayFlipAndShowCard(khiVanCards));
    }


    //IEnumerator PlayFlipAndShowCard(List<Sprite> cards)
    //{
    //    if (isFlipping) yield break;
    //    isFlipping = true;

    //    drawCoHoiButton.SetActive(false);
    //    drawKhiVanButton.SetActive(false);

    //    cardPanel.SetActive(true);

    //    // ✅ Random NGAY & gán sprite mới
    //    int index = Random.Range(0, cards.Count);
    //    Sprite pickedCard = cards[index];
    //    cardDisplay.sprite = pickedCard;

    //    // ✅ Sau đó mới chạy animation lật
    //    cardAnimator.SetTrigger("Flip");

    //    // ✅ Đợi animation chạy xong (0.5s)
    //    yield return new WaitForSeconds(0.5f);

    //    isFlipping = false;
    //}


    IEnumerator PlayFlipAndShowCard(List<Sprite> cards)
    {
        if (isFlipping) yield break;
        isFlipping = true;

        drawCoHoiButton.SetActive(false);
        drawKhiVanButton.SetActive(false);

        cardPanel.SetActive(true);

        // Random thẻ NGAY
        int index = Random.Range(0, cards.Count);
        Sprite pickedCard = cards[index];
        cardDisplay.sprite = pickedCard;

        if (!hasFlippedOnce)
        {
            // ✅ Chỉ lật lần đầu
            cardAnimator.SetTrigger("Flip");
            yield return new WaitForSeconds(0.5f); // Đợi animation
            hasFlippedOnce = true;
        }
        else
        {
            // ✅ Những lần sau: không cần đợi animation
            yield return null;
        }

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
