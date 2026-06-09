# Зроблено сьогодні (26 травня 2026)

## Domain: Error handling та Result-патерн

- [x] Реалізовано централізовану модель помилок у домені
  - `Error` як базова доменна модель помилки
  - `ErrorType` як класифікація типів доменних помилок
  - `ErrorCodes` як централізований набір кодів помилок по контекстах
  - `EmailError`, `PhoneError`, `PasswordError` як доменно-специфічні помилки

- [x] Введено систему доменних виключень
  - `BaseDomainException` як базовий контракт
  - `DomainInvalidInputException` для порушення інваріантів
  - `DomainInvalidOperationException` для некоректного доступу до стану об’єкта
  - `DomainDataInconsistencyException` для помилок відновлення з persistence шару

- [x] Реалізовано `Result` та `Result<T>`
  - інкапсуляція success/failure станів
  - гарантія коректних інваріантів Result
  - обмеження доступу до `Value` при failure стані
  - явне повернення помилок через `Error`

---

## Domain: Email Value Object

- [x] Реалізовано `Email` як Value Object
  - валідація формату через regex
  - перевірка довжини
  - нормалізація (lowercase)
- [x] Реалізовано `Create` з `Result<Email>`
  - повернення помилок через доменний `Error`
- [x] Реалізовано `Restore`
  - відновлення з persistence без Result (exception-based підхід)

---

## Domain: Phone Value Object (composite)

- [x] Реалізовано `Phone` як Value Object
  - композиція `CountryCode + Number`
  - очищення номера до цифр
  - перевірка довжини та формату
- [x] Реалізовано `CountryCode` як Value Object
  - перевірка формату (+ prefix, цифри, довжина)
- [x] Реалізовано `Create` з `Result<Phone>`
- [x] Реалізовано `Restore`
  - відновлення через exception при неконсистентних даних
- [x] Додано `GetFullNumber()` як доменне представлення

---

## Domain: Login (polymorphic Value Object)

- [x] Реалізовано `Login` як поліморфний Value Object
  - уніфікація Email та Phone під один тип
  - контракт: `Value` + `Type`
- [x] Реалізовано `EmailLogin`
  - обгортка над `Email` Value Object
- [x] Реалізовано `PhoneLogin`
  - обгортка над `Phone` Value Object
- [x] Реалізовано фабричні методи `Login.From`
- [x] Реалізовано `Login.Restore`
  - dispatch по `LoginType`
  - делегування до конкретних реалізацій
  - обробка unsupported discriminator через exception

---

## Domain: Password Value Object

- [x] Реалізовано `Password` як Value Object
  - зберігання тільки хешу пароля
- [x] Реалізовано `Create` з `Result<Password>`
  - перевірка на пусте значення

---

## Domain: Strongly Typed Identifiers

- [x] Реалізовано `UserId` як strongly-typed identifier
  - обгортка над `Guid`
  - створення та відновлення через фабричні методи
  - implicit conversion до `Guid`

---

## Domain: Aggregate Root infrastructure

- [x] Реалізовано базові абстракції доменної моделі
  - `Entity<TId>`
  - `AggregateRoot<TId, TIdType>`
- [x] Додано підтримку domain events
- [x] Реалізовано equality через Id (identity-based equality)

---

## Domain: User Aggregate (in progress)

- [x] Створено `User` як Aggregate Root
  - використання `UserId` як ідентифікатора
- [x] Додано базову структуру:
  - `Login`
  - `Password`
  - `Role`
  - метадані безпеки:
    - FailedLoginAttempts
    - IsBlocked
    - IsDeleted
    - LastLoginAt / LastLoginIp
    - IsLoginConfirmed
- [ ] Поведінкова логіка агрегату ще не реалізована

---

## Архітектурні принципи

- Value Objects відповідають за:
  - інваріанти
  - валідацію
  - нормалізацію
- `Result` використовується тільки для creation flow
- `Restore` використовує exception-based підхід для persistence consistency
- `Login` реалізовано як поліморфний Value Object
- Чітке розділення:
  - creation (Result)
  - reconstruction (Restore + exceptions)