#!/bin/bash

if [ -z "$STRAVA_ACCESS_TOKEN" ]; then
    echo "STRAVA_ACCESS_TOKEN is not set"
    exit 1
fi

curl -H "Authorization: Bearer $STRAVA_ACCESS_TOKEN" https://www.strava.com/api/v3/athlete