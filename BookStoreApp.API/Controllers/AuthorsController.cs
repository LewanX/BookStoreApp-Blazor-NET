using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookStoreApp.API.Data;
using BookStoreApp.API.Models.Author;
using AutoMapper;
using BookStoreApp.API.Static;

namespace BookStoreApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorsController : ControllerBase
    {
        private readonly BookStoreDbContext _context;
        private readonly IMapper mapper;
        private readonly ILogger<AuthorsController> logger;

        public AuthorsController(BookStoreDbContext context,IMapper mapper,ILogger<AuthorsController> logger)
        {
            _context = context;
            this.mapper = mapper;
            this.logger = logger; 
        }

        // GET: api/Authors
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AuthorReadOnlyDto>>> GetAuthors()
        {
            logger.LogInformation($"Request to {nameof(GetAuthors)}");
            try
            {
                var authors = await _context.Authors.ToListAsync();
                var authorsDTO = mapper.Map<IEnumerable<AuthorReadOnlyDto>>(authors);
                return Ok(authorsDTO);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error Performing GET {nameof(GetAuthors)}");
                return StatusCode(500,Messages.Error500Message);
            }
            
        }

        // GET: api/Authors/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AuthorReadOnlyDto>> GetAuthor(int id)
        {
            
            try
            {
                var author = await _context.Authors.FindAsync(id);
                if (author == null)
                {
                    logger.LogWarning($"Record Not Found {nameof(GetAuthor)} - ID: {id}");
                    return NotFound();
                }
                var authorDTO = mapper.Map<AuthorReadOnlyDto>(author);
                return Ok(authorDTO);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error Performing GET {nameof(GetAuthors)}");
                return StatusCode(500, Messages.Error500Message);
            }



           
        }

        // PUT: api/Authors/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult>PutAuthor(int id, AuthorUpdateDto authorDTO)
        {

            var author = await _context.Authors.FindAsync(id);
            if (id!= authorDTO.Id)
            {
                logger.LogWarning($"Update ID Not Found {nameof(PutAuthor)} - ID: {id}");
                return BadRequest();
            }
            if (author==null)
            {
                logger.LogWarning($" {nameof(Author)} record not found in {nameof(PutAuthor)} - ID:{id}");
                return NotFound();
            }
             mapper.Map(authorDTO, author);
            _context.Entry(author).State=EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!AuthorExists(id))
                {
                    return NotFound();
                }
                else
                {
                    logger.LogError(ex,$"Error Performing GET in {nameof(PutAuthor)}");
                    return StatusCode(500, Messages.Error500Message);
                }
                
            }
            return NoContent();

        }

        // POST: api/Authors
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<AuthorCreateDTO>> PostAuthor(AuthorCreateDTO authorDTO)
        {
            try
            {

                var author = mapper.Map<Author>(authorDTO);
                _context.Authors.Add(author);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetAuthor", new { id = author.Id }, author);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error Performing POST in {nameof(PostAuthor)}", authorDTO);
                return StatusCode(500,Messages.Error500Message);
                
            }
         
        }

        // DELETE: api/Authors/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAuthor(int id)
        {
            try
            {
                if (_context.Authors == null)
                {
                    logger.LogWarning($"{nameof(Author)} record not found in {nameof(DeleteAuthor)} - ID:{id}");
                    return NotFound();
                }
                var author = await _context.Authors.FindAsync(id);
                if (author == null)
                {
                    return NotFound();
                }

                _context.Authors.Remove(author);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                logger.LogError(ex,$"Error Performing DELETE in {nameof(DeleteAuthor)}");
                return StatusCode(500, Messages.Error500Message);
            }
           
        }

        private bool AuthorExists(int id)
        {
            return (_context.Authors?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
