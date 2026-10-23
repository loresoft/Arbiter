namespace Arbiter.Components.Services;

/// <summary>
/// Determines whether the current user has permission to change the value of a data bound component such as
/// <see cref="EntitySelect{TModel, TValue}"/> or <see cref="EntityTypeahead{TItem, TValue}"/>.
/// </summary>
/// <remarks>
/// <para>
/// Register an implementation with <see cref="ComponentServiceExtensions.AddPermissionResolver{TResolver}"/>
/// and every component that supports permissions is disabled automatically when the user is denied access, without
/// supplying a <c>PermissionResolver</c> parameter on each instance. When no implementation is registered,
/// <see cref="DefaultPermissionResolver"/> grants access to everything.
/// </para>
/// <para>
/// The resolver is injected into each component, so implementations can take scoped dependencies such
/// as an authorization or privilege service. Results are cached by the component for its lifetime, so the resolver is
/// called once per component instance; implementations that are expensive should cache internally. An exception is
/// reported through <see cref="INotificationService"/> and leaves the component disabled.
/// </para>
/// <para>
/// The components default the arguments when not supplied: the action is <c>update</c>, the subject is the type name
/// of the <c>EditContext</c> model and the field is the name of the bound field. When no subject can be determined,
/// the resolver is not called and access is granted.
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
    /// A <see cref="ValueTask{TResult}"/> that resolves to <see langword="true"/> if the user is allowed;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    ValueTask<bool> HasPermission(string action, string subject, string? field);
}
