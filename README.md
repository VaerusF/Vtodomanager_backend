# VTodoManager Backend
### О проекте
Бэкэнд проект таск-менеджера (API). 
Реализованы базовые операции CRUD для сущностей проекта:
Проект, доска, задача, а также регистрация и авторизация с использованием JWT токенов.

### Cтек технологий
* ASP.NET Core;
* Entity Framework Core;
* PostgreSQL 14;
* MediatR;
* Swashbuckle (Swagger);
* RabbitMQ;
* Redis;
* Docker.

### Запуск проекта
* Для запуска проекта требуется установить Docker и Docker Compose;
* Склонировать репозиторий `git clone https://github.com/VaerusF/Vtodomanager_backend.git`;
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

Метод POST `/api/v1/accounts/refresh_tokens` позволяет получить новую пару токенов.