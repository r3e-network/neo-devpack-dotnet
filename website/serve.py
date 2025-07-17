#!/usr/bin/env python3
"""
Simple HTTP server to preview the R3E DevPack website locally
"""

import http.server
import socketserver
import os
import sys

PORT = 8000

class MyHTTPRequestHandler(http.server.SimpleHTTPRequestHandler):
    def end_headers(self):
        # Add CORS headers
        self.send_header('Access-Control-Allow-Origin', '*')
        self.send_header('Access-Control-Allow-Methods', 'GET, POST, OPTIONS')
        self.send_header('Access-Control-Allow-Headers', 'Content-Type')
        super().end_headers()

    def do_GET(self):
        # Serve index.html for directory requests
        if self.path.endswith('/'):
            self.path += 'index.html'
        return super().do_GET()

os.chdir(os.path.dirname(os.path.abspath(__file__)))

print(f"Starting R3E DevPack website server...")
print(f"Server running at http://localhost:{PORT}")
print(f"Press Ctrl+C to stop")

with socketserver.TCPServer(("", PORT), MyHTTPRequestHandler) as httpd:
    try:
        httpd.serve_forever()
    except KeyboardInterrupt:
        print("\nServer stopped.")
        sys.exit(0)