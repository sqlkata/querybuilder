# SqlKata Query Builder 方

[![Build Status](https://travis-ci.org/sqlkata/querybuilder.svg?branch=master)](https://travis-ci.org/sqlkata/querybuilder)

A powerful Sql Query Builder written in C#, inspired by the top Query Builders available, like Laravel Query Builder, and Knex.

SqlKata follow a clean naming convention, very similar to the SQL syntax, to make writing SQL queries easy and funny, without the need to read long pages of documentations.

It provides a level of abstraction over the supported database engines, so you won't change your code when you need to upgrade to a newer database version, or if you want to switch to another database provider.

SqlKata supports complex queries, such as nested conditions, selection from SubQuery, filtering over SubQueries, Conditional Statements, Deep Joins and others.

Currently it has built-in compilers for **SqlServer 2008** and above, **MySql 5** and **PostgreSql 9**.

## Some fresh code
```cs
var compiler = new SqlServerCompiler();

var includeUnofficial = true;
var countries = new [] {"CA", "FR"};

var query = new Query("Products").DeepJoin("Providers.Countries")
    .WhereIn("CountryId", countries);
    .Where("Price", ">", 1000);

// or you can use the conditional statement `When(includeUnofficial, q => q.OrWhere("Official", true))`
if(includeUnofficial) 
{
    query.OrWhere("Official", true);
}

string sql = compiler.Compile(query).ToSql();
```

Check out the docs for other examples [SqlKata docs](http://sqlkata.vivida-apps.com)

## Why do I need a Query Builder ?
I've started building this Query Builder, when I was developing big applications that have complex dashboards, and reports.

before I've used to write my SQL queries in strings, and things get worse quickly when you have some dynamic conditions, and even when you are working with multiple database providers, like SqlServer and PostgreSql with the same code base.

## What about Linq and EntityFramework
Linq provide a strongly typed query mechanism with a High Level of abstraction, while this is good to some extent, but you get very limited when you need more flexiblity.

for instance if you need to *select from* | *filter over* a SubQuery, make complex joins, or using SQL functions.


One other case that I've always face is the missing **OrWhere** functionality.

```cs
var includeUnofficial = true;

// In Linq, you have to pass the condition to the SQL engine, or use advanced solutions like http://www.albahari.com/nutshell/predicatebuilder.aspx
var productsQuery = db.Products
    .Where(x => x.Price > 1000 && includeUnofficial || x.Official);
```

In this case the **includeUnofficial** variable get evaluated on the Database Server for each row in the products table.

Off course you can use other solutions like http://www.albahari.com/nutshell/predicatebuilder.aspx, but your code may sounds verbose a bit.

## Installation
Currently SqlKata is supported on `netcoreapp1.0`.

```bash
## using dotnet-cli
dotnet add package SqlKata -v 1.0.0-beta-32

## or run from the package manager
Install-Package SqlKata -Pre
```

## What Next ?
While SqlKata is still in beta, I've stopped adding new features untilI get a stable release.

But to give you an idea about my priorities, I will add the support to Execute Queries, Caching mechanism and to support more native features across the available compilers. 

## Contributions
I don't have a strict contribution guide till the moment, but you can contribute with ideas, bug fixing, or add more test cases.

One simple note to keep in mind, that there is no room for complex or unclean features.

