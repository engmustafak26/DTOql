using DTOql.ASP;
using DTOql.Extensions;
using DTOql.Interfaces;
using DTOql.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;


namespace DTOql.DataAccess
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {


        private DbContext _databaseContext;
        protected DbSet<TEntity> _entitiesSet;
        protected IQueryable<TEntity> _queryableEntitiesSet;
        protected IServiceProvider _serviceProvider;




        public Repository(DbContext databaseContext, IServiceProvider serviceProvider)
        {
            _databaseContext = databaseContext;
            _entitiesSet = _databaseContext.Set<TEntity>();
            _queryableEntitiesSet = _entitiesSet.AsQueryable();
            _serviceProvider = serviceProvider;
        }



        protected List<ILogicExecuter<TEntity>> _executers = new List<ILogicExecuter<TEntity>>();
        public List<ILogicExecuter<TEntity>> Executers => _executers;


        public void SetLogicExecuters(params ILogicExecuter<TEntity>[] executers)
        {
            if ((executers?.Length ?? default) > 0)
            {
                _executers.Clear();
                _executers.AddRange(executers);
            }
        }


        public void Add(TEntity entity)
        {
            foreach (var executer in _executers)
            {
                executer.ExecuteAsync(entity).GetAwaiter().GetResult();
            }

            var classValuedProperties = entity.GetType()
                    .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                    .Where(x => x.PropertyType.IsClass && !x.PropertyType.IsNumeric() && !x.PropertyType.IsString() && !x.PropertyType.IsDate())
                    .Where(x => x.GetValue(entity) != null)
                    .ToArray();
            if (classValuedProperties is { Length: 1 })
            {
                var entry = _databaseContext.Entry(entity);
                entry.State = EntityState.Added;
            }
            else
            {

                _entitiesSet.Add(entity);
            }
        }

        public void Edit(TEntity entity)
        {
            _entitiesSet.Update(entity);
        }

        public async Task<IEnumerable<dynamic>> GetAsync(Type ListModelType, ISearch searchModel)
        {
            try
            {
                ICollection resultSet = null;
                var logicFetcher = GetConverter(typeof(ILogicFetcher<>), ListModelType) as dynamic;
                if (logicFetcher != null)
                {
                    var returnSet = await logicFetcher.ExecuteAsync();
                    Type inType = null;
                    if (returnSet.GetType().IsArray)
                    {
                        inType = returnSet.GetType().GetElementType();
                    }
                    else
                    {
                        inType = returnSet.GetType().GetGenericArguments()[0];
                    }
                    var stringSet = JsonConvert.SerializeObject(returnSet);
                    resultSet = JsonConvert.DeserializeObject(stringSet, typeof(IEnumerable<>).MakeGenericType(inType)) as ICollection;
                }

                searchModel.PaginationWithSort ??= new Models.PagingWithSortModel();
                var searchString = searchModel.GetSearchCriteria();
                var anonymous = ListModelType.GetAnonymousTypeAsString();
                var query = resultSet != null ? resultSet.AsQueryable() :
                                              _queryableEntitiesSet.AsNoTrackingWithIdentityResolution();
                if (searchString.HasValue())
                {
                    query = query.Where(searchString);
                }


                var paging = searchModel.PaginationWithSort;
                if (resultSet is null)
                {
                    paging.TotalRowsCount = await ((IQueryable<TEntity>)query).CountAsync();
                }
                else
                {
                    paging.TotalRowsCount = query.Count();

                }

                //var tt = query.ToQueryString();

                var dynamicModel = await query.OrderBy(paging.SortBy)
                                  .Skip((paging.CurrentPage - 1) * paging.PageSize)
                                  .Take(paging.PageSize)
                                  .Select(anonymous)
                                  .ToDynamicArrayAsync();

                string modelString = JsonConvert.SerializeObject(dynamicModel);
                var typedModel = JsonConvert.DeserializeObject(modelString, typeof(IEnumerable<>).MakeGenericType(ListModelType));


                var interfaces = ListModelType.GetInterfaces().Concat(new Type[] { ListModelType });
                var logicDisplayerArray = interfaces.Select(x => GetConverter(typeof(ILogicDisplayer<>), x))
                                                    .Where(x => x is not null)
                                                    .ToArray() as dynamic;


                foreach (var item in logicDisplayerArray)
                {
                    foreach (var modelItem in typedModel as IEnumerable)
                    {
                        var returnTask = item.GetType().GetMethod("ExecuteAsync").Invoke(item, new object[] { modelItem });
                        await returnTask;
                    }

                }

                await ServiceProviderWrapper.ServiceProvider.GetRequiredService<LogicExecuterHolder>().ExecuteLogicDisplayers();


                return (typedModel as IEnumerable).AsQueryable().ToDynamicList();
                //return typedModel as dynamic;
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        object GetConverter(Type baseType, Type inType)

        {
            var converterType = baseType.MakeGenericType(inType);

            var converter = _serviceProvider.GetService(converterType);

            return converter;
        }

        public async Task<dynamic?> GetAsync(Type ListModelType, long id)
        {
            var anonymous = ListModelType.GetAnonymousTypeAsString();
            var listDto = await _queryableEntitiesSet
                                .AsNoTracking()
                                .Where($"Id == {id.ToString()}")
                                .Select(anonymous)
                                .ToDynamicArrayAsync();

            var dynamicModel = listDto!;


            string modelString = JsonConvert.SerializeObject(dynamicModel);
            var typedModel = JsonConvert.DeserializeObject(modelString, typeof(IEnumerable<>).MakeGenericType(ListModelType));

            var interfaces = ListModelType.GetInterfaces().Concat(new Type[] { ListModelType });
            var logicDisplayerArray = interfaces.Select(x => GetConverter(typeof(ILogicDisplayer<>), x))
                                                .Where(x => x is not null)
                                                .ToArray() as dynamic;

            foreach (var item in logicDisplayerArray)
            {
                foreach (var modelItem in typedModel as IEnumerable)
                {
                    var returnTask = item.GetType().GetMethod("ExecuteAsync").Invoke(item, new object[] { modelItem });
                    await returnTask;
                }

            }


            await ServiceProviderWrapper.ServiceProvider.GetRequiredService<LogicExecuterHolder>().ExecuteLogicDisplayers();

            return (typedModel as IEnumerable).AsQueryable().ToDynamicArray().FirstOrDefault();

        }


        public void Remove(TEntity entity)
        {

            var classValuedProperties = entity.GetType()
                  .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                  .Where(x => x.PropertyType.IsClass && !x.PropertyType.IsNumeric() && !x.PropertyType.IsString() && !x.PropertyType.IsDate())
                  .Where(x => x.GetValue(entity) != null)
                  .ToArray();
            if (classValuedProperties is { Length: 1 })
            {
                var entry = _databaseContext.Entry(entity);
                entry.State = EntityState.Deleted;
            }
            else
            {
                _entitiesSet.Remove(entity);
            }
        }

    }



}