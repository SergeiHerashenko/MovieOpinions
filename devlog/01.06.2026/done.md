# Зроблено сьогодні (1 червня 2026)

## Основні задачі

- [x] Розширено модель помилок доменного шару
    - Додано нові типи помилок у `ErrorType`:
        -`Forbidden`
        -`Conflict`

- [x] Уніфіковано та скориговано коди помилок (`ErrorCodes`)
    - Email:
        - `Empty` → `EmptyEmail`
    - Password:
        - `Empty` → `EmptyPassword`
    - User:
        - додано `Empty`
        - додано `OperationIsNotAllowed`
    - AccountStatus:
        - додано `AccountBlocked`
        - додано `AccountDeleted`
    - Change:
        - додано `UpdateNotRequired`

- [x] Приведено доменні помилки до консистентного вигляду
    - стандартизовано фабрики помилок:
        - `EmailError`
        - `PasswordError`
        - `UserError`
    - розділено типи помилок:
        - `EmptyValue`
        - `Validation`
        - `BusinessRuleViolation`
        - `Conflict`
        - `Forbidden`

---

## Рефакторинг базових доменних моделей

- [x] `Entity<TId>`
    - додано `CreatedAt`
    - додано конструктор створення (`UtcNow`)
    - додано конструктор відновлення з `createdAt`

- [x] `AggregateRoot<TId, TIdType>`
    - прибрано parameterless constructor
    - додано явний restore-конструктор

- [x] Додано `DomainEvent`
    - поле `OccurredOn`

---

## Value Objects

- [x] `Password`
    - додано `Restore`
    - додано перевірку на пусте значення при відновленні

- [x] `UserId`
    - додано перевірку `Guid.Empty`
    - перейменовано `Create` → `Restore`
    - залишено `CreateUnique`

---

## Aggregate: User

- [x] Оновлено створення `User`
    - приватний конструктор
    - фабрика `Create`
    - доменний event `UserRegisteredEvent`

- [x] Реалізовано restore-логіку
    - `User.Restore(...)`
    - перевірки:
        - `UserId`
        - `Login`
        - `Password`
        - `Role`

- [x] Реалізовано поведінкові методи
    - `ChangeLogin`
    - `ChangePassword`
    - `ConfirmLogin`
    - `Block`
    - `RemoveBlock`
    - `RecordFailedLoginAttempt`
    - `LoginSuccess`
    - `Delete`
    - `Undelete`

- [x] Додано guard
    - `ProvideAccess()`
    - блокування операцій для:
        - `IsBlocked`
        - `IsDeleted`

- [x] Додано доменні події
    - `UserRegisteredEvent`
    - `UserLoginChangedEvent`
    - `UserPasswordChangedEvent`
    - `UserBlockedEvent`
    - `UserRemoveBlockedEvent`
    - `UserLoggedInEvent`
    - `UserDeletedEvent`
    - `UserUndeleteEvent`

---

## Aggregate: UserDeletion (WIP)

- [x] Створено `UserDeletion`
- [x] Створено `UserDeletionId`