using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Web.Helpers;
using System.Web.Hosting;
using System.Globalization;
using System.Net;
using System.Configuration;
using System.Xml.Serialization;


public partial class XL_UNG_DUNG
{
    static XL_UNG_DUNG Ung_dung = null;
    static CultureInfo cultureInfo = CultureInfo.InvariantCulture;
    static string vnDateFormat = "dd-MM-yyyy";
    XL_DU_LIEU Du_lieu_Ung_dung = null;
    List<XL_TAI_KHOAN> Danh_sach_Tai_khoan = new List<XL_TAI_KHOAN>();
    List<XL_KHOAN_THU> Danh_sach_Khoan_thu = new List<XL_KHOAN_THU>();  // dùng để load danh sách và trả kết quả
    List<XL_KHOAN_CHI> Danh_sach_Khoan_chi = new List<XL_KHOAN_CHI>();  // dùng để load danh sách và trả kết quả


    public static XL_UNG_DUNG Khoi_dong_Ung_dung()
    {
        Ung_dung = new XL_UNG_DUNG();
        Ung_dung.Khoi_dong_Du_lieu_Ung_dung();
        return Ung_dung;
    }

    void Khoi_dong_Du_lieu_Ung_dung()
    {
        var Du_lieu_tu_Dich_vu = XL_DU_LIEU.Doc_Du_lieu();
        Du_lieu_Ung_dung = Du_lieu_tu_Dich_vu;
    }

    public XL_NGUOI_DUNG Khoi_dong_Nguoi_dung()
    {
        var Nguoi_dung = (XL_NGUOI_DUNG)HttpContext.Current.Session["Nguoi_dung"];
        if (Nguoi_dung == null || Nguoi_dung.Ten_Dang_nhap == null)
        {
            Nguoi_dung = new XL_NGUOI_DUNG();
        }
        else
        {
            // Lấy cái danh sách của người dùng ra, cho người dùng tự quản lý khoản thu/chi của mình ở trang của mình
            Nguoi_dung.Danh_sach_Khoan_chi_Rieng = Ung_dung.Du_lieu_Ung_dung
                .Danh_sach_Tai_khoan.FirstOrDefault<XL_TAI_KHOAN>(
                tk => tk.Ten_Dang_nhap == Nguoi_dung.Ten_Dang_nhap
                )
                .Danh_sach_Khoan_chi;
            Nguoi_dung.Danh_sach_Khoan_thu_Rieng = Ung_dung.Du_lieu_Ung_dung
              .Danh_sach_Tai_khoan.FirstOrDefault<XL_TAI_KHOAN>(
                tk => tk.Ten_Dang_nhap == Nguoi_dung.Ten_Dang_nhap
                )
              .Danh_sach_Khoan_thu;
            Nguoi_dung.Ho_ten = Ung_dung.Du_lieu_Ung_dung
              .Danh_sach_Tai_khoan.FirstOrDefault<XL_TAI_KHOAN>(
                tk => tk.Ten_Dang_nhap == Nguoi_dung.Ten_Dang_nhap
                ).Thong_tin_Thanh_vien.Ho_ten;
        }

        HttpContext.Current.Session["Nguoi_dung"] = Nguoi_dung;
        return Nguoi_dung;
    }

    // Xử lý chức năng của Khách tham quan.
    public string Khoi_dong_MH_Chinh()
    {
        var Nguoi_dung = (XL_NGUOI_DUNG)HttpContext.Current.Session["Nguoi_dung"];


        // Cho người dùng xem hết tất cả các khoản thu và chi của gia đình ở màn hình chính
        Nguoi_dung.Danh_sach_Khoan_chi_Xem = Danh_sach_Khoan_chi;
        Nguoi_dung.Danh_sach_Khoan_chi_Chon = new List<XL_KHOAN_CHI>();

        Nguoi_dung.Danh_sach_Khoan_thu_Xem = Danh_sach_Khoan_thu;
        Nguoi_dung.Danh_sach_Khoan_thu_Chon = new List<XL_KHOAN_THU>();

        // ?? Code rác thì phải. Nhưng thôi cứ để yên đó vì nó vẫn đang chạy :D
        var Chuoi_HTML = Tao_Chuoi_HTML_Xem();

        return Chuoi_HTML;
    }

    public string Dang_nhap(string Ten_Dang_nhap, string Mat_khau)
    {

        var Chuoi_HTML = "Vui lòng nhập thông tin đăng nhập";
        // Kiểm tra tài khoản tồn tại
        var Tai_khoan = Du_lieu_Ung_dung.Danh_sach_Tai_khoan.FirstOrDefault(
            x => x.Ten_Dang_nhap == Ten_Dang_nhap && x.Mat_khau == Mat_khau);

        // Nếu đăng nhập đúng thì set Session này là của người dùng
        // Rồi thực hiện khởi động màn hình chính
        if (Tai_khoan != null)
        {
            XL_NGUOI_DUNG user = Cap_nhat_Thong_tin_Nguoi_dung_Session(Tai_khoan);
            HttpContext.Current.Session["Nguoi_dung"] = user;
            HttpContext.Current.Response.Redirect("MH_Chinh.cshtml");
        }
        else
        {
            Chuoi_HTML = "Thông tin đăng nhập không tồn tại";
        }
        return Chuoi_HTML;

    }

    // Cài đặt user để sử dụng cho session này
    public XL_NGUOI_DUNG Cap_nhat_Thong_tin_Nguoi_dung_Session(XL_TAI_KHOAN Tai_khoan)
    {
        // Cài đặt user cho session này
        var user = new XL_NGUOI_DUNG()
        {
            Ho_ten = Tai_khoan.Thong_tin_Thanh_vien.Ho_ten,
            Ma_so = Tai_khoan.Ma_so,
            Ten_Dang_nhap = Tai_khoan.Ten_Dang_nhap,
            Danh_sach_Khoan_chi_Rieng = Tai_khoan.Danh_sach_Khoan_chi,
            Danh_sach_Khoan_thu_Rieng = Tai_khoan.Danh_sach_Khoan_thu
        };
        return user;
    }

    public string Chon_Khoan_thu(string Ma_so_Khoan_thu)
    {
        var Nguoi_dung = (XL_NGUOI_DUNG)HttpContext.Current.Session["Nguoi_dung"];
        // Xử lý
        var Khoan_thu = Nguoi_dung.Danh_sach_Khoan_thu_Rieng.FirstOrDefault(x => x.Ma_so == Ma_so_Khoan_thu);
        // Hiển thị khoản thu ở bên phải màn hình
        // Nghĩa là thay thế cho đối tượng hiển thị của phía bên phải màn hình
        // Cần tạo hàm generate HTML để hiển thị thông tin khoản thu/chi?

        var Chuoi_HTML = Tao_Chuoi_HTML_Xem();
        return Chuoi_HTML;
    }

    public string Chon_Khoan_chi(string Ma_so_Khoan_chi)
    {
        var Nguoi_dung = (XL_NGUOI_DUNG)HttpContext.Current.Session["Nguoi_dung"];
        // Xử lý
        var Khoan_chi = Nguoi_dung.Danh_sach_Khoan_thu_Rieng.FirstOrDefault(x => x.Ma_so == Ma_so_Khoan_chi);
        // Hiển thị khoản thu ở bên phải màn hình
        // Nghĩa là thay thế cho đối tượng hiển thị của phía bên phải màn hình
        // Cần tạo hàm generate HTML để hiển thị thông tin khoản thu/chi?

        var Chuoi_HTML = Tao_Chuoi_HTML_Xem();
        return Chuoi_HTML;
    }


}

// *********** View-Layers/Presentation Layers VL/PL ********************************
public partial class XL_UNG_DUNG
{
    public string Dia_chi_Media = $"{XL_DU_LIEU.Dia_chi_Dich_vu}/Media";
    public CultureInfo Dinh_dang_VN = CultureInfo.GetCultureInfo("vi-VN");

    // Hiển thị khoản thu trên màn hình chính: Chỉ hiện ngày và số tiền
    public string Tao_Chuoi_HTML_Xem()
    {
        var Khach_Tham_quan = (XL_NGUOI_DUNG)HttpContext.Current.Session["Nguoi_dung"];
        var Chuoi_HTML = $"<div>" +
                 $"{Tao_Chuoi_HTML_Danh_sach_Khoan_thu_Xem()}" +
             $"</div>";
        return Chuoi_HTML;
    }
    public string Tao_Chuoi_HTML_Danh_sach_Khoan_thu_Chung_Xem()
    {
        var Chuoi_HTML_Danh_sach = "";
        var Danh_sach_Tai_khoan_Sau_Tra_cuu = Xu_ly_Tra_cuu_Phieu_thu(Du_lieu_Ung_dung.Danh_sach_Tai_khoan);

        // Đi vào từng tài khoản
        Danh_sach_Tai_khoan_Sau_Tra_cuu.ForEach(Tai_khoan =>
        {
            // Lấy danh sách khoản thu của nó ra

            Tai_khoan.Danh_sach_Khoan_thu.ForEach(
                Khoan_thu =>
                    {
                        var Chuoi_Ten_thanh_vien = $"Tên thành viên: <span id='name'>" +
                        Tai_khoan.Thong_tin_Thanh_vien.Ho_ten +
                        $"</span><br/>";

                        var Chuoi_So_tien = $"Số tiền: <span id=\"amount\">" +
                        Khoan_thu.So_tien +
                        $"</span><br/>";

                        var Chuoi_Ngay_thu = $"Ngày thu: <span id=\"date\">" +
                        Khoan_thu.Ngay_thu +
                        $"<span><br/>";

                        var Chuoi_HTML = "<tr><td>" +
                        Chuoi_Ten_thanh_vien + Chuoi_So_tien + Chuoi_Ngay_thu +
                        "</tr></td>";

                        Chuoi_HTML_Danh_sach += Chuoi_HTML;
                    }
                    );
        });
        return Chuoi_HTML_Danh_sach;
    }

    public string Tao_Chuoi_HTML_Danh_sach_Khoan_chi_Chung_Xem()
    {
        var Chuoi_HTML_Danh_sach = "";
        var Danh_sach_Tai_khoan_Sau_Tra_cuu = Xu_ly_Tra_cuu_Phieu_chi(Du_lieu_Ung_dung.Danh_sach_Tai_khoan);
        // Đi vào từng tài khoản
        Danh_sach_Tai_khoan_Sau_Tra_cuu.ForEach(Tai_khoan =>
        {
            // Lấy danh sách khoản thu của nó ra
            Tai_khoan.Danh_sach_Khoan_chi.ForEach(
                Khoan_chi =>
                {
                    var Chuoi_Ten_thanh_vien = $"Tên thành viên: <span id='name'>" +
                    Tai_khoan.Thong_tin_Thanh_vien.Ho_ten +
                    $"</span><br/>";

                    var Chuoi_So_tien = $"Số tiền: <span id=\"amount\">" +
                    Khoan_chi.So_tien +
                    $"</span><br/>";

                    var Chuoi_Ngay_chi = $"Ngày chi: <span id=\"date\">" +
                    Khoan_chi.Ngay_chi +
                    $"<span><br/>";

                    var Chuoi_HTML = "<tr><td>" +
                    Chuoi_Ten_thanh_vien + Chuoi_So_tien + Chuoi_Ngay_chi +
                    "</tr></td>";

                    Chuoi_HTML_Danh_sach += Chuoi_HTML;
                }
                );
        });
        return Chuoi_HTML_Danh_sach;
    }
    public string Tao_Chuoi_HTML_Danh_sach_Khoan_thu_Ca_nhan_Xem(XL_NGUOI_DUNG user)
    {
        var Chuoi_HTML_Danh_sach = "";

        // Lấy tài khoản cá nhân ra.
        var Danh_sach_Tai_khoan_Ca_nhan = Du_lieu_Ung_dung.Danh_sach_Tai_khoan;
        // Chỗ này chắc sẽ không null (hi vọng)
        Danh_sach_Tai_khoan_Ca_nhan = Danh_sach_Tai_khoan_Ca_nhan.FindAll(Tai_khoan => Tai_khoan.Ma_so == user.Ma_so);
        var Danh_sach_Tai_khoan_Sau_Tra_cuu = Xu_ly_Tra_cuu_Phieu_thu(Danh_sach_Tai_khoan_Ca_nhan);

        // Xử lý in
        Danh_sach_Tai_khoan_Sau_Tra_cuu.ForEach(Tai_khoan =>
        {
            // Lấy danh sách khoản thu của nó ra. Thêm cái dòng "Cập nhật, xóa vô"
            Tai_khoan.Danh_sach_Khoan_thu.ForEach(
                Khoan_thu =>
                {
                    var Chuoi_Ten_thanh_vien = $"Tên thành viên: <span id='name'>" +
                    Tai_khoan.Thong_tin_Thanh_vien.Ho_ten +
                    $"</span><br/>";

                    var Chuoi_So_tien = $"Số tiền: <span id=\"amount\">" +
                    Khoan_thu.So_tien +
                    $"</span><br/>";

                    var Chuoi_Ngay_thu = $"Ngày thu: <span id=\"date\">" +
                    Khoan_thu.Ngay_thu +
                    $"<span><br/>";

                    // Chức năng cập nhật và xóa.
                    // Truyền tham số chức năng, và mã khoản thu là được.
                    // gửi tới trang MH_Thong_tin_Khoan
                    var Chuoi_Chuc_nang_Cap_nhat = $"<form method='post' action='MH_Thong_tin_Khoan.cshtml'>" +
                    $"<input type='hidden' name='Th_Ma_so_Chuc_nang' value='CAP_NHAT_KHOAN_THU' />" +
                    $"<input type='hidden' name='Th_Ma_so_Khoan_thu' value='{Khoan_thu.Ma_so}'>" +
                    $"<button class='btn btn-primary' type='submit'>" +
                    $" {user.Chuc_nang_Cap_nhat_Khoan_thu.Ten} </button>" +
                    $"</form>";

                    var Chuoi_Chuc_nang_Xoa = $"<form method='post' action='MH_Thong_tin_Khoan.cshtml'>" +
                    $"<input type='hidden' name='Th_Ma_so_Chuc_nang' value='XOA_KHOAN_THU' />" +
                    $"<input type='hidden' name='Th_Ma_so_Khoan_thu' value='{Khoan_thu.Ma_so}'>" +
                    $"<button class='btn btn-danger' type='submit'> " +
                    $"{user.Chuc_nang_Xoa_Khoan_thu.Ten} </button>" +
                    $"</form>";

                    var Chuoi_HTML = "<tr><td>" +
                    Chuoi_Ten_thanh_vien + Chuoi_So_tien + Chuoi_Ngay_thu + Chuoi_Chuc_nang_Cap_nhat + Chuoi_Chuc_nang_Xoa +
                    "</tr></td>";

                    Chuoi_HTML_Danh_sach += Chuoi_HTML;
                }
                    );
        });
        return Chuoi_HTML_Danh_sach;
    }

    public string Tao_Chuoi_HTML_Danh_sach_Khoan_chi_Ca_nhan_Xem(XL_NGUOI_DUNG user)
    {
        var Chuoi_HTML_Danh_sach = "";

        var Danh_sach_Tai_khoan_Ca_nhan = Du_lieu_Ung_dung.Danh_sach_Tai_khoan;
        Danh_sach_Tai_khoan_Ca_nhan = Danh_sach_Tai_khoan_Ca_nhan.FindAll(Tai_khoan => Tai_khoan.Ma_so == user.Ma_so);
        var Danh_sach_Tai_khoan_Sau_Tra_cuu = Xu_ly_Tra_cuu_Phieu_chi(Danh_sach_Tai_khoan_Ca_nhan);
        // Đi vào từng tài khoản
        Danh_sach_Tai_khoan_Sau_Tra_cuu.ForEach(Tai_khoan =>
        {
            // Lấy danh sách khoản thu của nó ra
            Tai_khoan.Danh_sach_Khoan_chi.ForEach(
                Khoan_chi =>
                {
                    var Chuoi_Ten_thanh_vien = $"Tên thành viên: <span id='name'>" +
                    Tai_khoan.Thong_tin_Thanh_vien.Ho_ten +
                    $"</span><br/>";

                    var Chuoi_So_tien = $"Số tiền: <span id=\"amount\">" +
                    Khoan_chi.So_tien +
                    $"</span><br/>";

                    var Chuoi_Ngay_chi = $"Ngày chi: <span id=\"date\">" +
                    Khoan_chi.Ngay_chi +
                    $"<span><br/>";

                    var Chuoi_Chuc_nang_Cap_nhat = $"<form method='post' action='MH_Thong_tin_Khoan.cshtml'>" +
$"<input type='hidden' name='Th_Ma_so_Chuc_nang' value='CAP_NHAT_KHOAN_CHI' />" +
$"<input type='hidden' name='Th_Ma_so_Khoan_chi' value='{Khoan_chi.Ma_so}'>" +
$"<button class='btn btn-primary' type='submit'> {user.Chuc_nang_Cap_nhat_Khoan_chi.Ten} " +
$"</button>" +
$"</form>";

                    var Chuoi_Chuc_nang_Xoa = $"<form method='post' action='MH_Thong_tin_Khoan.cshtml'>" +
                    $"<input type='hidden' name='Th_Ma_so_Chuc_nang' value='XOA_KHOAN_CHI' />" +
                    $"<input type='hidden' name='Th_Ma_so_Khoan_chi' value='{Khoan_chi.Ma_so}'>" +
                    $"<button class='btn btn-danger' type='submit'> {user.Chuc_nang_Xoa_Khoan_chi.Ten} </button>" +
                    $"</form>";

                    var Chuoi_HTML = "<tr><td>" +
                    Chuoi_Ten_thanh_vien + Chuoi_So_tien + Chuoi_Ngay_chi + Chuoi_Chuc_nang_Cap_nhat + Chuoi_Chuc_nang_Xoa +
                    "</tr></td>";

                    Chuoi_HTML_Danh_sach += Chuoi_HTML;
                }
                );
        });
        return Chuoi_HTML_Danh_sach;
    }


    public string Tao_Chuoi_HTML_Danh_sach_Khoan_thu_Xem()
    {
        var Nguoi_dung = (XL_NGUOI_DUNG)HttpContext.Current.Session["Nguoi_dung"];
        var Danh_sach_Khoan_thu_The_hien = Nguoi_dung.Danh_sach_Khoan_thu_Xem;
        var Danh_sach_Khoan_chi_The_hien = Nguoi_dung.Danh_sach_Khoan_chi_Xem;
        var Chuoi_HTML_Danh_sach = "<div class='row'>";

        // Test thể hiện
        Danh_sach_Khoan_chi_The_hien.ForEach(Khoan_chi =>
        {


            var Chuoi_Thong_tin = $"<tr><td>" +
            $"Tên thành viên: {Khoan_chi.Ngay_chi} <br/>" +
            $"Số tiền chi: {Khoan_chi.So_tien.ToString("n0", Dinh_dang_VN)}"
            ;

            var Chuoi_HTML = $"<div class ='col-md-5' style='margin-bottom':10px'>" +
            $"{Chuoi_Thong_tin}" +
            $"</td></tr>";

            Chuoi_HTML_Danh_sach += Chuoi_Thong_tin;
        });

        Chuoi_HTML_Danh_sach += "</div>";
        return Chuoi_HTML_Danh_sach;

    }

    public string Tao_Chuoi_HTML_Form_Them_Khoan_thu(XL_NGUOI_DUNG user)
    {
        var Chuoi_Form = "";

        // ------ Bước 1: Lấy mã khoản cuối -----
        // Lấy tài khoản của người dùng (cho an toàn)
        XL_TAI_KHOAN tk = Du_lieu_Ung_dung.Danh_sach_Tai_khoan.FirstOrDefault(Tai_khoan => Tai_khoan.Ma_so == user.Ma_so);
        // Lấy mã khoản cuối
        string Ma_cuoi = tk.Danh_sach_Khoan_thu.Last().Ma_so;
        // Lấy số mã
        string Chuoi_So_ma = Ma_cuoi.Substring(2);
        int So_ma = int.Parse(Chuoi_So_ma);
        // Làm chuỗi mã cuôi
        So_ma += 1;
        string Chuoi_Ma_Ket_qua = "KT" + So_ma.ToString();

        // ------ Bước 2 : Tạo form --------------
        string Chuoi_Input_Ma_Chuc_nang = "<input type=\"hidden\" name=\"Th_Ma_so_Chuc_nang\" value=\"THEM_KHOAN_THU\">";
        string Chuoi_Ma_Khoan =
            $"<input type=\"hidden\" name=\"Th_Ma_Khoan_thu\" value=\" {Chuoi_Ma_Ket_qua}\">";
        string Chuoi_Input_Ngay =
            $"<div class=\"mb-3\">" +
            $"<label for=\"date\" class=\"form-label\">Ngày</label>" +
            $"<input type=\"date\" class=\"form-control\" name=\"Th_Ngay\" id=\"date-input\" aria-describedby=\"helpId\"" +
            $"placeholder=\"Ngày\" />" +
            $"</div>";
        string Chuoi_Input_So_tien =
            $"<div class=\"mb-3\">" +
            $"<label for=\"amount\" class=\"form-label\">Số tiền</label>" +
            $"<input type=\"number\" class=\"form-control\" " +
            $"name=\"Th_So_tien\" id=\"amount-input\" aria-describedby=\"helpId" +
            $"placeholder=\"Số tiền\" /></div>";

        Chuoi_Form =
            "<form action=\"MH_Quan_ly_Thu_Chi_Rieng.cshtml\" method=\"post\">";
        Chuoi_Form += Chuoi_Input_Ma_Chuc_nang + Chuoi_Ma_Khoan + Chuoi_Input_So_tien + Chuoi_Input_Ngay;

        Chuoi_Form +=
            $"<button type=\"submit\" class=\"btn btn-primary\">Thêm </button> " +
            $"</form>";

        Chuoi_Form +=
            $"<form method='post' action='MH_Quan_ly_Thu_Chi_Rieng.cshtml'>" +
            $"<button type=\"submit\" class=\"btn btn-danger\">Hủy </button> " +
            $"</form>";


        return Chuoi_Form;
    }

    public string Tao_Chuoi_HTML_Form_Cap_nhat_Khoan_thu(XL_NGUOI_DUNG user, string Ma_khoan)
    {
        var Chuoi_Form = "";

        // ------ Bước 1: Lấy thông tin từ mã khoản -----
        // Lấy Khoản thu ra
        XL_TAI_KHOAN tk = Du_lieu_Ung_dung.Danh_sach_Tai_khoan.FirstOrDefault(Tai_khoan => Tai_khoan.Ma_so == user.Ma_so);
        XL_KHOAN_THU kt = tk.Danh_sach_Khoan_thu.FirstOrDefault(x => x.Ma_so == Ma_khoan);
        // Xử lý chuỗi ngày tháng
        string[] date_strings = kt.Ngay_thu.Split('-');
        string ngay = date_strings[0];
        string thang = date_strings[1];
        string nam = date_strings[2];
        string default_date_value = $"{nam}-{thang}-{ngay}";

        // ------ Bước 2 : Tạo form --------------
        string Chuoi_Input_Ma_Chuc_nang = "<input type=\"hidden\" name=\"Th_Ma_so_Chuc_nang\" value=\"CAP_NHAT_KHOAN_THU\">";
        string Chuoi_Ma_Khoan =
            $"<input type=\"hidden\" name=\"Th_Ma_Khoan_thu\" value=\" {Ma_khoan}\">";
        string Chuoi_Input_Ngay =
            $"<div class=\"mb-3\">" +
            $"<label for=\"date\" class=\"form-label\">Ngày</label>" +
            $"<input type=\"date\" value='{default_date_value}' class=\"form-control\" name=\"Th_Ngay\" id=\"date-input\" aria-describedby=\"helpId\"" +
            $"placeholder=\"Ngày\" />" +
            $"</div>";
        string Chuoi_Input_So_tien =
            $"<div class=\"mb-3\">" +
            $"<label for=\"amount\" class=\"form-label\">Số tiền</label>" +
            $"<input type=\"number\" value='{kt.So_tien}' class=\"form-control\" " +
            $"name=\"Th_So_tien\" id=\"amount-input\" aria-describedby=\"helpId" +
            $"placeholder=\"Số tiền\" /></div>";

        Chuoi_Form =
            "<form action=\"MH_Quan_ly_Thu_Chi_Rieng.cshtml\" method=\"post\">";
        Chuoi_Form += Chuoi_Input_Ma_Chuc_nang + Chuoi_Ma_Khoan + Chuoi_Input_So_tien + Chuoi_Input_Ngay;

        Chuoi_Form +=
            $"<button type=\"submit\" class=\"btn btn-primary\">Cập nhật </button> " +
            $"</form>";

        Chuoi_Form +=
            $"<form method='post' action='MH_Quan_ly_Thu_Chi_Rieng.cshtml'>" +
            $"<button type=\"submit\" class=\"btn btn-danger\">Hủy </button> " +
            $"</form>";


        return Chuoi_Form;
    }

    public string Tao_Chuoi_HTML_Form_Cap_nhat_Khoan_chi(XL_NGUOI_DUNG user, string Ma_khoan)
    {
        var Chuoi_Form = "";

        // ------ Bước 1: Lấy thông tin từ mã khoản -----
        // Lấy Khoản thu ra
        XL_TAI_KHOAN tk = Du_lieu_Ung_dung.Danh_sach_Tai_khoan.FirstOrDefault(Tai_khoan => Tai_khoan.Ma_so == user.Ma_so);
        XL_KHOAN_CHI kc = tk.Danh_sach_Khoan_chi.FirstOrDefault(x => x.Ma_so == Ma_khoan);
        // Xử lý chuỗi ngày tháng
        string[] date_strings = kc.Ngay_chi.Split('-');
        string ngay = date_strings[0];
        string thang = date_strings[1];
        string nam = date_strings[2];
        string default_date_value = $"{nam}-{thang}-{ngay}";

        // ------ Bước 2 : Tạo form --------------
        string Chuoi_Input_Ma_Chuc_nang = "<input type=\"hidden\" name=\"Th_Ma_so_Chuc_nang\" value=\"CAP_NHAT_KHOAN_THU\">";
        string Chuoi_Ma_Khoan =
            $"<input type=\"hidden\" name=\"Th_Ma_Khoan_thu\" value=\" {Ma_khoan}\">";
        string Chuoi_Input_Ngay =
            $"<div class=\"mb-3\">" +
            $"<label for=\"date\" class=\"form-label\">Ngày</label>" +
            $"<input type=\"date\" value='{default_date_value}' class=\"form-control\" name=\"Th_Ngay\" id=\"date-input\" aria-describedby=\"helpId\"" +
            $"placeholder=\"Ngày\" />" +
            $"</div>";
        string Chuoi_Input_So_tien =
            $"<div class=\"mb-3\">" +
            $"<label for=\"amount\" class=\"form-label\">Số tiền</label>" +
            $"<input type=\"number\" value='{kc.So_tien}' class=\"form-control\" " +
            $"name=\"Th_So_tien\" id=\"amount-input\" aria-describedby=\"helpId" +
            $"placeholder=\"Số tiền\" /></div>";

        Chuoi_Form =
            "<form action=\"MH_Quan_ly_Thu_Chi_Rieng.cshtml\" method=\"post\">";
        Chuoi_Form += Chuoi_Input_Ma_Chuc_nang + Chuoi_Ma_Khoan + Chuoi_Input_So_tien + Chuoi_Input_Ngay;

        Chuoi_Form +=
            $"<button type=\"submit\" class=\"btn btn-primary\">Cập nhật </button> " +
            $"</form>";

        Chuoi_Form +=
            $"<form method='post' action='MH_Quan_ly_Thu_Chi_Rieng.cshtml'>" +
            $"<button type=\"submit\" class=\"btn btn-danger\">Hủy </button> " +
            $"</form>";


        return Chuoi_Form;
    }

    public string Tao_Chuoi_HTML_Form_Xoa_Khoan_thu(XL_NGUOI_DUNG user, string Ma_khoan)
    {
        var Chuoi_Form = "";

        // ------ Bước 1: Lấy thông tin từ mã khoản -----
        // Lấy Khoản thu ra
        XL_TAI_KHOAN tk = Du_lieu_Ung_dung.Danh_sach_Tai_khoan.FirstOrDefault(Tai_khoan => Tai_khoan.Ma_so == user.Ma_so);
        XL_KHOAN_THU kt = tk.Danh_sach_Khoan_thu.FirstOrDefault(x => x.Ma_so == Ma_khoan);
        // Xử lý chuỗi ngày tháng
        string[] date_strings = kt.Ngay_thu.Split('-');
        string ngay = date_strings[0];
        string thang = date_strings[1];
        string nam = date_strings[2];
        string default_date_value = $"{nam}-{thang}-{ngay}";

        // ------ Bước 2 : Tạo form --------------
        string Chuoi_Input_Ma_Chuc_nang = "<input type=\"hidden\" name=\"Th_Ma_so_Chuc_nang\" value=\"XOA_KHOAN_THU\">";
        string Chuoi_Ma_Khoan =
            $"<input type=\"hidden\" name=\"Th_Ma_Khoan_thu\" value=\" {Ma_khoan}\">";
        string Chuoi_Input_Ngay =
            $"<div class=\"mb-3\">" +
            $"<label for=\"date\" class=\"form-label\">Ngày</label>" +
            $"<input disabled type=\"date\" value='{default_date_value}' class=\"form-control\" name=\"Th_Ngay\" id=\"date-input\" aria-describedby=\"helpId\"" +
            $"placeholder=\"Ngày\" />" +
            $"</div>";
        string Chuoi_Input_So_tien =
            $"<div class=\"mb-3\">" +
            $"<label for=\"amount\" class=\"form-label\">Số tiền</label>" +
            $"<input disabled type=\"number\" value='{kt.So_tien}' class=\"form-control\" " +
            $"name=\"Th_So_tien\" id=\"amount-input\" aria-describedby=\"helpId" +
            $"placeholder=\"Số tiền\" /></div>";

        Chuoi_Form =
            "<form action=\"MH_Quan_ly_Thu_Chi_Rieng.cshtml\" method=\"post\">";
        Chuoi_Form += Chuoi_Input_Ma_Chuc_nang + Chuoi_Ma_Khoan + Chuoi_Input_So_tien + Chuoi_Input_Ngay;

        Chuoi_Form +=
            $"<button type=\"submit\" class=\"btn btn-primary\">Xóa </button> " +
            $"</form>";

        Chuoi_Form +=
            $"<form method='post' action='MH_Quan_ly_Thu_Chi_Rieng.cshtml'>" +
            $"<button type=\"submit\" class=\"btn btn-danger\">Hủy </button> " +
            $"</form>";


        return Chuoi_Form;
    }
    public string Tao_Chuoi_HTML_Form_Xoa_Khoan_chi(XL_NGUOI_DUNG user, string Ma_khoan)
    {
        var Chuoi_Form = "";

        // ------ Bước 1: Lấy thông tin từ mã khoản -----
        // Lấy Khoản thu ra
        XL_TAI_KHOAN tk = Du_lieu_Ung_dung.Danh_sach_Tai_khoan.FirstOrDefault(Tai_khoan => Tai_khoan.Ma_so == user.Ma_so);
        XL_KHOAN_CHI kt = tk.Danh_sach_Khoan_chi.FirstOrDefault(x => x.Ma_so == Ma_khoan);
        // Xử lý chuỗi ngày tháng
        string[] date_strings = kt.Ngay_chi.Split('-');
        string ngay = date_strings[0];
        string thang = date_strings[1];
        string nam = date_strings[2];
        string default_date_value = $"{nam}-{thang}-{ngay}";

        // ------ Bước 2 : Tạo form --------------
        string Chuoi_Input_Ma_Chuc_nang = "<input type=\"hidden\" name=\"Th_Ma_so_Chuc_nang\" value=\"XOA_KHOAN_CHI\">";
        string Chuoi_Ma_Khoan =
            $"<input type=\"hidden\" name=\"Th_Ma_Khoan_thu\" value=\" {Ma_khoan}\">";
        string Chuoi_Input_Ngay =
            $"<div class=\"mb-3\">" +
            $"<label for=\"date\" class=\"form-label\">Ngày</label>" +
            $"<input disabled type=\"date\" value='{default_date_value}' class=\"form-control\" name=\"Th_Ngay\" id=\"date-input\" aria-describedby=\"helpId\"" +
            $"placeholder=\"Ngày\" />" +
            $"</div>";
        string Chuoi_Input_So_tien =
            $"<div class=\"mb-3\">" +
            $"<label for=\"amount\" class=\"form-label\">Số tiền</label>" +
            $"<input disabled type=\"number\" value='{kt.So_tien}' class=\"form-control\" " +
            $"name=\"Th_So_tien\" id=\"amount-input\" aria-describedby=\"helpId" +
            $"placeholder=\"Số tiền\" /></div>";

        Chuoi_Form =
            "<form action=\"MH_Quan_ly_Thu_Chi_Rieng.cshtml\" method=\"post\">";
        Chuoi_Form += Chuoi_Input_Ma_Chuc_nang + Chuoi_Ma_Khoan + Chuoi_Input_So_tien + Chuoi_Input_Ngay;

        Chuoi_Form +=
            $"<button type=\"submit\" class=\"btn btn-primary\">Xóa </button> " +
            $"</form>";

        Chuoi_Form +=
            $"<form method='post' action='MH_Quan_ly_Thu_Chi_Rieng.cshtml'>" +
            $"<button type=\"submit\" class=\"btn btn-danger\">Hủy </button> " +
            $"</form>";


        return Chuoi_Form;
    }

    public string Tao_Chuoi_HTML_Form_Them_Khoan_chi(XL_NGUOI_DUNG user)
    {
        var Chuoi_Form = "";

        // ------ Bước 1: Lấy mã khoản cuối -----
        // Lấy tài khoản của người dùng (cho an toàn)
        XL_TAI_KHOAN tk = Du_lieu_Ung_dung.Danh_sach_Tai_khoan.FirstOrDefault(Tai_khoan => Tai_khoan.Ma_so == user.Ma_so);
        // Lấy mã khoản cuối
        string Ma_cuoi = tk.Danh_sach_Khoan_chi.Last().Ma_so;
        // Lấy số mã
        string Chuoi_So_ma = Ma_cuoi.Substring(2);
        int So_ma = int.Parse(Chuoi_So_ma);
        // Làm chuỗi mã cuôi
        So_ma += 1;
        string Chuoi_Ma_Ket_qua = "KC" + So_ma.ToString();

        // ------ Bước 2 : Tạo form --------------
        string Chuoi_Input_Ma_Chuc_nang = "<input type=\"hidden\" name=\"Th_Ma_so_Chuc_nang\" value=\"THEM_KHOAN_CHI\">";
        string Chuoi_Ma_Khoan =
            $"<input type=\"hidden\" name=\"Th_Ma_Khoan_chi\" value=\" {Chuoi_Ma_Ket_qua}\">";
        string Chuoi_Input_Ngay =
            $"<div class=\"mb-3\">" +
            $"<label for=\"date\" class=\"form-label\">Ngày</label>" +
            $"<input type=\"date\" class=\"form-control\" name=\"Th_Ngay\" id=\"date-input\" aria-describedby=\"helpId\"" +
            $"placeholder=\"Ngày\" />" +
            $"</div>";
        string Chuoi_Input_So_tien =
            $"<div class=\"mb-3\">" +
            $"<label for=\"amount\" class=\"form-label\">Số tiền</label>" +
            $"<input type=\"number\" class=\"form-control\" " +
            $"name=\"Th_So_tien\" id=\"amount-input\" aria-describedby=\"helpId" +
            $"placeholder=\"Số tiền\" /></div>";

        Chuoi_Form =
            "<form action=\"MH_Quan_ly_Thu_Chi_Rieng.cshtml\" method=\"post\">";
        Chuoi_Form += Chuoi_Input_Ma_Chuc_nang + Chuoi_Ma_Khoan + Chuoi_Input_So_tien + Chuoi_Input_Ngay;

        Chuoi_Form +=
            $"<button type=\"submit\" class=\"btn btn-primary\">Thêm </button> " +
            $"</form>";

        Chuoi_Form +=
            $"<form method='post' action='MH_Quan_ly_Thu_Chi_Rieng.cshtml'>" +
            $"<button type=\"submit\" class=\"btn btn-danger\">Hủy </button> " +
            $"</form>";


        return Chuoi_Form;
    }
}

// *********** Business Layer BL *********
public partial class XL_UNG_DUNG
{
    // Hàm cũ, hiện giờ xài hàm mới ở dưới
    public List<XL_KHOAN_THU> Tra_cuu_Khoan_thu(string Chuoi_Tra_cuu, int option) // Thành viên
    {
        // Hàm cũ xài 3 cái radio button. Nhưng trong hàm mới không cần xét cái này.
        List<XL_KHOAN_THU> result = new List<XL_KHOAN_THU>();

        switch (option)
        {
            case 1:     // tên thành viên - thì lấy trong danh sách tài khoản ra.
                // Tìm tất cả thành viên có chuỗi trong tên
                var Danh_sach_Tai_khoan_Trung_ten = Danh_sach_Tai_khoan.FindAll(
                    tk => tk.Thong_tin_Thanh_vien.Ho_ten.Contains(Chuoi_Tra_cuu)
                    );
                // Add danh sách khoản thu của từng tài khoản vô trong danh sách kết quá
                Danh_sach_Tai_khoan_Trung_ten.ForEach(
                    tk => result.AddRange(tk.Danh_sach_Khoan_thu)
                    );
                break;
            case 2:     // ngày: từ - đến
                break;
            case 3:     // số tiền trên/dưới
                break;
            default:
                break;
        }
        return result;

    }

    // Xử lý tra cứu trong màn hình chung (Không có tra cứu trong màn hình riêng..?)
    // Chưa test search ngày tháng
    public List<XL_TAI_KHOAN> Xu_ly_Tra_cuu_Phieu_thu(List<XL_TAI_KHOAN> Danh_sach_Goc)
    {
        List<XL_TAI_KHOAN> Danh_sach_Result = Danh_sach_Goc;
        // Chuỗi xử lý tra cứu
        var Chuoi_Tra_cuu_Ho_ten = HttpContext.Current.Request["Th_Ho_ten"];
        var Chuoi_Tra_cuu_Date_Greater = HttpContext.Current.Request["Th_Date_Greater"];
        var Chuoi_Tra_cuu_Date_Less = HttpContext.Current.Request["Th_Date_Less"];
        var Chuoi_Tra_cuu_Money_Greater = HttpContext.Current.Request["Th_Money_Greater"];
        var Chuoi_Tra_cuu_Money_Less = HttpContext.Current.Request["Th_Money_Less"];

        // Rút ngắn danh sách theo chuỗi tra cứu
        // Họ tên
        // Lấy những người có chuỗi tra cứu trong họ tên
        if (Chuoi_Tra_cuu_Ho_ten != null && Chuoi_Tra_cuu_Ho_ten != "")
        {
            Danh_sach_Result = Danh_sach_Result.FindAll(
                Tai_khoan =>
                Tai_khoan.Thong_tin_Thanh_vien.Ho_ten.ToUpper()
                .Contains(Chuoi_Tra_cuu_Ho_ten.ToUpper())
            );
        }

        // Số tiền
        // Chỉ xử lý khi không null và nhập vào lớn hơn 0.
        if (Chuoi_Tra_cuu_Money_Greater != null && Chuoi_Tra_cuu_Money_Greater != "" && long.Parse(Chuoi_Tra_cuu_Money_Greater) > 0)
        {

            long search_amount = long.Parse(Chuoi_Tra_cuu_Money_Greater);
            Danh_sach_Result.ForEach(
                Tai_khoan =>
                {
                    Tai_khoan.Danh_sach_Khoan_thu = Tai_khoan.Danh_sach_Khoan_thu.FindAll(
                        Khoan_thu => Khoan_thu.So_tien > search_amount
                    );
                }
        );
        }
        if (Chuoi_Tra_cuu_Money_Less != null && Chuoi_Tra_cuu_Money_Less != "" && long.Parse(Chuoi_Tra_cuu_Money_Less) > 0)
        {
            long search_amount = long.Parse(Chuoi_Tra_cuu_Money_Less);
            Danh_sach_Result.ForEach(
                Tai_khoan =>
                {
                    Tai_khoan.Danh_sach_Khoan_thu = Tai_khoan.Danh_sach_Khoan_thu.FindAll(
                        Khoan_thu => Khoan_thu.So_tien < search_amount);
                }
            );
        }

        // Ngày tháng
        if (Chuoi_Tra_cuu_Date_Greater != null && Chuoi_Tra_cuu_Date_Greater != "")
        {
            Danh_sach_Result.ForEach(Tai_khoan =>
            {
                Tai_khoan.Danh_sach_Khoan_thu = Tai_khoan.Danh_sach_Khoan_thu.FindAll(
                    Khoan_thu => DateTime.ParseExact(Khoan_thu.Ngay_thu, vnDateFormat, cultureInfo) > DateTime.Parse(Chuoi_Tra_cuu_Date_Greater)
                );
            });
        }

        if (Chuoi_Tra_cuu_Date_Less != null && Chuoi_Tra_cuu_Date_Less != "")
        {
            Danh_sach_Result.ForEach(Tai_khoan =>
            {
                Tai_khoan.Danh_sach_Khoan_thu = Tai_khoan.Danh_sach_Khoan_thu.FindAll(
                    Khoan_thu => DateTime.ParseExact(Khoan_thu.Ngay_thu, vnDateFormat, cultureInfo) < DateTime.Parse(Chuoi_Tra_cuu_Date_Less)
                );
            });
        }
        return Danh_sach_Result;
    }

    public List<XL_TAI_KHOAN> Xu_ly_Tra_cuu_Phieu_chi(List<XL_TAI_KHOAN> Danh_sach_Goc)
    {
        List<XL_TAI_KHOAN> Danh_sach_Result = Danh_sach_Goc;
        // Chuỗi xử lý tra cứu
        var Chuoi_Tra_cuu_Ho_ten = HttpContext.Current.Request["Th_Ho_ten"];
        var Chuoi_Tra_cuu_Date_Greater = HttpContext.Current.Request["Th_Date_Greater"];
        var Chuoi_Tra_cuu_Date_Less = HttpContext.Current.Request["Th_Date_Less"];
        var Chuoi_Tra_cuu_Money_Greater = HttpContext.Current.Request["Th_Money_Greater"];
        var Chuoi_Tra_cuu_Money_Less = HttpContext.Current.Request["Th_Money_Less"];

        // Rút ngắn danh sách theo chuỗi tra cứu
        // Họ tên
        // Lấy những người có chuỗi tra cứu trong họ tên
        if (Chuoi_Tra_cuu_Ho_ten != null && Chuoi_Tra_cuu_Ho_ten != "")
        {
            Danh_sach_Result = Danh_sach_Result.FindAll(
                Tai_khoan =>
                Tai_khoan.Thong_tin_Thanh_vien.Ho_ten.ToUpper()
                .Contains(Chuoi_Tra_cuu_Ho_ten.ToUpper())
            );
        }

        // Số tiền
        // Chỉ xử lý khi không null và nhập vào lớn hơn 0.
        if (Chuoi_Tra_cuu_Money_Greater != null && Chuoi_Tra_cuu_Money_Greater != "" && long.Parse(Chuoi_Tra_cuu_Money_Greater) > 0)
        {

            long search_amount = long.Parse(Chuoi_Tra_cuu_Money_Greater);
            Danh_sach_Result.ForEach(
                Tai_khoan =>
                {
                    Tai_khoan.Danh_sach_Khoan_chi = Tai_khoan.Danh_sach_Khoan_chi.FindAll(
                        Khoan_chi => Khoan_chi.So_tien > search_amount
                    );
                }
        );
        }
        if (Chuoi_Tra_cuu_Money_Less != null && Chuoi_Tra_cuu_Money_Less != "" && long.Parse(Chuoi_Tra_cuu_Money_Less) > 0)
        {
            long search_amount = long.Parse(Chuoi_Tra_cuu_Money_Less);
            Danh_sach_Result.ForEach(
                Tai_khoan =>
                {
                    Tai_khoan.Danh_sach_Khoan_chi = Tai_khoan.Danh_sach_Khoan_chi.FindAll(
                        Khoan_chi => Khoan_chi.So_tien < search_amount);
                }
            );
        }

        // Ngày tháng
        if (Chuoi_Tra_cuu_Date_Greater != null && Chuoi_Tra_cuu_Date_Greater != "")
        {
            Danh_sach_Result.ForEach(Tai_khoan =>
            {
                Tai_khoan.Danh_sach_Khoan_chi = Tai_khoan.Danh_sach_Khoan_chi.FindAll(
                    Khoan_chi => DateTime.ParseExact(Khoan_chi.Ngay_chi, vnDateFormat, cultureInfo) > DateTime.Parse(Chuoi_Tra_cuu_Date_Greater)
                );
            });
        }

        if (Chuoi_Tra_cuu_Date_Less != null && Chuoi_Tra_cuu_Date_Less != "")
        {
            Danh_sach_Result.ForEach(Tai_khoan =>
            {
                Tai_khoan.Danh_sach_Khoan_chi = Tai_khoan.Danh_sach_Khoan_chi.FindAll(
                    Khoan_chi => DateTime.ParseExact(Khoan_chi.Ngay_chi, vnDateFormat, cultureInfo) < DateTime.Parse(Chuoi_Tra_cuu_Date_Less)
                );
            });
        }

        return Danh_sach_Result;
    }

    public string Xu_ly_Them_Khoan_chi(XL_KHOAN_CHI Khoan_chi_Moi)
    {
        string Kq = "";

        // Lấy instance tài khoản của người dùng hiện tại
        XL_NGUOI_DUNG user = (XL_NGUOI_DUNG)HttpContext.Current.Session["Nguoi_dung"];
        XL_TAI_KHOAN tk = Du_lieu_Ung_dung.Danh_sach_Tai_khoan.FirstOrDefault(existing_acc => existing_acc.Ma_so == user.Ma_so);

        // Bắn tài khoản và thông tin khoản chi mới qua bên kia
        // Nghĩa là Gọi hàm ghi dữ liệu ngay tại đây
        Kq = XL_DU_LIEU.Ghi_Them_Khoan_Chi(tk, Khoan_chi_Moi);

        return Kq;
    }
    public string Xu_ly_Them_Khoan_thu(XL_KHOAN_THU Khoan_thu_Moi)
    {
        string Kq = "";

        // Lấy instance tài khoản của người dùng hiện tại
        XL_NGUOI_DUNG user = (XL_NGUOI_DUNG)HttpContext.Current.Session["Nguoi_dung"];
        XL_TAI_KHOAN tk = Du_lieu_Ung_dung.Danh_sach_Tai_khoan.FirstOrDefault(existing_acc => existing_acc.Ma_so == user.Ma_so);

        // Bắn tài khoản và thông tin khoản chi mới qua bên kia
        // Nghĩa là Gọi hàm ghi dữ liệu ngay tại đây
        Kq = XL_DU_LIEU.Ghi_Them_Khoan_thu(tk, Khoan_thu_Moi);

        return Kq;
    }
    public string Xu_ly_Cap_nhat_Khoan_chi(XL_KHOAN_CHI Khoan_chi_Moi)
    {
        string Kq = "";

        // Lấy instance tài khoản của người dùng hiện tại
        XL_NGUOI_DUNG user = (XL_NGUOI_DUNG)HttpContext.Current.Session["Nguoi_dung"];
        XL_TAI_KHOAN tk = Du_lieu_Ung_dung.Danh_sach_Tai_khoan.FirstOrDefault(existing_acc => existing_acc.Ma_so == user.Ma_so);

        // Bắn tài khoản và thông tin khoản chi mới qua bên kia
        // Nghĩa là Gọi hàm ghi dữ liệu ngay tại đây
        Kq = XL_DU_LIEU.Ghi_Cap_nhat_Khoan_chi(tk, Khoan_chi_Moi);

        return Kq;
    }
    public string Xu_ly_Cap_nhat_Khoan_thu(XL_KHOAN_THU Khoan_thu_Moi)
    {
        string Kq = "";

        // Lấy instance tài khoản của người dùng hiện tại
        XL_NGUOI_DUNG user = (XL_NGUOI_DUNG)HttpContext.Current.Session["Nguoi_dung"];
        XL_TAI_KHOAN tk = Du_lieu_Ung_dung.Danh_sach_Tai_khoan.FirstOrDefault(existing_acc => existing_acc.Ma_so == user.Ma_so);

        // Bắn tài khoản và thông tin khoản chi mới qua bên kia
        // Nghĩa là Gọi hàm ghi dữ liệu ngay tại đây
        Kq = XL_DU_LIEU.Ghi_Cap_nhat_Khoan_thu(tk, Khoan_thu_Moi);

        return Kq;
    }

    public string Xu_ly_Xoa_Khoan_thu(XL_KHOAN_THU Khoan_thu_Moi)
    {
        string Kq = "";

        // Lấy instance tài khoản của người dùng hiện tại
        XL_NGUOI_DUNG user = (XL_NGUOI_DUNG)HttpContext.Current.Session["Nguoi_dung"];
        XL_TAI_KHOAN tk = Du_lieu_Ung_dung.Danh_sach_Tai_khoan.FirstOrDefault(existing_acc => existing_acc.Ma_so == user.Ma_so);
        // Xóa nghĩa là set số tiền về 0
        Khoan_thu_Moi.So_tien = 0;
        // Bắn tài khoản và thông tin khoản chi mới qua bên kia
        // Nghĩa là Gọi hàm ghi dữ liệu ngay tại đây
        Kq = XL_DU_LIEU.Ghi_Xoa_Khoan_thu(tk, Khoan_thu_Moi);

        return Kq;
    }
    public string Xu_ly_Xoa_Khoan_chi(XL_KHOAN_CHI Khoan_chi_Moi)
    {
        string Kq = "";

        // Lấy instance tài khoản của người dùng hiện tại
        XL_NGUOI_DUNG user = (XL_NGUOI_DUNG)HttpContext.Current.Session["Nguoi_dung"];
        XL_TAI_KHOAN tk = Du_lieu_Ung_dung.Danh_sach_Tai_khoan.FirstOrDefault(existing_acc => existing_acc.Ma_so == user.Ma_so);
        // Xóa nghĩa là set số tiền về 0
        Khoan_chi_Moi.So_tien = 0;
        // Bắn tài khoản và thông tin khoản chi mới qua bên kia
        // Nghĩa là Gọi hàm ghi dữ liệu ngay tại đây
        Kq = XL_DU_LIEU.Ghi_Xoa_Khoan_chi(tk, Khoan_chi_Moi);

        return Kq;
    }

    public XL_KHOAN_CHI Nhan_Du_lieu_Khoan_chi_Nguoi_dung(string Ma_khoan, string So_tien, string Ngay)
    {
        // Xử lý ngày -.-
        DateTime date = DateTime.Parse(Ngay);
        string day = date.Day.ToString();
        string month = date.Month.ToString();
        string year = date.Year.ToString();
        string ngay = $"{day}-{month}-{year}";


        var Khoan_chi = new XL_KHOAN_CHI()
        {
            Ma_so = Ma_khoan,
            So_tien = long.Parse(So_tien),
            Ngay_chi = ngay
        };
        return Khoan_chi;
    }

    public XL_KHOAN_THU Nhan_Du_lieu_Khoan_thu_Nguoi_dung(string Ma_khoan, string So_tien, string Ngay)
    {
        // Xử lý ngày -.-
        DateTime date = DateTime.Parse(Ngay);
        string day = date.Day.ToString();
        string month = date.Month.ToString();
        string year = date.Year.ToString();
        string ngay = $"{day}-{month}-{year}";


        var Khoan_thu = new XL_KHOAN_THU()
        {
            Ma_so = Ma_khoan,
            So_tien = long.Parse(So_tien),
            Ngay_thu = ngay
        };
        return Khoan_thu;
    }
}



// *********** Data-Layer DL *************
public partial class XL_DU_LIEU
{
    public static string Dia_chi_Dich_vu = "https://localhost:44329";
    public static string Dia_chi_Dich_vu_Du_lieu = $"{Dia_chi_Dich_vu}/1-Dich_vu_Giao_tiep/DV_Chinh.cshtml";

    public static XL_DU_LIEU Doc_Du_lieu()
    {
        var Xu_ly = new WebClient();
        Xu_ly.Encoding = System.Text.Encoding.UTF8;
        var Tham_so = "Ma_so_Xu_ly=KHOI_DONG_DU_LIEU";
        var Dia_chi_Xu_ly = $"{Dia_chi_Dich_vu_Du_lieu}?{Tham_so}";
        var Chuoi_JSON = Xu_ly.DownloadString(Dia_chi_Xu_ly);
        var Du_lieu = Json.Decode<XL_DU_LIEU>(Chuoi_JSON);
        return Du_lieu;
    }

    public static string Ghi_Them_Khoan_Chi(XL_TAI_KHOAN Tai_khoan, XL_KHOAN_CHI Khoan_chi_Moi)
    {
        string Kq = "";
        var Xu_ly = new WebClient();
        Xu_ly.Encoding = System.Text.Encoding.UTF8;

        var Du_lieu_Tai_khoan = Json.Encode(Tai_khoan);
        var Du_lieu_ghi = Json.Encode(Khoan_chi_Moi);

        var Tham_so = $"Ma_so_Xu_ly=GHI_THEM_KHOAN_CHI&Khoan_chi_Moi={Du_lieu_ghi}&Tai_khoan={Du_lieu_Tai_khoan}";

        var Dia_chi_Xu_ly = $"{Dia_chi_Dich_vu_Du_lieu}?{Tham_so}&";
        var Chuoi_Ket_qua = Xu_ly.DownloadString(Dia_chi_Xu_ly);
        return Chuoi_Ket_qua;
    }

    public static string Ghi_Them_Khoan_thu(XL_TAI_KHOAN Tai_khoan, XL_KHOAN_THU Khoan_thu_Moi)
    {
        string Kq = "";
        var Xu_ly = new WebClient();
        Xu_ly.Encoding = System.Text.Encoding.UTF8;

        var Du_lieu_Tai_khoan = Json.Encode(Tai_khoan);
        var Du_lieu_ghi = Json.Encode(Khoan_thu_Moi);

        var Tham_so = $"Ma_so_Xu_ly=GHI_THEM_KHOAN_THU&Khoan_thu_Moi={Du_lieu_ghi}&Tai_khoan={Du_lieu_Tai_khoan}";

        var Dia_chi_Xu_ly = $"{Dia_chi_Dich_vu_Du_lieu}?{Tham_so}&";
        var Chuoi_Ket_qua = Xu_ly.DownloadString(Dia_chi_Xu_ly);
        return Chuoi_Ket_qua;
    }

    public static string Ghi_Cap_nhat_Khoan_thu(XL_TAI_KHOAN Tai_khoan, XL_KHOAN_THU Khoan_thu_Moi)
    {
        string Kq = "";
        var Xu_ly = new WebClient();
        Xu_ly.Encoding = System.Text.Encoding.UTF8;

        var Du_lieu_Tai_khoan = Json.Encode(Tai_khoan);
        var Du_lieu_ghi = Json.Encode(Khoan_thu_Moi);

        var Tham_so = $"Ma_so_Xu_ly=GHI_CAP_NHAT_KHOAN_THU&Khoan_thu_Moi={Du_lieu_ghi}&Tai_khoan={Du_lieu_Tai_khoan}";

        var Dia_chi_Xu_ly = $"{Dia_chi_Dich_vu_Du_lieu}?{Tham_so}&";
        var Chuoi_Ket_qua = Xu_ly.DownloadString(Dia_chi_Xu_ly);
        return Chuoi_Ket_qua;
    }

    public static string Ghi_Cap_nhat_Khoan_chi(XL_TAI_KHOAN Tai_khoan, XL_KHOAN_CHI Khoan_chi_Moi)
    {
        string Kq = "";
        var Xu_ly = new WebClient();
        Xu_ly.Encoding = System.Text.Encoding.UTF8;

        var Du_lieu_Tai_khoan = Json.Encode(Tai_khoan);
        var Du_lieu_ghi = Json.Encode(Khoan_chi_Moi);

        var Tham_so = $"Ma_so_Xu_ly=GHI_CAP_NHAT_KHOAN_CHI&Khoan_chi_Moi={Du_lieu_ghi}&Tai_khoan={Du_lieu_Tai_khoan}";

        var Dia_chi_Xu_ly = $"{Dia_chi_Dich_vu_Du_lieu}?{Tham_so}&";
        var Chuoi_Ket_qua = Xu_ly.DownloadString(Dia_chi_Xu_ly);
        return Chuoi_Ket_qua;
    }
    public static string Ghi_Xoa_Khoan_thu(XL_TAI_KHOAN Tai_khoan, XL_KHOAN_THU Khoan_thu_Moi)
    {
        string Kq = "";
        var Xu_ly = new WebClient();
        Xu_ly.Encoding = System.Text.Encoding.UTF8;

        var Du_lieu_Tai_khoan = Json.Encode(Tai_khoan);
        var Du_lieu_ghi = Json.Encode(Khoan_thu_Moi);

        var Tham_so = $"Ma_so_Xu_ly=GHI_XOA_KHOAN_THU&Khoan_thu_Moi={Du_lieu_ghi}&Tai_khoan={Du_lieu_Tai_khoan}";

        var Dia_chi_Xu_ly = $"{Dia_chi_Dich_vu_Du_lieu}?{Tham_so}&";
        var Chuoi_Ket_qua = Xu_ly.DownloadString(Dia_chi_Xu_ly);
        return Chuoi_Ket_qua;
    }
    public static string Ghi_Xoa_Khoan_chi(XL_TAI_KHOAN Tai_khoan, XL_KHOAN_CHI Khoan_chi_Moi)
    {
        string Kq = "";
        var Xu_ly = new WebClient();
        Xu_ly.Encoding = System.Text.Encoding.UTF8;

        var Du_lieu_Tai_khoan = Json.Encode(Tai_khoan);
        var Du_lieu_ghi = Json.Encode(Khoan_chi_Moi);

        var Tham_so = $"Ma_so_Xu_ly=GHI_XOA_KHOAN_CHI&Khoan_chi_Moi={Du_lieu_ghi}&Tai_khoan={Du_lieu_Tai_khoan}";

        var Dia_chi_Xu_ly = $"{Dia_chi_Dich_vu_Du_lieu}?{Tham_so}&";
        var Chuoi_Ket_qua = Xu_ly.DownloadString(Dia_chi_Xu_ly);
        return Chuoi_Ket_qua;
    }
}