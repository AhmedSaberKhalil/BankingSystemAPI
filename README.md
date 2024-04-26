# Asp.Net Core Banking System Web API Using Unit Of work Pattern.

## overview
#### this project is a simple banking system web API using unit of work with crud operation, I used solid principles  and design patterns as I need.

#### ASP.NET Identity which is a membership system for adding login functionality to web applications. It provides APIs for user authentication, authorization, and user management. ASP.NET Identity allows you to manage users, roles, claims, and external logins, It includes features like user registration, login/logout, password management and account confirmation.

#### JWT is a token-based authentication mechanism. When a user logs in, the server generates a JWT token containing user information (claims) and sends it back to the client.

#### Rate limiting is a strategy used in software development and web services to control the rate of incoming requests from clients or users. It helps to prevent abuse, protect against denial-of-service (DoS) attacks, ensure fair usage of resources, and maintain system stability and performance.

#### Dependency Injection (DI) is a design pattern and a technique used in software engineering, particularly in object-oriented programming, to achieve loose coupling between components and to improve code maintainability, testability, and scalability.

#### Response Caching: This is used to cache the responses of HTTP requests. Response caching reduces the number of requests a client or proxy makes to a web server and reduces the amount of work the web server performs to generate a response. Response caching is configured in ASP.NET Core using the ResponseCachingMiddleware. The ResponseCacheAttribute defines cache parameters for an action method, and the caching ResponseCachingMiddleware middleware can handle cache headers for requests and store responses accordingly.

### Custom Validation  to validate the user input data, such as Email  and User Name. 

## Solid Principle and Some design Patterns.

## Unit of Work :
### Unit of Work is like a business transaction. This pattern will merge all CRUD transactions of Repositories into a single transaction. All changes will be committed only once. The main advantage is that application layer will need to know only one class (Unit of Work) to access each repository.

## Repository :
### Repositories are classes which implements data access logic. A repository represents a data entity, common CRUD operations and other special cases. The application layers consumes the APIs provided by the repository and does not need to care about how is implemented.

## Generic Repository :
### In each repository we will have to write the same CRUD operations, delete entity by id, get entity by id, and so on. In order to avoid copy, paste and try to follow DRY principle (Don’t Repeat Yourself) comes in handy Generic Repository pattern, in this class will be implemented all CRUD operations which will be used by all repository’s.

## Implementation :
### 1. Web API- will store the controllers and the logic (application layer).
### 2. Domain-will store the models’ classes and interfaces know by Web API project.
### 3. DataAccessEF- will contain the layer where all the content related with Entity Frameworks is implemented.
