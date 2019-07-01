# SqlKata Query Builder

[![Build status](https://ci.appveyor.com/api/projects/status/bh022c0ol5u6s41p?svg=true)](https://ci.appveyor.com/project/ahmad-moussawi/querybuilder)

[![SqlKata on Nuget](https://img.shields.io/nuget/vpre/SqlKata.svg)](https://www.nuget.org/packages/SqlKata)

[![SqlKata on MyGet](https://img.shields.io/myget/sqlkata/v/SqlKata.svg?label=myget)](https://www.myget.org/feed/sqlkata/package/nuget/SqlKata)

<a href="https://twitter.com/ahmadmuzavi?ref_src=twsrc%5Etfw" class="twitter-follow-button" data-size="large" data-show-count="false">Follow @ahmadmuzavi</a> for the latest updates about SqlKata.

![Quick Demo](https://i.imgur.com/jOWD4vk.gif)


SqlKata Query Builder is a powerful Sql Query Builder written in C#.

It's secure and framework agnostic. Inspired by the top Query Builders available, like Laravel Query Builder, and Knex.

SqlKata has an expressive API. it follows a clean naming convention, which is very similar to the SQL syntax.

By providing a level of abstraction over the supported database engines, that allows you to work with multiple databases with the same unified API.

SqlKata supports complex queries, such as nested conditions, selection from SubQuery, filtering over SubQueries, Conditional Statements and others. Currently it has built-in compilers for SqlServer, MySql, PostgreSql and Firebird.

Checkout the full documentation on [https://sqlkata.com](https://sqlkata.com)

## Installation

using dotnet cli
```sh
$ dotnet add package SqlKata
```

using Nuget Package Manager
```sh
PM> Install-Package SqlKata
```


## Quick Examples

### Setup Connection

```cs
var connection = new SqlConnection("...");
var compiler = new SqlCompiler();
var db = new QueryFactory(connection, compiler);
```

### Get all records
```cs
var books = db.Query("Books").Get();
```

### Published books only
```cs
var books = db.Query("Books").WhereTrue("IsPublished").Get();
```

### Get one book by Id
```cs
var introToSql = db.Query("Books").Where("Id", 145).Where("Lang", "en").First();
```

### Recent books: last 10
```cs
var recent = db.Query("Books").OrderByDesc("PublishedAt").Limit(10).Get();
```

### Join with authors table

```cs
var books = db.Query("Books")
    .Join("Authors", "Authors.Id", "Books.AuthorId")
    .Select("Books.*", "Authors.Name as AuthorName")
    .Get();

foreach(var book in books)
{
    Console.WriteLine($"{book.Title}: {book.AuthorName}");
}
```

### Conditional queries
```cs
var isFriday = DateTime.Today.DayOfWeek == DayOfWeek.Friday;

var books = db.Query("Books")
    .When(isFriday, q => q.WhereIn("Category", new [] {"OpenSource", "MachineLearning"}))
    .Get();
```

### Pagination

```cs
var page1 = db.Query("Books").Paginate(10);

foreach(var book in page1.List)
{
    Console.WriteLine(book.Name);
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
