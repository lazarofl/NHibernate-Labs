using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using log4net;
using NHibernate;
using NHibernate.Criterion;
using MyProject.Business.BusinessInterfaces;
using MyProject.Business.Data.DataImplementations;
using MyProject.Business.Data.DataInterfaces;
using MyProject.Core;
using MyProject.Core.Management;
using MyProject.Modal.Web;

namespace MyProject.Business
{
    public abstract class AbstractBusiness<T, ID> : IBusiness<T, ID>
    {
        protected ISession session
        {
            get
            {
                return SessionManager.Instance.GetSession();
            }
        }
        protected readonly ILog logError = LogManager.GetLogger(System.Configuration.ConfigurationManager.AppSettings["AdoNetAppenderLog"]);

        #region Private Members
        //private T _obj = new T(); 
        private IDaoFactory oDaoFactory = new DaoFactory();
        private IDao<T, ID> _IDao;
        private string _sMessage = string.Empty;
        #endregion

        /// <summary>
        /// Initializes a new instance of the AbstractBusiness class.
        /// </summary>
        public AbstractBusiness()
        {
            _IDao = (IDao<T, ID>)oDaoFactory.GetType().InvokeMember("Get" + typeof(T).Name + "Dao",
              BindingFlags.InvokeMethod, null, oDaoFactory, null);

        }

        /// <summary>
        /// Gets the IDAO.
        /// </summary>
        /// <value>The IDAO.</value>
        public IDao<T, ID> IDao
        {
            get { return _IDao; }
        }

        /// <summary>
        /// Methos for retrievel all active Ts registered in Database
        /// </summary>
        /// <returns>A List of T object</returns>
        public virtual IList<T> GetObjectCollection()
        {
            try
            {
                return _IDao.GetAll();
            }
            catch (Exception oException)
            {
                logError.Fatal(oException.Message, oException);
                throw;
            }
        }

        /// <summary>
        ///Insert/Update a T object in database.
        /// </summary>
        /// <param name="pObject">Object T</param>
        public virtual void SaveT(T pObject)
        {
            SaveT(pObject, false);
        }

        /// <summary>
        /// Insert/Update a T object in database.
        /// </summary>
        /// <param name="pObject">The T object.</param>
        /// <param name="pIsUsingTransaction">if set to <c>true</c> [is using transaction].</param>
        public virtual void SaveT(T pObject, bool pIsUsingTransaction)
        {
            try
            {
                if (pIsUsingTransaction == false)
                    SessionManager.Instance.GetSession().BeginTransaction();

                if (SessionManager.Instance.GetSession().IsDirty())
                    SessionManager.Instance.GetSession().Flush();

                SessionManager.Instance.GetSession().Clear();
                _IDao.SaveOrUpdate(pObject);

                if (pIsUsingTransaction == false)
                    SessionManager.Instance.GetSession().Transaction.Commit();
            }
            catch (Exception oException)
            {
                if (pIsUsingTransaction == false)
                    SessionManager.Instance.RollbackTransaction();
                _sMessage = "The following error : " + oException.Message + " was found while trying to AbstractBusiness.Save an T object typeof  : " + pObject.GetType() + " in Database";
                logError.Fatal(_sMessage, oException);
                throw;
            }
            finally
            {
                if (pIsUsingTransaction == false)
                    SessionManager.Instance.CloseSession();
            }
        }


        /// <summary>
        ///Insert /Update a List of T object in database.
        /// </summary>
        /// <param name="pListofObject">Object T</param>
        public virtual bool SaveAll(IList<T> pListofObject)
        {
            return this.SaveAll(pListofObject, false);
        }

        public bool SaveAll(IEnumerable<T> pListofObject, bool pIsUsingTransaction)
        {
            bool bIsDeleted;
            int indexObject = 0;
            if (!pIsUsingTransaction)
                SessionManager.Instance.BeginTransaction();
            try
            {
                foreach (T myObject in pListofObject)
                {
                    _IDao.SaveOrUpdate(myObject);
                    indexObject++;
                }

                if (!pIsUsingTransaction)
                    SessionManager.Instance.CommitTransaction();
                bIsDeleted = true;

            }
            catch (Exception oException)
            {
                if (!pIsUsingTransaction)
                    SessionManager.Instance.RollbackTransaction();
                logError.Fatal(oException.Message, oException);
                throw;
            }
            finally
            {
                if (!pIsUsingTransaction)
                    SessionManager.Instance.CloseSession();
            }
            return bIsDeleted;
        }


        /// <summary>
        /// Recover a Specific T Object
        /// </summary>
        /// <param name="pObjectId">ChallengeId string Property</param>
        /// <returns>Filled T object </returns>
        public T GetObjectById(ID pObjectId)
        {
            try
            {
                return _IDao.GetById(pObjectId, false);
            }
            catch (Exception oException)
            {
                logError.Fatal(oException.Message, oException);
                throw;
            }
        }

        /// <summary>
        /// Deletes the object.
        /// </summary>
        /// <param name="pObject">The T object.</param>
        /// <returns>If object deleted</returns>
        public virtual bool DeleteObject(T pObject)
        {
            return DeleteObject(pObject, false);
        }

        /// <summary>
        /// Deletes the object.
        /// </summary>
        /// <param name="pObject">The T object.</param>
        /// <param name="isUsingTransaction">if set to <c>true</c> [is using transaction].</param>
        /// <returns>if object deleted</returns>
        public virtual bool DeleteObject(T pObject, bool isUsingTransaction)
        {
            bool bIsDeleted;
            if (!isUsingTransaction)
                SessionManager.Instance.BeginTransaction();
            try
            {
                _IDao.Delete(pObject);

                if (!isUsingTransaction)
                    SessionManager.Instance.CommitTransaction();
                bIsDeleted = true;
            }
            catch (Exception oException)
            {
                if (!isUsingTransaction)
                    SessionManager.Instance.RollbackTransaction();
                logError.Fatal(oException.Message, oException);
                throw;
            }
            finally
            {
                if (!isUsingTransaction)
                    SessionManager.Instance.CloseSession();
            }
            return bIsDeleted;
        }

        #region Paginate

        /// <summary>
        /// Paginates the result.
        /// </summary>
        /// <param name="oICriteria">The criteria.</param>
        /// <param name="pPageSize">Size of the page.</param>
        /// <param name="pPageNumber">The page number.</param>
        /// <returns></returns>
        public Paginate<T> PaginateResult(ICriteria oICriteria, int pPageSize, int pPageNumber)
        {
            return this.PaginateResult(oICriteria, pPageSize, pPageNumber, null);
        }
        /// <summary>
        /// Paginates the result.
        /// </summary>
        /// <param name="pICriteria">The criteria.</param>
        /// <param name="pPageSize">Size of the page.</param>
        /// <param name="pPageNumber">The page number.</param>
        /// <param name="pOrder">The order.</param>
        /// <returns></returns>
        public Paginate<T> PaginateResult(ICriteria pICriteria, int pPageSize, int pPageNumber, Order pOrder)
        {
            try
            {
                pICriteria.SetMaxResults(pPageSize);
                pICriteria.SetFirstResult(pPageSize * (pPageNumber - 1));
                if (pOrder != null)
                    pICriteria.AddOrder(pOrder);
                IList<T> result = pICriteria.List<T>();

                if (pOrder != null)
                    pICriteria.ClearOrders();
                pICriteria.SetMaxResults(1);
                pICriteria.SetFirstResult(0);
                pICriteria.SetProjection(Projections.Count("Id"));
                int count = pICriteria.UniqueResult<int>();

                return new Paginate<T>(pPageNumber, count, pPageSize, result);
            }
            catch (Exception oException)
            {
                logError.Fatal(string.Format("The method \"PaginateResult\" throws a {0} exception: {1}", oException.GetType(), oException.Message), oException);
                throw;
            }
        }



        /// <summary>
        /// Paginates the result.
        /// </summary>
        /// <param name="pList">The list.</param>
        /// <param name="pPageSize">Size of the page.</param>
        /// <param name="pPageNumber">The page number.</param>
        /// <returns></returns>
        public Paginate<T> PaginateResult(IList<T> pList, int pPageSize, int pPageNumber)
        {
            try
            {
                return new Paginate<T>(
                        pPageNumber,
                        pList.Count,
                        pPageSize,
                        pList
                            .Skip(pPageSize * (pPageNumber - 1))
                            .Take(pPageSize)
                            .ToList()
                        );
            }
            catch (Exception oException)
            {
                logError.Fatal(string.Format("The method \"PaginateResult\" throws a {0} exception: {1}", oException.GetType(), oException.Message), oException);
                throw;
            }
        }

        #endregion
    }

}
