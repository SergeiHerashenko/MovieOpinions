# Зроблено сьогодні (25 травня 2026)

## Основні задачі
- [x] Ініціалізовано новий мікросервіс **Authorization**

- [x] Створено структуру проекту відповідно до Clean Architecture
    - `Authorization.Domain`
    - `Authorization.Application`
    - `Authorization.Infrastructure`
    - `Authorization.Api`

- [x] Реалізовано базову доменну модель (Domain Foundation Layer)
  - Створено базовий клас `Entity<TId>` з підтримкою:
    - унікального ідентифікатора
    - рівності сутностей через Id
    - системи доменних подій (`Domain Events`)
    - механізму додавання та очищення подій

  - Створено `AggregateRoot<TId, TIdType>` як базу для агрегатів
    - підтримка strongly-typed ID через `AggregateRootId<T>`
    - підтримка EF Core-friendly конструкції (parameterless constructor)

  - Реалізовано `ValueObject`
    - порівняння через набір компонентів (`GetEqualityComponents`)
    - перевизначення `Equals` та `GetHashCode`
    - підтримка семантичної рівності об’єктів

  - Створено `AggregateRootId<T>`
    - базовий тип для типізованих ідентифікаторів агрегатів
    - реалізація через Value Object підхід

  - Додано інтерфейси доменних подій:
    - `IDomainEvent`
    - `IHasDomainEvents`
    - базова інфраструктура для domain events (підготовка до eventual consistency / integration events)