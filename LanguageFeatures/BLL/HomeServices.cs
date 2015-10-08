using LanguageFeatures.DAL;
using LanguageFeatures.DataModels;
using LanguageFeatures.Models;
using LanguageFeatures.MyEnums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LanguageFeatures.BLL
{
	public class HomeServices : BaseServices
	{

		public System.Data.DataTable ListProducts(System.Data.DataTable products)
		{
			List<Product> listOfProducts = new List<Product>();

			using (ProductRepository rpt = new ProductRepository())
			{
				listOfProducts = rpt.GetlistProducts();
				foreach (Product n in listOfProducts)
				{
					products.Rows.Add(n.ProductID,n.Name,n.Description,n.Category,n.Price);
				}
			}

			return products;
		}

		/// <summary>
		/// show the product detail by getting existing id (150930)
		/// </summary>
		public ProductDetailsModels GetProductDetails(string productID)
		{
			ProductDetailsModels n = new ProductDetailsModels();
			Product data = new Product();
			using (ProductRepository rpt = new ProductRepository())
			{
				data = rpt.GetProductByProductID(productID);
				n.productID = data.ProductID;
				n.name = data.Name;
				n.decription = data.Description;
				n.cetagory = data.Category;
				n.price = data.Price;
			}

			return n;
		}

		/// <summary>
		/// if productID is not null, will force showing this record by adjusting current page (150923)
		/// </summary>
		public ShoppingCartModels GetShoppingCartModel(ShoppingCartModels inputModel, HttpCookieCollection cookies, string productID)
		{
			ShoppingCartModels n = new ShoppingCartModels();

			if (inputModel == null)
			{
				inputModel = new ShoppingCartModels();
				inputModel.newBeginWith = "all";
				inputModel.newSortOrder = "ascending";
				inputModel.newSortBy = "productID";
				inputModel.isSubmit = "";
				inputModel.newPage = 1;
				inputModel.newPageSize = 10;

				if (cookies != null)
				{
					if (cookies["cookieSortBy"] != null) inputModel.newSortBy = cookies["cookieSortBy"].Value;
					if (cookies["cookieSortOrder"] != null) inputModel.newSortOrder = cookies["cookieSortOrder"].Value;
					if (cookies["cookieBeginWith"] != null) inputModel.newBeginWith = cookies["cookieBeginWith"].Value;
					if (cookies["cookiePage"] != null) inputModel.newPage = int.Parse(cookies["cookiePage"].Value);
					if (cookies["cookiePageSize"] != null) inputModel.newPageSize = int.Parse(cookies["cookiePageSize"].Value);
				}
			}

			n.currentSortOrder = inputModel.newSortOrder;
			n.currentSortBy = inputModel.newSortBy;
			n.currentBeginWith = inputModel.newBeginWith;
			n.isSubmit = inputModel.isSubmit;
			n.currentPage = inputModel.newPage;
			n.currentPageSize = inputModel.newPageSize;

			if (productID != null)
			{
				using (ProductRepository rpt = new ProductRepository())
				{
					int rowIndex = rpt.GetRowIndexByProductID(productID, n.currentSortBy, n.currentSortOrder);

					// determine the currentPage based on this rowIndex (150923)
					n.currentPage = (rowIndex / n.currentPageSize) + 1;
				}
				cookies["cookiePage"].Value = n.currentPage.ToString();
			}
			else if (n.currentPage < 1)
			{
				n.currentPage = 1;
			}
			else if (inputModel.currentSortBy != inputModel.newSortBy || inputModel.currentSortOrder != inputModel.newSortOrder || inputModel.currentBeginWith != inputModel.newBeginWith || inputModel.currentPageSize != inputModel.newPageSize)
			{
				n.currentPage = 1;
			}

			int startRowIndex = (n.currentPage - 1) * n.currentPageSize;
			int maximumRows = n.currentPageSize;

			using (ProductRepository rpt = new ProductRepository())
			{
				n.Products = rpt.GetProductListByName(n.currentBeginWith, n.currentSortBy, n.currentSortOrder, startRowIndex, maximumRows);
				n.numOfProducts = rpt.GetProductCountByName(n.currentBeginWith);
			}

			if (n.numOfProducts % n.currentPageSize == 0) n.numOfPages = (n.numOfProducts / n.currentPageSize);
			else n.numOfPages = ((n.numOfProducts / n.currentPageSize) + 1);

			return n;
		}

		public MyError InsertOrUpdateProduct(Product product, bool isModify, out string ackMsg, out bool isProductIDOk, out bool isNameOk, out bool isDescriptionOk, out bool isCategoryOk, out bool isPriceOk)
		{
			int count;
			MyError n = MyError.Success;
			ackMsg = null;
			isProductIDOk = isNameOk = isDescriptionOk = isCategoryOk = isPriceOk = true;

			if (!isModify)
			{
				using (ProductRepository checkExistID = new ProductRepository())
				{
					count = checkExistID.CheckExistID(product.ProductID);
				}
				if (count > 0)
				{
					n = MyError.Invalid;
					ackMsg = "Invalid Input!";
					isProductIDOk = false;
				}
			}

			if (string.IsNullOrEmpty(product.ProductID) == true)
			{
				n = MyError.Invalid;
				ackMsg = "Invalid Input!";
				isProductIDOk = false;
			}

			if (string.IsNullOrEmpty(product.Name) == true)
			{
				n = MyError.Invalid;
				ackMsg = "Invalid Input!";
				isNameOk = false;
			}
			if (string.IsNullOrEmpty(product.Description) == true)
			{
				n = MyError.Invalid;
				ackMsg = "Invalid Input!";
				isDescriptionOk = false;
			}
			if (product.Price == 0)
			{
				n = MyError.Invalid;
				ackMsg = "Invalid Input!";
				isPriceOk = false;
			}

			if (n == MyError.Success)
			{
				using (ProductRepository rpt = new ProductRepository())
				{
					if (!isModify) n = rpt.InsertProduct(product);
					else rpt.UpdateProduct(product);
				}
			}

			return n;
		}

		public int GetProductRowIndexByProductID(string productID, string sortBy, string sortOrder)
		{
			int n = 0;

			using (ProductRepository rpt = new ProductRepository())
			{
				n = rpt.GetRowIndexByProductID(productID, sortBy, sortOrder);
			}

			return n;
		}
	}
}