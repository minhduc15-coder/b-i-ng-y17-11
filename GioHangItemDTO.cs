namespace RetailSalesSystem.DTO;
public class GioHangItemDTO
{
    public string MaSP { get; set; } = "";
    public string TenSP { get; set; } = "";
    public int SoLuong { get; set; }
    public decimal DonGia { get; set; }
    public decimal ThanhTien => SoLuong * DonGia;
}
