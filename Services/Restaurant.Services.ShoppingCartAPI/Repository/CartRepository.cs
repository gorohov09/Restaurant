using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Restaurant.Services.ShoppingCartAPI.DbContexts;
using Restaurant.Services.ShoppingCartAPI.Models;
using Restaurant.Services.ShoppingCartAPI.Models.Dto;

namespace Restaurant.Services.ShoppingCartAPI.Repository
{
    public class CartRepository : ICartRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;

        public CartRepository(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<bool> ApplyCoupon(string userId, string couponId)
        {
            var cartFromDb = await _db.CartHeaders.FirstOrDefaultAsync(c => c.UserId == userId);

            if (cartFromDb is null)
                return false;

            cartFromDb.CouponCode = couponId;
            _db.CartHeaders.Update(cartFromDb);
            await _db.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ClearCart(string userId)
        {
            var cartHeaderFromDb = await _db.CartHeaders
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cartHeaderFromDb is not null)
            {
                var cartDetails = _db.CartDetails.Where(c => c.CartHeaderId == cartHeaderFromDb.CartHeaderId);
                _db.CartDetails.RemoveRange(cartDetails);
                _db.CartHeaders.Remove(cartHeaderFromDb);

                await _db.SaveChangesAsync();
                return true;
            }

            return false;
        }


        public async Task<CartDto> CreateUpdateCart(CartDto cartDto)
        {
            var cart = _mapper.Map<Cart>(cartDto);

            /* Важный комментарий
             * Так как пользователь добавляет один товар в корзину, то в объекте Cart в свойстве CartDetails будет один товар
             */

            var cartDetail = cart.CartDetails.FirstOrDefault(); //Строка корзины

            if (cartDetail is null)
                throw new ArgumentException();

            //Проверяем, существует ли добавляемый товар в нашей БД(которая в микросервисе Корзины)
            var prodInDb = await _db.Products
                .FirstOrDefaultAsync(p => p.ProductId == cartDetail.ProductId);

            if (prodInDb is null)
            {
                _db.Products.Add(cartDetail.Product);
                await _db.SaveChangesAsync();
            }

            //Проверяем существует ли уже заголовок корзины нашего пользователя
            var cartHeaderFromDb = await _db.CartHeaders.AsNoTracking()
                .FirstOrDefaultAsync(c => c.UserId == cart.CartHeader.UserId);

            if (cartHeaderFromDb is null)
            {
                _db.CartHeaders.Add(cart.CartHeader);
                await _db.SaveChangesAsync();

                cartDetail.CartHeaderId = cart.CartHeader.CartHeaderId;
                cartDetail.Product = null;
                _db.CartDetails.Add(cartDetail);
                await _db.SaveChangesAsync();
            }
            else
            {
                var cartDetailsFromDb = await _db.CartDetails.AsNoTracking()
                    .FirstOrDefaultAsync(c => c.ProductId == cartDetail.ProductId &&
                    c.CartHeaderId == cartHeaderFromDb.CartHeaderId);

                if (cartDetailsFromDb is null)
                {
                    cartDetail.CartHeaderId = cartHeaderFromDb.CartHeaderId;
                    cartDetail.Product = null;
                    _db.CartDetails.Add(cartDetail);
                    await _db.SaveChangesAsync();
                }
                else
                {
                    cartDetail.CartDetailsId = cartDetailsFromDb.CartDetailsId;
                    cartDetail.CartHeaderId= cartDetailsFromDb.CartHeaderId;
                    cartDetail.Product = null;
                    cartDetail.Count += cartDetailsFromDb.Count;
                    _db.CartDetails.Update(cartDetail);
                    await _db.SaveChangesAsync();
                }
            }

            return _mapper.Map<CartDto>(cart);

        }

        public async Task<CartDto> GetCartByUserId(string userId)
        {
            Cart cart = new Cart();

            var cartHeaderFromDb = await _db.CartHeaders
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cartHeaderFromDb is null)
            {
                cart.CartHeader = null;
                cart.CartDetails = null;
            }
            else
            {
                var cartDetails = _db.CartDetails
                    .Where(c => c.CartHeaderId == cartHeaderFromDb.CartHeaderId)
                    .Include(c => c.Product);

                cart.CartHeader = cartHeaderFromDb;
                cart.CartDetails = cartDetails;
            }

            return _mapper.Map<CartDto>(cart);
        }

        public async Task<bool> RemoveCoupon(string userId)
        {
            var cartFromDb = await _db.CartHeaders.FirstOrDefaultAsync(c => c.UserId == userId);

            if (cartFromDb is null)
                return false;

            cartFromDb.CouponCode = "";
            _db.CartHeaders.Update(cartFromDb);
            await _db.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RemoveFromCart(int cartDetailsId)
        {
            try
            {
                var cartDetails = await _db.CartDetails
                    .FirstOrDefaultAsync(c => c.CartDetailsId == cartDetailsId);

                if (cartDetails is null)
                    return false;

                var totalCountOfCartItems = _db.CartDetails
                    .Count(c => c.CartHeaderId == cartDetails.CartHeaderId);

                _db.CartDetails.Remove(cartDetails);

                if (totalCountOfCartItems == 1)
                {
                    var cartHeaderToRemove = await _db.CartHeaders
                        .FirstOrDefaultAsync(c => c.CartHeaderId == cartDetails.CartHeaderId);

                    _db.CartHeaders.Remove(cartHeaderToRemove);
                }

                await _db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }
    }
}
