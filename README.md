# Kinetic Inventory API
API para gestionar un inventario de productos y publicar eventos a RabbitMQ. Usa SQLite para la gestion de la base de datos.

## Requisitos

- Docker
- .NET 8 SDK (solo si se quiere compilar local)

## Ejecución con Docker Compose

### 1. Clonar repo

git clone <url-repo-inventory>

cd kinetic-api-inventory/InventoryAPI

docker compose up

Este docker-compose.yml incluye RabbitMQ para pruebas locales. Se debe ejecutar primero. Al realizar la primera modificacion en el inventario se crean las colas de Rabbit.

### 2. Acceder a la API
Swagger: http://localhost:5000/swagger

En Swagger se pueden ver los endpoints solicitados para gestionar los productos de inventario.
Se agregaron validaciones para el POST y el PUT: el producto no puede tener name vacio, el price debe ser mayor a 0 y el stock debe ser mayor o igual a 0. 

### 3. Acceder a RabbitMQ (unicamente para visualizar las queues)
UI: http://localhost:15672

User: guest
Pass: guest

# Descripción de arquitectura — Inventory API
## Estructura general (diagrama mas abajo)

### API
Expone los Controllers para operaciones HTTP.

Maneja validaciones de datos de entrada usando FluentValidation.

Conoce el proyecto Services.

Conoce el proyecto DTOs.

### Services

Contiene la lógica de negocio. Manejaria validaciones de dominio si hubiera alguna definición.

Contiene los mappers para convertir entre entidades y DTOs.

Se encarga de la publicación de eventos en RabbitMQ, luego de un Create, Update o Delete de producto.

Conoce el proyecto DAL.

Conoce el proyecto DTOs.

### DAL - Data Access Layer

Acceso a datos con Entity Framework Core.

Define la base SQLite inventory.db
Esta base se puede observar dentro del contenedor de Docker en /app, y visualizar con herramientas como DB Browser (SQLite).
Podés copiar el archivo desde el contenedor y abrirlo localmente con herramientas como DB Browser SQLite.
Tambien podes acceder desde la consola, entrando al contenedor y copiando el archivo a local. El archivo puede demorar en actualizarse en el contenedor, se recomienda esperar unos minutos luego de procesar los mensajes.

docker ps
docker cp inventoryapi-api:/app/inventory.db ./inventory.db
Contiene entidades de dominio y repositorios.

### DTOs

Define los objetos de transferencia de datos.

# Diagrama de la arquitectura

                        +--------------------+
                        |      Client        |
                        +---------+----------+
                                  |
                                  v
                        +---------+----------+
                        |     API (ASP.NET)  |
                        |  - Controllers     |
                        |  - Validations     |
                        +---------+----------+
                                  |
                                  v
                        +---------+----------+
                        |     Services       |
                        |  - Logic           |
                        |  - Mappers         |
                        |  - Publish Events  |
                        +---------+----------+
                           |            |
            usa DAL ------+            +-----> publica eventos
                           |                     a
                           v
                 +---------+----------+        +-------------+
                 |     DAL (EF)       |        |  RabbitMQ   |
                 |  - Repositories    |        +-------------+
                 |  - Entities        |
                 |  - DbContext       |
                 +---------+----------+
                           |
                           v
                      +----+----+
                      | SQLite |
                      +---------+

API expone puerto 5000, conectado a RabbitMQ.
RabbitMQ expone puertos 5672 y 15672 para administrar.


