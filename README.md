<p align="center">
    <strong>SqlKata Query Builder</strong>
</p>

<p align="center">
    <img src="https://github.com/sqlkata/querybuilder/actions/workflows/build.yml/badge.svg">
    <a href="https://www.nuget.org/packages/SqlKata"><img src="https://img.shields.io/nuget/vpre/SqlKata.svg"></a>
    <a href="https://github.com/sqlkata/querybuilder/network/members"><img src="https://img.shields.io/github/forks/sqlkata/querybuilder"></a>
    <a href="https://github.com/sqlkata/querybuilder/stargazers"><img src="https://img.shields.io/github/stars/sqlkata/querybuilder"></a>
    <a href="https://twitter.com/intent/tweet?text=Wow:&url=https%3A%2F%2Fgithub.com%2Fsqlkata%2Fquerybuilder"><img alt="Twitter" src="https://img.shields.io/twitter/url?label=Tweet%20about%20SqlKata&style=social&url=https%3A%2F%2Fgithub.com%2Fsqlkata%2Fquerybuilder"></a>
</p>



> **WE ARE NOT ACCEPTING NEW COMPILERS, if you want to add your own compiler, we recommend to create a separate repo like SqlKata-Oracle**

Follow <a href="https://twitter.com/intent/tweet?text=Wow:&url=https%3A%2F%2Fgithub.com%2Fsqlkata%2Fquerybuilder"><img alt="Twitter" src="https://img.shields.io/twitter/url?label=%40ahmadmuzavi&style=social&url=https%3A%2F%2Ftwitter.com%2Fahmadmuzavi"></a> for the latest updates about SqlKata.


![Quick Demo](https://i.imgur.com/jOWD4vk.gif)



SqlKata Query Builder is a powerful Sql Query Builder written in C#.

It's secure and framework agnostic. Inspired by the top Query Builders available, like Laravel Query Builder, and Knex.

SqlKata has an expressive API. it follows a clean naming convention, which is very similar to the SQL syntax.

By providing a level of abstraction over the supported database engines, that allows you to work with multiple databases with the same unified API.

SqlKata supports complex queries, such as nested conditions, selection from SubQuery, filtering over SubQueries, Conditional Statements and others. Currently it has built-in compilers for SqlServer, MySql, PostgreSql and Firebird.

The SqlKata.Execution package provides the ability to submit the queries to the database, using [Dapper](https://github.com/StackExchange/Dapper) under the covers.

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

var db = new QueryFactory(connection, compiler)
```

> `QueryFactory` is provided by the SqlKata.Execution package.

### Retrieve all records
```cs
var books = db.Query("Books").Get();
```

### Retrieve published books only
```cs
var books = db.Query("Books").WhereTrue("IsPublished").Get();
```

### Retrieve one book
```cs
var introToSql = db.Query("Books").Where("Id", 145).Where("Lang", "en").First();
```

### Retrieve recent books: last 10
```cs
var recent = db.Query("Books").OrderByDesc("PublishedAt").Limit(10).Get();
```

### Include Author information
```cs
var books = db.Query("Books")
    .Include(db.Query("Authors")) // Assumes that the Books table have a `AuthorId` column
    .Get();
```

This will include the property "Author" on each "Book"
```jsonc
[{
    "Id": 1,
    "PublishedAt": "2019-01-01",
    "AuthorId": 2,
    "Author": { // <-- included property
        "Id": 2,
        "...": ""
    }
}]
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
