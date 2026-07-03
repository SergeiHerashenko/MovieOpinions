using Authorization.Domain.Common.Exceptions.DomainException;

namespace Authorization.Domain.Common.Validations
{
    public static class DomainGuard
    {
        /// <summary>
        /// Перевіряє групу полів на null. Перериває виконання та викидає виняток при першому знайденому null.
        /// </summary>
        /// <typeparam name="TContext">Тип сутності/об'єкта, для якого проводиться валідація.</typeparam>
        /// <param name="fields">Набір кортежів (Значення, Назва_Поля).</param>
        public static void AgainstNull<TContext>(params (object? Value, string FieldName)[] fields)
            where TContext : class
        {
            foreach (var (value, fieldName) in fields)
            {
                if (value is null)
                    throw DomainDataInconsistencyException.Empty<TContext>(fieldName);
            }
        }
    }
}
