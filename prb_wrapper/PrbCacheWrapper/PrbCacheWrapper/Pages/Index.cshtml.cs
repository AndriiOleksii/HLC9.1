using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Distributed;

namespace PrbCacheWrapper.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly ICacheStampedeRedis _cacheStampedeRedis;

        public IndexModel(ILogger<IndexModel> logger, 
            IDistributedCache distributedCache, ICacheStampedeRedis cacheStampedeRedis)
        {
            _logger = logger;
            _cacheStampedeRedis = cacheStampedeRedis;
        }

        public async Task OnGet()
        {
            Console.WriteLine(await _cacheStampedeRedis.Fetch(1, "Test Cache Key 1", TimeSpan.FromDays(1)));
        }
    }
}