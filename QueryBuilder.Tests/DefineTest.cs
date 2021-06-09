using static SqlKata.Expressions;
using SqlKata.Compilers;
using SqlKata.Tests.Infrastructure;
using Xunit;

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
            var query = new Query("Products")
              .Define("@name", "Anto")
              .Where("ProductName", Variable("@name"));

            var c = Compile(query);

            Assert.Equal("SELECT * FROM [Products] WHERE [ProductName] = 'Anto'", c[EngineCodes.SqlServer]);

        }

        [Fact]
        public void Test_Define_SubQuery()
        {

            var subquery = new Query("Products")
                .SelectAverage("unitprice")
                .Define("@UnitsInSt", 10)
                .Where("UnitsInStock", ">", Variable("@UnitsInSt"));

            var query = new Query("Products")
                .Where("unitprice", ">", subquery)
                .Where("UnitsOnOrder", ">", 5);

            var c = Compile(query);

            Assert.Equal("SELECT * FROM [Products] WHERE [unitprice] > (SELECT AVG([unitprice]) AS [avg] FROM [Products] WHERE [UnitsInStock] > 10) AND [UnitsOnOrder] > 5", c[EngineCodes.SqlServer]);

        }


        [Fact]
        public void Test_Define_WhereEnds()
        {

            var query1 = new Query("Products")
                .Select("ProductId")
                .Define("@product", "Coffee")
                .WhereEnds("ProductName", Variable("@product"));


            var query2 = new Query("Products")
                .Select("ProductId", "ProductName")
                .Define("@product", "Coffee")
                .WhereEnds("ProductName", Variable("@product"), true);

            var c1 = Compile(query1);
            var c2 = Compile(query2);

            Assert.Equal("SELECT [ProductId] FROM [Products] WHERE LOWER([ProductName]) like '%coffee'", c1[EngineCodes.SqlServer]);

            Assert.Equal("SELECT [ProductId], [ProductName] FROM [Products] WHERE [ProductName] like '%Coffee'", c2[EngineCodes.SqlServer]);

        }



        [Fact]
        public void Test_Define_WhereStarts()
        {


            var query1 = new Query("Products")
                        .Select("ProductId", "QuantityPerUnit")
                        .Define("@perUnit", "12")
                        .WhereStarts("QuantityPerUnit", Variable("@perUnit"));


            var query2 = new Query("Products")
                               .Select("ProductId", "QuantityPerUnit")
                               .Define("@perUnit", "12")
                               .WhereStarts("QuantityPerUnit", Variable("@perUnit"), true);

            var c1 = Compile(query1);
            var c2 = Compile(query2);

            Assert.Equal("SELECT [ProductId], [QuantityPerUnit] FROM [Products] WHERE LOWER([QuantityPerUnit]) like '12%'", c1[EngineCodes.SqlServer]);
            Assert.Equal("SELECT [ProductId], [QuantityPerUnit] FROM [Products] WHERE [QuantityPerUnit] like '12%'", c2[EngineCodes.SqlServer]);
        }


        [Fact]
        public void Test_Define_WhereContains()
        {

            var query1 = new Query("Products")
                .Define("@perUnit", "500")
                .Select("ProductId", "QuantityPerUnit")
                .WhereContains("QuantityPerUnit", Variable("@perUnit"));


            var query2 = new Query("Products")
                .Define("@perUnit", "500")
                .Select("ProductId", "QuantityPerUnit")
                .WhereContains("QuantityPerUnit", Variable("@perUnit"), true);

            var c1 = Compile(query1);
            var c2 = Compile(query2);

            Assert.Equal("SELECT [ProductId], [QuantityPerUnit] FROM [Products] WHERE LOWER([QuantityPerUnit]) like '%500%'", c1[EngineCodes.SqlServer]);
            Assert.Equal("SELECT [ProductId], [QuantityPerUnit] FROM [Products] WHERE [QuantityPerUnit] like '%500%'", c2[EngineCodes.SqlServer]);

        }


        [Fact]
        public void Test_Define_WhereLike()
        {
            var query1 = new Query("Products")
                .Select("ProductId", "ProductName", "SupplierID")
                .Define("@id", "20")
                .WhereLike("SupplierID", Variable("@id"));


            var query2 = new Query("Products")
                .Select("ProductId", "ProductName", "SupplierID")
                .Define("@id", "20")
                .WhereLike("SupplierID", Variable("@id"), true);

            var c1 = Compile(query1);
            var c2 = Compile(query2);

            Assert.Equal("SELECT [ProductId], [ProductName], [SupplierID] FROM [Products] WHERE LOWER([SupplierID]) like '20'", c1[EngineCodes.SqlServer]);

            Assert.Equal("SELECT [ProductId], [ProductName], [SupplierID] FROM [Products] WHERE [SupplierID] like '20'", c2[EngineCodes.SqlServer]);
        }


        [Fact]
        public void Test_Define_WhereInSubquery()
        {

            var subquery = new Query("Orders")
                               .Define("@shipId", 3)
                               .Select("ShipVia").Where("ShipVia", Variable("@shipId"));


            var query1 = new Query("Shippers")
                               .Select("ShipperID", "CompanyName")
                               .WhereIn("ShipperID", subquery);


            var c1 = Compile(query1);

            Assert.Equal("SELECT [ShipperID], [CompanyName] FROM [Shippers] WHERE [ShipperID] IN (SELECT [ShipVia] FROM [Orders] WHERE [ShipVia] = 3)", c1[EngineCodes.SqlServer]);
        }

        [Fact]
        public void Test_Define_Having()
        {
            var c = Compile(new Query("Table")
                .Define("@foo", 1)
                .Having("Id", "=", Variable("@foo")));

            Assert.Equal("SELECT * FROM [Table] HAVING [Id] = 1", c[EngineCodes.SqlServer]);
        }

        /*
        [Fact]
        public void Test_Define_HavingRaw()
        {
            var query1 = new Query("Orders")
                               .Define("@count", 80)
                               .Select("Employees.LastName")
                               .SelectRaw("COUNT(Orders.OrderID) AS NumberOfOrders")
                               .Join("Employees", "Employees.EmployeeID", "Orders.EmployeeID")
                               .GroupBy("LastName")
                               .HavingRaw("COUNT(Orders.OrderID) > @count");

            var c = Compile(query1);

            Assert.Equal("SELECT [Employees].[LastName], COUNT(Orders.OrderID) AS NumberOfOrders FROM [Orders] \nINNER JOIN [Employees] ON [Employees].[EmployeeID] = [Orders].[EmployeeID] GROUP BY [LastName] HAVING COUNT(Orders.OrderID) > 80", c[EngineCodes.SqlServer]);

        }
        */

        [Fact]
        public void Test_Define_HavingStarts()
        {

            var query = new Query("Customers")
                        .Define("@label", "U")
                        .SelectRaw("COUNT(CustomerID)")
                        .Select("Country")
                        .GroupBy("Country")
                        .HavingStarts("Country", Variable("@label"));

            var c = Compile(query);

            Assert.Equal("SELECT COUNT(CustomerID), [Country] FROM [Customers] GROUP BY [Country] HAVING LOWER([Country]) like 'u%'", c[EngineCodes.SqlServer]);

        }



        [Fact]
        public void Test_Define_Having_Ends()
        {
            var query = new Query("Customers")
                            .Define("@label", "d")
                            .SelectRaw("COUNT(CustomerID)")
                            .Select("Country")
                            .GroupBy("Country")
                            .HavingEnds("Country", Variable("@label"));

            var c = Compile(query);

            Assert.Equal("SELECT COUNT(CustomerID), [Country] FROM [Customers] GROUP BY [Country] HAVING LOWER([Country]) like '%d'", c[EngineCodes.SqlServer]);
        }


        [Fact]
        public void Test_Define_Having_Contains()
        {


            var query = new Query("Customers")
                            .Define("@label", "d")
                            .SelectRaw("COUNT(CustomerID)")
                            .Select("Country")
                            .GroupBy("Country")
                            .HavingContains("Country", Variable("@label"));

            var c = Compile(query);

            Assert.Equal("SELECT COUNT(CustomerID), [Country] FROM [Customers] GROUP BY [Country] HAVING LOWER([Country]) like '%d%'", c[EngineCodes.SqlServer]);

        }


        [Fact]
        public void Test_Define_NestedCondition()
        {
            var query = new Query("Orders")
               .Define("@shipReg", null)
               .Define("@one", 1)
               .Where(q =>
                   q.Where("ShipRegion", "!=", Variable("@shipReg"))
                //    .WhereRaw("1 = @one")
                ).SelectCount();

            var c = Compile(query);

            Assert.Equal("SELECT COUNT(*) AS [count] FROM [Orders] WHERE ([ShipRegion] != NULL)", c[EngineCodes.SqlServer]);

        }


        [Fact]
        public void Test_Define_WhereDate()
        {
            var dateObj = new System.DateTime(year: 1996, month: 8, day: 1);

            var query = new Query("Orders")
                .Define("@d", dateObj)
                .WhereDate("RequiredDate", Variable("@d"));


            var query2 = new Query("Orders")
                .Define("@d", 1996)
                .WhereDatePart("year", "RequiredDate", "=", Variable("@d"));

            var query3 = new Query("Orders")
                .Define("@d", "00:00:00")
                .WhereTime("RequiredDate", "!=", Variable("@d"));

            var c = Compile(query);
            var c2 = Compile(query2);
            var c3 = Compile(query3);

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
            var query = new Query("Customers")
                .WhereExists(q => q.From("Orders")
                .Define("@postal", "8200")
                .Where("ShipPostalCode", Variable("@postal"))
            );

            var c = Compile(query);
            Assert.Equal("SELECT * FROM [Customers] WHERE EXISTS (SELECT 1 FROM [Orders] WHERE [ShipPostalCode] = '8200')", c[EngineCodes.SqlServer]);
        }



        [Fact]
        public void Test_Define_With()
        {

            var query = new Query("Products")
                        .Define("@unit", 10)
                        .Join("Categories", "Categories.CategoryID", "Products.CategoryID")
                        .Select("Categories.CategoryName", "Products.UnitPrice")
                        .Where("Products.UnitPrice", ">", Variable("@unit"));

            var queryCTe = new Query("prodCTE")
                            .With("prodCTE", query);

            var c = Compile(queryCTe);


            Assert.Equal("WITH [prodCTE] AS (SELECT [Categories].[CategoryName], [Products].[UnitPrice] FROM [Products] \nINNER JOIN [Categories] ON [Categories].[CategoryID] = [Products].[CategoryID] WHERE [Products].[UnitPrice] > 10)\nSELECT * FROM [prodCTE]", c[EngineCodes.SqlServer]);
        }



        /*
        [Fact]
        public void Test_Define_WithRaw()
        {

            //WithRaw
            var query = new Query("prodCTE")
                .Define("@unit", 10)
                .Define("@foo", 2)
                .Select("CategoryName", "UnitPrice")
                .WithRaw("prodCTE", "SELECT c.CategoryName, p.UnitPrice FROM Products p INNER JOIN Categories c ON c.CategoryID = p.CategoryID WHERE p.UnitPrice > @unit AND  2 = @foo");

            var c = Compile(query);

            Assert.Equal("WITH [prodCTE] AS (SELECT c.CategoryName, p.UnitPrice FROM Products p INNER JOIN Categories c ON c.CategoryID = p.CategoryID WHERE p.UnitPrice > 10 AND  2 = 2)\nSELECT [CategoryName], [UnitPrice] FROM [prodCTE]", c[EngineCodes.SqlServer]);

        }
        */

        //
        [Fact]
        public void Test_Define_Union()
        {
            var q1 = new Query("Suppliers")
                        .Define("@foo", "Beirut")
                        .Select("City")
                        .Where("City", Variable("@foo"));

            var q2 = new Query("Customers")
                            .Define("@city", "Z")
                            .Select("City")
                            .Union(q1)
                            .WhereNotLike("City", Variable("@city"));

            var c = Compile(q2);
            Assert.Equal("SELECT [City] FROM [Customers] WHERE NOT (LOWER([City]) like 'z') UNION SELECT [City] FROM [Suppliers] WHERE [City] = 'Beirut'", c[EngineCodes.SqlServer]);
        }


        [Fact]
        public void Test_Define_Except()
        {
            var q1 = new Query("Suppliers")
                        .Define("@foo", "Beirut")
                        .Select("City")
                        .Where("City", Variable("@foo"));

            var q2 = new Query("Customers")
                        .Define("@city", "Z")
                        .Select("City")
                        .Except(q1)
                        .WhereNotLike("City", Variable("@city"));

            var c = Compile(q2);
            Assert.Equal("SELECT [City] FROM [Customers] WHERE NOT (LOWER([City]) like 'z') EXCEPT SELECT [City] FROM [Suppliers] WHERE [City] = 'Beirut'", c[EngineCodes.SqlServer]);
        }

        [Fact]
        public void Test_Define_Intersect()
        {
            var q1 = new Query("Suppliers")
                        .Define("@foo", "Beirut")
                        .Select("City")
                        .Where("City", Variable("@foo"));

            var q2 = new Query("Customers")
                        .Define("@city", "Z")
                        .Select("City")
                        .Intersect(q1)
                        .WhereNotLike("City", Variable("@city"));

            var c = Compile(q2);
            Assert.Equal("SELECT [City] FROM [Customers] WHERE NOT (LOWER([City]) like 'z') INTERSECT SELECT [City] FROM [Suppliers] WHERE [City] = 'Beirut'", c[EngineCodes.SqlServer]);
        }

        /*
        [Fact]
        public void Test_Define_CombineRaw()
        {

            var query = new Query("Customers")
                        .Define("@foo", 1)
                        .Define("@faa", 2)
                        .Select("City")
                        .CombineRaw("UNION ALL SELECT City FROM Suppliers WHERE 1 = @foo AND 2 = @faa");

            var c = Compile(query);
            Assert.Equal("SELECT [City] FROM [Customers] UNION ALL SELECT City FROM Suppliers WHERE 1 = 1 AND 2 = 2", c[EngineCodes.SqlServer]);
        }
        */

    }
}
