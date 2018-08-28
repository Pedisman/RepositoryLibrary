using System;

namespace RepositoryLibrary.Attributes
{
    public abstract class DAL_AttributeBase : Attribute
    {
        public string Name { get; set; }
    }
}
