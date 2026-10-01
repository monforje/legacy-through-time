# syntax=docker/dockerfile:1
# QR code generator for `task qr`.
FROM alpine:3.22
RUN apk add --no-cache libqrencode-tools
WORKDIR /work
ENTRYPOINT ["qrencode"]
