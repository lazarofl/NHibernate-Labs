using System.Collections.Generic;

namespace Sindiconet.SnetV3.Modal.Web
{
    public class Paginate<T>
    {
        public int CurrentPage { get; private set; }
        public int FoundedRecords { get; private set; }
        public int PageSize { get; private set; }
        public IList<T> CurrentPageResults { get; private set; }
        public int NumberOfPages { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Paginate&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="pPagenumber">The pagenumber.</param>
        /// <param name="pFoundedrecords">The number of founded records.</param>
        /// <param name="pPageSize">Size of each page.</param>
        /// <param name="pCurrentPageResults">The current page results.</param>
        public Paginate(int pPagenumber, int pFoundedrecords, int pPageSize, IList<T> pCurrentPageResults)
        {
            this.CurrentPage = pPagenumber;
            this.FoundedRecords = pFoundedrecords;
            this.PageSize = pPageSize;
            this.CurrentPageResults = pCurrentPageResults;
            this.NumberOfPages = ((pFoundedrecords / pPageSize) + ((pFoundedrecords % pPageSize).Equals(0) ? 0 : 1));
            if (this.NumberOfPages.Equals(0))
                this.NumberOfPages = 1;
        }

    }
}
