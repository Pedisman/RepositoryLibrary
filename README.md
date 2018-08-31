# RepositoryLibrary
When using direct sql to implement my repository interfaces I noticed there tended to be a lot of repeated code and magic strings scattered throughout the code. This library uses a combination of custom attributes and reflection to tackle these issues and provide a tested clean and easy to use api.
## Getting Started
These instructions will get you a copy of the project up and running on your local machine for development and testing purposes.
### Prerequisites
You will need visual studio 15 or later to open and compile this project.
### Examples
Create a model and apply column and property attributes to the required properties.
Columm attributes should be used to link the table/output column name to the property.
Parameter attributes should be used to link the parameter name and type to the property.
A maximum of one column and one parameter attribute should be used per property.
```
public class User 
{
  [Column(Name="NAME")]
  [Parameter(Name = "@name", Type = System.Data.SqlDbType.VarChar, Length = 50)]
  public string Name { get; set; }
  
  [Column(Name="AGE")]
  [Parameter(Name = "@age", Type = System.Data.SqlDbType.Int)]
  public int Age { get; set; }
}
```
