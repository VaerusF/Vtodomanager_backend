# VTodoManager Backend
### О проекте
Бэкэнд проект таск-менеджера (API). 
Реализованы базовые операции CRUD для сущностей проекта:
Проект, доска, задача, а также регистрация и авторизация с использованием JWT токенов.

### Основной стек технологий
* ASP.NET Core;
* Entity Framework Core;
* PostgreSQL 14;
* MediatR;
* Swashbuckle (Swagger);
* AutoMapper;
* Docker.

### Запуск проекта
* Для запуска проекта требуется установить Docker и Docker Compose;
* Склонировать репозиторий `git clone https://github.com/TVSergey/VTodoManager_Backend_AspNetCore.git`;
* Перейти в корневой каталог репозитория;
* Запустить проект, выполнив `docker-compose up --build`;
* Перейти на `http://127.0.0.1:8080/`.

Для доступа к основным функциям API необходимо создать аккаунт с помощью метода POST `/api/v1/accounts`, например:
    `{
        "email": "test@test.com",
        "username": "test",
        "password": "testpassw",
        "confirmPassword": "testpassw"
    }`
и затем указать accessToken в окне Authorize. 

По умолчанию accessToken действителен 5 минут. Время жизни токенов можно изменить в файле конфигураций 
MainApp/Vtodo.Web/appsettings.json. 

Метод POST `/api/v1/accounts/refresh_tokens` позволяет получить новую пару токенов.