using System;

namespace DeliveryMonitoring.Services
{
    /// <summary>
    /// Provides methods for setting, retrieving, and deleting secure cookies.
    /// </summary>
    public interface ISecureCookieService
    {
        /// <summary>
        /// Sets a secure, encrypted cookie with the specified key, value, and expiration time.
        /// </summary>
        void SetCookie(string key, string value, TimeSpan expiry);

        /// <summary>
        /// Retrieves and decrypts a cookie by key. Returns null if cookie does not exist or is invalid.
        /// </summary>
        string? GetCookie(string key);

        /// <summary>
        /// Deletes a cookie by key.
        /// </summary>
        void DeleteCookie(string key);
    }
}