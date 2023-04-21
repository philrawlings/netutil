# netutil - Network utilities for dotnet

- Implemented as a console application but with public classes.

## Features

- [x] TCP Proxy server
- [ ] UDP Proxy Server

## TCP Proxy Server

Listens on a specified port and forwards data to destination address and vice-versa (bi-directional).

- Example usage from command line: `netutil proxy -b 0.0.0.0:6341 -d 127.0.0.1:6340 -c -f AsciiText`
- `-b` = Bind address
- `-d` = Destination address
- `-c` = Display captured data in console
- `-f` = Data format (`Binary` | `AsciiText` | `Utf8Text`) - for rendering in console.

Logging to file not currently implemented, but is planned.