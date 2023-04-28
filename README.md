# netutil - Network utilities for dotnet

- Implemented as a console application but with public classes for use within another application.
- Also implemented as a dotnet global tool for eway deployment.

## Installation

- `dotnet tool install --global netutil`

## Features

- Proxy Server
- Echo Server
- Receive Client

## Echo Server

Receives data sent by the client and echoes the same data back to the to client. Allows multiple client connections.
Currently only supports TCP, but future `-p` `--protocol` switch will allow the use of UDP.

Example usage from command line: `netutil echo-server -b 0.0.0.0:6340 -d -f AsciiText -e log.csv`

- `-b` or `--bind`: Bind to end point
- `-d` or `--display-data`: Display data in console
- `-f` or `--format`: Data format (`Binary` | `AsciiText` | `Utf8Text`) - for rendering in console and event log file
- `-e` or `--event-log-file`: Log events (including data) to file

## Proxy Server

Receives data sent by the client and forwards to another server and vice-versa (bi-directional). Allows multiple client connections.
Currently only supports TCP, but future `-p` `--protocol` switch will allow the use of UDP.

Example usage from command line: `netutil proxy-server -b 0.0.0.0:6341 -c 127.0.0.1:6340 -d -f AsciiText -e log.csv`

- `-b` or `--bind`: Bind to end point
- `-c` or `--connect`: Connect to end point
- `-d` or `--display-data`: Display data in console
- `-f` or `--format`: Data format (`Binary` | `AsciiText` | `Utf8Text`) - for rendering in console and event log file
- `-e` or `--event-log-file`: Log events (including data) to file

## Client Receive

Receives data from a server.
Currently only supports TCP, but future `-p` `--protocol` switch will allow the use of UDP.

Example usage from command line: `netutil receive-client -c 127.0.0.1:6340 -d -f AsciiText -e log.csv`

- `-c` or `--connect`: Connect to end point
- `-d` or `--display-data`: Display data in console
- `-f` or `--format`: Data format (`Binary` | `AsciiText` | `Utf8Text`) - for rendering in console and event log file
- `-e` or `--event-log-file`: Log events (including data) to file

# TODO

- Need to add connection timeouts as internal reader tasks stay running indefinitely