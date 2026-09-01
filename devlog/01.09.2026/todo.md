# Зроблено сьогодні (01 вересня 2026)

### Локалізація помилок і формування API-відповідей

- [x] Зафіксовано три окремі шляхи формування відповіді: необроблений exception, очікуваний `Result.Failure` та успішний `Result`.
- [x] Відокремлено визначення HTTP status code від локалізації повідомлення: `ErrorStatusCodeMapper` відповідає за HTTP-код, а `ErrorMessageProvider` — за текст відповідною мовою.
- [x] Реалізовано нормалізацію culture, перелік підтримуваних мов, default culture та локалізовані fallback-повідомлення.
- [x] Каталог локалізованих повідомлень розділяється за доменними поняттями, щоб не накопичувати всі тексти в одному файлі.
- [x] FluentValidation-помилки перетворюються на власний `Error` і використовують доменні error codes замість стандартних кодів на кшталт `NotEmptyValidator`.
- [x] Внутрішній `Error.Message` зафіксовано як нелокалізований діагностичний опис, який не повертається користувачеві напряму.

### Рефакторинг Domain/Common — errors та exceptions

- [x] Остаточно розділено очікувані помилки (`Error` + `Result`) і внутрішні технічні несправності (exceptions).
- [x] `ErrorType` залишено винятково для семантичної категоризації очікуваних помилок, а для exceptions створено окремий `ExceptionType`.
- [x] `BaseException` стандартизовано через `ExceptionCode`, `ExceptionType`, внутрішнє повідомлення, діагностичний `Context` та `InnerException`.
- [x] Створено стабільний внутрішній каталог `DomainExceptionCodes` у форматі `DOMAIN.<CATEGORY>.<REASON>`; ці коди не використовуються для локалізації та не повертаються клієнту.
- [x] Завершено `DomainDataInconsistencyException` для порожніх, неформатних, непідтримуваних і позадіапазонних внутрішніх даних.
- [x] Завершено `DomainInvalidOperationException` для недоступного значення та null-callback із окремими діагностичними кодами.
- [x] Завершено `DomainInvariantViolationException` для неможливого стану агрегату або сутності.
- [x] Діагностичний контекст уніфіковано через ключі `Layer`, `Type`, `Operation`, `Field`, `Rule`, `ValueName` і `CallbackName`.
- [x] Для довільних потенційно чутливих значень у exceptions зберігається лише `ValueType`; raw value не потрапляє в стандартне повідомлення або контекст.
- [x] `OperationType` уніфіковано для операцій `Restore`, `Create`, `Update`, `Delete`, `Read` і `Compare`.
- [x] `Error` залишено immutable `sealed class`, додано перевірки порожніх `Code`/`Message` та невідомого `ErrorType`.

### Рефакторинг Domain/Common — базові доменні моделі

- [x] У `ValueObject` зафіксовано структурну рівність за конкретним runtime-типом і впорядкованими equality components.
- [x] `AggregateRootId<TId>` централізує рівність strongly typed ID за скалярним `Value`, а конкретні ID самостійно перевіряють свій тип значення.
- [x] Для `AggregateRoot<TId, TIdType>` поширено `notnull`-обмеження скалярного типу strongly typed ID.
- [x] `Entity<TId>` зроблено immutable щодо `Id` і `CreatedAt`; рівність визначається конкретним типом сутності та ID.
- [x] Реєстрацію domain events і aggregate changes закрито як `protected`, а їх очищення залишено публічним для dispatcher-ів.
- [x] Уточнено семантичну різницю між `DomainEvent` як бізнес-фактом і `AggregateChange` як зафіксованою зміною стану для persistence.
- [x] Базові `DomainEvent` та `AggregateChange` зберігають явно переданий `OccurredOn` і не отримують поточний час приховано.
- [x] Контракти накопичення domain events та aggregate changes задокументовано як списки подій/змін, що очікують успішної обробки та очищення.
- [x] `DomainGuard.AgainstNull` приймає явний `OperationType`, тому однаково коректно описує порушення під час `Create`, `Restore` та інших операцій.
- [x] Для validation orchestrator зафіксовано порядок правил за `ValidationPriority`, fail-fast поведінку та відкладене створення exception для restore-сценаріїв.
- [x] Папку `Domain/Common` вважаємо завершеною; конкретні error-фабрики переноситимуться до доменного поняття-власника під час подальшого перегляду агрегатів.

### Рефакторинг UserPendingRegistration

- [x] `UserPendingRegistrationId` оформлено як strongly typed ID на основі UUID v7 із окремими шляхами генерації та відновлення.
- [x] Створено `RegistrationFlowToken` як криптографічно випадковий непрозорий ідентифікатор реєстраційного потоку на основі 64 random bytes і Base64.
- [x] `RegistrationFlowToken.Parse` повертає `Result<RegistrationFlowToken>` та перевіряє порожнє значення, encoded length, Base64-формат і фактичну кількість декодованих байтів.
- [x] Зафіксовано правило: Domain визначає валідність token value, Application інтерпретує failure як очікуваний `Result`, а Infrastructure може перетворити failure з persistence на exception.
- [x] Створено окремий `RegistrationFlowTokenErrors` поруч з агрегатом із кодами `EMPTY`, `INVALID_LENGTH` та `INVALID_FORMAT` і без потрапляння raw token у діагностичні повідомлення.
- [x] `UserPendingRegistration` зроблено `sealed`; `Login` є незмінним, а `Password`, `RegistrationFlowToken` і `ExpiresAt` змінюються лише через доменну поведінку.
- [x] `Create` приймає вже валідні VO, генерує aggregate ID і flow token, встановлює годинний lifetime та повертає агрегат без зайвого `Result`.
- [x] Null typed VO у `Create`, `Restore` і `Refresh` трактуються як порушення внутрішнього контракту та обробляються через `DomainGuard` і exception.
- [x] `Restore` не створює domain events і відхиляє стан, у якому `ExpiresAt` не пізніше `CreatedAt`, через `DomainInvariantViolationException`.
- [x] `Refresh` замінює пароль, генерує новий flow token, поновлює expiration і не дозволяє виконати операцію раніше `CreatedAt`.
- [x] Перевірка завершення lifetime використовує включну межу `now >= ExpiresAt`.
- [x] Подію перейменовано на `UserRegistrationRequestedEvent`, оскільки вона описує бізнес-факт нового запиту реєстрації як під час `Create`, так і під час `Refresh`.
- [x] `UserRegistrationRequestedEvent` зроблено `sealed`, її payload уточнено через `PendingRegistrationId`, `Login` і `OccurredOn`, а конструктор обмежено рівнем Domain.
- [x] Публічні контракти aggregate, strongly typed ID, flow token та domain event задокументовано без перевантаження приватної реалізації коментарями.