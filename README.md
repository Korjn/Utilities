# Korjn.Utilities

Korjn.Utilities is a utility library for .NET, providing lightweight helper tools, including a high-performance unique ID generator.

## 📦 Installation

Install the package from NuGet:

```bash
dotnet add package Korjn.Utilities
```
or via the NuGet Package Manager:
```bash
Install-Package Korjn.Utilities
```

## 📝 Usage example

```csharp
using Korjn.Utilities;

string uniqueId = IdGenerator.NewId();
Console.WriteLine($"Generated ID: {uniqueId}");
```

🔍 About IdGenerator
IdGenerator generates a unique 22-character base32 string identifier:

✅ Guaranteed uniqueness across machines
✅ Guaranteed uniqueness across processes on the same machine
✅ Guaranteed uniqueness across threads within a process
✅ Supports up to 16 million IDs per second per process
✅ Lock-free implementation (thread-safe without locking)

The ID is constructed from the following components:

timestamp (32 bits) — current Unix timestamp in seconds

machineId (32 bits) — unique machine identifier (hash of hostname)

pid (16 bits) — current process ID

increment (24 bits) — atomic increment counter

randomExtra (8 bits) — random byte generated once per process start

All combined into a 112-bit number encoded in base32 (22 characters).

📚 License
This project is licensed under the MIT License. See LICENSE for details.