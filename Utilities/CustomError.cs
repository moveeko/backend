namespace backend.Utilities
{
    [Serializable]
    public class CustomError : Exception
    {
        public string Name { get; set; }
        public new string Message { get; set; }
        public int StatusCode { get; set; }

        public CustomError(string name, int statusCode = 409, string message = "")
        {
            this.StatusCode = statusCode;
            this.Name = name;
            this.Message = message;
        }
    }
}