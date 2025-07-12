using UnityEngine;

[System.Serializable]
public class CardData
{
    public Sprite sprite;
    public string name;
    public CardEffectType effect;
    public int moneyAmount;
}

public enum CardEffectType
{
    None,
    TROLAI_XUATPHAT,
    DI_LUI_3_BUOC,
    TOI_CONGTY_GANNHAT,
    TOI_BENXE_GANNHAT,
    VAO_TU,
    THE_RA_TU_MIENPHI,

    DAUTU_THANHCONG,
    TRUNG_XOSO,
    HOANTHU_CUOINAM,
    NHAN_TIEN_TIETKIEM,
    NHAN_HOCBONG,
    THANG_GIAI_TOANTUAN,
    TANG_CA,
    NHAN_TU_THIEN,

    BI_LUA_DAO,
    THUA_KIEN,
    NOP_PHAT_GIAOTHONG,
    TRA_MOI_NGUOI,
    TO_CHUC_TIEC,
    MUA_QUA_LUU_NIEM,
    TRA_TIEN_HOCPHI,
    DAN_BAN_GAI_DI_CHOI,
    CHO_CON_DI_HOC,
    VE_BAO_LANH_RA_TU,
    DI_DAM_CUOI,
    NHA_BI_CUOP,
    BONG_DUNG_TRUNG_SO,
    SUA_XE,
    DONG_TIEN_TU_THIEN,

    GAP_SUCO_BO1LUOT,
    MATGIAY_KO_MUA_DAT_LUOTKE,
    CHON_MUA_O_DAT_GIAM50PHANTRAM,
}
