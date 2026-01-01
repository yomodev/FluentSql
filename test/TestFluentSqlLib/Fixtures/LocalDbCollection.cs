namespace TestFluentSqlLib.Fixtures;

[CollectionDefinition("SharedLocalDb")]
public class SharedLocalDbCollection : ICollectionFixture<LocalDbFixture>
{
}
