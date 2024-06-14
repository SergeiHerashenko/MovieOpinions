using MovieOpinions.Domain.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieOpinions.DAL.Interface
{
    public interface IBaseRepository<T>
    {
        Task<BaseResponse<bool>> Create(T Entity);
        Task<BaseResponse<bool>> Delete(T Entity);
        Task<BaseResponse<T>> Update(T Entity);
    }
}
