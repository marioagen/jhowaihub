import api from "@/services/api";

export default {
    Login(credentials) {
        return api.post('/Account/Login', credentials, {
            headers: { 'Authorization': "" }
        })
            .then(({ data }) => {
                return {
                    id: "",
                    name: data.name,
                    login: data.email,
                    isAdmin: "",
                    tokenAzure: "",
                    tokenApi: data.token,
                    tenant: data.tenant,
                };
            })
            .catch((error) => {
                throw error;
            });
    },
    LoginSSO(form, userAzure) {
        return api.post("/Account/Login-sso", form, {
            headers: { Authorization: `Bearer ${userAzure}` },
        })
            .then(({ data }) => {
                return {
                    tokenApi: data.token,
                    tenant: data.tenant,
                };
            })
            .catch((error) => {
                throw error;
            });
    },
    GetClientId() {
        return api.get("/Account/clientId")
            .then(({ data }) => {
                return data;
            })
            .catch((error) => {
                throw error;
            });
    },
    Logout() {
        return api.post("/account/logout")
            .catch(err => {
                throw err;
            })
    }
};