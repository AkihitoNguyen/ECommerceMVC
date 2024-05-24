using System.ComponentModel.DataAnnotations;

namespace ECommerceMVC.ViewModels
{
    public class RegisterVM
    {
        [Display(Name = "Tên đăng nhập")]
        [Required(ErrorMessage = "*")]
        [MaxLength(20, ErrorMessage = "Tối đa 20 kí tự")]
        public string MaKh { get; set; } = null!;
        
        [Required(ErrorMessage = "*")]
        [Display(Name = "Mật Khẩu")]
        [DataType(DataType.Password)]
        public string MatKhau { get; set; }

        [Required(ErrorMessage = "*")]
        [Display(Name = "Họ và tên")]
        [MaxLength(50, ErrorMessage = "Tối đa 50 kí tự")]
        public string HoTen { get; set; }

       
        public bool GioiTinh { get; set; } = true;

        [DataType(DataType.Date)]
        [Display(Name = "Ngày Sinh")]
        public DateTime? NgaySinh { get; set; }

        [MaxLength(60, ErrorMessage = "Tối đa 60 kí tự")]
        public string DiaChi { get; set; }

        [MaxLength(24, ErrorMessage = "Tối đa 24 kí tự")]
        [RegularExpression(@"0[9875]\d{8}", ErrorMessage = ("Chưa đúng định dạng"))]
        public string DienThoai { get; set; }

        [EmailAddress(ErrorMessage ="Chưa đúng định dạng email")]
        public string Email { get; set; } = null!;

        public string? Hinh { get; set; }

    }
}
