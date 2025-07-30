import api from "@/services/api";

export default {
    getPermissions() {
        return api
            .get("/Permission/FindAll/")
            .then(({ data }) => {
                return {
                    permissions: data,
                };
            })
            .catch(function (e) {
                console.log(e);
            });
    },
};
