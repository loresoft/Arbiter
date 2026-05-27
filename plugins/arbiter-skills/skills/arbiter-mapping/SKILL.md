---
name: arbiter-mapping
description: Use when defining or registering Arbiter.Mapping source-generated mappers — [GenerateMapper] attribute, partial class deriving from MapperProfile<TSource, TDestination>, ConfigureMapping with MappingBuilder, IMapper / IMapper<TSrc,TDst>, ProjectTo for IQueryable, ServiceProviderMapper registration.
---

# Arbiter.Mapping

Roslyn incremental source generator that emits object-to-object mapping code at build time. Zero reflection, AOT-friendly, supports records, init-only properties, primary constructors, and `IQueryable` projection.

## Install

```bash
dotnet add package Arbiter.Mapping
```

The generator (`Arbiter.Mapping.Generators`) is referenced transitively. Mapping classes must be `partial` so the generator can emit the implementation.

## Canonical pattern — one mapper per direction

```csharp
using Arbiter.Mapping;

[GenerateMapper]
public partial class UserToUserDtoMapper : MapperProfile<User, UserDto>
{
    protected override void ConfigureMapping(MappingBuilder<User, UserDto> mapping)
    {
        mapping.Property(d => d.FullName)       .From(s => s.FirstName + " " + s.LastName);
        mapping.Property(d => d.Age)            .From(s => DateTime.Now.Year - s.BirthDate.Year);
        mapping.Property(d => d.DepartmentName) .From(s => s.Department!.Name);
        mapping.Property(d => d.AddressCount)   .From(s => s.Addresses.Count());
        // Properties with matching names + compatible types are mapped automatically.
    }
}
```

If no overrides are needed, leave `ConfigureMapping` empty (or omit it) — the generator still emits the auto-property mapping.

## Register

Register each closed mapper plus the generic dispatcher:

```csharp
services.AddSingleton<IMapper<User, UserDto>, UserToUserDtoMapper>();
services.AddSingleton<IMapper, ServiceProviderMapper>();
```

`ServiceProviderMapper` implements the non-generic `IMapper` and looks up the right `IMapper<TSrc, TDst>` on demand.

## Use

```csharp
public class UserService
{
    private readonly IMapper<User, UserDto> _mapper;
    public UserService(IMapper<User, UserDto> mapper) => _mapper = mapper;

    public UserDto ToDto(User user) => _mapper.Map(user);
}

// Or via the generic IMapper:
UserDto dto = mapper.Map<User, UserDto>(user);
```

## IQueryable projection

```csharp
IQueryable<User> users = db.Users.Where(u => u.IsActive);

// Translates property selection into the underlying query (EF Core, etc.)
IQueryable<UserDto> dtos = users.ProjectTo<User, UserDto>(mapper);

var list = await dtos.ToListAsync(ct);
```

## Mapping into records / init-only / primary ctors

Supported out of the box — name the parameter or `init` property to match the source property, or add a `mapping.Property(...).From(...)` override.

```csharp
public record UserDto(int Id, string FullName, int Age);

[GenerateMapper]
public partial class UserToUserDtoMapper : MapperProfile<User, UserDto>
{
    protected override void ConfigureMapping(MappingBuilder<User, UserDto> m)
    {
        m.Property(d => d.FullName).From(s => s.FirstName + " " + s.LastName);
        m.Property(d => d.Age).From(s => DateTime.Now.Year - s.BirthDate.Year);
    }
}
```

## Notes

- Mappers must be `partial` and derive from `MapperProfile<TSrc, TDst>`.
- One mapper per direction — define a second `partial class DtoToUserMapper : MapperProfile<UserDto, User>` if you need the reverse.
- Register one mapper per direction; `ServiceProviderMapper` is the single non-generic entry point.

## Reference

- Mapping guide: https://github.com/loresoft/Arbiter/tree/main/docs/guide/mapping
- Code generation: https://github.com/loresoft/Arbiter/blob/main/docs/guide/codeGeneration.md
