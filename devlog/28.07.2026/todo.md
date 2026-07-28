# MovieOpinions Authorization Service

> Поточний стан реалізації Domain Layer після повного рефакторингу.

---

# ✅ Domain Layer

## Загальна архітектура

Повністю проведено рефакторинг доменного шару відповідно до принципів Domain-Driven Design.

Основною метою було побудувати домен, який повністю ізольований від Infrastructure та Application і містить виключно бізнес-логіку.

Поточна архітектура базується на наступних принципах:

- Domain не має залежностей від інших шарів.
- Уся бізнес-логіка знаходиться лише у Domain.
- Всі бізнес-інваріанти захищені на рівні домену.
- Всі зміни проходять через Aggregate Root.
- Домен не містить технічної логіки (ORM, HTTP, JWT, Database тощо).

---

# ✅ Aggregate Root

## User

Повністю переписано агрегат `User`.

User став єдиною точкою входу для будь-яких змін стану користувача.

Будь-які модифікації виконуються лише через доменні методи агрегату.

Всередині агрегату реалізовано керування:

- Pending Registration
- Pending Change
- Refresh Tokens
- Restrictions
- Restriction Sessions
- Deletion
- Access Validation

Внутрішній стан агрегату максимально інкапсульований.

---

# ✅ Domain Entities

Логіка агрегату розділена між окремими Entity.

Реалізовано:

- UserPendingRegistration
- UserPendingChange
- UserRefreshToken
- UserRestriction
- UserRestrictionSession

Кожна Entity відповідає лише за власну поведінку та власні бізнес-інваріанти.

Завдяки цьому вдалося значно зменшити розмір Aggregate Root та покращити читабельність доменної моделі.

---

# ✅ Value Objects

Практично всі примітивні типи були замінені на Value Objects.

Реалізовано окремі Value Objects для:

## Login

- Login
- Email
- Phone
- CountryCode

## Password

- PlainPassword
- PasswordHash

## Restriction

- RestrictionRule

## Device

- DeviceInfo

## Network

- IpAddress

## Security

- ConfirmationToken

## Common

- Identifier

Кожен Value Object:

- самостійно виконує власну валідацію;
- гарантує коректний стан після створення;
- приховує внутрішню реалізацію;
- інкапсулює всі бізнес-правила, пов'язані зі своїм значенням.

---

# ✅ Validation System

Повністю побудовано власну систему доменної валідації.

Реалізовано:

- ValidationRule
- ValidationFailure
- ValidationPriority
- ValidationOrchestrator

Для кожного Value Object використовується власний набір Validation Rules.

Правила виконуються у визначеному порядку відповідно до пріоритетів.

Валідація максимально наближена до моделей та більше не дублюється всередині сутностей.

---

# ✅ Validation Rules

Для більшості Value Objects створено окремі Validation Rules.

Кожне правило відповідає лише за одну перевірку.

Приклади:

- Empty Rules
- Length Rules
- Format Rules
- Allowed Characters Rules
- Business Rules

Таким чином досягнуто принципу Single Responsibility навіть на рівні окремих правил валідації.

---

# ✅ Result Pattern

У Domain реалізовано власну систему Result Pattern.

Підтримуються:

- Result
- Result<T>

Очікувані бізнес-помилки більше не генерують винятки.

Замість цього використовується керований результат виконання.

---

# ✅ Error System

Повністю централізовано систему доменних помилок.

Реалізовано:

- DomainErrorCodes
- ErrorType
- Common Errors
- Entity Errors
- Validation Errors

Кожна помилка має:

- стабільний Error Code;
- власний ErrorType;
- можливість створення відповідного Domain Exception.

Це забезпечує єдину модель обробки помилок у всьому домені.

---

# ✅ Domain Exceptions

Створено окрему ієрархію Domain Exceptions.

Винятки використовуються лише для критичних порушень роботи домену та внутрішньої неконсистентності.

Очікувані бізнес-сценарії працюють через Result Pattern.

---

# ✅ Domain Events

Підготовлена підтримка Domain Events.

Domain повідомляє про важливі бізнес-події без залежності від способу їх подальшої обробки.

Таким чином домен залишається повністю незалежним від Infrastructure.

---

# ✅ Aggregate Changes

Реалізовано механізм Aggregate Changes.

Aggregate Root накопичує інформацію про власні зміни.

Infrastructure може використовувати ці зміни для подальшого збереження або інтеграції без втручання Domain.

---

# ✅ Domain Contracts

Створено окремий набір Domain Contracts.

Contracts використовуються як безпечний спосіб передачі даних усередині домену без передачі Entity.

Приклад:

- RestrictionData

Це дозволяє не порушувати інкапсуляцію агрегатів.

---

# ✅ Domain Policies

Почато формування окремого шару Domain Policies.

Політики використовуються для реалізації бізнес-правил, які не належать конкретній сутності.

Це дозволяє не перевантажувати Aggregate Root сторонньою логікою.

---

# ✅ Common Infrastructure

Створено набір спільних доменних компонентів.

Реалізовано:

- AggregateRoot
- Entity
- ValueObject
- Identifier
- Result
- Validation
- Errors
- Exceptions
- Policies
- Contracts

Спільний код винесено в окремий модуль, що дозволило значно зменшити дублювання.

---

# ✅ Архітектурні рішення

Під час рефакторингу були прийняті наступні принципові рішення.

## Domain-Driven Design

Домен будується відповідно до DDD.

Уся бізнес-логіка знаходиться лише всередині Domain.

---

## Rich Domain Model

Сутності містять поведінку, а не лише дані.

Бізнес-логіка максимально наближена до моделей.

---

## Encapsulation

Практично весь стан моделей прихований.

Будь-які зміни можливі лише через доменні методи.

---

## Primitive Obsession

Більшість примітивних типів були замінені на Value Objects.

Це дозволило перенести значну частину валідації безпосередньо у моделі.

---

## Single Responsibility

Валідація розділена на окремі Rules.

Кожен Rule відповідає лише за одну перевірку.

---

## Centralized Error Handling

Усі доменні помилки централізовані.

Кожна помилка має стабільний код та єдину модель представлення.

---

## Current Progress

На даний момент Domain Layer можна вважати сформованим.

Основна архітектура побудована, бізнес-модель сформована, інваріанти захищені.

Розпочато написання Unit Tests для перевірки доменної логіки та бізнес-інваріантів.