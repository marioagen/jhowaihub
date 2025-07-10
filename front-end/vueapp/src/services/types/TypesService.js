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
                        totalPages: data.pageCount,
                        rowCount: data.rowCount,
                        totalItems: data.rowCount,
                    }
                }
            }).catch(function (e) {
                console.log(e);
            });
    },
    editType(params) {
        return api.put('/TypeDoc', params)
            .then(({ data, status }) => {
                return {
                    success: true,
                    status,
                    data
                }
            })
            .catch((e) => {
                const status = e?.response?.status ?? 500
                const message = e?.response?.data?.message || 'Erro desconhecido'
                return {
                    success: false,
                    status,
                    error: message
                }
            })
    },

    deleteTypeById(teamId) {
        return api.delete('/TypeDoc/DeleteByIds', { data: [teamId] })
            .then(() => {
                return true;
            }).catch(function (e) {
                console.log(e);
                return false;
            });
    },
    addType(name) {
        return api.post('/TypeDoc?name=' + name)
             .then(({ data, status }) => {
                return {
                    success: true,
                    status,
                    data
                }
            })
            .catch((e) => {
                const status = e?.response?.status ?? 500
                const message = e?.response?.data?.message || 'Erro desconhecido'
                return {
                    success: false,
                    status,
                    error: message
                }
            })
    },
}