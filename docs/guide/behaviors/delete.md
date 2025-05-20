# Soft Delete Behavior

A base behavior for appending soft delete (IsDeleted) filter

## DeletedFilterBehavior

The `DeletedFilterBehavior` is a behavior that automatically filters out entities marked as deleted by applying a soft delete filter (typically using an `IsDeleted` property). When this behavior is applied, queries will exclude entities where `IsDeleted` is set to `true`, ensuring that soft-deleted records are not returned in standard query results. This allows you to implement soft delete functionality without modifying every query manually, making it easier to maintain and enforce consistent data access rules across your application.

To enable this behavior, your entities should implement the `ITrackDeleted` interface.  

- `ITrackDeleted` is used for entities that need to support soft deletion by including an `IsDeleted` property.  
When the `DeletedFilterBehavior` is applied, it will automatically filter out entities where `IsDeleted` is `true` for all queries involving entities that implement this interface.
