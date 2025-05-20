# Multi Tenant Behavior

Multi-Tenant behaviors

## TenantFilterBehavior

The `TenantFilterBehavior` is a behavior that automatically applies a tenant filter to queries, ensuring that only data belonging to the current tenant is accessible. This is essential for multi-tenant applications, where data isolation between tenants must be enforced. When this behavior is applied, it appends a filter condition (typically using a `TenantId` property) to all queries, so that only entities associated with the active tenant are returned. This helps maintain data security and separation without requiring manual filtering in every query.

To enable this behavior, your entities should implement the `IHaveTenant<TKey>` interface.  

- `IHaveTenant<TKey>` is used for entities that need to be associated with a tenant by including a `TenantId` property.  
When the `TenantFilterBehavior` is applied, it will automatically filter entities based on the current tenant for all queries involving entities that implement this interface.

## TenantAuthenticateCommandBehavior

The `TenantAuthenticateCommandBehavior` is a behavior that ensures command operations are performed within the context of the correct tenant. It validates that the tenant information provided in a command matches the current tenant context, helping to prevent cross-tenant data access or modification. This behavior is essential for maintaining strict tenant isolation and security in multi-tenant applications, as it enforces that commands cannot inadvertently or maliciously affect data belonging to another tenant.

## TenantDefaultCommandBehavior

The `TenantDefaultCommandBehavior` is a behavior that automatically sets the tenant identifier (`TenantId`) on entities during create or update operations if it is not already specified. This ensures that new or modified entities are always associated with the current tenant context, reducing the risk of orphaned or misassigned data. By applying this behavior, you can enforce tenant assignment consistently across your application without requiring explicit handling in every command handler.
