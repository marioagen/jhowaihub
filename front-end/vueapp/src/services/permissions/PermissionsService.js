import api from "@/services/api";
import PaginationDivider from "@/utils/paginationDivider";
const divider = new PaginationDivider();

export default {
    getPermissions() {
        return api
            .get("/Permission/FindAll/")
            .then(({ data }) => {
                return {
                    permissions: data
                };
            })
            .catch(function (e) {
                console.log(e);
            });
    },
};
