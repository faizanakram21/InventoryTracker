# Inventory & Stock Movements — .NET Developer Assignment

A small ASP.NET Core MVC app for tracking products and their stock levels, built on **.NET 10**.

## Tech Stack

- ASP.NET Core MVC (.NET 10)
- Entity Framework Core with SQLite
- xUnit for unit tests

## How to Run

1. Make sure you have the **.NET 10 SDK** installed (`dotnet --version` should show `10.x`).
2. Clone/unzip the repository.
3. From the repository root:

```bash
cd InventoryTracker.Web
dotnet run
```

4. The app will apply EF Core migrations and seed sample data automatically on startup.
5. Open the URL shown in the console (e.g. `http://localhost:5034`) in your browser.

No manual database setup is needed — the SQLite database file (`inventory.db`) is created automatically, migrations are applied, and 3 sample products with stock movements are seeded on first run.

## Running Tests

```bash
cd InventoryTracker.Tests
dotnet test
```

There are 6 unit tests covering:
- Current stock calculation (sum of In − sum of Out)
- Rejecting an "Out" movement that would take stock below zero
- Allowing an "Out" movement that brings stock exactly to zero
- Rejecting zero/negative quantities
- Rejecting duplicate SKUs (case-insensitive)

## Project Structure