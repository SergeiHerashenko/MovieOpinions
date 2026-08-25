# Зроблено сьогодні (25 серпня 2026)

### Спільний flow підтвердження дій користувача

- [x] Винесено спільну логіку надсилання підтвердження pending action у `ISendUserActionConfirmationService` та `SendUserActionConfirmationService`.
- [x] Винесено спільну логіку перевірки коду або URL у `IVerifyUserActionConfirmationService` та `VerifyUserActionConfirmationService`.
- [x] Реалізовано generic-обробку дій через `SendAsync<TAction>` і `VerifyAsync<TAction>`, де `TAction : UserAction`.
- [x] Додано `IUserPendingActionRetriever` та `UserPendingActionRetriever` для спільного завантаження і перевірки pending action за `UserId`, `ConfirmationToken`, типом дії та строком життя.
- [x] Додано моделі `SendActionConfirmationData`, `SendActionConfirmationResult`, `VerifiedUserActionData` і `VerifiedUserActionResult`.
- [x] Реалізовано `IUserActionConfirmationMapping` та `UserActionConfirmationMapping`.
- [x] Налаштовано відповідність між `PasswordChangeAction`, `LoginChangeAction`, `DeleteAccountAction` та їхніми `RateLimitAction`, `NotificationType` і `VerificationType`.
- [x] `SendDeletionConfirmationHandler` перетворено на вузький handler, який делегує спільний flow сервісу через `DeleteAccountAction`.
- [x] `ConfirmationDeletingHandler` використовує спільну верифікацію, після чого виконує тільки транзакційне завершення видалення.

### Завершення pending action у домені

- [x] Узагальнено отримання дії для підтвердження через generic-перевірку конкретного типу `UserAction`.
- [x] До завершення pending action додано перевірку точного `UserPendingActionId`, отриманого після верифікації.
- [x] `CompleteAction<TAction>` переведено з `Action<TAction>` на `Func<TAction, Result>`.
- [x] Помилка застосування конкретної дії тепер повертається з домену й зупиняє завершення pending action.
- [x] Pending action очищається та переводиться в завершений стан тільки після успішного виконання конкретної доменної операції.
- [x] `AggregateChanges` і `UserActionEvent` створюються після успішного застосування дії.

### Поділ агрегату `User` на partial-файли

- [x] Великий файл агрегату `User` фізично розділено через `partial class` без зміни меж агрегату.
- [x] Логіку підтвердження дій винесено в `User.ActionConfirmation.cs`.
- [x] Логіку автентифікації винесено в `User.Authentication.cs`.
- [x] Логіку видалення винесено в `User.Deletion.cs`.
- [x] Логіку pending actions винесено в `User.PendingActions.cs`.
- [x] Логіку refresh-токенів винесено в `User.RefreshTokens.cs`.
- [x] Логіку обмежень винесено в `User.Restrictions.cs`.
- [x] У `User.cs` залишено основний стан агрегату, створення, відновлення та загальні guards.

### Спільна перевірка пароля користувача

- [x] Додано `IUserPasswordAttemptService` та `UserPasswordAttemptService`.
- [x] Реалізовано завантаження користувача для перевірки пароля як за `UserId`, так і за `Login`.
- [x] Спільне ядро сервісу отримує lazy loader через `Func<CancellationToken, Task<User?>>`.
- [x] `PlainPassword` створюється та перевіряється до звернення до репозиторію.
- [x] Для відсутнього користувача виконується `FakeVerifyPassword`, після чого повертається внутрішня помилка `NotFound`.
- [x] Правильний пароль повертає завантаженого користувача через `Result<User>`.
- [x] При неправильному паролі користувач повторно завантажується з `FOR UPDATE`.
- [x] Невдала спроба записується доменним методом `RecordFailedPasswordAttempt` на актуальному заблокованому агрегаті.
- [x] Зміни невдалої спроби та `AggregateChanges` зберігаються в одній транзакції.
- [x] Domain events після невдалої спроби обробляються після commit.
- [x] Після успішного збереження невдалої спроби сервіс повертає `InvalidPassword`, а не помилковий `Success`.

### Захист конкурентності під час входу

- [x] `SignInFlowCoordinator` переведено на спільний `IUserPasswordAttemptService`.
- [x] Після успішної перевірки пароля зберігається точний підтверджений password hash.
- [x] Перед створенням сесії користувач повторно завантажується через `GetUserByIdForUpdateAsync`.
- [x] Під блокуванням password hash порівнюється з раніше підтвердженим значенням через `StringComparison.Ordinal`.
- [x] При паралельній зміні облікових даних повертається `CredentialsChanged`.
- [x] Перевірка актуального доступу, `LoginSuccess`, створення токенів, оновлення користувача та створення refresh-токена виконуються на актуальному агрегаті в одній транзакції.
- [x] Додано `SignInTransactionResult` для повернення користувача й токенів із транзакційної частини flow.
- [x] Domain events входу обробляються після успішного commit.

### Спільне отримання активних контактів

- [x] Додано `IActiveContactsProvider` та `ActiveContactsProvider`.
- [x] Додано внутрішню application-модель `ActiveContactChannel`.
- [x] Provider формує `ActiveChannelsRequest`, викликає Contact Service та перетворює інтеграційні DTO у внутрішню модель.
- [x] Технічні помилки Contact Service передаються через `Result` без прив’язки provider до конкретного use case.
- [x] Порожній список активних підтверджених контактів обробляється як порушення міжсервісного інваріанта та логується як критична помилка.

### Компенсація pending action при помилці контактів

- [x] Додано `IPendingActionContactsCoordinator` та `PendingActionContactsCoordinator` у спільний flow `UserActionConfirmation`.
- [x] Coordinator отримує контакти через універсальний `IActiveContactsProvider`.
- [x] Якщо контакти отримати неможливо, coordinator запускає компенсацію вже створеної pending action.
- [x] Для компенсації користувач повторно завантажується з `FOR UPDATE` у новій транзакції.
- [x] Доменний метод `FailPendingAction` викликається з точним `UserPendingActionId`.
- [x] Перехід pending action у `Failed` зберігається через `AggregateChanges`.
- [x] Невдала компенсація логується як критична помилка з `UserId` і `ActionId`.
- [x] Спільний coordinator підключено до `StartDeletingUser` та `StartChangePassword`.

### Рефакторинг початку видалення користувача

- [x] Власну дубльовану перевірку поточного пароля в `StartDeletingUser` замінено на `IUserPasswordAttemptService`.
- [x] Після перевірки пароля зберігається підтверджений password hash.
- [x] У транзакції створення deletion action користувач повторно завантажується з `FOR UPDATE`.
- [x] Перед створенням action перевіряється, що password hash не змінився після первинної перевірки.
- [x] Дубльоване отримання контактів і компенсацію прибрано з handler та замінено викликом `IPendingActionContactsCoordinator`.

### Початок зміни пароля

- [x] Реалізовано use case `StartChangePassword`.
- [x] Додано перевірку IP-адреси, `UserId` та rate limiting для початку зміни пароля.
- [x] Поточний пароль перевіряється через `IUserPasswordAttemptService`.
- [x] Новий пароль створюється як `PlainPassword`, хешується через `IPasswordHasher` і перетворюється на доменний `Password`.
- [x] Перед створенням password change action користувач повторно завантажується з `FOR UPDATE`.
- [x] Під блокуванням перевіряється незмінність раніше підтвердженого password hash.
- [x] Створення `PasswordChangeAction` виконується доменним методом `ActionChangePassword`.
- [x] `AggregateChanges` password change action зберігаються всередині транзакції, а domain events обробляються після commit.
- [x] Після створення action контакти отримуються через `IPendingActionContactsCoordinator`.
- [x] Use case повертає `ConfirmationToken` і доступні активні контактні канали.

### Доменна перевірка повторного пароля

- [x] У `Password` додано внутрішній метод `Matches`, який повертає `Result<bool>`.
- [x] Конкретний алгоритм порівняння передається в Domain через `Func<PlainPassword, string, bool>`.
- [x] Application передає `_passwordHasher.VerifyPassword` як method group без залежності Domain від BCrypt або Application.
- [x] Рішення про заборону повторного пароля та помилка `NoUpdateNeeded` залишаються в агрегаті `User`.
- [x] До `ActionChangePassword` додано `ExpirePendingActionIfNeeded`, тому прострочена action не блокує нову зміну пароля.
- [x] `null` callback обробляється через `DomainInvalidOperationException.NullCallback<User>` з операцією `OperationType.Compare`.
- [x] У доменний exception додано структурований контекст із шаром, власником, операцією та назвою callback.