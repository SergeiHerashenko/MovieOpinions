namespace Authorization.Domain.Common.Errors.Enums
{
    public enum ErrorType
    {
        InvalidFormat = 0,          // Кривий формат

        UnsupportedType = 1,        // Невідомий дискримінатор/тип

        OutOfRange = 2,             // Значення поза межами

        InvariantViolation = 3,     // Порушення логіки/стану

        InvalidOperation = 4,       // Невалідна операція

        Validation = 5,             // Некоректні вхідні дані

        NotFound = 6,               // Об'єкт не знайдено

        Conflict = 7,               // Бізнес-конфлікт

        PolicyViolation = 8,        // Порушення безпеки/прав

        EmptyValue = 9,            // Пусте значення

        Forbidden = 10,             // Заборона
    }
}
