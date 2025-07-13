using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class CardManager : NetworkBehaviour
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
    private PlayerController currentCardPlayer;

    void Start()
    {
        LoadAllCards();
    }

    void LoadAllCards()
    {
        coHoiCards = new List<CardData>();
        khiVanCards = new List<CardData>();

        // CƠ HỘI (18)
        AddCoHoiCard("DICHUYEN1", CardEffectType.TROLAI_XUATPHAT, 0);
        AddCoHoiCard("DICHUYEN2", CardEffectType.DI_LUI_3_BUOC, 0);
        AddCoHoiCard("DICHUYEN3", CardEffectType.TOI_O_NHA_CAO_NHAT, 0);
        AddCoHoiCard("MATTIEN1", CardEffectType.BI_LUA_DAO, -80);
        AddCoHoiCard("MATTIEN2", CardEffectType.THUA_KIEN, -100);
        AddCoHoiCard("MATTIEN3", CardEffectType.SUA_XE, -150);
        AddCoHoiCard("SPEC1", CardEffectType.GAP_SUCO_BO1LUOT, 0);
        AddCoHoiCard("SPEC2", CardEffectType.MATGIAY_KO_MUA_DAT_LUOTKE, 0);
        AddCoHoiCard("SPEC3", CardEffectType.CHON_MUA_O_DAT_GIAM50PHANTRAM, 0);
        AddCoHoiCard("TRAPHI1", CardEffectType.TRA_MOI_NGUOI, 0);
        AddCoHoiCard("TRAPHI2", CardEffectType.TO_CHUC_TIEC, -150);
        AddCoHoiCard("TRAPHI3", CardEffectType.MUA_QUA_LUU_NIEM, -50);
        AddCoHoiCard("TU1", CardEffectType.NOP_PHAT_GIAOTHONG, -100);
        AddCoHoiCard("TU2", CardEffectType.THE_RA_TU_MIENPHI, 0);
        AddCoHoiCard("TU3", CardEffectType.BI_BAT_GIU_DOT_XUAT, 0);
        AddCoHoiCard("NHANTHUONG1", CardEffectType.DAUTU_THANHCONG, 150);
        AddCoHoiCard("NHANTHUONG2", CardEffectType.TRUNG_XOSO, 150);
        AddCoHoiCard("NHANTHUONG3", CardEffectType.HOANTHUE_CUOINAM, 90);

        // KHÍ VẬN (18)
        AddKhiVanCard("DICHUYEN1", CardEffectType.TROLAI_XUATPHAT, 0);
        AddKhiVanCard("DICHUYEN2", CardEffectType.TOI_BENXE_GANNHAT, 0);
        AddKhiVanCard("DICHUYEN3", CardEffectType.TOI_CONGTY_GANNHAT, 0);
        AddKhiVanCard("TU1", CardEffectType.VAO_TU, 0);
        AddKhiVanCard("TU2", CardEffectType.THE_RA_TU_MIENPHI, 0);
        AddKhiVanCard("TU3", CardEffectType.VE_BAO_LANH_RA_TU, 0);
        AddKhiVanCard("NHANTHUONG1", CardEffectType.THANG_GIAI_TOAN_TU_DUY, 200);
        AddKhiVanCard("NHANTHUONG2", CardEffectType.NHAN_TIEN_TIETKIEM, 100);
        AddKhiVanCard("NHANTHUONG3", CardEffectType.NHAN_HOCBONG, 250);
        AddKhiVanCard("SPEC1", CardEffectType.TANG_CA, 70);
        AddKhiVanCard("SPEC2", CardEffectType.NHA_BI_CUOP, -50);
        AddKhiVanCard("SPEC3", CardEffectType.BONG_DUNG_TRUNG_SO, -100);
        AddKhiVanCard("TRAPHI1", CardEffectType.TRA_TIEN_HOCPHI, -200);
        AddKhiVanCard("TRAPHI2", CardEffectType.DAN_BAN_GAI_DI_CHOI, -250);
        AddKhiVanCard("TRAPHI3", CardEffectType.CHO_CON_DI_HOC, -150);
        AddKhiVanCard("XH-GD1", CardEffectType.NHAN_TU_THIEN, 200);
        AddKhiVanCard("XH-GD2", CardEffectType.DONG_TIEN_TU_THIEN, -150);
        AddKhiVanCard("XH-GD3", CardEffectType.DI_DAM_CUOI, -200);

        Debug.Log($"✅ Loaded {coHoiCards.Count} Cơ Hội cards & {khiVanCards.Count} Khí Vận cards");
    }

    void AddCoHoiCard(string spriteName, CardEffectType effect, int money)
    {
        var sprite = LoadSprite($"CoHoiCard/{spriteName}");
        if (sprite != null)
            coHoiCards.Add(new CardData { name = spriteName, sprite = sprite, effect = effect, moneyAmount = money });
        else
            Debug.LogWarning($"❌ Không tìm thấy sprite: CoHoiCard/{spriteName}");
    }

    // Hàm thêm thẻ Khí Vận
    void AddKhiVanCard(string spriteName, CardEffectType effect, int money)
    {
        var sprite = LoadSprite($"KhiVanCard/{spriteName}");
        if (sprite != null)
            khiVanCards.Add(new CardData { name = spriteName, sprite = sprite, effect = effect, moneyAmount = money });
        else
            Debug.LogWarning($"❌ Không tìm thấy sprite: KhiVanCard/{spriteName}");
    }

    // Load sprite từ thư mục Resources/Sprites
    Sprite LoadSprite(string path)
    {
        return Resources.Load<Sprite>("Sprites/" + path);
    }

    // Gọi khi người chơi bấm nút rút thẻ Cơ Hội
    public void DrawCoHoiCard(PlayerController player)
    {
        if (coHoiCards.Count == 0 || isFlipping) return;
        currentCardPlayer = player;
        StartCoroutine(PlayShuffleAndShowCard(coHoiCards, "Cơ Hội"));
    }

    public void DrawKhiVanCard(PlayerController player)
    {
        if (khiVanCards.Count == 0 || isFlipping) return;
        currentCardPlayer = player;
        StartCoroutine(PlayShuffleAndShowCard(khiVanCards, "Khí Vận"));
    }

    // Coroutine xử lý animation lật thẻ, chọn thẻ ngẫu nhiên, và áp dụng hiệu ứng
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
        PlayerController player = currentCardPlayer;

        // Chỉ xử lý tiền ở đây nếu card.moneyAmount khác 0
        if (card.moneyAmount != 0)
        {
            if (card.moneyAmount > 0)
            {
                player.money.Value += card.moneyAmount;
                Debug.Log($"💰 Nhận {card.moneyAmount}$ từ thẻ {card.name}");
            }
            else
            {
                player.TryPayServerRpc(-card.moneyAmount);
                Debug.Log($"💸 Mất {-card.moneyAmount}$ từ thẻ {card.name}");
            }
        }

        // Áp dụng hiệu ứng đặc biệt
        switch (card.effect)
        {
            case CardEffectType.TROLAI_XUATPHAT:
                GameManager.Instance.MovePlayerToTileServerRpc(player.NetworkObject, 0);
                break;

            case CardEffectType.DI_LUI_3_BUOC:
                GameManager.Instance.MovePlayerByStepsServerRpc(player.NetworkObject, -3);
                break;

            case CardEffectType.TOI_CONGTY_GANNHAT:
                GameManager.Instance.MovePlayerToNearestTileWithTagServerRpc(player.NetworkObject, "Company");
                break;

            case CardEffectType.TOI_BENXE_GANNHAT:
                GameManager.Instance.MovePlayerToNearestTileWithTagServerRpc(player.NetworkObject, "Station");
                break;

            case CardEffectType.TOI_O_NHA_CAO_NHAT:
                GameManager.Instance.MovePlayerToMostExpensivePropertyServerRpc(player.NetworkObject);
                break;

            case CardEffectType.VAO_TU:
            case CardEffectType.BI_BAT_GIU_DOT_XUAT:
                GameManager.Instance.MovePlayerToTileServerRpc(player.NetworkObject, GameManager.Instance.jailPosition.GetSiblingIndex());
                player.inJail.Value = true;
                player.jailTurns.Value = 3;
                break;

            case CardEffectType.THE_RA_TU_MIENPHI:
                player.hasGetOutOfJailFreeCard.Value = true;
                break;

            case CardEffectType.VE_BAO_LANH_RA_TU:
                player.TryPayServerRpc(150);
                player.GetOutOfJailServerRpc();
                break;

            case CardEffectType.TRA_MOI_NGUOI:
                GameManager.Instance.PayEveryoneServerRpc(player.NetworkObject, 50);
                break;

            case CardEffectType.GAP_SUCO_BO1LUOT:
                player.skipNextTurn.Value = true;
                break;

            case CardEffectType.MATGIAY_KO_MUA_DAT_LUOTKE:
                player.cannotBuyNextTurn.Value = true;
                break;

            case CardEffectType.CHON_MUA_O_DAT_GIAM50PHANTRAM:
                player.canBuyDiscountProperty.Value = true;
                break;

            default:
                Debug.Log($"✨ Hiệu ứng {card.effect} không có logic đặc biệt hoặc đã được xử lý qua tiền.");
                break;
        }
    }
    public void CloseCardPanel()
    {
        cardPanel.SetActive(false);
        drawCoHoiButton.SetActive(true);
        drawKhiVanButton.SetActive(true);
        currentCardPlayer = null;
    }
}