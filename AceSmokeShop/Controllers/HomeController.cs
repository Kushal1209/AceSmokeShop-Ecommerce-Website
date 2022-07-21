using AceSmokeShop.Areas.Identity.Data;
using AceSmokeShop.Core.IConfiguration;
using AceSmokeShop.Core.Repositories;
using AceSmokeShop.Data;
using AceSmokeShop.Models;
using AceSmokeShop.Services;
using AceSmokeShop.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Tasks;
using Stripe;
using System;

namespace AceSmokeShop.Controllers
{
    public class HomeController : Controller
    {
        public UHomeViewModel _uHomeViewModel;
        public UProductViewModel _uProductViewModel;
        public WebServices _webServices;
        public PaymentServices _paymentServices;
        public ProductDescriptionViewModel _productDescriptionViewModel;
        private readonly UserManager<AppUser> _userManager;
        public CartInputModel Input { get; set; }
        public HomeController(ILogger<HomeController> logger, UserManager<AppUser> userManager,
            DBContext context)
        {
            _userManager = userManager;
            _uHomeViewModel = new UHomeViewModel();
            _uProductViewModel = new UProductViewModel();
            _productDescriptionViewModel = new ProductDescriptionViewModel();
            Input = new CartInputModel();
            _paymentServices = new PaymentServices(new ProductRepository(context, logger),
                new CategoryRepository(context, logger), new SubCategoryRepository(context, logger),
                new StateRepository(context, logger), userManager, new CartRepository(context, logger), 
                new AddressRepository(context, logger), new TransactionRepository(context, logger));
            _webServices = new WebServices(new ProductRepository(context, logger),
                new CategoryRepository(context, logger), new SubCategoryRepository(context, logger),
                new StateRepository(context, logger), userManager, new CartRepository(context, logger), new AddressRepository(context, logger), _paymentServices,
                new UserOrdersRepository(context, logger), new OrderItemRepository(context, logger), new TransactionRepository(context, logger));
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if(user != null && user.UserRole == "ADMIN")
            {
                return RedirectToAction("Index", "Admin");
            }
            _uHomeViewModel = _webServices.GetHomeViewModel(user);

            return View("Index",_uHomeViewModel);
        }

        public async Task<IActionResult> Products(int CategoryID = 0, int SubCategoryID = 0, int pageFrom = 1, int pageTotal = 10, 
                                                string search = "", string type = "")
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                user = new AppUser();
            }

            _uProductViewModel = _webServices.GetUserProductsViewModel(user, CategoryID, SubCategoryID, pageFrom, pageTotal, search,
                type);
            _uProductViewModel.CategorySelect.CategoryID = CategoryID;
            _uProductViewModel.SubCategorySelect.SubCategoryID = SubCategoryID;
            _uProductViewModel.Search = search;
            return View("Products", _uProductViewModel);
        }


        public async Task<IActionResult> AddtoCart(CartInputModel cartInputModel)
        {
            var user = await _userManager.GetUserAsync(User);
            if(user == null)
            {
                return StatusCode(401);
            }
            var result = _webServices.AddtoCart(user, cartInputModel.ProductId, cartInputModel.Quantity);
            if (result.ToLower().Contains("success"))
            {
                return StatusCode(200, "Successfully Added to Cart");
            }
            return StatusCode(500, result);
        }

        public async Task<int> GetShipping()
        {
            var user = await _userManager.GetUserAsync(User);
            var shipping = _webServices.GetShipping(user);

            return shipping;
        }

        public async Task<IActionResult> ProductDetails(string productid = "")
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                user = new AppUser();
            }
            _productDescriptionViewModel = _webServices.GetProductDetailsViewModel(user, productid);
            return View("ProductDetails", _productDescriptionViewModel);
        }

        public async Task<int> CartCounter()
        {
            var user = await _userManager.GetUserAsync(User);
            var cartcount = _webServices.GetCartCount(user);

            return cartcount;
        }

        [HttpGet]
        public async Task<IActionResult> MyCart()
        {
            var user = await _userManager.GetUserAsync(User);
            if(user != null)
            {
                var model = _webServices.GetMyCart(user);
                foreach (var item in model)
                {
                    item.Product.BasePrice = 0;
                    if (user.UserRole.ToLower() != "vendor")
                    {
                        item.Product.VendorPrice = 0;
                    }
                }
                return View("MyCart", model);
            }
            else
            {
                _uHomeViewModel = _webServices.GetHomeViewModel(user);

                return View("Index", _uHomeViewModel);
            }
        }
        
        public async Task<IActionResult> EditCartQuantity(int CartId, int Quantity)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user != null)
            {
                var str = _webServices.EditCartQty(user, CartId, Quantity);
                if (str.ToLower().Contains("success"))
                {
                    return StatusCode(200, str); 
                }
                return StatusCode(500, str);
            }
            else
            {
                _uHomeViewModel = _webServices.GetHomeViewModel(user);

                return View("Index", _uHomeViewModel);
            }
        }

        public async Task<IActionResult> RemoveFromCart(int CartId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user != null)
            {
                var str = _webServices.RemoveFromCart(user, CartId);
                if (str.ToLower().Contains("success"))
                {
                    return StatusCode(200, str);
                }
                return StatusCode(500, str);
            }
            else
            {
                _uHomeViewModel = _webServices.GetHomeViewModel(user);

                return View("Index", _uHomeViewModel);
            }
        }

        public async Task<IActionResult> PlaceOrder(int cardId, string productId = "", int qty = 0, bool pickatstore = false)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user != null)
            {
                var str = "";

                if(productId.Length > 0)
                {
                    if(qty > 0)
                    {
                        str = _webServices.PlaceProductOrder(user, cardId, productId, qty, pickatstore);
                        if (str.ToLower().Contains("success"))
                        {
                            return StatusCode(200, str);
                        }
                        return StatusCode(500, str);
                    }
                    else
                    {
                        return StatusCode(500, "InValid Quantity");
                    }
                }
                else
                {
                    str = _webServices.PlaceCartOrder(user, cardId, pickatstore);
                    if (str.ToLower().Contains("success"))
                    {
                        return StatusCode(200, str);
                    }
                    return StatusCode(500, str);
                }
               
            }
            else
            {
                _uHomeViewModel = _webServices.GetHomeViewModel(user);

                return View("Index", _uHomeViewModel);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Checkout(string ProductId = "", int Quantity = 1)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var model = _webServices.GetCheckoutViewModel(user, ProductId, Quantity);

                if(model.Subtotal == 0)
                {
                    _uHomeViewModel = _webServices.GetHomeViewModel(user);

                    return View("Index", _uHomeViewModel);
                }

                return View("Checkout", model);
            }
            else
            {
                return Redirect("/Identity/Account/Login?ReturnUrl=%2FHome/Checkout?productid=" + ProductId + "&Quantity=" + Quantity);
            }
        }

        public async Task<IActionResult> SetAsShipping(int addressId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var result = _webServices.SetAsShipping(addressId, user);
                if (result.ToLower().Contains("success"))
                {
                    return StatusCode(200);
                }
                return StatusCode(500, result);
            }
            else
            {
                return StatusCode(401, "Unauthorized");
            }
        }

        [HttpGet]
        public async Task<IActionResult> AddAddressGet()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {

                var model = new CheckoutViewModel();
                model.Address.StateList = _webServices.GetStateList();
                model.Address.StateList.Insert(0, new State { StateID = 0, StateName = "Select State" });
                return View("AddAddress", model);
            }
            else
            {
                _uHomeViewModel = _webServices.GetHomeViewModel(user);
                return View("Index", _uHomeViewModel);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAddress(CheckoutViewModel checkoutViewModel)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var result = _webServices.AddNewAddress(user, checkoutViewModel.Address.newAddress);
                if (result.ToLower().Contains("success"))
                {
                    return StatusCode(200);
                }
                else
                {
                    return StatusCode(500, result);
                }
            }
            else
            {
                return StatusCode(401, "Unauthorized");
            }
        }

        [HttpGet]
        public IActionResult AddCardGet()
        {
            return View("AddCard");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCard(PaymentCardViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var result = _paymentServices.AddCard(user, model);

                if (result.ToLower().Contains("success"))
                {
                    return StatusCode(200);
                }

                return StatusCode(500, result);
            }
            else
            {
                return StatusCode(401, "Unauthorized");
            }
        }

        public async Task<IActionResult> RemoveAddress(int AddressId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user != null)
            {
                var result = _webServices.RemoveAddress(user, AddressId);
                if (result.ToLower().Contains("success"))
                {
                    return StatusCode(200, result);
                }
                return StatusCode(500, result);
            }
            else
            {
                _uHomeViewModel = _webServices.GetHomeViewModel(user);

                return View("Index", _uHomeViewModel);
            }
        }

        public async Task<IActionResult> RemoveCard(int CardId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user != null)
            {
                var result = _paymentServices.RemoveCard(user, CardId);
                if (result.ToLower().Contains("success"))
                {
                    return StatusCode(200, result);
                }
                return StatusCode(500, result);
            }
            else
            {
                _uHomeViewModel = _webServices.GetHomeViewModel(user);

                return View("Index", _uHomeViewModel);
            }
        }

        public async Task<IActionResult> MyOrders(string OrderStatus = "", string ShippingStatus = "", string PaymentStatus = "", string Search = "", string datefrom = "", string dateto = "")
        {
            var user = await _userManager.GetUserAsync(User);

            if (user != null)
            {
                DateTime DateFrom = datefrom == null || datefrom == "" ? DateTime.MinValue : DateTime.Parse(datefrom);
                DateTime DateTo;
                if (dateto == null || dateto == "")
                {
                    DateTo = DateTime.MaxValue;
                }
                else
                {
                    DateTo = DateTime.Parse(dateto);
                    DateTo = new DateTime(DateTo.Year, DateTo.Month, DateTo.Day, 23, 59, 59);

                }
                OrderStatus = OrderStatus == null || OrderStatus.ToLower().Contains("select") ? "" : OrderStatus.ToLower();
                ShippingStatus = ShippingStatus == null || ShippingStatus.ToLower().Contains("select") ? "" : ShippingStatus.ToLower();
                PaymentStatus = PaymentStatus == null || PaymentStatus.ToLower().Contains("select") ? "" : PaymentStatus.ToLower();
                Search = Search == null || Search.Length <= 1 ? "" : Search.ToLower();

                var model = _webServices.GetOrdersViewModel(user, OrderStatus, ShippingStatus, PaymentStatus, Search, DateFrom, DateTo);

                return View("MyOrders", model);
            }
            else
            {
                return Redirect("/Identity/Account/Login?ReturnUrl=%2FHome/MyOrders");
            } 
        }

        [HttpGet]
        public async Task<IActionResult> CancelOrderGet(string orderid)
        {
            var user = await _userManager.GetUserAsync(User);
            var model = new OrderViewModel();
            model.OrderSelectId = orderid;
            if (user != null)
            {
                var result = _webServices.CancelOrderValidation(user, orderid);
                if (result.ToLower().Contains("success"))
                {
                    return View("CancelOrder", model);
                }

                return StatusCode(500, result);
            }
            else
            {
                _uHomeViewModel = _webServices.GetHomeViewModel(user);

                return View("Index", _uHomeViewModel);
            }
        }

        public async Task<IActionResult> CancelOrder(OrderViewModel orderViewModel)
        {
            var orderId = orderViewModel.OrderSelectId;
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var result = _webServices.CancelOrderValidation(user, orderId);
                if (result.ToLower().Contains("success"))
                {
                    result = _webServices.CancelOrder(orderId);
                    if (result.ToLower().Contains("success"))
                    {
                        return StatusCode(200);
                    }
                    return StatusCode(500, result);
                }
                return StatusCode(500, result);
            }
            else
            {
                _uHomeViewModel = _webServices.GetHomeViewModel(user);

                return View("Index", _uHomeViewModel);
            }
        }

        [HttpGet]
        public async Task<IActionResult> PayNowGet(string CustOrderId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _uHomeViewModel = _webServices.GetHomeViewModel(user);

                return View("Index", _uHomeViewModel);
            }
            var result = _webServices.GetPayNowViewModel(user, CustOrderId);
           
            if(result == null)
            {
                _uHomeViewModel = _webServices.GetHomeViewModel(user);

                return View("Index", _uHomeViewModel);
            }

            return View("PayNow", result);
        }

        public async Task<IActionResult> PayNow(int cardId, string orderId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return StatusCode(401);
            }

            var result = _webServices.PayNow(user, cardId, orderId);

            if (result.ToLower().Contains("success"))
            {
                return StatusCode(200);
            }


            _uHomeViewModel = _webServices.GetHomeViewModel(user);

            return View("Index", _uHomeViewModel);

        }

        public IActionResult ContactUs()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
