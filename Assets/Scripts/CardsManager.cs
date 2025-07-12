using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardManager : MonoBehaviour
{
    public List<CardData> coHoiCards;
    public List<CardData> khiVanCards;

    public Image cardDisplay;
    public GameObject cardPanel;
    public GameObject drawCoHoiButton;
    public GameObject drawKhiVanButton;

    public Animator cardsBackAnimator;
    public GameObject cardsBackPanel;

    private bool isFlipping = false;

    void Start()
    {
        LoadAllCards();
    }

    void LoadAllCards()
    {
        coHoiCards = new List<CardData>();
        khiVanCards = new List<CardData>();

        AddCoHoiCard("DICHUYEN1", CardEffectType.TROLAI_XUATPHAT, 200);
        AddCoHoiCard("DICHUYEN2", CardEffectType.DI_LUI_3_BUOC, 0);
        AddCoHoiCard("DICHUYEN3", CardEffectType.TOI_CONGTY_GANNHAT, 0);
        AddCoHoiCard("TU1", CardEffectType.VAO_TU, 0);
        AddCoHoiCard("TU2", CardEffectType.THE_RA_TU_MIENPHI, 0);
        AddCoHoiCard("NHANTHUONG1", CardEffectType.THANG_GIAI_TOANTUAN, 200);
        AddCoHoiCard("NHANTHUONG2", CardEffectType.NHAN_TIEN_TIETKIEM, 100);
        AddCoHoiCard("NHANTHUONG3", CardEffectType.NHAN_HOCBONG, 250);
        AddCoHoiCard("SPEC1", CardEffectType.TANG_CA, 70);
        AddCoHoiCard("SPEC2", CardEffectType.NHA_BI_CUOP, -50);
        AddCoHoiCard("SPEC3", CardEffectType.BONG_DUNG_TRUNG_SO, -100);
        AddCoHoiCard("TRAPHI1", CardEffectType.TRA_TIEN_HOCPHI, -200);
        AddCoHoiCard("TRAPHI2", CardEffectType.DAN_BAN_GAI_DI_CHOI, -250);
        AddCoHoiCard("TRAPHI3", CardEffectType.CHO_CON_DI_HOC, -150);
        AddCoHoiCard("TU3", CardEffectType.VE_BAO_LANH_RA_TU, -150);
        AddCoHoiCard("XH-GD1", CardEffectType.NHAN_TU_THIEN, 200);
        AddCoHoiCard("XH-GD2", CardEffectType.DONG_TIEN_TU_THIEN, -150);
        AddCoHoiCard("XH-GD3", CardEffectType.DI_DAM_CUOI, -200);

        AddKhiVanCard("DICHUYEN1", CardEffectType.TROLAI_XUATPHAT, 200);
        AddKhiVanCard("DICHUYEN2", CardEffectType.DI_LUI_3_BUOC, 0);
        AddKhiVanCard("DICHUYEN3", CardEffectType.TOI_BENXE_GANNHAT, 0);
        AddKhiVanCard("NHANTHUONG1", CardEffectType.TRUNG_XOSO, 150);
        AddKhiVanCard("NHANTHUONG2", CardEffectType.HOANTHU_CUOINAM, 120);
        AddKhiVanCard("NHANTHUONG3", CardEffectType.DAUTU_THANHCONG, 150);
        AddKhiVanCard("SPEC1", CardEffectType.GAP_SUCO_BO1LUOT, 0);
        AddKhiVanCard("SPEC2", CardEffectType.MATGIAY_KO_MUA_DAT_LUOTKE, 0);
        AddKhiVanCard("SPEC3", CardEffectType.CHON_MUA_O_DAT_GIAM50PHANTRAM, 0);
        AddKhiVanCard("TRAPHI1", CardEffectType.TRA_MOI_NGUOI, -50);
        AddKhiVanCard("TRAPHI2", CardEffectType.TO_CHUC_TIEC, -150);
        AddKhiVanCard("TRAPHI3", CardEffectType.MUA_QUA_LUU_NIEM, -50);
        AddKhiVanCard("TU1", CardEffectType.NOP_PHAT_GIAOTHONG, -100);
        AddKhiVanCard("TU2", CardEffectType.THE_RA_TU_MIENPHI, 0);
        AddKhiVanCard("TU3", CardEffectType.BI_LUA_DAO, -80);
        AddKhiVanCard("MATTIEN1", CardEffectType.THUA_KIEN, -100);
        AddKhiVanCard("MATTIEN2", CardEffectType.SUA_XE, -150);
        AddKhiVanCard("MATTIEN3", CardEffectType.DI_DAM_CUOI, -200);

        Debug.Log($"✅ Loaded {coHoiCards.Count} Cơ Hội cards & {khiVanCards.Count} Khí Vận cards");
    }

    void AddCoHoiCard(string spriteName, CardEffectType effect, int money)
    {
        var sprite = LoadSprite($"CoHoiCard/{spriteName}");
        if (sprite != null)
            coHoiCards.Add(new CardData { name = spriteName, sprite = sprite, effect = effect, moneyAmount = money });
        else
            Debug.LogWarning($"Không tìm thấy sprite: CoHoiCard/{spriteName}");
    }

    void AddKhiVanCard(string spriteName, CardEffectType effect, int money)
    {
        var sprite = LoadSprite($"KhiVanCard/{spriteName}");
        if (sprite != null)
            khiVanCards.Add(new CardData { name = spriteName, sprite = sprite, effect = effect, moneyAmount = money });
        else
            Debug.LogWarning($"Không tìm thấy sprite: KhiVanCard/{spriteName}");
    }

    Sprite LoadSprite(string path)
    {
        return Resources.Load<Sprite>("Sprites/" + path);
    }

    public void DrawCoHoiCard()
    {
        if (coHoiCards.Count == 0 || isFlipping) return;
        StartCoroutine(PlayShuffleAndShowCard(coHoiCards, "Cơ Hội"));
    }

    public void DrawKhiVanCard()
    {
        if (khiVanCards.Count == 0 || isFlipping) return;
        StartCoroutine(PlayShuffleAndShowCard(khiVanCards, "Khí Vận"));
    }

    IEnumerator PlayShuffleAndShowCard(List<CardData> cards, string cardType)
    {
        isFlipping = true;

        drawCoHoiButton.SetActive(false);
        drawKhiVanButton.SetActive(false);

        cardsBackPanel.SetActive(true);
        cardsBackAnimator.SetTrigger("Shuffle");

        yield return new WaitForSeconds(1f);
        cardsBackPanel.SetActive(false);

        yield return new WaitForSeconds(0.5f);

        int index = Random.Range(0, cards.Count);
        CardData pickedCard = cards[index];

        cardDisplay.sprite = pickedCard.sprite;
        cardPanel.SetActive(true);

        Debug.Log($"🎉 Bạn rút được thẻ {cardType}: {pickedCard.name} - Hiệu ứng: {pickedCard.effect}");

        ApplyCardEffect(pickedCard);

        isFlipping = false;
    }

    void ApplyCardEffect(CardData card)
    {
        if (card.moneyAmount != 0)
        {
            if (card.moneyAmount > 0)
                Debug.Log($"💰 Nhận {card.moneyAmount} tiền");
            else
                Debug.Log($"💸 Mất {-card.moneyAmount} tiền");
        }
        else
        {
            switch (card.effect)
            {
                case CardEffectType.TROLAI_XUATPHAT:
                    Debug.Log("🔁 Về ô xuất phát!");
                    break;
                case CardEffectType.DI_LUI_3_BUOC:
                    Debug.Log("🔙 Lùi 3 bước!");
                    break;
                case CardEffectType.TOI_CONGTY_GANNHAT:
                    Debug.Log("🏢 Đến công ty gần nhất!");
                    break;
                case CardEffectType.TOI_BENXE_GANNHAT:
                    Debug.Log("🚌 Đến bến xe gần nhất!");
                    break;
                case CardEffectType.VAO_TU:
                    Debug.Log("🚓 Vào tù!");
                    break;
                case CardEffectType.THE_RA_TU_MIENPHI:
                    Debug.Log("🆓 Có thẻ ra tù miễn phí!");
                    break;
                case CardEffectType.GAP_SUCO_BO1LUOT:
                    Debug.Log("⚠️ Gặp sự cố - bỏ lượt tiếp theo!");
                    break;
                case CardEffectType.MATGIAY_KO_MUA_DAT_LUOTKE:
                    Debug.Log("📄 Mất giấy - không được mua đất lượt kế!");
                    break;
                case CardEffectType.CHON_MUA_O_DAT_GIAM50PHANTRAM:
                    Debug.Log("🏷 Được chọn 1 lô đất giảm 50% giá!");
                    break;
                default:
                    Debug.Log("✨ Hiệu ứng chưa được xử lý rõ.");
                    break;
            }
        }
    }

    public void CloseCardPanel()
    {
        cardPanel.SetActive(false);
        drawCoHoiButton.SetActive(true);
        drawKhiVanButton.SetActive(true);
    }
}