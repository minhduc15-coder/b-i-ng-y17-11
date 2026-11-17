using System.Data;
using Microsoft.Data.SqlClient;
using RetailSalesSystem.DTO;

namespace RetailSalesSystem.DAL;

public class BanHangDAL
{
    private readonly Db _db;
    public BanHangDAL(Db db) => _db = db;

    public DataTable GetProductsForSale()
    {
        using var cn = _db.CreateConnection();
        using var da = new SqlDataAdapter("SELECT MaSP,TenSP,GiaBan,SoLuongTon FROM SanPham WHERE SoLuongTon>0 ORDER BY TenSP", cn);
        var dt = new DataTable(); da.Fill(dt); return dt;
    }

    public int Checkout(int maNV, int? maKH, List<GioHangItemDTO> cart)
    {
        if (cart.Count == 0) throw new InvalidOperationException("Giỏ hàng đang trống.");
        using var cn = _db.CreateConnection(); cn.Open();
        using var trans = cn.BeginTransaction(IsolationLevel.Serializable);
        try
        {
            decimal total = cart.Sum(x => x.ThanhTien);
            int maHD;
            using (var cmd = new SqlCommand(
                "INSERT INTO HoaDon(MaNV,MaKH,TongTien) VALUES(@nv,@kh,@tong); SELECT CAST(SCOPE_IDENTITY() AS INT);", cn, trans))
            {
                cmd.Parameters.Add("@nv", SqlDbType.Int).Value = maNV;
                cmd.Parameters.Add("@kh", SqlDbType.Int).Value = (object?)maKH ?? DBNull.Value;
                var p = cmd.Parameters.Add("@tong", SqlDbType.Decimal); p.Precision = 18; p.Scale = 2; p.Value = total;
                maHD = (int)cmd.ExecuteScalar()!;
            }

            foreach (var item in cart)
            {
                using var stock = new SqlCommand(
                    "UPDATE SanPham SET SoLuongTon=SoLuongTon-@sl WHERE MaSP=@ma AND SoLuongTon>=@sl", cn, trans);
                stock.Parameters.Add("@sl", SqlDbType.Int).Value = item.SoLuong;
                stock.Parameters.Add("@ma", SqlDbType.VarChar, 20).Value = item.MaSP;
                if (stock.ExecuteNonQuery() != 1)
                    throw new InvalidOperationException($"Sản phẩm {item.MaSP} không đủ tồn kho hoặc không tồn tại.");

                using var detail = new SqlCommand(
                    "INSERT INTO ChiTietHoaDon(MaHD,MaSP,SoLuong,DonGia) VALUES(@hd,@sp,@sl,@gia)", cn, trans);
                detail.Parameters.Add("@hd", SqlDbType.Int).Value = maHD;
                detail.Parameters.Add("@sp", SqlDbType.VarChar, 20).Value = item.MaSP;
                detail.Parameters.Add("@sl", SqlDbType.Int).Value = item.SoLuong;
                var price = detail.Parameters.Add("@gia", SqlDbType.Decimal); price.Precision = 18; price.Scale = 2; price.Value = item.DonGia;
                detail.ExecuteNonQuery();
            }

            if (maKH.HasValue)
            {
                using var points = new SqlCommand(
                    "UPDATE KhachHang SET DiemTichLuy=DiemTichLuy+@diem WHERE MaKH=@kh", cn, trans);
                points.Parameters.Add("@diem", SqlDbType.Int).Value = (int)(total / 100000m);
                points.Parameters.Add("@kh", SqlDbType.Int).Value = maKH.Value;
                points.ExecuteNonQuery();
            }
            trans.Commit();
            return maHD;
        }
        catch
        {
            trans.Rollback();
            throw;
        }
    }

    public decimal GetRevenueToday()
    {
        using var cn = _db.CreateConnection(); cn.Open();
        using var cmd = new SqlCommand("SELECT ISNULL(SUM(TongTien),0) FROM HoaDon WHERE NgayLap>=CONVERT(date,GETDATE()) AND NgayLap<DATEADD(day,1,CONVERT(date,GETDATE()))", cn);
        return Convert.ToDecimal(cmd.ExecuteScalar());
    }

    public decimal GetRevenueMonth()
    {
        using var cn = _db.CreateConnection(); cn.Open();
        using var cmd = new SqlCommand("SELECT ISNULL(SUM(TongTien),0) FROM HoaDon WHERE YEAR(NgayLap)=YEAR(GETDATE()) AND MONTH(NgayLap)=MONTH(GETDATE())", cn);
        return Convert.ToDecimal(cmd.ExecuteScalar());
    }

    public DataTable GetTop5()
    {
        using var cn = _db.CreateConnection();
        const string sql = @"SELECT TOP 5 sp.MaSP,sp.TenSP,SUM(ct.SoLuong) AS TongSoLuong
          FROM ChiTietHoaDon ct JOIN SanPham sp ON sp.MaSP=ct.MaSP
          GROUP BY sp.MaSP,sp.TenSP ORDER BY TongSoLuong DESC";
        using var cmd = new SqlCommand(sql, cn);
        using var reader = cmd.ExecuteReader();
        var dt = new DataTable(); dt.Load(reader); return dt;
    }
}
