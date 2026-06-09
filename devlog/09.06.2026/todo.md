# Зроблено сьогодні (9 червня 2026)

## Основні задачі

- [x] Розширено доменну модель Refresh Token
    - реалізовано `UserRefreshToken` як повноцінний Aggregate Root
    - додано поля:
        - `ConsumedAt`
        - `RevokedAt`
        - `ExpiresAt`
        - `DeviceInfo`
        - `IpAddress`
        - `City`
    - введено статусну модель:
        - `Active`
        - `Consumed`
        - `Revoked`
        - `Expired`

- [x] Реалізовано поведінку Refresh Token
    - `Consume(now)` — переводить токен у `Consumed`
    - `Revoke(now)` — переводить токен у `Revoked`
    - `Expire(now)` — переводить токен у `Expired`
    - `IsActive()` — перевірка стану токена
    - додано захист від повторного використання неактивних токенів

- [x] Впроваджено Result-підхід для доменної логіки токена
    - `Create()` повертає `Result<UserRefreshToken>`
    - валідація вхідних параметрів через `Result.Failure`
    - відмова від “тихих” null / invalid state

- [x] Додано жорстку інваріантну модель станів токена
    - статуси є фінальними (terminal states):
        - `Consumed`
        - `Revoked`
        - `Expired`
    - після переходу зі `Active` подальша модифікація заборонена

---

## Value Objects

- [x] `IpAddress`
    - реалізовано IPv4 валідацію (4 октети)
    - додано `Create` з Result-патерном
    - додано `Restore` з domain exception при неконсистентних даних
    - нормалізація значення (`Trim`)
    - захист від пустих значень при відновленні

- [x] `RefreshToken`
    - генерація через `RandomNumberGenerator`
    - довжина токена стандартизована (64 bytes)
    - додано `Restore` з перевіркою пустих значень

- [x] `DeviceInfo`
    - опис клієнтського середовища (device/browser/os/model)
    - реалізовано `ToDisplayString()`
    - приведено до ValueObject

---

## Aggregate: UserPendingChange

- [x] Покращено модель Pending Change
    - додано `UserChange` як ValueObject-ієрархію
    - реалізовано типи змін:
        - `PasswordChange`
        - `PasswordReset`
        - `LoginChange`

- [x] Додано статусну модель зміни
    - `Active`
    - `Confirmed`
    - `Expired`

- [x] Реалізовано поведінку Pending Change
    - `ConfirmChange(now)` — підтвердження зміни
    - `MarkAsExpired(now)` — прострочення зміни
    - додано guard-логіку від повторного виконання

- [x] Додано повну state validation логіку при Restore
    - перевірка консистентності:
        - статус ↔ поля (confirmationTime / expiredAt)
    - захист від некоректного відновлення агрегату з БД

- [x] Додано Domain Event
    - `UserPendingChangeRequestedEvent`

---

## Aggregate: UserDeletion

- [x] Розширено модель UserDeletion
    - додано статусну модель:
        - `Deleted`
        - `Restored`
        - `PermanentlyDeleted`
    - додано поля:
        - `DeletedAt`
        - `RestoreUntil`
        - `RestoredAt`
        - `UpdatedAt`

- [x] Реалізовано поведінку видалення користувача
    - `Undelete(now)` — відновлення акаунта
    - `MarkAsExpired(now)` — фінальне видалення після дедлайну
    - додано бізнес-обмеження на restore window

- [x] Додано Domain Event
    - `UserAccountDeletionRequestedEvent`

---

## Value Objects / Ids

- [x] Створено та стандартизовано Id-об’єкти
    - `UserRefreshTokenId`
    - `UserPendingChangeId`
    - `UserRestrictionId`
    - `UserDeletionId`

- [x] Уніфіковано підхід до:
    - `CreateUnique()`
    - `Restore(Guid)`
    - перевірки `Guid.Empty`
    - implicit conversion to `Guid`

---

## Domain Events

- [x] Додано Domain Event механіку в агрегати
    - `UserPendingChangeRequestedEvent`
    - `UserAccountDeletionRequestedEvent`
    - підготовлено структуру для подальшого розширення подіями:
        - refresh token lifecycle events (WIP)

---

## Архітектурні зміни

- [x] Підсилено інваріанти доменних агрегатів
    - заборона неконсистентного стану при Restore
    - перевірка бізнес-логіки на рівні Aggregate Root

- [x] Уніфіковано підхід до станів
    - статус = єдина точка істини для lifecycle
    - timestamps = доказовий шар стану (audit trail)

- [x] Посилено domain consistency layer
    - активне використання `DomainDataInconsistencyException`
    - розмежування:
        - validation (Create)
        - consistency (Restore)

---

## Рефакторинг

- [x] Уніфіковано поведінкові методи агрегатів
    - всі зміни стану тільки через domain methods
    - прямий set станів заборонений поза агрегатом

- [x] Покращено читабельність lifecycle моделей
    - чітке розділення:
        - Create
        - Restore
        - Behavior
        - Guards