using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TodoList.Api.Exceptions;

[assembly: InternalsVisibleTo("TodoList.Api.UnitTests")]
namespace TodoList.Api.Services;

internal class TodoItemsService : ITodoItemsService
{
    private readonly TodoContext _context;

    public TodoItemsService(TodoContext context)
    {
        this._context = context;
    }

    /*
     * Note:
     * When we have lots of items in the DB, it might be worth it to consider adding paging and other filtering (e.g. created date, etc)
     * That way, we don't have to load the entire data into memory
     */
    public async Task<IEnumerable<TodoItem>> GetTodoItemsAsync(bool includeAll)
    {
        var result = await TodoItemsQuery(includeAll).ToListAsync();
        return result;
    }

    public async Task<TodoItem> GetTodoItemAsync(Guid id)
    {
        EnsureIdIsValid(id);

        var result = await _context.TodoItems.FindAsync(id)
                     ?? throw new ResourceNotFoundException($"Unable to find ToDoItem with ID: {id}");

        return result;
    }

    public async Task<TodoItem> CreateTodoItemAsync(TodoItem newTodoItem)
    {
        EnsurePayloadIsNotNull(newTodoItem);
        EnsureStatusIsIncomplete(newTodoItem);
        EnsureDescriptionIsValid(newTodoItem.Description);
        await EnsureDescriptionDoesNotExist(newTodoItem.Description);

        _context.TodoItems.Add(newTodoItem);
        await _context.SaveChangesAsync();

        return newTodoItem;
    }

    public async Task UpdateTodoItemAsync(Guid id, TodoItem updatedTodoItem)
    {
        EnsurePayloadIsNotNull(updatedTodoItem);
        EnsureIdIsValid(id);
        EnsureIdIsConsistent(id, updatedTodoItem.Id);
        EnsureDescriptionIsValid(updatedTodoItem.Description);

        var todoItem = await GetTodoItemAsync(id);
        todoItem.Description = updatedTodoItem.Description;
        todoItem.IsCompleted = updatedTodoItem.IsCompleted;
        
        await _context.SaveChangesAsync();
    }

    private IQueryable<TodoItem> TodoItemsQuery(bool includeAll) => _context.TodoItems.Where(x => includeAll || !x.IsCompleted);
    
    private static void EnsureIdIsValid(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("ID is required", nameof(id));
    }

    private static void EnsureIdIsConsistent(Guid idFromQueryString, Guid idFromBody)
    {
        if (idFromQueryString != idFromBody)
            throw new ArgumentException("ID is inconsistent");
    }

    private void EnsureDescriptionIsValid(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description is required");
    }

    private async Task EnsureDescriptionDoesNotExist(string description)
    {
        /*
         * Note:
         * String.Equals method with StringComparison will NOT work when we're using SQL Server
         * This code only works because we're using in-memory database
         * With a real DB (e.g., SQL Server) strings comparison are case insensitive by default
         */
        var alreadyExists = await TodoItemsQuery(false).AnyAsync(x => x.Description.Equals(description, StringComparison.OrdinalIgnoreCase));
        if (alreadyExists)
            throw new ArgumentException("Description already exists");
    }

    private static void EnsurePayloadIsNotNull(TodoItem todoItem)
    {
        if (todoItem == null)
            throw new ArgumentNullException(nameof(todoItem));
    }

    private static void EnsureStatusIsIncomplete(TodoItem newTodoItem)
    {
        // assumption: usually people add new to do items with incomplete status
        // of course, this assumption could be wrong and should be verified with the BA
        if (newTodoItem.IsCompleted)
            throw new ArgumentException("Cannot create a completed Todo");
    }
}