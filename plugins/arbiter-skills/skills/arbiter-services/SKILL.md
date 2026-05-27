---
name: arbiter-services
description: Use when the user needs Arbiter.Services utilities — CSV reading/writing (CsvReader / CsvWriter), AES or XOR encryption, cache key building (CacheTagger), continuation tokens, numeric encoding, glob matching, URL building (UrlBuilder, QueryStringEncoder), or secure token generation (TokenService).
---

# Arbiter.Services

Grab-bag of small zero-dependency utilities used across Arbiter. There are no DI extension methods — instantiate the types directly or register them yourself as needed.

## Install

```bash
dotnet add package Arbiter.Services
```

## What's in the box

| Type | Purpose |
| --- | --- |
| `CsvReader` / `CsvWriter` | Streaming CSV parse/write, RFC-4180-compatible, optional headers |
| `AesEncryption` | Symmetric AES encrypt/decrypt with key + IV |
| `XorEncryption` | Lightweight XOR obfuscation (not for sensitive data) |
| `CacheTagger` | Build consistent cache keys + tags from arbitrary values |
| `ContinuationToken` | Opaque token encoder for cursor-paged queries |
| `NumericEncoder` | Base-N encoding for short numeric IDs |
| `GlobMatcher` | Wildcard (`*`, `?`) path/string matching |
| `TokenService` | Cryptographically secure random tokens |
| `UrlBuilder` / `QueryStringEncoder` | Fluent URL construction + query string encoding |
| `ObjectPool` + extensions | Lightweight per-thread object pooling |
| `ValueStringBuilder`, `TypeBuffer` | Stack-allocated builders for hot paths |

## CSV

```csharp
using Arbiter.Services;
using var reader = new CsvReader(File.OpenText("orders.csv"));
foreach (var row in reader.ReadAll())     // IEnumerable<string[]>
{
    var id   = row[0];
    var name = row[1];
}

using var writer = new CsvWriter(File.CreateText("out.csv"));
writer.WriteHeader("Id", "Name");
writer.WriteRow("1", "Widget");
```

## AES encryption

```csharp
var aes = new AesEncryption(key: keyBytes, iv: ivBytes);
string cipher = aes.Encrypt("secret");
string plain  = aes.Decrypt(cipher);
```

## URL builder

```csharp
string url = new UrlBuilder("https://api.example.com/users")
    .AppendPath("42", "orders")
    .AddQuery("status", "open")
    .AddQuery("page", 2)
    .ToString();
// → https://api.example.com/users/42/orders?status=open&page=2
```

## Cache key + tags

```csharp
var key = CacheTagger.Build("product", productId, version);
var tag = CacheTagger.Tag("product", productId);
```

## Secure token

```csharp
string token = TokenService.Create(byteLength: 32);   // URL-safe base64
```

## Continuation token (cursor paging)

```csharp
string token = ContinuationToken.Encode(new { LastId = 1234, Sort = "Name" });
var cursor = ContinuationToken.Decode<MyCursor>(token);
```

## Notes

- Most types are `sealed` / stack-friendly; allocate per use.
- Symmetric encryption helpers are convenience wrappers — for production secrets prefer Azure Key Vault / Data Protection.

## Reference

- Source: https://github.com/loresoft/Arbiter/tree/main/src/Arbiter.Services
