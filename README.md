# RepositoryLibrary
When using direct sql to implement my repository interfaces I noticed there tended to be a lot of repeated code and magic strings scattered throughout the code. This library uses a combination of custom attributes and reflection to tackle these issues and provide a tested clean and easy to use api.
## Getting Started
These instructions will get you a copy of the project up and running on your local machine for development and testing purposes.
### Prerequisites
You will need visual studio 15 or later to open and compile this project.
### Examples
Create a model which will represent the output of a query and apply column and property attributes to the required properties.
Columm attributes should be used to link the table/output column name to the property.
Parameter attributes should be used to link the parameter name and type to the property.
A maximum of one column and one parameter attribute should be used per property.
```
public class User 
{
    [Parameter(Name = "@userId", Type = System.Data.SqlDbType.Int)]
    public int UserId { get; set; }

    [Parameter(Name = "@name", Type = System.Data.SqlDbType.VarChar, Length = 50)]
    public string Name { get; set; }

    [Parameter(Name = "@age", Type = System.Data.SqlDbType.Int)]
    public int Age { get; set; }    
}

public class Item
{
    [Column(Name="ITEM_ID")]
    public int ItemId { get; set; }
    
    [Column(Name="ITEM_NAME")]
    public string ItemName { get; set; }
    
    [Column(Name="PRICE")]
    public double Price { get; set; }
}
```
To use the repository API a user should create a repository class which inherits from the RepositoryBase class the API can then be called as shown below.
```
public class AccountRepository : RepositoryLibrary.Database.RepositoryBase, IAccountRepository
{
    public IEnumerable<Item> GetPurchaseHistory(User user)
    {
        using (SqlCommand command = new SqlCommand())
        {            
            AddInputParameters(command, inputUser, new List<string>
            {
                "@userId" // to prevent all parameters from being added by default we can add a filter list which contains the names of the parameters we wish to add
            });                
            return ExecuteReaderSP<Item>(command, "usp_GetPurchaseHistory");            
        }              
    }
}
```
