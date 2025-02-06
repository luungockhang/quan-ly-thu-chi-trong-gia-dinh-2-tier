using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

// Mục đích của file này là để định nghĩa các đối tượng

// ********* Đối tượng Con người ***********

// Tài khoản.
// Trong trường hợp các thành viên trong gia đình sử dụng tài khoản riêng
// để tự nhập thu chi của mình. Nếu chỉ có một người nhập hết cho tất cả
// thì sẽ không cần tới đối tượng TK này. Nhưng dư còn hơn thiếu.

public class XL_TAI_KHOAN
{
    public string Ma_so = "", Ten_Dang_nhap, Mat_khau;
    public List<XL_KHOAN_THU> Danh_sach_Khoan_thu = new List<XL_KHOAN_THU>();
    public List<XL_KHOAN_CHI> Danh_sach_Khoan_chi = new List<XL_KHOAN_CHI>();
    public XL_THANH_VIEN Thong_tin_Thanh_vien = new XL_THANH_VIEN();
}

// Người dùng.
// Quản lý thông tin người dùng trong tài khoản.
public class XL_THANH_VIEN
{
    public string Ho_ten, Ma_so = "";
    public XL_GIOI_TINH Gioi_tinh = new XL_GIOI_TINH();
    public string Ngay_sinh;      // Ngày sinh định dạng datetime
    public string Hinh;             // Lưu địa chỉ hình trong dữ liệu
}


// ********* Đối tượng Sự vật 
// Khoản thu và chi.
public class XL_KHOAN_THU
{
    public string Ma_so = "";
    public string Ngay_thu;
    public long So_Tien;
}

public class XL_KHOAN_CHI
{
    public string Ma_so = "";
    public string Ngay_chi;
    public long So_Tien;
}

// ********* Đối tượng Tổ chức *************
// Trong trường hợp có nhiều gia đình sử dụng một ứng dụng này thì cần quản lý ai là người thuộc gia đình nào.
public class XL_GIA_DINH
{
    public string Ma_so = "";
    public string Ten;
    public List<string> Danh_sach_Ma_tai_khoan = new List<string>();
    // Quản lý các tài khoản thuộc một gia đình
}

public class XL_GIOI_TINH
{
    public string Ma_so = "", Ten;
}

// ********* Đối tượng Dữ liệu *************
public partial class XL_DU_LIEU
{
    // Now I have these two.
    // Let's try loading them.
    public List<XL_THANH_VIEN> Danh_sach_Thanh_vien = new List<XL_THANH_VIEN>();
    public List<XL_TAI_KHOAN> Danh_sach_Tai_khoan = new List<XL_TAI_KHOAN>();
    public List<XL_GIA_DINH> Danh_sach_Gia_dinh = new List<XL_GIA_DINH>();
}