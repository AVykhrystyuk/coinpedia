# Coinpedia

## Compile and run solution in one step (recommended)
**Prerequisite:** Docker
```bash
./run
```
This script makes sure .env exists and then runs docker-compose.
After the first run, it might be beneficial to use docker-compose up directly.

## Compile and run solution in few steps
```bash
# make sure .env exists in the solution folder, take .env.example as an example
copy .env.example .env >nul
```

TODO: