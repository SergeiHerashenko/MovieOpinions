# Зроблено сьогодні (9 жовтня 2026)

## Виконано

### Проєктування User.Action

- [x] Переглянуто старий файл `User.Action` і відокремлено ініціацію pending action від її подальшого підтвердження.
- [x] Зафіксовано три підтримувані запити користувача: зміна логіну, зміна пароля та видалення акаунта.
- [x] Публічні методи перейменовуються відповідно до наміру користувача: `RequestLoginChange`, `RequestPasswordChange` та `RequestAccountDeletion`.
- [x] Для першого сценарію вирішено почати з `RequestLoginChange`, а спільну абстракцію для трьох запитів виділяти лише після появи реального дублювання.
- [x] Підтверджено доцільність plan-підходу, оскільки вже існують щонайменше два різні сценарії: створення першої pending action і заміна попередньої простроченої action.
- [x] Зафіксовано загальну послідовність: повна перевірка і підготовка плану -> застосування мутації -> запис domain events та aggregate changes.

### Життєвий цикл UserPendingAction

- [x] Переглянуто набір статусів `UserPendingAction`.
- [x] Залишено статуси `Active`, `Confirmed`, `Cancelled` та `Expired`.
- [x] Статус `Failed` видалено, оскільки для нього поки немає конкретного доменного сценарію; його можна додати пізніше разом із реальною поведінкою.
- [x] Для кінцевих переходів використовуються окремі часові позначки: `ConfirmationTime`, `CancelledTime` та `ExpiredAt`.
- [x] `ExpiresAt` описує заплановану межу строку дії, тоді як `ExpiredAt` фіксує фактичний момент системного переходу у статус `Expired`.
- [x] `GetExpirationDate()` повертає встановлене `ExpiresAt`.
- [x] `IsActive(now)` повертає `true` лише для статусу `Active` у часовому інтервалі `CreatedAt <= now < ExpiresAt`.

### Підтвердження pending action

- [x] Підготовку та мутацію розділено на `ValidateConfirmation(token, now)` і `MarkAsConfirmed(token, now)`.
- [x] Перед підтвердженням перевіряються часовий порядок, визначеність enum-значення та активний статус сутності.
- [x] Прострочена action повертає очікувану помилку через `Result`.
- [x] Невідповідний confirmation token повертає очікувану помилку через `Result`.
- [x] `MarkAsConfirmed` захисно повторює валідацію перед мутацією; failure на apply-етапі трактується як порушення внутрішньої передумови.
- [x] Успішний перехід установлює `Status = Confirmed` і `ConfirmationTime = now`.

### Скасування pending action

- [x] Підготовку та мутацію розділено на `ValidateCancellation(token, now)` і `MarkAsCancelled(token, now)`.
- [x] Скасування вимагає той самий confirmation token, оскільки він ідентифікує конкретний confirmation flow.
- [x] За поточною моделлю прострочену action не можна скасувати: вона має перейти у `Expired`.
- [x] Невідповідний token або завершений строк повертають очікувану помилку через `Result`.
- [x] `MarkAsCancelled` захисно повторює валідацію перед мутацією.
- [x] Успішний перехід установлює `Status = Cancelled` і `CancelledTime = now`.

### Системне завершення строку pending action

- [x] Системний перехід розділено на `ValidateCanExpire(now)` і `MarkAsExpired(now)`.
- [x] Для expiration не передається confirmation token, оскільки це часовий системний перехід, а не зовнішнє підтвердження користувача.
- [x] `ValidateCanExpire` дозволяє перехід лише коли `now >= ExpiresAt`.
- [x] Спроба завершити action раніше встановленої межі трактується як порушення внутрішньої передумови й завершується exception.
- [x] Успішний перехід установлює `Status = Expired` і `ExpiredAt = now`.

### Спільні перевірки UserPendingAction

- [x] Спільну перевірку поточного стану винесено в `EnsureActiveState(now)`.
- [x] `EnsureActiveState` перевіряє, що час операції не передує `CreatedAt`, значення `Status` визначене та сутність перебуває у статусі `Active`.
- [x] Неактивна action, завантажена як поточна pending action користувача, розглядається як порушення інваріанта, оскільки агрегат відновлює лише активну дочірню action.
- [x] Перевірку токена винесено в `ValidateConfirmationToken`.
- [x] Null token трактується як порушення внутрішнього контракту, а коректно сформований, але невідповідний token — як очікувана доменна відмова.

### Створення та відновлення UserPendingAction

- [x] `Create` перевіряє обов’язкові доменні об’єкти, генерує `UserPendingActionId` і `ConfirmationFlowToken` та створює action у статусі `Active`.
- [x] Новій action установлюється тридцятихвилинний строк; усі часові позначки кінцевих переходів залишаються `null`.
- [x] `Restore` не генерує нові ID або token і не створює domain events.
- [x] `Restore` перевіряє обов’язкові об’єкти та повну узгодженість збереженого життєвого циклу через `ValidateState`.
- [x] Для `Active` усі три transition timestamps мають бути `null`.
- [x] Для `Confirmed` має існувати лише `ConfirmationTime` у межах `CreatedAt <= ConfirmationTime < ExpiresAt`.
- [x] Для `Cancelled` має існувати лише `CancelledTime` у межах `CreatedAt <= CancelledTime < ExpiresAt`.
- [x] Для `Expired` має існувати лише `ExpiredAt`, причому `ExpiredAt >= ExpiresAt`.
- [x] Для будь-якого стану виконується базове правило `ExpiresAt > CreatedAt`.
- [x] Детальні значення `ConfirmationTime`, `ExpiredAt` і `CancelledTime` записуються в exception context.

### Документація UserPendingAction

- [x] Оновлено XML-коментарі властивостей і query-методів.
- [x] Додано окремі XML-коментарі до validation- та mutation-методів кожного переходу.
- [x] Задокументовано приватні helpers `EnsureActiveState`, `ValidateConfirmationToken` і `ValidateState`.
- [x] Додано окремі коментарі до приватного creation-конструктора та фабрики `Create`.
- [x] Додано окремі коментарі до приватного restoration-конструктора та фабрики `Restore`.
- [x] `UserPendingAction` вважаємо завершеною й approved для поточного доменного дизайну.