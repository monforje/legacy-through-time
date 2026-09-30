# syntax=docker/dockerfile:1
# inklecate (Ink compiler) as a tool image. Used by `task ink`.
# Bump INK_VERSION and INK_SHA256 together: https://github.com/inkle/ink/releases
ARG INK_VERSION=1.2.1
ARG INK_SHA256=1997ff5bca618c90003ecd5fecb286e7468abb955005a2a185042949642f8fb5

FROM alpine:3.22 AS fetch
ARG INK_VERSION
ARG INK_SHA256
ADD --checksum=sha256:${INK_SHA256} https://github.com/inkle/ink/releases/download/v${INK_VERSION}/inklecate_linux.zip /ink.zip
RUN unzip /ink.zip -d /ink && chmod +x /ink/inklecate

# Self-contained .NET binary: needs only the native runtime deps.
FROM mcr.microsoft.com/dotnet/runtime-deps:8.0-bookworm-slim
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1 \
    HOME=/tmp
COPY --from=fetch /ink/ /opt/inklecate/
WORKDIR /work
ENTRYPOINT ["/opt/inklecate/inklecate"]
