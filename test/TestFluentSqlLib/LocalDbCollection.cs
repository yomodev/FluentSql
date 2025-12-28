namespace TestFluentSqlLib;

[CollectionDefinition("SharedLocalDb")]
public class SharedLocalDbCollection : ICollectionFixture<LocalDbFixture>
{
}
