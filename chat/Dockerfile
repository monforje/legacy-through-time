# The hero chat server. Built on tender from the slice of the repo `task chat:deploy` sends (the paths of the
# repo are kept, rag/config.py finds the content by them); the index and the logs are the mounted chat/data.
FROM python:3.13-alpine
COPY --from=ghcr.io/astral-sh/uv:0.12.22 /uv /bin/uv
WORKDIR /app/chat
ENV UV_COMPILE_BYTECODE=1 UV_LINK_MODE=copy UV_PYTHON_DOWNLOADS=never
COPY chat/pyproject.toml chat/uv.lock ./
RUN uv sync --frozen --no-dev --no-install-project
COPY content/chat /app/content/chat
# a story with a chat is one more line here and in `task chat:deploy`
COPY content/stories/golden-cage/chat /app/content/stories/golden-cage/chat
COPY Assets/Resources/Fonts /app/Assets/Resources/Fonts
COPY docs /app/docs
COPY chat/rag rag
COPY chat/web web
RUN adduser -D -u 1000 chat && mkdir -p data && chown chat data
USER chat
EXPOSE 8765
CMD [".venv/bin/uvicorn", "rag.server:app", "--host", "0.0.0.0", "--port", "8765", "--proxy-headers", "--forwarded-allow-ips", "*"]
