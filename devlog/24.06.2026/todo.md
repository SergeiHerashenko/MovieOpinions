# Зроблено сьогодні (24 червня 2026)


## 📌 Загальна архітектура

Проєкт перебудовано на CQRS підхід із використанням:
- MediatR для оркестрації команд
- FluentValidation для валідації запитів
- ApplicationResult / ApplicationResult<T> як єдиний контракт відповіді
- Domain Error model для уніфікації помилок
- Centralized exception handling на рівні API middleware

---

# ✅ API Layer

## AuthenticationController
- Додано MediatR інтеграцію через IMediator
- Реалізовано endpoint:
  - POST /register
- Побудовано Command з DTO (RegistrationRequest → RegistrationCommand)
- Додано обробку результату:
  - return BadRequest(result.Errors) при failure
  - return Ok(result.Value) при success

---

## Error Handling Middleware

## AuthExceptionHandler (IExceptionHandler)
- Реалізовано глобальний exception handler
- Додано:
  - логування критичних помилок
  - мапінг ErrorCode → HTTP StatusCode
  - локалізація повідомлень через IUserContext + IErrorMessageProvider
- Побудовано ErrorResponse контракт для API

---

## ErrorStatusCodeMapper
- Реалізовано централізований mapping:
  - Domain / Application ErrorCode → HTTP status code
- Покрито:
  - validation errors (400)
  - auth errors (401/403)
  - conflicts (409)
  - system errors (500)

---

## Response DTO
- ErrorResponse введено як контракт API
- Підтримує:
  - IsSuccess flag
  - Messages list
  - StatusCode
  - ErrorCode list

---

# ✅ Application Layer

## ApplicationResult (core abstraction)

- Реалізовано Result pattern:
  - ApplicationResult
  - ApplicationResult<T>
- Підтримка:
  - Success state
  - Failure state
  - IReadOnlyList<Error>
- Заборонені неконсистентні стани через invariants:
  - success + errors → exception
  - failure + empty errors → exception

---

## Domain Error model usage

- Використовується unified Error:
  - Code
  - Message
  - Type (Validation / Domain / System)
- Валідаційні помилки трансформуються в Domain Error

---

## ValidationBehavior (MediatR Pipeline)

- Реалізовано pipeline behavior:
  - перехоплює всі commands/queries
  - виконує FluentValidation validators
- Якщо validation fails:
  - перетворює FluentValidation → Domain Error
  - повертає ApplicationResult.Failure
- Підтримка:
  - ApplicationResult
  - ApplicationResult<T> через reflection factory

---

## CreateFailureResult Factory

- Реалізовано універсальну фабрику:
  - створення failure результату для generic/non-generic
- Використано reflection для:
  - ApplicationResult<T>.Failure(IEnumerable<Error>)

---

## Registration Feature (CQRS example)

### Command
- RegistrationCommand
- Implements IRequest<ApplicationResult<RegistrationResult>>
- містить:
  - Login
  - Password
  - ConfirmPassword
  - AcceptTerms

---

### Validator
- FluentValidation rules:
  - login required
  - password complexity
  - confirm password match
  - terms acceptance required

---

### Handler
- RegistrationHandler
- повертає ApplicationResult<RegistrationResult>
- success flow stub:
  - RegistrationNextStep.EmailConfirmation

---

## RegistrationResult DTO
- містить:
  - NextStep enum
  - Message

---

# ✅ Cross-cutting concerns

## IUserContext
- Абстракція доступу до HTTP context metadata
- використовується для:
  - language resolution
  - request context enrichment

---

## IErrorMessageProvider
- інтерфейс для локалізації помилок
- дозволяє мапити:
  - errorCode + culture → message

---

# 🧠 Архітектурні рішення

## Result Pattern
- Використовується як єдиний контракт між Application → API
- Розділення:
  - Success → Value
  - Failure → Errors

---

## Pipeline Validation Strategy
- Валідація винесена в MediatR pipeline
- Handler не містить validation logic

---

## Error Flow Strategy
1. FluentValidation → ValidationBehavior
2. ValidationBehavior → Domain Error
3. Domain Error → ApplicationResult.Failure
4. Controller → HTTP mapping
