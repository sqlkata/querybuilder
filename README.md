# SqlKata Query Builder
<!--方-->

[![Build Status](https://travis-ci.org/sqlkata/querybuilder.svg?branch=master)](https://travis-ci.org/sqlkata/querybuilder)

<img src="/logo.png?raw=true" width="180" height="180" />

A powerful Sql Query Builder written in C#, secure and framework agnostic.

Inspired by the top Query Builders available, like Laravel Query Builder, and Knex.

SqlKata has an expressive API and follow a clean naming convention, very similar to the SQL syntax, that make writing SQL queries easy and funny, without the need to read long pages of documentations.

It provides a level of abstraction over the supported database engines, that allows you to work with multiple database with the same unified API.

SqlKata supports complex queries, such as nested conditions, selection from SubQuery, filtering over SubQueries, Conditional Statements, Deep Joins and others.

Currently it has built-in compilers for **SqlServer 2008** and above, **MySql 5** and **PostgreSql 9**.

## Some fresh code
```cs
var compiler = new SqlServerCompiler();

var includeSportsCars = true;

var fastCarsQuery = new Query("Cars")
    .Where("Speed", ">", 120);

if(includeSportsCars) 
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
Linq provide a strongly typed query mechanism with a High Level of abstraction, while this is good to some extent, but you get very limited when you need more flexiblity and a lower level of control.

for instance if you need to *select from* | *filter over* a SubQuery, make complex joins, or using SQL functions.

One case that I've always face is the missing **OrWhere** functionality.

In linq to Get all cars that are faster than 120mph **OR** the car is categorized as sports car.

You can write your query like this: 

```cs
var fastCarsQuery = db.Cars
    .Where(x => x.MaxSpeed > 120 || x.IsSportCar);
```

Now if in some conditions you wont need to include the sports cars, you have to parametrize this condition.

```cs
var includeSportsCars = false;

var fastCarsQuery = db.Cars
    .Where(x => x.MaxSpeed > 120)
    .Where(x => includeSportsCars || x.IsSportsCar);
```

Now this query will retrieve sports cars, only if the **includeSportsCars** variable is **true**.

One problem here is that developers may get confused easily by these kind of constraints, another problem is that **includeSportsCars** get evaluated on the database server. 

To avoid this you have either to use advanced solutions like the [Predicate Builder](http://www.albahari.com/nutshell/predicatebuilder.aspx) or you should write two separate queries.

```cs
var fastCarsQuery = db.Cars.AsQueryable();

if(includeSportsCars)
{
    fastCarsQuery = fastCarsQuery.Where(x => x.MaxSpeed > 120 || x.IsSportsCar);
} else 
{
    fastCarsQuery = fastCarsQuery.Where(x => x.MaxSpeed > 120);
}
```

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

