# RuletaApi

Proyecto de juego de ruleta usando API de C# .NET.

Realizado en Visual Studio 2022 con los paquetes Microsoft.EntityFrameworkCore, Microsoft.EntityFrameworkCore.SqlServer y Microsoft.EntityFrameworkCore.Tools para el manejo de bases de datos
La base de datos usada en el proyecto usa SQL Express (SQL Server12.0.6024)
Para crear la base de datos en el equipo que corre el proyecto debe correr el comando "Update-database" en la consola del Package Manager
Se debe verificar el string de conexión "Connection" en el archivo appsettings.json

Dentro del proyecto se hace la construcción de 5 endpoints para la API que se encargan de:

1. Crear Ruleta
2. Crear Usuario
3. Abrir Ruletas creadas
4. Crear Apuestas en una ruleta abierta
5. Cerrar las apuestas en una ruleta abierta

El código para cada endpoint se encuentra en el archivo GameController.cs de la carpeta Controllers

La interacción con los endpoints para pruebas se hace en Swagger, ejecutando el proyceto en Visual Studio 2022.

Para clonar el repositorio y que funcionen las configuraciones, es recomendable clonar directamente desde Visual Studio.
