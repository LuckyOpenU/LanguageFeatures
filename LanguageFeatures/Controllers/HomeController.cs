using iGuardPayroll_WebRole;
using LanguageFeatures.BLL;
using LanguageFeatures.DAL;
using LanguageFeatures.DataModels;
using LanguageFeatures.Models;
using LanguageFeatures.MyEnums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LanguageFeatures.Controllers
{
	public class HomeController : Controller
	{
		HomeServices services = null;
		private const string FILE_DOWNLOAD_COOKIE_NAME = "fileDownload";

		public HomeController()
		{
			services = new HomeServices();
		}

		public ActionResult InsertProduct(Product product)
		{
			string ackMsg;
			bool isProductIDOk, isNameOk, isDescriptionOk, isCategoryOk, isPriceOk;

			MyError myError = services.InsertOrUpdateProduct(product, false, out ackMsg, out isProductIDOk, out isNameOk, out isDescriptionOk, out isCategoryOk, out isPriceOk);

			if (myError != MyError.Success)
			{
				return Json(new { ackMsg = ackMsg, isProductIDOk = isProductIDOk, isNameOk = isNameOk, isDescriptionOk = isDescriptionOk, isCategoryOk = isCategoryOk, isPriceOk = isPriceOk }, JsonRequestBehavior.AllowGet);
			}
			else
			{
				ShoppingCartModels model = services.GetShoppingCartModel(null, Request.Cookies, product.ProductID);
				model.insertedRowID = product.ProductID;
				model.insertRecordAckMsg = "Add Succeeded!";
				return PartialView("ProductList", model);
			}
		}

		public ActionResult UpdateProduct(Product product)
		{
			string ackMsg;
			bool isProductIDOk, isNameOk, isDescriptionOk, isCategoryOk, isPriceOk;

			MyError myError = services.InsertOrUpdateProduct(product, true, out ackMsg, out isProductIDOk, out isNameOk, out isDescriptionOk, out isCategoryOk, out isPriceOk);

			if (myError != MyError.Success)
			{
				return Json(new { ackMsg = ackMsg, isProductIDOk = isProductIDOk, isNameOk = isNameOk, isDescriptionOk = isDescriptionOk, isCategoryOk = isCategoryOk, isPriceOk = isPriceOk }, JsonRequestBehavior.AllowGet);
			}
			else
			{
				ShoppingCartModels model = services.GetShoppingCartModel(null, Request.Cookies, product.ProductID);
				model.insertedRowID = product.ProductID;
				model.insertRecordAckMsg = "Update Succeeded!";
				return PartialView("ProductList", model);
			}
		}

		[HttpGet]
		public ActionResult Index()
		{
			return (Index(null));
		}

		[HttpPost]
		public ActionResult Index(ShoppingCartModels inputModel)
		{
			if (Request.Cookies[FILE_DOWNLOAD_COOKIE_NAME] != null) Response.Cookies[FILE_DOWNLOAD_COOKIE_NAME].Expires = DateTime.Now.AddYears(-1);

			ShoppingCartModels model = services.GetShoppingCartModel(inputModel, Request.Cookies, null);

			// set cookies (150914)
			HttpCookie cookieSortBy = new HttpCookie("cookieSortBy");
			cookieSortBy.Value = model.currentSortBy.ToString();
			Response.Cookies.Add(cookieSortBy);
			HttpCookie cookieSortOrder = new HttpCookie("cookieSortOrder");
			cookieSortOrder.Value = model.currentSortOrder.ToString();
			Response.Cookies.Add(cookieSortOrder);
			HttpCookie cookieBeginWith = new HttpCookie("cookieBeginWith");
			cookieBeginWith.Value = model.currentBeginWith.ToString();
			Response.Cookies.Add(cookieBeginWith);
			HttpCookie cookiePage = new HttpCookie("cookiePage");
			cookiePage.Value = model.currentPage.ToString();
			Response.Cookies.Add(cookiePage);
			HttpCookie cookiePageSize = new HttpCookie("cookiePageSize");
			cookiePageSize.Value = model.currentPageSize.ToString();
			Response.Cookies.Add(cookiePageSize);

			return View(model);

			//if (model.isSubmit != "") return PartialView("ProductList", model);
			//else return View(model);
		}

		public ActionResult Submit(ShoppingCartModels inputModel)
		{
			ShoppingCartModels model = services.GetShoppingCartModel(inputModel, Request.Cookies, null);

			// set cookies (150914)
			HttpCookie cookieSortBy = new HttpCookie("cookieSortBy");
			cookieSortBy.Value = model.currentSortBy.ToString();
			Response.Cookies.Add(cookieSortBy);
			HttpCookie cookieSortOrder = new HttpCookie("cookieSortOrder");
			cookieSortOrder.Value = model.currentSortOrder.ToString();
			Response.Cookies.Add(cookieSortOrder);
			HttpCookie cookieBeginWith = new HttpCookie("cookieBeginWith");
			cookieBeginWith.Value = model.currentBeginWith.ToString();
			Response.Cookies.Add(cookieBeginWith);
			HttpCookie cookiePage = new HttpCookie("cookiePage");
			cookiePage.Value = model.currentPage.ToString();
			Response.Cookies.Add(cookiePage);
			HttpCookie cookiePageSize = new HttpCookie("cookiePageSize");
			cookiePageSize.Value = model.currentPageSize.ToString();
			Response.Cookies.Add(cookiePageSize);

			return PartialView("ProductList", model);
		}


		//for learning only
		public ViewResult AutoProperty()
		{
			//create a new Product object
			ProductModels myProduct = new ProductModels();

			//set the property value
			myProduct.FirstName = "Sean";
			myProduct.LastName = "Cheng";
			myProduct.FullName = "Brian Leung";

			//generate the view
			return View("Result", myProduct);
		}

		//for learning only
		public ActionResult CreateProduct()
		{
			//create a new Product object
			Product myProduct = new Product();

			//set the property values
			myProduct.ProductID = "100";
			myProduct.Name = "Sean";
			myProduct.Description = "A boat for one person";
			myProduct.Price = 275M;
			myProduct.Category = "Watersports";

			return View("Result", myProduct);
		}

		//for learning only
		public ViewResult CreateCollection()
		{
			string[] stringArray = { "apple", "orange", "plum" };
			List<int> intList = new List<int> { 10, 20, 30, 40 };
			Dictionary<string, int> myDict = new Dictionary<string, int> { { "apple", 10 }, { "orange", 20 }, { "plum", 30 } };
			return View("Result", (object)stringArray[1]);
		}

		public ActionResult ProductDetails(string productID)
		{
			ProductDetailsModels model = services.GetProductDetails(productID);

			return PartialView("ProductDetails", model);
		}

		[FileDownload]
		public ActionResult ExportToExcel()
		{
			string contentType = "application/vnd.ms-excel";
			string s = "Hello World!";
			byte[] data = System.Text.Encoding.ASCII.GetBytes(s);

			contentType = "text/csv";
			Response.SetCookie(new HttpCookie(FILE_DOWNLOAD_COOKIE_NAME, "true") { Path = "/" });

			return File(data, contentType, "hello.csv");
		}
	}
}
