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
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо обов'язкове значення дорівнює null.
        /// </exception>
        public static void AgainstNull<TContext>(
            OperationType operationType,
            params (object? Value, string FieldName)[] fields)
            where TContext : class
        {
            foreach (var (value, fieldName) in fields)
            {
                if (value is null)
                {
                    throw DomainDataInconsistencyException.Empty<TContext>(
                        fieldName,
                        operationType
                    );
                }   
            }
        }

        /// <summary>
        /// Перевіряє, що фактичний момент часу не передує
        /// мінімально допустимому моменту. Рівність дозволена.
        ///
        /// (Checks that the actual point in time is not earlier
        /// than the minimum allowed point in time. Equality is allowed.)
        /// </summary>
        /// <typeparam name="TContext">
        /// Тип доменного об'єкта, для якого виконується перевірка.
        /// </typeparam>
        /// <param name="operationType">
        /// Операція, під час якої виконується перевірка.
        /// </param>
        /// <param name="actual">
        /// Перевірюваний момент часу та назва відповідного поля.
        /// </param>
        /// <param name="minimum">
        /// Мінімально допустимий момент часу та назва поля, яке визначає нижню межу.
        /// </param>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо перевірюваний момент часу передує мінімально допустимому.
        /// </exception>
        public static void AgainstEarlierThan<TContext>(
            OperationType operationType,
            (DateTimeOffset Value, string FieldName) actual,
            (DateTimeOffset Value, string FieldName) minimum)
            where TContext : class
        {
            if (actual.Value >= minimum.Value)
                return;

            throw DomainDataInconsistencyException.ValueOutOfRange<TContext>(
                actual.FieldName,
                actual.Value,
                operationType,
                context: new Dictionary<string, object>
                {
                    ["MinimumField"] = minimum.FieldName,
                    ["MinimumValue"] = minimum.Value
                }
            );
        }

        /// <summary>
        /// Перевіряє, що передані значення enum оголошені
        /// у відповідних типах переліків.
        ///
        /// (Validates that the supplied enum values are declared
        /// by their corresponding enum types.)
        /// </summary>
        /// <typeparam name="TEntity">
        /// Доменний контекст, для якого виконується перевірка.
        /// </typeparam>
        /// <param name="operationType">Тип доменної операції.</param>
        /// <param name="fields">Значення enum разом із назвами відповідних полів.</param>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо значення не оголошене у відповідному enum.
        /// </exception>
        /// <remarks>
        /// Не призначений для перевірки комбінованих значень enum із атрибутом Flags.
        ///
        /// (Not intended for validating combined values of enums marked with Flags.)
        /// </remarks>
        public static void AgainstUndefinedEnum<TEntity>(
            OperationType operationType,
            params (Enum value, string name)[] fields)
            where TEntity : class
        {
            foreach (var (value, fieldName) in fields)
            {
                if (!Enum.IsDefined(value.GetType(), value))
                {
                    throw DomainDataInconsistencyException.UnsupportedDiscriminator<TEntity>(
                        fieldName,
                        value,
                        operationType
                    );
                }
            }
        }
    }
}
