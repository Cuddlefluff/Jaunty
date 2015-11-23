**Jaunty**  
  
Hi, this is a little project I made for myself. It's a CRUD wrapper put on top of Dapper.
It allows inserting, updating and deleting rows on the database.
  
It's built with the intention of supporting multiple databases in a modular manner.
As of right now, SQL Server and PostgreSQL is supported.
  
**Why?**  
  
Well, I started a pet project, and I decided to try keeping it simple by just using Dapper and SQL, without any type of major ORM.
This worked fine, but it turned out that writing and maintaining `INSERT` and `UPDATE` statements is extremely tedious and prone to errors.
So I decided to go online and find something that could fix this problem for me, but most projects were either slow performers, or they had grown "overambitious" rather than just sticking to one thing.
I think this is what makes Dapper popular - it doesn't try to exceed its own scope, and that's what I'm trying to do as well; make it do one thing and do it well.  
  
**Ok, so over to some instructions**  
  
Well, it's simple; all you really need is to install one of the providers (which includes dependencies to Jaunty and Dapper);  
  
`Install-Package Jaunty.SqlClient` for Microsoft SQL Server and `Install-Package Jaunty.PgSql` for PostgreSQL.  
  
After this, you need to configure which provider to use; `Jaunty.SqlClient.SqlClientCommandExecutor.Configure();` or `Jaunty.PgSql.PgSqlCommandExecutor.Configure();`.  
  
Defining schema's is the same as you'd expect in EntityFramework or basically anything else. For example :   
  
```csharp  
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("MyTable")]
public class MyTable
{
	[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public int Id {get;set;}
		
	public string Name {get;set;}

	[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
	public DateTime Created {get;set;}
}
```
All schema attributes are optional (except Key and DatabaseGenerated), as they have sane defaults : 
Tablemame will default to the type name  
Properties will default to the field name  
Schema will default to "dbo" for SQL Server and "public" for Postgre  
Database will assume whatever's supplied in the connection string  


And the SQL to create this table would be something like this :  

```sql  
CREATE TABLE MyTable (
	Id INT NOT NULL IDENTITY(1, 1) PRIMARY KEY CLUSTERED,
	Name NVARCHAR(100) NULL,
	Created DATETIME2 NOT NULL DEFAULT(SYSUTCDATETIME())
)
```

Alternatively for PostgreSQL  

```sql
CREATE TABLE "MyTable" (
    "Id" SERIAL NOT NULL PRIMARY KEY CLUSTERED,
	"Name" VARCHAR(100) NULL,
	"Created" TIMESTAMP NOT NULL DEFAULT(clock_timestamp())
)
```

One of the interesting parts of Jaunty is that it uses the OUTPUT and RETURNING clauses to return the values inserted or updated, which means that database generated data will be automatically populated, which makes it far easier to use stuff like `DEFAULT` and `NEWSEQUENTIALID()` or `uuid_generate_v1()`.

Jaunty can also assign databases to tables, which was a requirement of mine because I needed initially certain tables to be placed in different databases (SQL Server Only).

Now, to insert some stuff..

```csharp

public void Test()
{
	using(var connection = new System.Data.SqlClient.SqlConnection("..."))
	{

		var objectToInsert = new MyTable() { Name = "Hello" };

		var result = connection.Insert(objectToInsert);

		// result should now contain an object with an Id, the string "Hello" in Name and the current date and time.
		// now let's modify

		result = connection.Update(result.SetProperties(new MyTable() { Name = "Hello after update" }));

		// You can use any object, you don't have to use MyTable, but if you do you'll get static typing for your updates.
		// However when you do this, make sure there are no special operations in the constructor or default values in the class
		// Or Jaunty will mistakenly believe they have been altered no matter what.
		
		result = connection.Update(result.SetProperties(new { Name = "Updated with an anonymous type" });

		// As you can see, you can also use anonymous types.

		// It also supports retrieving a single item using the keys on the object, like so :

		var findById = connection.FindById<MyTable>(new { Id = 1 });

		// To delete an object just Delete it.

		connection.Delete(result);

		// you can also delete using only the Id, but I had to name that one DeleteById in order to not caus ambiguity.

		connection.DeleteById(new { Id = 1 });

	}
}

```

All methods also have collection and Async counterparts.

**MySQL**

I won't add support for MySQL right now (why would you use MySQL when you can use Postgre anyway?), as it doesn't support `OUTPUT` or `RETURNING`.
It's still possible to implement support, but that would add performance penalties, as the `INSERT`, `UPDATE` and `DELETE` statements wouldn't work on their own, meaning a second call would be needed.  

It would also be a lot of work to add support for a database I personally don't need.

