using Restaurant.Services.OrderAPI.Abstractions;
using System.Security.Claims;

namespace Restaurant.Services.OrderAPI.Authentication
{
    public class UserContext : IUserContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="httpContextAccessor">Адаптер Http-context'а</param>
        public UserContext(IHttpContextAccessor httpContextAccessor)
            => _httpContextAccessor = httpContextAccessor;

        /// <inheritdoc/>
        public string? CurrentUserId => User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;
    }
}
