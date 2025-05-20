# Audit Behavior

Behavior for automatically setting created and updated properties

## TrackChangeCommandBehavior<TEntityModel, TResponse>

The `TrackChangeCommandBehavior<TEntityModel, TResponse>` behavior intercepts create, update, and delete commands to record metadata such as who performed the operation and when it occurred. This behavior ensures that audit fields like `CreatedBy`, `CreatedAt`, `UpdatedBy`, and `UpdatedAt` are consistently populated without requiring manual intervention in each command handler. It is useful for maintaining a reliable audit trail across your application.

To enable this behavior, your entities should implement the `ITrackCreated` and/or `ITrackUpdated` interfaces.  

- `ITrackCreated` is used for entities that need to record who created them and when (`CreatedBy`, `CreatedAt`).
- `ITrackUpdated` is used for entities that need to record who last updated them and when (`UpdatedBy`, `UpdatedAt`).

When the `TrackChangeCommandBehavior<TEntityModel, TResponse>` is applied, it will automatically set these properties on entities that implement the respective interfaces during create and update operations.

## Registering Behavior

To use `TrackChangeCommandBehavior<TEntityModel, TResponse>`, register it in your service configuration using the `AddEntityCommands` extension method. This ensures the behavior is applied to relevant entity commands.

### Example

````csharp
services.AddEntityCommands<Context.TrackerContext, Entities.Priority, int, Models.PriorityReadModel, Models.PriorityCreateModel, Models.PriorityUpdateModel>();
````
