using SqlKata.Compilers;
using SqlKata.Extensions;
using SqlKata.Tests.Infrastructure;
using Xunit;

namespace SqlKata.Tests
{
    /// <summary>
    /// If you want to test this queries against a database use NorthWind database
    /// </summary>
    public class WithVarTest : TestSupport
    {

        [Fact]
        public void Test_WithVar_WhereRaw()
        {

            var query = new Query("Products")
              .WithVar("@name", "Anto")
              .WithVar("@categoryID", 2)
              .WhereRaw("ProductName LIKE '%' + @name + '%' AND 2 = @categoryID")
              .WhereRaw("CategoryID = @categoryID AND 2 = @categoryID")
              .Where(new { UnitsOnOrder = 0 });

            var c = Compile(query);

            Assert.Equal("SELECT * FROM [Products] WHERE ProductName LIKE '%' + 'Anto' + '%' AND 2 = 2 AND CategoryID = 2 AND 2 = 2 AND [UnitsOnOrder] = 0", c[EngineCodes.SqlServer]);

        }


        [Fact]
        public void Test_WithVar_WhereIn()
        {
            var query = new Query("Customers")
                .WithVar("@regions", new[] { "SP", "BC" })
                .WhereIn<string>("Region", "@regions");

            var c = Compile(query);
            Assert.Equal("SELECT * FROM [Customers] WHERE [Region] IN ('SP', 'BC')", c[EngineCodes.SqlServer]);

        }

        [Fact]
        public void Test_WithVar_SubQuery()
        {

            var subquery = new Query("Products")
                .AsAverage("unitprice")
                .WithVar("@UnitsInSt", 10)
                .Where("UnitsInStock", ">", "@UnitsInSt");

            var query = new Query("Products")
                        .Where("unitprice", ">", subquery)
                        .Where("UnitsOnOrder", ">", 5);

            var c = Compile(query);

            Assert.Equal("SELECT * FROM [Products] WHERE [unitprice] > (SELECT AVG([unitprice]) AS [avg] FROM [Products] WHERE [UnitsInStock] > 10) AND [UnitsOnOrder] > 5", c[EngineCodes.SqlServer]);

        }


        [Fact]
        public void Test_WithVar_WhereEnds()
        {

            var query1 = new Query("Products")
                        .Select("ProductId", "ProductName")
                        .WithVar("@product", "Coffee")
                        .WhereEnds("ProductName", "@product");


            var query2 = new Query("Products")
                       .Select("ProductId", "ProductName")
                       .WithVar("@product", "Coffee")
                       .WhereEnds("ProductName", "@product", true);

            var c1 = Compile(query1);
            var c2 = Compile(query2);

            Assert.Equal("SELECT [ProductId], [ProductName] FROM [Products] WHERE LOWER([ProductName]) like '%coffee'", c1[EngineCodes.SqlServer]);
            Assert.Equal("SELECT [ProductId], [ProductName] FROM [Products] WHERE [ProductName] like '%Coffee'", c2[EngineCodes.SqlServer]);

        }



        [Fact]
        public void Test_WithVar_WhereStarts()
        {


            var query1 = new Query("Products")
                        .Select("ProductId", "QuantityPerUnit")
                        .WithVar("@perUnit", "12")
                        .WhereStarts("QuantityPerUnit", "@perUnit");


            var query2 = new Query("Products")
                               .Select("ProductId", "QuantityPerUnit")
                               .WithVar("@perUnit", "12")
                               .WhereStarts("QuantityPerUnit", "@perUnit", true);

            var c1 = Compile(query1);
            var c2 = Compile(query2);

            Assert.Equal("SELECT [ProductId], [QuantityPerUnit] FROM [Products] WHERE LOWER([QuantityPerUnit]) like '12%'", c1[EngineCodes.SqlServer]);
            Assert.Equal("SELECT [ProductId], [QuantityPerUnit] FROM [Products] WHERE [QuantityPerUnit] like '12%'", c2[EngineCodes.SqlServer]);
        }


        [Fact]
        public void Test_WithVar_WhereContains()
        {

            var query1 = new Query("Products")
                        .Select("ProductId", "QuantityPerUnit")
                        .WithVar("@perUnit", "500")
                        .WhereContains("QuantityPerUnit", "@perUnit");




            var query2 = new Query("Products")
                               .Select("ProductId", "QuantityPerUnit")
                               .WithVar("@perUnit", "500")
                               .WhereContains("QuantityPerUnit", "@perUnit", true);

            var c1 = Compile(query1);
            var c2 = Compile(query2);

            Assert.Equal("SELECT [ProductId], [QuantityPerUnit] FROM [Products] WHERE LOWER([QuantityPerUnit]) like '%500%'", c1[EngineCodes.SqlServer]);
            Assert.Equal("SELECT [ProductId], [QuantityPerUnit] FROM [Products] WHERE [QuantityPerUnit] like '%500%'", c2[EngineCodes.SqlServer]);

        }


        [Fact]
        public void Test_WithVar_WhereLike()
        {
            var query1 = new Query("Products")
                                      .Select("ProductId", "ProductName", "SupplierID")
                                      .WithVar("@id", 20)
                                      .WhereLike("SupplierID", "@id");


            var query2 = new Query("Products")
                               .Select("ProductId", "ProductName", "SupplierID")
                               .WithVar("@id", 20)
                               .WhereLike("SupplierID", "@id", true);

            var c1 = Compile(query1);
            var c2 = Compile(query2);

            Assert.Equal("SELECT [ProductId], [ProductName], [SupplierID] FROM [Products] WHERE LOWER([SupplierID]) like '20'", c1[EngineCodes.SqlServer]);
            Assert.Equal("SELECT [ProductId], [ProductName], [SupplierID] FROM [Products] WHERE [SupplierID] like 20", c2[EngineCodes.SqlServer]);
        }


        [Fact]
        public void Test_WithVar_WhereInSubquery()
        {

            var subquery = new Query("Orders")
                               .WithVar("@shipId", 3)
                               .Select("ShipVia").Where("ShipVia", "@shipId");


            var query1 = new Query("Shippers")
                               .Select("ShipperID", "CompanyName")
                               .WhereIn("ShipperID", subquery);


            var c1 = Compile(query1);

            Assert.Equal("SELECT [ShipperID], [CompanyName] FROM [Shippers] WHERE [ShipperID] IN (SELECT [ShipVia] FROM [Orders] WHERE [ShipVia] = 3)", c1[EngineCodes.SqlServer]);
        }

        [Fact]
        public void Test_WithVar_Having()
        {
            var c = Compile(new Query("Table")
                .WithVar("@foo",1)
                .Having("Id", "=", "@foo"));

            Assert.Equal("SELECT * FROM [Table] HAVING [Id] = 1", c[EngineCodes.SqlServer]);
        }

        [Fact]
        public void Test_WithVar_HavingRaw()
        {
            var query1 = new Query("Orders")
                               .WithVar("@count", 80)
                               .Select("Employees.LastName")
                               .SelectRaw("COUNT(Orders.OrderID) AS NumberOfOrders")
                               .Join("Employees", "Employees.EmployeeID", "Orders.EmployeeID")
                               .GroupBy("LastName")
                               .HavingRaw("COUNT(Orders.OrderID) > @count");

            var c = Compile(query1);

            Assert.Equal("SELECT [Employees].[LastName], COUNT(Orders.OrderID) AS NumberOfOrders FROM [Orders] \nINNER JOIN [Employees] ON [Employees].[EmployeeID] = [Orders].[EmployeeID] GROUP BY [LastName] HAVING COUNT(Orders.OrderID) > 80", c[EngineCodes.SqlServer]);

        }

        [Fact]
        public void Test_WithVar_HavingStarts()
        {

            var query = new Query("Customers")
                        .WithVar("@label", "U")
                        .SelectRaw("COUNT(CustomerID)")
                        .Select("Country")
                        .GroupBy("Country")
                        .HavingStarts("Country", "@label");

            var c = Compile(query);

            Assert.Equal("SELECT COUNT(CustomerID), [Country] FROM [Customers] GROUP BY [Country] HAVING LOWER([Country]) like 'u%'", c[EngineCodes.SqlServer]);

        }



        [Fact]
        public void Test_WithVar_Having_Ends()
        {
            var query = new Query("Customers")
                            .WithVar("@label", "d")
                            .SelectRaw("COUNT(CustomerID)")
                            .Select("Country")
                            .GroupBy("Country")
                            .HavingEnds("Country", "@label");

            var c = Compile(query);

            Assert.Equal("SELECT COUNT(CustomerID), [Country] FROM [Customers] GROUP BY [Country] HAVING LOWER([Country]) like '%d'", c[EngineCodes.SqlServer]);
        }


        [Fact]
        public void Test_WithVar_Having_Contains()
        {


            var query = new Query("Customers")
                            .WithVar("@label", "d")
                            .SelectRaw("COUNT(CustomerID)")
                            .Select("Country")
                            .GroupBy("Country")
                            .HavingContains("Country", "@label");

            var c = Compile(query);

            Assert.Equal("SELECT COUNT(CustomerID), [Country] FROM [Customers] GROUP BY [Country] HAVING LOWER([Country]) like '%d%'", c[EngineCodes.SqlServer]);

        }


        [Fact]
        public void Test_WithVar_NestedCondition()
        {
            var query = new Query("Orders")
               .WithVar("@shipReg", null)
               .WithVar("@one", 1)
               .Where(q =>
                   q.Where("ShipRegion", "!=", "@shipReg")
                   .WhereRaw("1 = @one")
                ).AsCount();

            var c = Compile(query);

            Assert.Equal("SELECT COUNT(*) AS [count] FROM [Orders] WHERE ([ShipRegion] != NULL AND 1 = 1)", c[EngineCodes.SqlServer]);

        }


        [Fact]
        public void Test_WithVar_WhereDate()
        {
            var dateObj = new System.DateTime(year: 1996, month: 8, day: 1);

            var query = new Query("Orders")
                .WithVar("@d", dateObj)
                .WhereDate("RequiredDate", "@d");


            var query2 = new Query("Orders")
                .WithVar("@d", 1996)
                .WhereDatePart("year", "RequiredDate", "=", "@d");

            var query3 = new Query("Orders")
                .WithVar("@d", "00:00:00")
                .WhereTime("RequiredDate", "!=", "@d");

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
        public void Test_WithVar_WhereExists()
        {
            var query = new Query("Customers").WhereExists(q => q.From("Orders").WithVar("@postal", "8200").Where("ShipPostalCode", "@postal"));
            var c = Compile(query);
            Assert.Equal("SELECT * FROM [Customers] WHERE EXISTS (SELECT TOP (1) 1 FROM [Orders] WHERE [ShipPostalCode] = '8200')", c[EngineCodes.SqlServer]);
        }



        [Fact]
        public void Test_WithVar_With()
        {

            var query = new Query("Products")
                        .WithVar("@unit", 10)
                        .Join("Categories", "Categories.CategoryID", "Products.CategoryID")
                        .Select("Categories.CategoryName", "Products.UnitPrice")
                        .Where("Products.UnitPrice", ">", "@unit");

            var queryCTe = new Query("prodCTE")
                            .With("prodCTE", query);

            var c = Compile(queryCTe);


            Assert.Equal("WITH [prodCTE] AS (SELECT [Categories].[CategoryName], [Products].[UnitPrice] FROM [Products] \nINNER JOIN [Categories] ON [Categories].[CategoryID] = [Products].[CategoryID] WHERE [Products].[UnitPrice] > 10)\nSELECT * FROM [prodCTE]", c[EngineCodes.SqlServer]);
        }



        [Fact]
        public void Test_WithVar_WithRaw()
        {

            //WithRaw
            var query = new Query("prodCTE")
                .WithVar("@unit", 10)
                .WithVar("@foo", 2)
                .Select("CategoryName", "UnitPrice")
                .WithRaw("prodCTE", "SELECT c.CategoryName, p.UnitPrice FROM Products p INNER JOIN Categories c ON c.CategoryID = p.CategoryID WHERE p.UnitPrice > @unit AND  2 = @foo");

            var c = Compile(query);

            Assert.Equal("WITH [prodCTE] AS (SELECT c.CategoryName, p.UnitPrice FROM Products p INNER JOIN Categories c ON c.CategoryID = p.CategoryID WHERE p.UnitPrice > 10 AND  2 = 2)\nSELECT [CategoryName], [UnitPrice] FROM [prodCTE]", c[EngineCodes.SqlServer]);

        }
        //
        [Fact]
        public void Test_WithVar_Union()
        {
            var q1 = new Query("Suppliers")
                        .WithVar("@foo", 1)
                        .WithVar("@faa", 2)
                        .Select("City").WhereRaw("1 = @foo AND 2 = @faa");

            var q2 = new Query("Customers")
                            .WithVar("@city", "Z")
                            .Select("City")
                            .Union(q1)
                            .WhereNotLike("City", "@city");

            var c = Compile(q2);
            Assert.Equal("SELECT [City] FROM [Customers] WHERE NOT (LOWER([City]) like 'z') UNION SELECT [City] FROM [Suppliers] WHERE 1 = 1 AND 2 = 2", c[EngineCodes.SqlServer]);
        }


        [Fact]
        public void Test_WithVar_Except()
        {
            var q1 = new Query("Suppliers")
                        .WithVar("@foo", 1)
                        .WithVar("@faa", 2)
                        .Select("City").WhereRaw("1 = @foo AND 2 = @faa");

            var q2 = new Query("Customers")
                        .WithVar("@city", "Z")
                        .Select("City")
                        .Except(q1)
                        .WhereNotLike("City", "@city");

            var c = Compile(q2);
            Assert.Equal("SELECT [City] FROM [Customers] WHERE NOT (LOWER([City]) like 'z') EXCEPT SELECT [City] FROM [Suppliers] WHERE 1 = 1 AND 2 = 2", c[EngineCodes.SqlServer]);
        }

        [Fact]
        public void Test_WithVar_Intersect()
        {
            var q1 = new Query("Suppliers")
                         .WithVar("@foo", 1)
                         .WithVar("@faa", 2)
                         .Select("City").WhereRaw("1 = @foo AND 2 = @faa");

            var q2 = new Query("Customers")
                        .WithVar("@city", "Z")
                        .Select("City")
                        .Intersect(q1)
                        .WhereNotLike("City", "@city");

            var c = Compile(q2);
            Assert.Equal("SELECT [City] FROM [Customers] WHERE NOT (LOWER([City]) like 'z') INTERSECT SELECT [City] FROM [Suppliers] WHERE 1 = 1 AND 2 = 2", c[EngineCodes.SqlServer]);
        }

        [Fact]
        public  void Test_WithVar_CombineRaw()
        {

            var query = new Query("Customers")
                        .WithVar("@foo", 1)
                        .WithVar("@faa", 2)
                        .Select("City")
                        .CombineRaw("UNION ALL SELECT City FROM Suppliers WHERE 1 = @foo AND 2 = @faa");

            var c = Compile(query);
            Assert.Equal("SELECT [City] FROM [Customers] UNION ALL SELECT City FROM Suppliers WHERE 1 = 1 AND 2 = 2", c[EngineCodes.SqlServer]);
        }

    }
}
