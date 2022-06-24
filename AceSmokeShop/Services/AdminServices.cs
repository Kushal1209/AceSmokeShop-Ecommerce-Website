using AceSmokeShop.Areas.Identity.Data;
using AceSmokeShop.Core.Repositories;
using AceSmokeShop.Models;
using AceSmokeShop.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace AceSmokeShop.Services
{
    public class AdminServices
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ProductRepository _productRepository;
        private readonly CategoryRepository _categoryRepository;
        private readonly SubCategoryRepository _subcategoryRepository;
        private readonly StateRepository _stateRepository;
        private readonly CartRepository _cartRepository;
        private readonly AddressRepository _addressRepository;
        private readonly UserOrdersRepository _userOrdersRepository;
        private readonly OrderItemRepository _orderItemRepository;
        private readonly TransactionRepository _transactionRepository;
        private PaymentServices _paymentServices;
        private int ProductCount = 0;
        private int UserCount = 0;
        private int UserOrderCount = 0;
        public AdminUserOrderViewModel model = new AdminUserOrderViewModel();
        public AdminServices(ProductRepository productRepository, CategoryRepository categoryRepository, SubCategoryRepository subcategoryRepository,
                StateRepository stateRepository, UserManager<AppUser> userManager, CartRepository cartRepository, AddressRepository addressRepository,
                PaymentServices paymentServices, UserOrdersRepository userOrdersRepository, OrderItemRepository orderItemRepository, TransactionRepository transactionRepository)
        {
            //_userManager = userManager;
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _subcategoryRepository = subcategoryRepository;
            _stateRepository = stateRepository;
            _userManager = userManager;
            _cartRepository = cartRepository;
            _addressRepository = addressRepository;
            _paymentServices = paymentServices;
            _userOrdersRepository = userOrdersRepository;
            _orderItemRepository = orderItemRepository;
            _transactionRepository = transactionRepository;
        }

        public AdminFinancialViewModel GetAdminFinancialViewModel(AppUser user, int pageFrom, int pageTotal, int period,
                         DateTime chooseDateFrom, DateTime chooseDateTo)
        {
            var model = new AdminFinancialViewModel();
            List<string> months = new List<string> { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sept", "Oct", "Nov", "Dec" };
            if (user == null || user.UserRole != "ADMIN")
            {
                return null;
            }

            model.currentBalance = _paymentServices.GetCurrentBalance(user);
            model.Period = period;
            model.CurrentPage = pageFrom;
            model.ItemsPerPage = pageTotal;
            model.TotalTransactions = _transactionRepository._dbSet.Where(x => x.CreateDate >= chooseDateFrom && x.CreateDate <= chooseDateTo).Count();
            var dateto = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59);
            if(period != 2 && period != 3)
            {
                var temp = DateTime.Now.AddMonths(-1);
                var datefrom = new DateTime(temp.Year, temp.Month, temp.Day, 0, 0, 0); 

                var days = Math.Ceiling((dateto - datefrom).TotalDays);
                var templist = _transactionRepository._dbSet.Where(x => x.CreateDate >= datefrom && x.CreateDate <= dateto && !x.Status.Equals("Refunded")).ToList();

                var xstr = "";
                var ystr = "";

                for (int i = 0; i<=days; i++)
                {
                    var tempDateStart = datefrom.AddDays(i);
                    var tempDateEnd = tempDateStart.AddHours(24).AddSeconds(-1);
                    xstr += tempDateStart.Day + "-" + months[tempDateStart.Month - 1] + ",";
                    ystr += (int)Math.Ceiling(templist.Where(x => x.CreateDate >= tempDateStart && x.CreateDate <= tempDateEnd).Select(x => x.Amount).Sum()) + ",";
                }
                    
                model.xValues = xstr.Remove(xstr.Length - 1);
                model.yValues = ystr.Remove(ystr.Length - 1);
                    
            }
            else if (period == 2)
            {
                var temp = DateTime.Now.AddMonths(-6);
                var datefrom = new DateTime(temp.Year, temp.Month, temp.Day, 0, 0, 0);

                var totalmonths = 6;
                var templist = _transactionRepository._dbSet.Where(x => x.CreateDate >= datefrom && x.CreateDate <= dateto && !x.Status.Equals("Refunded")).ToList();

                var xstr = "";
                var ystr = "";

                for (int i = 0; i <= totalmonths; i++)
                {
                    var tempDateStart = datefrom.AddMonths(i);
                    var tempDateEnd = tempDateStart.AddMonths(1);
                    xstr += months[tempDateStart.Month - 1] + ",";
                    ystr += (int)Math.Ceiling(templist.Where(x => x.CreateDate >= tempDateStart && x.CreateDate <= tempDateEnd).Select(x => x.Amount).Sum()) + ",";
                }

                model.xValues = xstr.Remove(xstr.Length - 1);
                model.yValues = ystr.Remove(ystr.Length - 1);

            }
            else if (period == 3)
            {

                var temp = DateTime.Now.AddMonths(-12);
                var datefrom = new DateTime(temp.Year, temp.Month, temp.Day, 0, 0, 0);

                var totalmonths = 12;
                var templist = _transactionRepository._dbSet.Where(x => x.CreateDate >= datefrom && x.CreateDate <= dateto && !x.Status.Equals("Refunded")).ToList();

                var xstr = "";
                var ystr = "";

                for (int i = 0; i <= totalmonths; i++)
                {
                    var tempDateStart = datefrom.AddMonths(i);
                    var tempDateEnd = tempDateStart.AddMonths(1);
                    xstr += months[tempDateStart.Month - 1] + ",";
                    ystr += (int)Math.Ceiling(templist.Where(x => x.CreateDate >= tempDateStart && x.CreateDate <= tempDateEnd).Select(x => x.Amount).Sum()) + ",";
                }

                model.xValues = xstr.Remove(xstr.Length - 1);
                model.yValues = ystr.Remove(ystr.Length - 1);
            }
               
            pageFrom--;
            model.ListofTransactions = _transactionRepository._dbSet.Where(x => x.CreateDate >= chooseDateFrom && x.CreateDate <= chooseDateTo).OrderByDescending(x => x.Id).Skip(pageTotal * pageFrom).Take(pageTotal).ToList();
            model.TotalRevenue = model.ListofTransactions.Where(x => !x.Status.Equals("Refunded")).Select(x => x.Amount).Sum();
            model.VendorRevenue = model.ListofTransactions.Where(x => x.UserRole == "VENDOR" && !x.Status.Equals("Refunded")).Select(x => x.Amount).Sum();
            model.UserRevenue = (model.TotalRevenue - model.VendorRevenue);
            model.TaxCollected = (model.TotalRevenue / 1.0625);
            model.CardPayment = model.ListofTransactions.Where(x => !x.Status.Equals("Refunded") && x.PaymentMethod.Equals("Card")).Select(x => x.Amount).Sum();
            model.InStorePayment = model.TotalRevenue - model.CardPayment;
            model.TotalPages = (int)Math.Ceiling((double)((double)model.TotalTransactions / (double)pageTotal));
            model.DateFrom = chooseDateFrom;
            model.DateTo = chooseDateTo;

            return model;
        }

        public async Task<AdminProductViewModel> GetAdminProductViewModelAsync(AppUser user, int CategoryId = 0, int SubCategoryId = 0, int Min = 0,
                                                 int Max = 10000, string search = "", int sortBy = 0,
                                                 int sortByOrder = 1, int pageFrom = 1, int pageTotal = 20)
        {
            var model = new AdminProductViewModel();
            if (user.UserRole != "ADMIN" || user.LockoutEnabled)
            {
                return model;
            }
            else
            {
                model.ProductList = await GetFilteredProductList(user, CategoryId, SubCategoryId, Min, Max, search, sortBy, sortByOrder, pageFrom, pageTotal);
                model.CategoryList = await GetCategoryListAsync(user);
                model.TotalProducts = ProductCount;
                model.OutOfStock = _productRepository._dbSet.Where(x => x.Stock == 0).Count();
                model.RunningOutOfStock = _productRepository._dbSet.Where(x => x.Stock < 5).Count();
                model.TotalUnits = _productRepository._dbSet.Sum(x => x.Stock);

                model.CurrentPage = pageFrom;
                model.ItemsPerPage = pageTotal;
                model.TotalPages = (int)Math.Ceiling((double)((double)model.TotalProducts / (double)pageTotal));

                return model;
            }

            throw new NotImplementedException();
        }

        public AdminOrderDetailsViewModel GetOrderDetails(string order)
        {
            if(order == null || order.Length < 3)
            {
                return new AdminOrderDetailsViewModel();
            }

            var model = new AdminOrderDetailsViewModel();
            var userOrder = _userOrdersRepository._dbSet.Where(x => x.CustOrderId == order).FirstOrDefault();
            if(userOrder == null)
            {
                return model;
            }
            model.ShippingAddress = _addressRepository._dbSet.Where(x => x.Id == userOrder.ShippingAddressId).FirstOrDefault();
            model.ListOfOrderItem = _orderItemRepository._dbSet.Where(x => x.CustOrderId == order).Include(x => x.Product).Include(x => x.Product.Category).Include(x => x.Product.SubCategory).ToList();
            
            if(model.ShippingAddress.AddressLineA.ToLower().Contains("store address") || (model.ShippingAddress.AddressLineB!= null && model.ShippingAddress.AddressLineB.ToLower().Contains("store address"))) 
            {
                model.ShippingAddress = null;
            }
            
            model.userOrder = userOrder;
            model.States = _stateRepository._dbSet.ToList();
            return model;
        }

        public string GetOrderStatus(AppUser user, string orderid)
        {
            if (user.UserRole != null && user.UserRole == "ADMIN")
            {
                return _userOrdersRepository._dbSet.Where(x => x.CustOrderId.ToLower().Equals(orderid.ToLower())).FirstOrDefault().OrderStatus;
            }
            return "";
        }

        public async Task<string> EditUserOrderStatus(AppUser user, string orderid, string orderstatus)
        {
            if (user.UserRole != "ADMIN" || user.LockoutEnabled)
            {
                return "UnAuthorized";
            }
            else
            {
                try
                {
                    var userorder = _userOrdersRepository._dbSet.Where(x => x.CustOrderId == orderid).FirstOrDefault();
                    
                    //if cancelled, create refund
                    if (userorder.OrderStatus.ToLower().Contains("delivered"))
                    {
                        return "Fail: Cannot change status of a Delivered Item";
                    }
                    userorder.OrderStatus = orderstatus;
                    if (orderstatus.ToLower().Contains("deliver"))
                    {
                        userorder.DeliveryDate = DateTime.Now;
                    }
                    if (userorder.OrderStatus.ToLower().Contains("cancelled"))
                    {
                        var refund = _paymentServices.CreateRefund(userorder.PaymentId, "duplicate");
                        userorder.OrderItems = _orderItemRepository._dbSet.Where(x => x.CustOrderId == orderid).Include(x => x.Product).ToList();
                        var productlist = new List<Product>();
                        foreach(var item in userorder.OrderItems)
                        {
                            item.Product.Stock += item.Quantity;
                            productlist.Add(item.Product);
                        }

                        _productRepository._dbSet.UpdateRange(productlist);
                        _productRepository._context.SaveChanges();
                    }

                    _userOrdersRepository._dbSet.Update(userorder);
                    var result = await _userOrdersRepository._context.SaveChangesAsync();

                    return "Success";
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            }
        }

        public string DeleteVendorOrder(AppUser user, int items, int orderId)
        {
            if (user.UserRole != "ADMIN" || user.LockoutEnabled)
            {
                return "UnAuthorized!";
            }
            else
            {
                var thisVendorOrder = _orderItemRepository._dbSet.Where(x => x.Id == orderId).Include(x => x.Product).FirstOrDefault();
                thisVendorOrder.UserOrders = _userOrdersRepository._dbSet.Where(x => x.CustOrderId == thisVendorOrder.CustOrderId).FirstOrDefault();
                if (thisVendorOrder == null || thisVendorOrder.UserOrders == null)
                {
                    return "Fail: Something Went Wrong";
                }
                if (thisVendorOrder.UserOrders.IsPaid == false && thisVendorOrder.UserOrders.OrderStatus.ToLower().Contains("place"))
                {
                    if(items <= 1)
                    {
                        //Change Order Status to Cancelled and Return
                        thisVendorOrder.UserOrders.OrderStatus = "Cancelled";
                        _userOrdersRepository._dbSet.Update(thisVendorOrder.UserOrders);
                        _userOrdersRepository._context.SaveChanges();

                        return "Success";
                    }
                    else
                    {
                        //Delete from DB
                        //Reduce SubTotal and GrandTotal in User Orders and return
                        double oldPrice = thisVendorOrder.Price * thisVendorOrder.Quantity;
                        _orderItemRepository._dbSet.Remove(thisVendorOrder);
                        _orderItemRepository._context.SaveChanges();
                        double newPrice = 0;
                        var userorder = thisVendorOrder.UserOrders;
                        userorder.SubTotal += (newPrice - oldPrice);
                        userorder.TotalAmount = userorder.SubTotal + (userorder.SubTotal * userorder.Tax / 100);

                        _userOrdersRepository._dbSet.Update(userorder);
                         _userOrdersRepository._context.SaveChanges();
                    }
                   

                    return "Success";
                }
                else
                {
                    return "Fail: Error occured while deleting Order Item!";
                }
            }
        }

        public string UploadProductImages(AppUser user, int prodId, IFormFile primary, IFormFile second, IFormFile third)
        {
            if(user == null || user.UserRole != "ADMIN")
            {
                return "UnAuthorized";
            }

            var product = _productRepository._dbSet.Where(x => x.ProductID == prodId).FirstOrDefault();
            var defaultImage = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTl20Tuf0ZhpbNa24X1Kaw3NgZ1Rhy2bdbkGw&usqp=CAU";

            if (product == null) return "Fail";

            var img1 = (product.PrimaryImage != null && product.PrimaryImage.Length > 5) ? product.PrimaryImage : defaultImage;
            var img2 = (product.SecondaryImage1 != null && product.SecondaryImage1.Length > 5) ? product.SecondaryImage1 : defaultImage;
            var img3 = (product.SecondaryImage2 != null && product.SecondaryImage2.Length > 5) ? product.SecondaryImage2 : defaultImage;

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(" https://api.imgbb.com/1/upload?key=fa4f68b086423d3975452aa7d7676daf");
                MultipartFormDataContent multiContent = new MultipartFormDataContent();
                byte[] data;
                HttpResponseMessage result = new HttpResponseMessage();

                //Image 1
                try
                {
                    using (var br = new BinaryReader(primary.OpenReadStream()))
                        data = br.ReadBytes((int)primary.OpenReadStream().Length);
                    multiContent.Add(new ByteArrayContent(data), "image", primary.FileName);
                    result = client.PostAsync("", multiContent).Result;
                    if (result.IsSuccessStatusCode)
                    {
                        var response = JsonConvert.DeserializeObject<ImageAPIResponse>(result.Content.ReadAsStringAsync().Result);
                        img1 = response.data.display_url;
                    }
                }
                catch (Exception)
                {
                }
                

                //Image 2
                try
                {
                    using (var br = new BinaryReader(second.OpenReadStream()))
                        data = br.ReadBytes((int)second.OpenReadStream().Length);
                    multiContent = new MultipartFormDataContent();
                    multiContent.Add(new ByteArrayContent(data), "image", second.FileName);
                    result = client.PostAsync("", multiContent).Result;
                    if (result.IsSuccessStatusCode)
                    {
                        var response = JsonConvert.DeserializeObject<ImageAPIResponse>(result.Content.ReadAsStringAsync().Result);
                        img2 = response.data.display_url;
                    }
                }
                catch (Exception)
                {
                }
                

                //Image 3
                try
                {
                    using (var br = new BinaryReader(third.OpenReadStream()))
                        data = br.ReadBytes((int)third.OpenReadStream().Length);
                    multiContent = new MultipartFormDataContent();
                    multiContent.Add(new ByteArrayContent(data), "image", third.FileName);
                    result = client.PostAsync("", multiContent).Result;
                    if (result.IsSuccessStatusCode)
                    {
                        var response = JsonConvert.DeserializeObject<ImageAPIResponse>(result.Content.ReadAsStringAsync().Result);
                        img3 = response.data.display_url;
                    }
                }
                catch (Exception)
                {
                }

                if(product != null)
                {
                    product.PrimaryImage = img1;
                    product.SecondaryImage1 = img2;
                    product.SecondaryImage2 = img3;

                    _productRepository._dbSet.Update(product);
                    _productRepository._context.SaveChanges();
                }

                return product.Barcode;

            }
        }

        public string MarkAsPaid(AppUser user, string custOrderId)
        {
            if (user == null || user.UserRole != "ADMIN")
            {
                return "UnAuthorized!";
            }

            var userorder = _userOrdersRepository._dbSet.Where(x => x.CustOrderId == custOrderId).FirstOrDefault();

            if(userorder == null)
            {
                return "Err: This user doesn't exist";
            }
            if (userorder.OrderStatus.ToLower().Contains("cancel"))
            {
                return "Fail: Cannot accept payment for cancelled Orders";
            }
            if (userorder.IsPaid)
            {
                return "Fail: This order is already PaidOff";
            }

            userorder.IsPaid = true;

            _userOrdersRepository._dbSet.Update(userorder);
            _userOrdersRepository._context.SaveChanges();

            var createpayment = new Transactions();
            createpayment.UserId = userorder.UserId;
            createpayment.OrderId = custOrderId;
            createpayment.Amount = userorder.TotalAmount;
            createpayment.UserRole = "VENDOR";
            createpayment.CreateDate = DateTime.Now;
            createpayment.PaymentMethod = "PAID AT STORE";
            createpayment.TransactionType = "Full";
            createpayment.Status = "Completed";

            _transactionRepository._dbSet.Add(createpayment);
            _transactionRepository._context.SaveChanges();


            return "Success";
        }

        public async Task<string> EditVendorOrder(AppUser user, string custOrderId, int orderId, int qty)
        {
            if (user.UserRole != "ADMIN" || user.LockoutEnabled)
            {
                return "UnAuthorized!";
            }
            else
            {
                if (qty <= 0)
                {
                    return "Fail: InValid Qty.";
                }
                var thisVendorOrder = await _orderItemRepository._dbSet.Where(x => x.CustOrderId == custOrderId && x.Id == orderId).Include(x => x.Product).FirstOrDefaultAsync();
                thisVendorOrder.UserOrders = await _userOrdersRepository._dbSet.Where(x => x.CustOrderId == custOrderId).FirstOrDefaultAsync();
                if(thisVendorOrder == null || thisVendorOrder.UserOrders == null)
                {
                    return "Fail: Something Went Wrong";
                }
                if(thisVendorOrder.UserOrders.IsPaid == false && thisVendorOrder.UserOrders.OrderStatus.ToLower().Contains("place"))
                {
                    var userorder = thisVendorOrder.UserOrders;
                    double oldPrice = thisVendorOrder.Price * thisVendorOrder.Quantity;
                    thisVendorOrder.Price = thisVendorOrder.Product.VendorPrice;
                    thisVendorOrder.Quantity = qty;
                    double newPrice = thisVendorOrder.Price * thisVendorOrder.Quantity;
                    userorder.SubTotal += (newPrice - oldPrice);
                    userorder.TotalAmount = userorder.SubTotal + (userorder.SubTotal * userorder.Tax / 100);

                    _userOrdersRepository._dbSet.Update(userorder);
                    await _userOrdersRepository._context.SaveChangesAsync();

                    _orderItemRepository._dbSet.Update(thisVendorOrder);
                    await _orderItemRepository._context.SaveChangesAsync();

                    return "Success";
                }
                else
                {
                    return "Err: Cannot change Qty of the Order";
                }
            }
        }

        public AdminUserOrderViewModel GetAdminUserOrderViewModel(AppUser user, string orderstatus, string paymentstatus, string searchuser, int sortbyorder, 
                        int sortbyid, string search, int pageFrom, int pageTotal, DateTime datefrom, DateTime dateto)
        {
            if (user.UserRole != "ADMIN" || user.LockoutEnabled)
            {
                return model;
            }
            else
            {
                model.userOrdersList = GetFilteredUserOrderList(user, orderstatus, searchuser, paymentstatus, sortbyorder, sortbyid, search, pageFrom, pageTotal, datefrom, dateto);
                model.FilteredOrders = UserOrderCount;
                model.TotalOrders = _userOrdersRepository._dbSet.Count();
                model.VendorOrder = _userOrdersRepository._dbSet.Where(x => x.IsVendor).Count();
                model.UserOrder = _userOrdersRepository._dbSet.Where(x => x.IsVendor == false).Count();
                model.CancelledOrders = _userOrdersRepository._dbSet.Where(x => x.OrderStatus.ToLower().Contains("cancelled")).Count();
                model.CurrentPage = pageFrom;
                model.NewOrders = _userOrdersRepository._dbSet.Where(x => x.OrderStatus.ToLower().Contains("place")).Count();
                model.ItemsPerPage = pageTotal;
                model.TotalPages = (int)Math.Ceiling((double)((double)model.FilteredOrders / (double)pageTotal));
                model.search = search;
                model.DateFrom = datefrom;
                model.DateTo = dateto;
                model.OrderStatus = orderstatus;
                model.SearchByUser = searchuser;
                model.SortByID = 0;
                model.SortByOrder = sortbyorder;

                return model;
            }
        }

        public List<UserOrders> GetFilteredUserOrderList(AppUser user, string orderstatus, string searchuser, string paymentstatus, int sortbyorder, int sortbyid, string search, int pageFrom, int pageTotal, DateTime datefrom, DateTime dateto)
        {
            var orderlist = new List<UserOrders>();
            pageFrom--;
            if (user.UserRole != "ADMIN" || user.LockoutEnabled)
            {
                return orderlist;
            }
            else
            {   
                //paymentStatus: Paid ---> userOrder.isPaid, UnPaid -->> UserOrder.IsPaid == false, Refunded ---> isPaid and OrderStatus.Cancelled, Paid in Card ---> paymentId, InStore ---> isPaid  and PaymentId null 


                if (sortbyorder == 1)
                {
                    
                    orderlist = _userOrdersRepository._dbSet.Where(x => (x.CustOrderId.Contains(search) ||
                       x.User.Fullname.Contains(search) || (x.User.Email.Contains(search.Trim())))
                       && (searchuser.ToLower().Equals("") ? true : searchuser.ToLower().Contains("vendor") ? x.IsVendor == true : x.IsVendor == false) 
                       &&(paymentstatus.Equals("") ? true : 
                            paymentstatus.Equals("paid") ? (x.IsPaid && !x.OrderStatus.ToLower().Contains("cancel")) : 
                            paymentstatus.Equals("unpaid") ? (x.IsPaid == false && !x.OrderStatus.ToLower().Contains("cancel")) : 
                            paymentstatus.Equals("refunded") ? (x.IsPaid && x.OrderStatus.ToLower().Contains("cancel")) : 
                            paymentstatus.Contains("card payment") ? (x.PaymentId != "" && !x.OrderStatus.ToLower().Contains("cancel")) : 
                            paymentstatus.Contains("in-store") ? (x.IsPaid && x.PaymentId == "" && !x.OrderStatus.ToLower().Contains("cancel")) : false)
                       && x.OrderStatus.ToLower().Contains(orderstatus.ToLower().Trim())
                       && x.CreateDate >= datefrom && x.CreateDate <= dateto).Include(x => x.User).OrderByDescending(x => x.CreateDate).ToList();
                }
                else
                {
                    orderlist = _userOrdersRepository._dbSet.Where(x => (x.CustOrderId.Contains(search) ||
                       x.User.Fullname.Contains(search) || (x.User.Email.Contains(search.Trim())))
                       && (searchuser.ToLower().Equals("") ? true : searchuser.ToLower().Contains("vendor") ? x.IsVendor == true : x.IsVendor == false)
                        && (paymentstatus.Equals("") ? true :
                            paymentstatus.Equals("paid") ? (x.IsPaid && !x.OrderStatus.ToLower().Contains("cancel")) :
                            paymentstatus.Equals("unpaid") ? (x.IsPaid == false && !x.OrderStatus.ToLower().Contains("cancel")) :
                            paymentstatus.Equals("refunded") ? (x.IsPaid && x.OrderStatus.ToLower().Contains("cancel")) :
                            paymentstatus.Contains("card payment") ? (x.PaymentId != "" && !x.OrderStatus.ToLower().Contains("cancel")) :
                            paymentstatus.Contains("in-store") ? (x.IsPaid && x.PaymentId == "" && !x.OrderStatus.ToLower().Contains("cancel")) : false)
                       && x.OrderStatus.ToLower().Contains(orderstatus.ToLower().Trim())
                       && x.CreateDate >= datefrom && x.CreateDate <= dateto).Include(x => x.User).ToList();
                }

                UserOrderCount = orderlist.Count();
                model.UnPaidAmount = orderlist.Where(x => !x.IsPaid && !x.OrderStatus.ToLower().Contains("cancel")).Select(x => x.TotalAmount).Sum();
                model.UnPaidOrders = orderlist.Where(x => !x.IsPaid && !x.OrderStatus.ToLower().Contains("cancel")).Count();

                return orderlist.Skip(pageTotal * pageFrom).Take(pageTotal).ToList();
            }
        }

        public async Task<AdminUserViewModel> GetAdminUserAccountAsync(AppUser user, int stateID = 0, string search = "", string UserRole = "", int pageFrom = 1, int pageTotal = 20)
        {
            var usermodel = new AdminUserViewModel();
            if (user.UserRole != "ADMIN" || user.LockoutEnabled)
            {
                return usermodel;
            }
            else
            {
                usermodel.UserList = GetFilteredUserList(user, stateID, search, UserRole, pageFrom, pageTotal);
                usermodel.StateList = await GetStateListAsync(user);
                usermodel.TotalUsers = UserCount;
                usermodel.ActiveUsers = _userManager.Users.Where(x => x.LockoutEnabled == false).Count();
                usermodel.BlockedUsers = _userManager.Users.Where(x => x.LockoutEnabled == true).Count();
                usermodel.AdminUsers = _userManager.Users.Where(x => x.UserRole == "ADMIN").Count();
                usermodel.VendorUsers = _userManager.Users.Where(x => x.UserRole == "VENDOR").Count();
                usermodel.TotalPages = (int)Math.Ceiling((double)((double)usermodel.TotalUsers / (double)pageTotal));
                return usermodel;
            }

            throw new NotImplementedException();
        }

        public async Task<List<State>> GetStateListAsync(AppUser user)
        {
            var statelist = new List<State>();
            if (user.UserRole != "ADMIN" || user.LockoutEnabled)
            {
                return statelist;
            }
            else
            {
                statelist = await _stateRepository._dbSet.Where(x => x.StateID > 0).ToListAsync();
                return statelist;
            }
        }

        public async Task<List<SubCategory>> GetSubCategoryListAsync(int CatID = 0)
        {
            var subcategorylist = new List<SubCategory>();
            
                subcategorylist = await _subcategoryRepository._dbSet.Where(x => x.SubCategoryID > 0 && x.Category.CategoryID == CatID).Include(x => x.Category).ToListAsync();
                return subcategorylist;
            
        }

        public async Task<List<Product>> UploadProductSheetAsync(IFormFile file)
        {
            var newlist = new List<Product>();
            var oldList = new List<Product>();
            var CategoryList = _categoryRepository._dbSet.ToList();
            var SubCategoryList = _subcategoryRepository._dbSet.ToList();
            List<string> barcodeList = _productRepository._dbSet.Select(x => x.Barcode).ToList();
            List<int> ProdId = _productRepository._dbSet.Select(x => x.ProductID).ToList();
            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                using (var package = new ExcelPackage(stream))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    var rowcount = worksheet.Dimension.Rows;
                    for (int row = 2; row <= rowcount; row++)
                    {
                        var newProduct = new Product();
                        try
                        {
                            var barcode = worksheet.Cells[row, 1].Value.ToString().Trim();
                            if(barcode == null || barcode.Length <= 3)
                            {
                                continue;
                            }
                            newProduct.Barcode = barcode;

                            if (barcodeList.Where(x => x.Equals(barcode)).Any())
                            {
                                var index = barcodeList.IndexOf(barcode);
                                newProduct.ProductID = ProdId[index];
                            }

                            if(newProduct.ProductID == 0)
                            {
                                var productName = worksheet.Cells[row, 2].Value.ToString().Trim();
                                if (productName == null || productName.Length <= 3)
                                {
                                    continue;
                                }
                                newProduct.ProductName = productName;
                                var category = worksheet.Cells[row, 3].Value.ToString().Trim();
                                if (category == null || category.Length <= 2)
                                {
                                    continue;
                                }
                                var cat = CategoryList.Where(x => x.CategoryName.ToLower().Equals(category.ToLower())).FirstOrDefault();
                                if (cat == null)
                                {
                                    var newCat = new Category();
                                    newCat.CategoryName = category;
                                    _categoryRepository._dbSet.Add(newCat);
                                    _categoryRepository._context.SaveChanges();
                                    cat = _categoryRepository._dbSet.Where(x => x.CategoryName.Equals(newCat.CategoryName)).FirstOrDefault();
                                    CategoryList.Add(cat);
                                    newProduct.CategoryID = cat.CategoryID;
                                }
                                else
                                {
                                    newProduct.CategoryID = cat.CategoryID;
                                }
                                var subcategory = worksheet.Cells[row, 4].Value.ToString().Trim();
                                if (subcategory == null || subcategory.Length <= 2)
                                {
                                    continue;
                                }
                                var subcat = SubCategoryList.Where(x => x.SubCategoryName.ToLower().Equals(subcategory.ToLower())).FirstOrDefault();
                                if (subcat == null)
                                {
                                    var newSubCat = new SubCategory();
                                    newSubCat.SubCategoryName = subcategory;
                                    newSubCat.CategoryID = cat.CategoryID;
                                    _subcategoryRepository._dbSet.Add(newSubCat);
                                    _subcategoryRepository._context.SaveChanges();
                                    subcat = _subcategoryRepository._dbSet.Where(x => x.SubCategoryName.Equals(subcategory)).FirstOrDefault();
                                    SubCategoryList.Add(subcat);
                                    newProduct.SubCategoryID = subcat.SubCategoryID;
                                }
                                else
                                {
                                    newProduct.SubCategoryID = subcat.SubCategoryID;
                                }
                            }
                           
                            var stock = worksheet.Cells[row, 6].Value.ToString().Trim();
                            if(stock == null || stock.Length < 1 || stock.Contains('-') || stock.Contains(".")){
                                continue;
                            }
                            else
                            {
                                newProduct.Stock = int.Parse(stock);
                            }
                            var baseprice = worksheet.Cells[row, 8].Value.ToString().Trim();
                            if(baseprice == null || baseprice.Length < 1)
                            {
                                continue;
                            }
                            else
                            {
                                newProduct.BasePrice = double.Parse(baseprice);
                            }
                            var saleprice = worksheet.Cells[row, 9].Value.ToString().Trim();
                            if (saleprice == null || saleprice.Length < 1)
                            {
                                continue;
                            }
                            else
                            {
                                newProduct.SalePrice = double.Parse(saleprice);
                            }
                            var desc = worksheet.Cells[row, 10].Value.ToString().Trim();
                            newProduct.Description = desc;
                            newProduct.IsFeatured = false;
                            newProduct.IsPromoted = false;
                            newProduct.IsRemoved = false;
                            if(newProduct.ProductID != 0)
                            {
                                oldList.Add(newProduct);
                            }
                            else
                            {
                                newlist.Add(newProduct);
                            }
                        }
                        catch(Exception ex)
                        {
                            var m = ex.ToString();
                        }

                    }
                    _productRepository._dbSet.AddRange(newlist);
                    _productRepository._dbSet.UpdateRange(oldList);
                    _productRepository._context.SaveChanges();
                }
            }
            return newlist;
        }

        public async Task<string> ChangeProductPromotion(AppUser user, string barcode)
        {
            if (user.UserRole != "ADMIN" || user.LockoutEnabled)
            {
                return "UnAuthorized";
            }
            else
            {
                var Promotionproduct = _productRepository._dbSet.Where(x => x.Barcode == barcode).FirstOrDefault();

                if (Promotionproduct == null)
                {
                    return "Product dosen't Exists";
                }

                var curr = Promotionproduct.IsPromoted;
                Promotionproduct.IsPromoted = !Promotionproduct.IsPromoted;
                _productRepository._dbSet.Update(Promotionproduct);
                await _productRepository._context.SaveChangesAsync();
                if (curr != _productRepository._dbSet.Where(x => x.Barcode == barcode).FirstOrDefault().IsPromoted)
                {
                    return "Success";
                }

                return "Something went Wrong!!";
            }
        }
        public async Task<string> ChangeProductFeature(AppUser user ,string barcode)
        {
            if (user.UserRole != "ADMIN" || user.LockoutEnabled)
            {
                return "UnAuthorized";
            }
            else
            {
                var featuredproduct = _productRepository._dbSet.Where(x => x.Barcode == barcode).FirstOrDefault();

                if(featuredproduct == null)
                {
                    return "Product dosen't Exists";
                }

                var currIsFeatured = featuredproduct.IsFeatured;
                featuredproduct.IsFeatured = !featuredproduct.IsFeatured;
                _productRepository._dbSet.Update(featuredproduct);
                await _productRepository._context.SaveChangesAsync();
                if(currIsFeatured != _productRepository._dbSet.Where(x => x.Barcode == barcode).FirstOrDefault().IsFeatured)
                {
                    return "Success";
                }

                return "Something went Wrong!!";
            }
        }

        public async Task<List<Category>> GetCategoryListAsync(AppUser user)
        {
            var categorylist = new List<Category>();
            if (user.UserRole != "ADMIN" || user.LockoutEnabled)
            {
                return categorylist;
            }
            else
            {
                categorylist = await _categoryRepository._dbSet.Where(x => x.CategoryID > 0).ToListAsync();
                return categorylist;
            }
        }

        public List<AppUser> GetFilteredUserList(AppUser user, int stateID = 0, string search ="", string UserRole = "", int pageFrom = 1, int pageTotal = 20)
        {
            var userlist = new List<AppUser>();
            pageFrom--;
            if (user.UserRole != "ADMIN" || user.LockoutEnabled)
            {
                return userlist;
            }
            else
            {
                if(search == null || search.Length < 1)
                {
                    search = "";
                }
                if (UserRole == null || UserRole.Length < 1 || UserRole.Equals("Select-Role"))
                {
                    UserRole = "";
                }
                if (stateID > 0)
                {
                    userlist = _userManager.Users.Where(x => x.StateID == stateID && x.UserRole.Contains(UserRole) && (x.Fullname.Contains(search) || x.Email.Contains(search))).Skip(pageTotal * pageFrom).Take(pageTotal).ToList();
                    UserCount = _userManager.Users.Where(x => x.StateID == stateID && x.UserRole.Contains(UserRole) && (x.Fullname.Contains(search) || x.Email.Contains(search))).Count();
                }
                else
                {
                    userlist = _userManager.Users.Where(x => x.UserRole.Contains(UserRole) && (x.Fullname.Contains(search) || x.Email.Contains(search)))
                        .Skip(pageTotal * pageFrom).Take(pageTotal).ToList();
                    UserCount = _userManager.Users.Where(x => x.UserRole.Contains(UserRole) && (x.Fullname.Contains(search) || x.Email.Contains(search))).Count();

                }
               
                return userlist;
            }

        }

        public async Task<List<Product>> GetFilteredProductList(AppUser user, int CategoryId = 0, int SubCategoryId = 0, int Min = 0,
                                                 int Max = 10000, string search = "", int sortBy = 0,
                                                 int sortByOrder = 1, int pageFrom = 1, int pageTotal = 20)
        {
            var productlist = new List<Product>();
            pageFrom--;
            if (user.UserRole != "ADMIN" || user.LockoutEnabled)
            {
                return productlist;
            }
            else
            {
                //SetUp
                if(Min >= Max)
                {
                    Min = 0;
                    Max = 100000;
                }

                //General DB Call
                if (search != null && search != "" && search.Length >= 3)
                {
                    productlist = await _productRepository._dbSet.Where(x => (x.ProductName.Contains(search) || x.Description.Contains(search) || x.Barcode.Contains(search)) && x.IsRemoved == false).Skip(pageTotal * pageFrom).Take(pageTotal).Include(x => x.Category).Include(x => x.SubCategory).ToListAsync();
                    ProductCount = _productRepository._dbSet.Where(x => (x.ProductName.Contains(search) || x.Description.Contains(search) || x.Barcode.Contains(search)) && x.IsRemoved == false).Count();
                }
                else
                {
                    if (CategoryId > 0 && SubCategoryId > 0)
                    {
                        if (sortBy == 0)
                        {
                            productlist = await _productRepository._dbSet.Where(x => (x.SubCategoryID == SubCategoryId && x.SalePrice >= Min && x.SalePrice <= Max) && x.IsRemoved == false).Skip(pageTotal * pageFrom).Take(pageTotal).Include(x => x.Category).Include(x => x.SubCategory).ToListAsync();

                        }
                        else if (sortBy == 1)
                        {
                            if (sortByOrder == 1)
                            {
                                productlist = await _productRepository._dbSet.Where(x => (x.SubCategoryID == SubCategoryId && x.SalePrice >= Min && x.SalePrice <= Max) && x.IsRemoved == false).OrderBy(x => x.Stock).Skip(pageTotal * pageFrom).Take(pageTotal).Include(x => x.Category).Include(x => x.SubCategory).ToListAsync();
                            }
                            else
                            {
                                productlist = await _productRepository._dbSet.Where(x => (x.SubCategoryID == SubCategoryId && x.SalePrice >= Min && x.SalePrice <= Max) && x.IsRemoved == false).OrderByDescending(x => x.Stock).Skip(pageTotal * pageFrom).Take(pageTotal).Include(x => x.Category).Include(x => x.SubCategory).ToListAsync();
                            }
                        }
                        else if (sortBy == 2)
                        {
                            if (sortByOrder  == 1)
                            {
                                productlist = await _productRepository._dbSet.Where(x => (x.SubCategoryID == SubCategoryId && x.SalePrice >= Min && x.SalePrice <= Max) && x.IsRemoved == false).OrderBy(x => x.BasePrice).Skip(pageTotal * pageFrom).Take(pageTotal).Include(x => x.Category).Include(x => x.SubCategory).ToListAsync();
                            }
                            else
                            {
                                productlist = await _productRepository._dbSet.Where(x => (x.SubCategoryID == SubCategoryId && x.SalePrice >= Min && x.SalePrice <= Max) && x.IsRemoved == false).OrderByDescending(x => x.BasePrice).Skip(pageTotal * pageFrom).Take(pageTotal).Include(x => x.Category).Include(x => x.SubCategory).ToListAsync();
                            }
                        }
                        else
                        {
                            if (sortByOrder == 1)
                            {
                                productlist = await _productRepository._dbSet.Where(x => (x.SubCategoryID == SubCategoryId && x.SalePrice >= Min && x.SalePrice <= Max) && x.IsRemoved == false).OrderBy(x => x.SalePrice).Skip(pageTotal * pageFrom).Take(pageTotal).Include(x => x.Category).Include(x => x.SubCategory).ToListAsync();
                            }
                            else
                            {
                                productlist = await _productRepository._dbSet.Where(x => (x.SubCategoryID == SubCategoryId && x.SalePrice >= Min && x.SalePrice <= Max) && x.IsRemoved == false).OrderByDescending(x => x.SalePrice).Skip(pageTotal * pageFrom).Take(pageTotal).Include(x => x.Category).Include(x => x.SubCategory).ToListAsync();
                            }
                        }
                        ProductCount = _productRepository._dbSet.Where(x => (x.SubCategoryID == SubCategoryId && x.SalePrice >= Min && x.SalePrice <= Max) && x.IsRemoved == false).Count();
                    }
                    else if (CategoryId > 0)
                    {
                        if (sortBy == 0)
                        {
                            productlist = await _productRepository._dbSet.Where(x => (x.CategoryID == CategoryId && x.SalePrice >= Min && x.SalePrice <= Max) && x.IsRemoved == false).Skip(pageTotal * pageFrom).Take(pageTotal).Include(x => x.Category).Include(x => x.SubCategory).ToListAsync();
                        }
                        else if (sortBy == 1)
                        {
                            if (sortByOrder == 1)
                            {
                                productlist = await _productRepository._dbSet.Where(x => (x.CategoryID == CategoryId && x.SalePrice >= Min && x.SalePrice <= Max) && x.IsRemoved == false).OrderBy(x => x.Stock).Skip(pageTotal * pageFrom).Take(pageTotal).Include(x => x.Category).Include(x => x.SubCategory).ToListAsync();
                            }
                            else
                            {
                                productlist = await _productRepository._dbSet.Where(x => (x.CategoryID == CategoryId && x.SalePrice >= Min && x.SalePrice <= Max) && x.IsRemoved == false).OrderByDescending(x => x.Stock).Skip(pageTotal * pageFrom).Take(pageTotal).Include(x => x.Category).Include(x => x.SubCategory).ToListAsync();
                            }
                        }
                        else if (sortBy == 2)
                        {
                            if (sortByOrder == 1)
                            {
                                productlist = await _productRepository._dbSet.Where(x => (x.CategoryID == CategoryId && x.SalePrice >= Min && x.SalePrice <= Max) && x.IsRemoved == false).OrderBy(x => x.BasePrice).Skip(pageTotal * pageFrom).Take(pageTotal).Include(x => x.Category).Include(x => x.SubCategory).ToListAsync();
                            }
                            else
                            {
                                productlist = await _productRepository._dbSet.Where(x => (x.CategoryID == CategoryId && x.SalePrice >= Min && x.SalePrice <= Max) && x.IsRemoved == false).OrderByDescending(x => x.BasePrice).Skip(pageTotal * pageFrom).Take(pageTotal).Include(x => x.Category).Include(x => x.SubCategory).ToListAsync();
                            }
                        }
                        else
                        {
                            if (sortByOrder == 1)
                            {
                                productlist = await _productRepository._dbSet.Where(x => (x.CategoryID == CategoryId && x.SalePrice >= Min && x.SalePrice <= Max) && x.IsRemoved == false).OrderBy(x => x.SalePrice).Skip(pageTotal * pageFrom).Take(pageTotal).Include(x => x.Category).Include(x => x.SubCategory).ToListAsync();
                            }
                            else
                            {
                                productlist = await _productRepository._dbSet.Where(x => (x.CategoryID == CategoryId && x.SalePrice >= Min && x.SalePrice <= Max) && x.IsRemoved == false).OrderByDescending(x => x.SalePrice).Skip(pageTotal * pageFrom).Take(pageTotal).Include(x => x.Category).Include(x => x.SubCategory).ToListAsync();
                            }
                        }
                        ProductCount = _productRepository._dbSet.Where(x => (x.CategoryID == CategoryId && x.SalePrice >= Min && x.SalePrice <= Max) && x.IsRemoved == false).Count();
                    }
                    else
                    {
                        ProductCount = _productRepository._dbSet.Where(x => (x.SalePrice >= Min && x.SalePrice <= Max) && x.IsRemoved == false).Count();
                        if (sortBy == 0)
                        {
                            productlist = await _productRepository._dbSet.Where(x => (x.SalePrice >= Min && x.SalePrice <= Max) && x.IsRemoved == false).Skip(pageTotal * pageFrom).Take(pageTotal).Include(x => x.Category).Include(x => x.SubCategory).ToListAsync();
                        }
                        else if (sortBy == 1)
                        {
                            if (sortByOrder == 1)
                            {
                                productlist = await _productRepository._dbSet.Where(x => (x.SalePrice >= Min && x.SalePrice <= Max) && x.IsRemoved == false).OrderBy(x => x.Stock).Skip(pageTotal * pageFrom).Take(pageTotal).Include(x => x.Category).Include(x => x.SubCategory).ToListAsync();
                            }
                            else
                            {
                                productlist = await _productRepository._dbSet.Where(x => (x.SalePrice >= Min && x.SalePrice <= Max) && x.IsRemoved == false).OrderByDescending(x => x.Stock).Skip(pageTotal * pageFrom).Take(pageTotal).Include(x => x.Category).Include(x => x.SubCategory).ToListAsync();
                            }
                        }
                        else if (sortBy == 2)
                        {
                            if (sortByOrder == 1)
                            {
                                productlist = await _productRepository._dbSet.Where(x => (x.SalePrice >= Min && x.SalePrice <= Max) && x.IsRemoved == false).OrderBy(x => x.BasePrice).Skip(pageTotal * pageFrom).Take(pageTotal).Include(x => x.Category).Include(x => x.SubCategory).ToListAsync();
                            }
                            else
                            {
                                productlist = await _productRepository._dbSet.Where(x => (x.SalePrice >= Min && x.SalePrice <= Max) && x.IsRemoved == false).OrderByDescending(x => x.BasePrice).Skip(pageTotal * pageFrom).Take(pageTotal).Include(x => x.Category).Include(x => x.SubCategory).ToListAsync();
                            }
                        }
                        else
                        {
                            if (sortByOrder == 1)
                            {
                                productlist = await _productRepository._dbSet.Where(x => (x.SalePrice >= Min && x.SalePrice <= Max) && x.IsRemoved == false).OrderBy(x => x.SalePrice).Skip(pageTotal * pageFrom).Take(pageTotal).Include(x => x.Category).Include(x => x.SubCategory).ToListAsync();
                            }
                            else
                            {
                                productlist = await _productRepository._dbSet.Where(x => (x.SalePrice >= Min && x.SalePrice <= Max) && x.IsRemoved == false).OrderByDescending(x => x.SalePrice).Skip(pageTotal * pageFrom).Take(pageTotal).Include(x => x.Category).Include(x => x.SubCategory).ToListAsync();
                            }
                        }
                    }
                }
                return productlist;
            }
        }

        public async Task<string> deleteProduct(AppUser user, int productID)
        {
            if(user.UserRole != "ADMIN" || user.LockoutEnabled)
            {
                return "UnAuthorized";
            }

            var prd = _productRepository._dbSet.Where(x => x.ProductID == productID).FirstOrDefault();
            if (prd != null && prd.ProductID > 0)
            {
                try
                {
                    prd.IsRemoved = true;
                    _productRepository._dbSet.Update(prd);
                    await _productRepository._context.SaveChangesAsync();

                    return "Success";
                }
                catch (Exception)
                {

                    return "Something went Wrong";
                }
            }
            else
            {
                return "Product doesn't Exist";
            }

        }

        public async Task<string> addProductAsync(AppUser user, Product newProduct)
        {
            if (user.UserRole != "ADMIN" || user.LockoutEnabled)
            {
                return "UnAuthorized";
            }
            else
            {
                try
                {
                    var existingProd = _productRepository._dbSet.Where(x => x.ProductID == newProduct.ProductID || x.Barcode == newProduct.Barcode || x.ProductName == newProduct.ProductName).FirstOrDefault();
                    if (existingProd != null){

                        if (existingProd.IsRemoved)
                        {
                            existingProd.IsRemoved = false;
                            existingProd.SalePrice = newProduct.SalePrice;
                            existingProd.BasePrice = newProduct.BasePrice;
                            existingProd.VendorPrice = newProduct.VendorPrice;
                            existingProd.Description = newProduct.Description;

                            _productRepository._dbSet.Update(existingProd);
                            _productRepository._context.SaveChanges();

                            return "Success";
                        }
                        else
                        {
                            return "This Product Already Exists: ProductID: " + newProduct.ProductID + ", BarCode: " + newProduct.Barcode + ", ProductName: " + newProduct.ProductName;
                        }
                    }
                       
                    else
                    {
                        string err = "Fail:";
                        bool error = false;
                        if (newProduct.Barcode == null || newProduct.Barcode.Length <= 3)
                        {
                            error = true;
                            err.Replace("Unknown", " ");
                            err += " Invalid Barcode";
                        }
                        if(newProduct.VendorPrice <= 0 || (newProduct.VendorPrice < newProduct.BasePrice || newProduct.VendorPrice > newProduct.SalePrice) )
                        {
                            error = true;
                            err.Replace("Unknown", "");
                            err += " Invalid Vendor Price";
                        }
                        if (newProduct.ProductName == null || newProduct.ProductName.Length <= 1)
                        {
                            error = true;
                            err.Replace("Unknown", "");
                            err += " Invalid Product Name";
                        }
                        if (newProduct.BasePrice <= 0 || newProduct.SalePrice <= newProduct.BasePrice)
                        {
                            error = true;
                            err.Replace("Unknown", "");
                            err += " Invalid Prices";
                        }
                        if (error)
                        {
                            return err;
                        }
                        newProduct.IsFeatured = false;
                        newProduct.IsPromoted = false;
                        newProduct.PrimaryImage = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTl20Tuf0ZhpbNa24X1Kaw3NgZ1Rhy2bdbkGw&usqp=CAU";
                        newProduct.SecondaryImage1 = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTl20Tuf0ZhpbNa24X1Kaw3NgZ1Rhy2bdbkGw&usqp=CAU";
                        newProduct.SecondaryImage2 = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTl20Tuf0ZhpbNa24X1Kaw3NgZ1Rhy2bdbkGw&usqp=CAU";
                        var currentCount = _productRepository._dbSet.Count();
                        var prod = _productRepository._dbSet.Add(newProduct);
                        await _productRepository._context.SaveChangesAsync();
                        if(currentCount == _productRepository._dbSet.Count() - 1)
                        {
                            return "Success" + prod.Entity.ProductID;
                        }

                        return "Something went wrong!";
                    }
                }
                catch(Exception ex)
                {
                    return ex.ToString();
                }
            }
        }

        public async Task<string> editProductAsync(AppUser user, Product editproduct)
        {
            if (user.UserRole != "ADMIN" || user.LockoutEnabled)
            {
                return "UnAuthorized";
            }
            else
            {
                try
                {
                    var thisProduct = await _productRepository._dbSet.Where(x => x.ProductID == editproduct.ProductID)
                                                                    .FirstOrDefaultAsync();

                    if (thisProduct != null && thisProduct.ProductID == editproduct.ProductID)
                    {
                        if (editproduct.Stock >= 0 && editproduct.Stock < 1000000)
                        {
                            thisProduct.Stock = editproduct.Stock;
                        }
                        else
                        {
                            return "Please enter appropriate Stock Price";
                        }
                        if (editproduct.BasePrice > 0 && editproduct.SalePrice > editproduct.BasePrice)
                        {
                            thisProduct.SalePrice = editproduct.SalePrice;
                            thisProduct.BasePrice = editproduct.BasePrice;
                        }
                        else
                        {
                            return "Base Price must be lesser than Sale Price and greater than 0";
                        }
                        thisProduct.Description = editproduct.Description;

                        _productRepository._dbSet.Update(thisProduct);
                        var result = await _productRepository._context.SaveChangesAsync();

                        return "Success";
                    }
                    else
                    {
                        return "The Product Doesn't Exists, Please Add The Product First.";
                    }
                    
                    
                }
                catch (Exception ex)
                {
                    return ex.ToString();
                }

            }
        }

        public async Task<string> addCategoryAsync(AppUser user, Category newcategory)
        {
            if (user.UserRole != "ADMIN" || user.LockoutEnabled)
            {
                return "UnAuthorized";
            }
            else
            {
                try
                {
                    await _categoryRepository._dbSet.AddAsync(newcategory);
                    var result = await _categoryRepository._context.SaveChangesAsync();
                    if (result != 1)
                    {
                        return "Fail";
                    }
                    return "Success";
                }
                catch (Exception ex)
                {
                    return ex.ToString();
                }

            }
        }

        public async Task<string> addSubCategoryAsync(AppUser user, SubCategory newsubcategory)
        {
            if (user.UserRole != "ADMIN" || user.LockoutEnabled)
            {
                return "UnAuthorized";
            }
            else
            {
                try
                {
                    await _subcategoryRepository._dbSet.AddAsync(newsubcategory);
                    var result = await _subcategoryRepository._context.SaveChangesAsync();
                    if (result != 1)
                    {
                        return "Fail";
                    }
                    return "Success";
                }
                catch (Exception ex)
                {
                    return ex.ToString();
                }

            }
        }

        public async Task<string> editUserAsync(AppUser user, EditUserViewModel editUser)
        {
            if (user.UserRole != "ADMIN" || user.LockoutEnabled)
            {
                return "UnAuthorized";
            }
            else
            {
                try
                {
                    var thisuser = await _userManager.Users.Where(x => x.Email == editUser.UserEmail)
                                                                    .FirstOrDefaultAsync();
                    //Admin cannot change his own user data
                    if (user.Email != editUser.UserEmail && thisuser!= null)
                    {
                        thisuser.UserRole = editUser.UserRole;

                        thisuser.LockoutEnabled = editUser.IsActive;
                        thisuser.IsAccounting = editUser.IsAccounting;
                        //TODO: Toggle isActive Set in user like above.

                        var metaData = new Dictionary<string, string>();
                        metaData.Add("UserRole", editUser.UserRole);

                        var options = new Stripe.CustomerUpdateOptions
                        {
                            Metadata = metaData
                        };
                        var service = new Stripe.CustomerService();
                        service.Update(thisuser.CustomerId, options);


                        await _userManager.UpdateAsync(thisuser);

                        return "Success";
                    }
                    else
                    {
                        return "You cannot change your own Info!!";
                    }
                }
                catch (Exception ex)
                {
                    return ex.ToString();
                }
            }
        }
    }
}