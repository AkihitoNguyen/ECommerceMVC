using AutoMapper;
using ECommerceMVC.Data;
using ECommerceMVC.Helpers;
using ECommerceMVC.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Security.Claims;

namespace ECommerceMVC.Controllers
{
    public class KhachHangController : Controller
    {
        private readonly Hshop2023Context db;
        private readonly IMapper _mapper;

        public KhachHangController(Hshop2023Context context, IMapper mapper) { db = context; _mapper = mapper; }


        [HttpGet]
        public IActionResult DangKy()
        {

            return View();
        }
        #region Register
        [HttpPost]
        public IActionResult DangKy(RegisterVM model, IFormFile Hinh)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var KhachHang = _mapper.Map<KhachHang>(model);
                    KhachHang.RandomKey = MyUtil.GenerateRandomKey();
                    KhachHang.MatKhau = model.MatKhau.ToMd5Hash(KhachHang.RandomKey);
                    KhachHang.HieuLuc = true; //se xu li khi dung Mail de active
                    KhachHang.VaiTro = 0;

                    if (Hinh != null)
                    {
                        KhachHang.Hinh = MyUtil.UploadHinh(Hinh, "KhachHang");
                    }

                    db.Add(KhachHang);
                    db.SaveChanges();
                    return RedirectToAction("Index", "HangHoa");
                }
                catch (Exception ex) { }
            }
            return View();
        }

        #endregion

        #region Login
        [HttpGet]
        public IActionResult DangNhap(string? ReturnUrl)
        {
            ViewBag.ReturnUrl = ReturnUrl;
            return View();
        }

        #endregion

        [HttpPost]
        public async Task<IActionResult> DangNhap(LoginVM model, string? ReturnUrl)
        {
            ViewBag.ReturnUrl = ReturnUrl;
            if (ModelState.IsValid)
            {
                var khachHang = db.KhachHangs.SingleOrDefault(kh => kh.MaKh == model.LoginName);

                if (khachHang == null)
                {
                    ModelState.AddModelError("Loi", "Không có khách hàng này"); ;
                }
                else
                {
                    if (!khachHang.HieuLuc)
                    {
                        ModelState.AddModelError("Loi", "Tài khoản đã bị khóa. Vui lòng liên hệ Admin");
                    }
                    else
                    {
                        if (khachHang.MatKhau == model.Password.ToMd5Hash(khachHang.RandomKey))
                        {
                            ModelState.AddModelError("Loi", "Sai thong tin dang nhap");
                        }
                        else
                        {
                            var claims = new List<Claim>
                            {
                                new Claim(ClaimTypes.Email, khachHang.Email),
                                new Claim(ClaimTypes.Name, khachHang.HoTen),
                                new Claim("CustomerID", khachHang.MaKh),
                                new Claim(ClaimTypes.Role, "Customer")
                            };

                            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                            await HttpContext.SignInAsync(claimsPrincipal);

                            if (Url.IsLocalUrl(ReturnUrl))
                            {
                                return Redirect(ReturnUrl);
                            }
                            else
                            {
                                return Redirect("/");
                            }
                        }
                    }
                }
            }
            return View();
        }

        [Authorize]
        public IActionResult Profile()
        {
            return View();
        }


        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return Redirect("/");
        }

    }

    }

