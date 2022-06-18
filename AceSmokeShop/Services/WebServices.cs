using AceSmokeShop.Areas.Identity.Data;
using AceSmokeShop.Core.Repositories;
using AceSmokeShop.Models;
using AceSmokeShop.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AceSmokeShop.Services
{
    public class WebServices
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

        public WebServices(ProductRepository productRepository,
            CategoryRepository categoryRepository, SubCategoryRepository subcategoryRepository,
            StateRepository stateRepository, UserManager<AppUser> userManager, CartRepository cartRepository, AddressRepository addressRepository,
            PaymentServices paymentServices, UserOrdersRepository userOrdersRepository, OrderItemRepository orderItemRepository, TransactionRepository transactionRepository)
        {
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

        public string GenerateOrderID()
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var stringChars = new char[11];
            var random = new Random();
            var exists = true;
            while (exists)
            {
                for (int i = 0; i < stringChars.Length; i++)
                {
                    stringChars[i] = chars[random.Next(chars.Length)];
                    if(i == 3 || i == 7)
                    {
                        stringChars[i] = '-';
                    }
                    else
                    {
                        stringChars[i] = chars[random.Next(chars.Length)];
                    }
                }
                var finalString = new String(stringChars);

                exists = _userOrdersRepository._dbSet.Where(x => x.CustOrderId.Equals(finalString)).Any();
            }  

            return new String(stringChars);
        }

        public UHomeViewModel GetHomeViewModel(AppUser user)
        {
            var model = new UHomeViewModel();

            model.FeaturedList = _productRepository._dbSet.Where(x => x.IsFeatured && !x.IsRemoved && x.Stock > 0).Include(x => x.Category).Include(x => x.SubCategory).Take(3).ToList();
            model.PromotedList = _productRepository._dbSet.Where(x => x.IsPromoted && !x.IsRemoved && x.Stock > 0).Include(x => x.Category).Include(x => x.SubCategory).Take(3).ToList();
            model.CategoryList = _categoryRepository._dbSet.ToList();
            foreach(var item in model.FeaturedList)
            {
                item.BasePrice = 0;
                if(user != null && user.UserRole.ToLower() != "vendor")
                {
                    item.VendorPrice = 0;
                }
            }
            foreach (var item in model.PromotedList)
            {
                item.BasePrice = 0;
                if (user != null && user.UserRole.ToLower() != "vendor")
                {
                    item.VendorPrice = 0;
                }
            }
            return model;
        }

        public UProductViewModel GetUserProductsViewModel(AppUser user, int CategoryID = 0, int SubCategoryID = 0, int pageFrom = 1, int ItemsPerPage = 10,
                                                string search = "", string type = "")
        {
            var model = new UProductViewModel();

            model.CategoryList = _categoryRepository._dbSet.ToList();
            model.CategoryList.Insert(0, new Category { CategoryID = 0, CategoryName = "Select Category" });
            model.SubCategoryList.Insert(0, new SubCategory { SubCategoryID = 0, SubCategoryName = "Select SubCategory" });
            var ProdctList = _productRepository._dbSet.Where(x => x.IsRemoved == false).Include(x => x.Category).Include(x => x.SubCategory).ToList();
            pageFrom--;
            if(CategoryID > 0)
            {
                model.SubCategoryList = _subcategoryRepository._dbSet.Where(x => x.CategoryID == CategoryID).ToList();
                ProdctList = ProdctList.Where(x => x.CategoryID == CategoryID).ToList();
            }
            if (SubCategoryID > 0)
            {
                ProdctList = ProdctList.Where(x => x.SubCategoryID == SubCategoryID).ToList();
                model.SubCategorySelect.SubCategoryID = SubCategoryID;
            }
            if (search != null && search.Length > 0)
            {
                ProdctList = ProdctList.Where(x => x.Category.CategoryName.Contains(search, StringComparison.OrdinalIgnoreCase) || x.SubCategory.SubCategoryName.Contains(search, StringComparison.OrdinalIgnoreCase) || x.ProductName.Contains(search, StringComparison.OrdinalIgnoreCase) || x.Description.Contains(search, StringComparison.OrdinalIgnoreCase) || x.Barcode.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            if (type != null && (type.Contains("featured") || type.Contains("popular")))
            {
                if (type.Contains("featured"))
                {
                    ProdctList = ProdctList.Where(x => x.IsFeatured).ToList();
                    model.IsFeatured = true;
                    model.IsPromoted = false;
                    type = "featured";
                }

                if (type.Contains("popular"))
                {
                    ProdctList = ProdctList.Where(x => x.IsPromoted).ToList();
                    model.IsPromoted = true;
                    model.IsFeatured = false;
                    type = "popular";
                }
            }
            model.ItemsPerPage = ItemsPerPage;
            model.TotalPages = (int)Math.Ceiling((double)((double)ProdctList.Count() / (double)ItemsPerPage));
            model.ProdctList = ProdctList.Skip(ItemsPerPage * pageFrom).Take(ItemsPerPage).ToList();
            foreach(var items in model.ProdctList)
            {
                items.BasePrice = 0;
                if (user != null && user.UserRole.ToLower() != "vendor")
                {
                    items.VendorPrice = 0;
                }
            }
            model.CurrentPage = ++pageFrom;
            model.Type = type;
           
            return model;
        }

        public string PlaceProductOrder(AppUser user, int cardId, string productId, int qty, bool pickatstore)
        {
            var product = _productRepository._dbSet.Where(x => x.Barcode == productId).FirstOrDefault();
            if(product == null || product.IsRemoved)
            {
                return "Fail: Product is not available";
            }
            if(product.Stock < qty)
            {
                return "Only " + product.Stock + " of " + product.ProductName + " remain in stock, Please reduce Quantity";
            }
            double SubTotal = 0.00;
            if (user.UserRole.ToLower().Contains("vendor"))
            {
               SubTotal = product.VendorPrice * qty;
            }
            else
            {
               SubTotal = product.SalePrice * qty;
            }

            var ShippingAddress = _addressRepository._dbSet.Where(x => x.UserId == user.Id && x.IsRemoved == false && x.IsShipping == false).FirstOrDefault();

            if (user.UserRole.ToLower() != "vendor")
            {
                if (cardId < 0)
                {
                    return "Fail: Please Select a Card";
                }
            }

            if (pickatstore)
            {
                var add = _addressRepository._dbSet.Where(x => x.AddressLineA == "Store Address").FirstOrDefault();
                if(add == null)
                {
                    add = new Addresses();
                    add.AddressLineA = "Store Address";
                    add.AddressLineB = "Store Address";
                    add.StateID = 5;
                    add.City = "Blackwood";
                    add.Zipcode = 17057;
                    add.UserId = "adminUser";
                    _addressRepository._dbSet.Add(add);
                    _addressRepository._context.SaveChanges();
                    add = _addressRepository._dbSet.Where(x => x.AddressLineA == "Store Address").FirstOrDefault();
                }
                ShippingAddress = add;
            }
            else if (!pickatstore && ShippingAddress == null)
            {
                return "Fail: Please Select a Shipping Location or Select to Pick At Store";
            }


            double Tax = 6.625;
            var GrandTotal = SubTotal + (SubTotal * Tax / 100);

            var OrderId = GenerateOrderID();

            var Order = new UserOrders();
            Order.CustOrderId = OrderId;
            Order.OrderStatus = "Order Placed";
            Order.UserId = user.Id;
            Order.TotalAmount = GrandTotal;
            Order.SubTotal = SubTotal;
            Order.Tax = Tax;
            Order.CreateDate = DateTime.Today;
            Order.IsVendor = user.UserRole.ToLower().Contains("vendor") ? true : false;
            Order.ShippingAddressId = ShippingAddress.Id;
            var paymentResult = "";

            if (user.UserRole.ToLower() != "vendor" || cardId >= 0)
            {
                paymentResult = _paymentServices.PlaceOrder(user, cardId, GrandTotal, OrderId);
                if (paymentResult.ToLower().Contains("err"))
                {
                    return paymentResult;
                }
                Order.IsPaid = true;
                Order.PaymentId = paymentResult;
                _userOrdersRepository._dbSet.Add(Order);
                _userOrdersRepository._context.SaveChanges();
                Order = _userOrdersRepository._dbSet.Where(x => x.CustOrderId == OrderId).FirstOrDefault();

                if (Order == null)
                {
                    //Refund
                    var refund = _paymentServices.CreateRefund(paymentResult, "duplicate");

                    return "Payment Processed but Something Went Wrong! Refund is in Process";
                }
            }
            else
            {
                Order.IsPaid = false;
                Order.PaymentId = "";
                _userOrdersRepository._dbSet.Add(Order);
                _userOrdersRepository._context.SaveChanges();
                Order = _userOrdersRepository._dbSet.Where(x => x.CustOrderId == OrderId).FirstOrDefault();
                if (Order == null)
                {
                    return "Something Went Wrong! Please Try Again";
                }
            }

            var orderItem = new OrderItem();
            orderItem.OrderId = Order.Id;

            if (user.UserRole.ToLower().Contains("vendor"))
            {
                orderItem.Price = product.VendorPrice;
            }
            else
            {
                orderItem.Price = product.SalePrice;
            }

           
            orderItem.ProductId = product.ProductID;
            orderItem.CustOrderId = OrderId;
            orderItem.Quantity = qty;

            _orderItemRepository._dbSet.Add(orderItem);
            _orderItemRepository._context.SaveChanges();

            if (user.UserRole.ToLower() != "vendor" || cardId >= 0)
            {
                product.Stock = product.Stock - qty;
                _productRepository._dbSet.Update(product);
                _productRepository._context.SaveChanges();
            }

            return "Success";
        }

        public string RemoveFromCart(AppUser user, int cartId)
        {
            var cartItem = _cartRepository._dbSet.Where(x => x.Id == cartId).FirstOrDefault();
            if (cartItem == null)
            {
                return "Fail: Item doesn't Exist";
            }
            if (cartItem.UserId != user.Id)
            {
                return "UnAuthorized";
            }
            try
            {
                cartItem.IsRemoved = true;
                _cartRepository._dbSet.Update(cartItem);
                _cartRepository._context.SaveChanges();
                return "Success";
            }
            catch (Exception)
            {

                return "Fail: Something went Wrong";
            }
        }

        public string PlaceCartOrder(AppUser user, int cardId, bool pickatstore)
        {
            //Validate Everything and Calculate Total
            var cartItems = GetMyCart(user);
            var ShippingAddress = _addressRepository._dbSet.Where(x => x.UserId == user.Id && x.IsRemoved == false && x.IsShipping).FirstOrDefault();
            if (cartItems == null)
            {
                return "Please Add Items to Cart";
            }

            if (user.UserRole.ToLower() != "vendor")
            {
                if (cardId < 0)
                {
                    return "Fail: Please Select a Card";
                }
            }

            if (pickatstore)
            {
                var add = _addressRepository._dbSet.Where(x => x.AddressLineA == "Store Address").FirstOrDefault();
                if (add == null)
                {
                    add = new Addresses();
                    add.AddressLineA = "Store Address";
                    add.AddressLineB = "Store Address";
                    add.StateID = _stateRepository._dbSet.Where(x => x.StateName.Contains("Jersey")).FirstOrDefault().StateID;
                    add.City = "Blackwood";
                    add.Zipcode = 17057;
                    add.UserId = _userManager.Users.Where(x => x.UserRole == "ADMIN").FirstOrDefault().Id;
                    _addressRepository._dbSet.Add(add);
                    _addressRepository._context.SaveChanges();
                    add = _addressRepository._dbSet.Where(x => x.AddressLineA == "Store Address").FirstOrDefault();
                }
                ShippingAddress = add;
            }
            else if(!pickatstore && ShippingAddress == null)
            {
                return "Fail: Please Select a Shipping Location or Select to Pick At Store";
            }

            double SubTotal = 0.0;
            foreach(var item in cartItems)
            {
                if(item.Quantity > item.Product.Stock)
                {
                    return "Only " + item.Product.Stock + " of " + item.Product.ProductName + " remain in stock, Please reduce Quantity";
                }
                if (item.Product.IsRemoved)
                {
                    return item.Product.ProductName + " is Out Of Stock, Please try again";
                }
                if (user.UserRole.ToLower().Contains("vendor"))
                {
                    SubTotal += (item.Product.VendorPrice * item.Quantity);
                }
                else
                {
                    SubTotal += (item.Product.SalePrice * item.Quantity);
                }
                
            }
            var Tax = 6.625;
            var GrandTotal = SubTotal + (SubTotal * Tax / 100);

            var OrderId = GenerateOrderID();

            var paymentResult = "";

            var Order = new UserOrders();
            Order.CustOrderId = OrderId;
            Order.OrderStatus = "Order Placed";
            Order.UserId = user.Id;
            Order.TotalAmount = GrandTotal;
            Order.SubTotal = SubTotal;
            Order.Tax = Tax;
            Order.CreateDate = DateTime.Today;
            Order.ShippingAddressId = ShippingAddress.Id;
            Order.IsVendor = user.UserRole.ToLower().Contains("vendor") ? true : false;

            if (user.UserRole.ToLower() != "vendor" || cardId >= 0)
            {
                paymentResult = _paymentServices.PlaceOrder(user, cardId, GrandTotal, OrderId);
                if (paymentResult.ToLower().Contains("err"))
                {
                    return paymentResult;
                }
                Order.IsPaid = true;
                Order.PaymentId = paymentResult;

                _userOrdersRepository._dbSet.Add(Order);
                _userOrdersRepository._context.SaveChanges();
                Order = _userOrdersRepository._dbSet.Where(x => x.CustOrderId == OrderId).FirstOrDefault();
                if (Order == null)
                {
                    //Refund
                    var refund = _paymentServices.CreateRefund(paymentResult, "duplicate");

                    return "Payment Processed but Something Went Wrong! Refund is in Process";
                }
            }
            else
            {
                Order.IsPaid = false;
                Order.PaymentId = paymentResult;

                _userOrdersRepository._dbSet.Add(Order);
                _userOrdersRepository._context.SaveChanges();

                Order = _userOrdersRepository._dbSet.Where(x => x.CustOrderId == OrderId).FirstOrDefault();
                if (Order == null)
                {
                    return "Something Went Wrong Please Try Again";
                }
            }
            var OrderItems = new List<OrderItem>();
            var productList = new List<Product>();

            foreach (var item in cartItems)
            {
                var orderItem = new OrderItem();
                orderItem.OrderId = Order.Id;
                if (user.UserRole.ToLower().Contains("vendor"))
                {
                    orderItem.Price = item.Product.VendorPrice;
                }
                else
                {
                    orderItem.Price = item.Product.SalePrice;
                }
              
                orderItem.ProductId = item.Product.ProductID;
                orderItem.CustOrderId = OrderId;
                item.IsRemoved = true;
                orderItem.Quantity = item.Quantity;
                OrderItems.Add(orderItem);

                var product = item.Product;
                product.Stock = product.Stock - item.Quantity;
                productList.Add(product);
            }

            _orderItemRepository._dbSet.AddRange(OrderItems);
            _orderItemRepository._context.SaveChanges();

            if (user.UserRole.ToLower() != "vendor" || cardId >= 0)
            {
                _productRepository._dbSet.UpdateRange(productList);
                _productRepository._context.SaveChanges();
            }
          

            //Empty cart after order placing
            _cartRepository._dbSet.UpdateRange(cartItems);
            _cartRepository._context.SaveChanges();

            return "Success";
        }

        public string PayNow(AppUser user, int cardId, string custOrderId)
        {
            var order = _userOrdersRepository._dbSet.Where(x => x.CustOrderId == custOrderId).FirstOrDefault();
            if (order == null || order.UserId != user.Id || order.IsPaid || order.OrderStatus.ToLower().Contains("cancel"))
            {
                return "Error: Something Went Wrong";
            }

            //Stripe Payment
            var paymentDetails = _paymentServices.PlaceOrder(user, cardId, order.TotalAmount, custOrderId);
            if (paymentDetails.ToLower().Contains("err"))
            {
                return paymentDetails;
            }
            //UserOrder Update
            order.IsPaid = true;
            order.PaymentId = paymentDetails;
            _userOrdersRepository._dbSet.Update(order);
            _userOrdersRepository._context.SaveChanges();

            return "Success";
        }

        public PayNowViewModel GetPayNowViewModel(AppUser user, string custOrderId)
        {
            var order = _userOrdersRepository._dbSet.Where(x => x.CustOrderId == custOrderId).FirstOrDefault();
            if(order == null || order.UserId != user.Id || order.IsPaid || order.OrderStatus.ToLower().Contains("cancel"))
            {
                return null;
            }
            //prepare order: fill Shipping & OrderItem(Include:Product=>Category=>Subcategory)

            order.ShippingAddress = _addressRepository._dbSet.Where(x => x.Id == order.ShippingAddressId).FirstOrDefault();
            order.OrderItems = _orderItemRepository._dbSet.Where(x => x.CustOrderId == custOrderId).Include(x => x.Product).Include(x => x.Product.Category).Include(x => x.Product.SubCategory).ToList();

            if(order.IsVendor == true && order.ShippingAddress.AddressLineA.ToLower().Contains("store address"))
            {
                order.ShippingAddress = null;
            }

            var model = new PayNowViewModel();
            model.userOrder = order;
            model.PaymentMethods = _paymentServices.GetMyCards(user.CustomerId);

            return model;
        }

        public string CancelOrder(string orderId)
        {
            var userorder = _userOrdersRepository._dbSet.Where(x => x.CustOrderId.Equals(orderId)).FirstOrDefault();

            var paymentCancelResult = _paymentServices.CreateRefund(userorder.PaymentId, "duplicate");
            if (!paymentCancelResult.ToLower().Contains("fail"))
            {
                try
                {
                    userorder.OrderStatus = "Cancelled";
                    _userOrdersRepository._dbSet.Update(userorder);
                    _userOrdersRepository._context.SaveChanges();
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }

                return "Success";
            }
            
            return "Fail: Something Went Wrong";
        }

        public string CancelOrderValidation(AppUser user, string orderid)
        {
            var userorder = _userOrdersRepository._dbSet.Where(x => x.CustOrderId.Equals(orderid)).FirstOrDefault();

            if(userorder == null)
            {
                return "Fail: Order doesn't Exist";
            }
            if(userorder.UserId != user.Id) 
            {
                return "UnAuthorized";
            }
            if (!userorder.OrderStatus.Equals("Order Placed"))
            {
                return "Fail: Order Cannot be Cancelled at this stage";
            }

            return "Success";
        }

        public OrderViewModel GetOrdersViewModel(AppUser user, string OrderStatus, string ShippingStatus, string PaymentStatus, string Search, DateTime datefrom, DateTime dateto)
        {
            var model = new OrderViewModel();
            var addresses = _addressRepository._dbSet.Where(x => x.UserId == user.Id).ToList();

            if (user != null && user.UserRole == "VENDOR")
            {
                model.userOrdersList = GetFilteredVendorOrderList(user, addresses, OrderStatus, ShippingStatus, PaymentStatus, Search, datefrom, dateto);
                model.UnPaidAmount = model.userOrdersList.Where(x => !x.IsPaid && !x.OrderStatus.ToLower().Contains("cancel")).Select(x => x.TotalAmount).Sum();
                model.UnPaidOrders = model.userOrdersList.Where(x => !x.IsPaid && !x.OrderStatus.ToLower().Contains("cancel")).Count();
                model.search = Search;
                model.OrderStatus = OrderStatus;
                model.PaymentStatus = PaymentStatus;
                model.ShippingStatus = ShippingStatus;
            }
            else
            {
                model.userOrdersList = _userOrdersRepository._dbSet.Where(x => x.UserId == user.Id).OrderByDescending(x => x.Id).ToList();
            }
            model.States = _stateRepository._dbSet.ToList();

            foreach (var items in model.userOrdersList)
            {
                items.ShippingAddress = addresses.Where(x => x.Id == items.ShippingAddressId).FirstOrDefault();
                items.OrderItems = _orderItemRepository._dbSet.Where(x => x.OrderId == items.Id).Include(x => x.Product).Include(x => x.Product.Category).Include(x => x.Product.SubCategory).ToList();
            }

            return model;
        }

        //TODO: Ship Status Filter for Vendor Product Filter
        private List<UserOrders> GetFilteredVendorOrderList(AppUser user, List<Addresses> addresses, string orderStatus, string shippingStatus, string paymentStatus, string search, DateTime datefrom, DateTime dateto)
        {
            List<string> orderItemCustId = new List<string>();
            string str = "";
            if (!search.Equals(""))
            {
                orderItemCustId = _orderItemRepository._dbSet.Include(x => x.UserOrders).Include(x => x.Product).Include(x => x.Product.Category).Include(x => x.Product.SubCategory)
                                    .Where(x => x.UserOrders.UserId == user.Id &&
                                    (x.Product.ProductName.ToLower().Contains(search)
                                    || x.Product.Category.CategoryName.ToLower().Contains(search)
                                    || x.Product.SubCategory.SubCategoryName.ToLower().Contains(search))).Select(x => x.CustOrderId).Distinct().ToList();


                foreach (var item in orderItemCustId)
                {
                    str += item.ToString() + "/";
                }
            }
            string addressstr = "-";
            foreach(var item in addresses)
            {
                addressstr += item.Id + "-";
            }
            
            var list = _userOrdersRepository._dbSet.Where(x => x.UserId == user.Id 
                                                            && (orderStatus.Equals("") ? true : x.OrderStatus.ToLower().Contains(orderStatus))
                                                            && (paymentStatus.Equals("") ? true : paymentStatus.Equals("paid") ? (x.IsPaid && !x.OrderStatus.ToLower().Contains("cancel")) : (!x.IsPaid && !x.OrderStatus.ToLower().Contains("cancel")))
                                                            && (search.Equals("") ? true : (x.CustOrderId.ToLower().Contains(search) || str.Contains(x.CustOrderId)))
                                                            && (shippingStatus.Equals("") ? true : shippingStatus.Contains("store") ? !addressstr.Contains("-" + x.ShippingAddressId + "-") : addressstr.Contains("-" + x.ShippingAddressId + "-"))
                                                            && (x.CreateDate >= datefrom && x.CreateDate <= dateto)).ToList();

            return list;
        }

        public CheckoutViewModel GetCheckoutViewModel(AppUser user, string productId, int qty)
        {
            var model = new CheckoutViewModel();
            model.Tax = 6.625;
            if (productId != null && productId.Length > 3)
            {
                var product = _productRepository._dbSet.Where(x => x.Barcode.Equals(productId) && x.IsRemoved == false && x.Stock >= qty).FirstOrDefault();
                if(product != null)
                {
                    if (user.UserRole.ToLower().Contains("vendor"))
                    {
                        model.Subtotal = product.VendorPrice * qty;
                    }
                    else
                    {
                        model.Subtotal = product.SalePrice * qty;
                    }
                    model.BuyNowProduct = product;
                    model.Qty = qty;
                }
                else
                {
                    return model;
                }
            }
            else
            {
                var cartItems = GetMyCart(user);
                foreach(var item in cartItems)
                {
                    item.Product.BasePrice = 0;
                    if (!user.UserRole.ToLower().Contains("vendor"))
                    {
                        item.Product.VendorPrice = 0;
                    }
                }
                if (user.UserRole.ToLower().Contains("vendor"))
                {
                    model.Subtotal = cartItems.Select(x => x.Quantity * x.Product.VendorPrice).Sum();
                }
                else
                {
                    model.Subtotal = cartItems.Select(x => x.Quantity * x.Product.SalePrice).Sum();
                }         
                model.CartList = cartItems;
            }

            model.Grandtotal = Math.Ceiling((model.Subtotal + (model.Subtotal * model.Tax / 100)) * 100)/100;
            model.Address.AddressList = _addressRepository._dbSet.Where(x => x.UserId == user.Id && x.IsRemoved == false).ToList();
            model.Address.newAddress = new Addresses();
            model.Address.StateList = GetStateList();
            model.PaymentMethods = _paymentServices.GetMyCards(user.CustomerId);

            return model;
        }

        public string SetAsShipping(int addressId, AppUser user)
        {
            var currAddress = _addressRepository._dbSet.Where(x => x.UserId == user.Id && !x.IsRemoved && x.Id == addressId).FirstOrDefault();

            if(currAddress == null)
            {
                return "Fail: The address doesn't exist";
            }

            var shippingAddress = _addressRepository._dbSet.Where(x => x.IsShipping && x.UserId == user.Id && !x.IsRemoved).FirstOrDefault();

            if(shippingAddress == null)
            {
                return "Fail: Something Went Wrong";
            }
            else if(shippingAddress.Id == currAddress.Id)
            {
                return "Fail: Already is Shipping";
            }

            shippingAddress.IsShipping = false;
            currAddress.IsShipping = true;
            _addressRepository._dbSet.Update(shippingAddress);
            _addressRepository._dbSet.Update(currAddress);
            _addressRepository._context.SaveChanges();

            return "Success";
        }

        public List<State> GetStateList()
        {
            return _stateRepository._dbSet.ToList();
        }

        public string AddNewAddress(AppUser user, Addresses newAddress)
        {
            var address = new Addresses();
            if(newAddress.StateID <= 0 || newAddress.AddressLineA.Length <=0 || newAddress.City.Length <= 0 || newAddress.Zipcode < 1000)
            {
                return "Fail: Please enter Correct Fields";
            }
            else
            {
                address.StateID = newAddress.StateID;
                address.State = _stateRepository._dbSet.Where(x => x.StateID == address.StateID).FirstOrDefault();
                address.AddressLineA = newAddress.AddressLineA;
                address.AddressLineB = newAddress.AddressLineB;
                address.City = newAddress.City;
                address.Zipcode = newAddress.Zipcode;
                address.UserId = user.Id;
                address.IsRemoved = false;
                address.IsShipping = false;

                var addExists = _addressRepository._dbSet.Where(x => x.UserId == user.Id && x.IsRemoved == false).Any();

                if (!addExists)
                {
                    address.IsShipping = true;
                }
                
                try
                {
                    var count = _addressRepository._dbSet.Count();
                    _addressRepository._dbSet.Add(address);
                    _addressRepository._context.SaveChanges();
                    if (count + 1 == _addressRepository._dbSet.Count())
                    {
                        return "Success";
                    }
                    else
                    {
                        return "Fail: Something Went Wrong";
                    }
                }
               catch(Exception)
                {
                    return "Fail: Something Went Wrong";
                }
            }
                
        }

        public string RemoveAddress(AppUser user, int addressId)
        {
            var addressItem = _addressRepository._dbSet.Where(x => x.Id == addressId).FirstOrDefault();
            if(addressItem == null)
            {
                return "Fail: Something Went Wrong";
            }
            if (addressItem.UserId != user.Id)
            {
                return "UnAuthorized";
            }
            if (addressItem.IsShipping)
            {
                return "Fail: You cannot delete Shipping Address. Please Select another Address Or Add a new Address.";
            }
            
            try
            {
                addressItem.IsRemoved = true;
                _addressRepository._dbSet.Update(addressItem);
                _addressRepository._context.SaveChanges();
                return "Success";
            }
            catch (Exception)
            {

                return "Fail: Something went Wrong";
            }
        }

        public string EditCartQty(AppUser user, int cartId, int quantity)
        {
            var cartitem = _cartRepository._dbSet.Where(x => x.Id == cartId).Include(x => x.Product).FirstOrDefault();
            if(cartitem == null)
            {
                return "Fail: Please add Product";
            }
            if(cartitem.UserId != user.Id)
            {
                return "UnAuthorized";
            }
            if (cartitem.IsRemoved)
            {
                cartitem.IsRemoved = false;
            }
            if(quantity <= 0)
            {
                return "Invalid Quantity";
            }
            if(quantity > cartitem.Product.Stock)
            {
                return "Only " + cartitem.Product.Stock + " left in stock.";
            }
            cartitem.Quantity = quantity;
            try
            {
                _cartRepository._dbSet.Update(cartitem);
                _cartRepository._context.SaveChanges();
                return "Success";
            }
            catch (Exception)
            {
                return "Fail: Something went Wrong";
            }
        }

        public List<Cart> GetMyCart(AppUser user)
        {
            var cartItems = _cartRepository._dbSet.Include(x => x.Product).Where(x => x.IsRemoved == false && x.UserId == user.Id && x.Product.IsRemoved == false && x.Product.Stock > 0).ToList();
            var updatedCartList = new List<Cart>();
            foreach (var item in cartItems)
            {
                if (item.Quantity > item.Product.Stock)
                {
                    item.Quantity = item.Product.Stock;
                    updatedCartList.Add(item);          
                }
            }
            if (updatedCartList.Count() > 0)
            {
                _cartRepository._dbSet.UpdateRange(updatedCartList);
                _cartRepository._context.SaveChanges();
            }

            return cartItems;
        }

        public string AddtoCart(AppUser user, string productId, int qty)
        {
            var cart = new Cart();

            cart.UserId = user.Id;
            var product = _productRepository._dbSet.Where(x => x.Barcode == productId).FirstOrDefault();
            // Product Validation
            if (product == null || product.Barcode.Length < 1)
            {
                return "Fail: Product Doesn't exists";
            }
            else if(product.Stock < qty)
            {
                return "Fail: Only " + product.Stock + " left in inventory";
            }
            if(qty <= 0)
            {
                return "Fail: Please enter valid Quantity";
            }

            cart.ProductId = product.ProductID;
            cart.Quantity = qty;
            cart.IsRemoved = false;
            var cartItem = _cartRepository._dbSet.Where(x => x.ProductId == product.ProductID && x.UserId == user.Id).FirstOrDefault();
            if (cartItem != null)
            {
                if (cartItem.IsRemoved == false)
                {
                    return "Cart item already exists";
                }
                cartItem.IsRemoved = false;
                cartItem.Quantity = qty;
                var count = _cartRepository._dbSet.Count();
                _cartRepository._dbSet.Update(cartItem);
                _cartRepository._context.SaveChanges();

                 return "Success";
            }

            var cartcount = _cartRepository._dbSet.Count();
           
            _cartRepository._dbSet.Add(cart);
            _cartRepository._context.SaveChanges();
            if(cartcount + 1 == _cartRepository._dbSet.Count())
            {
                return "Success";
            }
            else
            {
                return "Fail: Something went Wrong. Please try again";
            }
            
        }

        public  ProductDescriptionViewModel GetProductDetailsViewModel(AppUser user, string productid)
        {
            var model = new ProductDescriptionViewModel();

            model.product = _productRepository._dbSet.Where(x => x.Barcode == productid).Include(x => x.Category).Include(x => x.SubCategory).FirstOrDefault();
            model.product.BasePrice = 0;
            model.RecomendedProductList = _productRepository._dbSet.Include(x => x.Category).Include(x => x.SubCategory).Take(3).ToList();
            foreach(var item in model.RecomendedProductList)
            {
                item.BasePrice = 0;
                if (user != null && user.UserRole.ToLower() != "vendor")
                {
                    item.VendorPrice = 0;
                }
            }

            return model;
        }
    }
}
