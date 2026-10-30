# Gym Management System

## Опис

Gym Management проект изработен со ASP.NET Core 8 MVC.

Апликацијата претставува систем за управување со теретана и овозможува управување со корисници, членарини, часови, резервации и присуство.

Проектот користи ASP.NET Core Identity за регистрација, најава, автентикација и управување со кориснички улоги.
---
## Тест кориснички сметки

За тестирање на различните кориснички улоги може да се користат следните демо сметки:

| Улога | Email | Лозинка |
|------|-------|---------|
| Member | member@gym.com | Member123! |
| Trainer | john.doe@gym.com | Trainer123! |
| Admin | admin@gym.com | Admin123! |

Секоја сметка има пристап до функционалностите достапни за соодветната корисничка улога.
---

## Технологии

- ASP.NET Core 8
- ASP.NET Core MVC
- C#
- Entity Framework Core
- ASP.NET Core Identity
- Razor Views
- Bootstrap
- HTML
- CSS
- SQLite

---

## Кориснички улоги

Во системот постојат три кориснички улоги:

- Admin
- Member
- Trainer

Улогите работат преку ASP.NET Core Identity.

Корисникот се најавува преку Identity Login, а системот според неговата улога одредува до кои функционалности има пристап.

### Admin

Admin има пристап до административниот дел и може да:

- прегледува Admin Dashboard
- управува со членарини
- креира членарини
- уредува членарини
- брише членарини
- пребарува членарини
- управува со корисници
- креира корисници
- брише корисници
- управува со резервации
- креира резервации
- брише резервации
- прегледува статистика

### Member

Member може да:

- ги прегледува достапните членарини
- избере членарина
- се претплати на членарина
- ја прегледува својата претплата
- го прегледува статусот на членарината
- ги користи функционалностите достапни за членови

### Trainer

Trainer може да:

- ја прегледува својата програма
- креира нов час
- ги прегледува своите часови
- отвора Class Roster
- ги прегледува регистрираните членови
- означува присуство
- го зачувува присуството

---

## Authentication и Authorization

Апликацијата користи ASP.NET Core Identity.

Identity се користи за:

- Login
- Logout
- Registration
- Password management
- User management
- Roles
- Authentication
- Authorization

Улогите се:

    Admin
    Member
    Trainer

Пристапот до одредени контролери и функционалности е ограничен според улогата на најавениот корисник.

Пример:

    [Authorize(Roles = "Admin")]

За Trainer:

    [Authorize(Roles = "Trainer")]

За Member:

    [Authorize(Roles = "Member")]

На овој начин Admin, Member и Trainer имаат различни дозволи во системот.

---

## Главни функционалности

### Membership Plans

Администраторот може да креира, уредува и брише членарини.

Секоја членарина содржи:

- Name
- Description
- Price
- DurationInDays

Членовите можат да ги прегледуваат достапните планови.

---

### Subscription

Најавен Member може да избере членарина и да ја потврди претплатата.

При претплата се зачувуваат:

- Membership Type
- Start Date
- End Date
- Active status

Member може преку My Subscription да ја прегледува својата активна или истечена членарина.

---

### Gym Classes

Trainer може да креира нов Gym Class.

За секој час се внесуваат:

- Name
- StartTime
- DurationInMinutes
- MaxCapacity
- Trainer

Trainer преку My Schedule може да ги прегледува своите часови.

---

### Class Roster

Trainer може да отвори Class Roster за својот час.

Roster-от ги прикажува:

- Member
- Booking date
- Attendance status

Trainer може да означи дали членот присуствувал на часот и да го зачува присуството.

---

### Bookings

Admin може да ги прегледува сите резервации.

Резервацијата содржи:

- Gym Class
- Member
- Trainer
- Class Time
- Booked At

Admin може да креира и брише резервации.

---

## Admin Dashboard

Admin Dashboard прикажува основна статистика за системот:

- Total Members
- Active Subscriptions
- Total Classes
- Today's Bookings

Dashboard-от содржи и Quick Actions за:

- Manage Memberships
- Manage Bookings
- Manage Users

---

# Инсталација

## Барања

За стартување на проектот потребно е:

- .NET 8 SDK
- IDE по избор (JetBrains Rider, Visual Studio или Visual Studio Code)
- SQLite
- Entity Framework Core

Проектот користи ASP.NET Core 8.

Проверка на .NET верзијата:

    dotnet --version

Потребно е да се користи .NET 8 SDK.

---

## Преземање на проектот

Клонирајте го repository-то:

    git clone <repository-url>

Влезете во проектот:

    cd GymManagement

Преземете ги NuGet пакетите:

    dotnet restore

---

## База на податоци

Проектот користи SQLite база на податоци преку Entity Framework Core.

Connection string-от се наоѓа во:

    appsettings.json

SQLite базата се користи за зачувување на податоците од апликацијата, вклучувајќи корисници, улоги, членарини, претплати, часови и резервации.

Доколку проектот има Entity Framework Core migrations, базата може да се креира или ажурира со:

    dotnet ef database update

---

## Entity Framework Core

Доколку Entity Framework Core CLI не е инсталиран:

    dotnet tool install --global dotnet-ef

Потоа базата може да се ажурира со:

    dotnet ef database update

Доколку проектот има migrations, Entity Framework Core ќе ја креира/ажурира базата според migrations.

---

## Стартување на проектот

Проектот може да се стартува преку Terminal:

    dotnet run


Или преку IDE по избор (JetBrains Rider, Visual Studio или Visual Studio Code).

По стартувањето, ASP.NET Core ќе прикаже локална URL адреса, на пример:

    https://localhost:7000

Адресата може да биде различна во зависност од локалната конфигурација.

---

# ASP.NET Core Identity

ASP.NET Core Identity се користи за управување со корисничките accounts.

Корисникот се најавува преку Identity Login.

По успешниот login, неговата Identity улога се користи за authorization.

Системот има три улоги:

    Admin
    Member
    Trainer

Овие улоги се користат за контрола на пристапот до функционалностите на системот.

Пример:

    [Authorize(Roles = "Admin")]

само Admin корисници можат да пристапат до одреден endpoint.

Trainer функционалностите се заштитени со:

    [Authorize(Roles = "Trainer")]

Member функционалностите се заштитени со:

    [Authorize(Roles = "Member")]

---

# Пример на тек на системот

## Admin

    Login
      |
      v
    Admin Dashboard
      |
      +-- Manage Memberships
      |
      +-- Manage Bookings
      |
      +-- Manage Users

## Member

    Login
      |
      v
    Membership Plans
      |
      +-- Subscribe
      |
      +-- My Subscription

## Trainer

    Login
      |
      v
    My Schedule
      |
      +-- Create Class
      |
      +-- Class Roster
              |
              +-- Mark Attendance

---

# Безбедност

Проектот користи ASP.NET Core Identity и role-based authorization.

За POST операции се користи Anti-Forgery Token каде што е потребно.

Пример:

    @Html.AntiForgeryToken()

и:

    [ValidateAntiForgeryToken]

Ова обезбедува дополнителна заштита при извршување на POST requests.

---

# Универзитетски проект

Gym Management System е развиен како универзитетски проект со цел да се демонстрира практична употреба на:

- ASP.NET Core 8 MVC
- C#
- Entity Framework Core
- ASP.NET Core Identity
- Authentication
- Authorization
- Role-based authorization
- CRUD операции
- Database relationships
- Razor Views
- Bootstrap
- MVC архитектура