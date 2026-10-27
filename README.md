# MusicStore — «Музыкальная Планета»

Учебный проект музыкального магазина «Музыкальная Планета» на **.NET 10 / ASP.NET Core**: backend + сайт на Razor.

Состав решения `MusicStore.sln`:

| Проект | Назначение | Адрес (dev) |
|---|---|---|
| `MusicStore.Web` | Основной сайт: ASP.NET Core MVC (Razor) + Web API, EF Core + PostgreSQL | http://localhost:5189 |
| `IdentityService` | Отдельный сервер авторизации: ASP.NET Core Identity, выдаёт JWT | http://localhost:5179 |

`IdentityService` — единственный источник пользователей и ролей. Основной сайт проверяет
выпущенный им JWT (подпись тем же ключом) и хранит токен в **HttpOnly cookie** (не в `localStorage`).

---

## 1. Технологии

- .NET 10 (LTS), ASP.NET Core MVC + Web API
- Entity Framework Core 10 + Npgsql (PostgreSQL 14+)
- ASP.NET Core Identity (только в IdentityService)
- JWT Bearer аутентификация (в обоих приложениях)

## 2. Структура MusicStore.Web

```
Controllers/        MVC-контроллеры + Controllers/Api — API-контроллеры
Models/             Доменные модели (Product, CartItem, Order, OrderItem, DailySpin,
                    DeliveryAddress), enum-ы, DTO в Models/Dtos
MVC/                ViewModels для Razor-страниц
Services/           CatalogService, CartService, OrderService, SpinService,
                    IdentityApiClient (вызовы IdentityService через HttpClient)
Helpers/            PriceCalculator, AuthCookie, JwtHelper, ApiExceptionFilter и др.
Data/               StoreDbContext + EF-миграции + демо-товары (ProductSeeder)
Views/              Razor-представления
wwwroot/            css, js (анимация слотов), изображения товаров
```

Все бизнес-правила — в сервисах, контроллеры только вызывают их. Все цены считает сервер
(`decimal`), клиентским ценам доверять нельзя.

## 3. Настройка PostgreSQL

Сервер PostgreSQL должен быть запущен. Локальные подключения по умолчанию настроены на
`localhost:5432`, пользователь `postgres`.

Подключения задаются в `appsettings.json`:

- `MusicStore.Web` → строка `StoreConnection` (база `musicstore_store`)
- `IdentityService` → строка `IdentityConnection` (база `musicstore_identity`)

По умолчанию там стоит пароль `postgres` — **замените его** через User Secrets
(рекомендуется) или правкой `appsettings.json`:

```bash
cd MusicStore.Web
dotnet user-secrets set "ConnectionStrings:StoreConnection" "Host=localhost;Port=5432;Database=musicstore_store;Username=postgres;Password=ВАШ_ПАРОЛЬ"

cd ../IdentityService
dotnet user-secrets set "ConnectionStrings:IdentityConnection" "Host=localhost;Port=5432;Database=musicstore_identity;Username=postgres;Password=ВАШ_ПАРОЛЬ"
```

## 4. Секреты (обязательно)

Пароли и JWT-ключ не хранятся в исходниках. Задаются через User Secrets или переменные
окружения (`SeedAdmin__Password`, `Jwt__Key`).

```bash
# Ключ подписи JWT — один и тот же в обоих приложениях (любая случайная длинная строка)
cd IdentityService
dotnet user-secrets init
dotnet user-secrets set "Jwt:Key" "<случайный ключ минимум 32 символа>"
dotnet user-secrets set "SeedAdmin:Password" "<пароль администратора>"

cd ../MusicStore.Web
dotnet user-secrets init
dotnet user-secrets set "Jwt:Key" "<тот же ключ>"
```

## 5. Миграции и запуск

```bash
# применить миграции (базы создаются автоматически)
dotnet ef database update --project IdentityService --startup-project IdentityService
dotnet ef database update --project MusicStore.Web   --startup-project MusicStore.Web

# запустить оба приложения (в отдельных терминалах)
cd IdentityService && dotnet run     # http://localhost:5179
cd MusicStore.Web  && dotnet run     # http://localhost:5189
```

При старте IdentityService создаёт роли `User` и `Admin` и **первоначального администратора**
из конфигурации (`SeedAdmin:Username`, `SeedAdmin:Email`, `SeedAdmin:Password`). Если пароль
не задан, администратор не создаётся, а в лог пишется предупреждение. Обычная регистрация
всегда выдаёт только роль `User`.

`MusicStore.Web` при старте БД не мигрирует — выполните `database update` вручную
(см. выше). Демонстрационные товары (12 позиций во всех 6 категориях) добавляются миграцией.

Открытые страницы сайта: каталог с фильтрами, карточка товара, вход/регистрация, корзина,
оформление заказа, «Слоты удачи», мои заказы.

## 6. API маршруты

### IdentityService (`http://localhost:5179`)

| Метод | Маршрут | Описание |
|---|---|---|
| POST | `/api/auth/register` | Регистрация (role=User всегда) |
| POST | `/api/auth/login` | Вход, возвращает JWT |
| GET | `/api/auth/me` | Текущий пользователь (Bearer) |
| POST | `/api/auth/logout` | Выход (Bearer) |

### MusicStore.Web (`http://localhost:5189`)

Авторизация API: заголовок `Authorization: Bearer <token>` **или** HttpOnly cookie
`MusicStore.Auth` (браузером ставится автоматически после входа через сайт).

**Каталог (анонимно):**

| Метод | Маршрут | Параметры |
|---|---|---|
| GET | `/api/products` | `search`, `category` (Guitars, Strings, Pianos, Drums, Vinyl, Amplifiers), `minPrice`, `maxPrice`, `onlyDiscounted`, `sort` (`name`, `price_asc`, `price_desc`, `discount`, `newest`), `page`, `pageSize` (≤50) |
| GET | `/api/products/{id}` | |

**Администрирование товаров (только `Admin`):**

| Метод | Маршрут | Описание |
|---|---|---|
| GET | `/api/admin/products` | Все товары, включая неактивные |
| POST | `/api/admin/products` | Создать (`name`, `description`, `category`, `price`, `discountPercent`, `stockQuantity`, `imageUrl`, `isActive`) |
| PUT | `/api/admin/products/{id}` | Изменить |
| DELETE | `/api/admin/products/{id}` | Удалить (товар из заказов удалить нельзя — только `isActive=false`) |

**Корзина (авторизованный):**

| Метод | Маршрут | Описание |
|---|---|---|
| GET | `/api/cart` | Просмотр |
| POST | `/api/cart` | Добавить `{productId, quantity}` |
| PUT | `/api/cart/{cartItemId}` | Изменить количество `{quantity}` |
| DELETE | `/api/cart/{cartItemId}` | Удалить позицию |
| DELETE | `/api/cart` | Очистить корзину |

**Заказы (авторизованный; `all` и статус — только `Admin`):**

| Метод | Маршрут | Описание |
|---|---|---|
| POST | `/api/orders` | Оформить: `{address: {city, street, house, apartment?, postalCode, recipientName?, phone?}, useSpinBonus}` |
| GET | `/api/orders` | Свои заказы |
| GET | `/api/orders/{id}` | Свой заказ |
| GET | `/api/orders/all` | Все заказы (Admin) |
| PUT | `/api/orders/{id}/status` | Смена статуса (Admin): `New`, `Paid`, `Shipped`, `Delivered`, `Cancelled` |

**«Слоты удачи» (авторизованный):**

| Метод | Маршрут | Описание |
|---|---|---|
| GET | `/api/spins/status` | Можно ли крутить сегодня + активный бонус |
| POST | `/api/spins/spin` | Прокрутка (одна в сутки, сервер решает всё сам) |

## 7. Примеры запросов

```bash
# регистрация и вход
curl -X POST http://localhost:5179/api/auth/register -H "Content-Type: application/json" \
  -d '{"userName":"ivan","email":"ivan@example.com","password":"qwerty123"}'
TOKEN=$(curl -s -X POST http://localhost:5179/api/auth/login -H "Content-Type: application/json" \
  -d '{"userName":"ivan","password":"qwerty123"}' | python -c "import sys,json;print(json.load(sys.stdin)['token'])")

# каталог с фильтрами
curl "http://localhost:5189/api/products?search=guitar&category=Guitars&onlyDiscounted=true&sort=price_asc&page=1&pageSize=10"

# корзина
curl -X POST http://localhost:5189/api/cart -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" -d '{"productId":4,"quantity":2}'

# оформление заказа с бонусом от слотов
curl -X POST http://localhost:5189/api/orders -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"address":{"city":"Moscow","street":"Tverskaya","house":"1","postalCode":"101000"},"useSpinBonus":true}'

# прокрутка слотов
curl -X POST http://localhost:5189/api/spins/spin -H "Authorization: Bearer $TOKEN"

# админ: создать товар (нужен токен администратора)
curl -X POST http://localhost:5189/api/admin/products -H "Authorization: Bearer $ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"name":"Yamaha FG800","description":"Акустика","category":"Guitars","price":45000,"discountPercent":5,"stockQuantity":3,"imageUrl":"","isActive":true}'
```

## 8. Механика скидок

**Обычные скидки товаров.** У товара есть `price` и `discountPercent`; каталог и сервер
считают `finalPrice` (`Helpers/PriceCalculator`). В интерфейсе показывается старая и новая цена.

**«Слоты удачи».** Бесплатная игра (ничего не платится и не теряется), одна попытка в сутки.
Результат определяется **только на сервере**: сначала выпадает уровень приза
(45% — без скидки, 30% — 5%, 15% — 10%, 8% — 15%, 2% — 20%), затем под него подбираются
символы барабанов (💎💎💎 — 20%, три одинаковых — 15%, два алмаза — 10%, пара — 5%,
три разных — без скидки). Клиент только анимирует готовый результат.

Защита от повторной прокрутки — **уникальный индекс `(UserId, SpinDate)` в PostgreSQL**,
поэтому даже параллельные запросы не дадут прокрутить дважды (повторный вызов вернёт 409).

Бонус живёт 24 часа, принадлежит пользователю, применяется **максимум к одному заказу**
(`DailySpins.IsUsed`) и только к сумме **после** обычных скидок товаров:
`total = round(itemsTotal * (1 - spinPercent/100), 2)`. Право на бонус и итоговую цену
проверяет сервер при оформлении заказа.

## 9. Полезное

- Пересоздать миграции: `dotnet ef migrations add <Имя> --project <Проект> --startup-project <Проект> -o Data/Migrations`
- Посмотреть секреты: `dotnet user-secrets list` в каталоге проекта
- Статусы заказов меняет только администратор через `PUT /api/orders/{id}/status`
