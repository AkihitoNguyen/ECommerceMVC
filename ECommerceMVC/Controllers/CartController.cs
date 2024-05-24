using ECommerceMVC.Data;
using ECommerceMVC.ViewModels;
using Microsoft.AspNetCore.Mvc;
using ECommerceMVC.Helpers;
using Microsoft.AspNetCore.Authorization;

namespace ECommerceMVC.Controllers
{
    public class CartController : Controller
    {
        private readonly PaypalClient _paypalClient;
        private readonly Hshop2023Context db;

        public CartController(Hshop2023Context context, PaypalClient paypalClient) {
            _paypalClient = paypalClient;
            db = context;
        }

        public List<CartItem> Cart => HttpContext.Session.Get<List<CartItem>>(MySetting.CART_KEY) ?? new List<CartItem>();
        public IActionResult Index()
        {

            return View(Cart);
        }

        public IActionResult AddToCart(int id, int quantity = 1)
        {
            var gioHang = Cart;
            var item = gioHang.SingleOrDefault(p => p.MaHh == id);
            if (item == null)
            {
                var hangHoa = db.HangHoas.SingleOrDefault(p => p.MaHh == id);
                if(hangHoa == null)
                {
                    TempData["Message"] = $"Không tìm thấy hàng hóa có mã {id}";
                    return Redirect("/404");
                }
                item = new CartItem
                {
                    MaHh = hangHoa.MaHh,
                    TenHh = hangHoa.TenHh,
                    DonGia = hangHoa.DonGia ?? 0,
                    Hinh = hangHoa.Hinh ?? string.Empty,
                    SoLuong = quantity
                };
                gioHang.Add(item);
            }
            else
            {
                item.SoLuong += quantity;
            }

            HttpContext.Session.Set(MySetting.CART_KEY, gioHang);
            return RedirectToAction("Index");
        }
        public IActionResult RemoveCart(int id)
        {
            var Giohang = Cart;
            var item = Giohang.SingleOrDefault(g => g.MaHh == id);
            if (item != null)
            {
                Giohang.Remove(item);
                HttpContext.Session.Set(MySetting.CART_KEY, Giohang);
            }
            return RedirectToAction("Index");
        }

        [Authorize]
        [HttpGet]
        public IActionResult Checkout()
        {
            if(Cart.Count == 0)
            {
                return Redirect("/");
            }
            ViewBag.PaypalClientID = _paypalClient.ClientId; 
            return View(Cart);
        }


        [Authorize]
        [HttpPost]
        public IActionResult Checkout(CheckoutVM model)
        {
            if (ModelState.IsValid == true)
            {
                var customerId = HttpContext.User.Claims.SingleOrDefault(p => p.Type == "CustomerID").Value;

                var khachHang = new KhachHang();
                if (model.GiongKhachHang)
                {
                    khachHang = db.KhachHangs.SingleOrDefault(p => p.MaKh == customerId);
                }

                var hoaDon = new HoaDon
                {
                    MaKh = customerId,
                    HoTen = model.HoTen ?? khachHang.HoTen,
                    DiaChi = model.DiaChi ?? khachHang.DiaChi,
                    NgayDat = DateTime.Now,
                    CachThanhToan = "COD",
                    CachVanChuyen = "GRAB",
                    MaTrangThai = 0,
                    GhiChu = model.GhiChu
                };
                    db.Database.BeginTransaction();
                try
                {
                    db.Database.CommitTransaction();
                    db.Add(hoaDon);
                    db.SaveChanges();

                    var cthd = new List<ChiTietHd>();
                    foreach (var item in Cart)
                    {
                        cthd.Add(new ChiTietHd
                        {
                            MaHd = hoaDon.MaHd,
                            SoLuong = item.SoLuong,
                            DonGia = item.DonGia,
                            MaHh = item.MaHh,
                            GiamGia = 0
                        });
                    }
                    db.AddRange(cthd);
                    db.SaveChanges();

                    HttpContext.Session.Set<List<CartItem>>(MySetting.CART_KEY, new List<CartItem>());
                    return View("Success");
                }
                catch (Exception ex)
                {
                    db.Database.RollbackTransaction();
                }
            }
            return View(Cart);
        }

        #region payment Palpal
        [Authorize]
        [HttpPost("/Cart/create-paypal-order")]

        public async Task<IActionResult> CreatePaypalOrder(CancellationToken cancellationToken)
        {
            //Thong tin don hang gui qua paypal
            var tongTien = Cart.Sum(p => p.ThanhTien).ToString();
            var donViTienTe = "USD";
            var maDonHangThamChieu = "DH" + DateTime.Now.Ticks.ToString();

            try
            {
                var respone = await _paypalClient.CreateOrder(tongTien, donViTienTe, maDonHangThamChieu);

                return Ok(respone);
            }catch (Exception ex)
            {
                var error = new
                {
                    ex.GetBaseException().Message
                };
                return BadRequest(error);
            }
        }

        [Authorize, HttpPost("/Cart/capture-paypal-order")]
        public async Task<IActionResult> CapturePaypalOrder(string orderID, CancellationToken cancellationToken)
        {
            try
            {
                var respone = await _paypalClient.CaptureOrder(orderID);

                //Luu database neu thanh cong

                return Ok(respone);

            }catch (Exception ex)
            {
                var error = new
                {
                    ex.GetBaseException().Message
                };
                return BadRequest(error);
            }
        }
        #endregion
    }

}

