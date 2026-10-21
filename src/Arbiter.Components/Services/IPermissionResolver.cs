namespace Arbiter.Components.Services;

/// <summary>
/// Determines whether the current user has permission to change the value of a data bound component such as
/// <see cref="EntitySelect{TModel, TValue}"/> or <see cref="EntityTypeahead{TItem, TValue}"/>.
/// </summary>
/// <remarks>
/// <para>
/// Register an implementation with <see cref="ComponentServiceExtensions.AddPermissionResolver{TResolver}"/>
/// and every component that supports permissions is disabled automatically when the user is denied access, without
/// supplying a <c>PermissionResolver</c> parameter on each instance.
/// </para>
/// <para>
/// The resolver is created from the component's service scope, so implementations can take scoped dependencies such
/// as an authorization or privilege service. Results are cached by the component for its lifetime, so the resolver is
/// called once per component instance. An exception is reported through <see cref="INotificationService"/> and leaves
/// the component disabled.
/// </para>
/// <para>
/// A <c>PermissionResolver</c> parameter supplied on a component takes priority over this service, and an explicit
/// <c>Disabled</c> value of <see langword="true"/> skips the check entirely.
/// </para>
/// </remarks>
public interface IPermissionResolver
{
    /// <summary>
    /// Determines whether the current user is allowed to perform <paramref name="action"/> on the specified
    /// <paramref name="subject"/> and <paramref name="field"/>.
    /// </summary>
    /// <param name="action">The action to check, such as <c>update</c>.</param>
    /// <param name="subject">The subject being acted on, by default the type name of the form model.</param>
    /// <param name="field">The name of the bound field, or <see langword="null"/> when the component is not bound to a field.</param>
    /// <returns>
    /// <see langword="true"/> if the user is allowed; otherwise, <see langword="false"/>.
    /// </returns>
    ValueTask<bool> HasPermission(string action, string subject, string? field);
}
