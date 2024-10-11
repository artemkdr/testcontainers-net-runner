using Tests;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Test;

public class SimpleTest : IDisposable
{
    private static readonly DbContextOptions<BaseDBContext<User>> userContextOptions = 
            new DbContextOptionsBuilder<BaseDBContext<User>>()
                .UseNpgsql(TestContainers.GetConnectionString()).Options;
    
    private BaseDBContext<User> userContext;

    public SimpleTest()
    {
        userContext = new BaseDBContext<User>(userContextOptions);
        userContext.Database.EnsureCreated();
        userContext.Database.BeginTransaction();
    }
    
    public void Dispose() {
        userContext.Database.RollbackTransaction();            
    }

    [DockerRequiredTheory]    
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1000)]
    [InlineData(10000)]    
    public void UsingTestContainers_CreateAndReturnsNRecords(int recordsNum) {
        // Arrange
        for (int i = 0; i < recordsNum; i++) {
            userContext.Items.Add(
                new User() { 
                    Username = Guid.NewGuid().ToString(), 
                    Status = "Active", 
                    Vars = JsonDocument.Parse($"{{ \"count\": {i} }}"),
                    CreateDate = DateTime.UtcNow, 
                    ModifyDate = DateTime.UtcNow 
                });
        }
        userContext.SaveChanges();

        // Act
        int count = userContext.Items.Count();

        // Assert
        Assert.Equal(recordsNum, count);
    }

    [DockerRequiredFact]
    public void UsingTestContainers_CanFindRecordByKeyColumn() {        
        // Arrange
        userContext.Items.Add(
                new User() { 
                    Username = "user1"                    
                });

        // Assert
        Assert.True(userContext.Items.Find("user1") != null);
    }
}

public class BaseDBContext<T> : DbContext where T : class
{
    public BaseDBContext(DbContextOptions<BaseDBContext<T>> options) : base(options)
    {
    }

    public DbSet<T> Items { get; set; } 
}



[Table("users")]
public class User
{
    [Key] 
    [Column("username")]
    public string? Username { get; set; }
    
    [Column("password")]
    [JsonIgnore]
    public string? Password { get; set; }
    
    [Column("vars")]
    public JsonDocument? Vars { get; set; } 
    
    [Column("createdate")]
    public DateTime CreateDate { get; set; }
    
    [Column("modifydate")]
    public DateTime ModifyDate { get; set; }

    [Column("status", TypeName = "text")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public string? Status { get; set; }
}
