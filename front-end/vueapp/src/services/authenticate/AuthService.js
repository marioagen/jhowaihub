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
                    name: "",
                    login: "",
                    isAdmin: "",
                    tokenAzure: "",
                    tokenApi: "",
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
                if(!data.success) {
                    return {
                        error: data.message,
                    }
                }
                
                return {
                    tokenApi: data.data.token,
                    tenant: data.data.tenant,
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