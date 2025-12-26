using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestProject1;

[Collection("SharedLocalDb")]
public class ReadTests(LocalDbFixture db)
{
    [Fact]
    public async Task CanReadUsers()
    {
        using var conn = new SqlConnection(db.ConnectionString);
        await conn.OpenAsync();

        using var cmd = new SqlCommand("SELECT COUNT(*) FROM Users;", conn);
        var count = (int)await cmd.ExecuteScalarAsync();

        Assert.True(count > 0);
    }
}
