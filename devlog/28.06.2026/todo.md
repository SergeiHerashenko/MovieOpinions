# Зроблено сьогодні (28.06.2026)

## Шар API 
- [х] Створено моделі вхідних запитів (DTO) презентаційного рівня для реєстрації: `RegistrationWithEmailRequest` та     `RegistrationWithPhoneRequest`. 

## Шар Application
- [x] Додано файл помилок для `User`.
- [x] Додано `enum` для `RateLimitAction`, `MessageActions`, `CommunicationChannel`
- [x] Додано кастомні Exception для шару Application `ApplicationInvalidInputException`, `ApplicationInvalidOperationException`, `BaseApplicationException`
- [x] **Побудовано CQRS-архітектуру реєстрації (через MediatR):** реалізовано ізольовані потоки для Email та Phone (команди, хендлери, валідація), впроваджено оркестрацію кроків (`RegistrationFlowCoordinator`) та обробку доменних подій.

## Шар Infrastructure
- [x] **Реалізовано базовий інфраструктурний шар (`Authorization.Infrastructure`):** розгорнуто рівень збереження даних на чистому ADO.NET (фабрики з'єднань, міграції та репозиторії).
- [x] Впроваджено сервіси безпеки (хешування `Hasher`, лімітування запитів `RateLimiter`).
- [x] Реалізовано диспетчер доменних подій (`DomainEventDispatcher`), а також систему централізованої обробки помилок та виключень.