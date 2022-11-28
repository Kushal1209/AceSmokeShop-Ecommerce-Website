using AceSmokeShop.Areas.Identity.Data;
using AceSmokeShop.Core.Repositories;
using AceSmokeShop.Models;
using AceSmokeShop.ViewModel;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using Stripe;
using AuthorizeNet;
using AuthorizeNet.Api.Contracts.V1;
using AuthorizeNet.Api.Controllers.Bases;
using AuthorizeNet.Api.Controllers;
using System.Threading.Tasks;

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
        private readonly TransactionRepository _transactionRepository;
        private readonly UserOrdersRepository _userOrdersRepository;

        public PaymentServices(ProductRepository productRepository,
            CategoryRepository categoryRepository, SubCategoryRepository subcategoryRepository,
            StateRepository stateRepository, UserManager<AppUser> userManager, CartRepository cartRepository,
            AddressRepository addressRepository, TransactionRepository transactionRepository, 
            UserOrdersRepository userOrdersRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _subcategoryRepository = subcategoryRepository;
            _stateRepository = stateRepository;
            _userManager = userManager;
            _cartRepository = cartRepository;
            _addressRepository = addressRepository;
            _transactionRepository = transactionRepository;
            _userOrdersRepository = userOrdersRepository;

            ApiOperationBase<ANetApiRequest, ANetApiResponse>.RunEnvironment = AuthorizeNet.Environment.SANDBOX;

            ApiOperationBase<ANetApiRequest, ANetApiResponse>.MerchantAuthentication = new merchantAuthenticationType()
            {
                name = "7Bsw4L4f",
                ItemElementName = ItemChoiceType.transactionKey,
                Item = "38Hy4de53j8D7BuD",
            };
        }

        public string AddCard(AppUser user, PaymentCardViewModel model)
        {
            if(user != null && user.CustomerId != null && user.CustomerId.Length > 0)
            {
                try
                {
                     var creditCard = new creditCardType
                     {
                        cardNumber = model.CardNumber.Replace(" ", ""),
                        expirationDate = model.ExpMonthYear.Replace(" / ", ""),
                        cardCode = model.CVC.ToString()
                     };

                    paymentType cc = new paymentType { Item = creditCard };

                    customerPaymentProfileType echeckPaymentProfile = new customerPaymentProfileType();
                    echeckPaymentProfile.payment = cc;

                    var request = new createCustomerPaymentProfileRequest
                    {
                        customerProfileId = user.CustomerId,
                        paymentProfile = echeckPaymentProfile,
                        validationMode = validationModeEnum.none
                    };

                    // instantiate the controller that will call the service
                    var controller = new createCustomerPaymentProfileController(request);
                    controller.Execute();

                    // get the response from the service (errors contained if any)
                    createCustomerPaymentProfileResponse response = controller.GetApiResponse();

                    // validate response 
                    if (response != null)
                    {
                        if (response.messages.resultCode == messageTypeEnum.Ok)
                        {
                            if (response.messages.message != null)
                            {
                                Console.WriteLine("Success! Customer Payment Profile ID: " + response.customerPaymentProfileId);

                                return "Success";
                            }
                        }
                        else
                        {
                            Console.WriteLine("Customer Payment Profile Creation Failed.");
                            Console.WriteLine("Error Code: " + response.messages.message[0].code);
                            Console.WriteLine("Error message: " + response.messages.message[0].text);
                           
                            return response.messages.message[0].text;
                        }
                    }
                    else
                    {
                        if (controller.GetErrorResponse().messages.message.Length > 0)
                        {
                            Console.WriteLine("Customer Payment Profile Creation Failed.");
                            Console.WriteLine("Error Code: " + controller.GetErrorResponse().messages.message[0].code);
                            Console.WriteLine("Error message: " + controller.GetErrorResponse().messages.message[0].text);

                            return controller.GetErrorResponse().messages.message[0].text;
                        }
                        else
                        {
                            Console.WriteLine("Null Response.");

                            return "Err: Please try again!!";
                        }
                    }
                }
                catch(Exception ex)
                {
                   return ex.Message;
                }
               
            }
            return "Something went Wrong";
        }

        public async Task<string> GetCustomerProfileID(AppUser user)
        {
            customerProfileType customerProfile = new customerProfileType();
            customerProfile.email = user.Email;

            var request = new createCustomerProfileRequest { profile = customerProfile, validationMode = validationModeEnum.none };

            // instantiate the controller that will call the service
            var controller = new createCustomerProfileController(request);
            controller.Execute();

            // get the response from the service (errors contained if any)
            createCustomerProfileResponse response = controller.GetApiResponse();

            // validate response 
            if (response != null)
            {
                if (response.messages.resultCode == messageTypeEnum.Ok)
                {
                    if (response.messages.message != null)
                    {
                        user.CustomerId = response.customerProfileId;
                        user.LockoutEnabled = false;
                       await _userManager.UpdateAsync(user);

                        return response.customerProfileId;
                    }
                    user.LockoutEnabled = false;
                    await _userManager.UpdateAsync(user);

                    return "";
                }
                else
                {
                    user.LockoutEnabled = false;
                    await _userManager.UpdateAsync(user);

                    return "";
                }
            }
            else
            {
                if (controller.GetErrorResponse().messages.message.Length > 0)
                {
                    user.LockoutEnabled = false;
                    await _userManager.UpdateAsync(user);

                    return "";
                }
                else
                {
                    user.LockoutEnabled = false;
                    await _userManager.UpdateAsync(user);

                    return "";
                }
            }
        }

        public double GetCurrentBalance(AppUser user)
        {
            if (user == null || user.UserRole != "ADMIN")
            {
                return 0;
            }

            var service = new BalanceService();
            Balance balance = service.Get();

            return (double)balance.Available[0].Amount / (double)100;
        }

        public List<CardInfo> GetMyCards(string profileId)
        {
            var cardList = new List<CardInfo>();

            try
            {
                var request = new getCustomerProfileRequest();
                request.customerProfileId = profileId;

                // instantiate the controller that will call the service
                var controller = new getCustomerProfileController(request);
                controller.Execute();

                // get the response from the service (errors contained if any)
                var response = controller.GetApiResponse();

                if (response != null && response.messages.resultCode == messageTypeEnum.Ok && response.profile != null && response.profile.paymentProfiles != null)
                {

                    for (int i = 0; i < response.profile.paymentProfiles.Count(); i++)
                    {
                        var Obj = new CardInfo();
                        Obj.paymentProfileId = response.profile.paymentProfiles[i].customerPaymentProfileId;
                        Obj.cardNumber = ((creditCardMaskedType)response.profile.paymentProfiles[i].payment.Item).cardNumber;
                        Obj.cardType = ((creditCardMaskedType)response.profile.paymentProfiles[i].payment.Item).cardType;
                        cardList.Add(Obj);
                    }
                }
                
                return cardList;
            }
            catch (Exception)
            {
                return cardList;
            } 
         
        }

        public string RemoveCard(AppUser user, int cardNumber)
        {
            try
            {
                //please update the subscriptionId according to your sandbox credentials
                var request = new deleteCustomerPaymentProfileRequest
                {
                    customerProfileId = user.CustomerId,
                    customerPaymentProfileId = cardNumber.ToString()
                };

                //Prepare Request
                var controller = new deleteCustomerPaymentProfileController(request);
                controller.Execute();

                //Send Request to EndPoint
                deleteCustomerPaymentProfileResponse response = controller.GetApiResponse();
                if (response != null && response.messages.resultCode == messageTypeEnum.Ok)
                {
                    if (response != null && response.messages.message != null)
                    {
                        return "Success";
                    }
                }
                else if (response != null)
                {
                    return "Err :" + response.messages.message[0].code + "  " + response.messages.message[0].text;
                }

                return "Err: Something Went Wrong!!";

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
                var Obj = new CardInfo();

                customerProfilePaymentType profileToCharge = new customerProfilePaymentType();
                profileToCharge.customerProfileId = user.CustomerId;
                profileToCharge.paymentProfile = new paymentProfile { paymentProfileId = cardId.ToString() };

                var transactionRequest = new transactionRequestType
                {
                    transactionType = transactionTypeEnum.authCaptureTransaction.ToString(),    // refund type
                    amount = (decimal)grandTotal,
                    profile = profileToCharge,
                    refTransId = OrderId
                };

                var request = new createTransactionRequest { transactionRequest = transactionRequest };

                // instantiate the collector that will call the service
                var controller = new createTransactionController(request);
                controller.Execute();

                // get the response from the service (errors contained if any)
                var response = controller.GetApiResponse();

                // validate response
                if (response != null)
                {
                    if (response.messages.resultCode == messageTypeEnum.Ok)
                    {
                        if (response.transactionResponse.messages != null)
                        {
                            var transactionDetails = GetTransactionDetails(response.transactionResponse.transId);
                            if(transactionDetails.transaction.transactionStatus.Equals("capturedPendingSettlement") || transactionDetails.transaction.transactionStatus.Equals("settledSuccessfully"))
                            {
                                var createpayment = new Transactions();
                                createpayment.UserId = user.Id;
                                createpayment.OrderId = OrderId;
                                createpayment.Amount = grandTotal;
                                createpayment.UserRole = user.UserRole;
                                createpayment.CreateDate = DateTime.Now;
                                createpayment.PaymentMethod = "Card";
                                createpayment.PaymentIntentId = response.transactionResponse.transId;
                                createpayment.TransactionType = "Full";
                                createpayment.Status = "Completed";

                                _transactionRepository._dbSet.Add(createpayment);
                                _transactionRepository._context.SaveChanges();

                                return response.transactionResponse.transId;
                            }

                            return "Err: Unsuccessfull. Something Went Wrong!";
                        }
                        else
                        {
                            if (response.transactionResponse.errors != null)
                            {
                                return "Err " + response.transactionResponse.errors[0].errorText;
                            }
                            return "Err: Unsuccessfull. Something Went Wrong!";
                        }
                    }
                    else
                    {
                        Console.WriteLine("Failed Transaction.");
                        if (response.transactionResponse != null && response.transactionResponse.errors != null)
                        {
                            return "Err " + response.transactionResponse.errors[0].errorText;
                        }
                        else
                        {
                            return "Err: Unsuccessfull. Something Went Wrong!";
                        }
                    }
                }
                else
                {
                    return "Err: Something Went Wrong";
                }
            }
            catch (Exception ex)
            {
                return "Err:" + ex.Message;
            }
        }

        private getTransactionDetailsResponse GetTransactionDetails(string pmId)
        {
            var request = new getTransactionDetailsRequest();
            request.transId = pmId;

            // instantiate the controller that will call the service
            var controller = new getTransactionDetailsController(request);
            controller.Execute();

            // get the response from the service (errors contained if any)
            var response = controller.GetApiResponse();
            return response;
        }

        private string GetRefundTrans(string pmId, string refundId) 
        {
            if (refundId == null || refundId == "")
            {
                var cardresponse = GetTransactionDetails(refundId);
                var cardInfo = (creditCardMaskedType)cardresponse.transaction.payment.Item;
                var creditCard = new creditCardType
                {
                    cardNumber = cardInfo.cardNumber,
                    expirationDate = cardInfo.expirationDate
                };

                //standard api call to retrieve response
                var paymentType = new paymentType { Item = creditCard };

                var transactionRequest = new transactionRequestType
                {
                    transactionType = transactionTypeEnum.refundTransaction.ToString(),    // refund type
                    payment = paymentType,
                    amount = cardresponse.transaction.authAmount,
                    refTransId = cardresponse.transaction.refTransId
                };

                var request = new createTransactionRequest { transactionRequest = transactionRequest };

                // instantiate the controller that will call the service
                var controller = new createTransactionController(request);
                controller.Execute();

                // get the response from the service (errors contained if any)
                var response = controller.GetApiResponse();

                // validate response
                if (response != null)
                {
                    if (response.messages.resultCode == messageTypeEnum.Ok)
                    {
                        if (response.transactionResponse.messages != null)
                        {
                            var userorder = _userOrdersRepository._dbSet.Where(x => x.PaymentId == pmId).FirstOrDefault();
                            if(userorder != null)
                            {
                                userorder.RefundId = response.transactionResponse.transId;
                                _userOrdersRepository._dbSet.Update(userorder);
                                _userOrdersRepository._context.SaveChanges();
                            }
                            var transaction = _transactionRepository._dbSet.Where(x => x.PaymentIntentId == pmId).FirstOrDefault();
                            if(transaction != null)
                            {
                                transaction.RefundId = response.transactionResponse.transId;
                                _transactionRepository._dbSet.Update(transaction);
                                _transactionRepository._context.SaveChanges();
                            }
                            return GetTransactionDetails(response.transactionResponse.transId).transaction.transactionStatus;
                        }
                        else
                        {
                            if (response.transactionResponse.errors != null)
                            {

                                return "Err: " + response.transactionResponse.errors[0].errorText;
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Failed Transaction.");
                        if (response.transactionResponse != null && response.transactionResponse.errors != null)
                        {
                            Console.WriteLine("Error Code: " + response.transactionResponse.errors[0].errorCode);
                            Console.WriteLine("Error message: " + response.transactionResponse.errors[0].errorText);

                            return "Err: " + response.transactionResponse.errors[0].errorText;
                        }
                        else
                        {
                            Console.WriteLine("Error Code: " + response.messages.message[0].code);
                            Console.WriteLine("Error message: " + response.messages.message[0].text);

                            return "Err: " + response.messages.message[0].text;
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Null Response.");

                    return "Err: Null Response";
                }
            }
            else
            {
                var trans = GetTransactionDetails(refundId);

                return "RedundSettledSuccessfully";
            }

            return "Err: Something Went Wrong";
        }

        private string GetVoidedTrans(string pmId, string voidId)
        {
            if (voidId == null || voidId == "")
            {
                var transactionRequest = new transactionRequestType
                {
                    transactionType = transactionTypeEnum.voidTransaction.ToString(),    // refund type
                    refTransId = pmId
                };

                var request = new createTransactionRequest { transactionRequest = transactionRequest };

                // instantiate the controller that will call the service
                var controller = new createTransactionController(request);
                controller.Execute();

                // get the response from the service (errors contained if any)
                var response = controller.GetApiResponse();

                // validate response
                if (response != null)
                {
                    if (response.messages.resultCode == messageTypeEnum.Ok)
                    {
                        if (response.transactionResponse.messages != null)
                        {
                            var userorder = _userOrdersRepository._dbSet.Where(x => x.PaymentId == pmId).FirstOrDefault();
                            if (userorder != null)
                            {
                                userorder.VoidId = response.transactionResponse.transId;
                                _userOrdersRepository._dbSet.Update(userorder);
                                _userOrdersRepository._context.SaveChanges();
                            }
                            var transaction = _transactionRepository._dbSet.Where(x => x.PaymentIntentId == pmId).FirstOrDefault();
                            if (transaction != null)
                            {
                                transaction.VoidId = response.transactionResponse.transId;
                                _transactionRepository._dbSet.Update(transaction);
                                _transactionRepository._context.SaveChanges();
                            }
                            return GetTransactionDetails(response.transactionResponse.transId).transaction.transactionStatus;
                        }
                        else
                        {
                            if (response.transactionResponse.errors != null)
                            {
                                return "Err :" + response.transactionResponse.errors[0].errorText;
                            }
                        }
                    }
                    else
                    {
                        if (response.transactionResponse != null && response.transactionResponse.errors != null)
                        {
                            return "Err :" + response.transactionResponse.errors[0].errorText;
                        }
                        else
                        {
                            return "Err :" + response.messages.message[0].text;
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Null Response.");

                    return "Err : Null Response";
                }
            }
            else
            {
                var trans = GetTransactionDetails(voidId);

                return "CapturedPendingSettlement";
            }

            return "Err : Something Went Wrong";
        }

        public string CancelTransaction(string pmId, string reason, string refundId, string voidId)
        {
            try
            {
                var transDetails = GetTransactionDetails(pmId);

                if (transDetails != null && transDetails.messages.resultCode == messageTypeEnum.Ok)
                {
                    if (transDetails.transaction == null)
                    {
                        return "Err: Something Went Wrong.";
                    }
                    else if(transDetails.transaction.transId == pmId)
                    {
                        switch (transDetails.transaction.transactionStatus)
                        {
                            case "capturedPendingSettlement":
                                return GetVoidedTrans(pmId, voidId);
                            case "settledSuccessfully":
                                return GetRefundTrans(pmId, refundId);
                            default: return transDetails.transaction.transactionStatus;
                        }
                    }
                    else
                    {
                        return "Err: This Transaction Doesn't Exist";
                    }
                }
                else if (transDetails != null)
                {
                    return "Err :" + transDetails.messages.message[0].code + "  " + transDetails.messages.message[0].text;
                }

                return "Err : Somtething Went Wrong";
            }
            catch (Exception ex)
            {
                return "Fail: " + ex.Message;
            }
        }
    }
}
//var cardInfo = (creditCardMaskedType)response.transaction.payment.Item;
//var creditCard = new creditCardType
//{
//    cardNumber = cardInfo.cardNumber,
//    expirationDate = cardInfo.expirationDate
//};

////standard api call to retrieve response
//var paymentType = new paymentType { Item = creditCard };

//var transactionRequest = new transactionRequestType
//{
//    transactionType = transactionTypeEnum.refundTransaction.ToString(),    // refund type
//    payment = paymentType,
//    amount = response.transaction.authAmount,
//    refTransId = pmId,
//    amountSpecified = true
//};

//var refundrequest = new createTransactionRequest { transactionRequest = transactionRequest };

//// instantiate the controller that will call the service
//var refundcontroller = new createTransactionController(refundrequest);
//refundcontroller.Execute();

//// get the response from the service (errors contained if any)
//var refundresponse = refundcontroller.GetApiResponse();

//// validate response
//if (refundresponse != null)
//{
//    if (refundresponse.messages.resultCode == messageTypeEnum.Ok)
//    {
//        if (refundresponse.transactionResponse.messages != null)
//        {
//            return "Success";
//        }
//        else
//        {
//            if (refundresponse.transactionResponse.errors != null)
//            {
//                return "Err :" + refundresponse.transactionResponse.errors[0].errorText;
//            }
//            return "Err: Failed Transaction";
//        }
//    }
//    else
//    {
//        if (refundresponse.transactionResponse != null && refundresponse.transactionResponse.errors != null)
//        {
//            return "Err :" + refundresponse.transactionResponse.errors[0].errorText;
//        }
//        else
//        {
//            return "Err :" + response.messages.message[0].code + "  " + refundresponse.messages.message[0].text;
//        }
//    }
//}
//else
//{
//    return "Err : Something Went Wrong";
//}