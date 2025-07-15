import api from '@/services/api'
import PaginationDivider from '@/utils/paginationDivider'
const divider = new PaginationDivider()

export default {
    getTeams(params) {
        return api
            .get('/Team/Paged/', { params: params })
            .then(({ data }) => {
                return {
                    content: data.content,
                    pagination: {
                        currentPage: data.currentPage,
                        totalPages: data.pageCount,
                        rowCount: data.rowCount,
                        totalItems: data.rowCount,
                    },
                }
            })
            .catch(function (e) {
                console.log(e)
            })
    },
    deleteTeamById(teamId) {
        return api
            .delete('/Team/DeleteByIds', { data: [teamId] })
            .then(() => {
                return true
            })
            .catch(function (e) {
                console.log(e)
                return false
            })
    },
}
