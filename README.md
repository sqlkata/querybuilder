# SqlKata Query Builder

[![Build status](https://ci.appveyor.com/api/projects/status/bh022c0ol5u6s41p?svg=true)](https://ci.appveyor.com/project/ahmad-moussawi/querybuilder)

[![SqlKata on Nuget](https://img.shields.io/nuget/vpre/SqlKata.svg)](https://www.nuget.org/packages/SqlKata)

<img src="/logo.png?raw=true" width="180" height="180" />
SqlKata Query Builder is a powerful Sql Query Builder written in C#. 

it's secure and framework agnostic. Inspired by the top Query Builders available, like Laravel Query Builder, and Knex. 

SqlKata has an expressive API. it follows a clean naming convention, which is very similar to the SQL syntax.

It make writing SQL queries easy and funny, with no need to read long pages of documentations. 

It provides a level of abstraction over the supported database engines, that allows you to work with multiple databases with the same unified API.

SqlKata supports complex queries, such as nested conditions, selection from SubQuery, filtering over SubQueries, Conditional Statements, Deep Joins and others. Currently it has built-in compilers for SqlServer 2008 and above, MySql and PostgreSql.

## Some fresh code
```cs
var compiler = new SqlServerCompiler();

var withSportCars = Config.get('IncludeSportsCar');

var fastCarsQuery = new Query("Cars")
    .Where("Speed", ">", 120);

if(withSportCars) 
{
    fastCarsQuery.OrWhere("IsSportCar", true);
}

string sql = compiler.Compile(fastCarsQuery).Sql;
```

Check out the docs for other examples [SqlKata docs](http://sqlkata.com)

## Why do I need a Query Builder ?
I've started building this Query Builder, when I was developing big applications that have complex dashboards, and reports.

before I've used to write my SQL queries in strings, and things get worse quickly when you have some dynamic conditions, and even when you are working with multiple database providers, like SqlServer and PostgreSql with the same code base.

## Installation
SqlKata is supported on both __dotnet standard__ and __net framework 4.5.*__.

To install it check the installation guide from the [Installing SqlKata](https://sqlkata.com/docs#installation)


## What Next ?
While SqlKata is still in beta, I've stopped adding new major features until I get a stable release.

But to give you an idea about my priorities, I will add the support to Execute Queries, Caching mechanism and to support more native features across available compilers. 

## Contributions
I don't have a strict contribution guide till the moment, but you can contribute with ideas, bug fixing, or add more test cases.

One simple note to keep in mind, that there is no room for complex or unclean features.

A big thanks for all [contributors](https://github.com/sqlkata/querybuilder/graphs/contributors), for making SqlKata an awesome library 