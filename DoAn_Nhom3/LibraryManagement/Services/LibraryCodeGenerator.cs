using System.Linq.Expressions;
using System.Text.RegularExpressions;
using LibraryManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Services;

public interface ILibraryCodeGenerator
{
}

public class LibraryCodeGenerator : ILibraryCodeGenerator
{
    private readonly LibraryDbContext _db;

    public LibraryCodeGenerator(LibraryDbContext db)
    {
        _db = db;
    }
}