using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;
using System.Web.Helpers;
using System.Web.Hosting;

// Làm tiếp dịch vụ chính ? cshtml


// ------------ Business Layer ----------------------
public partial class XL_DICH_VU
{
    static XL_DICH_VU Dich_vu = null;

    XL_DU_LIEU Du_lieu_Dich_vu = new XL_DU_LIEU();
    XL_GIA_DINH Gia_dinh = new XL_GIA_DINH();
    List<XL_GIA_DINH> Danh_sach_Gia_dinh = new List<XL_GIA_DINH>();
    List<XL_TAI_KHOAN> Danh_sach_Tai_khoan = new List<XL_TAI_KHOAN>();
    List<XL_THANH_VIEN> Danh_sach_Thanh_vien = new List<XL_THANH_VIEN>();

    public static XL_DICH_VU Khoi_dong_Dich_vu()
    {
        if (Dich_vu == null)
        {
            Dich_vu = new XL_DICH_VU();
            Dich_vu.Khoi_dong_Du_lieu_Dich_vu();
        }

        return Dich_vu;

    }

    public void Khoi_dong_Du_lieu_Dich_vu()
    {
        var Du_lieu_Luu_tru = XL_DU_LIEU.Doc_Du_lieu_Luu_tru();
        Du_lieu_Dich_vu = Du_lieu_Luu_tru;

        // =========== Thành viên ==============
        Danh_sach_Thanh_vien = Du_lieu_Dich_vu.Danh_sach_Thanh_vien;
        // =========== Tài khoản ==============
        Danh_sach_Tai_khoan = Du_lieu_Dich_vu.Danh_sach_Tai_khoan;
        // cập nhật danh sách thu chi của từng thành viên? --- Động tác thừa. Trong JSON có lưu sẵn dữ liệu này rồi.
        // Nhưng nếu mà lưu các khoản thu chi thành các file riêng cho từng thành viên thì duyệt qua các json để kiếm cái mã tương ứng cho từng thành viên mà áp vào.
        /*        Danh_sach_Tai_khoan.ForEach(tai_khoan =>
                {
                    // Cài đặt Newtonsoft JSON nếu ko chạy được chỗ này
                    var dsKhoanThu = JsonConvert.DeserializeObject<List<XL_KHOAN_THU>>("Danh_sach_Khoan_thu");
                    var dsKhoanChi = JsonConvert.DeserializeObject<List<XL_KHOAN_CHI>>("Danh_sach_Khoan_chi");
                    tai_khoan.Danh_sach_Khoan_thu = dsKhoanThu;
                    tai_khoan.Danh_sach_Khoan_chi = dsKhoanChi;
                ;*/
        // =========== Gia đình ===============
        Danh_sach_Gia_dinh = Du_lieu_Dich_vu.Danh_sach_Gia_dinh;
    }

    public XL_DU_LIEU Tao_Du_lieu()
    {
        var Du_lieu = Du_lieu_Dich_vu;
        return Du_lieu;
    }

    public string Them_Khoan_chi_Moi(XL_TAI_KHOAN Tai_khoan, XL_KHOAN_CHI Khoan_chi_Moi)
    {
        string Kq = XL_DU_LIEU.Ghi_Them_Khoan_chi(Tai_khoan, Khoan_chi_Moi);
        return Kq;
    }

    public string Them_Khoan_thu_Moi(XL_TAI_KHOAN Tai_khoan, XL_KHOAN_THU Khoan_thu_Moi)
    {
        string Kq = XL_DU_LIEU.Ghi_Them_Khoan_thu(Tai_khoan, Khoan_thu_Moi);
        return Kq;
    }

    public string Cap_nhat_Khoan_thu_Moi(XL_TAI_KHOAN Tai_khoan, XL_KHOAN_THU Khoan_thu_Moi)
    {
        string Kq = XL_DU_LIEU.Ghi_Cap_nhat_Khoan_thu(Tai_khoan, Khoan_thu_Moi);
        return Kq;
    }
    public string Cap_nhat_Khoan_chi_Moi(XL_TAI_KHOAN Tai_khoan, XL_KHOAN_CHI Khoan_chi_Moi)
    {
        string Kq = XL_DU_LIEU.Ghi_Cap_nhat_Khoan_chi(Tai_khoan, Khoan_chi_Moi);
        return Kq;
    }
    public string Xoa_Khoan_thu(XL_TAI_KHOAN Tai_khoan, XL_KHOAN_THU Khoan_thu_Moi)
    {
        string Kq = XL_DU_LIEU.Ghi_Xoa_Khoan_thu(Tai_khoan, Khoan_thu_Moi);
        return Kq;
    }
    public string Xoa_Khoan_chi(XL_TAI_KHOAN Tai_khoan, XL_KHOAN_CHI Khoan_chi_Moi)
    {
        string Kq = XL_DU_LIEU.Ghi_Xoa_Khoan_chi(Tai_khoan, Khoan_chi_Moi);
        return Kq;
    }
}

// ------------------------ DataLayer
// Class xu ly du lieu.
// Trong day co gi?
public partial class XL_DU_LIEU
{
    // ------- Các thư mục quản lý dữ liệu ----------
    static DirectoryInfo Thu_muc_Project = new DirectoryInfo(HostingEnvironment.ApplicationPhysicalPath);
    // GetDirectories lấy các thư mục trùng tên và trả mảng. Ở đây chọn cái đầu tiên
    static DirectoryInfo Thu_muc_Du_lieu = Thu_muc_Project.GetDirectories("3-Du_lieu_Luu_tru")[0];
    static DirectoryInfo Thu_muc_Gia_dinh = Thu_muc_Du_lieu.GetDirectories("Gia_dinh")[0];
    static DirectoryInfo Thu_muc_Thanh_vien = Thu_muc_Du_lieu.GetDirectories("Thanh_vien")[0];
    static DirectoryInfo Thu_muc_Tai_khoan = Thu_muc_Du_lieu.GetDirectories("Tai_khoan")[0];

    public static XL_DU_LIEU Doc_Du_lieu_Luu_tru()
    {
        var Du_lieu_Luu_tru = new XL_DU_LIEU();
        Du_lieu_Luu_tru.Danh_sach_Gia_dinh = Doc_Danh_sach_Gia_dinh();
        Du_lieu_Luu_tru.Danh_sach_Tai_khoan = Doc_Danh_sach_Tai_khoan();
        Du_lieu_Luu_tru.Danh_sach_Thanh_vien = Doc_Danh_sach_Thanh_vien();

        return Du_lieu_Luu_tru;
    }

    // ********* Đọc *********
    static List<XL_GIA_DINH> Doc_Danh_sach_Gia_dinh()
    {
        var Danh_sach = new List<XL_GIA_DINH>();
        var Danh_sach_Tap_tin = Thu_muc_Gia_dinh.GetFiles("*.json").ToList();       // Lấy các tập tin json trong thư mục gia đình và gán vô đây thành List
        Danh_sach_Tap_tin.ForEach(Tap_tin =>    // "For each file, do:
        {
            var Duong_dan = Tap_tin.FullName;                               // Get the file path
            var Chuoi_JSON = File.ReadAllText(Duong_dan);                   // Extract string from file
            var Doi_tuong = Json.Decode<XL_GIA_DINH>(Chuoi_JSON);           // Decode JSON object from JSON file
            Danh_sach.Add(Doi_tuong);                                       // Add the object to this List and return.
        });
        return Danh_sach;
    }

    static List<XL_TAI_KHOAN> Doc_Danh_sach_Tai_khoan()
    {
        var Danh_sach = new List<XL_TAI_KHOAN>();
        var Danh_sach_Tap_tin = Thu_muc_Tai_khoan.GetFiles("*.json").ToList(); // -> List of files

        Danh_sach_Tap_tin.ForEach(Tap_tin =>
        {                                  // For each files in list, do:
            var Duong_dan = Tap_tin.FullName;
            var Chuoi_JSON = File.ReadAllText(Duong_dan);
            var Doi_tuong = Json.Decode<XL_TAI_KHOAN>(Chuoi_JSON);
            Danh_sach.Add(Doi_tuong);
        });
        return Danh_sach;
    }

    static List<XL_THANH_VIEN> Doc_Danh_sach_Thanh_vien()
    {
        var Danh_sach = new List<XL_THANH_VIEN>();
        var Danh_sach_Tap_tin = Thu_muc_Thanh_vien.GetFiles("*.json").ToList();       // Lấy các tập tin json trong thư mục gia đình và gán vô đây thành List
        Danh_sach_Tap_tin.ForEach(Tap_tin =>    // "For each file, do:
        {
            var Duong_dan = Tap_tin.FullName;                               // Get the file path
            var Chuoi_JSON = File.ReadAllText(Duong_dan);                   // Extract string from file
            var Doi_tuong = Json.Decode<XL_THANH_VIEN>(Chuoi_JSON);           // Decode JSON object from JSON file
            Danh_sach.Add(Doi_tuong);                                       // Add the object to this List and return.
        });
        return Danh_sach;
    }


    // ********* Ghi ***********

    public static string Ghi_Them_Khoan_chi(XL_TAI_KHOAN Tai_khoan, XL_KHOAN_CHI Khoan_chi)
    {
        var Kq = "";
        Tai_khoan.Danh_sach_Khoan_chi.Add(Khoan_chi);   // Thêm khoản thu vô trong danh sách thu của thành viên
        Kq = Ghi_Tai_khoan(Tai_khoan);                  // Ghi xuống file thành viên
        if (Kq != "OK")
        {
            // Nếu gặp lỗi, bỏ khoản thu này ra khỏi danh sách khoản thu của tài khoản
            Tai_khoan.Danh_sach_Khoan_chi.Remove(Khoan_chi);
        }
        return Kq;
    }
    public static string Ghi_Them_Khoan_thu(XL_TAI_KHOAN Tai_khoan, XL_KHOAN_THU Khoan_thu)
    {
        var Kq = "";
        Tai_khoan.Danh_sach_Khoan_thu.Add(Khoan_thu);   // Thêm khoản thu vô trong danh sách thu của thành viên
        Kq = Ghi_Tai_khoan(Tai_khoan);                  // Ghi xuống file thành viên
        if (Kq != "OK")
        {
            // Nếu gặp lỗi, bỏ khoản thu này ra khỏi danh sách khoản thu của tài khoản
            Tai_khoan.Danh_sach_Khoan_thu.Remove(Khoan_thu);
        }
        return Kq;
    }

    public static string Ghi_Cap_nhat_Khoan_chi(XL_TAI_KHOAN Tai_khoan, XL_KHOAN_CHI Khoan_chi)
    {
        var Kq = "";
        List<XL_KHOAN_CHI> Danh_sach_goc = Tai_khoan.Danh_sach_Khoan_chi;
        // Tìm Khoản chi trùng và ghi đè lên cái khoản chi đó.
        // Vậy thì khỏi lo handle case tìm không thấy
        Tai_khoan.Danh_sach_Khoan_chi.ForEach(kc =>
        {
            if (kc.Ma_so.Equals(Khoan_chi.Ma_so))
            {
                kc = Khoan_chi;
            }
        });

        Kq = Ghi_Tai_khoan(Tai_khoan);                  // Ghi xuống file thành viên
        if (Kq != "OK")
        {
            // Nếu gặp lỗi, bỏ khoản thu này ra khỏi danh sách khoản thu của tài khoản
            Tai_khoan.Danh_sach_Khoan_chi = Danh_sach_goc;
        }
        return Kq;
    }


    public static string Ghi_Cap_nhat_Khoan_thu(XL_TAI_KHOAN Tai_khoan, XL_KHOAN_THU Khoan_thu)
    {
        var Kq = "";
        List<XL_KHOAN_THU> Danh_sach_goc = Tai_khoan.Danh_sach_Khoan_thu;

        // Tìm Khoản thu trùng và ghi đè lên cái khoản chi đó.
        // Vậy thì khỏi lo handle case tìm không thấy
        Tai_khoan.Danh_sach_Khoan_thu.ForEach(kt =>
        {
            if (kt.Ma_so.Equals(Khoan_thu.Ma_so))
            {
                kt = Khoan_thu;
            }
        });

        Kq = Ghi_Tai_khoan(Tai_khoan);                  // Ghi xuống file Tài khoản
        if (Kq != "OK")
        {
            // Nếu gặp lỗi, bỏ khoản thu này ra khỏi danh sách khoản thu của tài khoản
            Tai_khoan.Danh_sach_Khoan_thu = Danh_sach_goc;
        }
        return Kq;
    }
    public static string Ghi_Xoa_Khoan_chi(XL_TAI_KHOAN Tai_khoan, XL_KHOAN_CHI Khoan_chi)
    {
        var Kq = "";
        List<XL_KHOAN_CHI> Danh_sach_goc = Tai_khoan.Danh_sach_Khoan_chi;
        // Tìm Khoản chi trùng và ghi đè lên cái khoản chi đó.
        // Vậy thì khỏi lo handle case tìm không thấy
        Tai_khoan.Danh_sach_Khoan_chi.ForEach(kc =>
        {
            if (kc.Ma_so.Equals(Khoan_chi.Ma_so))
            {
                kc = Khoan_chi;
            }
        });

        Kq = Ghi_Tai_khoan(Tai_khoan);                  // Ghi xuống file thành viên
        if (Kq != "OK")
        {
            // Nếu gặp lỗi, bỏ khoản thu này ra khỏi danh sách khoản thu của tài khoản
            Tai_khoan.Danh_sach_Khoan_chi = Danh_sach_goc;
        }
        return Kq;
    }


    public static string Ghi_Xoa_Khoan_thu(XL_TAI_KHOAN Tai_khoan, XL_KHOAN_THU Khoan_thu)
    {
        var Kq = "";
        List<XL_KHOAN_THU> Danh_sach_goc = Tai_khoan.Danh_sach_Khoan_thu;

        // Tìm Khoản thu trùng và ghi đè lên cái khoản chi đó.
        // Vậy thì khỏi lo handle case tìm không thấy
        Tai_khoan.Danh_sach_Khoan_thu.ForEach(kt =>
        {
            if (kt.Ma_so.Equals(Khoan_thu.Ma_so))
            {
                kt = Khoan_thu;
            }
        });

        Kq = Ghi_Tai_khoan(Tai_khoan);                  // Ghi xuống file Tài khoản
        if (Kq != "OK")
        {
            // Nếu gặp lỗi, bỏ khoản thu này ra khỏi danh sách khoản thu của tài khoản
            Tai_khoan.Danh_sach_Khoan_thu = Danh_sach_goc;
        }
        return Kq;
    }

    static string Ghi_Tai_khoan(XL_TAI_KHOAN Tai_khoan)
    {
        var Kq = "";
        var Duong_dan = $"{Thu_muc_Tai_khoan.FullName}\\{Tai_khoan.Ma_so}.json";
        var Chuoi_JSON = Json.Encode(Tai_khoan);
        try
        {
            File.WriteAllText(Duong_dan, Chuoi_JSON);
            Kq = "OK";
        }
        catch (Exception ex)
        {
            Kq = ex.Message;
        }
        return Kq;
    }
}