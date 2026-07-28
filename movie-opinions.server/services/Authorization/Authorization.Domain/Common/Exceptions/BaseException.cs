using Authorization.Domain.Common.Errors.Enums;

namespace Authorization.Domain.Common.Exceptions
{
    /// <summary>
    /// Базовий абстрактний клас для всіх специфічних винятків у домені. 
    /// (A base abstract class for all domain-specific exceptions.)
    /// Забезпечує стандартизовану структуру для передачі помилок з кодом, типом та контекстом.
    /// (Provides a standardized structure for communicating errors with code, type, and context.)
    /// </summary>
    public abstract class BaseException : Exception
    {
        /// <summary>
        /// Унікальний ідентифікатор помилки. (Unique error identifier.)
        /// </summary>
        public string ErrorCode { get; }

        /// <summary>
        /// Категорія помилки, що визначає, як її обробляти. (The category of the error, which determines how it should be handled.)
        /// </summary>
        public ErrorType ErrorType { get; }

        /// <summary>
        /// Додаткові дані про стан системи під час виникнення помилки для полегшення дебагу. (Additional data about the system state when the error occurred to facilitate debugging.)
        /// </summary>
        public IReadOnlyDictionary<string, object> Context { get; }

        /// <summary>
        /// Конструктор для ініціалізації базового винятку.
        /// </summary>
        /// <param name="errorCode">Код помилки. (Error code.)</param>
        /// <param name="errorType">Тип помилки. (Error type.)</param>
        /// <param name="message">Опис помилки для розробника або логів. (A description of the error for developers or logs.)</param>
        /// <param name="context">Словник з додатковими даними (опціонально). (A dictionary with additional data (optional).)</param>
        /// <param name="innerException">Оригінальний виняток, що спричинив цей виняток (опціонально). (The original exception that caused this exception (optional).)</param>
        protected BaseException(
            string errorCode,
            ErrorType errorType,
            string message,
            IReadOnlyDictionary<string, object>? context = null,
            Exception? innerException = null)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
            ErrorType = errorType;
            Context = context ?? new Dictionary<string, object>();
        }
    }
}
