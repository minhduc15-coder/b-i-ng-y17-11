using System.Data;
using RetailSalesSystem.BUS;
using RetailSalesSystem.DAL;
using RetailSalesSystem.DTO;

namespace RetailSalesSystem.GUI;

public class MainForm : Form
{
    private readonly string _connectionString;\n    private readonly DanhMucBUS _catalog;
    private readonly BanHangBUS _sales;
    private readonly DataGridView grid = new();
    private readonly TextBox txtSearch = new();
    private readonly TextBox txtId = new();
    private readonly TextBox txtName = new();
    private readonly TextBox txtPrice = new();
    private readonly TextBox txtStock = new();
    private readonly ComboBox cboCategory = new();
    private readonly DataGridView cartGrid = new();
    private readonly ComboBox cboEmployee = new();
    private readonly ComboBox cboCustomer = new();
    private readonly ComboBox cboProduct = new();
    private readonly NumericUpDown numQuantity = new();
    private readonly Label lblTotal = new();
    private readonly Label lblRevenue = new();
    private readonly DataGridView reportGrid = new();
    private string currentCategory = "SanPham";
    private bool editing = false;
    private readonly List<GioHangItemDTO> cart = new();

    public MainForm(string connectionString)
    {
        var db = new Db(connectionString);
        _catalog = new DanhMucBUS(new DanhMucDAL(db));
        _sales = new BanHangBUS(new BanHangDAL(db));
        Text = "RETAIL SALES SYSTEM - QUAN LY BAN HANG";
        Width = 1150; Height = 760; StartPosition = FormStartPosition.CenterScreen;
        BuildUI();
        LoadCategory();
    }

    private void BuildUI()
    {
        var tabs = new TabControl { Dock = DockStyle.Fill };
        var tabCatalog = new TabPage("Quản lý danh mục");
        var tabSales = new TabPage("Bán hàng / Hóa đơn");
        var tabReport = new TabPage("Báo cáo - Thống kê");
        tabs.TabPages.AddRange(new[] { tabCatalog, tabSales, tabReport });
        Controls.Add(tabs);

        var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 42, Padding = new Padding(8) };
        cboCategory.DropDownStyle = ComboBoxStyle.DropDownList; cboCategory.Width = 160;
        cboCategory.Items.AddRange(new object[] { "Sản phẩm", "Khách hàng", "Nhân viên" });
        cboCategory.SelectedIndex = 0;
        cboCategory.SelectedIndexChanged += (_, _) => { currentCategory = cboCategory.SelectedIndex == 0 ? "SanPham" : cboCategory.SelectedIndex == 1 ? "KhachHang" : "NhanVien"; LoadCategory(); };
        txtSearch.Width = 220; txtSearch.PlaceholderText = "Tìm sản phẩm theo mã/tên";
        var btnSearch = new Button { Text = "Tìm kiếm", AutoSize = true }; btnSearch.Click += (_, _) => Search();
        top.Controls.AddRange(new Control[] { new Label { Text = "Danh mục:", AutoSize = true, Padding = new Padding(0,8,0,0) }, cboCategory, txtSearch, btnSearch });
        tabCatalog.Controls.Add(grid); grid.Dock = DockStyle.Fill; grid.ReadOnly = true; grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect; grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        tabCatalog.Controls.Add(top);

        var editor = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 105, Padding = new Padding(8) };
        txtId.Width=100; txtName.Width=180; txtPrice.Width=110; txtStock.Width=90;
        editor.Controls.AddRange(new Control[] {
            new Label { Text="Mã",AutoSize=true },txtId,new Label { Text="Tên",AutoSize=true },txtName,
            new Label { Text="Giá",AutoSize=true },txtPrice,new Label { Text="Tồn/Điểm",AutoSize=true },txtStock,
            MakeButton("Thêm", AddRecord), MakeButton("Sửa", UpdateRecord), MakeButton("Xóa", DeleteRecord), MakeButton("Làm mới", ClearEditor)
        });
        tabCatalog.Controls.Add(editor);
        grid.SelectionChanged += (_, _) => FillEditor();

        var saleTop = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 85, Padding = new Padding(8) };
        cboEmployee.Width=170; cboEmployee.DropDownStyle=ComboBoxStyle.DropDownList;
        cboCustomer.Width=170; cboCustomer.DropDownStyle=ComboBoxStyle.DropDownList;
        cboProduct.Width=260; cboProduct.DropDownStyle=ComboBoxStyle.DropDownList;
        numQuantity.Minimum=1; numQuantity.Maximum=100000; numQuantity.Width=70;
        saleTop.Controls.AddRange(new Control[] {
            new Label {Text="Nhân viên",AutoSize=true},cboEmployee,
            new Label {Text="Khách hàng (để trống = khách lẻ)",AutoSize=true},cboCustomer,
            new Label {Text="Sản phẩm",AutoSize=true},cboProduct,
            new Label {Text="SL",AutoSize=true},numQuantity,MakeButton("Thêm vào giỏ", AddToCart)
        });
        tabSales.Controls.Add(cartGrid); cartGrid.Dock=DockStyle.Fill; cartGrid.ReadOnly=true; cartGrid.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill;
        tabSales.Controls.Add(saleTop);
        var saleBottom = new FlowLayoutPanel { Dock=DockStyle.Bottom, Height=55, Padding=new Padding(8) };
        lblTotal.AutoSize=true; lblTotal.Text="Tổng tiền: 0 đ"; lblTotal.Padding=new Padding(10);
        saleBottom.Controls.AddRange(new Control[] { lblTotal,MakeButton("Xóa giỏ", ClearCart),MakeButton("Thanh toán", Checkout) });
        tabSales.Controls.Add(saleBottom);

        var reportPanel = new FlowLayoutPanel { Dock=DockStyle.Top, Height=60, Padding=new Padding(8) };
        reportPanel.Controls.Add(MakeButton("Cập nhật thống kê", LoadReports));
        reportPanel.Controls.Add(lblRevenue);
        tabReport.Controls.Add(reportGrid); reportGrid.Dock=DockStyle.Fill; reportGrid.ReadOnly=true; reportGrid.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill;
        tabReport.Controls.Add(reportPanel);

        tabs.SelectedIndexChanged += (_, _) => { if (tabs.SelectedTab == tabSales) LoadSaleCombos(); if (tabs.SelectedTab == tabReport) LoadReports(); };
    }

    private Button MakeButton(string text, Action action)
    {
        var b = new Button { Text=text, AutoSize=true, Margin=new Padding(5) };
        b.Click += (_, _) => Safe(action); return b;
    }
    private void Safe(Action action)
    {
        try { action(); }
        catch(Exception ex) { MessageBox.Show(ex.Message,"Thông báo",MessageBoxButtons.OK,MessageBoxIcon.Warning); }
    }
    private void LoadCategory()
    {
        grid.DataSource = _catalog.GetTable(currentCategory);
        txtSearch.Visible = currentCategory == "SanPham";
        if (currentCategory == "SanPham") txtSearch.PlaceholderText = "Tìm theo mã hoặc tên";
        else txtSearch.Text = "";
        editing = false;
    }
    private void Search()
    {
        if (currentCategory == "SanPham") grid.DataSource = _catalog.SearchProducts(txtSearch.Text.Trim());
        else LoadCategory();
    }
    private void FillEditor()
    {
        if (grid.CurrentRow == null) return;
        try
        {
            if (currentCategory=="SanPham")
            {
                txtId.Text=Convert.ToString(grid.CurrentRow.Cells["MaSP"].Value);
                txtName.Text=Convert.ToString(grid.CurrentRow.Cells["TenSP"].Value);
                txtPrice.Text=Convert.ToString(grid.CurrentRow.Cells["GiaBan"].Value);
                txtStock.Text=Convert.ToString(grid.CurrentRow.Cells["SoLuongTon"].Value);
            }
            else if(currentCategory=="KhachHang")
            {
                txtId.Text=Convert.ToString(grid.CurrentRow.Cells["MaKH"].Value);
                txtName.Text=Convert.ToString(grid.CurrentRow.Cells["TenKH"].Value);
                txtPrice.Text=Convert.ToString(grid.CurrentRow.Cells["DienThoai"].Value);
                txtStock.Text=Convert.ToString(grid.CurrentRow.Cells["DiemTichLuy"].Value);
            }
            else
            {
                txtId.Text=Convert.ToString(grid.CurrentRow.Cells["MaNV"].Value);
                txtName.Text=Convert.ToString(grid.CurrentRow.Cells["TenNV"].Value);
                txtPrice.Text=Convert.ToString(grid.CurrentRow.Cells["SoDienThoai"].Value);
                txtStock.Text=Convert.ToString(grid.CurrentRow.Cells["ChucVu"].Value);
            }
            editing=true;
        }
        catch { }
    }
    private void AddRecord()
    {
        if(currentCategory=="SanPham")
            _catalog.SaveProduct(new SanPhamDTO { MaSP=txtId.Text.Trim(),TenSP=txtName.Text.Trim(),GiaBan=decimal.Parse(txtPrice.Text),SoLuongTon=int.Parse(txtStock.Text) },true);
        else if(currentCategory=="KhachHang")
            _catalog.SaveCustomer(new KhachHangDTO { TenKH=txtName.Text.Trim(),DienThoai=NullIfEmpty(txtPrice.Text),DiemTichLuy=int.Parse(txtStock.Text) },true);
        else
            _catalog.SaveEmployee(new NhanVienDTO { TenNV=txtName.Text.Trim(),SoDienThoai=NullIfEmpty(txtPrice.Text),ChucVu=NullIfEmpty(txtStock.Text) },true);
        LoadCategory(); ClearEditor(); MessageBox.Show("Thêm dữ liệu thành công.");
    }
    private void UpdateRecord()
    {
        if(!editing) throw new Exception("Hãy chọn dòng cần sửa.");
        if(currentCategory=="SanPham")
            _catalog.SaveProduct(new SanPhamDTO { MaSP=txtId.Text.Trim(),TenSP=txtName.Text.Trim(),GiaBan=decimal.Parse(txtPrice.Text),SoLuongTon=int.Parse(txtStock.Text) },false);
        else if(currentCategory=="KhachHang")
            _catalog.SaveCustomer(new KhachHangDTO { MaKH=int.Parse(txtId.Text),TenKH=txtName.Text.Trim(),DienThoai=NullIfEmpty(txtPrice.Text),DiemTichLuy=int.Parse(txtStock.Text) },false);
        else
            _catalog.SaveEmployee(new NhanVienDTO { MaNV=int.Parse(txtId.Text),TenNV=txtName.Text.Trim(),SoDienThoai=NullIfEmpty(txtPrice.Text),ChucVu=NullIfEmpty(txtStock.Text) },false);
        LoadCategory(); MessageBox.Show("Cập nhật thành công.");
    }
    private void DeleteRecord()
    {
        if(!editing) throw new Exception("Hãy chọn dòng cần xóa.");
        if(MessageBox.Show("Bạn có chắc muốn xóa?","Xác nhận",MessageBoxButtons.YesNo)!=DialogResult.Yes)return;
        if(currentCategory=="SanPham") _catalog.DeleteProduct(txtId.Text.Trim());
        else if(currentCategory=="KhachHang") _catalog.DeleteCustomer(int.Parse(txtId.Text));
        else _catalog.DeleteEmployee(int.Parse(txtId.Text));
        LoadCategory(); ClearEditor();
    }
    private static string? NullIfEmpty(string s)=>string.IsNullOrWhiteSpace(s)?null:s.Trim();
    private void ClearEditor()
    {
        txtId.Clear();txtName.Clear();txtPrice.Clear();txtStock.Clear();editing=false;grid.ClearSelection();
    }
    private void LoadSaleCombos()
    {
        var emp=_catalog.GetEmployees(); cboEmployee.DataSource=emp;cboEmployee.DisplayMember="TenNV";cboEmployee.ValueMember="MaNV";
        var cust=_catalog.GetCustomers(); var blank=cust.NewRow();blank["MaKH"]=DBNull.Value;blank["TenKH"]="Khách lẻ";cust.Rows.InsertAt(blank,0);
        cboCustomer.DataSource=cust;cboCustomer.DisplayMember="TenKH";cboCustomer.ValueMember="MaKH";
        var products=new BanHangDAL(new Db(GetConnectionString())).GetProductsForSale();
        cboProduct.DataSource=products;cboProduct.DisplayMember="TenSP";cboProduct.ValueMember="MaSP";
    }
    private string GetConnectionString()
    {
        // Read from the same setting used by Program.cs
        return System.Configuration.ConfigurationManager.ConnectionStrings["SalesDB"]?.ConnectionString
            ?? "Data Source=.;Initial Catalog=SalesDB;Integrated Security=True;TrustServerCertificate=True";
    }
    private void AddToCart()
    {
        if(cboProduct.SelectedItem is not DataRowView row) throw new Exception("Chưa có sản phẩm để chọn.");
        string id=Convert.ToString(row["MaSP"])!;
        int qty=(int)numQuantity.Value;
        int stock=Convert.ToInt32(row["SoLuongTon"]);
        if(qty>stock) throw new Exception("Số lượng vượt tồn kho hiện tại.");
        var existing=cart.FirstOrDefault(x=>x.MaSP==id);
        if(existing!=null)
        {
            if(existing.SoLuong+qty>stock) throw new Exception("Tổng số lượng trong giỏ vượt tồn kho.");
            existing.SoLuong+=qty;
        }
        else cart.Add(new GioHangItemDTO{MaSP=id,TenSP=Convert.ToString(row["TenSP"])!,SoLuong=qty,DonGia=Convert.ToDecimal(row["GiaBan"])});
        RefreshCart();
    }
    private void RefreshCart()
    {
        cartGrid.DataSource=null;cartGrid.DataSource=cart.Select(x=>new {x.MaSP,x.TenSP,x.SoLuong,x.DonGia,ThanhTien=x.ThanhTien}).ToList();
        lblTotal.Text=$"Tổng tiền: {cart.Sum(x=>x.ThanhTien):N0} đ";
    }
    private void ClearCart(){cart.Clear();RefreshCart();}
    private void Checkout()
    {
        if(cboEmployee.SelectedValue==null) throw new Exception("Cần có nhân viên trước khi thanh toán.");
        int maNV=Convert.ToInt32(cboEmployee.SelectedValue);
        int? maKH=null;
        if(cboCustomer.SelectedValue!=null && cboCustomer.SelectedValue!=DBNull.Value) maKH=Convert.ToInt32(cboCustomer.SelectedValue);
        int invoice=_sales.Checkout(maNV,maKH,cart);
        MessageBox.Show($"Thanh toán thành công. Mã hóa đơn: {invoice}","Hoàn tất");
        ClearCart();LoadSaleCombos();
    }
    private void LoadReports()
    {
        decimal today=_sales.GetRevenueToday(),month=_sales.GetRevenueMonth();
        lblRevenue.Text=$"Doanh thu hôm nay: {today:N0} đ     |     Tháng này: {month:N0} đ";
        reportGrid.DataSource=_sales.GetTop5();
    }
}
