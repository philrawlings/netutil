# netutil - Network utilities for dotnet

- Implemented as a console application but with public classes for use within another application.

## Features

- Proxy Server
- Echo Server
- Receive Client

## Echo Server

Receives data sent by the client and echoes the same data back to the to client. Allows multiple client connections.
Currently only supports TCP, but future `-p` `--protocol` switch will allow the use of UDP.

Example usage from command line: `netutil echo-server -b 0.0.0.0:6340 -w -f AsciiText`

- `-b` = Bind to end point
- `-w` = Write data to console
- `-f` = Data format (`Binary` | `AsciiText` | `Utf8Text`) - for rendering in console.

## Proxy Server

Receives data sent by the client and forwards to another server and vice-versa (bi-directional). Allows multiple client connections.
Currently only supports TCP, but future `-p` `--protocol` switch will allow the use of UDP.

Example usage from command line: `netutil proxy-server -b 0.0.0.0:6341 -c 127.0.0.1:6340 -w -f AsciiText`

- `-b` = Bind to end point
- `-c` = Connect to end point
- `-w` = Write data to console
- `-f` = Data format (`Binary` | `AsciiText` | `Utf8Text`) - for rendering in console.

## Client Receive

Receives data from a server.
Currently only supports TCP, but future `-p` `--protocol` switch will allow the use of UDP.

Example usage from command line: `netutil receive-client -c 127.0.0.1:6340 -w -f AsciiText`

- `-c` = Connect to end point
- `-w` = Write data to console
- `-f` = Data format (`Binary` | `AsciiText` | `Utf8Text`) - for rendering in console.

# TODO

- Logging to file not currently implemented, but is planned.
- Probably need to add connection timeout as internal tasks stay running indefinitely