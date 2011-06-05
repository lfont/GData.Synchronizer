using System;
using System.Linq;

namespace GData.Synchronizer.Storage
{
    public interface IDataContext : IDisposable
    {
        void Save(object poco);
        IQueryable<T> Query<T>();
    }
}
