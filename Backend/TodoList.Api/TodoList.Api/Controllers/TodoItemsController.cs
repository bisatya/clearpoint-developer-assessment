using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using TodoList.Api.Exceptions;
using TodoList.Api.Services;

namespace TodoList.Api.Controllers
{
    /*
     * Note:
     * The idea is to keep the controller layer as lean as possible
     * Controllers should call the service layer which contains all the business logic
     * Sometimes, it's prudent to put the service layer in a separate assembly
     * That way, the "core" assembly can be reused by other solutions (e.g., web api, console app)
     */
    [Route("api/[controller]")]
    [ApiController]
    public class TodoItemsController : ControllerBase
    {
        private readonly ITodoItemsService _todoItemsService;
        private readonly ILogger<TodoItemsController> _logger;

        public TodoItemsController(ITodoItemsService todoItemsService, ILogger<TodoItemsController> logger)
        {
            _todoItemsService = todoItemsService;
            _logger = logger;
        }

        // GET: api/TodoItems
        [HttpGet]
        public async Task<IActionResult> GetTodoItems()
        {
            var results = await _todoItemsService.GetTodoItemsAsync(false);
            return Ok(results);
        }

        // GET: api/TodoItems/...
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTodoItem(Guid id)
        {
            try
            {
                var result = await _todoItemsService.GetTodoItemAsync(id);
                return Ok(result);
            }
            catch (ResourceNotFoundException ex)
            {
                _logger.LogError(ex, null);
                return NotFound();
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, null);
                return BadRequest(ex.Message);
            }
        }

        // PUT: api/TodoItems/... 
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTodoItem(Guid id, TodoItem todoItem)
        {
            try
            {
                await _todoItemsService.UpdateTodoItemAsync(id, todoItem);
                return NoContent();
            }
            catch (ResourceNotFoundException ex)
            {
                _logger.LogError(ex, null);
                return NotFound();
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, null);
                return BadRequest(ex.Message);
            }
        } 

        // POST: api/TodoItems 
        [HttpPost]
        public async Task<IActionResult> PostTodoItem(TodoItem todoItem)
        {
            try
            {
                var result = await _todoItemsService.CreateTodoItemAsync(todoItem);
                return CreatedAtAction(nameof(GetTodoItem), new { id = result.Id }, result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, null);
                return BadRequest(ex.Message);
            }
        } 
    }
}
