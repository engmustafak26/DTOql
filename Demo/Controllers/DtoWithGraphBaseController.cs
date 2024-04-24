using DTOql.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;


namespace Demo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DtoWithGraphBaseController<TEntity, ListDto, SearchDto, GetItemDto, SaveDto> : ControllerBase
                                                                                   where TEntity : class
                                                                                   where SearchDto : ISearch
                                                                                   where SaveDto : class, IEntityState

    {


        protected readonly IService<TEntity> _service;

        public DtoWithGraphBaseController(IService<TEntity> service)
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
        [Route("Save")]
        public async Task<IActionResult> SaveAsync([FromBody] SaveDto dto)
        {

            return Ok(await _service.SaveRangeAsync(new SaveDto[] { dto }));

        }

        [HttpPost]
        [Route("SaveAll")]
        public async Task<IActionResult> SaveAllAsync([FromBody] SaveDto[] dto)
        {

            return Ok(await _service.SaveRangeAsync(dto));

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
