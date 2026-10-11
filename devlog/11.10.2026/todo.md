# Зроблено сьогодні (11 жовтня 2026)

### Ініціація pending action на рівні User

- [x] Реалізовано публічні методи `RequestLoginChange`, `RequestPasswordChange` і `RequestAccountDeletion`.
- [x] Усі три методи повертають `Result<UserPendingAction>`, щоб Application отримувала створену дію без розкриття деталей її внутрішньої підготовки.
- [x] Конкретний public-метод перевіряє власні аргументи, формує відповідний `UserAction` і записує конкретну доменну подію.
- [x] Спільний метод `RequestPendingAction` оркеструє підготовку плану, застосування мутації, оновлення `UpdatedAt` і запис aggregate changes.
- [x] Якщо підготовка плану повертає failure або exception, стан агрегату до мутації не змінюється.
- [x] Доменні події не записуються універсальними планами: їх додають конкретні public-методи після успішного застосування pending action.

### Спільна перевірка доступу і часу

- [x] Створено `ValidateAccessForPendingActionRequest(now)` для спільної перевірки часової послідовності та `ProvideAccess(now)`.
- [x] Час операції перевіряється до використання його правилами доступу та до будь-якої мутації.
- [x] Спільний `EnsureValidOperationTimestamp(now)` винесено з `User.Authentication` у файл `User.OperationValidation.cs`.
- [x] `EnsureValidOperationTimestamp` залишається приватним: усі файли `partial class User` мають доступ до одного приватного члена після компіляції.
- [x] Загальний файл не названо `User.Helpers`, щоб не створювати невизначене сховище випадкових методів.

### RequestLoginChange

- [x] `newLogin` перевіряється як обов’язковий доменний аргумент.
- [x] Запит із логіном, який дорівнює поточному `Login`, повертає `NoUpdateNeeded`.
- [x] Метод створює `UserAction` зміни логіну, але не змінює поточний логін до підтвердження.
- [x] Після успішного створення pending action записується `UserLoginChangeRequestedEvent`.
- [x] Подія містить `UserId`, `UserPendingActionId`, поточний логін, запитаний логін, confirmation token, `ExpiresAt` і час виникнення.
- [x] Один бізнес-факт зміни логіну представлено однією доменною подією; Application зможе на її основі надіслати підтвердження на новий логін і окреме security-повідомлення на поточний.

### RequestPasswordChange

- [x] Метод приймає `PlainPassword`, новий `Password` із хешем і callback перевірки хешу.
- [x] Перевіряється, що переданий новий хеш справді відповідає переданому відкритому паролю.
- [x] Невідповідність plaintext і нового хешу трактується як порушення контракту виклику та завершується `PreconditionFailed` exception без додавання секретних значень у context.
- [x] Якщо новий відкритий пароль збігається з поточним паролем користувача, повертається `NoUpdateNeeded`.
- [x] `Password.Matches` використовує `OperationType.Compare`, оскільки метод лише порівнює значення і не виконує мутацію.
- [x] Метод не змінює поточний пароль і не додає plaintext або hash до доменної події.
- [x] Після успішного створення pending action записується `UserPasswordChangeRequestedEvent` із confirmation token та `ExpiresAt`.

### RequestAccountDeletion

- [x] Доступ користувача перевіряється до створення pending action.
- [x] Необов’язкова причина проходить через `DeletionReason.Create`.
- [x] Метод створює pending action видалення, але не видаляє користувача до підтвердження.
- [x] Після успішного створення pending action записується `UserAccountDeletionRequestedEvent` із confirmation token та `ExpiresAt`.
- [x] Причину видалення не включено до доменної події, оскільки вона не потрібна для confirmation-повідомлення.

### Плани створення та заміни pending action

- [x] `PreparePendingActionRequestPlan` лише готує повністю допустимий план і не змінює стан агрегату.
- [x] Якщо `_action is null`, створюється `CreatePendingActionPlan`.
- [x] Якщо поточна action ще активна у вказаний момент, повертається `UserErrors.ActionAlreadyExists`.
- [x] Якщо `IsActive(now)` повертає `false`, викликається `ValidateCanExpire(now)`: допустимим replacement-сценарієм є лише активна за статусом дія, строк якої завершився.
- [x] Недопустимий status, невизначений enum або некоректний час завершуються exception до створення плану мутації.
- [x] `CreatePendingActionPlan.Apply` установлює нову action через `SetPendingAction`.
- [x] `CreatePendingActionPlan.RecordAggregateChanges` записує `UserPendingActionCreated`.
- [x] `ReplaceExpiredPendingActionPlan.Apply` спочатку переводить попередню дію у `Expired`, а потім установлює нову action.
- [x] Replacement-план зберігає окреме посилання на попередню action, тому пряме присвоєння `_action` не втрачає дані, необхідні для persistence.
- [x] `ReplaceExpiredPendingActionPlan.RecordAggregateChanges` спочатку записує `UserPendingActionUpdated` для старої дії, а потім `UserPendingActionCreated` для нової.
- [x] Назву `RecordEventsAndChanges` замінено на точну `RecordAggregateChanges`, оскільки плани більше не записують domain events.

### Межа збереження стану

- [x] `UpdatedAt` оновлюється в агрегаті після успішного застосування плану.
- [x] Корінь `User`, включно з `UpdatedAt`, надалі зберігається Application через `UpdateUserAsync`.
- [x] Aggregate changes описують persistence дочірніх `UserPendingAction`; окрема штучна зміна `UserUpdated` для цього сценарію не потрібна.

### Неймінг, структура і документація

- [x] Методи у файлі впорядковано зверху вниз: public behavior -> спільна обробка -> підготовка плану -> типи планів.
- [x] Код поділено на регіони `RequestLoginChange`, `RequestPasswordChange`, `RequestAccountDeletion`, `Pending Action Request Processing` і `Pending Action Request Plans`.
- [x] Додано XML-коментарі до трьох public-методів, private processing-методів, базового плану, конкретних планів, їхніх властивостей, конструкторів і переходів.