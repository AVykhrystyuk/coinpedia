#!/bin/bash

# this script runs the solution in one step
# the main purpose of this script is to make sure we have .env before running docker-compose
# after the first run, it might be beneficial to use docker-compose up directly


if [ ! -f .env ]; then
  echo "Creating .env from .env.example..."
  cp .env.example .env
else
  echo ".env already exists"
fi

docker-compose up --build
