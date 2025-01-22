# Coinpedia

A service that accepts a cryptocurrency code as input. The application then display its current quote in the following currencies:
- USD
- EUR
- BRL
- GBP
- AUD

## User Story
As a user running the application
I can view a list of the latest quotes for a user-submitted cryptocurrency code (e.g. BTC)
So that I know what the exchange rate is for every currency

**Acceptance criteria**
- For the known code BTC, results are returned
- Values for USD, EUR, BRL, GBP, and AUD are shown

Only these two APIs as data-source are allowed:
- https://exchangeratesapi.io
- https://coinmarketcap.com/api (free version)

The code compiles and runs in one step (see below)

#### Investigated API limitations
- [coinmarketcap.com](https://coinmarketcap.com/api/pricing/)

| Feature    | Basic plan |
| -------- | ------- |
| Rate limit | 30 Requests per minute |
| Monthly API credits | 10,000 per month |


- [exchangeratesapi.io](https://manage.exchangeratesapi.io/plan)

| Feature    | Basic plan |
| -------- | ------- |
| Rate limit | ??? |
| Monthly API calls | 100 per month |


the following is not taking [overage](https://exchangeratesapi.io/documentation/#billing-overages) into account treating the **overage** as a safety net, hoping it won't be needed.

**FREE** plan gives us 100 API calls per month ⇒ 31 (days/month) * 24 (hours/day) = 744 / 100 (calls) = approximately a call every ~7.44 hours (7 hours and 26 minutes)

## Compile and run the solution in one step (recommended)
**Prerequisite:** Docker
```bash
./run
```
This script makes sure `.env` exists in the root of the solution and then runs docker-compose.
After the first run, it might be beneficial to use docker-compose up directly.

if there were no errors, try to open:
http://localhost:8080/swagger/


## Logs
Seq is a real-time search and analysis server for structured application logs and traces.
https://docs.datalust.co/docs/an-overview-of-seq

After the solution is up and running, Seq should be available at http://localhost:8081/#/events?range=1d
