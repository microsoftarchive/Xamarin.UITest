using System;
using System.Threading.Tasks;
using Xamarin.UITest.XDB.Entities;

namespace Xamarin.UITest.XDB.Services
{
    interface IHttpService
    {
        Task<IHttpResult<T>> DeleteAsync<T>(
            string url,
            TimeSpan? timeout = null,
            int attempts = 1,
            TimeSpan? retryInterval = null,
            bool errorIfUnavailable = true,
            bool logErrors = true);

        Task<IHttpResult<T>> GetAsync<T>(
            string url,
            TimeSpan? timeout = null,
            int attempts = 1,
            TimeSpan? retryInterval = null,
            bool errorIfUnavailable = true,
            bool logErrors = true);

        Task<IHttpResult<T>> PostAsync<T>(
            string url,
            TimeSpan? timeout = null,
            int attempts = 1,
            TimeSpan? retryInterval = null,
            bool errorIfUnavailable = true,
            bool logErrors = true);

        Task<IHttpResult<T>> PostAsJsonAsync<T>(
            string url,
            object payload,
            TimeSpan? timeout = null,
            int attempts = 1,
            TimeSpan? retryInterval = null,
            bool errorIfUnavailable = true,
            bool logErrors = true);
    }
}