using DTOql.ASP;
using DTOql.Continuations;
using DTOql.Enums;
using DTOql.Extensions;
using DTOql.Interfaces;
using DTOql.Models;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

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
            _unitOfWork.ClearEntries();
            var dtoLogic = GetConverter(typeof(IDtoLogicExecuter<>), typeof(T)) as dynamic;
            if (dtoLogic != null)
            {
                DTOqlBaseResponseDto<object> responseResult = await dtoLogic.ExecuteAsync(dto);
                if (!responseResult.IsSuccess)
                {
                    return new DTOqlBaseResponseDto<object>().Error(responseResult);
                }
            }

            DTOqlBaseResponseDto<object> executorResult = await ServiceProviderWrapper.ServiceProvider.GetRequiredService<LogicExecuterHolder>().ExecuteDtoLogicExecuters(dto);
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

        HashSet<object> _vistedObjects = new HashSet<object>();
        private void NestedEntityHandle(object dto, object entity, Type parentObjectType = null)
        {


            _vistedObjects.Add(entity);
            var ClassProperties = entity.GetType()
                                        .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                                        .Where(x => !x.PropertyType.IsString() &&
                                                      !x.PropertyType.IsNumeric() &&
                                                      !x.PropertyType.IsBoolean() &&
                                                      !x.PropertyType.IsDate() &&
                                                       x.GetValue(entity) != null &&
                                                       x.PropertyType != parentObjectType)
                                        //.Where(x => x.PropertyType.IsGenericType &&
                                        //           (x.PropertyType.GetInterfaces().Any(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>))) &&
                                        //            x.GetValue(entity) != null)
                                        .ToArray();

            foreach (var property in ClassProperties)
            {

                var collectionItems = property.GetValue(entity) as IEnumerable<dynamic>;
                if (collectionItems is null)
                {
                    collectionItems = new List<dynamic>() { property.GetValue(entity) };
                }

                bool loopDetected = false;
                foreach (var item in collectionItems)
                {
                    if (_vistedObjects.Contains(item))
                    {
                        loopDetected = true;
                        break;
                    }
                }
                if (loopDetected)
                    continue;

                var genericType = property.PropertyType.GenericTypeArguments.Length == 0 ? property.PropertyType : property.PropertyType.GenericTypeArguments[0];
                var repo = GetConverter(typeof(IRepository<>), genericType) as dynamic;
                var executer = GetConverter(typeof(ILogicExecuter<>), genericType) as dynamic;
                repo.SetLogicExecuters(executer);

                var stateProperty = dto.GetType().GetProperty(property.Name);
                var states = stateProperty.GetValue(dto) as ICollection<IEntityState>;

                if (states is null)
                {
                    var singleState = stateProperty.GetValue(dto) as IEntityState;
                    states = new List<IEntityState>(collectionItems.Count());
                    collectionItems.ForEach(x => states.Add(singleState ?? new _entityState { EntityState = EntityState.Add }));
                }

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

                            //property.SetValue(entity, null);

                            var props2 = itemCollection
                            .GetType()
                            .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                            foreach (var x in props2)
                            {
                                if (x.PropertyType == entity.GetType())
                                {
                                    x.SetValue(itemCollection, entity);
                                }
                            }


                            repo.UpdateDeep(itemCollection);
                            //repo.Remove(itemCollection);


                            //string serializedObjString = JsonConvert.SerializeObject(itemCollection, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                            //var copyItem = JsonConvert.DeserializeObject(serializedObjString, property.PropertyType.GenericTypeArguments[0]) as dynamic;

                            //copyItem.GetType().GetProperty("Id").SetValue(copyItem, default(int));
                            //props = copyItem
                            //.GetType()
                            //.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                            //foreach (var x in props)
                            //{
                            //    if (x.PropertyType == entity.GetType())
                            //    {
                            //        x.SetValue(copyItem, entity);
                            //    }
                            //}
                            //repo.Add(copyItem);
                            // repo.Edit(itemCollection);
                            break;
                        case EntityState.Delete:
                            repo.Remove(itemCollection);
                            break;

                        case EntityState.None:
                            repo.None(itemCollection);
                            break;
                        default:
                            break;
                    }

                }


            }

            ClassProperties.ForEach(x =>
            {
                var nested = x.GetValue(entity).GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                                          .Where(x => !x.PropertyType.IsString() &&
                                                      !x.PropertyType.IsNumeric() &&
                                                      !x.PropertyType.IsBoolean() &&
                                                      !x.PropertyType.IsDate())
                .ToArray();

                nested.ForEach(y =>
                {
                    if (dto.GetType().GetProperty(x.Name) is null)
                        return;

                    IEnumerable enumerable = (x.GetValue(entity) as IEnumerable);
                    var newDto = dto.GetType().GetProperty(x.Name).GetValue(dto);
                    if (enumerable is null)
                    {
                        NestedEntityHandle(newDto, x.GetValue(entity), entity.GetType());
                    }
                    else
                    {
                        int count = 0;
                        foreach (var item in enumerable)
                        {
                            var itemDto = (newDto as IEnumerable).ToDynamicArray().ElementAt(count++);
                            NestedEntityHandle(itemDto, item, entity.GetType());


                        }
                    }
                });

                //x.SetValue(entity, null);

            });

        }
        class _entityState : IEntityState
        {
            public EntityState EntityState { get; set; }
        }
        private object GetConverter(Type baseType, Type inType)

        {
            var converterType = baseType.MakeGenericType(inType);

            var converter = _serviceProvider.GetService(converterType);

            return converter;
        }

        public virtual async Task<DTOqlBaseResponseDto<object>> EditAsync<T>(T dto) where T : class
        {
            _unitOfWork.ClearEntries();
            var dtoLogic = GetConverter(typeof(IDtoLogicExecuter<>), typeof(T)) as dynamic;
            if (dtoLogic != null)
            {

                DTOqlBaseResponseDto<object> responseResult = await dtoLogic.ExecuteAsync(dto);
                if (!responseResult.IsSuccess)
                {
                    return new DTOqlBaseResponseDto<object>().Error(responseResult);
                }

            }

            DTOqlBaseResponseDto<object> executorResult = await ServiceProviderWrapper.ServiceProvider.GetRequiredService<LogicExecuterHolder>().ExecuteDtoLogicExecuters(dto);
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
            _unitOfWork.ClearEntries();
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
            _unitOfWork.ClearEntries();
            foreach (var item in dto)
            {
                DTOqlBaseResponseDto<object> executorResult = await _serviceProvider.GetRequiredService<LogicExecuterHolder>().ExecuteDtoLogicExecuters(item);
                if (!executorResult.IsSuccess)
                {
                    return new DTOqlBaseResponseDto<object>().Error(executorResult);
                }
            }

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