using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Models;
using TodoApi.Services;

namespace TodoApi.Controllers
{
    [Route("api/todos")]
    [ApiController]
    public class TodoController : ControllerBase
    {
        private TodoContext db;
        private readonly SentimentAnalysisService _sentimentAnalysisService;

        public TodoController(TodoContext dbContext, SentimentAnalysisService sentimentAnalysisService)
        {
            this.db = dbContext;
            _sentimentAnalysisService = sentimentAnalysisService;
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IEnumerable<Todo>> GetTodosAsync()
        {
            return await db.Todos.ToListAsync();
        }

        [HttpGet("{id}", Name ="GetById")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult<Todo>> GetTodoAsync(int id)
        {
            var item = await db.Todos.FindAsync(id);
            if (item != null)
                return Ok(item);
            else
                return NotFound();
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult<Todo>> AddTodoAsync([FromBody]Todo todo)
        {
            TryValidateModel(todo);
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            else
            {
                await db.Todos.AddAsync(todo);
                await db.SaveChangesAsync();
                return CreatedAtRoute("GetById",new { id = todo.Id }, todo);
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult<Todo>> DeleteAsync(int id)
        {
            var item = await db.Todos.FindAsync(id);
            if(item!=null)
            {
                db.Todos.Remove(item);
                await db.SaveChangesAsync();
                return Ok(item);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult<Todo>> UpdateAsync(int id, Todo todo)
        {
            var item = await db.Todos.FindAsync(id);
            if(item!=null)
            {
                TryValidateModel(todo);
                if (ModelState.IsValid)
                {
                    item.Title = todo.Title;
                    item.IsCompleted = todo.IsCompleted;
                    await db.SaveChangesAsync();
                    return Ok(item);
                }
                else
                {
                    return BadRequest(ModelState);
                }
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost("analyze-sentiment")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult<string>> AnalyzeSentiment([FromBody] string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return BadRequest("Text cannot be null or empty.");
            }

            var sentiment = await _sentimentAnalysisService.AnalyzeSentimentAsync(text);
            return Ok(sentiment);
        }
    }
}
