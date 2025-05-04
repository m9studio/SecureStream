# M9Studio.SecureStream

Encrypted session abstraction with TLS 1.3-style handshake and AES-GCM transport encryption.

[![NuGet](https://img.shields.io/nuget/v/SecureStream.svg)](https://www.nuget.org/packages/SecureStream)
[![License: Apache-2.0](https://img.shields.io/badge/license-Apache--2.0-blue.svg)](https://www.apache.org/licenses/LICENSE-2.0)

## Features

- Pluggable transport adapter (`ISecureTransportAdapter<TAddress>`)
- X25519-based handshake and key agreement
- AES-GCM symmetric encryption
- Agnostic to transport and address types (can use `IPEndPoint`, `int`, etc.)
- Designed to resemble lightweight TLS tunnel

## Installation

```bash
dotnet add package SecureStream
```

Or view on NuGet:  
[https://www.nuget.org/packages/SecureStream](https://www.nuget.org/packages/SecureStream)

## Usage

### Setup and connection

```csharp
var adapter = new MyTransportAdapter();
var manager = new SecureChannelManager<MyAddressType>(adapter);

manager.OnSecureSessionEstablished += session =>
{
    session.Send(Encoding.UTF8.GetBytes("Hello securely"));
    var response = session.Receive();
    Console.WriteLine("Decrypted: " + Encoding.UTF8.GetString(response));
};

manager.Connect(remoteAddress);
```

## Interface

### ISecureTransportAdapter<TAddress>

```csharp
public interface ISecureTransportAdapter<TAddress>
{
    event Action<TAddress> OnConnected;
    event Action<TAddress> OnDisconnected;

    bool SendTo(byte[] buffer, TAddress remote);
    byte[] ReceiveFrom(TAddress remote);
}
```

## License

This project is licensed under the [Apache License 2.0](https://www.apache.org/licenses/LICENSE-2.0).

## Repository

GitHub: [https://github.com/m9studio/SecureStream](https://github.com/m9studio/SecureStream)

## NuGet Author

Published on NuGet by: [mina987](https://www.nuget.org/profiles/M9Studio)
