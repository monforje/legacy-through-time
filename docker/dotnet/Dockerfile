# syntax=docker/dockerfile:1
# .NET SDK + inklecate for the narrative engine tests. Used by `task test`.
# The official ink runtime (ink-engine-runtime.dll) shipped with inklecate is the
# reference oracle for differential tests; it is never linked into the game.
FROM legacy-through-time/inklecate AS ink

FROM mcr.microsoft.com/dotnet/sdk:8.0
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    DOTNET_NOLOGO=1 \
    DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1 \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1 \
    HOME=/tmp \
    INK_DIR=/opt/inklecate
COPY --from=ink /opt/inklecate/ /opt/inklecate/
WORKDIR /work
