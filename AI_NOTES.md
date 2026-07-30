# AI Usage Notes

I used both **ChatGPT (OpenAI)** and **Claude (Anthropic)** as learning and development assistants while completing this assignment. I mainly used them to understand concepts, get guidance on implementation, and resolve issues during development. The overall project integration, debugging, testing, and final implementation were completed by me.

## What I used AI for

* Understanding the project requirements and planning the overall structure.
* Getting guidance on creating the solution and project structure using `dotnet` CLI commands.
* Learning how to implement EF Core models, `DbContext`, and the service layer.
* Understanding business logic for stock calculations and validation.
* Getting examples for the MVC controller, Razor views, and Bootstrap styling.
* Understanding how to write xUnit unit tests and improve test coverage.
* Debugging errors and resolving implementation issues during development.

## What I completed myself

* Created and organized the project structure.
* Implemented and integrated the models, services, controllers, and views.
* Connected all application components and ensured they worked together correctly.
* Debugged compile-time and runtime errors.
* Modified and improved the generated code where necessary.
* Verified the application functionality and ensured the business rules worked correctly.

## Something the AI got wrong that I had to correct

One suggested implementation always wrapped `RecordMovementAsync` inside a database transaction without considering the database provider. While this worked correctly with SQLite, it caused failures when running unit tests with EF Core's In-Memory provider. After investigating the issue, I updated the implementation so that transactions are only used when supported by the database provider, allowing both the application and the unit tests to run successfully.
