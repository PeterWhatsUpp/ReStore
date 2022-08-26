using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;

namespace API.Controllers
{
    public class BasketController : BaseApiController
    {
        private readonly StoreContext _context;

        public BasketController(StoreContext context)
        {
            this._context = context;
        }

        [HttpGet(Name ="GetBasket")]
        public async Task<ActionResult<BasketDTO>> GetBasket()
        {
            var basket = await RetrieveBasket();
            if (basket == null)
            {
                return NotFound();
            }
            return MapBasketToDto(basket);

           
        }

        [HttpPost]
        public async Task<ActionResult<BasketDTO>>
        AddItemToBasket(int productId, int quantity)
        {
            var basket = await RetrieveBasket();
            if (basket == null) basket = CreateBasket();
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return BadRequest(new ProblemDetails{Title="Product Not Found"});
            basket.AddItem (product, quantity);
            var result = await _context.SaveChangesAsync();
            if (result > 0) return CreatedAtRoute("GetBasket", MapBasketToDto(basket));
            return BadRequest(new ProblemDetails {
                Title = "Problem saving item to basket"
            });
        }

        
        [HttpDelete]
        public async Task<ActionResult> RemoveBasketItem(int productId, int quantity)
        {
            var basket=await RetrieveBasket();
            if (basket==null) return NotFound();
            basket.RemoveItem(productId, quantity);
            var result=await _context.SaveChangesAsync()>0;
            if (result)      return Ok();
            return BadRequest(new ProblemDetails{
                Title="Problem removing item from the basket"
            });
        }

        private async Task<Basket> RetrieveBasket()
        {
            return await _context
                .Baskets
                .Include(i => i.Items)
                .ThenInclude(p => p.Product)
                .FirstOrDefaultAsync(x =>
                    x.BuyerId == Request.Cookies["buyerId"]);
        }

        private Basket CreateBasket()
        {
            var buyerId = Guid.NewGuid().ToString();
            var cookieOptions =
                new CookieOptions {
                    IsEssential = true,
                    Expires = DateTime.Now.AddDays(30)
                };
            Response.Cookies.Append("buyerId", buyerId, cookieOptions);
            var basket = new Basket { BuyerId = buyerId };
            _context.Baskets.Add (basket);
            return basket;
        }

         private BasketDTO MapBasketToDto(Basket basket)
            {
                return new BasketDTO
                {
                    Id = basket.Id,
                    BuyerId = basket.BuyerId,
                    Items =
                        basket
                            .Items
                            .Select(item =>
                                new BasketItemsDto
                                {
                                    ProductId = item.ProductId,
                                    Name = item.Product.Name,
                                    Price = item.Product.Price,
                                    PictureUrl = item.Product.PictureUrl,
                                    Brand = item.Product.Brand,
                                    Type = item.Product.Type,
                                    Quantity = item.Quantity
                                })
                            .ToList()
                };
            }

    }
}
