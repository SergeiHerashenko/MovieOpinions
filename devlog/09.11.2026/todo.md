# Зроблено сьогодні (09 листопада 2026)

## Підсумок

- [x] Написано та перевірено тести для `Result` і `Result<T>`.
- [x] Написано та перевірено тести для `UsersPendingRegistration`.
- [x] Написано та перевірено `UserIdTests`.
- [x] Написано та перевірено тести для всіх п’яти груп `Users/Entities`, включно з їхніми VO та типами дій.
- [x] У перелічених нижче нових наборах підготовлено **1379 тестових сценаріїв**.
- [x] Відформатовано Domain: фігурні дужки для всіх `if` і циклів, очищення зайвих пробілів у коментарях.
- [x] Отримано зелений загальний прогін тестового проєкту: **2879 Passed, 0 Failed, 0 Skipped**.

## Написані тести

Наведено назви нових тестових файлів. Кількість сценаріїв враховує всі набори даних у `Theory`.

| Розділ | Написані тести | Сценаріїв |
| --- | --- | ---: |
| Results | Два набори тестів: для `Result` і `Result<T>` | 50 |
| UsersPendingRegistration / ValueObjects | `UserPendingRegistrationIdTests.cs` | 11 |
| UsersPendingRegistration / ValueObjects | `RegistrationFlowTokenTests.cs` | 34 |
| UsersPendingRegistration | `UserPendingRegistrationTests.cs` | 71 |
| Users / ValueObjects | `UserIdTests.cs` | 11 |
| Users / Entities / UsersDeletion / ValueObjects | `DeletionReasonTests.cs` | 65 |
| Users / Entities / UsersDeletion / ValueObjects | `UserDeletionIdTests.cs` | 11 |
| Users / Entities / UsersDeletion | `UserDeletionTests.cs` | 88 |
| Users / Entities / UsersPendingAction / ValueObjects | `UserPendingActionIdTests.cs` | 11 |
| Users / Entities / UsersPendingAction / ValueObjects | `ConfirmationFlowTokenTests.cs` | 34 |
| Users / Entities / UsersPendingAction / Actions | `UserActionTests.cs` | 14 |
| Users / Entities / UsersPendingAction / Actions | `ChangeLoginActionTests.cs` | 22 |
| Users / Entities / UsersPendingAction / Actions | `ChangePasswordActionTests.cs` | 15 |
| Users / Entities / UsersPendingAction / Actions | `DeleteUserActionTests.cs` | 15 |
| Users / Entities / UsersPendingAction | `UserPendingActionTests.cs` | 151 |
| Users / Entities / UsersRefreshToken / ValueObjects | `UserRefreshTokenIdTests.cs` | 11 |
| Users / Entities / UsersRefreshToken / ValueObjects | `RefreshTokenTests.cs` | 40 |
| Users / Entities / UsersRefreshToken / ValueObjects | `DeviceInfoTests.cs` | 116 |
| Users / Entities / UsersRefreshToken / ValueObjects | `IpAddressTests.cs` | 134 |
| Users / Entities / UsersRefreshToken | `UserRefreshTokenTests.cs` | 135 |
| Users / Entities / UsersRestriction / ValueObjects | `UserRestrictionIdTests.cs`, `RestrictionRuleTests.cs` | 86 |
| Users / Entities / UsersRestriction | `UserRestrictionTests.cs` | 102 |
| Users / Entities / UsersRestrictionSession / ValueObjects | `UserRestrictionSessionIdTests.cs` | 11 |
| Users / Entities / UsersRestrictionSession | `UserRestrictionSessionTests.cs` | 141 |
| **Разом у цьому зрізі** | | **1379** |

Для `UserRestrictionIdTests` та `RestrictionRuleTests` наведено спільну кількість за групою Test Explorer.

## Що перевірено в нових тестах

### Results

- [x] Успішні та неуспішні результати, значення `IsSuccess` і `IsFailure`.
- [x] Склад і збереження колекції помилок.
- [x] Відхилення `null`, порожньої колекції помилок для відмови та інших порушень інваріантів результату.
- [x] Збереження значення успішного `Result<T>` та виняток при доступі до `Value` неуспішного результату.

### UsersPendingRegistration

- [x] У `UserPendingRegistrationIdTests` перевірено генерацію UUID v7, відновлення, порожній GUID, перетворення на `Guid`, компоненти рівності й хеш-код.
- [x] У `RegistrationFlowTokenTests` перевірено створення, відновлення, довжину, формат, некоректні значення та рівність.
- [x] У `UserPendingRegistrationTests` перевірено початкові й відновлені значення властивостей, зміну стану під час оновлення та часові межі.
- [x] Перевірено null-аргументи, некоректні дати й діагностичні дані винятків.
- [x] Перевірено запис відповідних доменних подій та відсутність нових подій під час відновлення.

### Users / ValueObjects

- [x] У `UserIdTests` перевірено непорожній UUID v7 та різні значення окремих викликів.
- [x] Перевірено збереження непорожнього GUID і виняток для `Guid.Empty`.
- [x] Перевірено перетворення на `Guid`, компоненти рівності, рівність після відновлення та однакові хеш-коди рівних ID.

### Users / Entities / UsersDeletion

- [x] У `DeletionReasonTests` і `UserDeletionIdTests` перевірено допустимі та некоректні входи, початкові й відновлені значення, рівність.
- [x] У `UserDeletionTests` перевірено статуси, зміни стану, часові межі та повторні переходи.
- [x] Перевірено діагностику некоректного стану й дат.

### Users / Entities / UsersPendingAction

- [x] Перевірено ID та confirmation flow token.
- [x] У `UserActionTests` перевірено правильний конкретний тип дії, збереження переданих VO, значення властивостей і null-аргументи.
- [x] Написано окремі тести для `ChangeLoginAction`, `ChangePasswordAction` і `DeleteUserAction`: створення, відновлення, компоненти рівності та хеш-коди.
- [x] У `UserPendingActionTests` перевірено статуси, токен підтвердження, часові межі та відповідні зміни властивостей.
- [x] Перевірено повторні операції, некоректний стан і незмінність даних під час перевірок та відмов.

### Users / Entities / UsersRefreshToken

- [x] Написано тести для `UserRefreshTokenId`, `RefreshToken`, `DeviceInfo` та `IpAddress`.
- [x] Перевірено допустимі формати, граничні значення, null-аргументи, відновлені значення та рівність VO.
- [x] У `UserRefreshTokenTests` перевірено статуси, придатність до використання, строк дії та зміни часових міток.
- [x] Перевірено узгодженість дат зі статусом, повторні переходи, помилкові аргументи та незмінність стану при відмовах.

### Users / Entities / UsersRestriction

- [x] У `UserRestrictionIdTests` і `RestrictionRuleTests` перевірено ID, назву й тривалість правила, некоректні значення, відновлення та рівність.
- [x] У `UserRestrictionTests` перевірено початкові й відновлені властивості, статуси та часові межі.
- [x] Перевірено повторні та перехресні переходи стану, некоректні поєднання дат і статусів, повну діагностику винятків.

### Users / Entities / UsersRestrictionSession

- [x] Написано `UserRestrictionSessionIdTests` і `UserRestrictionSessionTests`.
- [x] Перевірено склад колекцій, належність обмежень, відповідність типів та унікальність ID.
- [x] Перевірено сумарну тривалість, граничні значення й переповнення арифметики.
- [x] Перевірено часові запити, залишок часу та точні межі завершення.
- [x] Перевірено зіставлення наборів ID, незалежність знімків колекції та копіювання початкових колекцій.
- [x] Перевірено результати пакетного додавання й вилучення, попередню валідацію та всі значення `RestrictionSessionRemovalEffect`.
- [x] Перевірено атомарність невдалих пакетних операцій, незмінність стану при валідації, повторні виклики та повторну перевірку вхідних даних.
- [x] Перевірено пріоритети помилок у конфліктних наборах даних і специфічні поля контексту винятків.

## Виконане форматування Domain

- [x] Відформатовано доменний код без зміни логіки.
- [x] Усі блоки `if` оформлено фігурними дужками, зокрема за наявності лише однієї інструкції.
- [x] Для циклів застосовано таке саме правило: тіло завжди оформлюється фігурними дужками.
- [x] Прибрано зайві пробіли в коментарях.

## Результат перевірки

- [x] Зафіксовано успішний локальний загальний прогін `Authorization.Domain.UnitTests`: **2879 Passed, 0 Failed, 0 Skipped**.
- [x] На наданому скріншоті Test Explorer: **0 Warnings, 0 Errors**.