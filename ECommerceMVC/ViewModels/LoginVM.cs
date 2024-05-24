using System.ComponentModel.DataAnnotations;

namespace ECommerceMVC.ViewModels
{
    public class LoginVM
    {
        [Display(Name = "Tên đăng nhập")]
        [Required(ErrorMessage = "*")]
        [MaxLength(20)]
        public string LoginName { get; set; }


        [Display(Name = "Mật khẩu")]
        [Required(ErrorMessage = "*")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
