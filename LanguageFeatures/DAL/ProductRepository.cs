using LanguageFeatures.DataModels;
using LanguageFeatures.MyEnums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LanguageFeatures.DAL
{
	public class ProductRepository : BaseRepository
	{
		//string cachePrefix = "Product_";

		public List<Product> GetlistProducts()
		{
			List<Product> n = ctx.Products.OrderBy(x => x.ID).ToList();
			return n;
		}

		public List<Product> GetProductListByName(string nameBeginWith, string sortBy, string sortOrder, int startRowIndex, int maximumRows)
		{
			string cacheKey = cachePrefix + nameBeginWith + "_" + sortBy + "_" + sortOrder + "_" + startRowIndex + "_" + maximumRows;

			List<Product> n = null;

			if (HttpRuntime.Cache[cacheKey] != null)
			{
				n = (List<Product>)HttpRuntime.Cache[cacheKey];
			}
			else
			{
				IQueryable<Product> query = ctx.Products;

				// handle filter (150916)
				if (nameBeginWith != "all") query = query.Where(x => x.Name.StartsWith(nameBeginWith));

				// then handle sort order (150916)
				if (sortOrder == "ascending")
				{
					if (sortBy == "productID") query = query.OrderBy(x => x.ProductID);
					else if (sortBy == "name") query = query.OrderBy(x => x.Name);
					else if (sortBy == "description") query = query.OrderBy(x => x.Description);
					else if (sortBy == "category") query = query.OrderBy(x => x.Category);
					else query = query.OrderBy(x => x.Price);
				}
				else
				{
					if (sortBy == "productID") query = query.OrderByDescending(x => x.ProductID);
					else if (sortBy == "name") query = query.OrderByDescending(x => x.Name);
					else if (sortBy == "description") query = query.OrderByDescending(x => x.Description);
					else if (sortBy == "category") query = query.OrderByDescending(x => x.Category);
					else query = query.OrderByDescending(x => x.Price);
				}

				// debug
				n = query.Skip(startRowIndex).Take(maximumRows).ToList();
				//Product newProduct = new Product();
				//newProduct.ProductID = "BB01";
				//newProduct.Name = "Hello";
				//newProduct.Description = "Hello World!";
				//newProduct.Category = "MyCategory";
				//newProduct.Price = 123;
				//n = new List<Product>();
				//n.Add(newProduct);

				HttpRuntime.Cache.Insert(cacheKey, n, null, System.Web.Caching.Cache.NoAbsoluteExpiration, TimeSpan.FromHours(1));
			}

			return n;
		}

		public int GetProductCountByName(string nameBeginWidth)
		{
			int n = 1;

			// debug
			
			if (nameBeginWidth == "all") n = ctx.Products.Count();
			else n = ctx.Products.Where(x => x.Name.StartsWith(nameBeginWidth)).Count();
			

			return n;
		}

		public int CheckExistID(string checkID)
		{
			int count = ctx.Products.Where(x => x.ProductID == checkID).Count();
			return count;
		}

		/// <summary>
		/// this is to get the row index of the new record, for showing the corresponding page after
		/// inserting this new record (150923)
		/// - this is a bad approach as it fetches ALL records to the server.  Sean will figure out a better way later (150923)
		/// </summary>
		public int GetRowIndexByProductID(string productID, string sortBy, string sortOrder)
		{
			int index = 0;

			List<Product> productList = GetProductListByName("all", sortBy, sortOrder, 0, int.MaxValue);

			List<string> idList = new List<string>();

			foreach (Product product in productList) idList.Add(product.ProductID);

			index = idList.IndexOf(productID);

			/*
			using (InventoryEntities ctx = new InventoryEntities())
			{
				IQueryable<Product> query = ctx.Products;
				product = query.Select((x, i) => new { Item = x, Index = i }).Where(itemWithIndex => itemWithIndex.Item.ProductID == productID).FirstOrDefault();
			}
			*/

			return index;
		}

		public Product GetProductByProductID(string productID)
		{
			Product n = ctx.Products.Where(x => x.ProductID == productID).FirstOrDefault();
			return n;
		}


		public MyError InsertProduct(Product product)
		{
			MyError n = MyError.Success;

			if (GetProductByProductID(product.ProductID) != null)
			{
				n = MyError.Duplicated;
			}
			else
			{
				ctx.Products.Add(product);
				ctx.SaveChanges();

				//// clear all related cache (151002)
				//List<string> itemsToRemove = new List<string>();
				//IDictionaryEnumerator enumerator = HttpRuntime.Cache.GetEnumerator();

				//while(enumerator.MoveNext())
				//{
				//	if(enumerator.Key.ToString().StartsWith(cachePrefix))
				//	{
				//		itemsToRemove.Add(enumerator.Key.ToString());
				//	}
				//}

				//foreach(string itemToRemove in itemsToRemove)
				//{
				//	HttpRuntime.Cache.Remove(itemToRemove);
				//}
				ClearCache();
			}

			return n;
		}

		public void UpdateProduct(Product product)
		{
			Product _product = ctx.Products.Where(x => x.ProductID == product.ProductID).First();
			_product.ProductID = product.ProductID;
			_product.Name = product.Name;
			_product.Description = product.Description;
			_product.Category = product.Category;
			_product.Price = product.Price;
			ctx.SaveChanges();
		}

		//public Product GetProductDetailsByProductID(string productID)
		//{
		//	Product n = null;
		//	IQueryable<Product> query = ctx.Products;
		//	n = query.Where(x => x.ProductID == productID).FirstOrDefault();
		//	return n;
		//}
	}
}