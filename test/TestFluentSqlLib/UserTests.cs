using Microsoft.Data.SqlClient;

namespace TestProject1;

[Collection("SharedLocalDb")]
public class UsersTests(LocalDbFixture db)
{
    [Fact]
    public async Task InsertUser_Works()
    {
        using var conn = new SqlConnection(db.ConnectionString);
        await conn.OpenAsync();

        using var cmd = new SqlCommand("CREATE TABLE Users(Id INT PRIMARY KEY, Name NVARCHAR(50));", conn);
        await cmd.ExecuteNonQueryAsync();

        using var insert = new SqlCommand("INSERT INTO Users VALUES (1,'Tom');", conn);
        await insert.ExecuteNonQueryAsync();
    }
}
