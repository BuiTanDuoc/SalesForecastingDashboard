# DotNet Sales Forecasting Dashboard

A web-based inventory monitoring and sales analytics system built with ASP.NET Core 8 MVC.  
The application tracks stock levels and safety stock, records daily sales, analyzes sales trends, generates 30-day demand forecasts per product, and provides reorder recommendations supported by interactive dashboards and charts.

This project is designed as a realistic decision-support system for small retail businesses.

---

## Key Features

- **Inventory Management**
  - Add and delete products
  - Track current stock and safety stock
  - Color-coded stock status indicators (OK / LOW)

- **Sales Tracking**
  - Record daily sales by product and quantity
  - Automatic stock updates after each sale
  - Tabular view of all sales transactions

- **30-Day Demand Forecasting**
  - Forecasts future demand based on historical daily sales
  - Individual forecast charts for each product
  - Average daily sales (ADS) calculation

- **Reorder Recommendations**
  - Automatic calculation of minimum reorder quantity
  - Formula: `ADS × lead-time + safety stock`
  - Estimated stock depletion time

- **Low Stock Notifications**
  - Database-stored notifications when stock falls below safety stock
  - Dedicated notifications page
  - Included in daily email reports

- **Daily Email Report (00:30)**
  - Automatically sends:
    - Low stock products
    - All daily sales records

- **CSV Export**
  - Inventory table
  - Daily sales table
  - 30-day forecast data
  - Reorder recommendation table

- **Graphical Dashboard**
  - Last 7 days sales chart
  - Last 30 days sales analytics
  - Top-selling products visualization
  - Reorder and stock depletion charts

---

## Tech Stack

- **ASP.NET Core 8 (Razor Pages / MVC)**
- **C#**
- **Entity Framework Core**
- **MsSQL**
- **BackgroundService** for scheduled tasks (daily report emails)
- **Chart.js** for data visualization
- **Bootstrap** for UI styling

---

## Configuration

Sensitive configuration values are not included in this repository.

1. Create a local configuration file:
   - Copy `appsettings.example.json` and rename it to `appsettings.json`

2. Update the following fields:
   - MsSQL connection string
   - Email sender configuration (for daily report emails)

> Note: `appsettings.json` is intentionally excluded from version control.

---

## Project Purpose
This project was developed as an academic and portfolio project to demonstrate:
Inventory forecasting and reorder decision logic
Data-driven dashboards and analytics
ASP.NET Core MVC application architecture

## License
This project is shared for educational and portfolio purposes.

