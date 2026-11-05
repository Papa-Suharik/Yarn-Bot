# Yarn-bot

Telegram customer loyality Bot

![image alt](Images/Logo.png)

A simple Telegram Bot designed for customers to track their promo codes and send scheduled notifications through integration with an external ERP system.

## Overview

Yarn-bot is a bot developed to simplify customer interaction with a loyality program.

It communicated with an external ERP system through REST API, retrives customer information and promo codes, stores relevant data in PostgreSQL and sends automated notifications.

## Features

- Customer identification by phone number
- Integration with an external 1C REST API
- Promo codes synchronization
- Automated notifications about expiring promo codes
- Scheduled background tasks
- Admin notifications and error logging
- Database caching to reduce external API requests

## Architecture

![image alt](Images/Architecture.png)

## Tech Stack

| Category | Technologies |
|:---:|:---:|
| Language | C# |
| Framework | .NET Worker Service |
| Database | PostgreSQL |
| ORM | Entity Framework Core |
| Integration | REST API, Telegram Bot API |
| Logging | Serilog |
| Deployment | Docker, Docker Compose |

## Getting Started

```bash
git clone https://github.com/Papa-Suharik/Yarn-Bot.git
cd Yarn-Bot
```

Configure your environment variables and database connection, then run:

```bash
dotnet restore
dotnet ef database update
dotnet run
```
### Configuration

| Variable | Description |
|:---|:---:|
| `BOT_TOKEN` | Telegram bot authentication token |
| `BOT_ADMIN_CHAT_ID` | Telegram chat ID of the bot administrator |
| `ONEC_LOGIN` | Login for authentication with the external 1C API |
| `ONEC_PASSWORD` | Password for authentication with the external 1C API |
| `ONEC_BASE_URL` | Base URL of the external 1C API |
| `ONEC_BYPHONE_ENDPOINT` | API endpoint for retrieving customer information by phone number |
| `ONEC_BYID_ENDPOINT` | API endpoint for retrieving customer information by customer ID |
| `ONEC_EXPPROMOS_ENDPOINT` | API endpoint for retrieving expiring promotional codes |
| `DB_HOST` | PostgreSQL database host |
| `DB_PORT` | PostgreSQL database port |
| `DB_NAME` | PostgreSQL database name |
| `DB_USER` | PostgreSQL database username |
| `DB_PASSWORD` | PostgreSQL database password |

## Deployment

The application can be containerized and deployed using Docker Compose.


