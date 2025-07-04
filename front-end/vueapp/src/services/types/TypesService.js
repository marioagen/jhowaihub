import api from "@/services/api";
import paginationDivider from "@/utils/paginationDivider";

export default {
    getTypes(params) {
        return api.get('/TypeDoc/Paged/', { params: params })
            .then(({ data }) => {
                console.log(data)
                return {
                    content: data.content,
                    pagination: {
                        currentPage: data.currentPage,
                        pageCount: data.pageCount,
                        rowCount: data.rowCount,
                        listPage: paginationDivider.calculatePageCount(data.pageCount, data.currentPage)
                    },   
                }
            }).catch(function (e) {
                console.log(e);
            });
    },
}