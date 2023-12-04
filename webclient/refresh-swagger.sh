#!/usr/bin/env bash

set -e
curl --silent --fail --insecure https://localhost:5000/swagger/v1/swagger.yaml > swagger.yaml ||
 (>&2 echo "Failed to download swagger file, ensure the server is running!"; exit 1 )
npm run swagger
