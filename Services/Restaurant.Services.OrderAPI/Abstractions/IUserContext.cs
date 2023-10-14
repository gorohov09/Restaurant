namespace Restaurant.Services.OrderAPI.Abstractions
{
    /// <summary>
	/// Контекст текущего пользователя
	/// </summary>
	public interface IUserContext
    {
        /// <summary>
        /// ИД текущего пользователя
        /// </summary>
        string? CurrentUserId { get; }
    }
}
