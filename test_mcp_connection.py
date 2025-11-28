import asyncio
import websockets
import json
import sys

async def test_mcp(path=""):
    uri = f"ws://localhost:8090{path}"
    print(f"Connecting to {uri}...")
    try:
        async with websockets.connect(uri) as websocket:
            print("Connected!")
            
            # 1. Send initialize
            init_msg = {
                "jsonrpc": "2.0",
                "id": 1,
                "method": "initialize",
                "params": {
                    "protocolVersion": "2024-11-05",
                    "capabilities": {},
                    "clientInfo": {
                        "name": "gemini-cli-client",
                        "version": "1.0.0"
                    }
                }
            }
            await websocket.send(json.dumps(init_msg))
            print(f"Sent initialize")
            
            response = await websocket.recv()
            print(f"Received initialize response: {response}")
            
            # 2. Send initialized notification
            await websocket.send(json.dumps({
                "jsonrpc": "2.0",
                "method": "notifications/initialized"
            }))
            print("Sent initialized")

            # 3. List tools
            tools_msg = {
                "jsonrpc": "2.0",
                "id": 2,
                "method": "tools/list"
            }
            await websocket.send(json.dumps(tools_msg))
            print("Sent tools/list")
            
            response = await websocket.recv()
            print(f"Received tools: {response}")
            return True
            
    except Exception as e:
        print(f"Error connecting to {uri}: {e}")
        return False

async def main():
    paths = ["", "/ws", "/mcp", "/unity"]
    for path in paths:
        if await test_mcp(path):
            print(f"SUCCESS with path: '{path}'")
            return
    print("All paths failed.")

if __name__ == "__main__":
    try:
        asyncio.run(main())
    except ImportError:
        print("websockets library not found.")