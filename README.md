# testcontainers-net-runner

This repository provides a simple example of how to use [Testcontainers](https://github.com/testcontainers/testcontainers-dotnet) in .NET to run integration tests against a PostgreSQL database.

## Features
* **Docker requirement checks:** Includes attributes (`DockerRequiredTheory`, `DockerRequiredFact`) to skip tests if Docker is not available ([Testcontainers](https://github.com/testcontainers/testcontainers-dotnet) needs Docker to be able to start on the test running machine).
* **Easy setup:** Provides a helper class (`TestContainers`) to manage the lifecycle of a PostgreSQL container: **only one container for all the tests**.
* **Custom initialization:** Allows you to initialize the database with a custom SQL script (`init.sql`).
* **Automatic container startup:** Ensures the container is running before tests are executed.


## Usage

1. **Install the NuGet package:**
    * `donet add package Testcontainers.PostgreSql`

2. **Create a `DbContext`:**
    
    Define your `DbContext` and entities as needed. See the [SimpleTest.cs](Tests/SimpleTest.cs) file for an example.

3. **Use the `TestContainers` class:**

    * `GetPostgresContainer()`:  Returns a `PostgreSqlContainer` instance.
    * `GetConnectionString()`:  Starts the container (if not started yet) and returns the connection string for the PostgreSQL container.

4. **Apply the `DockerRequiredTheory` or `DockerRequiredFact` attribute:**

    Use these attributes on your test methods to ensure that Docker is available before running the tests.

## Example

```csharp
using Tests;
using Microsoft.EntityFrameworkCore;
// ... other imports

public class SimpleTest : IDisposable
{    
    public SimpleTest()
    {
        var context = // ... create your EF context here
        context.Database.EnsureCreated(); // to create a table automatically
        context.Database.BeginTransaction(); // every tests runs in its isolated transaction
    }

    public void Dispose() {
        userContext.Database.RollbackTransaction(); // rollback test transation like nothing happened
    }

 [DockerRequiredTheory] 
 [InlineData(10)]
 public void UsingTestContainers_CreateAndReturnsNRecords(int recordsNum) 
 {
     // ... your test logic using the PostgreSQL container ...
 }
}
```