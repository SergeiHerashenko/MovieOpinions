# Зроблено сьогодні (22 червня 2026)

## Domain: Restriction System

- Додано `RestrictionType`
  - Ban
  - CommentMute
  - ChatMute
  - AllMute

- Реалізовано агрегат `UserRestriction`
  - створення обмежень
  - відновлення з БД через `Restore`
  - перевірка консистентності стану
  - скасування обмеження
  - розрахунок дати завершення
  - визначення активності обмеження
  - розрахунок залишкового часу

- Реалізовано агрегат `UserRestrictionSession`
  - зберігання активних обмежень користувача
  - підрахунок загальної кількості днів блокування
  - відновлення з БД
  - оновлення стану на основі актуальних обмежень

## Domain: Value Objects

- Додано `RestrictionRule`
  - перевірка назви правила
  - перевірка тривалості обмеження
  - підтримка Value Object equality

- Додано `UserRestrictionSessionId`
  - генерація нового ідентифікатора
  - відновлення з БД
  - перевірка порожнього Guid

## Domain: Exceptions

- Повністю перероблено `DomainDataInconsistencyException`
  - додано типізацію через `TEntity`
  - додано централізоване формування контексту помилки
  - реалізовано типи:
    - `EmptyOnRestore`
    - `InvalidValue`
    - `UnsupportedDiscriminator`
    - `ConsistencyViolation`
  - додано автоматичне формування повідомлень та службового контексту

## Domain: Infrastructure

- Додано `OperationType`
  - Restore
  - Create
  - Update
  - Delete

## Domain: Error Handling

- Розширено `ErrorCodes.DataInconsistencyError`
  - додано код `INVALID_VALUE`

- Додано `RestrictionError`
  - Empty
  - NotEnoughDays
  - WrongTime

## Restore Validation

- Додано перевірки консистентності при відновленні агрегатів
- Додано перевірки:
  - null-значень
  - невалідних значень
  - неузгодженого стану агрегатів
- Винесено перевірки в окремі guard-методи