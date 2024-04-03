using DTOql.ASP;
using DTOql.Continuations;
using DTOql.Enums;
using DTOql.Extensions;
using DTOql.Interfaces;
using DTOql.Models;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DTOql.DataAccess
{
    public class Service<TEntity> : IService<TEntity> where TEntity : class
    {
        protected readonly IRepository<TEntity> _repository;
        protected readonly IUnitOfWork _unitOfWork;
        protected readonly IServiceProvider _serviceProvider;

        protected List<ILogicExecuter<TEntity>> _executers;
        public List<ILogicExecuter<TEntity>> Executers => _executers;


        public Service(IUnitOfWork unitOfWork, IRepository<TEntity> repository, IServiceProvider serviceProvider)
        {
            _unitOfWork = unitOfWork;
            _repository = repository;
            _serviceProvider = serviceProvider;
        }

        public void SetLogicExecuters(params ILogicExecuter<TEntity>[] executers)
        {
            _executers.Clear();
            _executers.AddRange(executers);
        }

        public async Task<DTOqlBaseResponseDto<IEnumerable<dynamic>>> GetAsync(Type ListModel, ISearch searchModel)
        {
            return DTOqlBaseResponseDto<IEnumerable<dynamic>>.Success(await _repository.GetAsync(ListModel, searchModel), searchModel.PaginationWithSort);
        }
        public async Task<DTOqlBaseResponseDto<dynamic?>> GetAsync(Type ListModel, long id)
        {
            return DTOqlBaseResponseDto<dynamic?>.Success(await _repository.GetAsync(ListModel, id));


        }
        public virtual async Task<DTOqlBaseResponseDto<object>> AddAsync<T>(T dto) where T : class
        {

            var dtoLogic = GetConverter(typeof(IDtoLogicExecuter<>), typeof(T)) as dynamic;
            if (dtoLogic != null)
            {
                DTOqlBaseResponseDto<object> responseResult = await dtoLogic.ExecuteAsync(dto);
                if (!responseResult.IsSuccess)
                {
                    return new DTOqlBaseResponseDto<object>().Error(responseResult);
                }
            }

            DTOqlBaseResponseDto<object> executorResult = await ServiceProviderWrapper.ServiceProvider.GetRequiredService<LogicExecuterHolder>().ExecuteDtoLogicExecuters();
            if (!executorResult.IsSuccess)
            {
                return new DTOqlBaseResponseDto<object>().Error(executorResult);
            }

            var entity = dto.GetInstance<T, TEntity>();

            NestedEntityHandle(dto, entity);

            _repository.Add(entity);
            await _unitOfWork.SaveChangesAsync();

            return DTOqlBaseResponseDto<object>.Success(new { Id = entity.GetType().GetProperty("Id").GetValue(entity) });
        }

        private void NestedEntityHandle<T>(T dto, TEntity entity) where T : class
        {
            var ClassProperties = entity.GetType()
                                        .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                                        .Where(x => x.PropertyType.IsGenericType &&
                                                   (x.PropertyType.GetInterfaces().Any(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>))) &&
                                                    x.GetValue(entity) != null)
                                        .ToArray();


            foreach (var property in ClassProperties)
            {

                var collectionItems = property.GetValue(entity) as IEnumerable<dynamic>;
                property.SetValue(entity, null);

                var genericType = property.PropertyType.GenericTypeArguments[0];
                var repo = GetConverter(typeof(IRepository<>), genericType) as dynamic;
                var executer = GetConverter(typeof(ILogicExecuter<>), genericType) as dynamic;
                repo.SetLogicExecuters(executer);

                var stateProperty = dto.GetType().GetProperty(property.Name);
                var states = stateProperty.GetValue(dto) as IEnumerable<IEntityState>;


                int i = 0;

                ICollection<dynamic> items = new List<dynamic>();
                foreach (var itemCollection in collectionItems)
                {
                    switch (states.ElementAt(i++).EntityState)
                    {
                        case EntityState.Add:
                            var props = itemCollection
                             .GetType()
                             .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                            foreach (var x in props)
                            {
                                if (x.PropertyType == entity.GetType())
                                {
                                    x.SetValue(itemCollection, entity);
                                }
                            }
                            repo.Add(itemCollection);
                            break;
                        case EntityState.Update:
                            repo.Remove(itemCollection);


                            string serializedObjString = JsonConvert.SerializeObject(itemCollection, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                            var copyItem = JsonConvert.DeserializeObject(serializedObjString, property.PropertyType.GenericTypeArguments[0]) as dynamic;

                            copyItem.GetType().GetProperty("Id").SetValue(copyItem, default(int));
                            props = copyItem
                            .GetType()
                            .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                            foreach (var x in props)
                            {
                                if (x.PropertyType == entity.GetType())
                                {
                                    x.SetValue(copyItem, entity);
                                }
                            }
                            repo.Add(copyItem);
                            // repo.Edit(itemCollection);
                            break;
                        case EntityState.Delete:
                            repo.Remove(itemCollection);
                            break;
                        default:
                            break;
                    }

                }


            }
        }

        private object GetConverter(Type baseType, Type inType)

        {
            var converterType = baseType.MakeGenericType(inType);

            var converter = _serviceProvider.GetService(converterType);

            return converter;
        }

        public virtual async Task<DTOqlBaseResponseDto<object>> EditAsync<T>(T dto) where T : class
        {
            var dtoLogic = GetConverter(typeof(IDtoLogicExecuter<>), typeof(T)) as dynamic;
            if (dtoLogic != null)
            {

                DTOqlBaseResponseDto<object> responseResult = await dtoLogic.ExecuteAsync(dto);
                if (!responseResult.IsSuccess)
                {
                    return new DTOqlBaseResponseDto<object>().Error(responseResult);
                }

            }

            DTOqlBaseResponseDto<object> executorResult = await ServiceProviderWrapper.ServiceProvider.GetRequiredService<LogicExecuterHolder>().ExecuteDtoLogicExecuters();
            if (!executorResult.IsSuccess)
            {
                return new DTOqlBaseResponseDto<object>().Error(executorResult);
            }

            var entity = dto.GetInstance<T, TEntity>();
            NestedEntityHandle(dto, entity);

            _repository.Edit(entity);
            await _unitOfWork.SaveChangesAsync();

            return DTOqlBaseResponseDto<object>.Success(new());
        }
        public virtual async Task<DTOqlBaseResponseDto<object>> RemoveAsync(object id, bool restore = false)
        {
            var entity = Activator.CreateInstance<TEntity>();
            var idProperty = entity.GetType().GetProperty("Id");
            idProperty.SetValue(entity, id);

            var isDeletedProperty = entity.GetType().GetProperty("IsDeleted");
            isDeletedProperty.SetValue(entity, restore ? false : true);

            _repository.Remove(entity);
            await _unitOfWork.SaveChangesAsync();

            return DTOqlBaseResponseDto<object>.Success(new());
        }
        public async Task<DTOqlBaseResponseDto<object>> SaveRangeAsync<T>(IEnumerable<T> dto) where T : class, IEntityState

        {
            await _serviceProvider.GetRequiredService<LogicExecuterHolder>().ExecuteDtoLogicExecuters();

            foreach (var item in dto)
            {

                var entity = item.GetInstance<T, TEntity>();
                NestedEntityHandle(item, entity);

                switch (item.EntityState)
                {
                    case EntityState.Add:
                        _repository.Add(entity);
                        break;
                    case EntityState.Update:
                        _repository.Edit(entity);
                        break;
                    case EntityState.Delete:
                        _repository.Remove(entity);
                        break;
                    default:
                        break;
                }

            }

            await _unitOfWork.SaveChangesAsync();
            return DTOqlBaseResponseDto<object>.Success(new());
        }
    }

}