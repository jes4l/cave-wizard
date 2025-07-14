import http.server
import socketserver

PORT = 8000

class CustomHandler(http.server.SimpleHTTPRequestHandler):
    def do_GET(self):
        # Set correct headers for .br files
        if self.path.endswith('.br'):
            self.send_header('Content-Encoding', 'br')
        if self.path.endswith('.js.br'):
            self.send_header('Content-Type', 'application/javascript')
        elif self.path.endswith('.data.br'):
            self.send_header('Content-Type', 'application/octet-stream')
        elif self.path.endswith('.wasm.br'):
            self.send_header('Content-Type', 'application/wasm')
        super().do_GET()

with socketserver.TCPServer(("", PORT), CustomHandler) as httpd:
    print(f"Serving at http://localhost:{PORT}")
    httpd.serve_forever()