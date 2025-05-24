# M9Studio.SecureStream

Encrypted session abstraction with TLS 1.3-style handshake and AES-GCM transport encryption.

[![NuGet](https://img.shields.io/nuget/v/M9Studio.SecureStream.svg)](https://www.nuget.org/packages/M9Studio.SecureStream)
[![License: Apache-2.0](https://img.shields.io/badge/license-Apache--2.0-blue.svg)](https://www.apache.org/licenses/LICENSE-2.0)

## Features

* Pluggable transport adapter (`ISecureTransportAdapter<TAddress>`) to abstract over sockets, in-memory channels, etc.
* X25519-based handshake and key agreement
* AES-GCM symmetric encryption with integrity and confidentiality
* Agnostic to transport and address types (can use `IPEndPoint`, `int`, etc.)
* Designed to resemble lightweight TLS tunnel

## Installation

```bash
dotnet add package M9Studio.SecureStream
```

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

var session2 = manager.Connect(remoteAddress);
session2.Send(Encoding.UTF8.GetBytes("Hello securely"));
var response2 = session2.Receive();
Console.WriteLine("Decrypted: " + Encoding.UTF8.GetString(response2));
```

## Interface

### ISecureTransportAdapter<TAddress>

This interface abstracts the transport layer for sending and receiving encrypted data. You can implement it over any transport: UDP, TCP, or even in-process queues.

```csharp
public interface ISecureTransportAdapter<TAddress>
{
    event Action<TAddress> OnConnected;
    event Action<TAddress> OnDisconnected;

    bool SendTo(byte[] buffer, TAddress remote);
    byte[] ReceiveFrom(TAddress remote);
}
```

* `OnConnected`: triggered when a remote peer becomes reachable (e.g., after initial handshake packet is received)
* `OnDisconnected`: triggered when a remote peer is no longer available or manually closed
* `SendTo`: sends a raw encrypted packet to the given address
* `ReceiveFrom`: blocks until a packet is received from the specified address
