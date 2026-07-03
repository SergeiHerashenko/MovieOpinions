namespace Authorization.Domain.Common.Errors
{
    public enum ErrorType
    {
        MissingData = 0,            // Дані відсутні

        InvalidFormat = 1,          // Кривий формат

        UnsupportedType = 2,        // Невідомий дискримінатор/тип

        OutOfRange = 3,             // Значення поза межами

        InvariantViolation = 4,     // Порушення логіки/стану

        InvalidOperation = 5,       // Невалідна операція

        Validation = 6,             // Некоректні вхідні дані

        NotFound = 7,               // Об'єкт не знайдено

        Conflict = 8,               // Бізнес-конфлікт

        PolicyViolation = 9,        // Порушення безпеки/прав

        EmptyValue = 10,            // Пусте значення

        Forbidden = 11,             // Заборона
    }
}
