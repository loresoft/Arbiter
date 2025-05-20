# Behavior Abstracts

Abstract base classes for behaviors

## PipelineBehaviorBase

The `PipelineBehaviorBase` is an abstract base class for implementing custom pipeline behaviors. It provides a foundation for creating behaviors that can intercept and process requests and responses as they pass through the application's pipeline. By inheriting from `PipelineBehaviorBase`, you can encapsulate cross-cutting concerns such as logging, validation, caching, or auditing, and apply them consistently across your application's command and query handling logic.
