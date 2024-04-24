using DTOql.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;


namespace Demo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DtoBaseController<TEntity, ListDto, SearchDto, GetItemDto, CreateDto, UpdateDto> : ControllerBase
                                                                                   where TEntity : class
                                                                                   where SearchDto : ISearch
                                                                                   where CreateDto : class
                                                                                   where UpdateDto : class

    {


        protected readonly IService<TEntity> _service;

        public DtoBaseController(IService<TEntity> service)
        {
            _service = service;
        }

        [HttpPost]
        [Route("GetList")]
        public virtual async Task<IActionResult> GetListAsync([FromBody] SearchDto dto)
        {
            return Ok(await _service.GetAsync(typeof(ListDto), dto));
        }

        [HttpPost]
        [Route("Get/{id}")]
        public virtual async Task<IActionResult> GetAsync(long id)
        {
            return Ok(await _service.GetAsync(typeof(GetItemDto), id));
        }

        [HttpPost]
        [Route("Create")]
        public virtual async Task<IActionResult> CreateAsync([FromBody] CreateDto dto)
        {

            return Ok(await _service.AddAsync(dto));

        }

        [HttpPost]
        [Route("Update")]
        public virtual async Task<IActionResult> UpdateAsync([FromBody] UpdateDto dto)
        {
            return Ok(await _service.EditAsync(dto));
        }

        [HttpPost]
        [Route("Delete/{id}")]
        public virtual async Task<IActionResult> DeleteAsync(long id)
        {

            return Ok(await _service.RemoveAsync(id));
        }

        [HttpPost]
        [Route("Restore/{id}")]
        public virtual async Task<IActionResult> RestoreAsync(long id)
        {

            return Ok(await _service.RemoveAsync(id, true));
        }
    }
}
