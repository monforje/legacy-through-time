# syntax=docker/dockerfile:1
# UI asset tooling: python + librsvg (SVG -> PNG) + fonttools (static font weights).
# Used by `task assets:png` and `task assets:fonts`.
FROM python:3.13-alpine3.22
RUN apk add --no-cache rsvg-convert && pip install --no-cache-dir fonttools
WORKDIR /work
