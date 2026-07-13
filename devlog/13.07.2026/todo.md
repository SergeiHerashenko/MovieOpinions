# TODO

## Authentication

### Sign In
- [x] Завершити реалізацію `SignInWithEmailCommandHandler`.
- [x] Додати повернення результату після виконання `SignInFlowCoordinator`.
- [x] Додати обробку помилок `ProcessAsync`.

### SignInFlowCoordinator
- [x] Змінити сигнатуру `ProcessAsync` на `Result<SignInResult<Guid>>`.
- [x] Додати перевірку доступу користувача перед перевіркою пароля.
- [x] Інтегрувати `ITokenService`.
- [x] Додати використання `IClock`.
- [x] Реалізувати успішний сценарій входу:
  - оновлення інформації про останній вхід;
  - скидання лічильника невдалих спроб;
  - створення JWT-сесії;
  - збереження змін користувача;
  - повернення `SignInResult`.
- [x] Реалізувати сценарій невдалого входу:
  - збільшення кількості невдалих спроб;
  - автоматичне блокування після перевищення ліміту;
  - збереження змін.

---

## Access

### DeletionCheck
- [x] Реалізувати перевірку видаленого акаунта.
- [x] Додати використання `IUserDeletedRepository`.
- [x] Додати логування неконсистентного стану.
- [x] Автоматично відновлювати користувача, якщо запис про видалення відсутній.
- [x] Повертати помилку при активному видаленому акаунті.

---

## Application Errors

### UserErrors
- [x] Додати `ActiveDeleted()`.

### ApplicationErrorCodes
- [x] Додати `DeletionErrorCode`.
- [x] Додати код `ACCOUNT_DELETED`.

---

## Persistence

### UserDeletedRepository
- [x] Створити інтерфейс `IUserDeletedRepository`.
- [ ] Реалізувати `GetDeletionUserById()`.
- [ ] Реалізувати `CreateAsync()`.
- [ ] Реалізувати `UpdateAsync()`.
- [ ] Реалізувати `DeleteAsync()`.
- [ ] Реалізувати `MapReaderToUserDeletion()`.

### Database
- [x] Створити таблицю `user_deleted`.
- [x] Додати зовнішній ключ на `user_account`.
- [x] Додати індекс по `user_id`.

---

## Domain Tests

### User
- [x] Покрити тестами:
  - Create
  - Restore
  - ChangeLogin
  - ChangePassword
  - ConfirmLogin
  - LoginSuccess
  - Delete
  - Undelete
  - Block
  - RemoveBlock
  - FailedLoginAttempts
  - Guard-перевірки

### UserDeletion
- [x] Покрити тестами агрегат `UserDeletion`.
- [x] Покрити `UserDeletionId`.

### UserPendingChange
- [x] Покрити тестами агрегат `UserPendingChange`.
- [x] Покрити `UserPendingChangeId`.
- [x] Покрити `ConfirmationToken`.
- [x] Покрити всі реалізації `UserChange`.

### Value Objects
- [x] Email
- [x] EmailLogin
- [x] Phone
- [x] PhoneLogin
- [x] CountryCode
- [x] Login
- [x] Password
- [x] UserId