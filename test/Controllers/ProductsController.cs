﻿using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using test.Data;
using test.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace test.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private ShopDbContext _dbContext;

        public ProductsController(ShopDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET: api/<ItemsController>
        /*[HttpGet]
        public IActionResult Get(string? sort, int? pageNumber, int? pageSize)
        {
            var currentPageSize = pageSize ?? 20;
            var currentPageNumber = pageNumber ?? 1;
            var currentSort = sort ?? "none";

            if (sort == "asc")
            {
                var ourItems = _dbContext.Products.OrderBy(i => i.Price);
                return Ok(ourItems.Skip((currentPageNumber - 1) * currentPageSize).Take(currentPageSize));

            }
            else if (sort == "des")
            {
                var ourItems = _dbContext.Products.OrderByDescending(i => i.Price);
                return Ok(ourItems.Skip((currentPageNumber - 1) * currentPageSize).Take(currentPageSize));
            }
            else
            {
                return Ok(_dbContext.Products.Skip((currentPageNumber - 1) * currentPageSize).Take(currentPageSize));
            }
        }*/

        // GET api/<ItemsController>/5
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var myItem = _dbContext.Products.Find(id);
            if (myItem == null)
            {
                return NotFound("not found");
            }
            else
            {
                return Ok(myItem);
            }

        }

        // GET api/<ItemsController>/search?title=
        [HttpGet("[action]")]
        public IActionResult Search(string title)
        {
            var products = from Product in _dbContext.Products
                         where Product.ProductTitle.StartsWith(title)
                         select new
                         {
                             Id = Product.ProductId,
                             Title = Product.ProductTitle
                         };
            return Ok(products);
        }




        // POST api/<ItemsController>
        [HttpPost]
        public IActionResult Post([FromForm] Product itemObj)
        {
            /*var guid = Guid.NewGuid();
            var filePath = Path.Combine("wwwroot/img", guid + ".jpg");
            if (itemObj.Image != null)
            {
                var fileStream = new FileStream(filePath, FileMode.Create);
                itemObj.Image.CopyTo(fileStream);
            }
            //itemObj.ImageUrl = filePath.Remove(0, 7);*/
        _dbContext.Products.Add(itemObj);
            _dbContext.SaveChanges();

            return StatusCode(201, "item created");
        }

        
        // PUT api/items/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromForm] Product itemObj)
        {
            var myProduct = _dbContext.Products.Find(id);
            if (myProduct == null)
            {
                return NotFound("not found");
            }
            else
            {
                var guid = Guid.NewGuid();
                var filePath = Path.Combine("wwwroot/img", guid + ".jpg");
                /*if (itemObj.Image != null)
                {
                    var fileStream = new FileStream(filePath, FileMode.Create);
                    itemObj.Image.CopyTo(fileStream);
                    itemObj.ImageUrl = filePath.Remove(0, 7);
                    myProduct.ImageUrl = itemObj.ImageUrl;
                }*/

                myProduct.ProductDescription = itemObj.ProductDescription;
                myProduct.ProductTitle = itemObj.ProductTitle;
                myProduct.ProductBrand = itemObj.ProductBrand;

                _dbContext.SaveChanges();
                return Ok("updated");
            }

        }

        // DELETE api/<ItemsController>/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var myItem = _dbContext.Products.Find(id);
            if (myItem == null)
            {
                return NotFound("not found");
            }
            else
            {
                _dbContext.Products.Remove(myItem);
                _dbContext.SaveChanges();
                return Ok("deleted");
            }


        }
    }
}
