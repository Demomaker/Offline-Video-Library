using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DVL
{
    public interface Repository<T>
    {
        void Add(T addedObject, bool IncludeID);
        void Update(T updatedObject);
        void Remove(T removedObject);
        void RemoveWithId(int Id);
        T GetWithId(int Id);
        List<T> GetAll();
        List<T> GetAllWithCondition(Expression<Func<T, bool>> predicate);
    }
}
