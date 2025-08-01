import api from "@/services/api";

export default {
    Login(credentials) {
        return api.post('/Account/Login', credentials, {
            headers: { 'Authorization': "" }
        })
            .then(({ data }) => {
                console.log(data);
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
            .catch(() => {
                return {
                    erro: "Error",
                }
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
            .catch(() => {
                return {
                    error: "Error"
                }
            });
    },
    GetClientId() {
        return api.get("/Account/clientId")
            .then(({ data }) => {
                return data;
            })
            .catch(() => {
                return {
                    error: "Error"
                }
            });
    },
};