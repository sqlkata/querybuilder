using static SqlKata.Expressions;
using SqlKata.Compilers;
using SqlKata.Tests.Infrastructure;
using Xunit;
using System.Collections.Generic;

namespace SqlKata.Tests
{
    /// <summary>
    /// If you want to test this queries against a database use NorthWind database
    /// </summary>
    public class DefineTest : TestSupport
    {

        [Fact]
        public void Test_Define_Where()
        {
            Query query = new Query("Products")
              .Define("@name", "Anto")
              .Where("ProductName", Variable("@name"));

            IReadOnlyDictionary<string, string> c = Compile(query);

            Assert.Equal("SELECT * FROM [Products] WHERE [ProductName] = 'Anto'", c[EngineCodes.SqlServer]);

        }

        [Fact]
        public void Test_Define_SubQuery()
        {

            Query subquery = new Query("Products")
                .AsAverage("unitprice")
                .Define("@UnitsInSt", 10)
                .Where("UnitsInStock", ">", Variable("@UnitsInSt"));

            Query query = new Query("Products")
                .Where("unitprice", ">", subquery)
                .Where("UnitsOnOrder", ">", 5);

            IReadOnlyDictionary<string, string> c = Compile(query);

            Assert.Equal("SELECT * FROM [Products] WHERE [unitprice] > (SELECT AVG([unitprice]) AS [avg] FROM [Products] WHERE [UnitsInStock] > 10) AND [UnitsOnOrder] > 5", c[EngineCodes.SqlServer]);

        }


        [Fact]
        public void Test_Define_WhereEnds()
        {

            Query query1 = new Query("Products")
                .Select("ProductId")
                .Define("@product", "Coffee")
                .WhereEnds("ProductName", Variable("@product"));


            Query query2 = new Query("Products")
                .Select("ProductId", "ProductName")
                .Define("@product", "Coffee")
                .WhereEnds("ProductName", Variable("@product"), true);

            IReadOnlyDictionary<string, string> c1 = Compile(query1);
            IReadOnlyDictionary<string, string> c2 = Compile(query2);

            Assert.Equal("SELECT [ProductId] FROM [Products] WHERE LOWER([ProductName]) like '%coffee'", c1[EngineCodes.SqlServer]);

            Assert.Equal("SELECT [ProductId], [ProductName] FROM [Products] WHERE [ProductName] like '%Coffee'", c2[EngineCodes.SqlServer]);

        }



        [Fact]
        public void Test_Define_WhereStarts()
        {


            Query query1 = new Query("Products")
                        .Select("ProductId", "QuantityPerUnit")
                        .Define("@perUnit", "12")
                        .WhereStarts("QuantityPerUnit", Variable("@perUnit"));


            Query query2 = new Query("Products")
                               .Select("ProductId", "QuantityPerUnit")
                               .Define("@perUnit", "12")
                               .WhereStarts("QuantityPerUnit", Variable("@perUnit"), true);

            IReadOnlyDictionary<string, string> c1 = Compile(query1);
            IReadOnlyDictionary<string, string> c2 = Compile(query2);

            Assert.Equal("SELECT [ProductId], [QuantityPerUnit] FROM [Products] WHERE LOWER([QuantityPerUnit]) like '12%'", c1[EngineCodes.SqlServer]);
            Assert.Equal("SELECT [ProductId], [QuantityPerUnit] FROM [Products] WHERE [QuantityPerUnit] like '12%'", c2[EngineCodes.SqlServer]);
        }


        [Fact]
        public void Test_Define_WhereContains()
        {

            Query query1 = new Query("Products")
                .Define("@perUnit", "500")
                .Select("ProductId", "QuantityPerUnit")
                .WhereContains("QuantityPerUnit", Variable("@perUnit"));


            Query query2 = new Query("Products")
                .Define("@perUnit", "500")
                .Select("ProductId", "QuantityPerUnit")
                .WhereContains("QuantityPerUnit", Variable("@perUnit"), true);

            IReadOnlyDictionary<string, string> c1 = Compile(query1);
            IReadOnlyDictionary<string, string> c2 = Compile(query2);

            Assert.Equal("SELECT [ProductId], [QuantityPerUnit] FROM [Products] WHERE LOWER([QuantityPerUnit]) like '%500%'", c1[EngineCodes.SqlServer]);
            Assert.Equal("SELECT [ProductId], [QuantityPerUnit] FROM [Products] WHERE [QuantityPerUnit] like '%500%'", c2[EngineCodes.SqlServer]);

        }


        [Fact]
        public void Test_Define_WhereLike()
        {
            Query query1 = new Query("Products")
                .Select("ProductId", "ProductName", "SupplierID")
                .Define("@id", "20")
                .WhereLike("SupplierID", Variable("@id"));


            Query query2 = new Query("Products")
                .Select("ProductId", "ProductName", "SupplierID")
                .Define("@id", "20")
                .WhereLike("SupplierID", Variable("@id"), true);

            IReadOnlyDictionary<string, string> c1 = Compile(query1);
            IReadOnlyDictionary<string, string> c2 = Compile(query2);

            Assert.Equal("SELECT [ProductId], [ProductName], [SupplierID] FROM [Products] WHERE LOWER([SupplierID]) like '20'", c1[EngineCodes.SqlServer]);

            Assert.Equal("SELECT [ProductId], [ProductName], [SupplierID] FROM [Products] WHERE [SupplierID] like '20'", c2[EngineCodes.SqlServer]);
        }


        [Fact]
        public void Test_Define_WhereInSubquery()
        {

            Query subquery = new Query("Orders")
                               .Define("@shipId", 3)
                               .Select("ShipVia").Where("ShipVia", Variable("@shipId"));


            Query query1 = new Query("Shippers")
                               .Select("ShipperID", "CompanyName")
                               .WhereIn("ShipperID", subquery);


            IReadOnlyDictionary<string, string> c1 = Compile(query1);

            Assert.Equal("SELECT [ShipperID], [CompanyName] FROM [Shippers] WHERE [ShipperID] IN (SELECT [ShipVia] FROM [Orders] WHERE [ShipVia] = 3)", c1[EngineCodes.SqlServer]);
        }

        [Fact]
        public void Test_Define_Having()
        {
            IReadOnlyDictionary<string, string> c = Compile(new Query("Table")
                .Define("@foo", 1)
                .Having("Id", "=", Variable("@foo")));

            Assert.Equal("SELECT * FROM [Table] HAVING [Id] = 1", c[EngineCodes.SqlServer]);
        }

        /*
        [Fact]
        public void Test_Define_HavingRaw()
        {
            Query query1 = new Query("Orders")
                               .Define("@count", 80)
                               .Select("Employees.LastName")
                               .SelectRaw("COUNT(Orders.OrderID) AS NumberOfOrders")
                               .Join("Employees", "Employees.EmployeeID", "Orders.EmployeeID")
                               .GroupBy("LastName")
                               .HavingRaw("COUNT(Orders.OrderID) > @count");

            IReadOnlyDictionary<string, string> c = Compile(query1);

            Assert.Equal("SELECT [Employees].[LastName], COUNT(Orders.OrderID) AS NumberOfOrders FROM [Orders] \nINNER JOIN [Employees] ON [Employees].[EmployeeID] = [Orders].[EmployeeID] GROUP BY [LastName] HAVING COUNT(Orders.OrderID) > 80", c[EngineCodes.SqlServer]);

        }
        */

        [Fact]
        public void Test_Define_HavingStarts()
        {

            Query query = new Query("Customers")
                        .Define("@label", "U")
                        .SelectRaw("COUNT(CustomerID)")
                        .Select("Country")
                        .GroupBy("Country")
                        .HavingStarts("Country", Variable("@label"));

            IReadOnlyDictionary<string, string> c = Compile(query);

            Assert.Equal("SELECT COUNT(CustomerID), [Country] FROM [Customers] GROUP BY [Country] HAVING LOWER([Country]) like 'u%'", c[EngineCodes.SqlServer]);

        }



        [Fact]
        public void Test_Define_Having_Ends()
        {
            Query query = new Query("Customers")
                            .Define("@label", "d")
                            .SelectRaw("COUNT(CustomerID)")
                            .Select("Country")
                            .GroupBy("Country")
                            .HavingEnds("Country", Variable("@label"));

            IReadOnlyDictionary<string, string> c = Compile(query);

            Assert.Equal("SELECT COUNT(CustomerID), [Country] FROM [Customers] GROUP BY [Country] HAVING LOWER([Country]) like '%d'", c[EngineCodes.SqlServer]);
        }


        [Fact]
        public void Test_Define_Having_Contains()
        {


            Query query = new Query("Customers")
                            .Define("@label", "d")
                            .SelectRaw("COUNT(CustomerID)")
                            .Select("Country")
                            .GroupBy("Country")
                            .HavingContains("Country", Variable("@label"));

            IReadOnlyDictionary<string, string> c = Compile(query);

            Assert.Equal("SELECT COUNT(CustomerID), [Country] FROM [Customers] GROUP BY [Country] HAVING LOWER([Country]) like '%d%'", c[EngineCodes.SqlServer]);

        }


        [Fact]
        public void Test_Define_NestedCondition()
        {
            Query query = new Query("Orders")
               .Define("@shipReg", null)
               .Define("@one", 1)
               .Where(q =>
                   q.Where("ShipRegion", "!=", Variable("@shipReg"))
                //    .WhereRaw("1 = @one")
                ).AsCount();

            IReadOnlyDictionary<string, string> c = Compile(query);

            Assert.Equal("SELECT COUNT(*) AS [count] FROM [Orders] WHERE ([ShipRegion] != NULL)", c[EngineCodes.SqlServer]);

        }


        [Fact]
        public void Test_Define_WhereDate()
        {
            System.DateTime dateObj = new System.DateTime(year: 1996, month: 8, day: 1);

            Query query = new Query("Orders")
                .Define("@d", dateObj)
                .WhereDate("RequiredDate", Variable("@d"));


            Query query2 = new Query("Orders")
                .Define("@d", 1996)
                .WhereDatePart("year", "RequiredDate", "=", Variable("@d"));

            Query query3 = new Query("Orders")
                .Define("@d", "00:00:00")
                .WhereTime("RequiredDate", "!=", Variable("@d"));

            IReadOnlyDictionary<string, string> c = Compile(query);
            IReadOnlyDictionary<string, string> c2 = Compile(query2);
            IReadOnlyDictionary<string, string> c3 = Compile(query3);

            Assert.Equal("SELECT * FROM [Orders] WHERE CAST([RequiredDate] AS DATE) = '1996-08-01'", c[EngineCodes.SqlServer]);
            Assert.Equal("SELECT * FROM \"Orders\" WHERE \"RequiredDate\"::date = '1996-08-01'", c[EngineCodes.PostgreSql]);
            Assert.Equal("SELECT * FROM \"Orders\" WHERE strftime('%Y-%m-%d', \"RequiredDate\") = cast('1996-08-01' as text)", c[EngineCodes.Sqlite]);
            Assert.Equal("SELECT * FROM \"ORDERS\" WHERE CAST(\"REQUIREDDATE\" as DATE) = '1996-08-01'", c[EngineCodes.Firebird]);



            Assert.Equal("SELECT * FROM [Orders] WHERE DATEPART(YEAR, [RequiredDate]) = 1996", c2[EngineCodes.SqlServer]);
            Assert.Equal("SELECT * FROM [Orders] WHERE CAST([RequiredDate] AS TIME) != '00:00:00'", c3[EngineCodes.SqlServer]);

        }


        [Fact]
        public void Test_Define_WhereExists()
        {
            Query query = new Query("Customers")
                .WhereExists(q => q.From("Orders")
                .Define("@postal", "8200")
                .Where("ShipPostalCode", Variable("@postal"))
            );

            IReadOnlyDictionary<string, string> c = Compile(query);
            Assert.Equal("SELECT * FROM [Customers] WHERE EXISTS (SELECT 1 FROM [Orders] WHERE [ShipPostalCode] = '8200')", c[EngineCodes.SqlServer]);
        }



        [Fact]
        public void Test_Define_With()
        {

            Query query = new Query("Products")
                        .Define("@unit", 10)
                        .Join("Categories", "Categories.CategoryID", "Products.CategoryID")
                        .Select("Categories.CategoryName", "Products.UnitPrice")
                        .Where("Products.UnitPrice", ">", Variable("@unit"));

            Query queryCTe = new Query("prodCTE")
                            .With("prodCTE", query);

            IReadOnlyDictionary<string, string> c = Compile(queryCTe);


            Assert.Equal("WITH [prodCTE] AS (SELECT [Categories].[CategoryName], [Products].[UnitPrice] FROM [Products] \nINNER JOIN [Categories] ON [Categories].[CategoryID] = [Products].[CategoryID] WHERE [Products].[UnitPrice] > 10)\nSELECT * FROM [prodCTE]", c[EngineCodes.SqlServer]);
        }



        /*
        [Fact]
        public void Test_Define_WithRaw()
        {

            //WithRaw
            Query query = new Query("prodCTE")
                .Define("@unit", 10)
                .Define("@foo", 2)
                .Select("CategoryName", "UnitPrice")
                .WithRaw("prodCTE", "SELECT c.CategoryName, p.UnitPrice FROM Products p INNER JOIN Categories c ON c.CategoryID = p.CategoryID WHERE p.UnitPrice > @unit AND  2 = @foo");

            IReadOnlyDictionary<string, string> c = Compile(query);

            Assert.Equal("WITH [prodCTE] AS (SELECT c.CategoryName, p.UnitPrice FROM Products p INNER JOIN Categories c ON c.CategoryID = p.CategoryID WHERE p.UnitPrice > 10 AND  2 = 2)\nSELECT [CategoryName], [UnitPrice] FROM [prodCTE]", c[EngineCodes.SqlServer]);

        }
        */

        //
        [Fact]
        public void Test_Define_Union()
        {
            Query q1 = new Query("Suppliers")
                        .Define("@foo", "Beirut")
                        .Select("City")
                        .Where("City", Variable("@foo"));

            Query q2 = new Query("Customers")
                            .Define("@city", "Z")
                            .Select("City")
                            .Union(q1)
                            .WhereNotLike("City", Variable("@city"));

            IReadOnlyDictionary<string, string> c = Compile(q2);
            Assert.Equal("SELECT [City] FROM [Customers] WHERE NOT (LOWER([City]) like 'z') UNION SELECT [City] FROM [Suppliers] WHERE [City] = 'Beirut'", c[EngineCodes.SqlServer]);
        }


        [Fact]
        public void Test_Define_Except()
        {
            Query q1 = new Query("Suppliers")
                        .Define("@foo", "Beirut")
                        .Select("City")
                        .Where("City", Variable("@foo"));

            Query q2 = new Query("Customers")
                        .Define("@city", "Z")
                        .Select("City")
                        .Except(q1)
                        .WhereNotLike("City", Variable("@city"));

            IReadOnlyDictionary<string, string> c = Compile(q2);
            Assert.Equal("SELECT [City] FROM [Customers] WHERE NOT (LOWER([City]) like 'z') EXCEPT SELECT [City] FROM [Suppliers] WHERE [City] = 'Beirut'", c[EngineCodes.SqlServer]);
        }

        [Fact]
        public void Test_Define_Intersect()
        {
            Query q1 = new Query("Suppliers")
                        .Define("@foo", "Beirut")
                        .Select("City")
                        .Where("City", Variable("@foo"));

            Query q2 = new Query("Customers")
                        .Define("@city", "Z")
                        .Select("City")
                        .Intersect(q1)
                        .WhereNotLike("City", Variable("@city"));

            IReadOnlyDictionary<string, string> c = Compile(q2);
            Assert.Equal("SELECT [City] FROM [Customers] WHERE NOT (LOWER([City]) like 'z') INTERSECT SELECT [City] FROM [Suppliers] WHERE [City] = 'Beirut'", c[EngineCodes.SqlServer]);
        }

        /*
        [Fact]
        public void Test_Define_CombineRaw()
        {

            Query query = new Query("Customers")
                        .Define("@foo", 1)
                        .Define("@faa", 2)
                        .Select("City")
                        .CombineRaw("UNION ALL SELECT City FROM Suppliers WHERE 1 = @foo AND 2 = @faa");

            IReadOnlyDictionary<string, string> c = Compile(query);
            Assert.Equal("SELECT [City] FROM [Customers] UNION ALL SELECT City FROM Suppliers WHERE 1 = 1 AND 2 = 2", c[EngineCodes.SqlServer]);
        }
        */

    }
}
