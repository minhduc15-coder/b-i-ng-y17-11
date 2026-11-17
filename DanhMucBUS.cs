using System.Data;
using RetailSalesSystem.DAL;
using RetailSalesSystem.DTO;

namespace RetailSalesSystem.BUS;
public class DanhMucBUS
{
    private readonly DanhMucDAL _dal;
    public DanhMucBUS(DanhMucDAL dal) => _dal = dal;
    public DataTable GetTable(string table) => _dal.GetTable(table);
    public DataTable SearchProducts(string keyword) => _dal.SearchProducts(keyword);
    public void SaveProduct(SanPhamDTO p, bool isNew)
    {
        if (string.IsNullOrWhiteSpace(p.MaSP) || string.IsNullOrWhiteSpace(p.TenSP)) throw new Exception("Mã và tên sản phẩm không được để trống.");
        if (p.GiaBan <= 0 || p.SoLuongTon < 0) throw new Exception("Giá phải lớn hơn 0 và tồn kho không âm.");
        _dal.SaveProduct(p, isNew);
    }
    public void DeleteProduct(string id) => _dal.DeleteProduct(id);
    public void SaveCustomer(KhachHangDTO p, bool isNew)
    {
        if (string.IsNullOrWhiteSpace(p.TenKH)) throw new Exception("Tên khách hàng không được để trống.");
        if (p.DiemTichLuy < 0) throw new Exception("Điểm tích lũy không được âm.");
        _dal.SaveCustomer(p, isNew);
    }
    public void DeleteCustomer(int id) => _dal.DeleteCustomer(id);
    public void SaveEmployee(NhanVienDTO p, bool isNew)
    {
        if (string.IsNullOrWhiteSpace(p.TenNV)) throw new Exception("Tên nhân viên không được để trống.");
        _dal.SaveEmployee(p, isNew);
    }
    public void DeleteEmployee(int id) => _dal.DeleteEmployee(id);
    public DataTable GetEmployees() => _dal.GetEmployees();
    public DataTable GetCustomers() => _dal.GetCustomers();
}
