using AceSmokeShop.Areas.Identity.Data;
using AceSmokeShop.Core.Repositories;
using AceSmokeShop.Models;
using AceSmokeShop.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private int ProductCount = 0;
        private int UserCount = 0;

        public AdminServices(ProductRepository productRepository, 
            CategoryRepository categoryRepository, SubCategoryRepository subcategoryRepository,
            StateRepository stateRepository, UserManager<AppUser> userManager)
        {
            //_userManager = userManager;
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _subcategoryRepository = subcategoryRepository;
            _stateRepository = stateRepository;
            _userManager = userManager;
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
                UserCount = _userManager.Users.Count();
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

                }
                else
                {
                    userlist = _userManager.Users.Where(x => x.UserRole.Contains(UserRole) && (x.Fullname.Contains(search) || x.Email.Contains(search)))
                        .Skip(pageTotal * pageFrom).Take(pageTotal).ToList();

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
                    ProductCount = _productRepository._dbSet.Where(x => (x.ProductName.Contains(search) || x.Description.Contains(search) || x.Barcode.Contains(search)) && x.IsRemoved == false).Skip(pageTotal * pageFrom).Count();
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
                    if(_productRepository._dbSet.Where(x => x.ProductID == newProduct.ProductID || x.Barcode == newProduct.Barcode || x.ProductName == newProduct.ProductName).Any()){
                        return "This Product Already Exists: ProductID: " + newProduct.ProductID + ", BarCode: " + newProduct.Barcode + ", ProductName: " + newProduct.ProductName;
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
                        var currentCount = _productRepository._dbSet.Count();
                        _productRepository._dbSet.Add(newProduct);
                        await _productRepository._context.SaveChangesAsync();
                        if(currentCount == _productRepository._dbSet.Count() - 1)
                        {
                            return "Success";
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

                    if (user.Email != editUser.UserEmail && thisuser!= null)
                    {
                        thisuser.UserRole = editUser.UserRole;

                        //thisuser.LockoutEnabled = editUser.IsActive;
                        //TODO: Toggle isActive Set in user like above.
                        //TODO: Update UserRole change to stripe customer data.
                        await _userManager.UpdateAsync(thisuser);
                       // await _userManager.

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