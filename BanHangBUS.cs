using System.Data;
using RetailSalesSystem.DAL;
using RetailSalesSystem.DTO;

namespace RetailSalesSystem.BUS;
public class BanHangBUS
{
    private readonly BanHangDAL _dal;
    public BanHangBUS(BanHangDAL dal) => _dal = dal;
    public DataTable GetProductsForSale() => _dal.GetProductsForSale();
    public int Checkout(int maNV, int? maKH, List<GioHangItemDTO> cart) => _dal.Checkout(maNV, maKH, cart);
    public decimal GetRevenueToday() => _dal.GetRevenueToday();
    public decimal GetRevenueMonth() => _dal.GetRevenueMonth();
    public DataTable GetTop5() => _dal.GetTop5();
}
