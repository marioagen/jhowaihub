import api from "@/services/api";
import PaginationDivider from "@/utils/paginationDivider";
const divider = new PaginationDivider();

export default {
    getTypes(params) {
        return api.get('/TypeDoc/Paged/', { params: params })
            .then(({ data }) => {
                return {
                    content: data.content,
                    pagination: {
                        currentPage: data.currentPage,
                        pageCount: data.pageCount,
                        rowCount: data.rowCount,
                        listPage: divider.calculatePageCount(data.pageCount, data.currentPage),
                    }
                }
            }).catch(function (e) {
                console.log(e);
            });
    },
}