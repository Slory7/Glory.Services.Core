using System;
using System.Collections.Generic;
using System.Linq;
using Glory.Services.Core.Config;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Glory.Services.Core.DataStore
{
    public interface IDataStoreManager
    {
        Task Insert<T>(T doc, ProviderLevel level = ProviderLevel.Normal);
        Task InsertMany<T>(IEnumerable<T> documents, ProviderLevel level = ProviderLevel.Normal);
        IQueryable<T> Queryable<T>(ProviderLevel level = ProviderLevel.Normal);
        Task<bool> Remove<T>(Expression<Func<T, bool>> filter, ProviderLevel level = ProviderLevel.Normal);
        Task<bool> RemoveMany<T>(Expression<Func<T, bool>> filter, ProviderLevel level = ProviderLevel.Normal);
        Task<bool> Update<T>(Expression<Func<T, bool>> filter, T doc, ProviderLevel level = ProviderLevel.Normal);
        Task<T> IncrementField<T>(Expression<Func<T, bool>> filter, string field, int amount, ProviderLevel level = ProviderLevel.Normal);
        IQueryable<T> StageQueryable<T>(int stageOnMinutes);
        Task InsertStageData<T>(T doc, string createdDateField = "CreatedDate");
        Task UpdateStageData<T>(Expression<Func<T, bool>> filter, T doc);
        Task RemoveStageData<T>(Expression<Func<T, bool>> filter);
    }
}