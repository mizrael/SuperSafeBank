using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Xunit;

namespace SuperSafeBank.Web.API.Tests
{
    public static class TestUtils
    {
        private static readonly Func<int, TimeSpan> DefaultDelayFactory = (c) => TimeSpan.FromSeconds(Math.Pow(2, c));

        public static async Task Retry(Func<Task<bool>> predicate, string because = "", int maxRetries = 3, Func<int, TimeSpan> delayFactory = null)
        {
            int curr = 0;
            bool found = false;
            while (curr++ < maxRetries)
            {
                try
                {
                    if (await predicate())
                    {
                        found = true;
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }

                var delay = (delayFactory ?? DefaultDelayFactory)(curr);
                await Task.Delay(delay);
            }

            if (!found)
                Assert.False(true, because);
        }
    }
}