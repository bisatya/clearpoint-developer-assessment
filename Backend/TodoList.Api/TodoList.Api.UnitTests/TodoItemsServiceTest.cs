using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TodoList.Api.Exceptions;
using TodoList.Api.Services;
using Xunit;

namespace TodoList.Api.UnitTests
{
    public class TodoContextFixture : IDisposable
    {
        public TodoContext TodoContext { get; }
        
        public TodoContextFixture()
        {
            var optionsBuilder = new DbContextOptionsBuilder<TodoContext>();
            optionsBuilder.UseInMemoryDatabase("TodoItemsDB_Test");
            
            TodoContext = new TodoContext(optionsBuilder.Options);

            SeedData();
        }

        private void SeedData()
        {
            TodoContext.AddRange(
                new TodoItem { Id = new Guid("05aa8172-a582-477f-a0cb-0e5baa103603"), Description = "do laundry", IsCompleted = false },
                new TodoItem { Id = new Guid("fb699b3b-9430-4104-8369-b5ab7df8735c"), Description = "do the dishes", IsCompleted = false },
                new TodoItem { Id = new Guid("4797ba6e-10a1-4af2-8e19-f1a638eae961"), Description = "cook dinner", IsCompleted = true },
                new TodoItem { Id = new Guid("c37d2c3a-499d-4d78-89a1-e4335bffceda"), Description = "mow the lawn", IsCompleted = true }    
            );

            TodoContext.SaveChanges();
        }

        public void Dispose()
        {
            TodoContext.Dispose();
        }
    }
    
    /*
     * Note:
     * There are different views with regards to writing unit tests against the controller, especially if the dependency graph is complex
     * At that point, the tests might be doing too much and it becomes an integration test instead of unit test
     */
    public class TodoItemsServiceTest : IClassFixture<TodoContextFixture>
    {
        private readonly TodoContextFixture _fixture;
        private readonly ITodoItemsService _todoItemsService;

        private const string TODO_ID = "05aa8172-a582-477f-a0cb-0e5baa103603";
        private const string TODO_DESC_INCOMPLETE = "do the dishes";
        private const string TODO_DESC_COMPLETE = "cook dinner";
        
        public TodoItemsServiceTest(TodoContextFixture fixture)
        {
            _fixture = fixture;
            _todoItemsService = new TodoItemsService(fixture.TodoContext);
        }
        
        [Fact]
        public async Task GetTodoItemsAsync_IncludeAllIsTrue_ReturnsAllTodoItems()
        {
            var todoItems = await _todoItemsService.GetTodoItemsAsync(true);

            var expected = await _fixture.TodoContext.TodoItems.CountAsync();
            var actual = todoItems.Count();
            
            Assert.Equal(expected, actual);
        }
        
        [Fact]
        public async Task GetTodoItemsAsync_IncludeAllIsFalse_ReturnsIncompleteTodoItems()
        {
            var todoItems = await _todoItemsService.GetTodoItemsAsync(false);
            
            Assert.All(todoItems, x => Assert.False(x.IsCompleted));
        }
        
        [Fact]
        public async Task GetTodoItemAsync_ValidId_ReturnsTodoItem()
        {
            var expected = new Guid(TODO_ID);
            var todoItem = await _todoItemsService.GetTodoItemAsync(expected);
            var actual = todoItem.Id;
            
            Assert.Equal(expected, actual);
        }
        
        [Fact]
        public async Task GetTodoItemAsync_DefaultId_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>( () => _todoItemsService.GetTodoItemAsync(Guid.Empty));
        }

        [Fact]
        public async Task GetTodoItemAsync_RandomId_ThrowsNotFoundException()
        {
            await Assert.ThrowsAsync<ResourceNotFoundException>(() => _todoItemsService.GetTodoItemAsync(Guid.NewGuid()));
        }
        
        [Fact]
        public async Task CreateTodoItemAsync_ValidPayload_ReturnsTodoItemWithId()
        {
            var newTodoItem = new TodoItem { Description = $"do something - {Guid.NewGuid()}" };
            var createdTodoItem = await _todoItemsService.CreateTodoItemAsync(newTodoItem);

            Assert.NotEqual(Guid.Empty, createdTodoItem.Id);
        }
        
        [Fact]
        public async Task CreateTodoItemAsync_ValidPayload_TodoItemIsStoredInDb()
        {
            var newTodoItem = new TodoItem { Description = $"do something - {Guid.NewGuid()}" };
            var createdTodoItem = await _todoItemsService.CreateTodoItemAsync(newTodoItem);

            var storedInDb = await _fixture.TodoContext.TodoItems.AnyAsync(x => x.Id == createdTodoItem.Id);
            Assert.True(storedInDb);
        }

        [Fact]
        public async Task CreateTodoItemAsync_NullPayload_ThrowsArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _todoItemsService.CreateTodoItemAsync(null));
        }
        
        [Fact]
        public async Task CreateTodoItemAsync_EmptyDescription_ThrowsArgumentException()
        {
            var newTodoItem = new TodoItem { Description = string.Empty };
            await Assert.ThrowsAsync<ArgumentException>(() => _todoItemsService.CreateTodoItemAsync(newTodoItem));
        }
        
        [Fact]
        public async Task CreateTodoItemAsync_DuplicateDescriptionWithIncompleteTodo_ThrowsArgumentException()
        {
            var newTodoItem = new TodoItem { Description = TODO_DESC_INCOMPLETE };
            await Assert.ThrowsAsync<ArgumentException>(() => _todoItemsService.CreateTodoItemAsync(newTodoItem));
        }
        
        [Fact]
        public async Task CreateTodoItemAsync_DuplicateDescriptionWithCompletedTodo_DoesNotThrowException()
        {
            var newTodoItem = new TodoItem { Description = TODO_DESC_COMPLETE };
            var exception = await Record.ExceptionAsync(() => _todoItemsService.CreateTodoItemAsync(newTodoItem));

            Assert.Null(exception);
        }
        
        [Fact]
        public async Task CreateTodoItemAsync_AlreadyComplete_ThrowsArgumentException()
        {
            var description = $"do something - {Guid.NewGuid()}";
            var newTodoItem = new TodoItem { Description = description, IsCompleted = true };
            
            await Assert.ThrowsAsync<ArgumentException>(() => _todoItemsService.CreateTodoItemAsync(newTodoItem));
        }

        [Fact]
        public async Task UpdateTodoItemAsync_ValidPayload_TodoItemIsUpdatedInDb()
        {
            var id = new Guid(TODO_ID);
            var todoItem = new TodoItem
            {
                Id = id,
                Description = "do laundry without sulking",
                IsCompleted = true
            };
            await _todoItemsService.UpdateTodoItemAsync(id, todoItem);

            var todoItemFromDb = await _fixture.TodoContext.TodoItems.SingleAsync(x => x.Id == id);
            
            Assert.Equal(todoItemFromDb.Description, todoItem.Description);
            Assert.Equal(todoItemFromDb.IsCompleted, todoItem.IsCompleted);
        }

        [Fact]
        public async Task UpdateTodoItemAsync_NullPayload_ThrowsArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _todoItemsService.UpdateTodoItemAsync(new Guid(TODO_ID), null));
        }

        [Fact]
        public async Task UpdateTodoItemAsync_EmptyDescription_ThrowsArgumentException()
        {
            var id = new Guid(TODO_ID);
            var todoItem = new TodoItem
            {
                Id = id,
                Description = string.Empty,
                IsCompleted = true
            };

            await Assert.ThrowsAsync<ArgumentException>(() => _todoItemsService.UpdateTodoItemAsync(id, todoItem));
        }

        [Fact]
        public async Task UpdateTodoItemAsync_DefaultId_ThrowsNotFoundException()
        {
            var id = Guid.Empty;
            var todoItem = new TodoItem
            {
                Id = id,
                Description = string.Empty,
                IsCompleted = true
            };

            await Assert.ThrowsAsync<ArgumentException>(() => _todoItemsService.UpdateTodoItemAsync(id, todoItem));
        }

        [Fact]
        public async Task UpdateTodoItemAsync_InconsistentId_ThrowsArgumentException()
        {
            var id = new Guid(TODO_ID);
            var todoItem = new TodoItem
            {
                Id = Guid.Empty,
                Description = string.Empty,
                IsCompleted = true
            };

            await Assert.ThrowsAsync<ArgumentException>(() => _todoItemsService.UpdateTodoItemAsync(id, todoItem));
        }
    }
}
