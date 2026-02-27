# Copilot Instructions

## Project Guidelines
- When async variants are requested, include both Task and ValueTask overloads and include a CancellationToken parameter (default CancellationToken.None) in the method signatures.
- Follow KISS (Keep It Simple, Stupid) principles in your code.
- Follow the DRY (Don't Repeat Yourself) principle to avoid code duplication.
- Follow the YAGNI (You Aren't Gonna Need It) principle to avoid over-engineering and unnecessary features.
- Follow SOLID principles to ensure that the code is maintainable and scalable.
- Follow the principle of least surprise to ensure that the code is intuitive and easy to understand.
- Follow the principle of separation of concerns to ensure that different parts of the code are responsible for different tasks.
- Follow the principle of single responsibility to ensure that each class or method has a single responsibility.
- Follow the principle of open/closed to ensure that the code is open for extension but closed for modification.
- Follow the principle of Liskov substitution to ensure that derived classes can be substituted for their base classes without affecting the correctness of the program.
- Follow the principle of interface segregation to ensure that clients are not forced to depend on interfaces they do not use.
- Follow the principle of dependency inversion to ensure that high-level modules do not depend on low-level modules, but both depend on abstractions.
- Follow the command query separation principle to ensure that methods are either commands (which perform an action) or queries (which return data), but not both.
- Follow the fail fast principle to ensure that the code fails as early as possible when an error occurs, rather than allowing the error to propagate and cause more complex issues later on.
- Follow Microsoft's .NET coding conventions for naming, formatting, and other style guidelines to ensure consistency and readability in the codebase.
- Follow Microsoft's .NET API design guidelines to ensure that the code is designed in a way that is consistent with other .NET APIs and is easy to use for developers familiar with the .NET ecosystem.
- Follow Microsoft's .NET asynchronous programming guidelines to ensure that async code is written in a way that is efficient, responsive, and easy to understand for developers familiar with the .NET ecosystem.
- Follow Microsoft's .NET performance guidelines to ensure that the code is optimized for performance and does not introduce unnecessary overhead or inefficiencies.
- Follow Microsoft's .NET security guidelines to ensure that the code is secure and does not introduce vulnerabilities or security risks.
- Follow Microsoft's .NET testing guidelines to ensure that the code is testable and that tests are written in a way that is effective and maintainable.
- Follow Microsoft's .NET documentation guidelines to ensure that the code is well-documented and that documentation is clear, concise, and easy to understand for developers familiar with the .NET ecosystem.
- Follow Microsoft's .NET error handling guidelines to ensure that errors are handled in a way that is consistent with other .NET APIs and is easy to understand for developers familiar with the .NET ecosystem.
- Follow Microsoft's .NET logging guidelines to ensure that logging is implemented in a way that is consistent with other .NET APIs and is easy to understand for developers familiar with the .NET ecosystem.
- Do not include dependencies outside of the .NET standard library unless explicitly requested.
- Follow dependency injection principles to ensure that dependencies are injected into classes rather than being hard-coded, to improve testability and maintainability.