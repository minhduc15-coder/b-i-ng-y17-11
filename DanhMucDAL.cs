using System.Data;
using Microsoft.Data.SqlClient;
using RetailSalesSystem.DTO;

namespace RetailSalesSystem.DAL;

public class DanhMucDAL
{
    private readonly Db _db;
    public DanhMucDAL(Db db) => _db = db;

    public DataTable GetTable(string table)
    {
        string sql = table switch
        {
            "SanPham" => "SELECT MaSP,TenSP,GiaBan,SoLuongTon FROM SanPham ORDER BY MaSP",
            "KhachHang" => "SELECT MaKH,TenKH,DienThoai,DiemTichLuy FROM KhachHang ORDER BY MaKH",
            "NhanVien" => "SELECT MaNV,TenNV,SoDienThoai,ChucVu FROM NhanVien ORDER BY MaNV",
            _ => throw new ArgumentException("Danh mục không hợp lệ.")
        };
        using var cn = _db.CreateConnection();
        using var da = new SqlDataAdapter(sql, cn);
        var dt = new DataTable();
        da.Fill(dt);
        return dt;
    }

    public DataTable SearchProducts(string keyword)
    {
        using var cn = _db.CreateConnection();
        using var cmd = new SqlCommand(
            "SELECT MaSP,TenSP,GiaBan,SoLuongTon FROM SanPham WHERE MaSP LIKE @kw OR TenSP LIKE @kw ORDER BY MaSP", cn);
        cmd.Parameters.Add("@kw", SqlDbType.NVarChar, 170).Value = "%" + keyword + "%";
        using var da = new SqlDataAdapter(cmd);
        var dt = new DataTable();
        da.Fill(dt);
        return dt;
    }

    public void SaveProduct(SanPhamDTO p, bool isNew)
    {
        using var cn = _db.CreateConnection(); cn.Open();
        string sql = isNew
            ? "INSERT INTO SanPham(MaSP,TenSP,GiaBan,SoLuongTon) VALUES(@ma,@ten,@gia,@ton)"
            : "UPDATE SanPham SET TenSP=@ten,GiaBan=@gia,SoLuongTon=@ton WHERE MaSP=@ma";
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.Add("@ma", SqlDbType.VarChar, 20).Value = p.MaSP;
        cmd.Parameters.Add("@ten", SqlDbType.NVarChar, 150).Value = p.TenSP;
        cmd.Parameters.Add("@gia", SqlDbType.Decimal).Value = p.GiaBan;
        cmd.Parameters["@gia"].Precision = 18; cmd.Parameters["@gia"].Scale = 2;
        cmd.Parameters.Add("@ton", SqlDbType.Int).Value = p.SoLuongTon;
        cmd.ExecuteNonQuery();
    }

    public void DeleteProduct(string id)
    {
        using var cn = _db.CreateConnection(); cn.Open();
        using var cmd = new SqlCommand("DELETE FROM SanPham WHERE MaSP=@id", cn);
        cmd.Parameters.Add("@id", SqlDbType.VarChar, 20).Value = id;
        cmd.ExecuteNonQuery();
    }

    public void SaveCustomer(KhachHangDTO p, bool isNew)
    {
        using var cn = _db.CreateConnection(); cn.Open();
        string sql = isNew
            ? "INSERT INTO KhachHang(TenKH,DienThoai,DiemTichLuy) VALUES(@ten,@dt,@diem)"
            : "UPDATE KhachHang SET TenKH=@ten,DienThoai=@dt,DiemTichLuy=@diem WHERE MaKH=@id";
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.Add("@ten", SqlDbType.NVarChar, 100).Value = p.TenKH;
        cmd.Parameters.Add("@dt", SqlDbType.VarChar, 15).Value = (object?)p.DienThoai ?? DBNull.Value;
        cmd.Parameters.Add("@diem", SqlDbType.Int).Value = p.DiemTichLuy;
        if (!isNew) cmd.Parameters.Add("@id", SqlDbType.Int).Value = p.MaKH;
        cmd.ExecuteNonQuery();
    }

    public void DeleteCustomer(int id)
    {
        using var cn = _db.CreateConnection(); cn.Open();
        using var cmd = new SqlCommand("DELETE FROM KhachHang WHERE MaKH=@id", cn);
        cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
        cmd.ExecuteNonQuery();
    }

    public void SaveEmployee(NhanVienDTO p, bool isNew)
    {
        using var cn = _db.CreateConnection(); cn.Open();
        string sql = isNew
            ? "INSERT INTO NhanVien(TenNV,SoDienThoai,ChucVu) VALUES(@ten,@dt,@cv)"
            : "UPDATE NhanVien SET TenNV=@ten,SoDienThoai=@dt,ChucVu=@cv WHERE MaNV=@id";
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.Add("@ten", SqlDbType.NVarChar, 100).Value = p.TenNV;
        cmd.Parameters.Add("@dt", SqlDbType.VarChar, 15).Value = (object?)p.SoDienThoai ?? DBNull.Value;
        cmd.Parameters.Add("@cv", SqlDbType.NVarChar, 50).Value = (object?)p.ChucVu ?? DBNull.Value;
        if (!isNew) cmd.Parameters.Add("@id", SqlDbType.Int).Value = p.MaNV;
        cmd.ExecuteNonQuery();
    }

    public void DeleteEmployee(int id)
    {
        using var cn = _db.CreateConnection(); cn.Open();
        using var cmd = new SqlCommand("DELETE FROM NhanVien WHERE MaNV=@id", cn);
        cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
        cmd.ExecuteNonQuery();
    }

    public DataTable GetEmployees()
    {
        using var cn = _db.CreateConnection();
        using var da = new SqlDataAdapter("SELECT MaNV,TenNV FROM NhanVien ORDER BY TenNV", cn);
        var dt = new DataTable(); da.Fill(dt); return dt;
    }
    public DataTable GetCustomers()
    {
        using var cn = _db.CreateConnection();
        using var da = new SqlDataAdapter("SELECT MaKH,TenKH FROM KhachHang ORDER BY TenKH", cn);
        var dt = new DataTable(); da.Fill(dt); return dt;
    }
}
