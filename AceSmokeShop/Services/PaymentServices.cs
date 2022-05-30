using AceSmokeShop.Areas.Identity.Data;
using AceSmokeShop.Core.Repositories;
using AceSmokeShop.Models;
using AceSmokeShop.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Stripe;

namespace AceSmokeShop.Services
{
    public class PaymentServices
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ProductRepository _productRepository;
        private readonly CategoryRepository _categoryRepository;
        private readonly SubCategoryRepository _subcategoryRepository;
        private readonly StateRepository _stateRepository;
        private readonly CartRepository _cartRepository;
        private readonly AddressRepository _addressRepository;


        public PaymentServices(ProductRepository productRepository,
            CategoryRepository categoryRepository, SubCategoryRepository subcategoryRepository,
            StateRepository stateRepository, UserManager<AppUser> userManager, CartRepository cartRepository, AddressRepository addressRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _subcategoryRepository = subcategoryRepository;
            _stateRepository = stateRepository;
            _userManager = userManager;
            _cartRepository = cartRepository;
            _addressRepository = addressRepository;
        }

        public string AddCard(AppUser user, PaymentCardViewModel model)
        {

            if(user != null && user.CustomerId != null && user.CustomerId.Length > 0)
            {
                try
                {
                    var options = new PaymentMethodCreateOptions
                    {
                        Type = "card",
                        Card = new PaymentMethodCardOptions
                        {
                            Number = model.CardNumber.ToString(),
                            ExpMonth = model.Month,
                            ExpYear = model.Year,
                            Cvc = model.CVC.ToString(),
                        }
                    };
                    var service = new PaymentMethodService();
                    var paymentMethod = service.Create(options);

                    if(paymentMethod == null)
                    {
                        return "Something Went Wrong";
                    }

                    var attachOption = new PaymentMethodAttachOptions
                    {
                        Customer = user.CustomerId
                    };
                    var attachservice = new PaymentMethodService();
                    var result = attachservice.Attach(paymentMethod.Id, attachOption);
                }catch(Exception ex)
                {
                   return ex.Message;
                }
               
            }
            return "Success";
        }

        public StripeList<PaymentMethod> GetMyCards(string customerId)
        {
            var options = new PaymentMethodListOptions
            {
                Type = "card",
                Customer = customerId
            };
            var service = new PaymentMethodService();
            StripeList<PaymentMethod> paymentMethods = service.List(options);

            return paymentMethods;
        }

        public string RemoveCard(AppUser user, int cardNumber)
        {
            try
            {
                var pmId = GetMyCards(user.CustomerId).ElementAt(cardNumber).Id;

                var service = new PaymentMethodService();
                var pm = service.Get(pmId);

                if (pm == null)
                {
                    return "Fail: This Card doesn't Exist";
                }
                service.Detach(pmId);

                return "Success";

            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        public string PlaceOrder(AppUser user, int cardId, double grandTotal, string OrderId)
        {
            try
            {
                var pmId = GetMyCards(user.CustomerId).ElementAt(cardId).Id;

                if (pmId == null)
                {
                    return "err: Please Select Another Card";
                }
                var Amount = Math.Ceiling(grandTotal * 100);

                var option = new PaymentIntentCreateOptions
                {
                    Amount = (long?)Amount,
                    Currency = "usd",
                    PaymentMethodTypes = new List<string>
                    {
                        "card"
                    },
                    CaptureMethod = "automatic",
                    Confirm = true,
                    PaymentMethod = pmId,
                    Customer = user.CustomerId,
                    Description = "Ace Smoke Shop Order",
                    StatementDescriptor = OrderId,
                    
                };
                var service = new PaymentIntentService();
                var result = service.Create(option);

                return result.Id;

            }
            catch (Exception ex)
            {
                return "Err:" + ex.Message;
            }
        }

        public string CreateRefund(string pmId, string reason)
        {
            try
            {
                var options = new RefundCreateOptions
                {
                    PaymentIntent = pmId,
                    Reason = reason,
                };
                var service = new RefundService();
                service.Create(options);

                return "Success";
            }
            catch (Exception ex)
            {

                return "Fail: " + ex.Message;
            }
        }
    }
}
