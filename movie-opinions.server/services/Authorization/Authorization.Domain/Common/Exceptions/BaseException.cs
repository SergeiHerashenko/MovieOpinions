using Authorization.Domain.Common.Exceptions.Enums;

namespace Authorization.Domain.Common.Exceptions
{
    /// <summary>
    /// Базовий клас структурованих внутрішніх винятків.
    /// Зберігає діагностичний код, категорію та контекст винятку.
    ///
    /// (Base class for structured internal exceptions.
    /// Stores the diagnostic code, category, and exception context.)
    /// </summary>
    public abstract class BaseException : Exception
    {
        /// <summary>
        /// Стабільний діагностичний код виду винятку.
        /// 
        /// (Stable diagnostic code identifying the exception kind.)
        /// </summary>
        public string ExceptionCode { get; }

        /// <summary>
        /// Категорія винятку, що визначає, як її обробляти. 
        /// 
        /// (The category of the exception, which determines how it should be handled.)
        /// </summary>
        public ExceptionType ExceptionType { get; }

        /// <summary>
        /// Додаткові дані про стан системи під час виникнення винятку для полегшення дебагу. 
        /// 
        /// (Additional data about the system state when an exception occurs to facilitate debugging.)
        /// </summary>
        public IReadOnlyDictionary<string, object> Context { get; }

        /// <summary>
        /// Конструктор для ініціалізації базового винятку.
        /// 
        /// (Constructor for initializing the base selection.)
        /// </summary>
        /// <param name="exceptionCode">Код винятку. (Error code.)</param>
        /// <param name="exceptionType">Категорія винятку. (Exception category.)</param>
        /// <param name="message">Опис винятку для розробника або логів. (A description of the error for developers or logs.)</param>
        /// <param name="context">Словник з додатковими даними (опціонально). (A dictionary with additional data (optional).)</param>
        /// <param name="innerException">Оригінальний виняток, що спричинив цей виняток (опціонально). (The original exception that caused this exception (optional).)</param>
        protected BaseException(
            string exceptionCode,
            ExceptionType exceptionType,
            string message,
            IReadOnlyDictionary<string, object>? context = null,
            Exception? innerException = null)
            : base(message, innerException)
        {
            ExceptionCode = exceptionCode;
            ExceptionType = exceptionType;
            Context = context ?? new Dictionary<string, object>();
        }
    }
}
