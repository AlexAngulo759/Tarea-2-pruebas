namespace Proyecto_Grafos.Services.Validation
{
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string Message { get; set; }

        public ValidationResult(bool isValid, string message = "")
        {
            IsValid = isValid;
            Message = message;
        }

        public static ValidationResult Valid() => new ValidationResult(true);
        public static ValidationResult Invalid(string message) => new ValidationResult(false, message);
    }
}