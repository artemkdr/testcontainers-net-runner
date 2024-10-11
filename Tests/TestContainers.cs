using Testcontainers.PostgreSql;
using System.Diagnostics;

namespace Tests;

public class TestContainers
{
    public static string INIT_SQL_FILE = "init.sql";
    
    public static string POSTGRES_IMAGE = "postgres:15-alpine";

    private static readonly Lazy<PostgreSqlContainer> _lazyPosgresContainer = new Lazy<PostgreSqlContainer>(() =>
    {   
        var builder = new PostgreSqlBuilder().WithImage(POSTGRES_IMAGE);
        if (File.Exists(INIT_SQL_FILE)) 
            builder = builder.WithResourceMapping(new FileInfo(INIT_SQL_FILE), "/docker-entrypoint-initdb.d");        
        return builder.Build();        
    });

    public static PostgreSqlContainer GetPostgresContainer() => _lazyPosgresContainer.Value;

    public static string GetConnectionString() {
        var container = GetPostgresContainer();
        if (container.State != DotNet.Testcontainers.Containers.TestcontainersStates.Running) {
            container.StartAsync().Wait(new TimeSpan(0, 1, 0));        
        }
        return container.GetConnectionString();
    }
}


public class DockerRequiredTheory : TheoryAttribute
{
    public override string? Skip => DockerChecker.GetSkipReason();
}

public class DockerRequiredFact : FactAttribute
{
    public override string? Skip => DockerChecker.GetSkipReason();
}

public static class DockerChecker
{
    public static string? GetSkipReason()
    {            
        if (!IsDockerInstalled() || !IsDockerAvailable())
        {
            return "Docker is not installed or running.";
        }
        return null; // Don't skip the test            
    }

    private static bool? dockerAvailable = null;

    public static bool IsDockerAvailable()
    {
        if (dockerAvailable != null)
            return dockerAvailable.Value;
        try
        {
            var container = TestContainers.GetPostgresContainer();
            dockerAvailable = container != null;                
        } catch
        {
            dockerAvailable = false;
        }
        return dockerAvailable.Value;
    }

    private static bool? dockerInstalled = null;
    public static bool IsDockerInstalled()
    {
        if (dockerInstalled != null)
            return dockerInstalled.Value;
        try
        {
            Process.Start(new ProcessStartInfo("docker", "--version") { UseShellExecute = false })?.WaitForExit();
            dockerInstalled = true;
            return true;
        }
        catch (Exception)
        {
            dockerInstalled = false;
            return false;
        }
    } 
}