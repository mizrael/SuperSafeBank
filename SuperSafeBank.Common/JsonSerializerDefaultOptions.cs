namespace SuperSafeBank.Common
{
    public static class JsonSerializerDefaultOptions
    {
        public static readonly System.Text.Json.JsonSerializerOptions Defaults = new() 
        {            
            PropertyNameCaseInsensitive = true,            
        };
    }
}