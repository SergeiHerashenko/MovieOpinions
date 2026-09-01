using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;

namespace Authorization.Domain.Common.Guard
{
    /// <summary>
    /// Містить захисні перевірки внутрішньої консистентності доменних даних.
    /// 
    /// (Provides guard checks for internal domain-data consistency.)
    /// </summary>
    public static class DomainGuard
    {
        /// <summary>
        /// Перевіряє обов'язкові значення доменного об'єкта
        /// та зупиняється на першому виявленому null.
        ///
        /// (Checks required values of a domain object
        /// and stops at the first null value.)
        /// </summary>
        /// <typeparam name="TContext">Тип доменного об'єкта, для якого виконується перевірка.</typeparam>
        /// <param name="operationType">Операція, під час якої виконується перевірка.</param>
        /// <param name="fields">Набір пар, що містять значення та назву відповідного поля.</param>
        /// <exception cref="DomainDataInconsistencyException">Виникає, якщо обов'язкове значення дорівнює null.</exception>
        public static void AgainstNull<TContext>(
            OperationType operationType,
            params (object? Value, string FieldName)[] fields)
            where TContext : class
        {
            foreach (var (value, fieldName) in fields)
            {
                if (value is null)
                    throw DomainDataInconsistencyException.Empty<TContext>(
                        fieldName,
                        operationType
                    );
            }
        }
    }
}
