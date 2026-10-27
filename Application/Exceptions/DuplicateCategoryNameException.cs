namespace CatalogManager.Application.Exceptions;

public class DuplicateCategoryNameException : Exception
{
    public DuplicateCategoryNameException()
        : base("Category with this name already exists.")
    {
    }
}