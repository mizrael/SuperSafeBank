using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Xunit;

namespace SuperSafeBank.Web.API.Tests
{
    public static class TestUtils
    {
        public static async Task Retry(Func<Task<bool>> predicate, string because, int maxRetries = 10)
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
                catch(Exception ex)
                {
                    Debug.WriteLine(ex);
                }

                await Task.Delay((int) Math.Pow(2, curr) * 250);
            }

            if (!found)
                Assert.False(true, because);
        }
    }
}