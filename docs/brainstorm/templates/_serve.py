# Сервер макетов без кэша: правки CSS/JS видны сразу после обычного обновления страницы.
import http.server

class NoCache(http.server.SimpleHTTPRequestHandler):
    def end_headers(self):
        self.send_header("Cache-Control", "no-store")
        super().end_headers()

http.server.ThreadingHTTPServer(("", 8080), NoCache).serve_forever()
