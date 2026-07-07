# TODO

## Authentication

### Login
- [x] Додано endpoint `POST /login/email`.
- [x] Додано `LoginWithEmailRequest`.
- [x] Додано `LoginWithPhoneRequest` (підготовка до входу через телефон).
- [x] Створено `SignInWithEmailCommand`.
- [x] Додано валідацію `SignInWithEmailCommandValidator`.
- [x] Розпочато реалізацію `SignInWithEmailCommandHandler`.
- [x] Винесено загальну логіку входу в `SignInFlowCoordinator`.

---

## Cookie Authentication

- [x] Створено `ICookieProvider`.
- [x] Реалізовано `CookieProvider`.
- [x] Додано `CookieSettingsOptions`.
- [x] Реалізовано встановлення Access/Refresh Cookie.
- [x] Реалізовано очищення Cookie.
- [x] Автоматичне встановлення Cookie після підтвердження реєстрації.
- [x] Автоматичне встановлення Cookie після успішного входу.

---

## Confirm Registration

- [x] `ConfirmRegistrationResult` зроблено generic (`ConfirmRegistrationResult<TId>`).
- [x] Повернення `UserId` після успішної реєстрації.
- [x] Оновлено `ConfirmRegistrationCommand`.
- [x] Оновлено `ConfirmRegistrationCommandHandler`.
- [x] Уніфіковано модель відповіді із майбутнім Login Result.

---

## Access Control

### Access Pipeline

- [x] Створено `IAccessStep`.
- [x] Створено `IAccessService`.
- [x] Реалізовано `AccessService`.
- [x] Підтримка пріоритетів (`Priority`) для послідовного виконання перевірок.

### Access Steps

- [x] Реалізовано `BlockCheck`.

---

## User Restrictions

- [x] Додано `IUserRestrictionRepository`.
- [x] Додано `IUserRestrictionSessionRepository`.
- [x] Реалізовано перевірку активних банів.
- [x] Автоматичне зняття блокування після завершення строку бану.
- [x] Автоматичне видалення Restriction Session після завершення блокування.
- [x] Формування повідомлення із переліком активних причин блокування.

---

## User Errors

- [x] Додано:
  - `UserNotFound`
  - `InvalidPassword`
  - `ActiveRestrictions`
- [x] Додано нові Application Error Codes.

---

## Security

- [x] До `IHasher` додано `FakeVerify()`.
- [x] Реалізовано захист від User Enumeration при невірному логіні.

---

## Database

### Refresh Tokens

- [x] Додано Foreign Key для `user_refresh_token`.

### Restrictions

- [x] Створено таблицю `user_restriction`.
- [x] Створено таблицю `user_restriction_session`.
- [x] Додано необхідні індекси.
- [x] Додано Foreign Key зв'язки.

---

## API

- [x] AuthenticationController підтримує Cookie Authentication.
- [x] Додано endpoint Login by Email.

---

## Refactoring

- [x] Виділено окремий `AccessService`.
- [x] Почато винесення бізнес-логіки входу із Handler у Coordinator.
- [x] Уніфіковано Result-моделі між Registration та Login.