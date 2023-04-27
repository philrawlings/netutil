# netutil - Network utilities for dotnet

- Implemented as a console application but with public classes for use within another application.

## Features

- [x] TCP Proxy
- [ ] UDP Proxy
- [x] TCP Receiver

## TCP Proxy

Listens on a specified port and forwards data to destination address and vice-versa (bi-directional).

Example usage from command line: `netutil proxy -b 0.0.0.0:6341 -c 127.0.0.1:6340 -w -f AsciiText`

- `-b` = Bind to end point
- `-c` = Connect to end point
- `-w` = Write data to console
- `-f` = Data format (`Binary` | `AsciiText` | `Utf8Text`) - for rendering in console.

## TCP Receiver

Example usage from command line: `netutil receive -c 127.0.0.1:6340 -w -f AsciiText`

- `-c` = Connect to end point
- `-w` = Write data to console
- `-f` = Data format (`Binary` | `AsciiText` | `Utf8Text`) - for rendering in console.

# TODO

- Logging to file not currently implemented, but is planned.
- Probably need to add connection timeout as internal tasks stay running indefinitely