export default class paginationDivider 
{
   calculatePageCount(pageTotal, current) {
        var page = Math.ceil(current / 3.0) * 3;
        var pages = [];
        for (let i = page - 2; i <= page; i++) {
          if (i <= pageTotal) {
            pages.push(i);
          }
        }
       return pages;
    }
}

