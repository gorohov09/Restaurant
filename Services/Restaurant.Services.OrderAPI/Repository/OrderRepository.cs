using Microsoft.EntityFrameworkCore;
using Restaurant.Services.OrderAPI.DbContexts;
using Restaurant.Services.OrderAPI.Models;

namespace Restaurant.Services.OrderAPI.Repository
{
    public class OrderRepository : IOrderRepository
    {
        private readonly DbContextOptions<ApplicationDbContext> _dbContext;

        public OrderRepository(DbContextOptions<ApplicationDbContext> dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> AddOrder(OrderHeader orderHeader)
        {
            await using var _db = new ApplicationDbContext(_dbContext);
            _db.OrderHeaders.Add(orderHeader);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<List<OrderHeader>> GetAllOrdersByUser(string userId)
        {
            await using var _db = new ApplicationDbContext(_dbContext);
            return await _db.OrderHeaders
                .Include(o => o.OrderDetails)
                .Where(o => o.UserId == userId)
                .ToListAsync();
        }

        public async Task UpdateOrderPaymentStatus(int orderHeaderId, bool paid)
        {
            await using var _db = new ApplicationDbContext(_dbContext);
            var orderHeader = await _db.OrderHeaders.FirstOrDefaultAsync(o => o.OrderHeaderId == orderHeaderId);
            if (orderHeader != null)
                orderHeader.PaymentStatus = paid;

            await _db.SaveChangesAsync();
        }
    }
}
