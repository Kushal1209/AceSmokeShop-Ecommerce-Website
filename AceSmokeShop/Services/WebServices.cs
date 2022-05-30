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
        private PaymentServices _paymentServices;

        public WebServices(ProductRepository productRepository,
            CategoryRepository categoryRepository, SubCategoryRepository subcategoryRepository,
            StateRepository stateRepository, UserManager<AppUser> userManager, CartRepository cartRepository, AddressRepository addressRepository,
            PaymentServices paymentServices, UserOrdersRepository userOrdersRepository, OrderItemRepository orderItemRepository)
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

        public UHomeViewModel GetHomeViewModel()
        {
            var model = new UHomeViewModel();

            model.FeaturedList = _productRepository._dbSet.Where(x => x.IsFeatured).Include(x => x.Category).Include(x => x.SubCategory).Take(6).ToList();
            model.PromotedList = _productRepository._dbSet.Where(x => x.IsPromoted).Include(x => x.Category).Include(x => x.SubCategory).Take(6).ToList();
            model.CategoryList = _categoryRepository._dbSet.ToList();
            foreach(var item in model.FeaturedList)
            {
                item.BasePrice = 0;
            }
            foreach (var item in model.PromotedList)
            {
                item.BasePrice = 0;
            }

            return model;

        }

        public UProductViewModel GetUserProductsViewModel(int CategoryID = 0, int SubCategoryID = 0, int pageFrom = 1, int ItemsPerPage = 10,
                                                string search = "", string type = "")
        {
            var model = new UProductViewModel();

            model.CategoryList = _categoryRepository._dbSet.ToList();
            model.CategoryList.Insert(0, new Category { CategoryID = 0, CategoryName = "Select Category" });
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
            }
            model.CurrentPage = ++pageFrom;
            model.Type = type;
           
            return model;
        }

        public string PlaceProductOrder(AppUser user, int cardId, string productId, int qty)
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
            double SubTotal = product.SalePrice * qty;
            double Tax = 6.625;
            var GrandTotal = SubTotal + (SubTotal * Tax / 100);

            var OrderId = GenerateOrderID();
            var result = _paymentServices.PlaceOrder(user, cardId, GrandTotal, OrderId);

            if (result.ToLower().Contains("err"))
            {
                return result;
            }
            //Create Order and Update

            var Order = new UserOrders();
            Order.CustOrderId = OrderId;
            Order.OrderStatus = "Order Placed";
            Order.UserId = user.Id;
            Order.TotalAmount = GrandTotal;
            Order.SubTotal = SubTotal;
            Order.Tax = Tax;
            Order.PaymentId = result;
            Order.PaymentStatus = "Paid";
            Order.CreateDate = DateTime.Today;
            var ShippingAddress = _addressRepository._dbSet.Where(x => x.UserId == user.Id && x.IsRemoved == false && x.IsShipping).FirstOrDefault();
            var BillingAddress = _addressRepository._dbSet.Where(x => x.UserId == user.Id && x.IsRemoved == false && x.IsBilling).FirstOrDefault();

            Order.ShippingAddressId = ShippingAddress.Id;
            Order.BillingAddressId = BillingAddress.Id;

            _userOrdersRepository._dbSet.Add(Order);
            _userOrdersRepository._context.SaveChanges();

            Order = _userOrdersRepository._dbSet.Where(x => x.CustOrderId == OrderId).FirstOrDefault();
            if (Order == null)
            {
                //Refund
                var refund = _paymentServices.CreateRefund(result, "duplicate");

                return "Payment Processed but Something Went Wrong! Refund is in Process";
            }

            var orderItem = new OrderItem();
            orderItem.OrderId = Order.Id;
            orderItem.Price = product.SalePrice;
            orderItem.ProductId = product.ProductID;
            orderItem.CustOrderId = OrderId;
            orderItem.Quantity = qty;

            _orderItemRepository._dbSet.Add(orderItem);
            _orderItemRepository._context.SaveChanges();

            product.Stock = product.Stock - qty;
            _productRepository._dbSet.Update(product);
            _productRepository._context.SaveChanges();

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

        public string PlaceCartOrder(AppUser user, int cardId)
        {
            //Validate Everything and Calculate Total
            var cartItems = GetMyCart(user);
            var ShippingAddress = _addressRepository._dbSet.Where(x => x.UserId == user.Id && x.IsRemoved == false && x.IsShipping).FirstOrDefault();
            var BillingAddress = _addressRepository._dbSet.Where(x => x.UserId == user.Id && x.IsRemoved == false && x.IsBilling).FirstOrDefault();
            if (cartItems == null)
            {
                return "Please Add Items to Cart";
            }
            if(ShippingAddress == null || BillingAddress == null)
            {
                return "Fail: Please Choose shipping and billing Addresses";
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
                SubTotal += (item.Product.SalePrice * item.Quantity);
            }
            var Tax = 6.625;
            var GrandTotal = SubTotal + (SubTotal * Tax / 100);

            // Make Payment
            var OrderId = GenerateOrderID();
            var result = _paymentServices.PlaceOrder(user, cardId, GrandTotal, OrderId);

            if (result.ToLower().Contains("err"))
            {
                return result;
            }
            //Create Order and Update

            var Order = new UserOrders();
            Order.CustOrderId = OrderId;
            Order.OrderStatus = "Order Placed";
            Order.UserId = user.Id;
            Order.TotalAmount = GrandTotal;
            Order.SubTotal = SubTotal;
            Order.Tax = Tax;
            Order.PaymentId = result;
            Order.PaymentStatus = "Paid";
            Order.CreateDate = DateTime.Today;
            Order.ShippingAddressId = ShippingAddress.Id;
            Order.BillingAddressId = BillingAddress.Id;
            
            _userOrdersRepository._dbSet.Add(Order);
            _userOrdersRepository._context.SaveChanges();

            Order = _userOrdersRepository._dbSet.Where(x => x.CustOrderId == OrderId).FirstOrDefault();
            if(Order == null)
            {
                //Refund
                var refund = _paymentServices.CreateRefund(result, "duplicate");

                return "Payment Processed but Something Went Wrong! Refund is in Process";
            }

            var OrderItems = new List<OrderItem>();
            var productList = new List<Product>();

            foreach (var item in cartItems)
            {
                var orderItem = new OrderItem();
                orderItem.OrderId = Order.Id;
                orderItem.Price = item.Product.SalePrice;
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

            //Reduce product stock by Qty.
            _productRepository._dbSet.UpdateRange(productList);
            _productRepository._context.SaveChanges();

            //Empty cart after order placing
            _cartRepository._dbSet.UpdateRange(cartItems);
            _cartRepository._context.SaveChanges();

            return "Success";
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

        public OrderViewModel GetOrdersViewModel(AppUser user)
        {
            var model = new OrderViewModel();

            model.userOrdersList = _userOrdersRepository._dbSet.Where(x => x.UserId == user.Id).ToList();
            model.States = _stateRepository._dbSet.ToList();
            var addresses = _addressRepository._dbSet.Where(x => x.UserId == user.Id).ToList();

            foreach(var items in model.userOrdersList)
            {
                items.ShippingAddress = addresses.Where(x => x.Id == items.ShippingAddressId).FirstOrDefault();
                items.BillingAddress = addresses.Where(x => x.Id == items.BillingAddressId).FirstOrDefault();
                items.OrderItems = _orderItemRepository._dbSet.Where(x => x.OrderId == items.Id).Include(x => x.Product).Include(x => x.Product.Category).Include(x => x.Product.SubCategory).ToList();
            }

            return model;
        }

        public CheckoutViewModel GetCheckoutViewModel(AppUser user, string productId, int qty)
        {
            var model = new CheckoutViewModel();
            model.Tax = 6.625;
            if (productId != null && productId.Length > 3)
            {
                var product = _productRepository._dbSet.Where(x => x.Barcode == productId && x.IsRemoved == false && x.Stock >= qty).FirstOrDefault();
                if(product != null)
                {
                    model.Subtotal = product.SalePrice * qty;
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
                model.Subtotal = cartItems.Select(x => x.Quantity * x.Product.SalePrice).Sum();
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

        public string SetAsBilling(int addressId, AppUser user)
        {
            var currAddress = _addressRepository._dbSet.Where(x => x.UserId == user.Id && !x.IsRemoved && x.Id == addressId).FirstOrDefault();

            if (currAddress == null)
            {
                return "Fail: The address doesn't exist";
            }

            var billingAddress = _addressRepository._dbSet.Where(x => x.IsBilling && x.UserId == user.Id && !x.IsRemoved).FirstOrDefault();

            if (billingAddress == null)
            {
                return "Fail: Something Went Wrong";
            }
            else if (billingAddress.Id == currAddress.Id)
            {
                return "Fail: Already is Shipping";
            }

            billingAddress.IsBilling = false;
            currAddress.IsBilling = true;
            _addressRepository._dbSet.Update(billingAddress);
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
                address.IsBilling = false;
                address.IsShipping = false;

                var addExists = _addressRepository._dbSet.Where(x => x.UserId == user.Id && x.IsRemoved == false).Any();

                if (!addExists)
                {
                    address.IsBilling = true;
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
            if (addressItem.IsShipping || addressItem.IsBilling)
            {
                return "Fail: You cannot delete Shipping Or Billing Address. Please Select another Address Or Add a new Address.";
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
            var cartitem = _cartRepository._dbSet.Where(x => x.Id == cartId).FirstOrDefault();
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
            var cartItems = _cartRepository._dbSet.Include(x => x.Product).Where(x => x.IsRemoved == false && x.UserId == user.Id && x.Product.IsRemoved == false).ToList();
            foreach(var item in cartItems)
            {
                item.Product.BasePrice = 0;
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

        public  ProductDescriptionViewModel GetProductDetailsViewModel(string productid)
        {
            var model = new ProductDescriptionViewModel();

            model.product = _productRepository._dbSet.Where(x => x.Barcode == productid).Include(x => x.Category).Include(x => x.SubCategory).FirstOrDefault();
            model.product.BasePrice = 0;
            model.RecomendedProductList = _productRepository._dbSet.Include(x => x.Category).Include(x => x.SubCategory).Take(3).ToList();
            foreach(var item in model.RecomendedProductList)
            {
                item.BasePrice = 0;
            }

            return model;
        }
    }
}
