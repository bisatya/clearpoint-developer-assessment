using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TodoList.Api.Services;

/*
 * Note:
 * In a more complex application, it might be worth it to have a DTO class to avoid passing around "entity object"
 * That way we can have better control over which properties we want to expose to the consumers of this Service
 * and avoid exposing foreign key relationships that entity objects often have
 */
public interface ITodoItemsService
{
    Task<IEnumerable<TodoItem>> GetTodoItemsAsync(bool includeAll);
    
    Task<TodoItem> GetTodoItemAsync(Guid id);

    Task<TodoItem> CreateTodoItemAsync(TodoItem newTodoItem);

    Task UpdateTodoItemAsync(Guid id, TodoItem updatedTodoItem);
}