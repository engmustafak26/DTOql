using Demo.Domain;
using Demo.DTO.Organizations;
using Demo.DTO.Roles;
using Demo.Infrastructure;
using DTOql.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Demo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RoleController : DtoBaseController<Role, RoleListDto, RoleSearchDto, RoleListDto, RoleListDto, RoleListDto>
    {
        public RoleController(IService<Role> service) : base(service)
        {
        }
    }
}
