using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Models;
using Authorization.Domain.Results;
using Authorization.Domain.Users.Entities.UsersRestriction.ValueObjects.Restriction.Validation;
using Authorization.Domain.Common.Exceptions.DomainException;

namespace Authorization.Domain.Users.Entities.UsersRestriction.ValueObjects.Restriction
{
    /// <summary>
    /// Представляє правило обмеження користувача,
    /// визначене назвою та тривалістю в хвилинах.
    ///
    /// (Represents a user restriction rule
    /// defined by its name and duration in minutes.)
    /// </summary>
    public sealed class RestrictionRule : ValueObject
    {
        /// <summary>
        /// Назва правила обмеження.
        ///
        /// (Name of the restriction rule.)
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Тривалість обмеження у хвилинах.
        ///
        /// (Duration of the restriction in minutes.)
        /// </summary>
        public int DurationMinutes { get; }

        private RestrictionRule(string name, int durationMinutes)
        {
            Name = name;
            DurationMinutes = durationMinutes;
        }

        #region Creation
        /// <summary>
        /// Перевіряє вхідні дані та створює правило обмеження.
        ///
        /// (Validates the input and creates a restriction rule.)
        /// </summary>
        /// <param name="name">Назва правила обмеження.</param>
        /// <param name="durationMinutes">Тривалість обмеження у хвилинах.</param>
        /// <returns>
        /// Успішний результат із правилом або помилка його валідації.
        /// </returns>
        internal static Result<RestrictionRule> Create(string name, int durationMinutes)
        {
            var failure = RestrictionRuleValidator.ValidateForError(
                name,
                durationMinutes
            );

            if (failure is not null)
                return Result<RestrictionRule>.Failure(failure.Value);

            return Result<RestrictionRule>.Success(
                new RestrictionRule(
                    name,
                    durationMinutes
                )
            );
        }
        #endregion

        #region Restoration
        /// <summary>
        /// Відновлює правило обмеження зі збережених значень.
        ///
        /// (Restores a restriction rule from persisted values.)
        /// </summary>
        /// <param name="name">Збережена назва правила.</param>
        /// <param name="durationMinutes">Збережена тривалість у хвилинах.</param>
        /// <returns>Відновлене правило обмеження.</returns>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо збережені значення порушують правила RestrictionRule.
        /// </exception>
        public static RestrictionRule Restore(string name, int durationMinutes)
        {
            var failure = RestrictionRuleValidator.ValidateForException(
                name,
                durationMinutes,
                OperationType.Restore
            );

            if (failure is not null)
                throw failure.Value;

            return new RestrictionRule(
                name,
                durationMinutes
            );
        }
        #endregion

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return Name;
            yield return DurationMinutes;
        }
    }
}
