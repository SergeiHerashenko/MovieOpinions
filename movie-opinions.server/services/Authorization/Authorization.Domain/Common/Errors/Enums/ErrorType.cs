namespace Authorization.Domain.Common.Errors.Enums
{
    public enum ErrorType
    {
        Validation = 0,          // Некоректні вхідні дані (формат, пусте, out of range, unsupported)

        NotFound = 1,            // Ресурс не знайдено

        Conflict = 2,            // Бізнес-конфлікт / порушення стану (already exists, concurrency тощо)

        Forbidden = 3,           // Немає прав / policy violation

        BusinessRule = 4,        // Порушення інваріанта / бізнес-правила
    }
}
