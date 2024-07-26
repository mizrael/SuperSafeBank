using System.Text.Json;

namespace SuperSafeBank.Common
{
    public static class JsonSerializerDefaultOptions
    {
        public static readonly JsonSerializerOptions Defaults = new() 
        {            
            PropertyNameCaseInsensitive = true,            
        };
    }
}