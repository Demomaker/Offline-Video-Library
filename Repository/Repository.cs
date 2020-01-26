/* Repository.cs : Representation of a repository for this project
 * Author : Demomaker
 * Version : 1.0
 */
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

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
