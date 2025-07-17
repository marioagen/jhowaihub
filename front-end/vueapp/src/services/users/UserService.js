import api from "@/services/api";
import PaginationDivider from "@/utils/paginationDivider";
const divider = new PaginationDivider();

export default {
    getUsers(params) {
        return api
            .get("/User/Paged/", { params: params })
            .then(({ data }) => {
                return {
                    content: data.content,
                    pagination: {
                        currentPage: data.currentPage,
                        totalPages: data.pageCount,
                        rowCount: data.rowCount,
                        totalItems: data.rowCount,
                    },
                };
            })
            .catch(function (e) {
                console.log(e);
            });
    },
    deleteUsersById(userId) {
        return api
            .delete("/User/DeactivateByIds", { data: [userId] })
            .then(() => {
                return true;
            })
            .catch(function (e) {
                console.log(e);
                return false;
            });
    },
};
