using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Web.Helpers;
using System.Web.Hosting;
using System.Globalization;
using System.Net;
using Microsoft.SqlServer.Server;
public class XL_CHUC_NANG
{
    public string Ten, Ma_so;
}


// ********************* Đối tượng Dữ liệu ****************
public partial class XL_DU_LIEU
{
    public List<XL_THANH_VIEN> Danh_sach_Thanh_vien = new List<XL_THANH_VIEN>();
    public List<XL_TAI_KHOAN> Danh_sach_Tai_khoan = new List<XL_TAI_KHOAN>();
    public List<XL_GIA_DINH> Danh_sach_Gia_dinh = new List<XL_GIA_DINH>();
}

// =========== Đối tượng Con người ====================

public class XL_NGUOI_DUNG
{
    public string Ho_ten, Ma_so = "", Ten_Dang_nhap; // Ma_so
    // Khoản thu chi riêng của người này.
    public List<XL_KHOAN_CHI> Danh_sach_Khoan_chi_Rieng = new List<XL_KHOAN_CHI>();
    public List<XL_KHOAN_THU> Danh_sach_Khoan_thu_Rieng = new List<XL_KHOAN_THU>();

    // Chức năng
    public XL_CHUC_NANG Chuc_nang_Khoi_dong_MH_Chinh = new XL_CHUC_NANG()
    {
        Ten = "Khởi động",
        Ma_so = "KHOI_DONG_MH_CHINH"
    };
    public XL_CHUC_NANG Chuc_nang_Tra_cuu_Khoan_thu = new XL_CHUC_NANG()
    {
        Ten = "Tra cứu khoản thu",
        Ma_so = "TRA_CUU_KHOAN_THU"
    };
    public XL_CHUC_NANG Chuc_nang_Tra_cuu_Khoan_chi = new XL_CHUC_NANG()
    {
        Ten = "Tra cứu khoản chi",
        Ma_so = "TRA_CUU_KHOAN_CHI"
    };
    public XL_CHUC_NANG Chuc_nang_Cap_nhat_Khoan_thu = new XL_CHUC_NANG()
    {
        Ten = "Cập nhật",
        Ma_so = "CAP_NHAT_KHOAN_THU"
    };
    public XL_CHUC_NANG Chuc_nang_Cap_nhat_Khoan_chi = new XL_CHUC_NANG()
    {
        Ten = "Cập nhật",
        Ma_so = "CAP_NHAT_KHOAN_CHI"
    };
    public XL_CHUC_NANG Chuc_nang_Them_Khoan_thu = new XL_CHUC_NANG()
    {
        Ten = "Thêm khoản thu",
        Ma_so = "THEM_KHOAN_THU"
    };
    public XL_CHUC_NANG Chuc_nang_Them_Khoan_chi = new XL_CHUC_NANG()
    {
        Ten = "Thêm khoản chi",
        Ma_so = "THEM_KHOAN_CHI"
    };
    public XL_CHUC_NANG Chuc_nang_Xoa_Khoan_thu = new XL_CHUC_NANG()
    {
        Ten = "Xóa",
        Ma_so = "XOA_KHOAN_THU"
    };
    public XL_CHUC_NANG Chuc_nang_Xoa_Khoan_chi = new XL_CHUC_NANG()
    {
        Ten = "Xóa",
        Ma_so = "XOA_KHOAN_CHI"
    };
    public XL_CHUC_NANG Thong_ke_thu = new XL_CHUC_NANG() { 
        Ten = "Thống kê thu",
        Ma_so = "THONG_KE_THU"
    };
    public XL_CHUC_NANG Thong_ke_chi = new XL_CHUC_NANG()
    {
        Ten = "Thống kê chi",
        Ma_so = "THONG_KE_CHI"
    };
    public XL_CHUC_NANG Chuc_nang_Dang_nhap = new XL_CHUC_NANG()
    {
        Ten = "Đăng nhập",
        Ma_so = "DANG_NHAP"
    };
    public XL_CHUC_NANG Chuc_nang_Tra_cuu_Chung = new XL_CHUC_NANG()
    {
        Ten = "Tra cứu",
        Ma_so = "TRA_CUU_CHUNG"
    };
    public XL_CHUC_NANG Chuc_nang_Dang_xuat = new XL_CHUC_NANG()
    {
        Ten = "Đăng xuất",
        Ma_so = "DANG_XUAT"
    };

    // Online (cái này để làm gì vậy?) (à chắc là để xử lý giao diện)
    public List<XL_KHOAN_CHI> Danh_sach_Khoan_chi_Xem = new List<XL_KHOAN_CHI>();
    public List<XL_KHOAN_CHI> Danh_sach_Khoan_chi_Chon = new List<XL_KHOAN_CHI>();
    public List<XL_KHOAN_THU> Danh_sach_Khoan_thu_Xem = new List<XL_KHOAN_THU>();
    public List<XL_KHOAN_THU> Danh_sach_Khoan_thu_Chon = new List<XL_KHOAN_THU>();
}

public class XL_THANH_VIEN
{
    public string Ho_ten, Ma_so = "";
    public string Ngay_sinh;
    public XL_GIOI_TINH Gioi_tinh = new XL_GIOI_TINH();
    public string Hinh;
}

public class XL_TAI_KHOAN
{
    public string Ma_so = "", Ten_Dang_nhap, Mat_khau;
    public List<XL_KHOAN_THU> Danh_sach_Khoan_thu = new List<XL_KHOAN_THU>();
    public List<XL_KHOAN_CHI> Danh_sach_Khoan_chi = new List<XL_KHOAN_CHI>();
    public XL_THANH_VIEN Thong_tin_Thanh_vien = new XL_THANH_VIEN();
}

public class XL_GIOI_TINH
{
    public string Ma_so = "", Ten;
}

// ***************** Đối tượng Xử lý Chính *******************
public class XL_KHOAN_THU
{
    public string Ma_so;
    public string Ngay_thu;
    public long So_tien;

}

public class XL_KHOAN_CHI
{
    public string Ma_so;
    public string Ngay_chi;
    public long So_tien;

}

public class XL_GIA_DINH
{
    public string Ma_so = "";
    public string Ten;
    public List<string> Danh_sach_Ma_tai_khoan = new List<string>();
}
