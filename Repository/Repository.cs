using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVL
{
    interface Repository<T>
    {
        void Add(T addedObject);
        void Update(T updatedObject);
        void Remove(T removedObject);
        T GetWithId(int Id);
        List<T> GetAll();
    }
}
