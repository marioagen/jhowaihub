import api from "@/services/api";

export default {
    getDocuments(filters) {
        return api
            .get("/Document", { params: filters })
            .then(({ data }) => {
                return {
                    content: data.content,
                    pagination: {
                        currentPage: data.currentPage,
                        totalPages: data.pageCount,
                        itemsPerPage: 10,
                        totalItems: data.rowCount,
                    },
                };
            })
            .catch((e) => {
                return {
                    error: e,
                };
            });
    },
    deleteDocument(ids) {
        return api
            .delete("/Document/Delete", { data: ids })
            .then(() => {
                return true;
            })
            .catch(function (e) {
                return {
                    error: e,
                };
            });
    },
    findDocument(docId) {
        return api
            .get(`/Document/FindDocument/${docId}`, {
                responseType: "blob",
            })
            .then((response) => {
                return response.data;
            })
            .catch((error) => {
                return {
                    error: error,
                };
            });
    },
    checkPagesLength() {
        return api
            .get("/Document/CheckExceededPages")
            .then(({ data }) => {
                return data;
            })
            .catch((e) => {
                return false;
            });
    },
    findAllForExport(filters) {
        return api
            .get("/Document", { params: { ...filters, pageSize: 10000, page: 1 } })
            .then(({ data }) => data.content || [])
            .catch(() => []);
    },
    VerifyNormalize(id) {
        return api
            .get(`/Document/Status/${id}`)
            .then((response) => {
                return response.data;
            })
            .catch((e) => {
                return {
                    error: e,
                };
            });
    },
};
