:: this script runs the solution in one step
:: the main purpose of this script is to make sure we have .env before running docker-compose
:: after the first run, it might be beneficial to use docker-compose up directly

@echo off

IF NOT EXIST .env (
    echo Creating .env from .env.example
    copy .env.example .env >nul
) ELSE (
    echo .env already exists
)

docker-compose up --build
