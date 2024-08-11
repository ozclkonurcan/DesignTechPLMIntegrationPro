using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesignTechPLMIntegrationPro.Application.Interfaces.EntegrasyonModulu.WTPart.State
{
    //public interface IStateService<T> where T : class
    //{
    //    void RELEASED(T entity);
    //    void CANCELLED(T entity);
    //    void INWORK(T entity);
    //}
    public interface IStateService
    {
        Task RELEASED();
        Task CANCELLED();
        Task INWORK();
    }
}
