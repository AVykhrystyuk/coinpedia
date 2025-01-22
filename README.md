# Coinpedia

A service that accepts a cryptocurrency code as input. The application then displays its current quote in the following currencies: USD, EUR, BRL, GBP, and AUD.

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

## Investigated APIs, their plans and limitations
- [coinmarketcap.com](https://coinmarketcap.com/api/pricing/)

| Feature    | Basic plan |
| -------- | ------- |
| Rate limit | 30 Requests per minute |
| Monthly API credits/calls | 10,000 per month |


- [exchangeratesapi.io](https://manage.exchangeratesapi.io/plan)

| Feature    | Basic plan |
| -------- | ------- |
| Rate limit | ??? |
| Monthly API calls | 100 per month |


*NOTE: The API response should be cached to reduce the number of calls the service makes each month, which eventually helps to respond faster while also saving some money on API calls per month.*


#### How often can we call exchange rate API per month? => How long can we store the response data in the cache?
*The following is not taking [overage](https://exchangeratesapi.io/documentation/#billing-overages) into account treating the **overage** as a safety net, hoping it won't be needed.*

- **"FREE"** plan gives **100 API calls per month** which is approximately **a call every 7 hours and 26 minutes**
  - 31 (days/month) * 24 (hours/day) = 744 (hours/month) ⇒
  - 744 / 100 (calls/month) = ~7.44 hours

- **"Basic"** plan gives **10,000 API calls per month** which is approximately **a call every 4 minutes and 27 minutes**
  - 31 (days/month) * 24 (hours/day) * 60 (minutes/hour) = 44,640 (minutes/month) ⇒
  - 44,640 / 10,000 (calls/month) = ~4.46 minutes

- **"Pro Plan"** plan gives **100,000 API calls per month** which is approximately **a call every 26 seconds**
  - 31 (days/month) * 24 (hours/day) * 60 (minutes/hour) * 60 (second/minute) = 2,678,400 (seconds/month) ⇒
  - 2,678,400 / 100,000 (calls/month) = ~26.78 seconds


## Compile and run the solution in one step
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
