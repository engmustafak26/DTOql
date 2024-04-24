using Demo.Domain;
using Demo.DTO.Organizations;
using Demo.Infrastructure;
using DTOql.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Demo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrganizationController : DtoWithGraphBaseController<Organization, OrganizationListDto, OrganizationSearchDto, OrganizationListDto, OrganizationListDto>
    {
        public OrganizationController(IService<Organization> service) : base(service)
        {
        }
    }
}
