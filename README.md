<p align="center">
    <strong style="font-size: 2em;">SqlKata Query Builder</strong>
</p>
<p align="center">
    <img src="https://github.com/sqlkata/querybuilder/actions/workflows/build.yml/badge.svg">
    <a href="https://www.nuget.org/packages/SqlKata"><img src="https://img.shields.io/nuget/vpre/SqlKata.svg"></a>
    <a href="https://github.com/sqlkata/querybuilder/network/members"><img src="https://img.shields.io/github/forks/sqlkata/querybuilder"></a>
    <a href="https://github.com/sqlkata/querybuilder/stargazers"><img src="https://img.shields.io/github/stars/sqlkata/querybuilder"></a>
    <a href="https://twitter.com/intent/tweet?text=Wow:&url=https%3A%2F%2Fgithub.com%2Fsqlkata%2Fquerybuilder"><img alt="Twitter" src="https://img.shields.io/twitter/url?label=Tweet%20about%20SqlKata&style=social&url=https%3A%2F%2Fgithub.com%2Fsqlkata%2Fquerybuilder"></a>		
</p>

<p align="center">
    <strong>Follow and Upvote SqlKata on Product Hunt to encourage the development of this project</strong>
</p>
<p align="center">
    <a href="https://www.producthunt.com/products/sqlkata?utm_source=badge-follow&utm_medium=badge&utm_souce=badge-sqlkata" target="_blank"><img src="https://api.producthunt.com/widgets/embed-image/v1/follow.svg?post_id=398417&theme=light" alt="SqlKata - Dynamic&#0032;Sql&#0032;query&#0032;builder&#0032;for&#0032;dotnet | Product Hunt" style="width: 250px; height: 54px;" width="250" height="54" /></a>
</p>

https://private-user-images.githubusercontent.com/2517523/408899411-1f9ff89b-ed17-4d02-ae2f-afc1c66a5932.mp4

## 🚀 Introduction

SqlKata Query Builder is a powerful SQL query builder written in C#. It is secure, framework-agnostic, and inspired by top query builders like Laravel Query Builder and Knex.

### ✨ Key Features
- **Expressive API**: Clean and intuitive syntax similar to SQL.
- **Database Agnostic**: Work with multiple databases using a unified API.
- **Complex Queries**: Supports nested conditions, subqueries, conditional statements, and more.
- **Execution Support**: Use the SqlKata.Execution package to execute queries with Dapper.

## Installation

```sh
$ dotnet add package SqlKata
$ dotnet add package SqlKata.Execution # (optional) If you want the execution support
```

## Quick Examples

### Setup Connection

```cs
var connection = new SqlConnection("...");
var compiler = new SqlCompiler();
var db = new QueryFactory(connection, compiler);
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
    .Include(db.Query("Authors")) // Assumes that the Books table has an `AuthorId` column
    .Get();
```

This will include the property "Author" on each "Book":
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

## FAQ

### How to know when a new release or a feature is available?

I announce updates on my [Twitter Account](https://twitter.com/ahmadmuzavi), and you can subscribe to our newsletters from the website [https://sqlkata.com](https://sqlkata.com).

### The database that I want is not supported. Why?

It's impossible to support all available database vendors, which is why we focus on the major ones. We encourage you to create your own compiler for your database.

### Do you accept new compilers?

Unfortunately, no. The reason is that this would add overhead for the project contributors. We prefer to improve the quality of the existing compilers instead.

### How can I support the project?

- ⭐ Star the project here on GitHub, and share it with your friends.
- 🐱‍💻 Follow and upvote it on Product Hunt:
  <a href="https://www.producthunt.com/products/sqlkata?utm_source=badge-follow&utm_medium=badge&utm_souce=badge-sqlkata" target="_blank"><img src="https://api.producthunt.com/widgets/embed-image/v1/follow.svg?post_id=398417&theme=light&size=small" alt="SqlKata - Dynamic&#0032;Sql&#0032;query&#0032;builder&#0032;for&#0032;dotnet | Product Hunt" style="width: 86px; height: 32px;" width="250" height="54" /></a>
- 💰 You can also donate to support the project financially on open collection.
