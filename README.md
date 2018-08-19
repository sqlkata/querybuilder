# SqlKata Query Builder

[![Build status](https://ci.appveyor.com/api/projects/status/bh022c0ol5u6s41p?svg=true)](https://ci.appveyor.com/project/ahmad-moussawi/querybuilder)

[![SqlKata on Nuget](https://img.shields.io/nuget/vpre/SqlKata.svg)](https://www.nuget.org/packages/SqlKata)

[![SqlKata on MyGet](https://img.shields.io/myget/sqlkata/v/SqlKata.svg?label=myget)](https://www.myget.org/feed/sqlkata/package/nuget/SqlKata)

SqlKata Query Builder is a powerful Sql Query Builder written in C#.

It's secure and framework agnostic. Inspired by the top Query Builders available, like Laravel Query Builder, and Knex.

SqlKata has an expressive API. it follows a clean naming convention, which is very similar to the SQL syntax.

By providing a level of abstraction over the supported database engines, that allows you to work with multiple databases with the same unified API.

SqlKata supports complex queries, such as nested conditions, selection from SubQuery, filtering over SubQueries, Conditional Statements and others. Currently it has built-in compilers for SqlServer, MySql and PostgreSql.

Checkout the full documentation on [https://sqlkata.com](https://sqlkata.com)

## Quick Examples

### Setup Connection

```cs
var connection = new SqlConnection("...");
var compiler = new SqlCompiler();
var db = new QueryFactory(connection, compiler);
```

### Reading data

```cs
var users = db.Query("Users").Get();

// Filter by Active
var users = db.Query("Users").WhereTrue("Active").Get();

// Recent users
var users = db.Query("Users").OrderByDesc("CreatedAt").Limit(10).Get();

// Join with profiles
var users = db.Query("Users")
    .Join("Countries", "Countries.Id", "Users.CountryId")
    .Select(
        "Users.*",
        "Countries.Name as CountryName"
    )
    .Get();

foreach(var user in users)
{
    Console.WriteLine($"{user.Name}: {user.CountryName}");
}

// Conditional expressions
var lang = Config.Get("Lang");

var users = db.Query("Users")
    .When(lang == "en", q => q.Where("lang", "en"))
    .Get();
```

### Pagination

```cs
var page1 = db.Query("Users").Paginate(10);

foreach(var user in page1)
{
    Console.WriteLine(user.Name);
}

...

var page2 = page1.Next();
```

### Insert

```cs
int affected = db.Query("Users").Insert(new {
    Name = "Jane",
    CountryId = 1
});
```

### Update

```cs
int affected = db.Query("Users").Where("Id", 1).Update(new {
    Name = "Jane",
    CountryId = 1
});
```

### Delete

```cs
int affected = db.Query("Users").Where("Id", 1).Delete();
```
